using UnityEditor;
using UnityEngine;
using ShmupCreator.Runtime.Data;

namespace ShmupCreator.Editor.Windows
{
    public class ShmupGameplayWindow : EditorWindow
    {
        private ShmupGameplayData _gameplayData;
        private UnityEditor.Editor _editor;
        private Vector2 _scrollPos;

        [MenuItem("Shmup Creator/Gameplay Rules")]
        public static void ShowWindow()
        {
            var window = GetWindow<ShmupGameplayWindow>("Gameplay Rules");
            window.minSize = new Vector2(400, 300);
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.LabelField("Gameplay Rules", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _gameplayData = (ShmupGameplayData)EditorGUILayout.ObjectField(
                "Gameplay Data", _gameplayData, typeof(ShmupGameplayData), false);

            if (_gameplayData != null)
            {
                EditorGUILayout.Space();
                UnityEditor.Editor.CreateCachedEditor(_gameplayData, null, ref _editor);
                _editor.OnInspectorGUI();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "ShmupGameplayData アセットを選択してください。\n" +
                    "Assets > Create > Shmup Creator > Gameplay Data",
                    MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
