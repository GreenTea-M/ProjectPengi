using System;
using System.Collections.Generic;
using GameSystem.Save;
using TMPro;
using Tomato.Core.GameSystem.Save;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// This class holds variables that may affect overall gameplay and debugging features.
/// </summary>
/// todo: make prefabs rely on the values here and not on themselves
[CreateAssetMenu(fileName = "GameConfiguration",
    menuName = "ScriptableObjects/Data/GameConfiguration")]
public class GameConfiguration : ScriptableObject
{
    [Header("Constants")] [Tooltip("Number of max save slots")]
    public int maxSaveSlots = 3;
    [Tooltip("This will be the game configuration used on new game")]
    public GameConfiguration baseConfiguration;

    [Header("Option variables")] 
    [Tooltip("The delay in seconds that each character shows up; If less than 0, show instantly")]
    public float textRate = 0.025f;
    public float shakeStrength = 1f;
    public bool shouldShake = true;
    public TMP_FontAsset fontAsset;
    public float fontSize = 18f;
    [Range(0.6f,1f)]
    public float textOpacity = 0.97f;
    public Color fontColor = Color.black;
    public bool enableTextFormatting = true; // todo: implement
    
    [FormerlySerializedAs("saveData")]
    [Header("Save data")]
    [SerializeField]
    private SaveData currentSave;

    [Header("Auto save (Do not touch)")] 
    public bool isSaveDirty = false;
    [SerializeField]
    private SaveData autoSave;

    [Header("Other global stuff")] 
    private SaveIO saveIo;
    public GameInstance gameInstance;

    private void Awake()
    {
        saveIo = new SaveIO(this);
    }

    public float ShakeStrength => shouldShake ? shakeStrength : 0f;
    public float FontSize => fontSize;

    public SaveIO SaveIo => saveIo ?? (saveIo = new SaveIO(this));

    public void ResetSaveData()
    {
        currentSave.Overwrite(baseConfiguration.currentSave);
        autoSave.Overwrite(baseConfiguration.currentSave);
    }

    public void ResetOptions()
    {
        textRate = baseConfiguration.textRate;
        shouldShake = baseConfiguration.shouldShake;
        fontAsset = baseConfiguration.fontAsset;
        fontSize = baseConfiguration.fontSize;
        textOpacity = baseConfiguration.textOpacity;
        fontColor = baseConfiguration.fontColor;
        enableTextFormatting = baseConfiguration.enableTextFormatting;
    }

    public SaveClient RequestSaveAccess(SaveClientCallback saveClientCallback)
    {
        var saveClient = gameInstance.RequestSaveAccess();
        saveClient.currentSave = currentSave;
        saveClient.autoSave = autoSave;
        saveClient.saveClientCallback = saveClientCallback;
        return saveClient;
    }

    public void ReleaseSaveAccess(SaveClient saveClient)
    {
        gameInstance.RemoveSaveClient(saveClient);
    }

    public SaveData GetAutoSave()
    {
        return autoSave;
    }
}