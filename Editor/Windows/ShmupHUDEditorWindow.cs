using UnityEditor;
using UnityEngine;
using ShmupCreator.Runtime.Data;

namespace ShmupCreator.Editor.Windows
{
    public class ShmupHUDEditorWindow : EditorWindow
    {
        private ShmupHUDData _hudData;
        private UnityEditor.Editor _editor;
        private Vector2 _scrollPos;

        [MenuItem("Shmup Creator/HUD Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<ShmupHUDEditorWindow>("HUD Editor");
            window.minSize = new Vector2(400, 300);
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.LabelField("HUD Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _hudData = (ShmupHUDData)EditorGUILayout.ObjectField(
                "HUD Data", _hudData, typeof(ShmupHUDData), false);

            if (_hudData != null)
            {
                EditorGUILayout.Space();
                UnityEditor.Editor.CreateCachedEditor(_hudData, null, ref _editor);
                _editor.OnInspectorGUI();

                // TODO: Phase 4 - HUDプレビュー描画
                EditorGUILayout.Space();
                var previewRect = GUILayoutUtility.GetRect(300, 200);
                EditorGUI.DrawRect(previewRect, new Color(0.1f, 0.1f, 0.15f));
                GUI.Label(previewRect, "HUD プレビュー\n（Phase 4で実装予定）",
                    new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = Color.gray }
                    });
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "ShmupHUDData アセットを選択してください。\n" +
                    "Assets > Create > Shmup Creator > HUD Data",
                    MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
