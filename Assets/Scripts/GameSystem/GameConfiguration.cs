using System;
using GameSystem.Save;
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
    
    [Header("Save data")]
    public SaveData saveData;

    [Header("Auto save (Do not touch)")] 
    public bool isSaveDirty = false;
    public SaveData autoSave;

    public float ShakeStrength => shouldShake ? shakeStrength : 0f;

    [Header("Other global stuff")] 
    public SaveIO saveIo;

    private void Awake()
    {
        saveIo = new SaveIO(this);
    }

    public void ResetSaveData()
    {
        saveData = baseConfiguration.saveData;
    }
}