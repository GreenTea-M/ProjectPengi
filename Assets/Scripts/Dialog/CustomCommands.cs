using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cinemachine;
using Gameplay;
using GameSystem;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using Yarn.Unity;
using Object = UnityEngine.Object;

namespace Dialog
{
    // todo: play current song being played??? on load
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CustomCommands : MonoBehaviour
    {
        public GameConfiguration gameConfiguration;
        public MemoryStorage memoryStorage;
        public InputManager inputManager;

        [Header("Variables")] public float delayTime = 3f;

        [Header("Assets")] public AudioItem[] audioList;

        [Tooltip("Puzzles should have PuzzleParent script")]
        public PuzzleItem[] puzzleList;

        public SpriteItem[] headerList;

        public ShelfItemData[] shelfItemDataList;

        [Header("Prefabs")] public GameObject prefabFadedAudio;

        [Header("Scene objects")]
        // Drag and drop your Dialogue Runner into this variable.
        public DialogueRunner dialogueRunner;

        public DialogueUIManager dialogueUiManager;
        public SpriteRenderer headerSprite;

        private FadedAudio _lastAudio = null;

        private static readonly Stack<FadedAudio> Pool = new Stack<FadedAudio>();
        private List<ShelfItem> _shelfItemList = new List<ShelfItem>();
        private PuzzleParent _puzzle;
        private Action _onComplete;
        private CinemachineImpulseSource _impulseSignal;
        private ShelfItem _shownShelfItem;

        private const string PuzzleShelfArg = "shelf";

        private void Awake()
        {
            _impulseSignal = GetComponent<CinemachineImpulseSource>();

            Debug.Assert(gameConfiguration != null);
            Debug.Assert(dialogueRunner != null);
            Debug.Assert(dialogueUiManager != null);
            Debug.Assert(prefabFadedAudio != null);
            Debug.Assert(headerSprite != null);
            Debug.Assert(memoryStorage != null);
            Debug.Assert(inputManager != null);

            ChangeHeader(new[] {gameConfiguration.saveData.lastHeader});
            PlayAudio(new[] {gameConfiguration.saveData.lastAudioName});

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
                return;
            }

            string searchTerm = parameters[0].ToUpper();
            foreach (var item in headerList)
            {
                if (!item.name.ToUpper().Equals(searchTerm)) continue;

                headerSprite.sprite = item.sprite;

                return;
            }

            headerSprite.sprite = null; //  default behavior when no sprite found
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
                gameConfiguration.autoSave.lastAudioName = audioItem.name;

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
            if (_shownShelfItem == null) return;
            Destroy(_shownShelfItem.gameObject);
            _shownShelfItem = null;
        }

        private void DoPuzzle(string[] parameters, System.Action onComplete)
        {
            // todo??? puzzle shelf no args???
            if (parameters.Length != 1)
            {
                Debug.LogWarning("No such command doPuzzle with no arguments");
                _onComplete.Invoke();
                return;
            }

            if (parameters[0].Equals(PuzzleShelfArg, StringComparison.InvariantCultureIgnoreCase))
            {
                _onComplete = onComplete;
                ShowElements(false);
                DoShelfPuzzle();
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

        private void DoShelfPuzzle()
        {
            // todo: make shelf appear

            // todo: give each item the call plus variable to change
            _shelfItemList.Clear();
            Debug.Assert(shelfItemDataList.Length > 0);
            foreach (var shelfItemData in shelfItemDataList)
            {
                ShelfItem shelfItem = shelfItemData.CreateObject();
                _shelfItemList.Add(shelfItem);
                shelfItem.Initialize(shelfItemData, this);
            }

            inputManager.SetInputState(InputState.Shelving);
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
                    Destroy(_shelfItemList[i].gameObject);
                }
                else
                {
                    shelfItem.Display();
                }
                
                _shelfItemList.RemoveAt(i);
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
}