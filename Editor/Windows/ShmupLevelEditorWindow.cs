using UnityEditor;
using UnityEngine;
using ShmupCreator.Runtime.Data;

namespace ShmupCreator.Editor.Windows
{
    public class ShmupLevelEditorWindow : EditorWindow
    {
        private ShmupLevelData _levelData;
        private float _timelineZoom = 1f;
        private float _timelineScroll;
        private Vector2 _scrollPos;

        [MenuItem("Shmup Creator/Level Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<ShmupLevelEditorWindow>("Level Editor");
            window.minSize = new Vector2(800, 400);
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space();

            _levelData = (ShmupLevelData)EditorGUILayout.ObjectField(
                "Level Data", _levelData, typeof(ShmupLevelData), false);

            if (_levelData != null)
            {
                DrawTimeline();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "ShmupLevelData アセットを選択してください。\n" +
                    "Assets > Create > Shmup Creator > Level Data",
                    MessageType.Info);
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Level Editor", EditorStyles.boldLabel, GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            _timelineZoom = EditorGUILayout.Slider("Zoom", _timelineZoom, 0.1f, 5f, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTimeline()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            // TODO: Phase 2 - タイムラインUI実装
            // - 時間軸（X軸）にウェーブ・トリガーをドラッグ配置
            // - レイヤー（Y軸）でオブジェクト種別を分類
            // - スクラブバーでプレビュー再生ヘッド操作
            EditorGUILayout.HelpBox(
                "タイムラインエディタ（Phase 2で実装予定）\n" +
                "Wave数: " + (_levelData.waves != null ? _levelData.waves.Count : 0) + "\n" +
                "Trigger数: " + (_levelData.triggers != null ? _levelData.triggers.Count : 0),
                MessageType.Info);

            // Wave一覧の簡易表示
            if (_levelData.waves != null)
            {
                EditorGUILayout.LabelField("Waves", EditorStyles.boldLabel);
                for (int i = 0; i < _levelData.waves.Count; i++)
                {
                    var wave = _levelData.waves[i];
                    if (wave != null)
                    {
                        EditorGUILayout.LabelField($"  [{i}] {wave.name} - Time: {wave.spawnTime:F1}s, Count: {wave.count}");
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
