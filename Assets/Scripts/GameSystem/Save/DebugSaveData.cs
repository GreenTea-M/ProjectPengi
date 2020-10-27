using UnityEngine;

namespace Tomato.Core.GameSystem.Save
{
    [CreateAssetMenu(fileName = "SaveGameDebug", menuName = "ScriptableObjects/Debug/SaveGame", order = 0)]
    public class DebugSaveData : ScriptableObject
    {
        public SaveData saveData;
    }
}