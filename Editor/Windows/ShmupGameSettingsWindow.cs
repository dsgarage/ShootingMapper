using UnityEditor;
using UnityEngine;
using ShmupCreator.Editor.Styles;
using ShmupCreator.Runtime.Data;

namespace ShmupCreator.Editor.Windows
{
    /// <summary>
    /// ゲーム全体設定ウィンドウ。SHMUP Creator の Game Editor に相当。
    /// 青色スコープ＝ゲーム全体に影響する設定。
    /// </summary>
    public class ShmupGameSettingsWindow : EditorWindow
    {
        private ShmupGameData _gameData;
        private Vector2 _scrollPos;
        private int _tab;
        private static readonly string[] TabNames = { "General", "Player", "Display" };

        [MenuItem("Shmup Creator/Game Settings", false, 20)]
        public static void ShowWindow()
        {
            var window = GetWindow<ShmupGameSettingsWindow>("Game Settings");
            window.minSize = new Vector2(420, 350);
        }

        private void OnEnable()
        {
            // Dashboard と同じ GameData を自動復元
            if (EditorPrefs.HasKey("ShmupCreator_GameDataGUID"))
            {
                var path = AssetDatabase.GUIDToAssetPath(
                    EditorPrefs.GetString("ShmupCreator_GameDataGUID"));
                if (!string.IsNullOrEmpty(path))
                    _gameData = AssetDatabase.LoadAssetAtPath<ShmupGameData>(path);
            }
        }

        private void OnGUI()
        {
            // Toolbar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Game Settings", EditorStyles.boldLabel, GUILayout.Width(100));
            var newData = (ShmupGameData)EditorGUILayout.ObjectField(
                _gameData, typeof(ShmupGameData), false, GUILayout.Width(200));
            if (newData != _gameData) _gameData = newData;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (_gameData == null)
            {
                EditorGUILayout.Space(30);
                EditorGUILayout.HelpBox("ShmupGameData を選択してください。\nDashboard から自動で同期されます。", MessageType.Info);
                return;
            }

            _tab = ShmupEditorStyles.DrawTabBar(TabNames, _tab);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUI.BeginChangeCheck();
            switch (_tab)
            {
                case 0: DrawGeneralTab(); break;
                case 1: DrawPlayerTab(); break;
                case 2: DrawDisplayTab(); break;
            }
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(_gameData);

            EditorGUILayout.EndScrollView();
        }

        private void DrawGeneralTab()
        {
            ShmupEditorStyles.DrawScopeHeader("Game Info", true);
            EditorGUILayout.Space(4);
            _gameData.gameName = EditorGUILayout.TextField(
                new GUIContent("Game Name", "ゲームのタイトル"), _gameData.gameName);
            _gameData.scrollDirection = (ScrollDirection)EditorGUILayout.EnumPopup(
                new GUIContent("Scroll Direction", "メインのスクロール方向"), _gameData.scrollDirection);

            ShmupEditorStyles.DrawSeparator();
            ShmupEditorStyles.DrawScopeHeader("Levels", true);
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField($"登録済みレベル数: {_gameData.levels.Count}", EditorStyles.miniLabel);
            if (GUILayout.Button("Dashboard で管理 →"))
                Windows.ShmupDashboardWindow.ShowWindow();
        }

        private void DrawPlayerTab()
        {
            ShmupEditorStyles.DrawScopeHeader("Player", true);
            EditorGUILayout.Space(4);
            _gameData.playerData = (ShmupPlayerData)EditorGUILayout.ObjectField(
                "Player Data", _gameData.playerData, typeof(ShmupPlayerData), false);

            if (_gameData.playerData != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"Speed: {_gameData.playerData.speed}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Weapons: {_gameData.playerData.weaponSets.Count}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Hitbox: {_gameData.playerData.hitboxRadius}", EditorStyles.miniLabel);
                EditorGUI.indentLevel--;
            }
        }

        private void DrawDisplayTab()
        {
            ShmupEditorStyles.DrawScopeHeader("Display", true);
            EditorGUILayout.Space(4);
            _gameData.resolution = EditorGUILayout.Vector2IntField(
                new GUIContent("Resolution", "ゲーム解像度"), _gameData.resolution);
        }
    }
}
