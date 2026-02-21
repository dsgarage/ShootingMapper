using UnityEditor;
using UnityEngine;
using ShmupCreator.Editor.Styles;
using ShmupCreator.Runtime.Data;

namespace ShmupCreator.Editor.Windows
{
    /// <summary>
    /// SHMUP Creator本家 Gameplay Editor に準拠。
    /// Scoring / Lives / Items / Gauge の4タブ構成。
    /// </summary>
    public class ShmupGameplayWindow : EditorWindow
    {
        private ShmupGameplayData _gameplayData;
        private Vector2 _scrollPos;
        private int _tab;
        private static readonly string[] TabNames = { "Scoring", "Chain", "Medal", "Rank" };

        [MenuItem("Shmup Creator/Gameplay Rules", false, 16)]
        public static void ShowWindow()
        {
            var window = GetWindow<ShmupGameplayWindow>("Gameplay Rules");
            window.minSize = new Vector2(420, 380);
        }

        private void OnGUI()
        {
            // Toolbar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Gameplay Rules", EditorStyles.boldLabel, GUILayout.Width(110));
            var newData = (ShmupGameplayData)EditorGUILayout.ObjectField(
                _gameplayData, typeof(ShmupGameplayData), false, GUILayout.Width(200));
            if (newData != _gameplayData) _gameplayData = newData;
            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(22)))
            {
                var asset = ScriptableObject.CreateInstance<ShmupGameplayData>();
                var path = EditorUtility.SaveFilePanelInProject("Create Gameplay", "NewGameplay", "asset", "保存先");
                if (!string.IsNullOrEmpty(path)) { AssetDatabase.CreateAsset(asset, path); AssetDatabase.SaveAssets(); _gameplayData = asset; }
                else Object.DestroyImmediate(asset);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (_gameplayData == null)
            {
                EditorGUILayout.Space(30);
                EditorGUILayout.HelpBox("ShmupGameplayData を選択してください。", MessageType.Info);
                return;
            }

            _tab = ShmupEditorStyles.DrawTabBar(TabNames, _tab);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUI.BeginChangeCheck();
            switch (_tab)
            {
                case 0: DrawScoringTab(); break;
                case 1: DrawChainTab(); break;
                case 2: DrawMedalTab(); break;
                case 3: DrawRankTab(); break;
            }
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(_gameplayData);

            EditorGUILayout.EndScrollView();
        }

        private void DrawScoringTab()
        {
            ShmupEditorStyles.DrawScopeHeader("Scoring System", true);
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("基本スコアは各エネミーの Score Value で設定されます。", EditorStyles.wordWrappedMiniLabel);
            ShmupEditorStyles.DrawSeparator();

            EditorGUILayout.LabelField("Bullet Cancel", ShmupEditorStyles.SubHeaderStyle);
            _gameplayData.bulletCancel = EditorGUILayout.Toggle(
                new GUIContent("Enable", "ボムやボス撃破時の弾消しボーナス"), _gameplayData.bulletCancel);
            if (_gameplayData.bulletCancel)
            {
                EditorGUI.indentLevel++;
                _gameplayData.cancelBonusPerBullet = EditorGUILayout.IntField(
                    new GUIContent("Bonus / Bullet", "弾1つあたりのキャンセルボーナス"), _gameplayData.cancelBonusPerBullet);
                EditorGUI.indentLevel--;
            }
        }

        private void DrawChainTab()
        {
            ShmupEditorStyles.DrawScopeHeader("Chain System", true);
            EditorGUILayout.Space(4);
            _gameplayData.chainEnabled = EditorGUILayout.Toggle(
                new GUIContent("Enable", "連続撃破でスコア倍率が上昇"), _gameplayData.chainEnabled);

            if (_gameplayData.chainEnabled)
            {
                EditorGUI.indentLevel++;
                _gameplayData.chainTimeWindow = EditorGUILayout.Slider(
                    new GUIContent("Time Window", "チェーン継続の時間窓（秒）"), _gameplayData.chainTimeWindow, 0.1f, 5f);
                _gameplayData.chainMultiplierStep = EditorGUILayout.Slider(
                    new GUIContent("Multiplier Step", "1チェーンごとの倍率上昇量"), _gameplayData.chainMultiplierStep, 0.01f, 1f);
                EditorGUI.indentLevel--;
            }
        }

        private void DrawMedalTab()
        {
            ShmupEditorStyles.DrawScopeHeader("Medal System", true);
            EditorGUILayout.Space(4);
            _gameplayData.medalSystem = EditorGUILayout.Toggle(
                new GUIContent("Enable", "メダル回収によるスコアシステム"), _gameplayData.medalSystem);

            if (_gameplayData.medalSystem)
            {
                EditorGUI.indentLevel++;
                _gameplayData.medalBaseScore = EditorGUILayout.IntField(
                    new GUIContent("Base Score", "メダルの基本スコア"), _gameplayData.medalBaseScore);
                _gameplayData.medalScoreMultiplier = EditorGUILayout.FloatField(
                    new GUIContent("Score Multiplier", "連続回収時のスコア倍率"), _gameplayData.medalScoreMultiplier);
                EditorGUI.indentLevel--;
            }
        }

        private void DrawRankTab()
        {
            ShmupEditorStyles.DrawScopeHeader("Rank (Difficulty) System", true);
            EditorGUILayout.Space(4);
            _gameplayData.rankSystem = EditorGUILayout.Toggle(
                new GUIContent("Enable", "プレイ状況に応じた動的難易度調整"), _gameplayData.rankSystem);

            if (_gameplayData.rankSystem)
            {
                EditorGUI.indentLevel++;
                _gameplayData.rankIncreaseRate = EditorGUILayout.Slider(
                    new GUIContent("Increase Rate", "時間経過によるランク上昇速度"), _gameplayData.rankIncreaseRate, 0.001f, 0.1f);
                _gameplayData.rankDecreaseOnDeath = EditorGUILayout.Slider(
                    new GUIContent("Decrease on Death", "被弾時のランク減少量"), _gameplayData.rankDecreaseOnDeath, 0f, 1f);
                EditorGUI.indentLevel--;
            }
        }
    }
}
