using System;
using System.Collections;
using System.Collections.Generic;
using Dialog;
using Gameplay;
using UnityEngine;
using UnityEngine.Serialization;

public class IconManager : MonoBehaviour
{
    public GameConfiguration gameConfiguration;
    public IconItem[] iconList;
    public GameObject prefabCharacterIcon;

    private PortraitItem _previousSpeaker;
    private PortraitItem _currentSpeaker;
    private int _portraitIndex = 0;
    private bool _isLeft = true;

    private const int PoolCapacity = 10;
    private readonly List<PortraitItem> Pool = new List<PortraitItem>(PoolCapacity);

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

        _isLeft = gameConfiguration.saveData.isLeft;
        InformSpeaker(gameConfiguration.saveData.currentSpeaker, true);
        InformSpeaker(gameConfiguration.saveData.previousSpeaker, true);
    }

    public IconItem GetSprite(string speakerName)
    {
        foreach (var iconItem in iconList)
        {
            if (string.Equals(speakerName, iconItem.name, StringComparison.CurrentCultureIgnoreCase))
            {
                return iconItem;
            }
        }

        return null;
    }

    public void RemoveSpeaker(int count)
    {
        // todo implement remove speaker
    }

    public bool InformSpeaker(string candidateSpeaker)
    {
        return InformSpeaker(candidateSpeaker, false);
    }


    private bool InformSpeaker(string candidateSpeaker, bool isForced)
    {
        candidateSpeaker = candidateSpeaker.Trim();

        if (candidateSpeaker.Equals(""))
        {
            // do nothing ??
            return SaveState(false);
        }

        if (_currentSpeaker == null)
        {
            _currentSpeaker = GetSpeakerPortrait(candidateSpeaker);
            if (!isForced)
            {
                _currentSpeaker.Speak();
            }

            return SaveState(true);
        }

        if (_currentSpeaker.IsSameSpeaker(candidateSpeaker))
        {
            // todo: change emotions???
            return SaveState(false);
        }

        bool isBlocking = false;
        PortraitItem newSpeaker = null;
        if (_previousSpeaker == null)
        {
            // do nothing lol
        }
        else if (_previousSpeaker.IsSameSpeaker(candidateSpeaker))
        {
            newSpeaker = _previousSpeaker;
            _isLeft = !_isLeft; // swap
        }
        else
        {
            _previousSpeaker.Leave();
        }

        _previousSpeaker = _currentSpeaker;
        _currentSpeaker = newSpeaker;

        if (_currentSpeaker != null && _currentSpeaker.IsSameSpeaker(candidateSpeaker))
        {
            // todo: change emotions???
        }
        else
        {
            _currentSpeaker = GetSpeakerPortrait(candidateSpeaker);
            isBlocking = true;
        }

        if (!isForced)
        {
            _previousSpeaker.Idle();
            _currentSpeaker.Speak();
        }

        return SaveState(isBlocking);
    }

    private bool SaveState(bool isBlocking)
    {
        gameConfiguration.autoSave.isLeft = !_isLeft;
        gameConfiguration.autoSave.currentSpeaker = _currentSpeaker != null
            ? _currentSpeaker.Speaker
            : "";
        gameConfiguration.autoSave.previousSpeaker = _previousSpeaker != null
            ? _previousSpeaker.Speaker
            : "";
        return isBlocking;
    }

    private PortraitItem GetSpeakerPortrait(string candidateSpeaker)
    {
        PortraitItem portraitItem = Pool[_portraitIndex];
        _portraitIndex = (_portraitIndex + 1) % PoolCapacity;
        portraitItem.Appear(GetSprite(candidateSpeaker), candidateSpeaker, _isLeft);
        _isLeft = !_isLeft;
        return portraitItem;
    }

    public void ShowElements(bool shouldShow)
    {
        if (_currentSpeaker != null) _currentSpeaker.gameObject.SetActive(shouldShow);
        if (_previousSpeaker != null) _previousSpeaker.gameObject.SetActive(shouldShow);
    }
}

[Serializable]
public class IconItem
{
    [FormerlySerializedAs("sprite")] public Sprite mainSprite;
    public Sprite outlineSprite;
    public String name;
}