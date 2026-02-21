# ShootingMapper 設計書

**Unity Shmup Creator Editor - メニュー拡張設計書**
v2.0 | 2026年2月

---

## 1. プロジェクト概要

シューティングゲーム制作ツール「SHMUP Creator」の各機能を、Unity エディタの EditorWindow / MenuItem として再実装するプロジェクト。
SHMUP Creator 本家の UI/UX パターン（3パネル構成、色分け、即座プレイテスト）を忠実に踏襲しつつ、Unity ネイティブの操作感を提供する。

### UPM パッケージ情報

| 項目 | 値 |
|------|-----|
| パッケージ名 | `com.dsgarage.shootingmapper` |
| 表示名 | ShootingMapper |
| バージョン | 0.1.0 |
| Unity要件 | 2021.3 LTS 以上 |
| 依存 | `com.unity.mathematics` |
| 名前空間 | `ShmupCreator.Editor` / `ShmupCreator.Runtime` |
| リポジトリ | `https://github.com/dsgarage/ShootingMapper.git` |

---

## 2. SHMUP Creator 本家 UI 分析と設計方針

### 2.1 本家の特徴（Unity実装に反映）

| 特徴 | 本家の実装 | Unity での対応 |
|------|-----------|---------------|
| 空間ベースのレベル設計 | キャンバス上に敵を配置 → スクロールで出現 | EditorWindow 内カスタム描画キャンバス |
| 3パネル構成 | 左:Game Box / 中央:Canvas / 右:Properties | EditorWindow 内 3カラム分割 |
| 色分け | 青=ゲーム全体 / 黄=レベル固有 | `ShmupEditorStyles` で統一管理 |
| 4カラム武器エディタ | Sets → Weapons → Properties → Preview | 同構成の EditorWindow |
| 即座のプレイテスト | Space=エディタ内 / Tab=全画面 | `ShmupPlayTestManager` + MenuItem ショートカット |
| リアルタイムプレビュー | 弾幕・爆発・パーティクル | EditorApplication.update シミュレーション |
| タブナビゲーション | 各エディタ内で設定をタブ分類 | `ShmupEditorStyles.DrawTabBar()` |
| コンテキスト依存プロパティ | 選択対象に応じて右パネル変化 | 各ウィンドウの Property パネル |
| ツールチップ | 全UI要素にホバー説明 | `GUIContent` の tooltip パラメータ |

### 2.2 色分けルール

```
青 (GameScopeColor)  = ゲーム全体に影響する設定
  → Game Settings, Player, Weapon, Enemy 定義
  → ヘッダー背景: #263859

黄 (LevelScopeColor) = レベル固有の設定
  → Level Settings, Wave, Trigger, Background
  → ヘッダー背景: #594D1F
```

---

## 3. メニュー構成

```
Shmup Creator/
├── Dashboard             Ctrl+Shift+D   メインハブ
├── Level Editor          Ctrl+L         空間ベースレベル編集
├── Weapon Editor         Ctrl+W         4カラム弾幕設計
├── Enemy Editor          Ctrl+E         タブベースエネミー定義
├── HUD Editor                           HUD レイアウト + プレビュー
├── Gameplay Rules                       スコア/チェーン/メダル/ランク
├── FX Editor                            爆発/パーティクル + プレビュー
├── Game Settings                        ゲーム全体設定
├── ─────────────
├── ▶ Play Test           Space          エディタ内テスト開始/停止
├── ■ Stop Test           Shift+Space    テスト停止
├── ↻ Restart Test        Ctrl+Space     テストリスタート
├── ─────────────
└── Scene Overlay/
    ├── Toggle Overlay                   オーバーレイ表示切替
    ├── Show Paths                       エネミーパス表示
    ├── Show Wave Zones                  Wave出現エリア表示
    └── Show Camera Wake                 カメラ起動ゾーン表示
```

---

## 4. 各 EditorWindow 詳細設計

### 4.1 Dashboard (`ShmupDashboardWindow`)

メインハブ。ゲーム全体を俯瞰し、各エディタへの導線を提供する。

```
┌──────────────────────────────────────────────────┐
│ [Toolbar]  Shmup Creator          ▶ Play Test    │
├──────────────────────────────────────────────────┤
│ [GAME] Game Data                                 │
│  GameData: [■ MyGame.asset    ] [+]              │
│  Game Name: [My Shooting Game     ]              │
│  Resolution: [1920] [1080]                       │
│  Scroll: [Vertical ▾]                            │
├──────────────────────────────────────────────────┤
│ [GAME] Player                                    │
│  Player: [■ Player.asset     ] [+]               │
│  Speed: 5  |  Weapons: 2  |  Hitbox: 0.1        │
│  [Weapon Editor で編集 →]                         │
├──────────────────────────────────────────────────┤
│ [LEVEL] Levels                                   │
│  ┌────────────────────────────────┬─────┬──┬──┐  │
│  │ Stage 1 - Forest              │120s │W:5│▶│✎│ │
│  │ Stage 2 - Ocean               │ 90s │W:3│▶│✎│ │
│  │ Stage 3 - Boss Rush           │ 60s │W:8│▶│✎│ │
│  └────────────────────────────────┴─────┴──┴──┘  │
│  [+ Level]                                       │
├──────────────────────────────────────────────────┤
│ Quick Access                                     │
│  [Weapon Editor] [Enemy Editor] [HUD Editor]     │
│  [Gameplay Rules] [FX Editor] [Game Settings]    │
└──────────────────────────────────────────────────┘
```

**機能:**
- GameData アセットの自動検出・`EditorPrefs` による選択記憶
- Level 一覧から ▶(テスト) / ✎(編集) にワンクリックアクセス
- 全エディタへの Quick Access ボタン

### 4.2 Level Editor (`ShmupLevelEditorWindow`)

SHMUP Creator 本家に準拠した空間ベースのレベルエディタ。

```
┌────────────────────────────────────────────────────────┐
│ [Level.asset ▾] Grid(G) [===zoom===]    ▶ Test Level  │
│ [Waves] [Backgrounds] [Triggers] [Settings]            │
├──────┬──────────────────────────────────┬──────────────┤
│Game  │         Canvas                   │ Properties   │
│Box   │                                  │              │
│──────│   ┌────────────────┐             │ [LEVEL]      │
│Enemies│   │  Camera View   │             │ Properties   │
│ EnemyA│   │                │             │              │
│ EnemyB│   │    [WaveA]     │             │ Spawn: 5.0s  │
│ [+New]│   │                │             │ Count: 3     │
│──────│   │    [WaveB]     │             │ Spacing: 0.5 │
│Triggers│  │                │             │ Formation:   │
│ +Shake│   └────────────────┘             │  [V ▾]       │
│ +BGM  │                                  │ Enemy:       │
│──────│   ⚡ BGMChange @ 10.0s           │  [EnemyA ▾]  │
│Sound │                                  │              │
│Items │                                  │ [Enemy Editor│
│      │                                  │   で編集 →]  │
└──────┴──────────────────────────────────┴──────────────┘
```

**3パネル構成:**
- **左 (Game Box)**: Enemies / Triggers / Sound / Items のタブ切替パレット
- **中央 (Canvas)**: グリッド付き空間キャンバス。Wave・Trigger をスクロール位置に配置
- **右 (Properties)**: 選択オブジェクトのコンテキスト依存プロパティ

**操作:**
- Game Box の「+」で Wave をキャンバスに追加
- キャンバス上の Wave をクリックで選択 → 右パネルにプロパティ表示
- マウスホイールでキャンバススクロール
- G キーでグリッド表示切替

### 4.3 Weapon Editor (`ShmupWeaponEditorWindow`)

SHMUP Creator 本家の 4カラムドリルダウンレイアウト。

```
┌──────────────────────────────────────────────────────────┐
│ [Toolbar] Weapon Editor  [Weapon.asset ▾] [+]           │
├─────────────────┬────────────────┬────────────────────────┤
│ [GAME] Weapon   │ Properties     │ Preview               │
│                 │                │                ▶ ■ ↻ ♪│
│ Fire Rate: 0.2  │ [Burst][Bullets│                        │
│ Sound: [clip ▾] │                │   ┌──────────────┐    │
│─────────────────│ Spread         │   │              │    │
│ Bullet Patterns │                │   │    · · ·     │    │
│ ▶ FanPattern    │ [Fan] [Circle] │   │   ·  ↑  ·   │    │
│   CirclePattern │ [Line] [Random]│   │  ·   ■   ·  │    │
│                 │                │   │   ·     ·   │    │
│   [↑] [↓]      │ Count:  [==8=] │   │    · · ·     │    │
│                 │ Angle:  [=30=] │   │              │    │
│ [+ Pattern]     │ Offset: [==0=] │   └──────────────┘    │
│ [- Remove]      │                │                        │
│                 │ Sub Pattern    │ Time: 1.23s            │
│                 │ [None ▾]       │ Bullets: 24            │
└─────────────────┴────────────────┴────────────────────────┘
```

**4カラム構成:**
1. **Weapon / Pattern List**: 武器情報 + パターン一覧（上下並び替え対応）
2. **Properties**: Burst / Bullets の2タブ切替
3. **Preview**: `BulletSimulator` によるリアルタイム弾道描画

**弾道プレビュー:**
- `EditorApplication.update` でフレーム毎にシミュレーション
- `BulletSimulator.CreateBullets()` / `StepSimulation()` を使用
- パターン種別ごとに色分け（Fan=赤, Circle=青, Line=緑, Random=黄）
- 軌跡表示 + 弾数カウント

### 4.4 Enemy Editor (`ShmupEnemyEditorWindow`)

SHMUP Creator 本家に準拠した 5タブ構成。

```
タブ: [Settings] [Movement] [Weaponry] [Gameplay] [FX]
```

| タブ | 内容 |
|------|------|
| Settings | スプライト（プレビュー付き）・HP・スコア値 |
| Movement | 移動速度・ウェイポイント一覧・SceneView パス編集 |
| Weaponry | 武器データ参照・Weapon Editor への遷移リンク |
| Gameplay | スコア値・アイテムドロップ設定 |
| FX | 爆発スプライト・持続時間・SE |

**SceneView パス編集:**
- 「Scene で編集」ボタンで SceneView に Gizmo パスハンドルを表示
- `Handles.PositionHandle` でウェイポイントをドラッグ移動
- `Shift+Click` で新規ウェイポイント追加
- パスラインは `Handles.DrawLine` で接続表示

### 4.5 HUD Editor (`ShmupHUDEditorWindow`)

```
左カラム: Score / Life / Gauges / Font の4タブ切替
右カラム: HUD プレビュー（ゲーム画面のモック表示）
```

### 4.6 Gameplay Rules (`ShmupGameplayWindow`)

```
タブ: [Scoring] [Chain] [Medal] [Rank]
```

- Scoring: 弾消しボーナス設定
- Chain: 連続撃破チェーンシステム
- Medal: メダル回収スコアシステム
- Rank: 動的難易度調整（ランク）システム

### 4.7 FX Editor (`ShmupFXEditorWindow`)

```
左カラム: Explosions / Particles タブ切替プロパティ
右カラム: リアルタイムプレビュー（爆発・パーティクル描画）
```

---

## 5. デバッグ＆プレイテストシステム

### 5.1 ShmupPlayTestManager

SHMUP Creator 本家の「Space で即座テスト」ワークフローを再現。

| ショートカット | 動作 |
|--------------|------|
| `Space` | エディタ内テスト開始/停止（デバッグオーバーレイ付き） |
| `Shift+Space` | テスト停止 |
| `Ctrl+Space` | テストリスタート（停止→自動再開始） |
| `ESC` | Playモード中にテスト停止 |

**テスト実行フロー:**
1. 現在のシーンを保存
2. テスト用の空シーン `__ShmupPlayTest__` を自動生成
3. `ShmupPlayTestBootstrap` コンポーネントを配置
4. Play モードに入る
5. テスト停止時に元のシーンに自動復帰

**デバッグオーバーレイ:**
- SceneView 左上にゲーム名・レベル番号・FPS を表示
- Stop / Restart ボタン
- オーバーレイ表示の ON/OFF 切替

### 5.2 テスト用ランタイム（スタブ）

`ShmupPlayTestBootstrap` が Play モード開始時に以下を自動セットアップ:

- カメラ（オルソグラフィック）
- プレイヤー（Quad + WASD 操作）
- Wave 情報のコンソール出力

> Phase 5 でフルランタイム実装に差し替え予定。

### 5.3 Scene Overlay (`ShmupSceneOverlay`)

SceneView 上にレベル情報を常時表示。エディタ上でゲームの空間配置を確認可能。

| オーバーレイ | 表示内容 |
|------------|---------|
| Paths | 全エネミーのウェイポイントパス（色分け表示 + アニメーション） |
| Wave Zones | Wave の出現エリア |
| Camera Wake | カメラのエネミー起動ゾーン枠 |

- `PathEvaluator.EvaluateLinearPath()` によるパスアニメーション
- SceneView 右上にトグルボタン群

---

## 6. ScriptableObject データモデル

```
ShmupGameData
├── gameName, resolution, scrollDirection
├── playerData → ShmupPlayerData
│   ├── sprite, speed, hitboxRadius
│   └── weaponSets[] → ShmupWeaponData
│       ├── fireRate, fireSound
│       └── bulletPatterns[] → ShmupBulletPatternData
│           ├── spreadType, bulletCount, spreadAngle
│           ├── speed, acceleration, homing
│           └── subPattern → ShmupBulletPatternData (再帰)
└── levels[] → ShmupLevelData
    ├── levelName, duration
    ├── waves[] → ShmupWaveData
    │   ├── spawnTime, count, spacing, formation
    │   └── enemyData → ShmupEnemyData
    │       ├── sprite, hp, scoreValue, moveSpeed
    │       ├── weapon → ShmupWeaponData
    │       ├── movePath[] (Vector2)
    │       └── explosion (ExplosionData)
    ├── backgrounds[] (BackgroundEntry)
    └── triggers[] (TriggerEntry)

ShmupHUDData
├── scoreDisplay, lifeDisplay (HUDElementData)
├── gauges[] (HUDGaugeData)
└── font

ShmupGameplayData
├── chain: enabled, timeWindow, multiplierStep
├── medal: enabled, baseScore, multiplier
├── bulletCancel: enabled, bonusPerBullet
└── rank: enabled, increaseRate, decreaseOnDeath

ShmupItemData
└── type, sprite, effect
```

---

## 7. 共通スタイル基盤 (`ShmupEditorStyles`)

### 色定義
| 名前 | 用途 | RGB |
|------|------|-----|
| GameScopeColor | ゲーム全体設定ヘッダー文字色 | `(0.2, 0.4, 0.8)` |
| GameScopeBg | ゲーム全体設定ヘッダー背景 | `(0.15, 0.22, 0.35)` |
| LevelScopeColor | レベル固有設定ヘッダー文字色 | `(0.9, 0.75, 0.2)` |
| LevelScopeBg | レベル固有設定ヘッダー背景 | `(0.35, 0.30, 0.12)` |
| CanvasBg | キャンバス背景 | `(0.18, 0.18, 0.22)` |
| AccentGreen | 再生ボタン | `(0.3, 0.8, 0.4)` |
| AccentRed | 停止ボタン | `(0.9, 0.3, 0.3)` |

### 共通ヘルパー
- `DrawScopeHeader(title, isGameScope)` - 色付きスコープヘッダー
- `DrawTabBar(tabNames, selected)` - タブバー描画
- `DrawColumnSeparator()` - カラム区切り線
- `DrawSeparator()` - セクション区切り
- `LabelWithTooltip(label, tooltip)` - ツールチップ付きラベル

---

## 8. フォルダ構成

```
ShootingMapper/                        # UPM パッケージルート
├── package.json                       # UPM 定義
├── CHANGELOG.md
├── LICENSE
├── DESIGN.md                          # 本設計書
├── .gitignore
│
├── Editor/                            # Editor アセンブリ
│   ├── ShmupCreator.Editor.asmdef
│   ├── Windows/
│   │   ├── ShmupDashboardWindow.cs    # メインハブ
│   │   ├── ShmupLevelEditorWindow.cs  # 空間ベースレベル編集
│   │   ├── ShmupWeaponEditorWindow.cs # 4カラム弾幕設計
│   │   ├── ShmupEnemyEditorWindow.cs  # 5タブエネミー定義
│   │   ├── ShmupHUDEditorWindow.cs    # HUD + プレビュー
│   │   ├── ShmupGameplayWindow.cs     # ゲームルール 4タブ
│   │   ├── ShmupFXEditorWindow.cs     # FX + プレビュー
│   │   └── ShmupGameSettingsWindow.cs # ゲーム全体設定
│   ├── Debug/
│   │   └── ShmupPlayTestManager.cs    # 即座プレイテスト
│   ├── Drawers/                       # カスタム PropertyDrawer
│   ├── Gizmos/
│   │   └── ShmupSceneOverlay.cs       # SceneView オーバーレイ
│   └── Styles/
│       └── ShmupEditorStyles.cs       # 共通スタイル・色分け
│
├── Runtime/                           # Runtime アセンブリ
│   ├── ShmupCreator.Runtime.asmdef
│   ├── Data/                          # ScriptableObject 定義
│   │   ├── ShmupGameData.cs
│   │   ├── ShmupLevelData.cs
│   │   ├── ShmupWaveData.cs
│   │   ├── ShmupEnemyData.cs
│   │   ├── ShmupWeaponData.cs
│   │   ├── ShmupBulletPatternData.cs
│   │   ├── ShmupPlayerData.cs
│   │   ├── ShmupItemData.cs
│   │   ├── ShmupHUDData.cs
│   │   └── ShmupGameplayData.cs
│   └── Simulation/                    # Editor/Runtime 共用ロジック
│       ├── BulletSimulator.cs         # 弾道シミュレーション
│       └── PathEvaluator.cs           # パス評価（折れ線・ベジェ）
│
├── Tests/
│   ├── Editor/
│   │   └── ShmupCreator.Editor.Tests.asmdef
│   └── Runtime/
│       └── ShmupCreator.Runtime.Tests.asmdef
│
├── Resources/                         # デフォルトアセット・アイコン
└── Documentation~/
    └── index.md                       # UPM ドキュメント
```

---

## 9. ウィンドウ間の連携フロー

```
Dashboard ──▶ Play Test ボタン ──▶ ShmupPlayTestManager
    │                                      │
    ├── Level ✎ ──▶ Level Editor           ├── テスト用シーン自動生成
    │                  │                    ├── Bootstrap 配置
    │                  ├── Wave 選択        └── Play モード自動実行
    │                  │     └── Enemy Editor で編集 →
    │                  │              │
    │                  │              └── Weaponry タブ
    │                  │                    └── Weapon Editor で編集 →
    │                  └── ▶ Test Level
    │
    ├── Player → Weapon Editor で編集 →
    │
    └── Quick Access → 各 EditorWindow
```

**EditorPrefs によるアセット記憶:**
- 各ウィンドウは最後に選択したアセットの GUID を `EditorPrefs` に保存
- ウィンドウ再開時に自動復元
- Dashboard の GameData は全ウィンドウで共有

---

## 10. 開発フェーズ（更新版）

| フェーズ | 内容 | 状態 |
|---------|------|------|
| Phase 1 | データ基盤: ScriptableObject 定義・asmdef・package.json | **完了** |
| Phase 2 | Core Editors: Dashboard・Level Editor・Weapon Editor・Enemy Editor | **完了** |
| Phase 3 | Path & Preview: SceneView パス編集・弾道プレビュー・Scene Overlay | **完了** |
| Phase 4 | サブシステム: HUD Editor・Gameplay Rules・FX Editor (プレビュー付き) | **完了** |
| Phase 5 | デバッグ: PlayTestManager・テスト用ランタイム・ショートカット | **完了** |
| Phase 6 | フルランタイム: ゲーム実行エンジン・Wave スポーンシステム・弾幕実行 | 未着手 |
| Phase 7 | 出力・統合: ビルド出力・SHMUP Creator 形式 export | 未着手 |

---

## 11. 技術要件

| 項目 | 要件 |
|------|------|
| Unity バージョン | 2021.3 LTS 以上 |
| ターゲット | Unity Editor (Windows / macOS) |
| 依存パッケージ | `com.unity.mathematics` |
| スクリプト言語 | C# |
| 名前空間 | `ShmupCreator.Editor` / `ShmupCreator.Runtime` |
| アセンブリ定義 | `ShmupCreator.Editor.asmdef` / `ShmupCreator.Runtime.asmdef` |
| テスト | Unity Test Framework |
| 配布方式 | UPM (Git URL) |
