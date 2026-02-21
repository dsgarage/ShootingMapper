using UnityEditor;
using UnityEngine;
using ShmupCreator.Runtime.Data;

namespace ShmupCreator.Editor.Windows
{
    public class ShmupWeaponEditorWindow : EditorWindow
    {
        private ShmupWeaponData _weaponData;
        private Vector2 _scrollPos;
        private bool _isSimulating;
        private float _simTime;

        [MenuItem("Shmup Creator/Weapon Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<ShmupWeaponEditorWindow>("Weapon Editor");
            window.minSize = new Vector2(800, 500);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            DrawPatternList();
            DrawPreviewPanel();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPatternList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            EditorGUILayout.LabelField("Weapon Editor", EditorStyles.boldLabel);

            _weaponData = (ShmupWeaponData)EditorGUILayout.ObjectField(
                "Weapon Data", _weaponData, typeof(ShmupWeaponData), false);

            if (_weaponData != null)
            {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

                EditorGUILayout.LabelField($"Fire Rate: {_weaponData.fireRate:F2}s");
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Bullet Patterns", EditorStyles.boldLabel);
                if (_weaponData.bulletPatterns != null)
                {
                    for (int i = 0; i < _weaponData.bulletPatterns.Count; i++)
                    {
                        var pattern = _weaponData.bulletPatterns[i];
                        if (pattern != null)
                        {
                            EditorGUILayout.LabelField($"  [{i}] {pattern.spreadType} - {pattern.bulletCount} bullets");
                        }
                    }
                }

                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "ShmupWeaponData アセットを選択してください。\n" +
                    "Assets > Create > Shmup Creator > Weapon Data",
                    MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPreviewPanel()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Bullet Preview", EditorStyles.boldLabel);

            // TODO: Phase 2 - 弾道プレビュー実装
            // - EditorGUILayoutのカスタム描画で弾の軌跡をリアルタイムシミュレート
            // - 発射形状セレクタ（扇形・円形・直線・ランダム）
            // - 複数パターンの重ね合わせ表示（色分け）
            var previewRect = GUILayoutUtility.GetRect(400, 400);
            EditorGUI.DrawRect(previewRect, new Color(0.1f, 0.1f, 0.15f));
            GUI.Label(previewRect, "弾道プレビュー\n（Phase 2で実装予定）",
                new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.gray }
                });

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(_isSimulating ? "Stop" : "Simulate"))
            {
                _isSimulating = !_isSimulating;
                _simTime = 0f;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void Update()
        {
            if (_isSimulating)
            {
                _simTime += 0.016f;
                Repaint();
            }
        }
    }
}
