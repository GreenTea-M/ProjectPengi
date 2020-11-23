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
    public CharacterData defaultCharacter; // for none
    public GameObject prefabCharacterIcon;

    private PortraitItem _mainSpeaker;
    private PortraitItem _otherSpeaker;
    private int _portraitIndex = 0;
    private bool _isLeft = true;

    private const int PoolCapacity = 10;
    private readonly List<PortraitItem> Pool = new List<PortraitItem>(PoolCapacity);
    private SaveClient _saveClient;
    private UnifiedCharacterScript[] characterList;

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
        
        characterList = new UnifiedCharacterScript[characterDataList.Length];
        for (int i = 0; i < characterDataList.Length; i++)
        {
            characterList[i] = characterDataList[i].Instantiate();
        }

        for (int i = 0; i < PoolCapacity; i++)
        {
            var item = Instantiate(prefabCharacterIcon).GetComponent<PortraitItem>();
            Debug.Assert(item != null);
            Pool.Add(item);
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

    private InformSpeakerReturn InformSpeaker(string candidateSpeaker, bool isForced)
    {
        InformSpeakerReturn ret;

        foreach (var characterScript in characterList)
        {
            InformSpeakerReturn value = characterScript.IsSimilar(candidateSpeaker);
            if (!value.IsNull())
            {
                ret = value;
            }
        }

        if (ret.IsNull())
        {
            ret = defaultCharacter.Activate();
        }
        else
        {
            defaultCharacter.Deactivate();
        }

        return ret;
        
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
            portraitItem = Pool[_portraitIndex];
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
    public CharacterType characterType = CharacterType.Narrator;

    public UnifiedCharacterScript Instantiate()
    {
        var script = Object.Instantiate(prefab)
            .GetComponent<UnifiedCharacterScript>();
        Debug.Assert(script != null);
        script.SetData(this);
        return script;
    }
}

public class InformSpeakerReturn
{
    public UnifiedCharacterScript character;
    public bool isBlocking = false;
    public string realName = "";
    public bool isNull = false;
    public bool IsNull => isNull;
}