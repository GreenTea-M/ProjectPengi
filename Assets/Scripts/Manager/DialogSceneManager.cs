using System;
using Dialog;
using UnityEngine;
using Yarn.Unity;

namespace Manager
{
    /// <summary>
    /// 
    /// </summary>
    /// todo: create savable game (use the auto save file)
    /// todo: load save data from memory
    public class DialogSceneManager : MonoBehaviour
    {
        public GameConfiguration gameConfiguration;
        public DialogueRunner runner;
        public MemoryStorage memory;
        public DialogueUIManager dialogueUiManager;

        private void Awake()
        {
            Debug.Assert(gameConfiguration != null);
            Debug.Assert(runner != null);
            Debug.Assert(memory != null);
            
            // attach auto save node
            runner.onNodeStart.AddListener(AutoSaveNode);

            // todo: load all saved variable to here
            memory.defaultVariables = gameConfiguration.saveData.savedVariables.ToArray();
            memory.ResetToDefaults();
            
            runner.startNode = gameConfiguration.saveData.currentYarnNode;
        }

        private void AutoSaveNode(string currentNode)
        {
            // todo: save last node
            gameConfiguration.autoSave.currentYarnNode = currentNode;
            gameConfiguration.isSaveDirty = true;
            dialogueUiManager.RequestLastDialogWrite();

            // todo: save data
            memory.Write(gameConfiguration.autoSave);
        }
    }
}