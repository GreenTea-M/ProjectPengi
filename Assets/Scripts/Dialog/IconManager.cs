using System;
using System.Collections;
using System.Collections.Generic;
using Dialog;
using Gameplay;
using GameSystem.Save;
using UnityEngine;
using UnityEngine.Serialization;

public class IconManager : MonoBehaviour, SaveClientCallback
{
    public static string mainSpeakerName = "Pengi";
    
    public GameConfiguration gameConfiguration;
    public IconItem[] iconList;
    public GameObject prefabCharacterIcon;

    private PortraitItem _mainSpeaker;
    private PortraitItem _otherSpeaker;
    private int _portraitIndex = 0;
    private bool _isLeft = true;

    private const int PoolCapacity = 10;
    private readonly List<PortraitItem> Pool = new List<PortraitItem>(PoolCapacity);
    private SaveClient _saveClient;

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
            if (iconItem.IsSpeaker(speakerName))
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
        var ret = new InformSpeakerReturn();
        candidateSpeaker = candidateSpeaker.Trim();

        if (candidateSpeaker.Equals(""))
        {
            // do nothing ??
            Speak(null);
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
        return SaveState(ret);
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
public class IconItem
{
    [FormerlySerializedAs("sprite")] public Sprite mainSprite;
    public Sprite outlineSprite;
    public String name;
    public String[] aliases;

    public bool IsSpeaker(string speakerName)
    {
        if (string.Equals(speakerName, name, StringComparison.CurrentCultureIgnoreCase))
        {
            return true;
        }

        foreach (var alias in aliases)
        {
            if (string.Equals(speakerName, alias, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

public class InformSpeakerReturn
{
    public bool isBlocking = false;
    public string realName = "";
}