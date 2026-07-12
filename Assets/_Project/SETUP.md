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
- `lobbySpawnPoints` / `battleSpawnPoints` / `zombieSpawnPoints`
  にそれぞれTransformを配置して登録（`lobbySpawnPoints`はゲーム開始時の
  ロビー出現位置に加えて、**死亡時の帰還先**としても使われる）
- `playerDataRegistry` は手順5で作る `PlayerDataRegistry` を指す

## 3. ロビー & 戦闘エリア

- ロビー部屋（学校の一室）を作り、`lobbySpawnPoints` に対応する位置にTransformを置く
- 戦闘エリア（教室アセットで校舎を組む）を作り、`battleSpawnPoints` /
  `zombieSpawnPoints` を配置
- 戦闘エリアの床にNavMeshをBake（Window > AI > Navigation）

## 4. GameManager / WaveManager / HudController

- `GameManager` GameObjectを作り `GameManager.cs` を付与。`settings` / `waveManager` /
  `hud` / `audioManager`（手順3bで作るAudioManager）を後述の各GameObjectに紐付け
- `WaveManager` GameObjectを作り `WaveManager.cs` を付与。`settings` / `gameManager` /
  `hud` / `audioManager` / `zombiePool`（手順6のゾンビ配列）を紐付け
- Canvas上にHUDを作り `HudController.cs` を付与、TextMeshProフィールドとPanelを配線
- `deathPanel` / `deathCountdownText` は死亡時（HP0）に表示される演出用。無いと
  死亡してから`respawnDelay`秒間、画面上に何のフィードバックも出ないまま
  ロビーへテレポートされるだけになるので、簡単な暗転パネル＋
  「You Died / Respawning in N...」表示を用意しておくと体験が良くなる

## 3b. BGM・SFX（`AudioManager` — 追加しやすい設計）

音楽・効果音を増やすたびにコードを書かずに済むよう、**名前とクリップのペア配列**で
管理する専用ハブを用意した。

1. 空のGameObject `AudioManager` を作り `AudioManager.cs` を付与
2. 子に空のGameObjectを3つ作り、それぞれAudioSourceを付けて紐付ける:
   - `musicSourceA` / `musicSourceB`（BGM用、Loop有効・クロスフェードで自動的に
     切り替わる。2つ用意するのはフェード中に新旧2トラックを同時再生するため）
   - `sfxSource`（グローバルな効果音用、Loop不要）
3. `musicNames` / `musicClips` に好きな数だけBGMを登録（例: `Lobby`, `Battle`,
   `Victory`）。`sfxNames` / `sfxClips` も同様に効果音を登録
   （例: `CountdownStart`, `WaveStart`, `Victory`, `PlayerHurt`, `PlayerDown`）
4. 新しいBGM/SFXを増やしたい時は、この配列に1行追加するだけでよい。
   コード側は `audioManager.PlayMusic("名前")` / `audioManager.PlaySfx("名前")` を
   呼ぶだけで、存在しない名前を指定した場合はConsoleに警告が出るだけで安全に無視される
5. 現在コードから自動的に呼ばれるフック一覧（対応する名前を登録しておくこと）:
   - `GameManager`: ロビー/戦闘/勝利への遷移で `PlayMusic("Lobby" / "Battle" /
     "Victory")`、カウントダウン開始時に `PlaySfx("CountdownStart")`、勝利時に
     `PlaySfx("Victory")`
   - `WaveManager`: 各ウェーブ開始時に `PlaySfx("WaveStart")`
   - `PlayerHealthManager`: 被弾時に `PlaySfx("PlayerHurt")`、HP0になった瞬間に
     `PlaySfx("PlayerDown")`（本人のクライアントでのみ再生される）
   - 銃の発砲音・リロード音・ゾンビの鳴き声など**位置に紐づく音**は従来通り
     各オブジェクト個別のAudioSource（`Gun.fireSound`等）で再生され、この
     AudioManagerは経由しない（3D音源として正しく聞こえるようにするため）

## 5. プレイヤーHP・スコア（事前配置プール方式）

このVRChat SDKバージョン（3.7.6）には「Player Object」自動割り当て機能が無いため、
プレイヤーごとのデータ（HP・スコア）は**事前にシーンへ配置したプールを、参加時に
本人のクライアントが自分で確保する**方式で実装している。

1. 空のGameObject `PlayerSlot` を作り `PlayerHealthManager.cs` を付与
   （`settings` / `hud` / `audioManager` を紐付け）。これをワールドの最大人数分（例: 16〜32体）
   コピーしてシーンに並べる（`Player_00`, `Player_01`, ... のように）
   - 座標はどこでも良い（見た目を持たないデータ用オブジェクトのため）。
     まとめて1つの空GameObjectの子にしておくとHierarchyが整理される
2. 空のGameObject `PlayerDataRegistry` を作り `PlayerDataRegistry.cs` を付与し、
   `pool` 配列に手順1で並べた `PlayerHealthManager` を**全員分**登録する
   （★人数分を超えて参加されると新規プレイヤーはHP/スコアが機能しないので、
   余裕を持ったサイズにする。コンソールに警告ログが出るので不足時はすぐ分かる）
3. `GameSettings.playerDataRegistry` に手順2のオブジェクトを指定
4. 仕組み: プレイヤーが参加すると`OnPlayerJoined`が全クライアントで発火し、
   **参加した本人のクライアントだけ**が空きスロットを探して`ClaimForLocalPlayer()`
   で確保する（Ownershipを自分に移してHP/スコアを初期化）。退出時は
   `OnPlayerLeft`から1秒後に、そのスロットの新オーナー（通常はマスター）が
   `ReleaseSlot()`で解放し、次の参加者に再利用される

## 6. ゾンビ・プール

### ゾンビモデル（`Assets/ThirdParty/NewPunch/ShirtlessZombieFree`）

- `Prefabs/ShirtlessZombie_FREE.prefab`（Built-inプロジェクトなので `_HDRP` / `_URP`
  サフィックス無しの素の版を使う。既にBuilt-in向けマテリアルが入っている）
- 同梱の `ZombiesBundleV2/Zombies_Bundle_V2_Assets_Links.txt` は実体が無いリンク集
  （Asset Storeの無料配布ページへのURLのみ）。追加のゾンビ種類が欲しい場合は
  そこに載っている各パッケージを手動で取得する必要がある
- `FreeZombie_EyesGlow.cs` は付属のEditor専用スクリプト（`OnValidate`のみ、
  実行時には何もしない）なのでそのままで問題ない

### セットアップ手順（`Zombie Game > Zombies` メニューで自動化）

1. `ShirtlessZombie_FREE.prefab` をシーンにドラッグ&ドロップしてインスタンス化
2. そのGameObjectを選択した状態で `Zombie Game > Zombies > 2. Wire Selected
   GameObject As Zombie` を実行すると、CapsuleCollider・NavMeshAgent・
   AudioSource（3D）・`ZombieAI.cs` が自動付与され、末尾で非アクティブ化まで行われる
   （プールは非アクティブ状態で待機させる仕様のため）
3. Inspectorで `ZombieAI.config`（手順の`1. Generate Starter ZombieConfig`で
   生成される `ZombieConfig_Walker`）、`ZombieAI.settings`（GameSettings）、
   `ZombieAI.waveManager` を紐付ける
4. **VRC Object Sync**（もしくはUdonBehaviourのSync SettingsでContinuous
   Position/Rotation）を追加
5. 頭部に子Colliderを作り `ZombieHeadHitbox.cs` を付けるとヘッドショット倍率が有効になる
6. このGameObjectをコピーしてプール数分（同時出現数の上限）シーンに配置する
   （手順2のツールは非アクティブ化するので、複製してそのまま並べればOK）
7. 全インスタンスを `WaveManager.zombiePool` 配列に登録
8. 戦闘エリアの床にNavMeshが無ければ `Zombie Game > Zombies > 3. Bake NavMesh
   For Current Scene` で焼く（Window > AI > Navigationからでも可）

### アニメーション

このモデルのFBXには走行アニメーション（`FreeRunning`）が1つ入っているだけで、
待機・攻撃・死亡モーションは同梱されていない。そのため以下の方針にした:

- **移動**: `Zombie Game > Zombies > 4. Build Locomotion Animator Controller
  From Selected FBX` を実行（`ShirtlessZombie_FREE.fbx` を選択した状態、または
  Animator+Avatarを持つシーン上のインスタンスでもOK）すると、
  `Assets/_Project/Data/Zombies/ZombieLocomotion.controller` が生成され、
  中に入っている走行クリップをループ再生する1ステートのControllerになる。
  生成後、ゾンビプレハブの `Animator` コンポーネントの `Controller` 欄に
  割り当てる
- **攻撃・死亡**: 対応する既製アニメーションが無いため、`ZombieAI.cs`が
  **スクリプトだけで**簡易モーションを再現する（`Gun.cs`のスライドアニメと
  同じ考え方のLerpベースの手続き型アニメーション）:
  - 攻撃時: `ZombieAI.visualRoot`（任意）を前後にラウンジさせる。既定では
    未割り当てなら何もしない（NavMeshAgentが動かしているルートTransformを
    直接動かすと喧嘩するため）。ラウンジを使いたい場合はモデルの階層を
    「ルート(Collider/NavMeshAgent) → 子(見た目メッシュ一式=visualRoot)」の
    形に組み替えてから割り当てる
  - 死亡時: ルートTransform自体を`deathCollapseLocalRotationEuler`分だけ倒し、
    `deathSinkDistance`分だけ沈める（死亡と同時にNavMeshAgentを無効化する
    ので、ルートを直接動かしても衝突しない）。全クライアントで同時に見える
    （`syncedDead`フラグの同期で駆動されるため）
  - 各種の時間・移動量は `ZombieAI` のInspectorから調整可能

より作り込んだアニメーション一式（待機/攻撃/被弾/死亡モーション付き）が
欲しい場合は、`ZombiesBundleV2`のリンク集にある他のゾンビパッケージ、または
別途フルアニメーション付きのゾンビアセットを導入し、Animator Controllerに
ステートを追加してから `animator.SetTrigger("Die")` 等と噛み合わせる形に
拡張できる（`ZombieAI.cs`側は変更不要）。

### ゾンビのボイス（`Assets/ThirdParty/Zombie Voices Audio Pack`）

1. ゾンビのプレハブ（プール1体分）に `AudioSource` を追加し、`ZombieAI.voiceAudioSource`
   に紐付ける（3D Sound推奨: Spatial Blend = 1）
2. 手順1で作った `ZombieConfig` のInspectorで、`Assets/ThirdParty/Zombie Voices Audio Pack/`
   配下のWAVファイルを用途別に複数ドラッグ&ドロップする:
   - `attackClips` ← `Attack/` フォルダ
   - `damageClips` ← `Pain/` または `Damage/` フォルダ
   - `deathClips` ← `Death/` フォルダ
   - `idleClips` ← `Grunt/` や `Breathing/` フォルダ（追跡中にランダムで再生、
     `idleClipChancePerRetarget` で頻度調整）
3. 死亡音は同期フラグ経由で全クライアントに再生される。被弾音・徘徊音は
   現在そのゾンビを所有しているクライアント（主に攻撃者かマスター）でのみ
   再生される軽量実装（3D音源なので位置は正しく聞こえる）

## 7. 銃

- 銃モデルに `VRC Pickup` コンポーネント + `Gun.cs` を付与
  - `config` = 手順1のWeaponConfig
  - `muzzle` = 銃口のTransform
  - `settings` = GameSettings
  - `hud` = HudController（キル数によるティアアップ通知を出すため）
- 弾薬箱には `AmmoPickup.cs`（Trigger Collider必須）。`pickupSound`は拾得音用の
  AudioSourceで、`visual`（拾得後に非表示になる見た目オブジェクト）とは別の
  常時アクティブな場所に置くこと（`visual`側に置くと再生前に非表示化されて
  音が途切れる）

### スライド・チャージングハンドルのアニメーション（スクリプト駆動、任意）

`Gun.cs`はベイクされたAnimatorクリップに依存せず、スライド/ボルトとチャージング
ハンドルの前後移動を完全にコードで再現する（`Update()`内でlocalPositionを
Lerpするだけの軽量実装）。どのモデルでも「動かしたい部品のTransform」を
割り当てるだけで使えるので拡張・差し替えが容易:

- `Gun.slide` — 発砲するたびに後退→前進する部品（スライド/ボルト）のTransform。
  空なら何もしない
  - `slideBackOffset`（既定 (0,0,-0.03)） — 後退時のローカル座標オフセット
  - `slideBackDuration` / `slideForwardDuration` — 後退・前進にかかる時間
- `Gun.chargingHandle` — リロード完了時に1回だけ前後する部品のTransform
  - `chargingHandleBackOffset`（既定 (0,0,-0.05)）
  - `chargingHandleCycleDuration` — 後退+前進の往復にかかる合計時間

いずれも「見た目上動かしたい子オブジェクトのTransform」をInspectorにドラッグする
だけで有効になる。モデルのローカル軸によって符号（+/-）を調整すること。

### 命中エフェクト・装弾数UI（任意）

- `Gun.impactEffect` — 弾が何かに当たった瞬間（ゾンビでも環境でも）に、その位置へ
  移動して再生されるParticleSystem。1つのオブジェクトを使い回す実装（発砲毎に
  Instantiateしない）なので、あらかじめ銃の子オブジェクトとして1個だけ配置し、
  再生時間を1発分より短めに設定しておく
- `Gun.ammoDisplayText` — 銃のグリップ横などに小さく置いた**3D TextMeshPro**
  （CanvasのUIではなく`GameObject > 3D Object > Text - TextMeshPro`で作るワールド
  空間テキスト）。「現在弾数 / 予備弾数」を発砲・リロード・弾薬拾得のたびに自動更新。
  Desktop/VRどちらでも同じ見え方になる（画面固定UIではなく銃に物理的についている
  テキストのため）

### PC / VR 操作対応について

`OnPickupUseDown`/`OnPickupUseUp`（発砲）と`Interact()`（改造ボタン・ゲーム開始
ボタン・NPCなど）はVRChat SDKの統一入力イベントで、Desktopのクリック長押しと
VRのトリガー引きが自動的に同じイベントとして処理される。`WeaponUpgradeStation.cs`
の`GetPickupInHand`もLeft/Right両方の手を見ているため、VRのコントローラーは
もちろんDesktopの単一仮想ハンドでも正しく持っている銃を検出できる。
**つまりこのシステムは追加コード無しで両プラットフォーム対応済み。**

### Scene View ギズモ

設定を見やすくするため、主要スクリプトに`OnDrawGizmos`/`OnDrawGizmosSelected`を
追加した（Editor専用、Udonの実行には一切影響しない）:

| 色 | 対象 |
|---|---|
| 青 | `GameSettings.lobbySpawnPoints`（死亡時の帰還先も兼ねる） |
| 赤 | `GameSettings.battleSpawnPoints` |
| 橙 | `GameSettings.zombieSpawnPoints` |
| 赤い半透明球（選択時） | `ZombieAI` の攻撃範囲(`attackRange`) |
| 黄色い線（選択時） | `Gun` の射程(`range`)・スライド可動域 |
| 橙の線（選択時） | `Gun` のチャージングハンドル可動域 |
| 黄緑のワイヤーキューブ | `GameStartButton` |
| マゼンタのワイヤーキューブ | `WeaponUpgradeStation` |
| 黄色いワイヤー球 | `AmmoPickup` |

### 未インポートの武器パック（Low Poly AR/Pistol/SMG/Shotgun/WWII等）

`Assets/`直下に生の `.unitypackage` として置かれているだけの武器パックがある場合、
まずインポートが必要。Unity Editorが既に開いているため外部からの二重起動はせず、
Editor内メニューで完結させる:

1. `Zombie Game > Weapons > 0. Import Raw Weapon Packages (URP)` — 各パックの
   `_URP.unitypackage` を一括インポート（本プロジェクトはBuilt-in Render
   Pipelineのため、マテリアルは後でシェーダーをStandard系に張り替える必要がある）
2. インポート完了後（コンソールのImport進捗が終わってから）
   `Zombie Game > Weapons > 0b. Move Imported Packs Into ThirdParty` を実行すると、
   生の`.unitypackage`は`Assets/ThirdParty/_SourcePackages/`へ、展開されたフォルダは
   `Assets/ThirdParty/`直下へ自動整理される
3. 新しく追加するパック名は `WeaponSetupTool.cs` の `RawWeaponPackFolders` 配列に
   追記すれば同じ手順でインポート対象になる

### 銃種ごとのデータ作成

- `Zombie Game > Weapons > 1. Generate Starter WeaponConfigs` — Infima Gamesの
  AG14W/HVG7/LRAF9/MAK12/RC425/SP60/X13向けの仮WeaponConfigを生成
- `Zombie Game > Weapons > 1b. Generate Category Archetype WeaponConfigs` — 新しく
  追加したLow Poly AR/Pistol/SMG/Shotgun/WWIIパックはモデルごとの個別名が無いため、
  「アサルトライフル/ピストル/SMG/ショットガン/ボルトアクションライフル」の
  カテゴリ別アーキタイプとして生成される。実際に使うモデル1つにつき複製して
  リネーム・調整する運用を想定

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

## 7b. スコア & 改造ショップ（3段階強化）

流れ: **ゾンビを倒す → スコア獲得 → 改造ショップの改造ボタンをインタラクト →
持っている銃が1段階強化（最大3段階、段階が上がるほど必要スコアも増える）**。

### スコア（プレイヤーのウォレット）

- スコアは `PlayerHealthManager`（Player Object、手順5で作成済み）が保持する
  （`syncedScore`、Networked Sync済み）
- ゾンビを倒すと `ZombieConfig.scoreValue`（既定10点）が倒した本人に加算される
  （`Gun.cs`が`ZombieAI.TakeDamage`の戻り値でキルを検知し、`PlayerHealthManager.AddScore`
  を呼ぶ）
- HUDのCanvasに `scoreText` 用TextMeshProを追加し `HudController.scoreText` に紐付けると
  現在のスコアが表示される

### 改造ショップ

1. ロビーか戦闘エリアに「改造ボタン」用のGameObjectを置き、Collider +
   `WeaponUpgradeStation.cs` を付与。`hud` にHudControllerを紐付ける
2. プレイヤーが銃を手に持った状態でこのオブジェクトをインタラクトすると、
   `WeaponUpgradeStation` が `player.GetPickupInHand()` で持っている銃を検出し、
   `Gun.TryUpgrade()` を呼ぶ
3. `Gun.TryUpgrade()` は次ティアの必要スコア（`WeaponConfig.tierUpgradeCost`）を
   `PlayerHealthManager.TrySpendScore` でその場で消費し、成功すれば `Gun.tier` を+1する
   （最大3、`Gun.MaxTier`）
4. ティアは銃オブジェクト自体に同期保存されるため、他プレイヤーに渡っても引き継がれる

`WeaponConfig` のInspectorで調整できる項目（すべて長さ3の配列 = tier1/2/3）:

- `tierUpgradeCost`（既定 50 / 120 / 250 スコア） — 各段階を買うのに必要なスコア。
  **段階が上がるほど値を大きくする**とご要望通りの「踏むごとに価格が上がる」挙動になる
- `tierDamageMultiplier`（既定 1.15 / 1.35 / 1.6倍） — 威力
- `tierFireRateMultiplier`（既定 1.1 / 1.25 / 1.45倍） — 連射速度
- `tierMagazineSizeMultiplier`（既定 1.25 / 1.5 / 2倍） — 装弾数（弾数）
- `tierReloadTimeMultiplier`（既定 0.9 / 0.8 / 0.65倍、小さいほど速い） — リロード速度

ティアが上がると `HudController.OnWeaponTierChanged` が呼ばれ、`weaponTierText` に
「〇〇 Tier N Up!」というトースト表示が数秒出る（`Gun.tierUpSound` を割り当てれば
効果音も鳴る）。スコア不足・最大ティア到達・武器未所持のときは `shopMessageText` に
理由が表示される。HUDのCanvas上に `weaponTierText` / `shopMessageText` 用の
TextMeshProをそれぞれ追加して `HudController` に紐付けること。

## 8. スタートボタン

- ロビーに置くオブジェクトに Collider + `GameStartButton.cs` を付与、`gameManager` を紐付け
- インタラクトすると `GameManager.RequestStartGame` が全クライアントに送られ、
  マスターだけが実際に状態を進める

## 9. VRCSceneDescriptor

- `spawns` にロビーのデフォルトスポーン地点を設定
  （このSDKバージョンには「Player Objects」欄は無い。手順5の
  `PlayerDataRegistry`が同等の役割を担う）

## ゲーム全体のフロー（一周）

```
ロビー（全員待機）
  └─ 誰かが GameStartButton をインタラクト
       └─ カウントダウン（SFX: CountdownStart）
            └─ 戦闘エリアへ全員テレポート、WaveManagerがウェーブ開始（BGM: Battle）
                 ├─ ゾンビを倒す → スコア加算 → 改造ショップで銃を強化
                 ├─ 被弾してHP0 → 本人だけ即ロビーへ帰還・HP全回復
                 │     （respawnDelay秒後。生き残ったプレイヤーは戦闘続行）
                 ├─ 全ウェーブクリア → Victory（BGM/SFX: Victory）
                 │     → victoryDisplayTime秒後、自動でロビーへ全員帰還
                 └─ 参加中の全プレイヤーのHPが同時に0（全滅）→ Game Over
                       （BGM/SFX: GameOver、WaveManagerのスポーンも停止）
                       → gameOverDisplayTime秒後、自動でロビーへ全員帰還
       └─ ロビーに戻ったら再度GameStartButtonで次のラウンドを開始できる
```

- 個人の死亡は「戦線離脱してロビーで回復して待つ」という扱いで、他の生存者は
  戦闘を続けられる。全員が同時にHP0になった瞬間だけ全滅（Game Over）と判定される
  （`GameManager`が`gameOverCheckInterval`秒ごとにマスターだけでチェック）
- Victory/Game Overとも`ReturnToLobby`を呼ぶ点は共通で、次のラウンドへの
  導線は同じ

## 動作確認のコツ

- ClientSim または実機（2人以上）で「ロビー→GameStart→戦闘→ウェーブクリア→
  勝利→ロビー」の一周に加えて、「被弾してロビーに戻る」「全員死んでGame Over
  になる」の2パターンも必ず確認する
- ゾンビが動かない場合はNavMeshのBake漏れを疑う
- ダメージが反映されない場合はOwnership（`Networking.SetOwner`）周りのログを確認
- Game Overにならない場合は `GameSettings.playerDataRegistry` の紐付けと、
  `PlayerDataRegistry.pool` に全プレイヤー分のスロットが登録されているか確認

## 拡張ポイント

- 銃の種類を増やす: WeaponConfigを複製するだけ（コード変更不要）
- ウェーブを増やす/難易度調整: WaveConfigを増やす・数値を変えるだけ
- ゾンビの種類を増やす: ZombieConfigを複製し、ZombieAIの`config`違いのプール
  グループを用意すれば拡張可能（現状は1種類構成）
- BGM/SFXを増やす: `AudioManager`のNames/Clips配列に1行追加するだけ（3b節参照）
