using UnityEditor;
using UnityEngine;

namespace GameSystem.Editor
{
    [CustomEditor(typeof(GameConfiguration))]
    public class GameConfigurationEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUI.enabled = Application.isPlaying;

            GUILayout.Space(4f);
            GameConfiguration e = target as GameConfiguration;
            if (GUILayout.Button("Reset"))
                e.ResetSaveData();
        }
    }
}