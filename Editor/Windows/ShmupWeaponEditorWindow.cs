using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using ShmupCreator.Editor.Styles;
using ShmupCreator.Runtime.Data;
using ShmupCreator.Runtime.Simulation;

namespace ShmupCreator.Editor.Windows
{
    /// <summary>
    /// SHMUP Creator本家に準拠した4カラム構成の武器エディタ。
    /// Sets → Weapons → Properties → Preview のドリルダウンレイアウト。
    /// リアルタイム弾道プレビューを搭載。
    /// </summary>
    public class ShmupWeaponEditorWindow : EditorWindow
    {
        // --- Data ---
        private ShmupWeaponData _weaponData;
        private int _selectedPatternIndex = -1;
        private ShmupBulletPatternData _selectedPattern;

        // --- Preview Simulation ---
        private bool _isSimulating;
        private float _simTime;
        private double _lastUpdateTime;
        private List<BulletSimulator.BulletState> _simBullets = new List<BulletSimulator.BulletState>();
        private bool _previewSound = true;

        // --- Scroll ---
        private Vector2 _weaponListScroll;
        private Vector2 _propertyScroll;

        // --- Pattern Property Tabs ---
        private int _patternTab;
        private static readonly string[] PatternTabNames = { "Burst", "Bullets" };

        // --- Column Widths ---
        private const float WeaponListWidth = 180f;
        private const float PropertyWidth = 240f;

        private const string PrefKeyWeaponGUID = "ShmupCreator_WeaponDataGUID";

        [MenuItem("Shmup Creator/Weapon Editor %w", false, 11)]
        public static void ShowWindow()
        {
            var window = GetWindow<ShmupWeaponEditorWindow>("Weapon Editor");
            window.minSize = new Vector2(860, 480);
        }

        public void SetWeapon(ShmupWeaponData weapon)
        {
            _weaponData = weapon;
            _selectedPatternIndex = -1;
            _selectedPattern = null;
            SaveWeaponRef();
            Repaint();
        }

        private void OnEnable()
        {
            RestoreWeaponRef();
            EditorApplication.update += SimulationUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= SimulationUpdate;
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_weaponData == null)
            {
                EditorGUILayout.Space(40);
                EditorGUILayout.HelpBox(
                    "ShmupWeaponData アセットを選択するか、「+」ボタンで新規作成してください。",
                    MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();

            // Column 1: Weapon / Pattern List
            DrawWeaponListColumn();
            ShmupEditorStyles.DrawColumnSeparator();

            // Column 2: Pattern Properties
            DrawPropertyColumn();
            ShmupEditorStyles.DrawColumnSeparator();

            // Column 3: Real-time Preview
            DrawPreviewColumn();

            EditorGUILayout.EndHorizontal();
        }

        // --------------------------------------------------
        // Toolbar
        // --------------------------------------------------
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUILayout.LabelField("Weapon Editor", EditorStyles.boldLabel, GUILayout.Width(100));

            var newWeapon = (ShmupWeaponData)EditorGUILayout.ObjectField(
                _weaponData, typeof(ShmupWeaponData), false, GUILayout.Width(200));
            if (newWeapon != _weaponData)
            {
                _weaponData = newWeapon;
                _selectedPatternIndex = -1;
                _selectedPattern = null;
                SaveWeaponRef();
            }

            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(22)))
            {
                CreateNewWeapon();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        // --------------------------------------------------
        // Column 1: Weapon / Pattern List
        // --------------------------------------------------
        private void DrawWeaponListColumn()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(WeaponListWidth));
            var bgRect = GUILayoutUtility.GetRect(WeaponListWidth, 0);
            EditorGUI.DrawRect(new Rect(bgRect.x, bgRect.y, WeaponListWidth, position.height),
                ShmupEditorStyles.PanelBg);

            // Weapon info
            ShmupEditorStyles.DrawScopeHeader("Weapon", true);
            EditorGUILayout.Space(2);

            EditorGUI.BeginChangeCheck();
            var newRate = EditorGUILayout.FloatField(
                new GUIContent("Fire Rate", "発射間隔（秒）"), _weaponData.fireRate);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_weaponData, "Change Fire Rate");
                _weaponData.fireRate = newRate;
                EditorUtility.SetDirty(_weaponData);
            }

            _weaponData.fireSound = (AudioClip)EditorGUILayout.ObjectField(
                "Sound", _weaponData.fireSound, typeof(AudioClip), false);

            ShmupEditorStyles.DrawSeparator();

            // Pattern list
            EditorGUILayout.LabelField("Bullet Patterns", ShmupEditorStyles.SubHeaderStyle);
            _weaponListScroll = EditorGUILayout.BeginScrollView(_weaponListScroll);

            if (_weaponData.bulletPatterns != null)
            {
                for (int i = 0; i < _weaponData.bulletPatterns.Count; i++)
                {
                    var pattern = _weaponData.bulletPatterns[i];
                    if (pattern == null) continue;

                    bool isSelected = (i == _selectedPatternIndex);
                    var style = isSelected
                        ? ShmupEditorStyles.ListItemSelected
                        : ShmupEditorStyles.ListItem;

                    EditorGUILayout.BeginHorizontal(style);

                    // Visibility toggle
                    EditorGUILayout.LabelField(
                        isSelected ? "▶" : "  ", GUILayout.Width(16));

                    if (GUILayout.Button(pattern.name, EditorStyles.label, GUILayout.ExpandWidth(true)))
                    {
                        _selectedPatternIndex = i;
                        _selectedPattern = pattern;
                    }

                    GUILayout.Label($"{pattern.spreadType}", EditorStyles.miniLabel, GUILayout.Width(40));

                    // 上下移動ボタン
                    GUI.enabled = i > 0;
                    if (GUILayout.Button("↑", GUILayout.Width(20)))
                        SwapPatterns(i, i - 1);
                    GUI.enabled = i < _weaponData.bulletPatterns.Count - 1;
                    if (GUILayout.Button("↓", GUILayout.Width(20)))
                        SwapPatterns(i, i + 1);
                    GUI.enabled = true;

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();

            // Add / Remove buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Pattern"))
                AddNewPattern();
            GUI.enabled = _selectedPatternIndex >= 0;
            if (GUILayout.Button("- Remove"))
                RemoveSelectedPattern();
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        // --------------------------------------------------
        // Column 2: Pattern Properties (Burst / Bullets tabs)
        // --------------------------------------------------
        private void DrawPropertyColumn()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(PropertyWidth));
            var bgRect = GUILayoutUtility.GetRect(PropertyWidth, 0);
            EditorGUI.DrawRect(new Rect(bgRect.x, bgRect.y, PropertyWidth, position.height),
                ShmupEditorStyles.PanelBg);

            ShmupEditorStyles.DrawScopeHeader("Properties", true);

            if (_selectedPattern == null)
            {
                EditorGUILayout.HelpBox("パターンを選択してください", MessageType.None);
                EditorGUILayout.EndVertical();
                return;
            }

            // Tab bar: Burst / Bullets
            _patternTab = ShmupEditorStyles.DrawTabBar(PatternTabNames, _patternTab);
            EditorGUILayout.Space(4);

            _propertyScroll = EditorGUILayout.BeginScrollView(_propertyScroll);

            EditorGUI.BeginChangeCheck();

            switch (_patternTab)
            {
                case 0: DrawBurstTab(); break;
                case 1: DrawBulletsTab(); break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_selectedPattern);
                ResetSimulation();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawBurstTab()
        {
            EditorGUILayout.LabelField("Spread", ShmupEditorStyles.SubHeaderStyle);

            // Spread type selector (buttons)
            EditorGUILayout.BeginHorizontal();
            foreach (BulletSpreadType type in System.Enum.GetValues(typeof(BulletSpreadType)))
            {
                bool active = _selectedPattern.spreadType == type;
                GUI.backgroundColor = active ? ShmupEditorStyles.GameScopeColor : Color.white;
                if (GUILayout.Button(type.ToString(), active ? ShmupEditorStyles.TabActive : ShmupEditorStyles.TabNormal))
                {
                    _selectedPattern.spreadType = type;
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            _selectedPattern.bulletCount = EditorGUILayout.IntSlider(
                new GUIContent("Bullet Count", "同時発射弾数"), _selectedPattern.bulletCount, 1, 64);
            _selectedPattern.spreadAngle = EditorGUILayout.Slider(
                new GUIContent("Spread Angle", "拡散角度"), _selectedPattern.spreadAngle, 0f, 360f);
            _selectedPattern.angleOffset = EditorGUILayout.Slider(
                new GUIContent("Angle Offset", "角度オフセット"), _selectedPattern.angleOffset, -180f, 180f);

            ShmupEditorStyles.DrawSeparator();
            EditorGUILayout.LabelField("Sub Pattern", ShmupEditorStyles.SubHeaderStyle);
            _selectedPattern.subPattern = (ShmupBulletPatternData)EditorGUILayout.ObjectField(
                new GUIContent("Sub Pattern", "親弾から派生する弾パターン"),
                _selectedPattern.subPattern, typeof(ShmupBulletPatternData), false);
            _selectedPattern.subPatternDelay = EditorGUILayout.FloatField(
                new GUIContent("Delay", "サブパターン発射遅延"), _selectedPattern.subPatternDelay);
        }

        private void DrawBulletsTab()
        {
            EditorGUILayout.LabelField("Bullet Properties", ShmupEditorStyles.SubHeaderStyle);

            _selectedPattern.speed = EditorGUILayout.Slider(
                new GUIContent("Speed", "弾速"), _selectedPattern.speed, 0.1f, 30f);
            _selectedPattern.acceleration = EditorGUILayout.Slider(
                new GUIContent("Acceleration", "加速度"), _selectedPattern.acceleration, -10f, 10f);
            _selectedPattern.size = EditorGUILayout.Slider(
                new GUIContent("Size", "弾サイズ"), _selectedPattern.size, 0.1f, 5f);

            ShmupEditorStyles.DrawSeparator();
            EditorGUILayout.LabelField("Appearance", ShmupEditorStyles.SubHeaderStyle);
            _selectedPattern.sprite = (Sprite)EditorGUILayout.ObjectField(
                "Sprite", _selectedPattern.sprite, typeof(Sprite), false);

            ShmupEditorStyles.DrawSeparator();
            EditorGUILayout.LabelField("Homing", ShmupEditorStyles.SubHeaderStyle);
            _selectedPattern.homing = EditorGUILayout.Toggle(
                new GUIContent("Homing", "追尾弾"), _selectedPattern.homing);
            if (_selectedPattern.homing)
            {
                _selectedPattern.homingStrength = EditorGUILayout.Slider(
                    new GUIContent("Strength", "追尾強度"), _selectedPattern.homingStrength, 0.1f, 10f);
            }
        }

        // --------------------------------------------------
        // Column 3: Real-time Preview
        // --------------------------------------------------
        private void DrawPreviewColumn()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            // Preview header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Preview", ShmupEditorStyles.SubHeaderStyle);
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = _isSimulating ? ShmupEditorStyles.AccentRed : ShmupEditorStyles.AccentGreen;
            if (GUILayout.Button(_isSimulating ? "■ Stop" : "▶ Play", GUILayout.Width(60), GUILayout.Height(22)))
            {
                _isSimulating = !_isSimulating;
                if (_isSimulating) ResetSimulation();
            }
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("↻", GUILayout.Width(24), GUILayout.Height(22)))
                ResetSimulation();

            _previewSound = GUILayout.Toggle(_previewSound,
                new GUIContent("♪", "サウンド"), GUILayout.Width(24));
            EditorGUILayout.EndHorizontal();

            // Preview canvas
            var previewRect = GUILayoutUtility.GetRect(300, 300, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(previewRect, ShmupEditorStyles.DarkBg);

            DrawPreviewContent(previewRect);

            // Time display
            EditorGUILayout.LabelField($"Time: {_simTime:F2}s  |  Bullets: {_simBullets.Count}",
                EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();
        }

        private void DrawPreviewContent(Rect rect)
        {
            // 中心点（発射元）
            var center = rect.center;
            float scale = Mathf.Min(rect.width, rect.height) * 0.4f;

            // 発射元マーカー
            EditorGUI.DrawRect(new Rect(center.x - 3, center.y - 3, 6, 6), ShmupEditorStyles.AccentGreen);

            // 弾の描画
            if (_simBullets == null) return;

            // パターンごとに色分け
            var bulletColor = new Color(1f, 0.4f, 0.3f, 0.9f);
            if (_selectedPattern != null)
            {
                switch (_selectedPattern.spreadType)
                {
                    case BulletSpreadType.Fan: bulletColor = new Color(1f, 0.4f, 0.3f, 0.9f); break;
                    case BulletSpreadType.Circle: bulletColor = new Color(0.3f, 0.7f, 1f, 0.9f); break;
                    case BulletSpreadType.Line: bulletColor = new Color(0.4f, 1f, 0.4f, 0.9f); break;
                    case BulletSpreadType.Random: bulletColor = new Color(1f, 0.8f, 0.2f, 0.9f); break;
                }
            }

            for (int i = 0; i < _simBullets.Count; i++)
            {
                var b = _simBullets[i];
                if (!b.Active) continue;

                float px = center.x + b.Position.x * scale * 0.1f;
                float py = center.y - b.Position.y * scale * 0.1f;

                if (px < rect.x || px > rect.xMax || py < rect.y || py > rect.yMax)
                    continue;

                float bulletSize = _selectedPattern != null ? _selectedPattern.size * 3f : 3f;
                EditorGUI.DrawRect(new Rect(px - bulletSize * 0.5f, py - bulletSize * 0.5f,
                    bulletSize, bulletSize), bulletColor);

                // 軌跡（薄い色で直前位置も描画）
                float trailX = px - b.Velocity.x * 0.02f * scale * 0.1f;
                float trailY = py + b.Velocity.y * 0.02f * scale * 0.1f;
                var trailColor = new Color(bulletColor.r, bulletColor.g, bulletColor.b, 0.3f);
                EditorGUI.DrawRect(new Rect(trailX - 1, trailY - 1, 2, 2), trailColor);
            }

            // パターン未選択時のガイド
            if (_selectedPattern == null && !_isSimulating)
            {
                GUI.Label(rect, "パターンを選択して\n▶ Play で弾道プレビュー",
                    new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = new Color(1, 1, 1, 0.3f) }
                    });
            }
        }

        // --------------------------------------------------
        // Simulation
        // --------------------------------------------------
        private void SimulationUpdate()
        {
            if (!_isSimulating || _selectedPattern == null) return;

            double now = EditorApplication.timeSinceStartup;
            float dt = (float)(now - _lastUpdateTime);
            _lastUpdateTime = now;

            if (dt <= 0 || dt > 0.1f) dt = 0.016f;
            _simTime += dt;

            // 定期的に新規弾を発射
            if (_weaponData != null && _weaponData.fireRate > 0)
            {
                float fireInterval = _weaponData.fireRate;
                int expectedShots = Mathf.FloorToInt(_simTime / fireInterval);
                int currentBatches = _simBullets.Count / Mathf.Max(1, _selectedPattern.bulletCount);
                if (expectedShots > currentBatches && _simBullets.Count < 2000)
                {
                    var newBullets = BulletSimulator.CreateBullets(_selectedPattern, Vector2.zero, 90f);
                    _simBullets.AddRange(newBullets);
                }
            }

            // 弾を更新
            for (int i = 0; i < _simBullets.Count; i++)
            {
                var b = _simBullets[i];
                BulletSimulator.StepSimulation(ref b, dt, _selectedPattern.acceleration);
                // 画面外判定
                if (b.Position.magnitude > 50f) b.Active = false;
                _simBullets[i] = b;
            }

            Repaint();
        }

        private void ResetSimulation()
        {
            _simTime = 0f;
            _simBullets.Clear();
            _lastUpdateTime = EditorApplication.timeSinceStartup;

            if (_selectedPattern != null)
            {
                var initialBullets = BulletSimulator.CreateBullets(_selectedPattern, Vector2.zero, 90f);
                _simBullets.AddRange(initialBullets);
            }
        }

        // --------------------------------------------------
        // Data Operations
        // --------------------------------------------------
        private void SwapPatterns(int a, int b)
        {
            Undo.RecordObject(_weaponData, "Reorder Patterns");
            var tmp = _weaponData.bulletPatterns[a];
            _weaponData.bulletPatterns[a] = _weaponData.bulletPatterns[b];
            _weaponData.bulletPatterns[b] = tmp;
            if (_selectedPatternIndex == a) _selectedPatternIndex = b;
            else if (_selectedPatternIndex == b) _selectedPatternIndex = a;
            EditorUtility.SetDirty(_weaponData);
        }

        private void AddNewPattern()
        {
            var pattern = ScriptableObject.CreateInstance<ShmupBulletPatternData>();
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Bullet Pattern", "NewBulletPattern", "asset", "保存先を選択");
            if (string.IsNullOrEmpty(path)) { Object.DestroyImmediate(pattern); return; }
            AssetDatabase.CreateAsset(pattern, path);
            Undo.RecordObject(_weaponData, "Add Pattern");
            _weaponData.bulletPatterns ??= new List<ShmupBulletPatternData>();
            _weaponData.bulletPatterns.Add(pattern);
            _selectedPatternIndex = _weaponData.bulletPatterns.Count - 1;
            _selectedPattern = pattern;
            EditorUtility.SetDirty(_weaponData);
            AssetDatabase.SaveAssets();
        }

        private void RemoveSelectedPattern()
        {
            if (_selectedPatternIndex < 0 || _selectedPatternIndex >= _weaponData.bulletPatterns.Count) return;
            Undo.RecordObject(_weaponData, "Remove Pattern");
            _weaponData.bulletPatterns.RemoveAt(_selectedPatternIndex);
            _selectedPatternIndex = -1;
            _selectedPattern = null;
            EditorUtility.SetDirty(_weaponData);
        }

        private void CreateNewWeapon()
        {
            var asset = ScriptableObject.CreateInstance<ShmupWeaponData>();
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Weapon", "NewWeapon", "asset", "保存先を選択");
            if (string.IsNullOrEmpty(path)) { Object.DestroyImmediate(asset); return; }
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            _weaponData = asset;
            SaveWeaponRef();
        }

        // --- Persistence ---
        private void SaveWeaponRef()
        {
            if (_weaponData != null)
                EditorPrefs.SetString(PrefKeyWeaponGUID,
                    AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_weaponData)));
        }

        private void RestoreWeaponRef()
        {
            if (!EditorPrefs.HasKey(PrefKeyWeaponGUID)) return;
            var path = AssetDatabase.GUIDToAssetPath(EditorPrefs.GetString(PrefKeyWeaponGUID));
            if (!string.IsNullOrEmpty(path))
                _weaponData = AssetDatabase.LoadAssetAtPath<ShmupWeaponData>(path);
        }
    }
}
