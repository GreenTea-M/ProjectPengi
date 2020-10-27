using System;
using UnityEngine;

/// <summary>
/// This class holds variables that may affect overall gameplay and debugging features.
/// </summary>
[CreateAssetMenu(fileName = "GameConfiguration",
    menuName = "ScriptableObjects/Data/GameConfiguration")]
public class GameConfiguration : ScriptableObject
{
    [Header("Constants")] [Tooltip("Number of max save slots")]
    public int maxSaveSlots = 3;
}