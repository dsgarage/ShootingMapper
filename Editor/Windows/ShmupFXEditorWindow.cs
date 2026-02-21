using UnityEditor;
using UnityEngine;
using ShmupCreator.Editor.Styles;
using ShmupCreator.Runtime.Data;

namespace ShmupCreator.Editor.Windows
{
    /// <summary>
    /// SHMUP Creator本家 Explosion Editor / Particle Editor に準拠。
    /// Elements → Properties → Preview の3カラム構成。
    /// </summary>
    public class ShmupFXEditorWindow : EditorWindow
    {
        private Vector2 _scrollPos;
        private int _tab;
        private static readonly string[] TabNames = { "Explosions", "Particles" };

        // Explosion editing
        private Sprite _expSprite;
        private float _expDuration = 0.5f;
        private AudioClip _expSound;
        private Color _expColor = Color.yellow;
        private int _expFrameCount = 4;

        // Particle editing
        private float _ptclLifetime = 1f;
        private float _ptclSpeed = 3f;
        private float _ptclSize = 0.2f;
        private Color _ptclStartColor = Color.yellow;
        private Color _ptclEndColor = new Color(1f, 0.3f, 0f, 0f);
        private int _ptclCount = 20;
        private float _ptclSpread = 360f;

        // Preview
        private bool _isPlaying;
        private float _previewTime;
        private double _lastTime;

        [MenuItem("Shmup Creator/FX Editor", false, 17)]
        public static void ShowWindow()
        {
            var window = GetWindow<ShmupFXEditorWindow>("FX Editor");
            window.minSize = new Vector2(640, 420);
        }

        private void OnEnable()
        {
            EditorApplication.update += PreviewUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= PreviewUpdate;
        }

        private void OnGUI()
        {
            // Toolbar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("FX Editor", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            _tab = ShmupEditorStyles.DrawTabBar(TabNames, _tab);

            EditorGUILayout.BeginHorizontal();
            DrawPropertyPanel();
            ShmupEditorStyles.DrawColumnSeparator();
            DrawPreviewPanel();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPropertyPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(280));
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            switch (_tab)
            {
                case 0: DrawExplosionProperties(); break;
                case 1: DrawParticleProperties(); break;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawExplosionProperties()
        {
            ShmupEditorStyles.DrawScopeHeader("Explosion", true);
            EditorGUILayout.Space(4);

            _expSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", _expSprite, typeof(Sprite), false);
            _expDuration = EditorGUILayout.Slider(
                new GUIContent("Duration", "爆発の持続時間"), _expDuration, 0.1f, 3f);
            _expFrameCount = EditorGUILayout.IntSlider(
                new GUIContent("Frames", "アニメーションフレーム数"), _expFrameCount, 1, 16);
            _expColor = EditorGUILayout.ColorField(
                new GUIContent("Color", "爆発の色味"), _expColor);
            _expSound = (AudioClip)EditorGUILayout.ObjectField("Sound", _expSound, typeof(AudioClip), false);

            ShmupEditorStyles.DrawSeparator();
            EditorGUILayout.LabelField("Light", ShmupEditorStyles.SubHeaderStyle);
            EditorGUILayout.HelpBox("爆発時の光源エフェクト設定（今後追加予定）", MessageType.None);
        }

        private void DrawParticleProperties()
        {
            ShmupEditorStyles.DrawScopeHeader("Particle Emitter", true);
            EditorGUILayout.Space(4);

            _ptclCount = EditorGUILayout.IntSlider(
                new GUIContent("Count", "パーティクル数"), _ptclCount, 1, 200);
            _ptclLifetime = EditorGUILayout.Slider(
                new GUIContent("Lifetime", "生存時間"), _ptclLifetime, 0.1f, 5f);
            _ptclSpeed = EditorGUILayout.Slider(
                new GUIContent("Speed", "放出速度"), _ptclSpeed, 0.1f, 20f);
            _ptclSize = EditorGUILayout.Slider(
                new GUIContent("Size", "パーティクルサイズ"), _ptclSize, 0.05f, 2f);
            _ptclSpread = EditorGUILayout.Slider(
                new GUIContent("Spread", "放出角度"), _ptclSpread, 0f, 360f);

            ShmupEditorStyles.DrawSeparator();
            EditorGUILayout.LabelField("Color Over Lifetime", ShmupEditorStyles.SubHeaderStyle);
            _ptclStartColor = EditorGUILayout.ColorField("Start Color", _ptclStartColor);
            _ptclEndColor = EditorGUILayout.ColorField("End Color", _ptclEndColor);
        }

        private void DrawPreviewPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            // Controls
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Preview", ShmupEditorStyles.SubHeaderStyle);
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = _isPlaying ? ShmupEditorStyles.AccentRed : ShmupEditorStyles.AccentGreen;
            if (GUILayout.Button(_isPlaying ? "■ Stop" : "▶ Play", GUILayout.Width(60)))
            {
                _isPlaying = !_isPlaying;
                _previewTime = 0f;
                _lastTime = EditorApplication.timeSinceStartup;
            }
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("↻", GUILayout.Width(24)))
            {
                _previewTime = 0f;
            }
            EditorGUILayout.EndHorizontal();

            // Preview canvas
            var rect = GUILayoutUtility.GetRect(300, 300, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(rect, ShmupEditorStyles.DarkBg);

            if (_tab == 0)
                DrawExplosionPreview(rect);
            else
                DrawParticlePreview(rect);

            EditorGUILayout.LabelField($"Time: {_previewTime:F2}s", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawExplosionPreview(Rect rect)
        {
            if (!_isPlaying && _previewTime <= 0)
            {
                GUI.Label(rect, "▶ Play で爆発プレビュー",
                    new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1,1,1,0.3f) } });
                return;
            }

            float t = Mathf.Clamp01(_previewTime / _expDuration);
            float size = Mathf.Lerp(10, 60, t);
            float alpha = 1f - t;
            var color = new Color(_expColor.r, _expColor.g, _expColor.b, alpha);
            var center = rect.center;

            // 爆発の同心円
            for (int ring = 0; ring < 3; ring++)
            {
                float ringSize = size * (1f + ring * 0.4f);
                float ringAlpha = alpha * (1f - ring * 0.3f);
                var ringColor = new Color(color.r, color.g, color.b, ringAlpha * 0.5f);
                EditorGUI.DrawRect(new Rect(center.x - ringSize * 0.5f, center.y - ringSize * 0.5f,
                    ringSize, ringSize), ringColor);
            }

            // 中央の白フラッシュ
            if (t < 0.2f)
            {
                float flashSize = size * 0.5f;
                EditorGUI.DrawRect(new Rect(center.x - flashSize * 0.5f, center.y - flashSize * 0.5f,
                    flashSize, flashSize), new Color(1, 1, 1, (1f - t / 0.2f) * 0.8f));
            }
        }

        private void DrawParticlePreview(Rect rect)
        {
            if (!_isPlaying && _previewTime <= 0)
            {
                GUI.Label(rect, "▶ Play でパーティクルプレビュー",
                    new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1,1,1,0.3f) } });
                return;
            }

            var center = rect.center;
            float halfSpread = _ptclSpread * 0.5f;

            // 疑似パーティクル描画
            for (int i = 0; i < _ptclCount; i++)
            {
                float seed = i * 137.508f; // golden angle
                float angle = Mathf.Lerp(-halfSpread, halfSpread, (seed % 360f) / 360f) + 90f;
                float rad = angle * Mathf.Deg2Rad;
                float speedVariation = _ptclSpeed * (0.6f + (seed % 100f) / 100f * 0.8f);
                float life = _previewTime - (i * 0.02f);
                if (life < 0 || life > _ptclLifetime) continue;

                float t = life / _ptclLifetime;
                float dist = speedVariation * life * 20f;
                float px = center.x + Mathf.Cos(rad) * dist;
                float py = center.y - Mathf.Sin(rad) * dist;

                if (px < rect.x || px > rect.xMax || py < rect.y || py > rect.yMax) continue;

                var color = Color.Lerp(_ptclStartColor, _ptclEndColor, t);
                float size = _ptclSize * (1f - t * 0.5f) * 8f;
                EditorGUI.DrawRect(new Rect(px - size * 0.5f, py - size * 0.5f, size, size), color);
            }
        }

        private void PreviewUpdate()
        {
            if (!_isPlaying) return;
            double now = EditorApplication.timeSinceStartup;
            float dt = (float)(now - _lastTime);
            _lastTime = now;
            if (dt > 0.1f) dt = 0.016f;
            _previewTime += dt;

            float maxTime = _tab == 0 ? _expDuration : _ptclLifetime + _ptclCount * 0.02f;
            if (_previewTime > maxTime)
                _previewTime = 0f; // ループ

            Repaint();
        }
    }
}
