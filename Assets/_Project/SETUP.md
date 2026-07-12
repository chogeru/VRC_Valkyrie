# ゾンビWorld セットアップ手順

`Assets/_Project/Scripts` に一式のUdonSharpスクリプトを実装済み。Unity Editor側で
シーンに配置・配線する必要がある。以下の順で進める。

## 1. データアセットを作る（数値調整の中枢）

いずれも「空のGameObjectにスクリプトを付けるだけ」でOK（ScriptableObjectではなく
UdonSharpBehaviourなので、シーン上 or プレハブとして配置する）。

- `Assets/_Project/Data/Weapons/` に `WeaponConfig` を付けたGameObjectを銃の種類分作る
  （例: Pistol, Rifle, Shotgun）。damagePerHit / fireRate / isAutomatic などを調整。
- `Assets/_Project/Data/Zombies/` に `ZombieConfig` を1つ（Walker）。
- `Assets/_Project/Data/Waves/` に `WaveConfig` をウェーブ数分（Wave1, Wave2, ...）。

## 2. GameSettings（中央設定ハブ）

シーンに空のGameObject `GameSettings` を作り `GameSettings.cs` を付ける。
- `waves` に手順1のWaveConfigを順番に並べる
- `lobbySpawnPoints` / `battleSpawnPoints` / `playerRespawnPoints` / `zombieSpawnPoints`
  にそれぞれTransformを配置して登録
- `playerHealthObjectPrefab` は手順5で作るプレイヤーHPプレハブを指す

## 3. ロビー & 戦闘エリア

- ロビー部屋（学校の一室）を作り、`lobbySpawnPoints` に対応する位置にTransformを置く
- 戦闘エリア（教室アセットで校舎を組む）を作り、`battleSpawnPoints` /
  `zombieSpawnPoints` / `playerRespawnPoints` を配置
- 戦闘エリアの床にNavMeshをBake（Window > AI > Navigation）

## 4. GameManager / WaveManager / HudController

- `GameManager` GameObjectを作り `GameManager.cs` を付与。`settings` / `waveManager` /
  `hud` を後述の各GameObjectに紐付け
- `WaveManager` GameObjectを作り `WaveManager.cs` を付与。`settings` / `gameManager` /
  `hud` / `zombiePool`（手順6のゾンビ配列）を紐付け
- Canvas上にHUDを作り `HudController.cs` を付与、TextMeshProフィールドとPanelを配線

## 5. プレイヤーHP（Player Object）

1. 空のプレハブ `PlayerHealthObject` を作り `PlayerHealthManager.cs` を付与
   （`settings` / `hud` を紐付け）
2. Hierarchy上の `VRCSceneDescriptor` を選択し、Inspectorの **Player Objects** リストに
   このプレハブを登録する（★これを忘れるとプレイヤーごとのHPが機能しない）
3. `GameSettings.playerHealthObjectPrefab` に同じプレハブを指定

## 6. ゾンビ・プール

- ゾンビ用GameObject（Capsuleなどのプレースホルダー、または手持ちモデル）に:
  - `NavMeshAgent`
  - `Collider`
  - `ZombieAI.cs`（`config` = 手順1のZombieConfig, `settings`, `waveManager` を紐付け）
  - **VRC Object Sync**（もしくはUdonBehaviourのSync SettingsでContinuous Position/Rotation）
  - 頭部に子Colliderを作り `ZombieHeadHitbox.cs` を付けるとヘッドショット倍率が有効になる
- このGameObjectをコピーしてプール数分（同時出現数の上限）シーンに配置し、初期状態は
  非アクティブにしておく
- 全インスタンスを `WaveManager.zombiePool` 配列に登録

## 7. 銃

- 銃モデルに `VRC Pickup` コンポーネント + `Gun.cs` を付与
  - `config` = 手順1のWeaponConfig
  - `muzzle` = 銃口のTransform
  - `settings` = GameSettings
- 弾薬箱には `AmmoPickup.cs`（Trigger Collider必須）

### Infima Games等のアセットストア製FPSパックを使う場合の注意

`Assets/Infima Games/...` に同梱されている `FireBehaviour` / `CharacterBehaviour` などの
スクリプトは**通常のMonoBehaviourでUdonSharpではない**。VRChatにアップロードした
ワールドではUdon化されていないスクリプトは一切実行されないため、これらはUnity Editor上の
デモ再生専用と考える。実際に使うのは銃のメッシュ・マテリアル・（必要なら）アニメーション・
サウンドのみで、発砲/リロード等のロジックはすべてこちらの `Gun.cs`（U#）で行う。

作業を高速化するため `Assets/_Project/Editor/WeaponSetupTool.cs` にEditor専用ツールを用意した
（このスクリプトはEditorフォルダ内にあるためVRChatビルドには含まれない）:

1. メニュー `Zombie Game > Weapons > 1. Generate Starter WeaponConfigs` を実行すると
   `Assets/_Project/Data/Weapons/` に7種類（AG14W, HVG7, LRAF9, MAK12, RC425, SP60, X13）の
   WeaponConfigプレハブが自動生成される。名称・数値は「アサルトライフル/LMG/スナイパー/
   ピストル/SMG/ショットガン/マシンピストル」という**推測**でアーキタイプを割り当てた
   仮の値なので、実際のモデル形状を見て名前や数値をInspectorで調整すること
2. Infimaの武器プレハブ（例: `Assets/Infima Games/.../Prefabs/Weapons/AG14W/Variants/
   P_LPAMG_WEP_AG14W_Full_Default_B.prefab`）をシーンにドラッグ&ドロップしてインスタンス化
3. そのGameObjectを選択した状態でメニュー `Zombie Game > Weapons > 2. Wire Selected
   GameObject As Gun` を実行すると、Rigidbody・VRCPickup・Gun.cs が自動で付与される
4. Inspectorで `Gun.config` に手順1で生成した対応するWeaponConfigを割り当て、
   `Gun.muzzle` には銃口位置に作成した空のTransformを、`Gun.settings` にはGameSettingsを
   割り当てる
5. Colliderが無ければ追加する（VRC Pickupで掴むために必須）

## 8. スタートボタン

- ロビーに置くオブジェクトに Collider + `GameStartButton.cs` を付与、`gameManager` を紐付け
- インタラクトすると `GameManager.RequestStartGame` が全クライアントに送られ、
  マスターだけが実際に状態を進める

## 9. VRCSceneDescriptor

- `spawns` にロビーのデフォルトスポーン地点を設定
- Player Objects に手順5のプレハブを登録（再掲・重要）

## 動作確認のコツ

- ClientSim または実機（2人以上）でロビー→GameStart→戦闘→ウェーブクリア→勝利→ロビー
  復帰の一周を必ず確認する
- ゾンビが動かない場合はNavMeshのBake漏れを疑う
- ダメージが反映されない場合はOwnership（`Networking.SetOwner`）周りのログを確認

## 拡張ポイント

- 銃の種類を増やす: WeaponConfigを複製するだけ（コード変更不要）
- ウェーブを増やす/難易度調整: WaveConfigを増やす・数値を変えるだけ
- ゾンビの種類を増やす: ZombieConfigを複製し、ZombieAIの`config`違いのプール
  グループを用意すれば拡張可能（現状は1種類構成）
