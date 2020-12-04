using System;
using System.Collections.Generic;
using UI;
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
    public string lastAudioName = "fishMarket";
    public string lastHeader = "carpentry";
    
    public List<InMemoryVariableStorage.DefaultVariable> savedVariables = 
        new List<InMemoryVariableStorage.DefaultVariable>();
    
    public bool isLeft = true;
    public string currentSpeaker = "";
    public string previousSpeaker = "";
    public string[] activeCharacterList;
    public string shownItem = "";

    public SaveData(SaveData rhs)
    {
        Overwrite(rhs);
    }
    
    public SaveData()
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

    public void Overwrite(SaveData rhs)
    {
        name = rhs.name;
        currentYarnNode = rhs.currentYarnNode;
        lastDialog = rhs.lastDialog;
        lastAudioName = rhs.lastAudioName;
        lastHeader = rhs.lastHeader;
        isLeft = rhs.isLeft;
        currentSpeaker = rhs.currentSpeaker;
        previousSpeaker = rhs.previousSpeaker;
        shownItem = rhs.shownItem;

        // todo: test out for optimization
        savedVariables.Clear();
        foreach (var variable in rhs.savedVariables)
        {
            savedVariables.Add(new InMemoryVariableStorage.DefaultVariable()
            {
                name = variable.name,
                type = variable.type,
                value = variable.value
            });
        }
    }

    public void SetActiveSpeakerList(List<UnifiedCharacterScript> activeCharacters)
    {
        activeCharacterList = new String[activeCharacters.Count];
        for (int i = 0; i < activeCharacters.Count; i++)
        {
            activeCharacterList[i] = activeCharacters[i].RealName;
        }
    }
}