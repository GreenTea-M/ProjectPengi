using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using Yarn;
using Yarn.Unity;

namespace Dialog
{
    /// <summary>
    /// Based on YarnSpinner's sample DialogueUI
    /// </summary>
    /// <seealso cref="DialogueUI"/>
    ///  todo: add more documentation
    public class DialogueUIManager : Yarn.Unity.DialogueUIBehaviour
    {
        public IconManager iconManager;

        [Header("Prefabs")] [Tooltip("Prefab that contains the texts")]
        public GameObject dialogueItemPrefab;

        [Tooltip("Prefab that contains the portrait")]
        public GameObject dialoguePortraitPrefab;

        [Tooltip("Prefab for the options")] public GameObject dialogueOptionsPrefab;

        public GameObject prefabPortrait;

        [Header("Values")] [Tooltip("Text speed in characters per second")]
        public float textSpeed = 0.025f;

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

        private const int PoolSize = 10;
        private TextItem[] _textItems = new TextItem[PoolSize];
        private int _textIndex = 0;
        private PortraitItem[] _portraitItems = new PortraitItem[PoolSize];
        private int _portraitIndex = 0;
        private string _lastSpeaker = "";

        private void Awake()
        {
            Debug.Assert(_textItems != null);
            Debug.Assert(iconManager != null);

            for (int i = 0; i < PoolSize; i++)
            {
                _textItems[i] = Instantiate(dialogueItemPrefab).GetComponent<TextItem>();
                _textItems[i].Activate();

                _portraitItems[i] = Instantiate(dialoguePortraitPrefab).GetComponent<PortraitItem>();
                _portraitItems[i].Activate();
            }
        }

        public override Dialogue.HandlerExecutionType RunLine(Line line, ILineLocalisationProvider localisationProvider,
            Action onLineComplete)
        {
            StartCoroutine(DoRunLine(line, localisationProvider, onLineComplete));
            return Dialogue.HandlerExecutionType.PauseExecution;
        }

        private IEnumerator DoRunLine(Yarn.Line line, ILineLocalisationProvider localisationProvider,
            System.Action onComplete)
        {
            onLinesStart?.Invoke();

            userRequestedNextLine = false;

            string text = localisationProvider.GetLocalisedTextForLine(line);

            if (text == null)
            {
                Debug.LogWarning($"Line {line.ID} doesn't have any localised text.");
                text = line.ID;
            }

            string candidateSpeaker = text.Split(':')[0];
            Sprite newSprite = iconManager.GetSprite(candidateSpeaker);
            if (!candidateSpeaker.Equals(_lastSpeaker) || (newSprite == null && _lastSpeaker.Equals("")))
            {
                foreach (var portrait in _portraitItems)
                {
                    portrait.PushUpwards();
                }

                _portraitIndex++;
                _portraitItems[_portraitIndex].SetToCenter(newSprite);
            }

            // todo: push every text upwards
            foreach (var item in _textItems)
            {
                item.PushUpwards();
            }

            _textIndex = (_textIndex + 1) % PoolSize;
            TextItem textItem = _textItems[_textIndex];
            textItem.SetToCenter();
            onLineUpdate.AddListener(textItem.UpdateLine);

            if (textSpeed > 0.0f)
            {
                var stringBuilder = new StringBuilder();
                var markupBuilder = new StringBuilder();
                var originalTextSpeed = textSpeed;
                bool skippingChars = false;
                // todo: do something about markup symbols
                // todo: research text gui things that people may use
                // todo: change text speed

                foreach (var c in text)
                {
                    if (markupBuilder.Length != 0)
                    {
                        // adjustment for color to work
                        if (markupBuilder.ToString().Equals("<color="))
                        {
                            markupBuilder.Append("#");
                        }
                        
                        markupBuilder.Append(c);
                        
                        if (c.Equals('>'))
                        {
                            stringBuilder.Append(markupBuilder);
                            markupBuilder.Clear();
                        }
                        continue;
                    }
                    
                    if (c.Equals('<'))
                    {
                        markupBuilder.Append(c);
                        continue;
                    }

                    stringBuilder.Append(c);
                    onLineUpdate?.Invoke(stringBuilder.ToString());
                    if (userRequestedNextLine)
                    {
                        // We've requested a skip of the entire line.
                        // Display all of the text immediately.
                        onLineUpdate?.Invoke(text);
                        break;
                    }

                    yield return new WaitForSeconds(textSpeed);
                }
            }
            else
            {
                // Display the entire line immediately if textSpeed <= 0
                onLineUpdate?.Invoke(text);
            }

            // We're now waiting for the player to move on to the next line
            userRequestedNextLine = false;

            // Indicate to the rest of the game that the line has finished being delivered
            onLineFinishDisplaying?.Invoke();

            while (userRequestedNextLine == false)
            {
                yield return null;
            }

            // Avoid skipping lines if textSpeed == 0
            yield return new WaitForEndOfFrame();

            // Hide the text and prompt
            onLineEnd?.Invoke();

            // todo: check if this works
            onLineUpdate.RemoveListener(textItem.UpdateLine);

            onComplete();
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
            TextItem textItem = _textItems[_textIndex];
            textItem.CreateButtons(optionsCollection.Options.Length);

            // Display each option in a button, and make it visible
            int i = 0;

            waitingForOptionSelection = true;

            currentOptionSelectionHandler = selectOption;

            foreach (var optionString in optionsCollection.Options)
            {
                textItem.ActivateButtons(i, () => SelectOption(optionString.ID));

                var optionText = localisationProvider.GetLocalisedTextForLine(optionString.Line);

                if (optionText == null)
                {
                    Debug.LogWarning($"Option {optionString.Line.ID} doesn't have any localised text");
                    optionText = optionString.Line.ID;
                }

                textItem.SetButtonText(i, optionText);

                i++;
            }

            onOptionsStart?.Invoke();

            // Wait until the chooser has been used and then removed 
            while (waitingForOptionSelection)
            {
                yield return null;
            }

            textItem.HideAllButtons();

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
    }
}