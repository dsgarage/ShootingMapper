using UnityEditor;
using UnityEngine;

namespace ShmupCreator.Editor.Styles
{
    /// <summary>
    /// SHMUP Creator本家に準拠した共通スタイル定義。
    /// 色分け: 青=ゲーム全体設定 / 黄=レベル固有設定
    /// </summary>
    public static class ShmupEditorStyles
    {
        // === Color Coding (SHMUP Creator準拠) ===
        // 青: ゲーム全体に影響する設定
        public static readonly Color GameScopeColor = new Color(0.2f, 0.4f, 0.8f, 1f);
        public static readonly Color GameScopeBg = new Color(0.15f, 0.22f, 0.35f, 1f);
        // 黄: レベル固有の設定
        public static readonly Color LevelScopeColor = new Color(0.9f, 0.75f, 0.2f, 1f);
        public static readonly Color LevelScopeBg = new Color(0.35f, 0.30f, 0.12f, 1f);
        // グレー: エディタ背景
        public static readonly Color CanvasBg = new Color(0.18f, 0.18f, 0.22f, 1f);
        public static readonly Color PanelBg = new Color(0.22f, 0.22f, 0.26f, 1f);
        public static readonly Color DarkBg = new Color(0.12f, 0.12f, 0.15f, 1f);
        // アクセント
        public static readonly Color AccentGreen = new Color(0.3f, 0.8f, 0.4f, 1f);
        public static readonly Color AccentRed = new Color(0.9f, 0.3f, 0.3f, 1f);
        public static readonly Color GridColor = new Color(1f, 1f, 1f, 0.06f);
        public static readonly Color GridMajorColor = new Color(1f, 1f, 1f, 0.15f);

        // === GUIStyles (lazy init) ===
        private static GUIStyle _headerStyle;
        public static GUIStyle HeaderStyle => _headerStyle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            normal = { textColor = Color.white },
            padding = new RectOffset(8, 8, 6, 6)
        };

        private static GUIStyle _subHeaderStyle;
        public static GUIStyle SubHeaderStyle => _subHeaderStyle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 11,
            normal = { textColor = new Color(0.8f, 0.8f, 0.8f) },
            padding = new RectOffset(4, 4, 4, 2)
        };

        private static GUIStyle _tabNormal;
        public static GUIStyle TabNormal => _tabNormal ??= new GUIStyle("Button")
        {
            fixedHeight = 28,
            fontSize = 11,
            fontStyle = FontStyle.Normal
        };

        private static GUIStyle _tabActive;
        public static GUIStyle TabActive => _tabActive ??= new GUIStyle("Button")
        {
            fixedHeight = 28,
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        private static GUIStyle _listItem;
        public static GUIStyle ListItem => _listItem ??= new GUIStyle("CN EntryBackOdd")
        {
            fixedHeight = 24,
            padding = new RectOffset(8, 8, 4, 4),
            fontSize = 11
        };

        private static GUIStyle _listItemSelected;
        public static GUIStyle ListItemSelected => _listItemSelected ??= new GUIStyle("CN EntryBackOdd")
        {
            fixedHeight = 24,
            padding = new RectOffset(8, 8, 4, 4),
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.4f, 0.8f, 1f) }
        };

        private static GUIStyle _tooltipStyle;
        public static GUIStyle TooltipStyle => _tooltipStyle ??= new GUIStyle("HelpBox")
        {
            fontSize = 10,
            wordWrap = true,
            padding = new RectOffset(6, 6, 4, 4)
        };

        // === Helper Methods ===

        /// <summary>スコープ色付きのヘッダーバーを描画</summary>
        public static void DrawScopeHeader(string title, bool isGameScope)
        {
            var color = isGameScope ? GameScopeBg : LevelScopeBg;
            var labelColor = isGameScope ? GameScopeColor : LevelScopeColor;
            var scopeLabel = isGameScope ? "[GAME]" : "[LEVEL]";

            var rect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, color);

            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = labelColor },
                fontSize = 12
            };
            GUI.Label(rect, $"  {scopeLabel}  {title}", style);
        }

        /// <summary>タブバーを描画し、選択されたタブインデックスを返す</summary>
        public static int DrawTabBar(string[] tabNames, int selectedTab)
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < tabNames.Length; i++)
            {
                var style = (i == selectedTab) ? TabActive : TabNormal;
                if (GUILayout.Button(tabNames[i], style, GUILayout.MinWidth(80)))
                {
                    selectedTab = i;
                }
            }
            EditorGUILayout.EndHorizontal();
            return selectedTab;
        }

        /// <summary>カラム区切り線を描画</summary>
        public static void DrawColumnSeparator()
        {
            var rect = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandHeight(true), GUILayout.Width(1));
            EditorGUI.DrawRect(rect, new Color(0.4f, 0.4f, 0.4f, 0.5f));
        }

        /// <summary>パネル背景を塗る</summary>
        public static void DrawPanelBackground(Rect rect)
        {
            EditorGUI.DrawRect(rect, PanelBg);
        }

        /// <summary>ツールチップ付きフィールド</summary>
        public static void LabelWithTooltip(string label, string tooltip)
        {
            EditorGUILayout.LabelField(new GUIContent(label, tooltip));
        }

        /// <summary>セクション区切り</summary>
        public static void DrawSeparator()
        {
            EditorGUILayout.Space(2);
            var rect = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.4f, 0.4f, 0.4f, 0.3f));
            EditorGUILayout.Space(2);
        }
    }
}
