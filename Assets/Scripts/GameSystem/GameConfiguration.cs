using System;
using GameSystem.Save;
using TMPro;
using Tomato.Core.GameSystem.Save;
using UnityEngine;

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
    
    [Header("Save data")]
    public SaveData saveData;

    [Header("Auto save (Do not touch)")] 
    public bool isSaveDirty = false;
    public SaveData autoSave;

    [Header("Other global stuff")] 
    public SaveIO saveIo;

    private void Awake()
    {
        saveIo = new SaveIO(this);
    }

    public float ShakeStrength => shouldShake ? shakeStrength : 0f;
    public float FontSize => fontSize;

    public void ResetSaveData()
    {
        saveData = new SaveData(baseConfiguration.saveData);
        autoSave = new SaveData(baseConfiguration.saveData);
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
}