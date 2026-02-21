using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using ShmupCreator.Editor.Styles;
using ShmupCreator.Runtime.Data;

namespace ShmupCreator.Editor.Windows
{
    /// <summary>
    /// SHMUP Creator本家に準拠した空間ベースのレベルエディタ。
    /// 左:オブジェクトパレット / 中央:キャンバス / 右:プロパティパネル
    /// オブジェクトはキャンバス上に配置し、カメラスクロールで出現する。
    /// </summary>
    public class ShmupLevelEditorWindow : EditorWindow
    {
        private ShmupLevelData _levelData;
        private ShmupGameData _gameData;
        private Vector2 _canvasScroll;
        private Vector2 _paletteScroll;
        private Vector2 _propertyScroll;
        private float _zoom = 1f;
        private bool _showGrid = true;
        private int _selectedWaveIndex = -1;
        private int _paletteTab;

        private int _settingsTab;
        private static readonly string[] SettingsTabNames = { "Waves", "Backgrounds", "Triggers", "Settings" };
        private static readonly string[] PaletteTabNames = { "Enemies", "Triggers", "Sound", "Items" };

        private const string PrefKeyLevelGUID = "ShmupCreator_LevelDataGUID";
        private const float PaletteWidth = 160f;
        private const float PropertyWidth = 260f;

        [MenuItem("Shmup Creator/Level Editor %l", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<ShmupLevelEditorWindow>("Level Editor");
            window.minSize = new Vector2(900, 500);
        }

        public void SetLevel(ShmupLevelData level, ShmupGameData gameData)
        {
            _levelData = level;
            _gameData = gameData;
            _selectedWaveIndex = -1;
            SaveLevelRef();
            Repaint();
        }

        private void OnEnable()
        {
            RestoreLevelRef();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_levelData == null)
            {
                EditorGUILayout.Space(40);
                EditorGUILayout.HelpBox(
                    "Level Data を選択してください。\n" +
                    "Dashboard から「✎」ボタンで開くか、上部の ObjectField にドラッグしてください。",
                    MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            DrawPalettePanel();
            ShmupEditorStyles.DrawColumnSeparator();
            DrawCanvasPanel();
            ShmupEditorStyles.DrawColumnSeparator();
            DrawPropertyPanel();
            EditorGUILayout.EndHorizontal();
        }

        // --------------------------------------------------
        // Toolbar
        // --------------------------------------------------
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            var newLevel = (ShmupLevelData)EditorGUILayout.ObjectField(
                _levelData, typeof(ShmupLevelData), false, GUILayout.Width(200));
            if (newLevel != _levelData) { _levelData = newLevel; SaveLevelRef(); }

            GUILayout.Space(8);
            _showGrid = GUILayout.Toggle(_showGrid, new GUIContent("Grid(G)", "グリッド表示切替"),
                EditorStyles.toolbarButton, GUILayout.Width(56));
            _zoom = EditorGUILayout.Slider(_zoom, 0.25f, 3f, GUILayout.Width(120));
            GUILayout.FlexibleSpace();

            if (_levelData != null && _gameData != null)
            {
                GUI.backgroundColor = ShmupEditorStyles.AccentGreen;
                if (GUILayout.Button("▶ Test Level", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    int idx = _gameData.levels.IndexOf(_levelData);
                    ShmupPlayTestManager.StartPlayTest(_gameData, Mathf.Max(0, idx));
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();

            _settingsTab = ShmupEditorStyles.DrawTabBar(SettingsTabNames, _settingsTab);
        }

        // --------------------------------------------------
        // Left: Object Palette (Game Box)
        // --------------------------------------------------
        private void DrawPalettePanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(PaletteWidth));
            var bgRect = GUILayoutUtility.GetRect(PaletteWidth, 0);
            EditorGUI.DrawRect(new Rect(bgRect.x, bgRect.y, PaletteWidth, position.height), ShmupEditorStyles.PanelBg);
            EditorGUILayout.LabelField("Game Box", ShmupEditorStyles.SubHeaderStyle);

            _paletteTab = GUILayout.Toolbar(_paletteTab, PaletteTabNames, GUILayout.Height(22));
            _paletteScroll = EditorGUILayout.BeginScrollView(_paletteScroll);

            switch (_paletteTab)
            {
                case 0: DrawEnemyPalette(); break;
                case 1: DrawTriggerPalette(); break;
                case 2: DrawSoundPalette(); break;
                case 3: DrawItemPalette(); break;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawEnemyPalette()
        {
            EditorGUILayout.LabelField("Enemies", EditorStyles.miniLabel);
            var guids = AssetDatabase.FindAssets("t:ShmupEnemyData");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var enemy = AssetDatabase.LoadAssetAtPath<ShmupEnemyData>(path);
                if (enemy == null) continue;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(enemy.name, EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
                if (GUILayout.Button(new GUIContent("+", "Wave として追加"), GUILayout.Width(20)))
                    AddWaveForEnemy(enemy);
                EditorGUILayout.EndHorizontal();
            }
            ShmupEditorStyles.DrawSeparator();
            if (GUILayout.Button("+ New Enemy"))
                ShmupEnemyEditorWindow.ShowWindow();
        }

        private void DrawTriggerPalette()
        {
            EditorGUILayout.LabelField("Triggers", EditorStyles.miniLabel);
            foreach (var type in System.Enum.GetValues(typeof(TriggerType)))
            {
                if (GUILayout.Button($"+ {type}", EditorStyles.miniButton))
                    AddTrigger((TriggerType)type);
            }
        }

        private void DrawSoundPalette()
        {
            EditorGUILayout.LabelField("Sound", EditorStyles.miniLabel);
            EditorGUILayout.HelpBox("AudioClipをドラッグしてレベルに追加", MessageType.None);
        }

        private void DrawItemPalette()
        {
            EditorGUILayout.LabelField("Items", EditorStyles.miniLabel);
            var guids = AssetDatabase.FindAssets("t:ShmupItemData");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<ShmupItemData>(path);
                if (item != null)
                    GUILayout.Label($"  {item.name} ({item.type})", EditorStyles.miniLabel);
            }
        }

        // --------------------------------------------------
        // Center: Canvas (Spatial Level View)
        // --------------------------------------------------
        private void DrawCanvasPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            var canvasRect = GUILayoutUtility.GetRect(400, position.height - 70, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(canvasRect, ShmupEditorStyles.CanvasBg);

            if (_showGrid) DrawGrid(canvasRect);
            DrawCameraFrame(canvasRect);

            if (_settingsTab == 0 && _levelData.waves != null)
                DrawWavesOnCanvas(canvasRect);
            if (_settingsTab == 2 && _levelData.triggers != null)
                DrawTriggersOnCanvas(canvasRect);

            HandleCanvasInput(canvasRect);
            EditorGUILayout.EndVertical();
        }

        private void DrawGrid(Rect r)
        {
            float gs = 40f * _zoom;
            for (float x = r.x - (_canvasScroll.x % gs); x < r.xMax; x += gs)
                EditorGUI.DrawRect(new Rect(x, r.y, 1, r.height), ShmupEditorStyles.GridColor);
            for (float y = r.y - (_canvasScroll.y % gs); y < r.yMax; y += gs)
                EditorGUI.DrawRect(new Rect(r.x, y, r.width, 1), ShmupEditorStyles.GridColor);

            float ms = gs * 5f;
            for (float x = r.x - (_canvasScroll.x % ms); x < r.xMax; x += ms)
                EditorGUI.DrawRect(new Rect(x, r.y, 1, r.height), ShmupEditorStyles.GridMajorColor);
            for (float y = r.y - (_canvasScroll.y % ms); y < r.yMax; y += ms)
                EditorGUI.DrawRect(new Rect(r.x, y, r.width, 1), ShmupEditorStyles.GridMajorColor);
        }

        private void DrawCameraFrame(Rect r)
        {
            float fw = 200 * _zoom, fh = r.height * 0.8f;
            float cx = r.center.x - fw * 0.5f, cy = r.y + 20;
            var c = new Color(0.4f, 0.8f, 1f, 0.3f);
            EditorGUI.DrawRect(new Rect(cx, cy, fw, 1), c);
            EditorGUI.DrawRect(new Rect(cx, cy + fh, fw, 1), c);
            EditorGUI.DrawRect(new Rect(cx, cy, 1, fh), c);
            EditorGUI.DrawRect(new Rect(cx + fw, cy, 1, fh), c);
            GUI.Label(new Rect(cx + 4, cy + 2, 100, 16), "Camera View",
                new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = c } });
        }

        private void DrawWavesOnCanvas(Rect r)
        {
            for (int i = 0; i < _levelData.waves.Count; i++)
            {
                var wave = _levelData.waves[i];
                if (wave == null) continue;
                float yOff = wave.spawnTime * 40f * _zoom - _canvasScroll.y;
                float y = r.yMax - yOff - 20;
                if (y < r.y || y > r.yMax) continue;

                bool sel = (i == _selectedWaveIndex);
                var col = sel ? ShmupEditorStyles.AccentGreen : new Color(0.9f, 0.4f, 0.3f, 0.9f);
                var wr = new Rect(r.center.x - 40, y - 10, 80, 20);
                EditorGUI.DrawRect(wr, col);
                GUI.Label(wr, wave.name, new GUIStyle(EditorStyles.miniLabel)
                    { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } });

                if (Event.current.type == EventType.MouseDown && wr.Contains(Event.current.mousePosition))
                { _selectedWaveIndex = i; Event.current.Use(); Repaint(); }
            }
        }

        private void DrawTriggersOnCanvas(Rect r)
        {
            for (int i = 0; i < _levelData.triggers.Count; i++)
            {
                var t = _levelData.triggers[i];
                float y = r.yMax - (t.triggerTime * 40f * _zoom - _canvasScroll.y) - 20;
                if (y < r.y || y > r.yMax) continue;
                var tr = new Rect(r.x + 10, y - 8, r.width - 20, 16);
                EditorGUI.DrawRect(tr, new Color(0.9f, 0.75f, 0.2f, 0.3f));
                GUI.Label(tr, $"  {t.type} @ {t.triggerTime:F1}s",
                    new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = ShmupEditorStyles.LevelScopeColor } });
            }
        }

        private void HandleCanvasInput(Rect r)
        {
            var e = Event.current;
            if (!r.Contains(e.mousePosition)) return;
            if (e.type == EventType.ScrollWheel)
            { _canvasScroll.y += e.delta.y * 20f; _canvasScroll.y = Mathf.Max(0, _canvasScroll.y); e.Use(); Repaint(); }
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.G)
            { _showGrid = !_showGrid; e.Use(); Repaint(); }
        }

        // --------------------------------------------------
        // Right: Property Panel
        // --------------------------------------------------
        private void DrawPropertyPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(PropertyWidth));
            var bgRect = GUILayoutUtility.GetRect(PropertyWidth, 0);
            EditorGUI.DrawRect(new Rect(bgRect.x, bgRect.y, PropertyWidth, position.height), ShmupEditorStyles.PanelBg);
            ShmupEditorStyles.DrawScopeHeader("Properties", false);
            _propertyScroll = EditorGUILayout.BeginScrollView(_propertyScroll);

            switch (_settingsTab)
            {
                case 0: DrawWaveProperties(); break;
                case 1: DrawBackgroundProperties(); break;
                case 2: DrawTriggerProperties(); break;
                case 3: DrawLevelSettings(); break;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawWaveProperties()
        {
            if (_levelData.waves == null || _selectedWaveIndex < 0 || _selectedWaveIndex >= _levelData.waves.Count)
            {
                EditorGUILayout.HelpBox("Waveを選択してください", MessageType.None);
                if (_levelData.waves != null)
                    for (int i = 0; i < _levelData.waves.Count; i++)
                    { var w = _levelData.waves[i]; if (w != null && GUILayout.Button(w.name, EditorStyles.miniButton)) _selectedWaveIndex = i; }
                return;
            }
            var wave = _levelData.waves[_selectedWaveIndex];
            if (wave == null) return;
            EditorGUILayout.LabelField(wave.name, ShmupEditorStyles.SubHeaderStyle);
            ShmupEditorStyles.DrawSeparator();
            EditorGUI.BeginChangeCheck();
            wave.spawnTime = EditorGUILayout.FloatField(new GUIContent("Spawn Time", "出現時間(秒)"), wave.spawnTime);
            wave.count = EditorGUILayout.IntField(new GUIContent("Count", "同時出現数"), wave.count);
            wave.spacing = EditorGUILayout.FloatField(new GUIContent("Spacing", "出現間隔"), wave.spacing);
            wave.formation = (FormationType)EditorGUILayout.EnumPopup(new GUIContent("Formation", "隊列パターン"), wave.formation);
            wave.enemyData = (ShmupEnemyData)EditorGUILayout.ObjectField("Enemy", wave.enemyData, typeof(ShmupEnemyData), false);
            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(wave);
            EditorGUILayout.Space(4);
            if (wave.enemyData != null && GUILayout.Button("Enemy Editor で編集 →"))
            { var w = GetWindow<ShmupEnemyEditorWindow>("Enemy Editor"); w.SetEnemy(wave.enemyData); }
        }

        private void DrawBackgroundProperties()
        {
            EditorGUILayout.LabelField("Backgrounds", ShmupEditorStyles.SubHeaderStyle);
            if (_levelData.backgrounds != null)
                for (int i = 0; i < _levelData.backgrounds.Count; i++)
                {
                    var bg = _levelData.backgrounds[i];
                    EditorGUILayout.LabelField($"Layer {bg.layer}", EditorStyles.miniLabel);
                    bg.sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", bg.sprite, typeof(Sprite), false);
                    bg.scrollSpeed = EditorGUILayout.FloatField("Scroll Speed", bg.scrollSpeed);
                    bg.loop = EditorGUILayout.Toggle("Loop", bg.loop);
                    ShmupEditorStyles.DrawSeparator();
                }
            if (GUILayout.Button("+ Background Layer"))
            {
                Undo.RecordObject(_levelData, "Add Background");
                _levelData.backgrounds ??= new List<BackgroundEntry>();
                _levelData.backgrounds.Add(new BackgroundEntry { layer = _levelData.backgrounds.Count });
                EditorUtility.SetDirty(_levelData);
            }
        }

        private void DrawTriggerProperties()
        {
            EditorGUILayout.LabelField("Triggers", ShmupEditorStyles.SubHeaderStyle);
            if (_levelData.triggers != null)
                for (int i = 0; i < _levelData.triggers.Count; i++)
                {
                    var t = _levelData.triggers[i];
                    EditorGUILayout.BeginHorizontal();
                    t.type = (TriggerType)EditorGUILayout.EnumPopup(t.type, GUILayout.Width(120));
                    t.triggerTime = EditorGUILayout.FloatField(t.triggerTime, GUILayout.Width(60));
                    GUILayout.Label("s", GUILayout.Width(12));
                    EditorGUILayout.EndHorizontal();
                }
        }

        private void DrawLevelSettings()
        {
            ShmupEditorStyles.DrawScopeHeader("Level Settings", false);
            EditorGUI.BeginChangeCheck();
            _levelData.levelName = EditorGUILayout.TextField("Level Name", _levelData.levelName);
            _levelData.duration = EditorGUILayout.FloatField(new GUIContent("Duration (s)", "レベル全長"), _levelData.duration);
            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(_levelData);
        }

        // --------------------------------------------------
        // Data Operations
        // --------------------------------------------------
        private void AddWaveForEnemy(ShmupEnemyData enemy)
        {
            if (_levelData == null) return;
            var wave = ScriptableObject.CreateInstance<ShmupWaveData>();
            wave.enemyData = enemy; wave.spawnTime = _canvasScroll.y / (40f * _zoom); wave.count = 3;
            var path = EditorUtility.SaveFilePanelInProject("Create Wave", $"Wave_{enemy.name}", "asset", "保存先を選択");
            if (string.IsNullOrEmpty(path)) { Object.DestroyImmediate(wave); return; }
            AssetDatabase.CreateAsset(wave, path);
            Undo.RecordObject(_levelData, "Add Wave");
            _levelData.waves ??= new List<ShmupWaveData>();
            _levelData.waves.Add(wave);
            EditorUtility.SetDirty(_levelData);
            AssetDatabase.SaveAssets();
        }

        private void AddTrigger(TriggerType type)
        {
            if (_levelData == null) return;
            Undo.RecordObject(_levelData, "Add Trigger");
            _levelData.triggers ??= new List<TriggerEntry>();
            _levelData.triggers.Add(new TriggerEntry { type = type, triggerTime = _canvasScroll.y / (40f * _zoom) });
            EditorUtility.SetDirty(_levelData);
        }

        private void SaveLevelRef()
        {
            if (_levelData != null)
                EditorPrefs.SetString(PrefKeyLevelGUID, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_levelData)));
        }

        private void RestoreLevelRef()
        {
            if (!EditorPrefs.HasKey(PrefKeyLevelGUID)) return;
            var path = AssetDatabase.GUIDToAssetPath(EditorPrefs.GetString(PrefKeyLevelGUID));
            if (!string.IsNullOrEmpty(path)) _levelData = AssetDatabase.LoadAssetAtPath<ShmupLevelData>(path);
        }
    }
}
