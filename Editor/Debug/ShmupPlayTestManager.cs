using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ShmupCreator.Runtime.Data;

namespace ShmupCreator.Editor
{
    /// <summary>
    /// SHMUP Creator本家に準拠した即座プレイテストシステム。
    /// Space = エディタオーバーレイ付きテスト / Tab = 全画面テスト
    /// 停止→調整→再テストの高速反復ワークフローを実現する。
    /// </summary>
    [InitializeOnLoad]
    public static class ShmupPlayTestManager
    {
        private static ShmupGameData _testGameData;
        private static int _testLevelIndex;
        private static bool _isOverlayMode;
        private static bool _wasPlaying;
        private static string _previousScenePath;

        private const string TestSceneName = "__ShmupPlayTest__";
        private const string PrefKeyOverlay = "ShmupCreator_OverlayDebug";

        // --- Debug Overlay State ---
        public static bool ShowDebugOverlay
        {
            get => EditorPrefs.GetBool(PrefKeyOverlay, true);
            set => EditorPrefs.SetBool(PrefKeyOverlay, value);
        }

        public static bool IsTestRunning => EditorApplication.isPlaying && _testGameData != null;
        public static ShmupGameData CurrentTestData => _testGameData;
        public static int CurrentTestLevel => _testLevelIndex;

        static ShmupPlayTestManager()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        // --------------------------------------------------
        // Public API
        // --------------------------------------------------

        /// <summary>ゲーム全体をテスト（レベル1から開始）</summary>
        [MenuItem("Shmup Creator/▶ Play Test _SPACE", false, 100)]
        public static void PlayTestFromMenu()
        {
            // ダッシュボードから最後に使ったGameDataを取得
            var guid = EditorPrefs.GetString("ShmupCreator_GameDataGUID", "");
            if (string.IsNullOrEmpty(guid)) return;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var data = AssetDatabase.LoadAssetAtPath<ShmupGameData>(path);
            if (data != null) StartPlayTest(data);
        }

        /// <summary>指定ゲームデータでテスト開始</summary>
        public static void StartPlayTest(ShmupGameData gameData, int levelIndex = 0)
        {
            if (EditorApplication.isPlaying)
            {
                StopPlayTest();
                return;
            }

            if (gameData == null)
            {
                Debug.LogWarning("[ShmupCreator] Game Data が設定されていません。");
                return;
            }

            _testGameData = gameData;
            _testLevelIndex = levelIndex;
            _isOverlayMode = true;

            // 現在のシーンを保存
            var currentScene = SceneManager.GetActiveScene();
            _previousScenePath = currentScene.path;

            if (currentScene.isDirty)
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EnterPlayMode();
                }
            }
            else
            {
                EnterPlayMode();
            }
        }

        /// <summary>テスト停止</summary>
        [MenuItem("Shmup Creator/■ Stop Test #SPACE", false, 101)]
        public static void StopPlayTest()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
        }

        /// <summary>テストを最初からリスタート</summary>
        [MenuItem("Shmup Creator/↻ Restart Test %SPACE", false, 102)]
        public static void RestartPlayTest()
        {
            if (_testGameData != null)
            {
                EditorApplication.isPlaying = false;
                // PlayMode停止後に自動リスタート
                _wasPlaying = true;
            }
        }

        // --------------------------------------------------
        // Play Mode Handling
        // --------------------------------------------------
        private static void EnterPlayMode()
        {
            // テストシーンを作成して遷移
            var testScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            testScene.name = TestSceneName;

            // テスト用のブートストラップオブジェクトを配置
            var bootstrapObj = new GameObject("[ShmupPlayTest Bootstrap]");
            var bootstrap = bootstrapObj.AddComponent<ShmupPlayTestBootstrap>();
            bootstrap.gameData = _testGameData;
            bootstrap.startLevelIndex = _testLevelIndex;
            bootstrap.showOverlay = ShowDebugOverlay;

            Debug.Log($"[ShmupCreator] ▶ Play Test 開始: {_testGameData.gameName} - Level {_testLevelIndex}");
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    if (_wasPlaying && _testGameData != null)
                    {
                        // リスタート
                        _wasPlaying = false;
                        EditorApplication.delayCall += () => StartPlayTest(_testGameData, _testLevelIndex);
                    }
                    else
                    {
                        // テスト完全停止
                        if (!string.IsNullOrEmpty(_previousScenePath))
                        {
                            EditorSceneManager.OpenScene(_previousScenePath);
                            _previousScenePath = null;
                        }
                        _testGameData = null;
                        Debug.Log("[ShmupCreator] ■ Play Test 終了");
                    }
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                    if (_testGameData != null && ShowDebugOverlay)
                    {
                        // デバッグオーバーレイ登録
                        SceneView.duringSceneGui += DrawDebugOverlay;
                    }
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    SceneView.duringSceneGui -= DrawDebugOverlay;
                    break;
            }
        }

        // --------------------------------------------------
        // Debug Overlay
        // --------------------------------------------------
        private static void DrawDebugOverlay(SceneView sceneView)
        {
            if (!IsTestRunning) return;

            Handles.BeginGUI();

            // 左上にデバッグ情報
            var rect = new Rect(10, 10, 260, 120);
            GUI.Box(rect, GUIContent.none);
            GUILayout.BeginArea(new Rect(14, 14, 252, 112));

            GUILayout.Label($"[SHMUP Play Test]", EditorStyles.boldLabel);
            GUILayout.Label($"Game: {_testGameData.gameName}");
            GUILayout.Label($"Level: {_testLevelIndex}");
            GUILayout.Label($"FPS: {(1f / Time.unscaledDeltaTime):F0}");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("■ Stop", GUILayout.Width(60))) StopPlayTest();
            if (GUILayout.Button("↻ Restart", GUILayout.Width(70))) RestartPlayTest();
            ShowDebugOverlay = GUILayout.Toggle(ShowDebugOverlay, "Overlay");
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
            Handles.EndGUI();
        }
    }

    // --------------------------------------------------
    // Runtime Bootstrap Component
    // --------------------------------------------------
    /// <summary>
    /// Playモード時にシーンに配置され、テスト実行を担うMonoBehaviour。
    /// Phase 5でフルランタイム実装と差し替え予定。
    /// </summary>
    public class ShmupPlayTestBootstrap : MonoBehaviour
    {
        [HideInInspector] public ShmupGameData gameData;
        [HideInInspector] public int startLevelIndex;
        [HideInInspector] public bool showOverlay;

        private float _elapsed;
        private ShmupLevelData _currentLevel;

        private void Start()
        {
            if (gameData == null || gameData.levels.Count == 0)
            {
                Debug.LogWarning("[ShmupCreator] テストデータまたはレベルが空です。");
                return;
            }

            if (startLevelIndex >= gameData.levels.Count)
                startLevelIndex = 0;

            _currentLevel = gameData.levels[startLevelIndex];
            Debug.Log($"[ShmupCreator] Level \"{_currentLevel.levelName}\" を読み込み中...");

            SetupCamera();
            SpawnTestObjects();
        }

        private void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
        }

        private void SpawnTestObjects()
        {
            // プレイヤー（スタブ）
            if (gameData.playerData != null)
            {
                var playerObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                playerObj.name = "Player";
                playerObj.transform.position = new Vector3(0, -3, 0);
                playerObj.transform.localScale = Vector3.one * 0.5f;
                playerObj.GetComponent<Renderer>().material.color = Color.cyan;
                playerObj.AddComponent<ShmupTestPlayerController>();
            }

            // Wave情報表示
            if (_currentLevel.waves != null)
            {
                foreach (var wave in _currentLevel.waves)
                {
                    if (wave == null || wave.enemyData == null) continue;
                    Debug.Log($"  Wave: {wave.name} - Enemy: {wave.enemyData.name} x{wave.count} @ {wave.spawnTime}s");
                }
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;

            // ESCで停止
            if (Input.GetKeyDown(KeyCode.Escape))
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
            }
        }

        private void OnGUI()
        {
            if (!showOverlay) return;

            GUI.color = new Color(1, 1, 1, 0.8f);
            GUI.Label(new Rect(10, 10, 300, 20), $"Level: {(_currentLevel != null ? _currentLevel.levelName : "N/A")}");
            GUI.Label(new Rect(10, 30, 300, 20), $"Time: {_elapsed:F1}s");
            GUI.Label(new Rect(10, 50, 300, 20), "ESC: Stop  |  WASD/Arrow: Move");
        }
    }

    /// <summary>テスト用の簡易プレイヤー操作</summary>
    public class ShmupTestPlayerController : MonoBehaviour
    {
        private float _speed = 5f;

        private void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            transform.position += new Vector3(h, v, 0) * _speed * Time.deltaTime;
        }
    }
}
