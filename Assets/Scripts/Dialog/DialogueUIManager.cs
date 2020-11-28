using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Gameplay;
using GameSystem;
using GameSystem.Save;
using UI;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Yarn;
using Yarn.Unity;

namespace Dialog
{
    /// <summary>
    /// Based on YarnSpinner's sample DialogueUI
    /// </summary>
    /// <seealso cref="DialogueUI"/>
    ///  todo: add more documentation
    ///  todo: add aliases in button options
    public class DialogueUIManager : Yarn.Unity.DialogueUIBehaviour, SaveClientCallback
    {
        [FormerlySerializedAs("assetManager")] public IconManager iconManager;
        public InputManager inputManager;

        [Header("Critical")] public GameConfiguration gameConfiguration;

        public String[] markupWholeWhitelist;
        public String[] markupPhraseWhitelist;
        
        private bool userRequestedNextLine = false;

        private System.Action<int> currentOptionSelectionHandler;

        private bool waitingForOptionSelection = false;

        public UnityEvent onDialogueStart;

        public UnityEvent onDialogueEnd;

        public UnityEvent onLinesStart;

        public UnityEvent onLineFinishDisplaying;

        public DialogueRunner.StringUnityEvent onLineUpdate;

        public UnityEvent onLineEnd;

        public UnityEvent onOptionsStart;

        public UnityEvent onOptionsEnd;

        public DialogueRunner.StringUnityEvent onCommand;

        public DialogueBlocker dialogueBlocker = new DialogueBlocker();

        private int _textIndex = 0;
        private int _portraitIndex = 0;
        private string _lastSpeaker = "";
        private bool requestDialogWrite = false;
        private SaveClient saveClient;
        private string _lastDialog;

        private float TextRate => gameConfiguration.TextRate;

        public bool IsBlocking
        {
            get => dialogueBlocker.IsBlocking;
            set
            {
                if (value)
                {
                    dialogueBlocker.Block();
                }
                else
                {
                    dialogueBlocker.Unblock();
                }
            }
        }

        private void OnEnable()
        {
            saveClient = gameConfiguration.RequestSaveAccess(this);
        }

        private void OnDisable()
        {
            gameConfiguration.ReleaseSaveAccess(saveClient);
            saveClient = null;
        }

        private void Awake()
        {
            Debug.Assert(gameConfiguration != null);
            Debug.Assert(iconManager != null);
        }

        public override Dialogue.HandlerExecutionType RunLine(Line line, ILineLocalisationProvider localisationProvider,
            Action onLineComplete)
        {
            StartCoroutine(DoRunLine(line, localisationProvider, onLineComplete));
            return Dialogue.HandlerExecutionType.PauseExecution;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// For changing up the text. You have three places to watch out for:
        /// (1) When text is fed to the string slowly
        /// (2) When text was never fed to the string slowly
        /// (3) User decided to show the entire text immediately
        /// </remarks>
        /// <param name="line"></param>
        /// <param name="localisationProvider"></param>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        private IEnumerator DoRunLine(Yarn.Line line, ILineLocalisationProvider localisationProvider,
            System.Action onComplete)
        {
            onLinesStart?.Invoke();

            userRequestedNextLine = false;

            string text = localisationProvider.GetLocalisedTextForLine(line);
            text = text.Trim();

            // for empty //, we ignore them
            if (text.Trim().Equals("//"))
            {
                userRequestedNextLine = true;
                text = "";
            }

            // todo: deprecate
            // identifies speaker
            var argSplit = text.Split(':');
            InformSpeakerReturn speakerInfo = iconManager.InformSpeaker(argSplit.Length != 1 ? argSplit[0] : "");
            dialogueBlocker = speakerInfo.dialogueBlocker;
            var character = speakerInfo.character;
            _lastSpeaker = speakerInfo.realName;
            
            if (speakerInfo.realName.Length != 0)
            {
                text = text.Replace($"{argSplit[0]}:", $"{speakerInfo.realName}:");
            }

            // onLineUpdate.AddListener(textItem.UpdateLine);

            while (dialogueBlocker.IsBlocking && !userRequestedNextLine)
            {
                yield return new WaitForSeconds(0.03f);
            }

            if (TextRate <= 0f)
            {
                gameConfiguration.TextRate = 0f;
            }

            var formattedStringBuilder = new StringBuilder();
            var cleanStringBuilder = new StringBuilder();
            var markupBuilder = new StringBuilder();
            var textMarks = new Queue<SpecialTextMark>();
            // todo: do something about markup symbols
            // todo: research text gui things that people may use
            // todo: change text speed

            foreach (var c in text)
            {
                #region for hiding markup

                if (markupBuilder.Length != 0)
                {
                    markupBuilder.Append(c);

                    if (c.Equals('>'))
                    {
                        AdjustMarkups(markupBuilder);

                        var markupText = markupBuilder.ToString().ToLower();
                        if (!gameConfiguration.EnableTextFormatting  || !IsWhiteListed(markupText))
                        {
                            // don't do text formatting
                        }
                        else if (markupText.Contains("textspeed"))
                        {
                            var parseArgs = markupText.Split('=');
                            if (parseArgs.Length == 1)
                            {
                                textMarks.Enqueue(new SpecialTextMark
                                {
                                    argument = "1",
                                    effect = SpecialTextMark.Effect.TextSpeed,
                                    index = cleanStringBuilder.Length
                                });
                            }
                            else if (parseArgs.Length == 2 &&
                                     float.TryParse(parseArgs[1].Replace(">", "")
                                         .Replace("/", ""), out var tmpFloat))
                            {
                                textMarks.Enqueue(new SpecialTextMark
                                {
                                    argument = $"{1f / tmpFloat}",
                                    effect = SpecialTextMark.Effect.TextSpeed,
                                    index = cleanStringBuilder.Length
                                });
                            }
                            else
                            {
                                Debug.LogWarning($"Failed to translate markup: {markupText}");
                            }
                        }
                        else
                        {
                            formattedStringBuilder.Append(markupBuilder);
                        }

                        markupBuilder.Clear();
                    }

                    continue;
                }

                if (c.Equals('<'))
                {
                    markupBuilder.Append(c);
                    continue;
                }

                #endregion for hiding markup

                formattedStringBuilder.Append(c);
                cleanStringBuilder.Append(c);
            }

            var formattedString = gameConfiguration.EnableTextFormatting ? 
                formattedStringBuilder.ToString() : cleanStringBuilder.ToString();
            var textLength = cleanStringBuilder.Length;
            character.SetInitialText(formattedString);
            _lastDialog = cleanStringBuilder.ToString();
            
            bool isSkipping = false;
            var textSpeedMultiplier = 1f;
            for (int i = 0; i < textLength; i++)
            {
                if (textMarks.Count != 0 && textMarks.Peek().index <= i)
                {
                    var effect = textMarks.Dequeue();
                    switch (effect.effect)
                    {
                        case SpecialTextMark.Effect.TextSpeed:
                            if (float.TryParse(effect.argument, out var value))
                            {
                                textSpeedMultiplier = value;
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (userRequestedNextLine)
                {
                    userRequestedNextLine = false;
                    break;
                }
                
                character.ShowCharacters(i);
                while (inputManager.inputState == InputState.Pause)
                {
                    yield return new WaitForSeconds(1f/60f);
                }
                yield return new WaitForSeconds(TextRate * textSpeedMultiplier);
            }
            
            // i don't know why the last character's not shown sometimes
            character.ShowCharacters(textLength + 10);

            // Indicate to the rest of the game that the line has finished being delivered
            onLineFinishDisplaying?.Invoke();

            while (userRequestedNextLine == false)
            {
                yield return null;
            }

            userRequestedNextLine = false;

            // Avoid skipping lines if textSpeed == 0
            yield return new WaitForEndOfFrame();

            // Hide the text and prompt
            onLineEnd?.Invoke();

            // onLineUpdate.RemoveListener(textItem.UpdateLine);

            onComplete();
        }

        private bool IsWhiteListed(string markupText)
        {
            // todo: not in white list
            string markupLower = markupText.ToLower();
            foreach (var wholeMarkup in markupWholeWhitelist)
            {
                if (wholeMarkup.ToLower().Equals(markupLower))
                {
                    return true;
                }
            }

            foreach (var phraseMarkup in markupPhraseWhitelist)
            {
                if (markupLower.Contains(phraseMarkup.ToLower()))
                {
                    return true;
                }
            }
            
            Debug.Log($"Delete: {markupText}");
            
            return false;
        }

        private void AdjustMarkups(StringBuilder markupBuilder)
        {
            markupBuilder.Replace("<color=", "<color=#");
        }

        private string AdjustMarkups(String markupBuilder)
        {
            return markupBuilder.Replace("<color=", "<color=#");
        }

        public override void RunOptions(OptionSet optionSet, ILineLocalisationProvider localisationProvider,
            Action<int> onOptionSelected)
        {
            StartCoroutine(DoRunOptions(optionSet, localisationProvider, onOptionSelected));
        }


        /// Show a list of options, and wait for the player to make a
        /// selection.
        private IEnumerator DoRunOptions(Yarn.OptionSet optionsCollection,
            ILineLocalisationProvider localisationProvider,
            System.Action<int> selectOption)
        {
            iconManager.CreateButtons(optionsCollection.Options.Length);

            // Display each option in a button, and make it visible
            int i = 0;

            waitingForOptionSelection = true;

            currentOptionSelectionHandler = selectOption;

            foreach (var optionString in optionsCollection.Options)
            {
                iconManager.ActivateButtons(i, () => SelectOption(optionString.ID));

                var optionText = localisationProvider.GetLocalisedTextForLine(optionString.Line);

                if (optionText == null)
                {
                    Debug.LogWarning($"Option {optionString.Line.ID} doesn't have any localised text");
                    optionText = optionString.Line.ID;
                }

                iconManager.SetButtonText(i, optionText);

                i++;
            }

            onOptionsStart?.Invoke();

            // Wait until the chooser has been used and then removed 
            while (waitingForOptionSelection)
            {
                yield return null;
            }

            iconManager.HideAllButtons();

            onOptionsEnd?.Invoke();
        }


        /// Runs a command.
        /// <inheritdoc/>
        public override Dialogue.HandlerExecutionType RunCommand(Yarn.Command command, System.Action onCommandComplete)
        {
            // Dispatch this command via the 'On Command' handler.
            onCommand?.Invoke(command.Text);

            // Signal to the DialogueRunner that it should continue
            // executing. (This implementation of RunCommand always signals
            // that execution should continue, and never calls
            // onCommandComplete.)
            return Dialogue.HandlerExecutionType.ContinueExecution;
        }


        /// Called when the dialogue system has started running.
        /// <inheritdoc/>
        public override void DialogueStarted()
        {
            // todo: Enable the dialogue controls.
            // if (dialogueContainer != null)
            //     dialogueContainer.SetActive(true);

            onDialogueStart?.Invoke();
        }

        /// Called when the dialogue system has finished running.
        /// <inheritdoc/>
        public override void DialogueComplete()
        {
            onDialogueEnd?.Invoke();

            // todo: Hide the dialogue interface.
            // if (dialogueContainer != null)
            //     dialogueContainer.SetActive(false);
        }

        /// <summary>
        /// Signals that the user has finished with a line, or wishes to
        /// skip to the end of the current line.
        /// </summary>
        /// <remarks>
        /// This method is generally called by a "continue" button, and
        /// causes the DialogueUI to signal the <see
        /// cref="DialogueRunner"/> to proceed to the next piece of
        /// content.
        ///
        /// If this method is called before the line has finished appearing
        /// (that is, before <see cref="onLineFinishDisplaying"/> is
        /// called), the DialogueUI immediately displays the entire line
        /// (via the <see cref="onLineUpdate"/> method), and then calls
        /// <see cref="onLineFinishDisplaying"/>.
        /// </remarks>
        public void MarkLineComplete()
        {
            userRequestedNextLine = true;
        }

        /// <summary>
        /// Signals that the user has selected an option.
        /// </summary>
        /// <remarks>
        /// This method is called by the <see cref="Button"/>s in the <see
        /// cref="optionButtons"/> list when clicked.
        ///
        /// If you prefer, you can also call this method directly.
        /// </remarks>
        /// <param name="optionID">The <see cref="OptionSet.Option.ID"/> of
        /// the <see cref="OptionSet.Option"/> that was selected.</param>
        public void SelectOption(int optionID)
        {
            // todo: check what this means
            if (waitingForOptionSelection == false)
            {
                Debug.LogWarning("An option was selected, but the dialogue UI was not expecting it.");
                return;
            }

            waitingForOptionSelection = false;
            currentOptionSelectionHandler?.Invoke(optionID);
        }

        public void RequestLastDialogWrite()
        {
            requestDialogWrite = true;
        }

        public void ShowElements(bool shouldShow)
        {
            iconManager.ShowElements(shouldShow);
        }

        public void WriteAutoSave()
        {
            saveClient.autoSave.currentSpeaker = _lastSpeaker;
            saveClient.autoSave.lastDialog = _lastDialog;
        }

        public void SetFakeLastDialog(string speaker, string message)
        {
            _lastSpeaker = speaker;
            _lastDialog = message;
        }
    }

    public class SpecialTextMark
    {
        public string argument = "";
        public int index = 0;
        public Effect effect;

        public enum Effect
        {
            TextSpeed
        }
    }
}