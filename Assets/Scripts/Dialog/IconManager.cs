using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using Dialog;
using Gameplay;
using GameSystem.Save;
using Others;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class IconManager : MonoBehaviour, SaveClientCallback
{
    public static string mainSpeakerName = "Pengi";
    
    public GameConfiguration gameConfiguration;
    public IconItem[] iconList;
    public CharacterData[] characterDataList;
    public CharacterData mainCharacter; // for none
    public CharacterData narratingCharacter; // for none
    public GameObject prefabCharacterIcon;
    public InformSpeakerReturn informSpeakerReturnValue = new InformSpeakerReturn();

    private PortraitItem _mainSpeaker;
    private PortraitItem _otherSpeaker;
    private int _portraitIndex = 0;
    private bool _isLeft = true;

    private const int PoolCapacity = 10;
    private readonly List<PortraitItem> _pool = new List<PortraitItem>(PoolCapacity);
    private SaveClient _saveClient;
    private UnifiedCharacterScript[] _characterList;
    private UnifiedCharacterScript _mainCharacter;
    private UnifiedCharacterScript _narratingCharacter;
    private List<UnifiedCharacterScript> _activeCharacterList = new List<UnifiedCharacterScript>();
    public UnifiedCharacterScript currentSpeaking;

    private void OnEnable()
    {
        if (_saveClient == null)
        {
            _saveClient = gameConfiguration.RequestSaveAccess(this);
        }
    }

    private void OnDisable()
    {
        gameConfiguration.ReleaseSaveAccess(_saveClient);
        _saveClient = null;
    }

    private void Awake()
    {
        Debug.Assert(prefabCharacterIcon != null);
        Debug.Assert(gameConfiguration != null);
        
        _characterList = new UnifiedCharacterScript[characterDataList.Length];
        for (int i = 0; i < characterDataList.Length; i++)
        {
            _characterList[i] = characterDataList[i].Instantiate(this);
        }

        _narratingCharacter = narratingCharacter.Instantiate(this);
        _mainCharacter = mainCharacter.Instantiate(this);

        for (int i = 0; i < PoolCapacity; i++)
        {
            var item = Instantiate(prefabCharacterIcon).GetComponent<PortraitItem>();
            Debug.Assert(item != null);
            _pool.Add(item);
        }

        if (_saveClient == null)
        {
            _saveClient = gameConfiguration.RequestSaveAccess(this);
        }

        _isLeft = _saveClient.currentSave.isLeft;
        InformSpeaker(_saveClient.currentSave.currentSpeaker, true);
        InformSpeaker(_saveClient.currentSave.previousSpeaker, true);
    }

    public IconItem GetSprite(string speakerName)
    {
        foreach (var iconItem in iconList)
        {
            if (iconItem.IsSimilar(speakerName))
            {
                return iconItem;
            }
        }

        Debug.LogWarning($"Speaker not found: {speakerName}. Defaulting...");
        return iconList[0];
    }

    public void RemoveSpeaker(int count)
    {
        int removedCount = 0;

        if (removedCount > count)
        {
            return;
        }

        if (_mainSpeaker != null)
        {
            _mainSpeaker.Leave();
            _mainSpeaker = null;
            removedCount++;
        }

        if (removedCount > count)
        {
            return;
        }

        if (_otherSpeaker != null)
        {
            _otherSpeaker.Leave();
            _otherSpeaker = null;
        }
    }

    public void RemoveSpeaker(string speakerName)
    {
        if (_otherSpeaker != null && _otherSpeaker.IsSameSpeaker(speakerName))
        {
            Debug.Log($"Speaker leaving: {speakerName}");
            _otherSpeaker.Leave();
            _otherSpeaker = null;
        }

        if (_mainSpeaker != null && _mainSpeaker.IsSameSpeaker(speakerName))
        {
            Debug.Log($"Speaker leaving: {speakerName}");
            _mainSpeaker.Leave();
            _mainSpeaker = null;
        }
    }

    public InformSpeakerReturn InformSpeaker(string candidateSpeaker)
    {
        return InformSpeaker(candidateSpeaker, false);
    }

    private void Speak(PortraitItem currentSpeaker)
    {
        if (currentSpeaker == null)
        {
            if (_mainSpeaker != null)
            {
                _mainSpeaker.Idle();
            }

            if (_otherSpeaker != null)
            {
                _otherSpeaker.Idle();
            }

            return;
        }

        var otherSpeaker = _mainSpeaker == currentSpeaker ? _otherSpeaker : _mainSpeaker;

        currentSpeaker.Speak();
        if (otherSpeaker != null)
        {
            otherSpeaker.Idle();
        }
    }

    private InformSpeakerReturn InformSpeaker(string candidateSpeaker, 
        bool isForced)
    {    
        // find out who is the active speaker
        // arrange accordingly
        if (_narratingCharacter.IsSimilar(candidateSpeaker))
        {
            currentSpeaking = _narratingCharacter;
        } else if (_mainCharacter.IsSimilar(candidateSpeaker))
        {
            currentSpeaking = _mainCharacter;
        }
        else
        {
            foreach (var character in _activeCharacterList)
            {
                if (character.IsSimilar(candidateSpeaker))
                {
                    currentSpeaking = character;
                    break;
                }
            }
        }

        _narratingCharacter.UpdateStatus();
        _mainCharacter.UpdateStatus();

        foreach (var character in _activeCharacterList)
        {
            character.UpdateStatus();
        }

        informSpeakerReturnValue.character = currentSpeaking;

        return informSpeakerReturnValue;
        
        /*var ret = new InformSpeakerReturn();
        candidateSpeaker = candidateSpeaker.Trim();

        if (candidateSpeaker.Equals(""))
        {
            // do nothing ??
            Speak(defaultCharacter);
            ret.isBlocking = false;
            return SaveState(ret);
        }

        if (_mainSpeaker != null && _mainSpeaker.IsSameSpeaker(candidateSpeaker))
        {
            ret.realName = _mainSpeaker.GetRealName();
            Speak(_mainSpeaker);
            return SaveState(ret);
        }

        if (_otherSpeaker != null && _otherSpeaker.IsSameSpeaker(candidateSpeaker))
        {
            ret.realName = _otherSpeaker.GetRealName();
            Speak(_otherSpeaker);
            return SaveState(ret);
        }

        // did not match current speakers
        var currentSpeaker = GetSpeakerPortrait(candidateSpeaker);
        if (currentSpeaker.IsSameSpeaker(mainSpeakerName))
        {
            // we know that main speaker is null
            _mainSpeaker = currentSpeaker;
            Speak(_mainSpeaker); // must be in here
            ret.isBlocking = !isForced;
            ret.realName = _mainSpeaker.GetRealName();
            return SaveState(ret);
        }

        if (_otherSpeaker != null)
        {
            // we have replace the other speaker
            _otherSpeaker.Leave();
        }

        _otherSpeaker = currentSpeaker;
        Speak(_otherSpeaker); // must be here
        ret.isBlocking = !isForced;
        ret.realName = _otherSpeaker.GetRealName();
        return SaveState(ret);*/
    }

// todo: delete
    private InformSpeakerReturn SaveState(InformSpeakerReturn ret)
    {
        return ret;
    }

    private PortraitItem GetSpeakerPortrait(string candidateSpeaker)
    {
        PortraitItem portraitItem = null;
        do
        {
            portraitItem = _pool[_portraitIndex];
            _portraitIndex = (_portraitIndex + 1) % PoolCapacity;
        } while (portraitItem.IsActive);
        portraitItem.Setup(GetSprite(candidateSpeaker), candidateSpeaker);
        return portraitItem;
    }

    public void ShowElements(bool shouldShow)
    {
        if (_otherSpeaker != null) _otherSpeaker.gameObject.SetActive(shouldShow);
        if (_mainSpeaker != null) _mainSpeaker.gameObject.SetActive(shouldShow);
    }

    public void WriteAutoSave()
    {
        _saveClient.autoSave.isLeft = !_isLeft;
        _saveClient.autoSave.currentSpeaker = _otherSpeaker != null
            ? _otherSpeaker.Speaker
            : "";
        _saveClient.autoSave.previousSpeaker = _mainSpeaker != null
            ? _mainSpeaker.Speaker
            : "";
    }

    public void EnterStage(string characterName)
    {
        foreach (var characterScript in _characterList)
        {
            if (characterScript.IsSimilar(characterName))
            {
                _activeCharacterList.Add(characterScript);
                return;
            }
        }
        
        Debug.LogWarning($"EnterStage: Character {characterName} not in character list");
    }

    public void ExitStage(string characterName)
    {
        for (int i = 0; i < _activeCharacterList.Count; i--)
        {
            if (_activeCharacterList[i].IsSimilar(characterName))
            {
                _activeCharacterList.RemoveAt(i);
                return;
            }
        }
        
        Debug.LogWarning($"ExitStage: Character {characterName} not in character list");
    }
}

[Serializable]
public class IconItem : DataItem
{
    [FormerlySerializedAs("sprite")] public Sprite mainSprite;
    public Sprite outlineSprite;
}

[Serializable]
public class CharacterData : DataItem
{
    public GameObject prefab;
    public CharacterType characterType = CharacterType.Side;

    public UnifiedCharacterScript Instantiate(IconManager iconManager)
    {
        var script = Object.Instantiate(prefab)
            .GetComponent<UnifiedCharacterScript>();
        Debug.Assert(script != null);
        script.SetData(this, iconManager);
        return script;
    }
}

public class InformSpeakerReturn
{
    public UnifiedCharacterScript character;
    public readonly DialogueBlocker dialogueBlocker = new DialogueBlocker();
    public string realName = "";
}

public class DialogueBlocker
{
    private bool _isBlocking = false;
    public bool IsBlocking => _isBlocking;

    public void Unblock()
    {
        _isBlocking = false;
    }

    public void Block()
    {
        _isBlocking = true;
    }
}