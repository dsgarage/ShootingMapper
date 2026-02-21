using UnityEditor;
using UnityEngine;
using ShmupCreator.Runtime.Data;

namespace ShmupCreator.Editor.Windows
{
    public class ShmupGameSettingsWindow : EditorWindow
    {
        private ShmupGameData _gameData;
        private UnityEditor.Editor _editor;
        private Vector2 _scrollPos;

        [MenuItem("Shmup Creator/Game Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<ShmupGameSettingsWindow>("Game Settings");
            window.minSize = new Vector2(400, 300);
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.LabelField("Game Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _gameData = (ShmupGameData)EditorGUILayout.ObjectField(
                "Game Data", _gameData, typeof(ShmupGameData), false);

            if (_gameData != null)
            {
                EditorGUILayout.Space();
                UnityEditor.Editor.CreateCachedEditor(_gameData, null, ref _editor);
                _editor.OnInspectorGUI();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "ShmupGameData アセットを選択するか、新規作成してください。\n" +
                    "Assets > Create > Shmup Creator > Game Data",
                    MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
