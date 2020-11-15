using System;
using System.Collections.Generic;
using GameSystem.Save;
using Tomato.Core.GameSystem.Save;
using UnityEngine;
using Yarn.Unity;

/// <summary>
/// <c>GameInstance</c> primarily serves to contain several Subsystems or ServiceLocators
/// and it should only be used to access those.
/// </summary>
/// <remarks>
/// It may house some other variables for prototyping but those should be enclosed in either of the
/// two types of components that this Instance should hold.
/// </remarks>
/// <see cref="GameConfiguration"/>
public class GameInstance : MonoBehaviour
{
    private static GameInstance _instance = null;

    [Header("Variables")] [Tooltip("Used to access configurations that may be relevant for gameplay and debugging")]
    public GameConfiguration gameConfiguration;

    private List<SaveClient> saveClientList = new List<SaveClient>();
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        gameConfiguration.gameInstance = this;
    }

    public SaveClient RequestSaveAccess()
    {
        var client = new SaveClient();
        saveClientList.Add(client);
        return client;
    }

    public void RemoveSaveClient(SaveClient saveClient)
    {
        saveClientList.Remove(saveClient);
    }

    public void WriteOnAutoSave()
    {
        // make all writers write
        foreach (var saveClient in saveClientList)
        {
            saveClient.TryAutoSaveWrite();
        }
        gameConfiguration.SaveIo.RequestExecutor()
            .AtSlotIndex(0)
            .UsingSaveData(gameConfiguration.GetAutoSave())
            .OverwriteSlot();
    }
}