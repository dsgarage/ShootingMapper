using UnityEditor;
using UnityEngine;
using ShmupCreator.Editor.Styles;
using ShmupCreator.Runtime.Data;
using ShmupCreator.Runtime.Simulation;

namespace ShmupCreator.Editor.Gizmos
{
    /// <summary>
    /// SceneView上にレベル情報（エネミーパス・Wave配置・カメラ枠）を
    /// オーバーレイ表示するシステム。SHMUP Creator本家のエディタ内デバッグ表示に相当。
    /// </summary>
    [InitializeOnLoad]
    public static class ShmupSceneOverlay
    {
        private static bool _enabled = true;
        private static bool _showPaths = true;
        private static bool _showWaveZones = true;
        private static bool _showCameraWake = true;

        private const string PrefKeyEnabled = "ShmupCreator_SceneOverlay";

        static ShmupSceneOverlay()
        {
            _enabled = EditorPrefs.GetBool(PrefKeyEnabled, true);
            SceneView.duringSceneGui += OnSceneGUI;
        }

        [MenuItem("Shmup Creator/Scene Overlay/Toggle Overlay", false, 200)]
        private static void ToggleOverlay()
        {
            _enabled = !_enabled;
            EditorPrefs.SetBool(PrefKeyEnabled, _enabled);
            SceneView.RepaintAll();
        }

        [MenuItem("Shmup Creator/Scene Overlay/Show Paths", false, 201)]
        private static void TogglePaths() { _showPaths = !_showPaths; SceneView.RepaintAll(); }

        [MenuItem("Shmup Creator/Scene Overlay/Show Wave Zones", false, 202)]
        private static void ToggleWaveZones() { _showWaveZones = !_showWaveZones; SceneView.RepaintAll(); }

        [MenuItem("Shmup Creator/Scene Overlay/Show Camera Wake", false, 203)]
        private static void ToggleCameraWake() { _showCameraWake = !_showCameraWake; SceneView.RepaintAll(); }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!_enabled) return;

            // 全EnemyDataのパスを表示
            if (_showPaths) DrawAllEnemyPaths();

            // カメラウェイクゾーン
            if (_showCameraWake) DrawCameraWakeZone(sceneView);

            // SceneView上のHUD
            DrawOverlayHUD(sceneView);
        }

        private static void DrawAllEnemyPaths()
        {
            var guids = AssetDatabase.FindAssets("t:ShmupEnemyData");
            int colorIndex = 0;
            Color[] pathColors =
            {
                new Color(0.3f, 0.8f, 0.4f, 0.7f),
                new Color(0.4f, 0.6f, 1f, 0.7f),
                new Color(1f, 0.5f, 0.3f, 0.7f),
                new Color(1f, 0.8f, 0.2f, 0.7f),
                new Color(0.8f, 0.3f, 1f, 0.7f),
            };

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var enemy = AssetDatabase.LoadAssetAtPath<ShmupEnemyData>(path);
                if (enemy == null || enemy.movePath == null || enemy.movePath.Length < 2)
                    continue;

                var color = pathColors[colorIndex % pathColors.Length];
                Handles.color = color;

                // パスラインの描画
                for (int i = 0; i < enemy.movePath.Length - 1; i++)
                {
                    var from = new Vector3(enemy.movePath[i].x, enemy.movePath[i].y, 0);
                    var to = new Vector3(enemy.movePath[i + 1].x, enemy.movePath[i + 1].y, 0);
                    Handles.DrawLine(from, to);
                }

                // ウェイポイントドット
                for (int i = 0; i < enemy.movePath.Length; i++)
                {
                    var pos = new Vector3(enemy.movePath[i].x, enemy.movePath[i].y, 0);
                    Handles.DrawSolidDisc(pos, Vector3.forward, 0.1f);
                }

                // エネミー名ラベル
                var startPos = new Vector3(enemy.movePath[0].x, enemy.movePath[0].y, 0);
                Handles.Label(startPos + Vector3.up * 0.3f, enemy.name,
                    new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = color } });

                // パスプレビューアニメーション（シミュレータ使用）
                float t = (float)(EditorApplication.timeSinceStartup * 0.3 % 1.0);
                var animPos = PathEvaluator.EvaluateLinearPath(enemy.movePath, t);
                var animPos3 = new Vector3(animPos.x, animPos.y, 0);
                Handles.color = Color.white;
                Handles.DrawSolidDisc(animPos3, Vector3.forward, 0.15f);

                colorIndex++;
            }
        }

        private static void DrawCameraWakeZone(SceneView sceneView)
        {
            // カメラの視認範囲（エネミー起動ゾーン）を表示
            Handles.color = new Color(0.4f, 0.8f, 1f, 0.15f);
            float wakeExtent = 6f; // カメラ外の起動範囲
            var cam = sceneView.camera;
            if (cam == null) return;

            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            var center = sceneView.pivot;

            // 起動ゾーン枠
            var wakeColor = new Color(0.4f, 0.8f, 1f, 0.2f);
            Handles.color = wakeColor;
            Handles.DrawWireCube(center,
                new Vector3((halfW + wakeExtent) * 2, (halfH + wakeExtent) * 2, 0));
        }

        private static void DrawOverlayHUD(SceneView sceneView)
        {
            Handles.BeginGUI();

            // 右上にオーバーレイ情報
            float w = 180, h = 80;
            var rect = new Rect(sceneView.position.width - w - 10, 10, w, h);
            GUI.Box(rect, GUIContent.none);
            GUILayout.BeginArea(new Rect(rect.x + 4, rect.y + 4, w - 8, h - 8));

            GUILayout.Label("Shmup Overlay", EditorStyles.miniLabel);
            _showPaths = GUILayout.Toggle(_showPaths, "Paths", EditorStyles.miniButton);
            _showWaveZones = GUILayout.Toggle(_showWaveZones, "Wave Zones", EditorStyles.miniButton);
            _showCameraWake = GUILayout.Toggle(_showCameraWake, "Camera Wake", EditorStyles.miniButton);

            GUILayout.EndArea();
            Handles.EndGUI();
        }
    }
}
