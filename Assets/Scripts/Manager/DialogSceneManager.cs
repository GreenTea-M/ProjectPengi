using System;
using Dialog;
using GameSystem.Save;
using UnityEngine;
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
        
        private SaveClient _saveClient;
        private GameInstance _gameInstance;

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
            gameConfiguration.isSaveDirty = true;
            
            // write on save client
            _saveClient.autoSave.currentYarnNode = currentNode;
            _gameInstance.WriteOnAutoSave();
        }

        public void WriteAutoSave()
        {
            dialogueUiManager.RequestLastDialogWrite();
            memory.Write(_saveClient.autoSave);
        }
    }
}