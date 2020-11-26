using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cinemachine;
using Gameplay;
using GameSystem;
using GameSystem.Save;
using Others;
using UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Yarn.Unity;
using Object = UnityEngine.Object;

namespace Dialog
{
    // todo: play current song being played??? on load
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CustomCommands : MonoBehaviour, SaveClientCallback
    {
        public GameConfiguration gameConfiguration;
        public MemoryStorage memoryStorage;
        public InputManager inputManager;
        public IconManager iconManager;
        public SpriteRenderer blackScreen;
        public LocationPlate locationPlate;

        [Header("Variables")] public float delayTime = 3f;
        public float fadeRate = 0.05f;

        [Header("Assets")] public AudioItem[] audioList;

        [Tooltip("Puzzles should have PuzzleParent script")]
        public PuzzleItem[] puzzleList;

        [FormerlySerializedAs("headerList")] 
        public BackgroundItem[] backgroundList;

        public ShelfItemData[] shelfItemDataList;

        public ShowableItemData[] showableItemDataList;

        [Header("Prefabs")] public GameObject prefabFadedAudio;

        [Header("Scene objects")]
        // Drag and drop your Dialogue Runner into this variable.
        public DialogueRunner dialogueRunner;

        public DialogueUIManager dialogueUiManager;

        private FadedAudio _lastAudio = null;

        private static readonly Stack<FadedAudio> Pool = new Stack<FadedAudio>();
        private List<ShelfItem> _shelfItemList = new List<ShelfItem>();
        private PuzzleParent _puzzle;
        private Action _onComplete;
        private CinemachineImpulseSource _impulseSignal;
        private ShelfItem _shownShelfItem;
        private List<ShowableItem> _itemShownList = new List<ShowableItem>();

        private const string PuzzleShelfArg = "shelf";
        private State _state = State.None;
        private float _alpha = 0f;
        private BackgroundScript[] _backgroundScriptList;

        private enum State
        {
            None,
            GameEnding
        }

        private SaveClient _saveClient;
        private string _lastAudioName;
        private BackgroundScript _currentBg;

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

                    if (_alpha >= 1f)
                    {
                        SceneManager.LoadScene("EndGameScene");
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void EnterStage(string[] parameter)
        {
            foreach (var characterName in parameter)
            {
                iconManager.EnterStage(characterName);
            }
        }

        private void ExitStage(string[] parameter)
        {
            foreach (var characterName in parameter)
            {
                iconManager.ExitStage(characterName);
            }
        }

        // Note: not using Coroutine to allow for smoother fade
        private void GameEnd(string[] parameters)
        {
            _lastAudio.FadeOut();
            blackScreen.gameObject.SetActive(true);
            _state = State.GameEnding;
        }

        private void HideDialogue(string[] parameters)
        {
            ShowElements(false);
        }

        private void ShowDialogue(string[] parameters)
        {
            ShowElements(true);
        }

        private void ShowItem(string[] parameters)
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

        private void HideItem(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                Debug.LogWarning("YarnCommand: ShowItem has not parameters");
                return;
            }

            if (parameters[0].Equals("all", StringComparison.InvariantCultureIgnoreCase))
            {
                ChangeHeader(new[]{"None"});
                return;
            }

            foreach (var parameter in parameters)
            {
                if (parameter.Equals("bg", StringComparison.InvariantCultureIgnoreCase))
                {
                    ChangeHeader(new[]{"None"});
                }
                else
                {
                    
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

        private void ResetSpeaker(string[] parameters)
        {
            Debug.Log($"ResetSpeaker: {string.Join(" ", parameters)}");
            if (parameters.Length == 1 && float.TryParse(parameters[0], out _))
            {
                iconManager.RemoveSpeaker(2);
            }
            else
            {
                iconManager.RemoveSpeaker(String.Join(" ", parameters));
            }
        }

        /// <summary>
        /// Call in YarnSpinner as:
        /// <<changeHeader headerName>>
        /// </summary>
        /// <example>
        /// <<changeHeader fishMarket>>
        /// </example>
        /// <param name="parameters">1 name for the header</param>
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
                    if (_currentBg != null)
                    {
                        _currentBg.Disappear();
                    }
                    _currentBg = bg;
                    _currentBg.gameObject.SetActive(true);
                    _currentBg.Appear();
                    locationPlate.SetLocation(bg.DisplayName);
                    return;
                }
            }

            Debug.LogWarning($"Header not found: {parameters[0]}");
        }

        private void Shake(string[] parameter)
        {
            if (gameConfiguration.shouldShake)
            {
                _impulseSignal.GenerateImpulse(gameConfiguration.ShakeStrength);
            }
        }

        private void DebugLog(string[] parameter)
        {
            Debug.LogWarning("Incoming warning from Yarn");
            Debug.LogWarning(string.Join(" ", parameter));
        }

        #region PlayAudio

        /// <summary>
        /// 
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
                if (!audioItem.name.ToUpper().Equals(searchTerm)) continue;

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

        public void ReturnAudio(FadedAudio fadedAudio)
        {
            Pool.Push(fadedAudio);
        }

        #endregion PlayAudio

        #region DoPuzzle

        private void ClearShelfItem(string[] parameters)
        {
            foreach (var shelfItem in _shelfItemList)
            {
                shelfItem.gameObject.SetActive(false);
            }
            
            // for (int i = _shelfItemList.Count - 1; i >= 0; i--)
            // {
            //     Destroy(_shelfItemList[i].gameObject);
            //     _shelfItemList.RemoveAt(i);
            // }

            // if (_shownShelfItem == null) return;
            // Destroy(_shownShelfItem.gameObject);
            _shownShelfItem = null;
        }

        private void DoPuzzle(string[] parameters, System.Action onComplete)
        {
            // todo??? puzzle shelf no args???
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
            foreach (var puzzleItem in puzzleList)
            {
                if (!puzzleItem.name.ToUpper().Equals(searchTerm)) continue;

                // todo hide elements
                ShowElements(false);
                _puzzle = Instantiate(puzzleItem.puzzlePrefab).GetComponent<PuzzleParent>();
                _onComplete = onComplete;
                _puzzle.SetCustomCommand(this);
                Debug.Assert(_puzzle != null);

                return;
            }

            Debug.LogWarning($"Puzzle name not found: {searchTerm}");
            onComplete.Invoke();
        }

        private void DoShelfPuzzle(string[] parameters)
        {
            // todo: make shelf appear

            // todo: give each item the call plus variable to change
            if (_shelfItemList.Count == 0)
            {
                foreach (var shelfItemData in shelfItemDataList)
                {
                    ShelfItem shelfItem = shelfItemData.CreateObject();
                    _shelfItemList.Add(shelfItem);
                    shelfItem.Initialize(shelfItemData, this);
                }
            }
            
            foreach (var _shelfItem in _shelfItemList)
            {
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

        public void InformShelfItemTouched(ShelfItem shelfItem)
        {
            _shownShelfItem = shelfItem;
            inputManager.SetInputState(InputState.Normal);

            bool isDone = true;
            for (int i = _shelfItemList.Count - 1; i >= 0; i--)
            {
                isDone = isDone && _shelfItemList[i].IsDone();

                if (shelfItem != _shelfItemList[i])
                {
                    _shelfItemList[i].gameObject.SetActive(false);
                    // Destroy(_shelfItemList[i].gameObject);
                }
                else
                {
                    shelfItem.Display();
                }
            }

            if (isDone)
            {
                // todo: remove hard code
                memoryStorage.SetValue("$puzzleDone", true);
            }

            ShowElements(true);
            _onComplete.Invoke();
        }

        public void InformPuzzleDone()
        {
            StartCoroutine(CoroutineInformPuzzleDone());
        }

        private IEnumerator CoroutineInformPuzzleDone()
        {
            yield return new WaitForSeconds(delayTime);
            Destroy(_puzzle.gameObject);
            UnblockYarn();
        }

        #endregion DoPuzzle

        public void ShowElements(bool shouldShow)
        {
            dialogueUiManager.ShowElements(shouldShow);
        }

        public void UnblockYarn()
        {
            ShowElements(true);
            _onComplete.Invoke();
        }

        private void ChangeIcon(string[] paremeters)
        {
            // todo: implement change icon
        }

        /* Example:
         public void Awake() {
        
            // Create a new command called 'camera_look', which looks at a target.
            dialogueRunner.AddCommandHandler(
                "camera_look",     // the name of the command
                CameraLookAtTarget // the method to run
            );
        }
        
        // The method that gets called when '<<camera_look>>' is run.
        private void CameraLookAtTarget(string[] parameters) {
        
            // Take the first parameter, and use it to find the object
            string targetName = parameters[0];
            GameObject target = GameObject.Find(targetName);
        
            // Log an error if we can't find it
            if (target == null) {
                Debug.LogError($"Cannot make camera look at {targetName}:" + 
                               "cannot find target");
                return;
            }
        
            // Make the main camera look at this target
            Camera.main.transform.LookAt(target.transform);
        } */

        public void WriteAutoSave()
        {
            _saveClient.autoSave.lastAudioName = _lastAudioName;
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
            return Object.Instantiate(prefabShelfItem).GetComponent<ShelfItem>();
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