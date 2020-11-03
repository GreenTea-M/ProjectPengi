using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconManager : MonoBehaviour
{
    public IconItem[] iconList;
    public Sprite GetSprite(string speakerName)
    {
        foreach (var iconItem in iconList)
        {
            if (string.Equals(speakerName, iconItem.name, StringComparison.CurrentCultureIgnoreCase))
            {
                return iconItem.sprite;
            }
        }

        return null;
    }
}

[Serializable]
public class IconItem
{
    public Sprite sprite;
    public String name;
}