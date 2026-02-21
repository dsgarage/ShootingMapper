using UnityEditor;
using UnityEngine;
using ShmupCreator.Runtime.Data;

namespace ShmupCreator.Editor.Windows
{
    public class ShmupEnemyEditorWindow : EditorWindow
    {
        private ShmupEnemyData _enemyData;
        private UnityEditor.Editor _editor;
        private Vector2 _scrollPos;

        [MenuItem("Shmup Creator/Enemy Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<ShmupEnemyEditorWindow>("Enemy Editor");
            window.minSize = new Vector2(400, 400);
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.LabelField("Enemy Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _enemyData = (ShmupEnemyData)EditorGUILayout.ObjectField(
                "Enemy Data", _enemyData, typeof(ShmupEnemyData), false);

            if (_enemyData != null)
            {
                EditorGUILayout.Space();
                UnityEditor.Editor.CreateCachedEditor(_enemyData, null, ref _editor);
                _editor.OnInspectorGUI();

                EditorGUILayout.Space();
                if (GUILayout.Button("Scene上でパスを編集"))
                {
                    // TODO: Phase 3 - SceneView Gizmoでパス編集を起動
                    Debug.Log("Path Editor は Phase 3 で実装予定です。");
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "ShmupEnemyData アセットを選択してください。\n" +
                    "Assets > Create > Shmup Creator > Enemy Data",
                    MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
