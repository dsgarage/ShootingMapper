using UnityEditor;
using UnityEngine;
using ShmupCreator.Editor.Styles;
using ShmupCreator.Runtime.Data;

namespace ShmupCreator.Editor.Windows
{
    /// <summary>
    /// SHMUP Creator本家に準拠した HUD エディタ。
    /// 左:設定パネル / 右:HUDプレビュー の2カラム構成。
    /// </summary>
    public class ShmupHUDEditorWindow : EditorWindow
    {
        private ShmupHUDData _hudData;
        private Vector2 _scrollPos;
        private int _tab;
        private static readonly string[] TabNames = { "Score", "Life", "Gauges", "Font" };

        [MenuItem("Shmup Creator/HUD Editor", false, 15)]
        public static void ShowWindow()
        {
            var window = GetWindow<ShmupHUDEditorWindow>("HUD Editor");
            window.minSize = new Vector2(600, 400);
        }

        private void OnGUI()
        {
            // Toolbar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("HUD Editor", EditorStyles.boldLabel, GUILayout.Width(80));
            var newHud = (ShmupHUDData)EditorGUILayout.ObjectField(
                _hudData, typeof(ShmupHUDData), false, GUILayout.Width(200));
            if (newHud != _hudData) _hudData = newHud;
            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(22)))
            {
                var asset = ScriptableObject.CreateInstance<ShmupHUDData>();
                var path = EditorUtility.SaveFilePanelInProject("Create HUD", "NewHUD", "asset", "保存先");
                if (!string.IsNullOrEmpty(path)) { AssetDatabase.CreateAsset(asset, path); AssetDatabase.SaveAssets(); _hudData = asset; }
                else Object.DestroyImmediate(asset);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (_hudData == null)
            {
                EditorGUILayout.Space(30);
                EditorGUILayout.HelpBox("ShmupHUDData を選択してください。", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            DrawSettingsPanel();
            ShmupEditorStyles.DrawColumnSeparator();
            DrawPreviewPanel();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSettingsPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(280));
            _tab = ShmupEditorStyles.DrawTabBar(TabNames, _tab);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUI.BeginChangeCheck();
            switch (_tab)
            {
                case 0: DrawScoreTab(); break;
                case 1: DrawLifeTab(); break;
                case 2: DrawGaugesTab(); break;
                case 3: DrawFontTab(); break;
            }
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(_hudData);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawScoreTab()
        {
            ShmupEditorStyles.DrawScopeHeader("Score Display", true);
            EditorGUILayout.Space(4);
            if (_hudData.scoreDisplay == null) _hudData.scoreDisplay = new HUDElementData();
            _hudData.scoreDisplay.position = EditorGUILayout.Vector2Field("Position", _hudData.scoreDisplay.position);
            _hudData.scoreDisplay.size = EditorGUILayout.Vector2Field("Size", _hudData.scoreDisplay.size);
            _hudData.scoreDisplay.format = EditorGUILayout.TextField(
                new GUIContent("Format", "例: Score: {0}"), _hudData.scoreDisplay.format);
            _hudData.scoreDisplay.icon = (Sprite)EditorGUILayout.ObjectField("Icon", _hudData.scoreDisplay.icon, typeof(Sprite), false);
        }

        private void DrawLifeTab()
        {
            ShmupEditorStyles.DrawScopeHeader("Life Display", true);
            EditorGUILayout.Space(4);
            if (_hudData.lifeDisplay == null) _hudData.lifeDisplay = new HUDElementData();
            _hudData.lifeDisplay.position = EditorGUILayout.Vector2Field("Position", _hudData.lifeDisplay.position);
            _hudData.lifeDisplay.size = EditorGUILayout.Vector2Field("Size", _hudData.lifeDisplay.size);
            _hudData.lifeDisplay.icon = (Sprite)EditorGUILayout.ObjectField("Icon", _hudData.lifeDisplay.icon, typeof(Sprite), false);
        }

        private void DrawGaugesTab()
        {
            ShmupEditorStyles.DrawScopeHeader("Gauges", true);
            EditorGUILayout.Space(4);
            if (_hudData.gauges != null)
            {
                for (int i = 0; i < _hudData.gauges.Count; i++)
                {
                    var g = _hudData.gauges[i];
                    EditorGUILayout.LabelField($"Gauge [{i}]", ShmupEditorStyles.SubHeaderStyle);
                    g.label = EditorGUILayout.TextField("Label", g.label);
                    g.position = EditorGUILayout.Vector2Field("Position", g.position);
                    g.size = EditorGUILayout.Vector2Field("Size", g.size);
                    g.fillColor = EditorGUILayout.ColorField("Fill Color", g.fillColor);
                    g.backgroundColor = EditorGUILayout.ColorField("BG Color", g.backgroundColor);
                    ShmupEditorStyles.DrawSeparator();
                }
            }
            if (GUILayout.Button("+ Gauge"))
            {
                _hudData.gauges ??= new System.Collections.Generic.List<HUDGaugeData>();
                _hudData.gauges.Add(new HUDGaugeData());
            }
        }

        private void DrawFontTab()
        {
            ShmupEditorStyles.DrawScopeHeader("Font", true);
            EditorGUILayout.Space(4);
            _hudData.font = (Font)EditorGUILayout.ObjectField("Font", _hudData.font, typeof(Font), false);
        }

        private void DrawPreviewPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("HUD Preview", ShmupEditorStyles.SubHeaderStyle);

            var previewRect = GUILayoutUtility.GetRect(300, 200, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(previewRect, ShmupEditorStyles.DarkBg);

            // スコア表示プレビュー
            if (_hudData.scoreDisplay != null)
            {
                var scoreRect = new Rect(
                    previewRect.x + _hudData.scoreDisplay.position.x * previewRect.width,
                    previewRect.y + _hudData.scoreDisplay.position.y * previewRect.height,
                    Mathf.Max(80, _hudData.scoreDisplay.size.x), 20);
                EditorGUI.DrawRect(scoreRect, new Color(1, 1, 1, 0.1f));
                GUI.Label(scoreRect, string.Format(_hudData.scoreDisplay.format, "000000"),
                    new GUIStyle(EditorStyles.label) { normal = { textColor = Color.white }, fontSize = 12 });
            }

            // ライフ表示プレビュー
            if (_hudData.lifeDisplay != null)
            {
                var lifeRect = new Rect(
                    previewRect.x + _hudData.lifeDisplay.position.x * previewRect.width,
                    previewRect.y + _hudData.lifeDisplay.position.y * previewRect.height,
                    Mathf.Max(60, _hudData.lifeDisplay.size.x), 20);
                EditorGUI.DrawRect(lifeRect, new Color(1, 0.3f, 0.3f, 0.2f));
                GUI.Label(lifeRect, "♥♥♥",
                    new GUIStyle(EditorStyles.label) { normal = { textColor = Color.red }, fontSize = 14 });
            }

            // ゲージプレビュー
            if (_hudData.gauges != null)
            {
                foreach (var g in _hudData.gauges)
                {
                    var gRect = new Rect(
                        previewRect.x + g.position.x * previewRect.width,
                        previewRect.y + g.position.y * previewRect.height,
                        Mathf.Max(60, g.size.x), Mathf.Max(8, g.size.y));
                    EditorGUI.DrawRect(gRect, g.backgroundColor);
                    EditorGUI.DrawRect(new Rect(gRect.x, gRect.y, gRect.width * 0.7f, gRect.height), g.fillColor);
                    GUI.Label(gRect, g.label, new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.white } });
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}
