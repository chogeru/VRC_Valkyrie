# VRC_Valkyrie

VRChat向けワールドプロジェクト（Unity + VRChat SDK3 + UdonSharp、Built-in Render Pipeline）。

現在このリポジトリには2つのワールドが同居している:

- **ゾンビサバイバル**（`Assets/_Project`） — ウェーブ制ゾンビシューティング。ロビー→戦闘→ウェーブクリア→ロビーのループ、スコア・武器強化ショップ・NPCゾンビAIを実装
- **VRCDogWorld**（`Assets/_Project/Scenes/VRCDogWorld.unity`） — 犬と遊ぶ和風の街並みワールド。NavMeshAgentで動く犬AI、玩具/餌やり/アジリティ設備を実装

## セットアップ手順

ゾンビワールドの詳細なシーン組み立て手順（データアセットの作り方、GameManager配線、ゾンビ/武器の自動セットアップツールの使い方など）は
[`Assets/_Project/SETUP.md`](Assets/_Project/SETUP.md) を参照。

## ディレクトリ構成

```
Assets/
├── _Project/                自作コンテンツ一式（本プロジェクトの中心）
│   ├── Scenes/               VRCDogWorld.unity ほか
│   ├── Scripts/               UdonSharpBehaviour（.cs + 対応する .asset ペア）
│   │   ├── Core/               GameManager / AudioManager / GameSettings（中央設定ハブ）
│   │   ├── Dog/                 DogAI / DogToy / DogBall / FoodBowl / WaterBowl / AgilityWaypoint
│   │   ├── Player/              PlayerHealthManager / PlayerDataRegistry（HP・スコアの事前配置プール）
│   │   ├── UI/                   HudController / GameStartButton
│   │   ├── Waves/                WaveManager / WaveConfig
│   │   ├── Weapons/              Gun / AmmoPickup / WeaponConfig / WeaponUpgradeStation
│   │   └── Zombies/              ZombieAI / ZombieConfig / ZombieHeadHitbox
│   ├── Data/                  上記スクリプトのScriptableObject的データアセット（Weapons/Zombies/Waves/Dog）
│   ├── Editor/                 Editor専用自動化ツール（Zombie Game > ... メニュー）
│   │   ├── WeaponSetupTool.cs      武器パックの一括インポート・配線・Config生成
│   │   ├── ZombieSetupTool.cs      ゾンビプレハブの一括配線・NavMeshベイク
│   │   ├── ZombieAnimatorSetupTool.cs  ロコモーションAnimator Controller生成
│   │   ├── DogWorldMapBuilder.cs   VRCDogWorldのマップ自動構築
│   │   ├── DogWorldPostProcess.cs  VRCDogWorldのポストプロセス設定
│   │   └── OrganizeHierarchy.cs    シーンHierarchyの自動整理
│   ├── Animations / PostProcessing  各種アニメーション・ポストプロセスアセット
│   └── SETUP.md                手動セットアップ手順書（詳細は上記リンク参照）
│
├── ThirdParty/                外部アセット（キャラクター・環境・武器・音声など）
│   ├── Characters/              Sparrow等のキャラクターモデル
│   ├── Environment/              TsubokuLab日本の街並み、Japan_Village_ArtE等
│   ├── Weapons/                  Infima Games / Low Poly系武器パック
│   ├── Malbers Animations/       アニメーションアセット
│   └── _SourcePackages/          未展開の生 .unitypackage 置き場
│
├── UdonSharp/                  UdonSharp本体 + 汎用UtilityScripts（BoneFollower等）
├── SerializedUdonPrograms/      UdonSharpのビルド出力（自動生成、直接編集しない）
├── XR / Plugins / Screenshots   SDK付属・入力系・スクリーンショット
```

## 開発上のポイント

- `Assets/_Project/Scripts` 配下の `.cs`（ロジック）と同名 `.asset`（UdonSharpBehaviourの
  シリアライズ実体）は必ずペアで存在する。`.cs`だけ編集してもシーン上の挙動には反映されないため、
  Unity Editor側でコンパイルを通してから動作確認すること
- ゾンビ/武器プレハブの一括配線・データ生成はすべて `Zombie Game >` Editorメニューから行う
  （`WeaponSetupTool.cs` / `ZombieSetupTool.cs` 参照）。手動でコンポーネントを付け外しするより
  こちらを優先する
- 動作確認はClientSimでも可能だが、Ownership絡みの処理（ダメージ・スコア・ティア強化）は
  実機2人以上でのテストが必須（詳細は `SETUP.md` の「VRChatアップロード前チェックリスト」）
