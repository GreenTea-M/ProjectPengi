using System;
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

    [Header("Option variables")] 
    [Tooltip("The delay in seconds that each character shows up; If less than 0, show instantly")]
    public float textRate = 0.025f;
}