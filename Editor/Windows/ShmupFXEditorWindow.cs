using UnityEditor;
using UnityEngine;

namespace ShmupCreator.Editor.Windows
{
    public class ShmupFXEditorWindow : EditorWindow
    {
        private Vector2 _scrollPos;

        [MenuItem("Shmup Creator/FX Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<ShmupFXEditorWindow>("FX Editor");
            window.minSize = new Vector2(400, 300);
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.LabelField("FX Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // TODO: Phase 4 - 爆発・パーティクルエフェクト編集
            EditorGUILayout.HelpBox(
                "爆発・パーティクルエフェクトエディタ\n（Phase 4で実装予定）",
                MessageType.Info);

            EditorGUILayout.EndScrollView();
        }
    }
}
