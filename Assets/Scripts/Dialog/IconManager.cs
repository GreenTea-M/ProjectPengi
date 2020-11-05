using System;
using System.Collections;
using System.Collections.Generic;
using Dialog;
using Gameplay;
using UnityEngine;
using UnityEngine.Serialization;

public class IconManager : MonoBehaviour
{
    public IconItem[] iconList;
    public GameObject prefabCharacterIcon;

    private PortraitItem _previousSpeaker;
    private PortraitItem _currentSpeaker;
    private bool _isLeft = true;
    private int portraitIndex = 0;

    private const int PoolCapacity = 10;
    private readonly int HashGoLeft = 0;
    private readonly List<PortraitItem> Pool = new List<PortraitItem>(PoolCapacity);

    private void Awake()
    {
        Debug.Assert(prefabCharacterIcon != null);

        for (int i = 0; i < PoolCapacity; i++)
        {
            var item = Instantiate(prefabCharacterIcon).GetComponent<PortraitItem>();
            Debug.Assert(item != null);
            Pool.Add(item);
        }
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
        candidateSpeaker = candidateSpeaker.Trim();

        if (candidateSpeaker.Equals(""))
        {
            // do nothing ??
            return false;
        }

        if (_currentSpeaker == null)
        {
            _currentSpeaker = GetSpeakerPortrait(candidateSpeaker);
            _currentSpeaker.Speak();
            return true;
        }

        if (_currentSpeaker.IsSameSpeaker(candidateSpeaker))
        {
            // todo: change emotions???
            return false;
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
        
        _previousSpeaker.Idle();
        _currentSpeaker.Speak();

        return isBlocking;
    }

    private PortraitItem GetSpeakerPortrait(string candidateSpeaker)
    {
        PortraitItem portraitItem = Pool[portraitIndex];
        portraitIndex = (portraitIndex + 1) % PoolCapacity;
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