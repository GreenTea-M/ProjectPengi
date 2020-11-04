using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Yarn.Unity;

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
    public string name = "";
    public string currentYarnNode = "Start";
    public string lastDialog = "";
    public Sprite lastSprite = null;
    public string lastAudioName = "fishMarket";
    public string lastHeader = "carpentry";
    
    public List<InMemoryVariableStorage.DefaultVariable> savedVariables;

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