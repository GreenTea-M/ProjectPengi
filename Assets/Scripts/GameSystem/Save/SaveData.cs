using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// This class holds all serializable variables for one save slot.
/// </summary>
/// <remarks>
/// Since this does not have an interface, constructing SaveData is done through static factory
/// methods.
/// </remarks>
[Serializable]
public class SaveData
{
    [FormerlySerializedAs("characterName")] [Tooltip("Name used in-game for main character's name")]
    public String mainName;

    [Header("Prototype artefacts")]
    // todo: remove this two values after removing save game prototypes
    public int testNumber;

    public String testString;

    private SaveData()
    {
    }

    protected internal static SaveData AsNull()
    {
        return new SaveData();
    }

    public static SaveData AsNewGame()
    {
        return new SaveData();
    }
}