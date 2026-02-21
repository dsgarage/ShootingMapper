# ShootingMapper

Unity Editor拡張によるシューティングゲーム制作ツール。SHMUP Creatorのワークフローを参考に、Unityネイティブの操作感でレベル編集・弾幕設計・エネミー配置等を行えます。

## インストール

Unity Package Manager (UPM) からGitHub URLで導入できます。

1. Unity Editor で `Window > Package Manager` を開く
2. `+` ボタン > `Add package from git URL...` を選択
3. 以下のURLを入力:

```
https://github.com/dsgarage/ShootingMapper.git
```

## メニュー構成

Unityメニューバーの `Shmup Creator` からアクセス:

| メニュー | 機能 |
|---|---|
| Game Settings | ゲーム全体設定 |
| Level Editor | タイムラインベースのレベル編集 |
| Weapon Editor | 弾幕パターン設計 |
| Enemy Editor | エネミー定義・パス編集 |
| HUD Editor | HUIレイアウト |
| Gameplay Rules | スコア・ゲームルール |
| FX Editor | 爆発・パーティクル |

## データモデル

すべてのデータは ScriptableObject (.asset) で管理されます。`Assets > Create > Shmup Creator` から各データアセットを作成できます。

## 動作要件

- Unity 2021.3 LTS 以上
- 依存パッケージ: `com.unity.mathematics`
