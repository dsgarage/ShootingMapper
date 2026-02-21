using UnityEditor;
using UnityEngine;
using ShmupCreator.Editor.Styles;
using ShmupCreator.Runtime.Data;

namespace ShmupCreator.Editor.Windows
{
    /// <summary>
    /// SHMUP Creator本家に準拠したタブベースのエネミーエディタ。
    /// Settings / Wave / Gameplay / Weaponry / Collisions の6タブ構成。
    /// </summary>
    public class ShmupEnemyEditorWindow : EditorWindow
    {
        private ShmupEnemyData _enemyData;
        private Vector2 _scrollPos;
        private int _tab;
        private bool _editingPath;

        private static readonly string[] TabNames = { "Settings", "Movement", "Weaponry", "Gameplay", "FX" };
        private const string PrefKeyEnemyGUID = "ShmupCreator_EnemyDataGUID";

        [MenuItem("Shmup Creator/Enemy Editor %e", false, 12)]
        public static void ShowWindow()
        {
            var window = GetWindow<ShmupEnemyEditorWindow>("Enemy Editor");
            window.minSize = new Vector2(460, 480);
        }

        public void SetEnemy(ShmupEnemyData enemy)
        {
            _enemyData = enemy;
            SaveEnemyRef();
            Repaint();
        }

        private void OnEnable()
        {
            RestoreEnemyRef();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_enemyData == null)
            {
                EditorGUILayout.Space(40);
                EditorGUILayout.HelpBox(
                    "ShmupEnemyData アセットを選択するか、「+」ボタンで新規作成してください。",
                    MessageType.Info);
                return;
            }

            // Tab bar
            _tab = ShmupEditorStyles.DrawTabBar(TabNames, _tab);
            ShmupEditorStyles.DrawSeparator();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            switch (_tab)
            {
                case 0: DrawSettingsTab(); break;
                case 1: DrawMovementTab(); break;
                case 2: DrawWeaponryTab(); break;
                case 3: DrawGameplayTab(); break;
                case 4: DrawFXTab(); break;
            }

            EditorGUILayout.EndScrollView();
        }

        // --------------------------------------------------
        // Toolbar
        // --------------------------------------------------
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Enemy Editor", EditorStyles.boldLabel, GUILayout.Width(100));

            var newEnemy = (ShmupEnemyData)EditorGUILayout.ObjectField(
                _enemyData, typeof(ShmupEnemyData), false, GUILayout.Width(200));
            if (newEnemy != _enemyData) { _enemyData = newEnemy; SaveEnemyRef(); }

            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(22)))
                CreateNewEnemy();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        // --------------------------------------------------
        // Tab: Settings (Appearance + Stats)
        // --------------------------------------------------
        private void DrawSettingsTab()
        {
            ShmupEditorStyles.DrawScopeHeader("Appearance", true);
            EditorGUILayout.Space(4);

            EditorGUI.BeginChangeCheck();

            _enemyData.sprite = (Sprite)EditorGUILayout.ObjectField(
                new GUIContent("Sprite", "エネミーの見た目"),
                _enemyData.sprite, typeof(Sprite), false);

            // スプライトプレビュー
            if (_enemyData.sprite != null)
            {
                var previewRect = GUILayoutUtility.GetRect(64, 64, GUILayout.Width(64));
                EditorGUI.DrawRect(previewRect, ShmupEditorStyles.DarkBg);
                GUI.DrawTexture(previewRect, _enemyData.sprite.texture, ScaleMode.ScaleToFit);
            }

            ShmupEditorStyles.DrawSeparator();
            ShmupEditorStyles.DrawScopeHeader("Stats", true);
            EditorGUILayout.Space(4);

            _enemyData.hp = EditorGUILayout.IntField(
                new GUIContent("HP", "体力"), _enemyData.hp);
            _enemyData.scoreValue = EditorGUILayout.IntField(
                new GUIContent("Score Value", "撃破時のスコア"), _enemyData.scoreValue);

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(_enemyData);
        }

        // --------------------------------------------------
        // Tab: Movement (Path / Waypoints)
        // --------------------------------------------------
        private void DrawMovementTab()
        {
            ShmupEditorStyles.DrawScopeHeader("Movement", true);
            EditorGUILayout.Space(4);

            EditorGUI.BeginChangeCheck();

            _enemyData.moveSpeed = EditorGUILayout.Slider(
                new GUIContent("Move Speed", "移動速度"), _enemyData.moveSpeed, 0.1f, 20f);

            ShmupEditorStyles.DrawSeparator();
            EditorGUILayout.LabelField("Waypoints", ShmupEditorStyles.SubHeaderStyle);

            if (_enemyData.movePath != null && _enemyData.movePath.Length > 0)
            {
                for (int i = 0; i < _enemyData.movePath.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  [{i}]", GUILayout.Width(30));
                    _enemyData.movePath[i] = EditorGUILayout.Vector2Field(GUIContent.none, _enemyData.movePath[i]);
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        var list = new System.Collections.Generic.List<Vector2>(_enemyData.movePath);
                        list.RemoveAt(i);
                        _enemyData.movePath = list.ToArray();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("ウェイポイントが未設定です。\n「+ Waypoint」ボタンか、Sceneビューで Shift+Click で追加できます。", MessageType.None);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Waypoint"))
            {
                var list = _enemyData.movePath != null
                    ? new System.Collections.Generic.List<Vector2>(_enemyData.movePath)
                    : new System.Collections.Generic.List<Vector2>();
                list.Add(list.Count > 0 ? list[list.Count - 1] + Vector2.up : Vector2.zero);
                _enemyData.movePath = list.ToArray();
            }

            GUI.backgroundColor = _editingPath ? ShmupEditorStyles.AccentGreen : Color.white;
            if (GUILayout.Button(_editingPath ? "✓ パス編集中" : "Scene で編集",
                    GUILayout.Width(100)))
            {
                _editingPath = !_editingPath;
                if (_editingPath) SceneView.RepaintAll();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(_enemyData);
        }

        // --------------------------------------------------
        // Tab: Weaponry
        // --------------------------------------------------
        private void DrawWeaponryTab()
        {
            ShmupEditorStyles.DrawScopeHeader("Weapon", true);
            EditorGUILayout.Space(4);

            EditorGUI.BeginChangeCheck();
            _enemyData.weapon = (ShmupWeaponData)EditorGUILayout.ObjectField(
                new GUIContent("Weapon", "使用する武器データ"),
                _enemyData.weapon, typeof(ShmupWeaponData), false);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(_enemyData);

            if (_enemyData.weapon != null)
            {
                EditorGUILayout.Space(4);
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"Fire Rate: {_enemyData.weapon.fireRate:F2}s", EditorStyles.miniLabel);
                EditorGUILayout.LabelField(
                    $"Patterns: {(_enemyData.weapon.bulletPatterns != null ? _enemyData.weapon.bulletPatterns.Count : 0)}",
                    EditorStyles.miniLabel);
                EditorGUI.indentLevel--;

                EditorGUILayout.Space(4);
                if (GUILayout.Button("Weapon Editor で編集 →"))
                {
                    var w = GetWindow<ShmupWeaponEditorWindow>("Weapon Editor");
                    w.SetWeapon(_enemyData.weapon);
                }
            }
            else
            {
                EditorGUILayout.Space(4);
                if (GUILayout.Button("+ New Weapon"))
                {
                    ShmupWeaponEditorWindow.ShowWindow();
                }
            }
        }

        // --------------------------------------------------
        // Tab: Gameplay (Score, Drops)
        // --------------------------------------------------
        private void DrawGameplayTab()
        {
            ShmupEditorStyles.DrawScopeHeader("Gameplay", true);
            EditorGUILayout.Space(4);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Score", ShmupEditorStyles.SubHeaderStyle);
            _enemyData.scoreValue = EditorGUILayout.IntField(
                new GUIContent("Score Value", "撃破スコア"), _enemyData.scoreValue);

            ShmupEditorStyles.DrawSeparator();
            EditorGUILayout.LabelField("Item Drop", ShmupEditorStyles.SubHeaderStyle);
            EditorGUILayout.HelpBox("アイテムドロップ設定は今後のアップデートで追加予定です。", MessageType.None);

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(_enemyData);
        }

        // --------------------------------------------------
        // Tab: FX (Explosion)
        // --------------------------------------------------
        private void DrawFXTab()
        {
            ShmupEditorStyles.DrawScopeHeader("Effects", true);
            EditorGUILayout.Space(4);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Explosion", ShmupEditorStyles.SubHeaderStyle);
            if (_enemyData.explosion == null)
                _enemyData.explosion = new ExplosionData();

            _enemyData.explosion.sprite = (Sprite)EditorGUILayout.ObjectField(
                new GUIContent("Sprite", "爆発スプライト"),
                _enemyData.explosion.sprite, typeof(Sprite), false);
            _enemyData.explosion.duration = EditorGUILayout.FloatField(
                new GUIContent("Duration", "爆発の持続時間"), _enemyData.explosion.duration);
            _enemyData.explosion.sound = (AudioClip)EditorGUILayout.ObjectField(
                new GUIContent("Sound", "爆発SE"),
                _enemyData.explosion.sound, typeof(AudioClip), false);

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(_enemyData);
        }

        // --------------------------------------------------
        // SceneView: Path Editing Gizmo
        // --------------------------------------------------
        private void OnSceneGUI(SceneView sceneView)
        {
            if (!_editingPath || _enemyData == null || _enemyData.movePath == null) return;

            // ウェイポイントの描画とハンドル操作
            Handles.color = ShmupEditorStyles.AccentGreen;

            for (int i = 0; i < _enemyData.movePath.Length; i++)
            {
                var pos3d = new Vector3(_enemyData.movePath[i].x, _enemyData.movePath[i].y, 0);

                // ポジションハンドル
                EditorGUI.BeginChangeCheck();
                var newPos = Handles.PositionHandle(pos3d, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_enemyData, "Move Waypoint");
                    _enemyData.movePath[i] = new Vector2(newPos.x, newPos.y);
                    EditorUtility.SetDirty(_enemyData);
                }

                // ラベル
                Handles.Label(pos3d + Vector3.right * 0.3f, $"[{i}]");

                // パスライン
                if (i > 0)
                {
                    var prev = new Vector3(_enemyData.movePath[i - 1].x, _enemyData.movePath[i - 1].y, 0);
                    Handles.DrawLine(prev, pos3d);
                }
            }

            // Shift+Click でウェイポイント追加
            var evt = Event.current;
            if (evt.type == EventType.MouseDown && evt.shift && evt.button == 0)
            {
                var worldPos = HandleUtility.GUIPointToWorldRay(evt.mousePosition).origin;
                Undo.RecordObject(_enemyData, "Add Waypoint");
                var list = new System.Collections.Generic.List<Vector2>(_enemyData.movePath);
                list.Add(new Vector2(worldPos.x, worldPos.y));
                _enemyData.movePath = list.ToArray();
                EditorUtility.SetDirty(_enemyData);
                evt.Use();
                Repaint();
            }

            sceneView.Repaint();
        }

        // --------------------------------------------------
        // Data
        // --------------------------------------------------
        private void CreateNewEnemy()
        {
            var asset = ScriptableObject.CreateInstance<ShmupEnemyData>();
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Enemy", "NewEnemy", "asset", "保存先を選択");
            if (string.IsNullOrEmpty(path)) { Object.DestroyImmediate(asset); return; }
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            _enemyData = asset;
            SaveEnemyRef();
        }

        private void SaveEnemyRef()
        {
            if (_enemyData != null)
                EditorPrefs.SetString(PrefKeyEnemyGUID,
                    AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_enemyData)));
        }

        private void RestoreEnemyRef()
        {
            if (!EditorPrefs.HasKey(PrefKeyEnemyGUID)) return;
            var path = AssetDatabase.GUIDToAssetPath(EditorPrefs.GetString(PrefKeyEnemyGUID));
            if (!string.IsNullOrEmpty(path))
                _enemyData = AssetDatabase.LoadAssetAtPath<ShmupEnemyData>(path);
        }
    }
}
