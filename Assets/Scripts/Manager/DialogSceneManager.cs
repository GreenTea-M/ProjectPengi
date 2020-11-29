using System;
using Dialog;
using GameSystem;
using GameSystem.Save;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

namespace Manager
{
    /// <summary>
    /// 
    /// </summary>
    /// todo: create savable game (use the auto save file)
    /// todo: load save data from memory
    public class DialogSceneManager : MonoBehaviour, SaveClientCallback
    {
        public GameConfiguration gameConfiguration;
        public DialogueRunner runner;
        public MemoryStorage memory;
        public DialogueUIManager dialogueUiManager;
        public AutoSaveText autoSaveText;
        
        private SaveClient _saveClient;
        private GameInstance _gameInstance;
        private bool _isSaveDirty = false;
        private bool _isSaveDone = false;

        public bool IsSaveDone => _isSaveDone;

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
            Debug.Assert(gameConfiguration != null);
            Debug.Assert(runner != null);
            Debug.Assert(memory != null);
            Debug.Assert(dialogueUiManager != null);
            
            // set up
            if (_saveClient == null)
            {
                _saveClient = gameConfiguration.RequestSaveAccess(this);
            }
            
            runner.startNode = _saveClient.currentSave.currentYarnNode;
            _gameInstance = gameConfiguration.gameInstance;
            
            // attach auto save node
            runner.onNodeStart.AddListener(AutoSaveNode);
            
            memory.defaultVariables = _saveClient.currentSave.savedVariables.ToArray();
            memory.ResetToDefaults();
        }

        private void AutoSaveNode(string currentNode)
        {
            if (_isSaveDirty)
            {
                // write on save client
                _saveClient.autoSave.currentYarnNode = currentNode;
                _gameInstance.WriteOnAutoSave();
                autoSaveText.AnimateText();
                _isSaveDone = true;
            }
            
            _isSaveDirty = true;
        }

        public void WriteAutoSave()
        {
            memory.Write(_saveClient.autoSave);
        }

        public void GoBack()
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}