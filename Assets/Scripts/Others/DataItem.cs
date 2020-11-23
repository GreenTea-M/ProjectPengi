using System;
using UnityEngine;

namespace Others
{
    [Serializable]
    public class DataItem
    {
        [Header("Data item variables")]
        public string name;
        public string[] aliases;
        
        public bool IsSimilar(string speakerName)
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
}