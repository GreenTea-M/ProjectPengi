using System;

namespace GameSystem.Save
{
    /// <summary>
    /// Player preferences POD
    /// </summary>
    [Serializable]
    public class PlayerPreferences
    {
        public float textRate = 0.0125f;
        public bool showVisualEffects = true;
        public int fontIndex = 0;
        public float fontSize = 11;
        public bool enableTextFormatting = true;
        public float volume = 0.75f;
    }
}