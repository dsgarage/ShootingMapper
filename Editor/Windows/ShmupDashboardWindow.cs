using UnityEditor;
using UnityEngine;
using ShmupCreator.Editor.Styles;
using ShmupCreator.Runtime.Data;

namespace ShmupCreator.Editor.Windows
{
    /// <summary>
    /// メインハブウィンドウ。SHMUP Creatorのトップレベルナビゲーションに相当。
    /// ゲームデータの管理と各エディタへのクイックアクセスを提供する。
    /// </summary>
    public class ShmupDashboardWindow : EditorWindow
    {
        private ShmupGameData _gameData;
        private Vector2 _scrollPos;
        private int _selectedLevelIndex = -1;

        private const string PrefKeyGameData = "ShmupCreator_GameDataGUID";

        [MenuItem("Shmup Creator/Dashboard %#d", false, 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<ShmupDashboardWindow>("Shmup Creator");
            window.minSize = new Vector2(520, 400);
        }

        private void OnEnable()
        {
            RestoreGameData();
        }

        private void OnGUI()
        {
            DrawToolbar();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawGameDataSection();

            if (_gameData != null)
            {
                EditorGUILayout.Space(8);
                DrawPlayerSection();
                EditorGUILayout.Space(4);
                DrawLevelListSection();
                EditorGUILayout.Space(4);
                DrawQuickAccessSection();
            }

            EditorGUILayout.EndScrollView();
        }

        // --------------------------------------------------
        // Toolbar
        // --------------------------------------------------
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Shmup Creator", EditorStyles.boldLabel, GUILayout.Width(110));
            GUILayout.FlexibleSpace();

            if (_gameData != null)
            {
                GUI.backgroundColor = ShmupEditorStyles.AccentGreen;
                if (GUILayout.Button(new GUIContent("▶ Play Test", "Space: エディタ内テスト\nTab: 全画面テスト"),
                        EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    ShmupPlayTestManager.StartPlayTest(_gameData);
                }
                GUI.backgroundColor = Color.white;
            }

            if (GUILayout.Button("Preferences", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                // TODO: Preferences ウィンドウ
            }
            EditorGUILayout.EndHorizontal();
        }

        // --------------------------------------------------
        // Game Data Selection
        // --------------------------------------------------
        private void DrawGameDataSection()
        {
            ShmupEditorStyles.DrawScopeHeader("Game Data", true);
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            var newData = (ShmupGameData)EditorGUILayout.ObjectField(
                _gameData, typeof(ShmupGameData), false);
            if (newData != _gameData)
            {
                _gameData = newData;
                SaveGameData();
            }

            if (GUILayout.Button(new GUIContent("+", "新規 Game Data を作成"), GUILayout.Width(24)))
            {
                CreateAsset<ShmupGameData>("NewGameData");
            }
            EditorGUILayout.EndHorizontal();

            if (_gameData == null)
            {
                EditorGUILayout.HelpBox(
                    "Game Data を選択するか、「+」ボタンで新規作成してください。",
                    MessageType.Info);
                // 自動検出
                var guids = AssetDatabase.FindAssets("t:ShmupGameData");
                if (guids.Length > 0)
                {
                    EditorGUILayout.Space(2);
                    EditorGUILayout.LabelField("検出されたアセット:", EditorStyles.miniLabel);
                    foreach (var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("  " + path, EditorStyles.miniLabel);
                        if (GUILayout.Button("選択", EditorStyles.miniButton, GUILayout.Width(40)))
                        {
                            _gameData = AssetDatabase.LoadAssetAtPath<ShmupGameData>(path);
                            SaveGameData();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Game Name", GUILayout.Width(80));
                var newName = EditorGUILayout.TextField(_gameData.gameName);
                if (newName != _gameData.gameName)
                {
                    Undo.RecordObject(_gameData, "Change Game Name");
                    _gameData.gameName = newName;
                    EditorUtility.SetDirty(_gameData);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Resolution", GUILayout.Width(80));
                var newRes = EditorGUILayout.Vector2IntField(GUIContent.none, _gameData.resolution);
                if (newRes != _gameData.resolution)
                {
                    Undo.RecordObject(_gameData, "Change Resolution");
                    _gameData.resolution = newRes;
                    EditorUtility.SetDirty(_gameData);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Scroll", GUILayout.Width(80));
                var newDir = (ScrollDirection)EditorGUILayout.EnumPopup(_gameData.scrollDirection);
                if (newDir != _gameData.scrollDirection)
                {
                    Undo.RecordObject(_gameData, "Change Scroll Direction");
                    _gameData.scrollDirection = newDir;
                    EditorUtility.SetDirty(_gameData);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        // --------------------------------------------------
        // Player Section
        // --------------------------------------------------
        private void DrawPlayerSection()
        {
            ShmupEditorStyles.DrawScopeHeader("Player", true);
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            var newPlayer = (ShmupPlayerData)EditorGUILayout.ObjectField(
                _gameData.playerData, typeof(ShmupPlayerData), false);
            if (newPlayer != _gameData.playerData)
            {
                Undo.RecordObject(_gameData, "Change Player Data");
                _gameData.playerData = newPlayer;
                EditorUtility.SetDirty(_gameData);
            }
            if (GUILayout.Button("+", GUILayout.Width(24)))
            {
                CreateAsset<ShmupPlayerData>("NewPlayer");
            }
            EditorGUILayout.EndHorizontal();

            if (_gameData.playerData != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"Speed: {_gameData.playerData.speed}  |  " +
                    $"Weapons: {_gameData.playerData.weaponSets.Count}  |  " +
                    $"Hitbox: {_gameData.playerData.hitboxRadius}", EditorStyles.miniLabel);

                if (_gameData.playerData.weaponSets.Count > 0)
                {
                    if (GUILayout.Button("Weapon Editor で編集 →", EditorStyles.linkLabel))
                    {
                        var w = GetWindow<ShmupWeaponEditorWindow>("Weapon Editor");
                        w.SetWeapon(_gameData.playerData.weaponSets[0]);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        // --------------------------------------------------
        // Level List
        // --------------------------------------------------
        private void DrawLevelListSection()
        {
            ShmupEditorStyles.DrawScopeHeader("Levels", false);
            EditorGUILayout.Space(4);

            if (_gameData.levels == null || _gameData.levels.Count == 0)
            {
                EditorGUILayout.HelpBox("レベルがありません。「+ Level」で追加してください。", MessageType.None);
            }
            else
            {
                for (int i = 0; i < _gameData.levels.Count; i++)
                {
                    var level = _gameData.levels[i];
                    bool isSelected = (i == _selectedLevelIndex);
                    var style = isSelected ? ShmupEditorStyles.ListItemSelected : ShmupEditorStyles.ListItem;

                    EditorGUILayout.BeginHorizontal(style);

                    if (GUILayout.Button(level != null ? level.levelName : "(null)",
                            EditorStyles.label, GUILayout.ExpandWidth(true)))
                    {
                        _selectedLevelIndex = i;
                    }

                    if (level != null)
                    {
                        GUILayout.Label($"{level.duration:F0}s", EditorStyles.miniLabel, GUILayout.Width(40));
                        GUILayout.Label($"W:{(level.waves != null ? level.waves.Count : 0)}",
                            EditorStyles.miniLabel, GUILayout.Width(30));

                        if (GUILayout.Button(new GUIContent("▶", "このレベルをテスト"),
                                GUILayout.Width(24)))
                        {
                            ShmupPlayTestManager.StartPlayTest(_gameData, i);
                        }

                        if (GUILayout.Button(new GUIContent("✎", "Level Editor で編集"),
                                GUILayout.Width(24)))
                        {
                            var w = GetWindow<ShmupLevelEditorWindow>("Level Editor");
                            w.SetLevel(level, _gameData);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Level"))
            {
                var newLevel = CreateAsset<ShmupLevelData>("Level_" + (_gameData.levels.Count + 1));
                if (newLevel != null)
                {
                    Undo.RecordObject(_gameData, "Add Level");
                    _gameData.levels.Add(newLevel);
                    EditorUtility.SetDirty(_gameData);
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        // --------------------------------------------------
        // Quick Access
        // --------------------------------------------------
        private void DrawQuickAccessSection()
        {
            ShmupEditorStyles.DrawSeparator();
            EditorGUILayout.LabelField("Quick Access", ShmupEditorStyles.SubHeaderStyle);
            EditorGUILayout.Space(2);

            EditorGUILayout.BeginHorizontal();
            DrawQuickButton("Weapon Editor", () => ShmupWeaponEditorWindow.ShowWindow());
            DrawQuickButton("Enemy Editor", () => ShmupEnemyEditorWindow.ShowWindow());
            DrawQuickButton("HUD Editor", () => ShmupHUDEditorWindow.ShowWindow());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            DrawQuickButton("Gameplay Rules", () => ShmupGameplayWindow.ShowWindow());
            DrawQuickButton("FX Editor", () => ShmupFXEditorWindow.ShowWindow());
            DrawQuickButton("Game Settings", () => ShmupGameSettingsWindow.ShowWindow());
            EditorGUILayout.EndHorizontal();
        }

        private void DrawQuickButton(string label, System.Action onClick)
        {
            if (GUILayout.Button(label, GUILayout.Height(30)))
            {
                onClick?.Invoke();
            }
        }

        // --------------------------------------------------
        // Persistence (EditorPrefs)
        // --------------------------------------------------
        private void SaveGameData()
        {
            if (_gameData != null)
            {
                var path = AssetDatabase.GetAssetPath(_gameData);
                var guid = AssetDatabase.AssetPathToGUID(path);
                EditorPrefs.SetString(PrefKeyGameData, guid);
            }
            else
            {
                EditorPrefs.DeleteKey(PrefKeyGameData);
            }
        }

        private void RestoreGameData()
        {
            if (EditorPrefs.HasKey(PrefKeyGameData))
            {
                var guid = EditorPrefs.GetString(PrefKeyGameData);
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                {
                    _gameData = AssetDatabase.LoadAssetAtPath<ShmupGameData>(path);
                }
            }
        }

        // --------------------------------------------------
        // Asset Creation Helper
        // --------------------------------------------------
        private static T CreateAsset<T>(string defaultName) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            var path = EditorUtility.SaveFilePanelInProject(
                $"Create {typeof(T).Name}", defaultName, "asset",
                $"新規 {typeof(T).Name} の保存先を選択");

            if (string.IsNullOrEmpty(path))
            {
                Object.DestroyImmediate(asset);
                return null;
            }

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(asset);
            return asset;
        }
    }
}
