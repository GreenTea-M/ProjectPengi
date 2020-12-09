using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Gameplay;
using GameSystem;
using GameSystem.Save;
using Others;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Yarn.Unity;
using Object = UnityEngine.Object;

namespace Dialog
{
    /// <summary>
    /// CustomCommand has all the functions that can be used as a command in Yarn.
    /// </summary>
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CustomCommands : MonoBehaviour, SaveClientCallback, PoolableInstantAudio.IPooler
    {
        public GameConfiguration gameConfiguration;

        [Header("Variables")] public float delayTime = 3f;
        public float fadeRate = 0.05f;

        [Header("Assets")] public AudioItem[] audioList;
        public GameObject sfxEffectPrefab;
        public BackgroundItem[] backgroundList;
        public ShelfItemData[] shelfItemDataList;
        public ShowableItemData[] showableItemDataList;
        public GameObject prefabFadedAudio;

        [Header("Scene objects")]
        // Drag and drop your Dialogue Runner into this variable.
        public DialogueRunner dialogueRunner;

        public MemoryStorage memoryStorage;
        public InputManager inputManager;
        public IconManager iconManager;
        public SpriteRenderer blackScreen;
        public LocationPlate locationPlate;
        public DialogueUIManager dialogueUiManager;

        private State _state = State.None;
        private float _alpha = 0f;
        private SaveClient _saveClient;
        private string _lastAudioName;
        private BackgroundScript _currentBg;
        private string _lastLocation;
        private float _screenTransitionDuration = 1f;
        private float _targetAlpha = 1f;
        private float _transitionStartTime = 0f;
        private float _startAlpha = 0f;
        private float _diffAlpha = 1f;

        private FadedAudio _lastAudio = null;
        private readonly List<ShelfItem> _shelfItemList = new List<ShelfItem>();
        private Action _onComplete;
        private CinemachineImpulseSource _impulseSignal;
        private ShelfItem _shownShelfItem;
        private readonly List<ShowableItem> _itemShownList = new List<ShowableItem>();
        private BackgroundScript[] _backgroundScriptList;
        private readonly Stack<PoolableInstantAudio> _instantAudioPool = new Stack<PoolableInstantAudio>();

        private readonly Stack<FadedAudio> Pool = new Stack<FadedAudio>();

        private const string PuzzleShelfArg = "shelf";

        private enum State
        {
            None,
            GameEnding,
            ScreenFadeTransition
        }

        private void OnEnable()
        {
            if (_saveClient == null)
            {
                _saveClient = gameConfiguration.RequestSaveAccess(this);
            }
        }

        private void OnDisable()
        {
            gameConfiguration.ReleaseSaveAccess(_saveClient);
            _saveClient = null;
        }

        private void Awake()
        {
            _impulseSignal = GetComponent<CinemachineImpulseSource>();

            Debug.Assert(gameConfiguration != null);
            Debug.Assert(dialogueRunner != null);
            Debug.Assert(dialogueUiManager != null);
            Debug.Assert(prefabFadedAudio != null);
            Debug.Assert(memoryStorage != null);
            Debug.Assert(inputManager != null);
            Debug.Assert(iconManager != null);
            Debug.Assert(blackScreen != null);

            if (_saveClient == null)
            {
                _saveClient = gameConfiguration.RequestSaveAccess(this);
            }

            // initialize backgrounds
            _backgroundScriptList = new BackgroundScript[backgroundList.Length];
            for (int i = 0; i < backgroundList.Length; i++)
            {
                var script = Instantiate(backgroundList[i].prefab)
                    .GetComponent<BackgroundScript>();
                Debug.Assert(script != null);
                script.SetData(backgroundList[i]);
                _backgroundScriptList[i] = script;
            }

            ChangeHeader(new[] {_saveClient.currentSave.lastHeader});
            PlayAudio(new[] {_saveClient.currentSave.lastAudioName});

            // special case: shelf item
            if (!_saveClient.currentSave.shownItem.Equals(""))
            {
                foreach (var shelfItem in shelfItemDataList)
                {
                    if (shelfItem.variableName.Equals(_saveClient.currentSave.shownItem))
                    {
                        Debug.Log($"Shown item: ");
                        var o = shelfItem.CreateObject();
                        _shownShelfItem = o;
                        o.Initialize(shelfItem, this);
                        o.Display();
                        break;
                    }
                }
            }

            // load all active characters
            EnterStage(_saveClient.currentSave.activeCharacterList);

            dialogueRunner.AddCommandHandler(
                "playAudio", // the name of the command
                PlayAudio // the method to run
            );

            dialogueRunner.AddCommandHandler("doPuzzle", DoPuzzle);
            dialogueRunner.AddCommandHandler("changeHeader", ChangeHeader);
            dialogueRunner.AddCommandHandler("changeBackground", ChangeHeader); // alias
            dialogueRunner.AddCommandHandler("shake", Shake);
            dialogueRunner.AddCommandHandler("debugLog", DebugLog);
            dialogueRunner.AddCommandHandler("clearShelfItem", ClearShelfItem);
            dialogueRunner.AddCommandHandler("resetSpeaker", ResetSpeaker);
            dialogueRunner.AddCommandHandler("removeSpeaker", ResetSpeaker);
            dialogueRunner.AddCommandHandler("showItem", ShowItem);
            dialogueRunner.AddCommandHandler("hideItem", HideItem);
            dialogueRunner.AddCommandHandler("showDialogue", ShowDialogue);
            dialogueRunner.AddCommandHandler("hideDialogue", HideDialogue);
            dialogueRunner.AddCommandHandler("gameEnd", GameEnd);
            dialogueRunner.AddCommandHandler("enterStage", EnterStage);
            dialogueRunner.AddCommandHandler("exitStage", ExitStage);
            dialogueRunner.AddCommandHandler("fakeLastDialog", FakeLastDialog);
            dialogueRunner.AddCommandHandler("playSFX", PlaySfx);
            dialogueRunner.AddCommandHandler("playSfx", PlaySfx);
            dialogueRunner.AddCommandHandler("playsfx", PlaySfx);
            dialogueRunner.AddCommandHandler("fadePlainBackground", FadePlainBackground);
        }

        private void Start()
        {
            blackScreen.gameObject.SetActive(false);
        }

        private void Update()
        {
            switch (_state)
            {
                case State.None:
                    break;
                case State.GameEnding:
                    _alpha += fadeRate * Time.deltaTime;
                    var blackScreenColor = blackScreen.color;
                    blackScreenColor.a = _alpha;
                    blackScreen.color = blackScreenColor;

                    if (_alpha >= _targetAlpha)
                    {
                        SceneManager.LoadScene(PengiConstants.SceneEndGame);
                    }

                    break;
                case State.ScreenFadeTransition:
                    _alpha = _startAlpha +
                             ((Time.time - _transitionStartTime) / _screenTransitionDuration) * _diffAlpha;
                    var screenColor = blackScreen.color;
                    screenColor.a = _alpha;
                    blackScreen.color = screenColor;

                    if (_startAlpha < _targetAlpha && _alpha > _targetAlpha)
                    {
                        _state = State.None;
                        _onComplete?.Invoke();
                        _onComplete = null;
                    }
                    else if (_startAlpha >= _targetAlpha && _alpha < _targetAlpha)
                    {
                        _state = State.None;
                        blackScreen.gameObject.SetActive(false);
                        _onComplete?.Invoke();
                        _onComplete = null;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Give instructions to showable items
        /// </summary>
        /// <example><<instructShownItem fish jump>></example>
        /// <param name="parameters">
        /// 1st element is shownItem name
        /// 2nd element is the name of the command
        /// </param>
        private void InstructShownItem(string[] parameters)
        {
            if (parameters.Length != 2)
            {
                Debug.LogWarning($"InstructShownItem: Insufficient instructions: {string.Join(" ", parameters)}");
                return;
            }

            foreach (var showableItem in _itemShownList)
            {
                if (showableItem.Match(parameters[0]))
                {
                    Debug.Log($"Instructing {parameters[0]} to {parameters[1]}");
                    showableItem.Instruct(parameters[1]);
                    return;
                }
            }

            Debug.LogWarning($"InstructShownItem: Current item not shown: {string.Join(" ", parameters)}");
        }

        /// <summary>
        /// Plays a sound effect once. Sound effects can stack on each other.
        /// </summary>
        /// <example><<playSfx sfxName>></example>
        /// <param name="parameters">The name of the audio item containing the AudioClip in audioList</param>
        private void PlaySfx(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                Debug.Log("PlaySFX: no parameter");
                return;
            }

            string searchTerm = parameters[0].ToUpper();
            foreach (var audioItem in audioList)
            {
                if (!audioItem.name.ToUpper().Equals(searchTerm)) continue;

                AudioClip audioClip = audioItem.audioClip;

                PoolableInstantAudio sfx;
                if (_instantAudioPool.Count == 0)
                {
                    sfx = Instantiate(sfxEffectPrefab).GetComponent<PoolableInstantAudio>();
                    sfx.Initialize();
                }
                else
                {
                    sfx = _instantAudioPool.Pop();
                }

                Debug.Assert(sfx != null);
                sfx.Play(this, audioClip);

                return;
            }

            Debug.Log($"Audio clip not found for: {searchTerm}");
        }

        /// <summary>
        /// EnterStage will allow a specified character to appear in the scene.
        /// </summary>
        /// <param name="parameter">The name or alias of the character</param>
        /// <remarks>You can call multiple characters in one command</remarks>
        /// <example><<enterStage B oldMole>></example>
        private void EnterStage(string[] parameter)
        {
            foreach (var characterName in parameter)
            {
                iconManager.EnterStage(characterName);
            }
        }

        /// <summary>
        /// ExitStage forces a character to leave the scene. After a character leaves a scene and
        /// if their name appears in the script, they will not appear and the narrator will
        /// take place of them speaking.
        /// </summary>
        /// <param name="parameter">The name or alias of the character</param>
        /// <remarks>You can call multiple characters in one command</remarks>
        /// <example><<exitStage B oldMole>></example>
        private void ExitStage(string[] parameter)
        {
            foreach (var characterName in parameter)
            {
                iconManager.ExitStage(characterName);
            }
        }

        /// <summary>
        /// Calling GameEnd will make the screen fade to black and open the credit scene.
        /// </summary>
        /// <param name="parameters"></param>
        /// <example><<gameEnd>></example>
        private void GameEnd(string[] parameters)
        {
            if (_lastAudio != null)
            {
                _lastAudio.FadeOut();
            }

            if (blackScreen != null)
            {
                blackScreen.gameObject.SetActive(true);
            }

            blackScreen.color = Color.black;
            _targetAlpha = 1f;
            _state = State.GameEnding;
        }

        /// <summary>
        /// Forces all characters to leave without the side effect of them not reappearing when
        /// they need to speak.
        /// </summary>
        /// <param name="parameters"></param>
        /// <example><<hideDialogue>></example>
        /// Note: the game had more distinct elements back then. Now, it's just a matter of making
        /// the characters leave the scene without the side effect of not making them not appear
        /// when it's their turn to speak.
        private void HideDialogue(string[] parameters)
        {
            ShowElements(false);
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="parameters"></param>
        /// <example><<hideDialogue>></example>
        /// Note: the game had more distinct elements back then. Now, it's just a matter of making
        /// the characters leave the scene without the side effect of not making them not appear
        /// when it's their turn to speak.
        private void ShowDialogue(string[] parameters)
        {
            ShowElements(true);
        }

        /// <summary>
        /// Makes a showableItem appear.
        /// </summary>
        /// <param name="parameters">Name of the showable item in the showableItemDataList</param>
        /// <example><<showItem shelf>></example>
        public void ShowItem(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                Debug.LogWarning("YarnCommand: ShowItem has not parameters");
                return;
            }

            foreach (var showableItemData in showableItemDataList)
            {
                if (showableItemData.Match(parameters[0]))
                {
                    var obj = Instantiate(showableItemData.prefab).GetComponent<ShowableItem>();
                    obj.SetData(showableItemData);
                    _itemShownList.Add(obj);
                    return;
                }
            }

            Debug.LogWarning($"ShowItem: Unknown item: {parameters[0]}");
        }

        /// <summary>
        /// Makes a showableItem disappear.
        /// </summary>
        /// <param name="parameters">Name of the showable item in the showableItemDataList</param>
        /// <example><<hideItem shelf>></example>
        public void HideItem(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                Debug.LogWarning("YarnCommand: HideItem has not parameters");
                return;
            }

            if (parameters[0].Equals("all", StringComparison.InvariantCultureIgnoreCase))
            {
                ChangeHeader(new[] {"None"});
                return;
            }

            foreach (var parameter in parameters)
            {
                if (parameter.Equals("bg", StringComparison.InvariantCultureIgnoreCase))
                {
                    ChangeHeader(new[] {"None"});
                }
            }

            for (int i = _itemShownList.Count - 1; i >= 0; i--)
            {
                if (_itemShownList[i].Match(parameters[0]))
                {
                    _itemShownList[i].SelfDestroy();
                    _itemShownList.RemoveAt(i);
                    return;
                }
            }

            Debug.LogWarning($"HideItem: Unknown item: {parameters[0]}");
        }

        /// <summary>
        /// Does not do anything.
        /// </summary>
        /// <param name="parameters"></param>
        /// <example></example>
        private void ResetSpeaker(string[] parameters)
        {
            Debug.LogWarning("ResetSpeaker does not do anything.");
        }

        /// <summary>
        /// Changes the current active header. It also animates the location plate.
        /// </summary>
        /// <example>
        /// <<changeHeader fishMarket>>
        /// </example>
        /// <param name="parameters">1st element if the name for the header</param>
        private void ChangeHeader(string[] parameters)
        {
            if (parameters.Length != 1)
            {
                Debug.LogWarning("changeHeader has no parameters");
                return;
            }

            Debug.Log($"Change Header: {parameters[0]}");

            string searchTerm = parameters[0];
            foreach (var bg in _backgroundScriptList)
            {
                if (bg.IsSimilar(searchTerm))
                {
                    var previousBg = "";

                    if (_currentBg != null)
                    {
                        _currentBg.Disappear();
                        previousBg = _currentBg.CodeName;
                    }

                    _lastLocation = searchTerm;
                    _currentBg = bg;
                    _currentBg.gameObject.SetActive(true);
                    _currentBg.Appear();
                    locationPlate.SetLocation(bg.DisplayName);

                    // special cases
                    // bad hard coding LMAO
                    if (searchTerm.ToLower().Equals("sunset"))
                    {
                        iconManager.UpdateAlternativeTextLocation(TextAlternativeLocationState.Sunset);
                    }

                    if (previousBg.ToLower().Equals("sunset"))
                    {
                        iconManager.UpdateAlternativeTextLocation(TextAlternativeLocationState.Default);
                    }

                    return;
                }
            }

            Debug.LogWarning($"Header not found: {parameters[0]}");
        }

        private void Shake(string[] parameter)
        {
            if (gameConfiguration.ShowVisualEffects)
            {
                _impulseSignal.GenerateImpulse(gameConfiguration.ShakeStrength);
            }
        }

        /// <summary>
        /// Prints a message into Debug.LogWarning()
        /// </summary>
        /// <param name="parameter">The message to be displayed in Debug.LogWarning()</param>
        private void DebugLog(string[] parameter)
        {
            Debug.LogWarning("Incoming warning from Yarn");
            Debug.LogWarning(string.Join(" ", parameter));
        }

        #region PlayAudio

        /// <summary>
        /// Plays the looping background music. In the game, only one background music can be played at a time.
        /// When invoked, it will attempt to fade out the previous audio, and fade in the current
        /// audio requested.
        /// </summary>
        /// <example>
        /// <<playAudio audioName>>
        /// </example>
        /// <param name="parameters"></param>
        private void PlayAudio(string[] parameters)
        {
            if (parameters.Length != 1)
            {
                return;
            }

            string searchTerm = parameters[0].ToUpper();
            foreach (var audioItem in audioList)
            {
                if (!audioItem.name.ToUpper().Equals(searchTerm))
                {
                    continue;
                }

                if (audioItem.audioClip == null)
                {
                    Debug.LogWarning($"playAudio: {audioItem.name} has no clip");
                    return;
                }

                if (_lastAudio != null)
                {
                    _lastAudio.FadeOut();
                }

                _lastAudio = GetNewAudio();
                _lastAudio.FadeIn(audioItem.audioClip, this);
                _lastAudioName = audioItem.name;

                return;
            }

            Debug.LogWarning($"Audio name not found: {searchTerm}");
        }

        /// <summary>
        /// Gets an unused audio from the pool or a newly instantiated one.
        /// </summary>
        /// <returns></returns>
        private FadedAudio GetNewAudio()
        {
            if (Pool.Count == 0)
            {
                var fadedAudio = Instantiate(prefabFadedAudio).GetComponent<FadedAudio>();
                Debug.Assert(fadedAudio != null);
                return fadedAudio;
            }
            else
            {
                return Pool.Pop();
            }
        }

        /// <summary>
        /// Receives an unused audio to be put back into the pool
        /// </summary>
        /// <param name="fadedAudio"></param>
        public void ReturnAudio(FadedAudio fadedAudio)
        {
            Pool.Push(fadedAudio);
        }

        #endregion PlayAudio

        #region DoPuzzle

        /// <summary>
        /// Makes all shelf item disappear
        /// </summary>
        /// <param name="parameters"></param>
        /// <example><<clearShelfItem>></example>
        private void ClearShelfItem(string[] parameters)
        {
            foreach (var shelfItem in _shelfItemList)
            {
                shelfItem.gameObject.SetActive(false);
            }

            _saveClient.autoSave.shownItem = "";
        }

        /// <summary>
        /// Use DoPuzzle in Yarn as <<doPuzzle puzzleName>>
        /// </summary>
        /// <param name="parameters">The only puzzle name supported is "shelf"</param>
        /// <param name="onComplete">onComplete will be invoked when the puzzle is done</param>
        /// Note: Originally, there were supposed to be puzzles. The puzzles would fall apart,
        /// and you would have to put them in their proper positions. Then, we just
        /// scrapped the idea. The current mechanic is a lot more simplified than
        /// what was originally planned.
        private void DoPuzzle(string[] parameters, System.Action onComplete)
        {
            if (parameters.Length == 0)
            {
                Debug.LogWarning("No such command doPuzzle with no arguments");
                return;
            }

            if (parameters[0].Equals(PuzzleShelfArg, StringComparison.InvariantCultureIgnoreCase))
            {
                _onComplete = onComplete;
                ShowElements(false);
                DoShelfPuzzle(parameters);
                return;
            }

            string searchTerm = parameters[0].ToUpper();
            Debug.LogWarning("Deprecated use for doPuzzle");
            Debug.LogWarning($"Puzzle name not found: {searchTerm}");
            onComplete.Invoke();
        }

        /// <summary>
        /// Makes the interactive shelf part in Byrnhilda's story appear.
        /// </summary>
        /// <param name="parameters"></param>
        private void DoShelfPuzzle(string[] parameters)
        {
            if (_shelfItemList.Count == 0)
            {
                if (_shownShelfItem != null)
                {
                    Destroy(_shownShelfItem.gameObject);
                }

                _shownShelfItem = null;

                foreach (var shelfItemData in shelfItemDataList)
                {
                    ShelfItem shelfItem = shelfItemData.CreateObject();
                    _shelfItemList.Add(shelfItem);
                    shelfItem.Initialize(shelfItemData, this);
                }
            }

            foreach (var _shelfItem in _shelfItemList)
            {
                _shelfItem.gameObject.SetActive(false); // force reload
                _shelfItem.gameObject.SetActive(true);
            }

            if (parameters.Length == 2 && parameters[1].Equals("full", StringComparison.InvariantCultureIgnoreCase))
            {
                _onComplete.Invoke();
            }
            else
            {
                inputManager.SetInputState(InputState.Shelving);
            }
        }

        /// <summary>
        /// Acts as an interface for shelf item to call when they are clicked.
        /// </summary>
        /// <param name="shelfItem"></param>
        public void InformShelfItemTouched(ShelfItem shelfItem)
        {
            _shownShelfItem = shelfItem;
            _saveClient.autoSave.shownItem = _shownShelfItem.ShelfItemName;
            inputManager.SetInputState(InputState.Normal);

            bool isDone = true;
            for (int i = _shelfItemList.Count - 1; i >= 0; i--)
            {
                isDone = isDone && _shelfItemList[i].IsDone();

                if (shelfItem != _shelfItemList[i])
                {
                    _shelfItemList[i].gameObject.SetActive(false);
                }
                else
                {
                    shelfItem.Display();
                }
            }

            if (isDone)
            {
                memoryStorage.SetValue(PengiConstants.PuzzleDoneVariableName, true);
            }

            ShowElements(true);
            _onComplete.Invoke();
        }

        #endregion DoPuzzle

        /// <summary>
        /// ShowElements can only make characters leave.
        /// </summary>
        /// <param name="shouldShow">
        /// This function will only work if you pass shouldShow as true
        /// </param>
        public void ShowElements(bool shouldShow)
        {
            dialogueUiManager.ShowElements(shouldShow);
        }

        /// <summary>
        /// Sets the last dialog to the given parameter without making the dialog
        /// appear on-screen.
        /// </summary>
        /// <param name="parameters">Fake dialogue separated by a space/param>
        /// <remarks>
        /// This is very useful for parts without dialog, and we still want
        /// to give context about the last dialog when saving.
        /// </remarks>
        /// <example><<fakeLastDialog I just finished cleaning up the shelf...>></example>
        private void FakeLastDialog(String[] parameters)
        {
            string message = "";
            string speaker = "";

            if (parameters.Length == 1)
            {
                message = parameters[0];
            }
            else if (parameters.Length > 1)
            {
                speaker = parameters[0];
                message = string.Join(" ", parameters.Skip(1));
            }

            dialogueUiManager.SetFakeLastDialog(speaker, message);
        }

        /// <summary>
        /// Fades in or fades out a plain foreground.
        /// </summary>
        /// <param name="parameters">
        /// 1st element: on or off
        /// - on will make the foreground appear
        /// - off will make the foreground disappear
        /// 2nd element: float as transition duration
        /// 3rd element: block?
        /// - if the 3rd element is block, the function will block yarn until it finishes its transition
        /// 4th element: the color of the background
        /// - Only white or black is supported
        ///
        /// If there are 7 elements:
        /// - 4th, 5th, 6th, 7th elements would be float values that correspond to r, g, b, a in Color
        /// </param>
        /// <param name="onComplete"></param>
        private void FadePlainBackground(string[] parameters, System.Action onComplete)
        {
            if (!gameConfiguration.ShowVisualEffects)
            {
                onComplete.Invoke();
                return;
            }

            if (parameters.Length == 0)
            {
                Debug.LogWarning("fadePlainBackground needs one argument");
                return;
            }

            bool shouldAppear = parameters[0].ToLower().Equals("on");
            _screenTransitionDuration = 1f;
            var color = UnityEngine.Color.white;
            _onComplete = null;

            // duration
            if (parameters.Length > 1)
            {
                _screenTransitionDuration = float.Parse(parameters[1]);
            }

            // should block?
            if (parameters.Length > 2 && parameters[2].ToLower().Equals("block"))
            {
                _onComplete = onComplete;
            }
            else
            {
                onComplete.Invoke();
            }

            // color: accept by word or by value
            if (parameters.Length == 4)
            {
                switch (parameters[3].ToLower())
                {
                    case "white":
                        color = Color.white;
                        break;
                    case "black":
                        color = Color.white;
                        break;
                    default:
                        Debug.LogWarning($"Unknown color: {parameters[3].ToLower()} in fadePlainBackground");
                        break;
                }
            }
            else if (parameters.Length == 7)
            {
                color = new Color(float.Parse(parameters[3]), float.Parse(parameters[4]),
                    float.Parse(parameters[5]), float.Parse(parameters[6]));
            }

            if (shouldAppear)
            {
                blackScreen.gameObject.SetActive(true);
                _targetAlpha = color.a;
                color.a = 0f;
                blackScreen.color = color;
                _state = State.ScreenFadeTransition;
                _startAlpha = 0f;
            }
            else if (blackScreen.gameObject.activeSelf)
            {
                _state = State.ScreenFadeTransition;
                _startAlpha = blackScreen.color.a;
                _targetAlpha = 0f;
            }

            _diffAlpha = _targetAlpha - _startAlpha;
            _transitionStartTime = Time.time;
        }

        /// <summary>
        /// Writes relevant savable information to auto save when requested.
        /// </summary>
        public void WriteAutoSave()
        {
            _saveClient.autoSave.lastAudioName = _lastAudioName;
            _saveClient.autoSave.lastHeader = _lastLocation;
        }

        /// <summary>
        /// Returns unused audio into the audio pool.
        /// </summary>
        /// <param name="finished"></param>
        public void ReturnInstantAudio(PoolableInstantAudio finished)
        {
            _instantAudioPool.Push(finished);
        }
    }


    [Serializable]
    public class AudioItem
    {
        public AudioClip audioClip;
        public string name;
    }

    [Serializable]
    public class PuzzleItem
    {
        public GameObject puzzlePrefab;
        public String name;
    }

    [Serializable]
    public class SpriteItem
    {
        public Sprite sprite;
        public string name;
    }

    [Serializable]
    public class ShelfItemData
    {
        public GameObject prefabShelfItem;
        public string name;
        public string variableName;

        public ShelfItem CreateObject()
        {
            Debug.Assert(prefabShelfItem != null);
            ShelfItem o = Object.Instantiate(prefabShelfItem).GetComponent<ShelfItem>();
            o.SetName(variableName);
            return o;
        }
    }

    [Serializable]
    public class ShowableItemData
    {
        public GameObject prefab;
        public string name;

        public bool Match(string s)
        {
            return string.Equals(s, name, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}