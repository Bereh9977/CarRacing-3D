# Car Racing 3D: project structure (final design)

This document describes the **finished game**: every scene, class, use case and flow. It is meant for drawing the SADT (IDEF0), use-case, sequence and class diagrams.

The design is a port of the 2D pygame game [Car Racing](https://github.com/Bereh9977/Car-Racing) to Unity 6 (URP). Every rule (laps, bot, coins, obstacles, race points, rating, shop, pause, music) comes from the pygame code and keeps the same behaviour. Only the technology changes: 2D images and masks become 3D models, colliders and triggers.

**Status of every part:**
- ✅ **done:** in the repo and working.
- 🧪 **written, not tested:** in the repo, still being tested.
- 📋 **planned:** not written yet. The names, fields and methods below are the plan. If the code ends up different, this document will be updated.

The diagrams can show everything regardless of status. The status only says how far the work has got.

---

## 1. The game

### 1.1 Modes
| Mode | Racers | Camera | Coins and race points saved |
|---|---|---|---|
| **Single** | Player (WASD) vs Bot | one full-screen camera | yes |
| **Doubles** | Player 1 (WASD) vs Player 2 (arrows), one keyboard | split screen, left/right | no (race points are shown during the race but not saved) |

### 1.2 Race rules (from pygame)
- **Laps.** A race has 1 or 2 laps (`MapConfig.requiredLaps`). Every lap is a small race of its own: **the first car to cross the finish line wins the lap**. Then both cars go back to the start and a new countdown begins.
- **Winner.** When the last lap is over, the racer with more laps won wins the race. A 2-lap race can end in a tie (1:1).
- **Countdown.** Before every lap: the pictures 3, 2, 1 and GO from the pygame game, one second each. Cars can't move until GO.
- **Prize (Single only):** win **+200 coins**, tie **+100 coins**, loss 0.
- **Coins:** each race shows **5 random coins** of the ones placed on the map (at least 5), **+100 coins** each. Only the player collects them; Single mode only.
- **Obstacles:** each map has obstacles placed by hand on fixed spots. At race start **up to 4 random ones** are switched on. They affect only cars driven by people (both cars in Doubles), not the bot.

  | Obstacle | Maps | Effect | Disappears after a hit |
  |---|---|---|---|
  | Ice cube | Winter | like sand but stronger: while on it, the car turns by itself (270°/s) | no |
  | Banana peel | Summer | the car spins in place for 1 s (720°/s) with no control, then goes on with its speed | yes |
  | Sand pile | Map2, Map3, Beach | while on it, the car turns by itself in a random direction that changes every 1.5 s (135°/s, a bit more than steering) | no |

- **Walls** (road borders and fences) bounce the car back with 65 % of its speed, reversed.

### 1.3 Race points (from pygame `RaceTracker`)
Race points are separate from coins and are counted live during the race.

| Event | Points |
|---|---|
| Pass a checkpoint first (8 checkpoints per lap, in order; invisible lines across the road, spread evenly along the `BotRoute`) | +100 |
| Pass a checkpoint second | +50 |
| Win a lap | +200; the lap's points (checkpoints + win) are multiplied by the win streak: ×1, ×1.5, ×2 |
| Hit a wall (a new hit counts only 0.5 s after the last one; not the bot) | −20 |
| Hit an obstacle (cars driven by people only) | −50 |
| Win all laps of a 2-lap race ("flawless") | +200 |

Points never go below 0. In Single mode, the player's and the bot's points are added to their totals in the save file. Doubles shows the points but doesn't save them.

### 1.4 Rating
Two rows, the player and the Bot. The Bot starts with **5000** race points, so it is first at the beginning. If the points are equal, the Bot stays above. Under the table:
- "Mission: beat Bot! N race points to go", or
- "Mission complete! You beat Bot!"

### 1.5 Shop
Cars and maps can be locked. A locked item shows its price: cars **3000**, maps **5000** coins. Buying takes the coins and unlocks the item forever.

### 1.6 Keys
| Key | Action | Input action |
|---|---|---|
| W/S, A/D | Player 1 throttle and steering | `Player1/Throttle`, `Player1/Steer` ✅ |
| ↑/↓, ←/→ | Player 2 throttle and steering | `Player2/Throttle`, `Player2/Steer` ✅ |
| Esc | Pause / resume | `Game/Pause` ✅ |
| M | Music on/off | `Game/ToggleMusic` ✅ |
| N | Next music track | `Game/NextTrack` ✅ |

All the actions already exist in `InputSystem_Actions.inputactions`.

---

## 2. Folders
```
Assets/
├── Scenes/              MainMenu + 6 map scenes
├── Scripts/
│   ├── Car/             CarController ✅, PlayerCarInput ✅, CarCamera ✅, BotDriver 🧪
│   ├── Race/            RaceManager 🧪, FinishLine 🧪, Racer 🧪, RaceTracker 🧪,
│   │                    RaceHud 📋, PauseMenu 🧪
│   ├── Pickups/         Coin 🧪, Obstacle 🧪
│   ├── Audio/           AudioManager 📋
│   ├── Data/            MapConfig ✅, CarDefinition ✅, ObstacleType ✅
│   ├── Core/            SaveData ✅, SaveService ✅
│   ├── Editor/          CarRacingMenu ✅ (Unity Editor tools, not part of the game)
│   └── (root)           menu scripts (teammate): GameSession ✅, PanelNavigator ✅, ModeSelector ✅,
│                        CarSelector ✅, MapSelector ✅, ShopButton 📋, CoinsLabel 🧪,
│                        RatingPanel 🧪, OptionsPanel 📋
├── Data/
│   ├── Maps/            6 MapConfig assets (one per map)
│   └── Cars/            5 CarDefinition assets (car1–car5)
├── Prefabs/             5 cars (Mini_Cooper, Subaruu_Impreeza, Porschee_959_Paris_Dakar, Lanncia_Stratoss, Toyotta_Celicca), Race 🧪, Coin, Sand, Ice, Banana, AudioManager 📋
├── Resources/           AudioManager prefab 📋 (loaded by code, so no scene needs it)
├── InputSystem_Actions.inputactions
└── (asset packs)        BEDRILL (track pieces), Ash Assets (cars), BTM_Assets (coins), trees, rocks…
Docs/                    SceneContract.md (rules for map scenes), this file
```

---

## 3. Scenes and prefabs

### 3.1 Scenes
| Scene | What it is |
|---|---|
| `MainMenu` | All menu screens as UI panels: Main, Mode, Cars, Maps, Rating, Options |
| `Map2`, `Map3`, `Map_Champions_Field` | Loop tracks, 2 laps |
| `Map_Summer`, `Map_Winter`, `Map_Beach` | Tracks that aren't loops (start ≠ finish), 1 lap |

**Menu panels (MainMenu scene):**
| Panel | Contents | Goes to |
|---|---|---|
| Main | Start, Rating, Options buttons; coins label 📋 | Mode, Rating, Options |
| Mode | Single, Doubles | Single → Cars, Doubles → Maps |
| Cars | 5 cars; locked ones have a buy button 📋 | Maps |
| Maps | 6 maps; locked ones have a buy button 📋 | loads the map scene |
| Rating 📋 | Player and Bot rows, mission text | back |
| Options 📋 | Music on / off, volume slider, key list | back |

Every panel except Main has a Back button (`PanelNavigator.Back`).

### 3.2 What every map scene contains
| Object | Contents | Status |
|---|---|---|
| `Main Camera` | Gets `CarCamera` at runtime | ✅ |
| `Lighting`, `Environment`, `Track`, `Decorations` | Groups made by the Organize tool | ✅ |
| Start/finish road piece | Contains the `FinishLine` trigger | 🧪 |
| `Coins` group | at least 5 `Coin` / `StarCoin` objects; the race shows 5 random ones | ✅ |
| `Obstacles` group | `Obstacle` objects of the map's type on fixed spots (maps with obstacles only) | 🧪 |
| `BotRoute` | Route points for the bot along the road (children, in order), made by **Tools → Car Racing → Generate Bot Route**. A plain object, no script | ✅ |
| `Race` (prefab) | Everything the race needs; see 3.3 | 🧪 |

### 3.3 Prefabs and their components
| Prefab | Components (scripts in bold) |
|---|---|
| Car (`Mini_Cooper`, `Subaruu_Impreeza`, `Porschee_959_Paris_Dakar`, `Lanncia_Stratoss`, `Toyotta_Celicca`) | Rigidbody, CapsuleCollider, **CarController**, **PlayerCarInput** |
| Bot car (made at runtime from a car prefab) | as above, but `PlayerCarInput` is switched off and **BotDriver** is added |
| `Race` | **RaceManager**, **PauseMenu**; children `PlayerSpawn` (car 1), `BotSpawn` (car 2), `Canvas` (**RaceHud**: lap, points, coins, countdown, result panel, pause panel), `EventSystem`. In Doubles a copy of `Main Camera` is made at runtime for the right half |
| `Coin`, `StarCoin` | trigger collider, **Coin** |
| `Ice`, `Banana`, `Sand` | trigger collider on the `Obstacle` layer, **Obstacle** |
| `AudioManager` | 2 AudioSources (music, effects), **AudioManager** |

**Physics layers:** `Car` (8), `Wall` (9), `Pickup` (11), `Obstacle` (12).

---

## 4. Classes

### 4.1 Overview
| Class | Kind | Folder | Status | Purpose |
|---|---|---|---|---|
| `CarController` | MonoBehaviour | Car | ✅ (obstacle effects, reset and events 📋) | Moves a car; the same class for player and bot cars |
| `PlayerCarInput` | MonoBehaviour | Car | ✅ (`SetActions` 🧪) | Keyboard → `CarController` |
| `BotDriver` | MonoBehaviour | Car | 🧪 | Route points → `CarController` |
| `CarCamera` | MonoBehaviour | Car | ✅ | Follows a car |
| `RaceManager` | MonoBehaviour | Race | 🧪 (the full version is 📋) | Runs the race: spawn, countdown, laps, result, rewards |
| `Racer` | plain C# class | Race | 🧪 | One participant: name, car and race-point state |
| `RaceTracker` | plain C# class | Race | 🧪 | Counts race points; builds the 8 checkpoints from the `BotRoute` |
| `FinishLine` | MonoBehaviour | Race | 🧪 | Detects crossings of the finish line |
| `RaceHud` | MonoBehaviour | Race | 📋 | All texts and panels during the race |
| `PauseMenu` | MonoBehaviour | Race | 🧪 | Esc pause: Resume, Options, Menu |
| `Coin` | MonoBehaviour | Pickups | 🧪 | +100 coins on pickup |
| `Obstacle` | MonoBehaviour | Pickups | 🧪 | Ice / banana / sand effect |
| `AudioManager` | MonoBehaviour, one per game | Audio | 📋 | Music playlist and sound effects |
| `MapConfig` | ScriptableObject | Data | ✅ | Map settings |
| `CarDefinition` | ScriptableObject | Data | ✅ | Car settings |
| `ObstacleType` | enum | Data | ✅ | `None`, `IceCube`, `BananaPeel`, `SandPile` |
| `SaveData` | Serializable class | Core | ✅ (race results, music 📋) | The player's progress |
| `SaveService` | static class | Core | ✅ | Loads and saves `SaveData` as JSON |
| `GameSession` | static class | root | ✅ | Menu choices passed to the map scene |
| `GameMode` | enum | root | ✅ | `Single`, `Doubles` |
| `PanelNavigator` | MonoBehaviour | root (menu) | ✅ | Switches menu panels, Back history |
| `ModeSelector` | MonoBehaviour | root (menu) | ✅ | Mode buttons |
| `CarSelector` | MonoBehaviour | root (menu) | ✅ | Car buttons |
| `MapSelector` | MonoBehaviour | root (menu) | ✅ | Map buttons, loads the scene |
| `ShopButton` | MonoBehaviour | root (menu) | 📋 | Buy button of a locked car or map |
| `CoinsLabel` | MonoBehaviour | root (menu) | 🧪 | Shows the player's coins |
| `RatingPanel` | MonoBehaviour | root (menu) | 🧪 | Rating table and mission text |
| `OptionsPanel` | MonoBehaviour | root (menu) | 📋 | Music on/off, volume |
| `CarRacingMenu` | static class, Editor only | Editor | ✅ | Tools → Car Racing menu |

### 4.2 Class details
Notation: `+` public, `-` private, `[SF]` private field shown in the Inspector (`[SerializeField]`), `static` static member, `event` C# event. 📋 after a member = planned.

#### Car

**`CarController` : MonoBehaviour.** Requires `Rigidbody`. Arcade car physics, ported from pygame `Cars`. Player and bot cars use it the same way: something calls `SetInput` every frame.
- **Fields:**
  - `[SF] maxSpeed: float` (20 m/s; replaced by the map's value)
  - `[SF] maxReverseSpeed: float` (8)
  - `[SF] acceleration: float` (12)
  - `[SF] coastDeceleration: float` (8)
  - `[SF] turnSpeed: float` (120 °/s)
  - `[SF] bounceFactor: float` (0.65)
  - `[SF] spinSpeed: float` (720 °/s) 📋
  - `[SF] sandTurnSpeed: float` (60 °/s) 📋
  - `- body: Rigidbody`
  - `- wallLayer: int`
  - `- speed, throttle, steer: float`
  - `- frozenUntil, spinningUntil: float` (time) 📋
  - `- sandDirection: int` (−1, 0, 1) 📋
  - `- sandDirectionTime: float` 📋
- **Events:**
  - `+ event WallHit: Action<CarController>` 🧪
- **Methods:**
  - `+ SetMaxSpeed(value: float)`
  - `+ SetInput(throttle: float, steer: float)`: both −1…1
  - `+ Stop()` 🧪: speed 0 on the spot (when a lap ends and during the countdown)
  - `+ ResetTo(spawn: Transform)` 🧪: puts the car on the spawn point and sets speed 0
  - `+ Spin(seconds: float)` 🧪: banana
  - `+ SetDrift(degreesPerSecond: float)` 🧪: sand (135) and ice (270); 0 ends it. Picks a new random direction every 1.5 s and fades out below 5 m/s like steering, so a slow car isn't spun on the spot
  - `- Awake()`
  - `- FixedUpdate()`: speed, steering and velocity. While frozen it does nothing; while spinning it only rotates.
  - `- OnCollisionEnter(collision)`: bounce off `Wall`; raises `WallHit` 🧪 (on every wall contact; the cooldown is in `RaceTracker`)

**`PlayerCarInput` : MonoBehaviour.** Requires `CarController`.
- **Fields:**
  - `[SF] throttle, steer: InputActionReference` (Player1 by default)
  - `- car: CarController`
- **Methods:**
  - `+ SetActions(throttle, steer: InputActionReference)` 🧪: Player 2 gets the Player2 actions
  - `- Awake()`
  - `- OnEnable()`
  - `- Update()`: `car.SetInput(...)`

**`BotDriver` : MonoBehaviour** 🧪. Requires `CarController`. Ported from pygame `Bots`. It always drives forward and steers towards the next route point.
- **Fields:**
  - `- ReachDistance: const float` (8 m): how close counts as "reached"
  - `- FullSteerAngle: const float` (30°): the angle at which steering is full
  - `- car: CarController`
  - `- points: Transform[]`
  - `- currentPoint: int`
- **Methods:**
  - `+ SetRoute(route: Transform)`: the `BotRoute` object; its children are the points
  - `+ ResetRoute()`: back to the first point (new lap)
  - `- Update()`: after the last point it stops (`SetInput(0, 0)`). Otherwise it computes the angle to the point and calls `car.SetInput(1, Clamp(angle / FullSteerAngle, −1, 1))`. When close enough, it moves on to the next point.

**`CarCamera` : MonoBehaviour.** Follows a car from behind.
- **Fields:**
  - `[SF] target: Transform`
  - `[SF] offset: Vector3` (0, 4, −9)
  - `[SF] smoothing: float` (5)
- **Methods:**
  - `+ SetTarget(newTarget: Transform)`
  - `- LateUpdate()`

#### Race

**`RaceManager` : MonoBehaviour.** On the `Race` prefab. The centre of the race.
- **Fields:**
  - `[SF] maps: MapConfig[]`
  - `[SF] cars: CarDefinition[]`
  - `[SF] car1Spawn, car2Spawn: Transform`: the player / Player 1 spawn, and the bot / Player 2 spawn
  - `[SF] hud: RaceHud`
  - `[SF] player2Throttle, player2Steer: InputActionReference` 🧪
  - `[SF] countdownClip, lapWonClip, winClip, tieClip: AudioClip` 📋
  - `- map: MapConfig`
  - `- racer1, racer2: Racer` 🧪
  - `- tracker: RaceTracker` 🧪
  - `[SF] pointsText: TMP_Text` 🧪
  - `- lapsPlayed: int` 📋
  - `- raceOver: bool` 📋
- **Methods:**
  - `- Start()`: finds the map, spawns the racers, sets up the cameras, subscribes to the events, picks the obstacles, starts the first countdown
  - `- SpawnRacers()` 📋: Single: player + bot; Doubles: car1 (WASD) + car2 (arrows), both with `carMaxSpeed`
  - `- SpawnCar(definition, spawn, maxSpeed): CarController` 📋
  - `- ShowCoins()` 🧪: "Coins: N" from `SaveService.Data.coins`; called at the start, on `Coin.Collected` and after the prize. Hidden in Doubles
  - `- OnDestroy()` 🧪: unsubscribes from the static `Coin.Collected`
  - `- KeepRandom(items: Component[], keep: int)` 🧪: leaves `keep` random items on and turns the rest off. Coins: 5 in Single, 0 in Doubles; obstacles: 4
  - `- SetUpCameras()` 🧪: Single: `Main Camera`, full screen; Doubles: `Main Camera` on the left half and a runtime copy of it (no `AudioListener`) on the right half
  - `- Countdown(): IEnumerator` 📋: coroutine. Input off → 3, 2, 1, GO → input on.
  - `- OnFinishLineCrossed(car: CarController, direction: int)`: the first forward crossing in a lap → `FinishLap(winner)`
  - `- FixedUpdate()` 🧪: `tracker.Update()` while the cars can drive
  - `- Update()` 🧪: points text: Single "Race: N"; Doubles "Car 1: N", "Car 2: N"
  - `- OnWallHit(car)` 🧪 → `tracker.WallHit` (human cars only)
  - `- OnObstacleHit(car)` 🧪 → `tracker.ObstacleHit` (from the static `Obstacle.Hit`)
  - `- OnCoinCollected()` 📋 → `hud.ShowCoins`
  - `- FinishLap(winner: Racer)` 📋: `tracker.FinishLap`, a 1 s pause, then either the last lap → `FinishRace()`, or reset the cars + bot route + `Countdown()`
  - `- FinishRace()` 📋: `tracker.FinishRace`, decides the result, `GiveRewards()`, `hud.ShowResult`
  - `- GiveRewards(result)` 📋: Single only: prize coins, `SaveData.AddRaceResult`, `SaveService.Save()`
  - `- SetRacersInput(enabled: bool)` 📋
  - `+ BackToMenu()`: `Time.timeScale = 1` (in case of pause), loads `MainMenu`; used by the Menu buttons on the result and pause panels
  - `- FindMap(sceneName): MapConfig`
  - `- FindCar(id): CarDefinition`

**`Racer`** 🧪 (plain C# class). One participant: who it is and its race-point state (pygame `RacerStats`).
- **Fields:**
  - `+ name: string` ("You", "Bot", "Car 1", "Car 2")
  - `+ car: CarController`
  - `+ points: int`
  - `+ lapsWon: int`
  - `+ streak: int`
  - `+ nextCheckpoint: int`
  - `+ lapCheckpointPoints: int`
  - `+ lastWallHitTime: float`
  - `+ previousPosition: Vector3`: for checkpoint crossings
- **Methods:**
  - `+ Racer(name, car)`: constructor
  - `+ ResetLap()`

**`RaceTracker`** 🧪 (plain C# class). Race points (pygame `RaceTracker`). The point values from 1.3 are constants in this class.
- **Fields:**
  - `- racers: Racer[]`
  - `- checkpoints, checkpointDirections: Vector3[]` (8): a point in the middle of the road and the driving direction there
  - `- checkpointTaken: bool[]` (8)
- **Methods:**
  - `+ RaceTracker(racers: Racer[], route: Transform)`: puts 8 checkpoints evenly along the `BotRoute` points, like the pygame gates
  - `+ Update()`: every physics step; a racer crossing its next checkpoint forwards, within 10 m of the middle, → `PassCheckpoint`
  - `- Crossed(checkpoint, from, to): bool`
  - `- PassCheckpoint(racer)`: +100 if first, +50 if second
  - `+ WallHit(racer)`: −20, with a 0.5 s cooldown
  - `+ ObstacleHit(racer)`: −50
  - `+ FinishLap(winner: Racer)`: +200 × streak multiplier; resets the streak of the others; resets every racer's lap state
  - `+ FinishRace(requiredLaps: int)`: the flawless bonus
  - `- Penalty(racer, amount)`: never below 0

**`FinishLine` : MonoBehaviour** 🧪. A trigger box on the start/finish road piece; its Z axis is the driving direction.
- **Members:**
  - `+ event Crossed: Action<CarController, int>`: +1 forward, −1 backward
  - `- enteredInFront: Dictionary<CarController, bool>`
- **Methods:**
  - `- OnTriggerEnter(other)`
  - `- OnTriggerExit(other)`
  - `- IsInFront(car): bool`

**`RaceHud` : MonoBehaviour** 📋. On the `Race` canvas.
- **Fields:**
  - `[SF] lapText, pointsText, coinsText: TMP_Text`
  - `[SF] countdownImage: Image`, `[SF] countdownSprites: Sprite[]` (the pygame pictures 3, 2, 1, GO)
  - `[SF] resultPanel: GameObject`
  - `[SF] resultTitle, resultDetails: TMP_Text`
- **Methods:**
  - `+ ShowLap(lap, laps)`
  - `+ ShowPoints(racers: List<Racer>)`: Single "Race: N"; Doubles "Car 1: N", "Car 2: N"
  - `+ ShowCoins(coins)`
  - `+ ShowCountdown(step)`: shows picture `step` (3, 2, 1, GO), or hides it
  - `+ ShowResult(title, lines: string[])`: "You win!" / "Bot wins!" / "Car 1 wins!" / "It's a tie!" + points per racer

**`PauseMenu` : MonoBehaviour** 🧪. On the `Race` prefab. Turned off by `RaceManager` when the race ends.
- **Fields:**
  - `[SF] pauseAction: InputActionReference` (`Game/Pause`)
  - `[SF] pausePanel: GameObject` ("Paused" and the Menu button)
  - `[SF] optionsPanel: GameObject` 📋
  - `- paused: bool`
- **Methods:**
  - `- OnEnable()`: enables the action, hides the panel
  - `- Update()`: Esc → `Pause()` or `Resume()`
  - `+ Pause()`: `Time.timeScale = 0`, shows the panel
  - `+ Resume()`: `Time.timeScale = 1`, hides the panels
  - `+ OpenOptions()` 📋

#### Pickups

**`Coin` : MonoBehaviour** 🧪. On the `Coin` / `StarCoin` prefabs (trigger, `Pickup` layer).
- **Members:**
  - `[SF] value: int` (100)
  - `[SF] spinSpeed: float` (90°/s)
  - `[SF] sound: AudioClip`
  - `+ static event Collected: Action`
- **Methods:**
  - `- Update()`: spins the coin
  - `- OnTriggerEnter(other)`: only a car with `PlayerCarInput` (not the bot) → `SaveService.Data.coins += value`, `Save()`, play the sound, `Collected()`, destroy the coin

`RaceManager.KeepRandom` keeps 5 random coins on in Single and turns them all off in Doubles.

**`Obstacle` : MonoBehaviour** 🧪. On the `Ice`, `Banana`, `Sand` prefabs (trigger, `Obstacle` layer).
- **Fields:**
  - `[SF] type: ObstacleType`, `+ Type` (read-only, used by Validate)
  - `[SF] sound: AudioClip` 📋
- **Methods:**
  - `- OnTriggerEnter(other)`: only human cars. Banana → `car.Spin(1)` and disappears; Sand → `car.SetDrift(135)`; Ice → `car.SetDrift(270)`. It raises the static `Hit(car)`. Later 📋: it plays the sound.
  - `- OnTriggerExit(other)`: Sand and ice → `car.SetDrift(0)`

#### Audio

**`AudioManager` : MonoBehaviour** 📋. A single object for the whole game (pygame `music.py`). It's created by code on game start from `Resources/AudioManager` and kept between scenes (`DontDestroyOnLoad`), so no scene has to contain it.
- **Fields:**
  - `+ static Instance: AudioManager`
  - `[SF] tracks: AudioClip[]` (14 music tracks)
  - `[SF] musicSource, effectsSource: AudioSource`
  - `[SF] toggleMusicAction, nextTrackAction: InputActionReference`
  - `- currentTrack: int`
- **Methods:**
  - `- static CreateOnStart()` (`[RuntimeInitializeOnLoadMethod]`)
  - `- Update()`: M → `ToggleMusic()`, N → `NextTrack()`; starts the next track when one ends
  - `+ PlayEffect(clip: AudioClip)`
  - `+ ToggleMusic()`
  - `+ SetMusicOn(on: bool)`
  - `+ NextTrack()`
  - `+ SetVolume(volume: float)`: saves to `SaveData`

#### Data

**`MapConfig` : ScriptableObject** ✅. One asset per map.
- `+ sceneName: string` (also the map id)
- `+ isFree: bool`
- `+ price: int` (5000)
- `+ requiredLaps: int`
- `+ obstacleType: ObstacleType`
- `+ carMaxSpeed: float`
- `+ botMaxSpeed: float`
- `+ displayName: string` 📋 ("Meadow Rush"…)

**`CarDefinition` : ScriptableObject** ✅. One asset per car.
- `+ id: string` ("car1"…"car5")
- `+ isFree: bool`
- `+ price: int` (3000)
- `+ prefab: GameObject`

**`ObstacleType` (enum)** ✅: `None`, `IceCube`, `BananaPeel`, `SandPile`.

#### Core

**`SaveData` (Serializable)** ✅. Everything kept between sessions, in `save.json`.
- **Fields:**
  - `+ coins: int`
  - `+ purchasedCars: List<string>`
  - `+ purchasedMaps: List<string>`
  - `+ racePoints: int`
  - `+ botRacePoints: int` (5000)
  - `+ musicOn: bool` (true) 📋
  - `+ musicVolume: float` (0.5) 📋
- **Methods:**
  - `+ OwnsCar(car): bool`
  - `+ OwnsMap(map): bool`
  - `+ TryBuyCar(car): bool`
  - `+ TryBuyMap(map): bool`
  - `- TryBuy(id, price, alreadyOwned, owned): bool`
  - `+ PointsToFirstPlace: int` (property)
  - `+ AddRaceResult(points, botPoints)` 🧪: adds to the totals

**`SaveService` (static)** ✅.
- `- data: SaveData`
- `- FilePath: string`
- `+ Data: SaveData` (loads on first use)
- `+ Save()`
- `+ ResetProgress()`

**`GameSession` (static)** ✅.
- `+ Mode: GameMode`
- `+ CarId: string` ("car1")
- `+ MapId: string`

**`GameMode` (enum)** ✅: `Single`, `Doubles`.

#### Menu (teammate's scripts, MainMenu scene)

**`PanelNavigator` : MonoBehaviour** ✅.
- **Fields:**
  - `[SF] startPanel: GameObject`
  - `- history: Stack<GameObject>`
  - `- current: GameObject`
- **Methods:**
  - `- Start()`
  - `+ Show(panel)`
  - `+ Back()`

**`ModeSelector` : MonoBehaviour** ✅.
- **Fields:**
  - `[SF] navigator: PanelNavigator`
  - `[SF] carsPanel, mapsPanel: GameObject`
- **Methods:**
  - `+ SelectSingle()`
  - `+ SelectDoubles()`

**`CarSelector` : MonoBehaviour** ✅.
- **Fields:**
  - `[SF] navigator: PanelNavigator`
  - `[SF] mapsPanel: GameObject`
- **Methods:**
  - `+ SelectCar(carId)`: final version: only works if the car is owned 📋

**`MapSelector` : MonoBehaviour** ✅.
- **Methods:**
  - `+ SelectMap(mapId)`: final version: only works if the map is owned 📋

**`ShopButton` : MonoBehaviour** 📋. On every car/map card. Shows the price on a locked item and hides itself on an owned one.
- **Fields:**
  - `[SF] car: CarDefinition` or `[SF] map: MapConfig` (one is set)
  - `[SF] priceText: TMP_Text`
  - `[SF] purchaseSound: AudioClip`
- **Methods:**
  - `- OnEnable()` → `Refresh()`
  - `+ Buy()`: `SaveService.Data.TryBuyCar/TryBuyMap` → `Save()` → sound → `Refresh()`
  - `- Refresh()`

**`CoinsLabel` : MonoBehaviour** 🧪.
- Requires `TMP_Text` on the same object (`- label: TMP_Text`, set in `Awake`)
- `- Update()`: shows `SaveService.Data.coins`

**`RatingPanel` : MonoBehaviour** 🧪.
- **Fields:**
  - `[SF] firstName, firstPoints, secondName, secondPoints, missionText: TMP_Text`
  - `[SF] firstAvatar, secondAvatar: Image`
  - `[SF] playerIcon, botIcon: Sprite`
- **Methods:**
  - `- OnEnable()`: sorts the player and the Bot (Bot above when equal); shows the mission from `PointsToFirstPlace`

**`OptionsPanel` : MonoBehaviour** 📋. Used in the menu and in the pause menu.
- **Fields:**
  - `[SF] volumeSlider: Slider`
- **Methods:**
  - `- OnEnable()`: slider = saved volume
  - `+ MusicOn()`
  - `+ MusicOff()`
  - `+ OnVolumeChanged(value: float)` → `AudioManager`

#### Editor

**`CarRacingMenu` (static, Editor only)** ✅.
- `+ ValidateOpenScene()`: checks one `RaceManager`, one `FinishLine`, a `BotRoute` with ≥ 2 points and at least 5 coins.
- `+ OrganizeOpenScene()`
- `- GroupFor(root): string`
- `+ GenerateBotRoute()`: walks the road from the `Race` start to the finish line, keeping to the middle between the walls and turning before corners, and puts a point every 10 m under a new `BotRoute` object
- `+ ResetSaveData()`

### 4.3 Relationships (for the class diagram)
| From | Relationship | To | Multiplicity |
|---|---|---|---|
| `PlayerCarInput` | dependency (calls `SetInput`) | `CarController` | 1 → 1 |
| `BotDriver` | dependency (calls `SetInput`) | `CarController` | 1 → 1 |
| `BotDriver` | association (route points) | `BotRoute` object's children (`Transform`) | 1 → * |
| `CarCamera` | association (`target`) | car `Transform` | 1 → 1 |
| `RaceManager` | aggregation (Inspector arrays) | `MapConfig`, `CarDefinition` | 1 → 6, 1 → 5 |
| `RaceManager` | composition (creates and owns) | `Racer` | 1 → 2 |
| `RaceManager` | composition | `RaceTracker` | 1 → 1 |
| `RaceManager` | association | `RaceHud`, `PauseMenu` (same prefab) | 1 → 1 |
| `RaceManager` | creates (Instantiate) | car with `CarController` + `PlayerCarInput` / `BotDriver` | 1 → 2 |
| `RaceManager` | adds / configures | `CarCamera` | 1 → 1..2 |
| `RaceManager` | subscribes to events | `FinishLine.Crossed`, `CarController.WallHit`, `Obstacle.Hit`, `Coin.Collected` | |
| `RaceManager` | dependency | `GameSession`, `SaveService`, `AudioManager` | |
| `Racer` | association | `CarController` | 1 → 1 |
| `RaceTracker` | aggregation | `Racer` | 1 → 2 |
| `Coin` | dependency | `SaveService`, `AudioManager` | |
| `Obstacle` | dependency (calls `Spin/SetDrift`) | `CarController` | |
| `Obstacle`, `MapConfig` | association | `ObstacleType` | |
| `CarDefinition` | association | car prefab (`GameObject`) | 1 → 1 |
| `SaveService` | composition | `SaveData` | 1 → 1 |
| `SaveData` | dependency (parameters) | `CarDefinition`, `MapConfig` | |
| `GameSession` | association | `GameMode` | |
| `ModeSelector`, `CarSelector` | association | `PanelNavigator` | |
| `ModeSelector`, `CarSelector`, `MapSelector` | dependency (write) | `GameSession` | |
| `CarSelector`, `MapSelector`, `ShopButton`, `CoinsLabel`, `RatingPanel` | dependency | `SaveService` | |
| `OptionsPanel`, `ShopButton`, `PauseMenu` | dependency | `AudioManager` | |
| `PauseMenu` | association | `OptionsPanel` | |
| `CarRacingMenu` | dependency | `RaceManager`, `FinishLine`, `Coin`, `SaveService`; creates the `BotRoute` object | |

Inheritance: all MonoBehaviours inherit from `UnityEngine.MonoBehaviour`, and `MapConfig` and `CarDefinition` inherit from `ScriptableObject`. There is no inheritance between our own classes. The pygame `Bots(Cars)` inheritance became composition: the bot is a normal car, `BotDriver` drives it, and it has no `PlayerCarInput`.

---

## 5. Use cases

### 5.1 Actors
| Actor | Who | Notes |
|---|---|---|
| **Player** (Player 1) | the person at the keyboard, WASD + mouse | primary actor |
| **Player 2** | a second person, arrow keys | Doubles mode only |
| **Bot** | the computer opponent | secondary actor, or show it as part of the system |
| **Developer / map author** | uses the Unity Editor tools | outside the game; separate diagram or leave out |

### 5.2 Use cases
| # | Use case | Actors | Relations | Status |
|---|---|---|---|---|
| UC1 | Start the game (open the main menu) | Player | | ✅ |
| UC2 | Choose mode | Player | | ✅ |
| UC3 | Choose car | Player | Single only; «extend» UC5 when the car is locked | ✅ / buying 📋 |
| UC4 | Choose map | Player | «extend» UC5 when the map is locked | ✅ / buying 📋 |
| UC5 | Buy a car or map | Player | | 📋 |
| UC6 | View rating | Player | | 🧪 |
| UC7 | Change options (music on/off, volume) | Player | | 📋 |
| UC8 | Play a race | Player, Player 2 / Bot | «include» UC9, UC10, UC13, UC14 | 🧪 |
| UC9 | Wait for the countdown | Player, Player 2 | | 🧪 |
| UC10 | Drive the car | Player, Player 2 | «include» bounce off walls | ✅ |
| UC11 | Collect a coin | Player | «extend» UC10, Single only | 🧪 |
| UC12 | Hit an obstacle | Player, Player 2 | «extend» UC10, maps with obstacles | 🧪 |
| UC13 | Pass checkpoints and earn race points | Player, Player 2, Bot | | 🧪 |
| UC14 | Win a lap | Player, Player 2, Bot | | 🧪 |
| UC15 | See the race result and get the prize | Player | | 📋 |
| UC16 | Pause the race | Player | «extend» UC8; can «include» UC7 | 🧪 (Options 📋) |
| UC17 | Return to the main menu | Player | from the result or the pause menu | 🧪 |
| UC18 | Control music (M on/off, N next track) | Player | anywhere | 📋 |
| UC19 | Race against the bot | Bot (drives by itself) | specialization of UC8 | 🧪 |
| UC20 | Validate / organize a map scene, reset the save | Developer | | ✅ |

---

## 6. Sequences
Format: **caller → callee.method() → result**. Each subsection is one sequence diagram.

### 6.1 Choose settings and start a race
1. Player clicks **Start** → `PanelNavigator.Show(modePanel)`.
2. Player clicks **Single** → `ModeSelector.SelectSingle()` → `GameSession.Mode = Single` → `PanelNavigator.Show(carsPanel)`.
   (In Doubles: `SelectDoubles()` → `PanelNavigator.Show(mapsPanel)`; step 3 is skipped.)
3. Player clicks a car → `CarSelector.SelectCar(id)` → `SaveService.Data.OwnsCar(car)` → if owned: `GameSession.CarId = id` → `PanelNavigator.Show(mapsPanel)`.
4. Player clicks a map → `MapSelector.SelectMap(id)` → `OwnsMap(map)` → if owned: `GameSession.MapId = id` → `SceneManager.LoadScene(id)`.

### 6.2 Buy a car (the same for a map)
1. Player clicks **Buy** → `ShopButton.Buy()`.
2. `ShopButton` → `SaveService.Data.TryBuyCar(car)` → `false` if there aren't enough coins (nothing happens). Otherwise `coins −= price`, `purchasedCars.Add(id)` → `true`.
3. `ShopButton` → `SaveService.Save()` → writes `save.json`.
4. `ShopButton` → `AudioManager.Instance.PlayEffect(purchaseSound)`.
5. `ShopButton` → `Refresh()` → hides itself; `CoinsLabel` shows the new amount.

### 6.3 View rating
1. Player clicks **Rating** → `PanelNavigator.Show(ratingPanel)` → `RatingPanel.OnEnable()`.
2. `RatingPanel` → `SaveService.Data` → `racePoints`, `botRacePoints`.
3. `RatingPanel` sorts the two rows and shows the mission from `PointsToFirstPlace`.

### 6.4 Race setup (`RaceManager.Start`)
1. `RaceManager` → `FindMap(sceneName)` → `MapConfig`.
2. `RaceManager` → `SpawnRacers()`:
   - **Single:** `SpawnCar(FindCar(GameSession.CarId), spawn1, map.carMaxSpeed)` → `Racer("You")`; `SpawnCar(random car, spawn2, map.botMaxSpeed)` → disables `PlayerCarInput`, adds `BotDriver` → `BotDriver.SetRoute(BotRoute)` (found by name) → `Racer("Bot")`.
   - **Doubles:** `SpawnCar(car1, spawn1)` → `Racer("Car 1")`; `SpawnCar(car2, spawn2)` → `PlayerCarInput.SetActions(Player2 actions)` → `Racer("Car 2")`. All coins are turned off.
   - `SpawnCar` → `Instantiate(definition.prefab)` → `CarController.SetMaxSpeed(...)`.
3. `RaceManager` → `SetUpCameras()` → `AddComponent<CarCamera>().SetTarget(...)` for one or two cameras.
4. `RaceManager` → `new RaceTracker(racers, BotRoute)` → 8 checkpoints along the route.
5. `RaceManager` subscribes to `FinishLine.Crossed`, `Coin.Collected`, `Obstacle.Hit`, and `WallHit` of the human cars.
6. `RaceManager` → `KeepRandom(obstacles, 4)`.
7. `RaceManager` → `hud.ShowLap(1, laps)`, `ShowPoints`, `ShowCoins`.
8. `RaceManager` → `StartCoroutine(Countdown())`.

### 6.5 Countdown (before every lap)
1. `RaceManager` → `SetRacersInput(false)`.
2. `RaceManager` → `AudioManager.PlayEffect(countdownClip)`.
3. Pictures 3, 2, 1: `hud.ShowCountdown(step)` → wait 1 s.
4. Picture GO: `hud.ShowCountdown(step)` → `SetRacersInput(true)` → wait 1 s → hide the picture.

### 6.6 Driving (every frame / physics step)
1. **Human:** Unity → `PlayerCarInput.Update()` → Input System → `CarController.SetInput(throttle, steer)`.
2. **Bot:** Unity → `BotDriver.Update()` → angle to `points[currentPoint]` → `CarController.SetInput(1, steer)`; if the point is reached → `currentPoint++`.
3. Unity → `CarController.FixedUpdate()` → `Rigidbody.MoveRotation`, `Rigidbody.linearVelocity`.
4. Unity → `CarCamera.LateUpdate()`.

### 6.7 Hit a wall
1. Unity physics → `CarController.OnCollisionEnter(collision)`.
2. Wall layer and moving into it → `speed = −0.65 × speed` → `WallHit(car)`.
3. `RaceManager.OnWallHit(car)` → `tracker.WallHit(racer)` → −20 if 0.5 s passed since the last hit → `hud.ShowPoints`.

### 6.8 Pass a checkpoint
1. Unity → `RaceManager.FixedUpdate()` → `tracker.Update()`.
2. For every racer: `Crossed(racer.nextCheckpoint, previousPosition, position)` → `PassCheckpoint(racer)`.
3. +100 if no one took it this lap, otherwise +50; `nextCheckpoint++`.
4. `RaceManager` → `hud.ShowPoints(racers)`.

### 6.9 Collect a coin (Single)
1. Player's car enters the trigger → `Coin.OnTriggerEnter`.
2. `Coin` → `SaveService.Data.coins += 100` → `SaveService.Save()`.
3. `Coin` → `AudioManager.PlayEffect(sound)` → `Collected()` → `Destroy(coin)`.
4. `RaceManager.OnCoinCollected()` → `hud.ShowCoins(SaveService.Data.coins)`.

### 6.10 Hit an obstacle
1. Human car enters the trigger → `Obstacle.OnTriggerEnter`.
2. Depending on the type:
   - **Banana:** `CarController.Spin(1)`.
   - **Sand / Ice:** `CarController.SetDrift(135 / 270)`, until `OnTriggerExit` → `SetDrift(0)`.
3. `Obstacle` → `AudioManager.PlayEffect(sound)`; banana → `Destroy(obstacle)`.
4. `Obstacle` → static `Hit(car)` → `RaceManager.OnObstacleHit` → `tracker.ObstacleHit(racer)` (−50) → `hud.ShowPoints`.

### 6.11 Finish a lap
1. Car crosses the finish line → `FinishLine.OnTriggerExit` → `Crossed(car, +1)`.
2. `RaceManager.OnFinishLineCrossed` → the first crossing in this lap → `FinishLap(winner)`.
3. `RaceManager` → `tracker.FinishLap(winner)` → winner +200 × streak; `winner.lapsWon++`; `lapsPlayed++`.
4. `RaceManager` → `AudioManager.PlayEffect(lapWonClip)` → wait 1 s.
5. If `lapsPlayed < requiredLaps`: every racer → `CarController.ResetTo(racer.spawn)`; `BotDriver.ResetRoute()`; `Countdown()`.
6. Otherwise → `FinishRace()` (6.12).

### 6.12 Finish the race
1. `RaceManager` → `SetRacersInput(false)`.
2. `RaceManager` → `tracker.FinishRace(requiredLaps)` → flawless bonus.
3. `RaceManager` compares `lapsWon` → win / loss / tie → `AudioManager.PlayEffect(winClip or tieClip)`.
4. **Single only:** `GiveRewards()`:
   - `SaveService.Data.coins += 200` on a win, `+= 100` on a tie;
   - `SaveService.Data.AddRaceResult(player.points, bot.points)`;
   - `SaveService.Save()`.
5. `RaceManager` → `hud.ShowResult(title, points per racer)`.
6. Player clicks **Menu** → `RaceManager.BackToMenu()` → `LoadScene("MainMenu")`.

### 6.13 Pause
1. Player presses Esc → `PauseMenu.Update()` → `Pause()` → `Time.timeScale = 0` → shows `pausePanel`.
2. Then one of:
   - **Resume** (or Esc) → `Resume()` → `Time.timeScale = 1`;
   - **Options** → `OpenOptions()` → `OptionsPanel` (6.14);
   - **Menu** → `RaceManager.BackToMenu()` → `Time.timeScale = 1` → `LoadScene("MainMenu")`. The race result is not saved.

### 6.14 Music and options
1. Game start → `AudioManager.CreateOnStart()` → instantiates the prefab → `DontDestroyOnLoad` → reads `musicOn` and `musicVolume` from `SaveData` → plays track 1.
2. Player presses M → `AudioManager.ToggleMusic()`; N → `NextTrack()`.
3. Player moves the volume slider → `OptionsPanel.OnVolumeChanged(v)` → `AudioManager.SetVolume(v)` → `SaveData.musicVolume = v` → `SaveService.Save()`.

### 6.15 Editor: organize and validate a map
1. Developer → **Tools → Car Racing → Organize Open Scene** → `CarRacingMenu.OrganizeOpenScene()` → for each root object `GroupFor()` → moves it into its group.
2. Developer → **Validate Open Scene** → `ValidateOpenScene()` → counts `RaceManager`, `FinishLine` the route, coins and obstacles → writes the result to the Console.

---

## 7. SADT (IDEF0)

### 7.1 A-0: context
**A0 — Play a car race**

| Arrow | Contents |
|---|---|
| **Inputs** (left) | Player 1 and Player 2 keyboard input; mouse clicks in menus; saved progress (`save.json`) |
| **Controls** (top) | Game rules (laps, prizes, race point values, obstacle effects); map and car data (`MapConfig`, `CarDefinition`); key bindings (`InputSystem_Actions`); map scene rules (`SceneContract.md`) |
| **Outputs** (right) | Rendered race and menus, sound and music; race result (winner, race points); updated progress (coins, purchases, race points, best results, music settings) in `save.json` |
| **Mechanisms** (bottom) | Unity 6 engine (PhysX, Input System, uGUI/TextMeshPro, URP, Audio); C# scripts; 3D assets (track pieces, cars, decorations, coins, obstacles); the Bot (`BotDriver`); PC with keyboard |

### 7.2 A0 decomposition
| Block | Function | Inputs | Controls | Outputs | Mechanisms (classes) |
|---|---|---|---|---|---|
| **A1** | Manage profile and choose race settings | mouse clicks, `save.json` | prices (`CarDefinition`, `MapConfig`), shop rules | `GameSession` (mode, car, map), purchases, music settings, loaded map scene | `PanelNavigator`, `ModeSelector`, `CarSelector`, `MapSelector`, `ShopButton`, `CoinsLabel`, `RatingPanel`, `OptionsPanel`, `SaveService` |
| **A2** | Prepare the race | `GameSession`, map scene | `MapConfig` (speeds, laps, obstacle type), `CarDefinition` (prefab) | spawned racers, cameras, chosen obstacles, HUD, started countdown | `RaceManager`, `CarCamera`, `RaceHud`, `BotRoute` |
| **A3** | Drive the cars | keyboard input, bot route, "GO!" signal | key bindings, physics settings, max speeds | car movement and positions | `PlayerCarInput`, `BotDriver`, `CarController` |
| **A4** | Handle track events | car positions and contacts | race rules (coin value, obstacle effects, point values) | coins, obstacle effects on cars, race points changes | `Coin`, `Obstacle`, `CarController` (walls), `RaceTracker` (checkpoints) |
| **A5** | Count laps and decide the winner | finish line crossings | required laps, lap winner rule | lap results, race result, reset for next lap | `FinishLine`, `RaceManager`, `RaceTracker`, `RaceHud` |
| **A6** | Save progress | race result, points, coins, purchases, settings | prize rules, Single-only saving | `save.json`, rating | `SaveData`, `SaveService` |

**Arrows between the blocks:**
- A1 → A2: race settings (`GameSession`)
- A2 → A3: ready cars and the "GO!" signal
- A3 → A4: car positions and collisions
- A3 → A5: car positions at the finish line
- A4 → A5: race points
- A5 → A2: control arrow "next lap", which resets the cars and runs the countdown
- A5 → A6: race result
- A4 → A6: collected coins
- A6 → A1: feedback arrow with the updated coins, purchases and rating

The **pause** (`PauseMenu`) and **music** (`AudioManager`) are control and mechanism arrows across A2–A5. They don't need blocks of their own. If a deeper level is needed:
- **A3** splits into: A31 Read input (human / bot), A32 Compute speed and turn, A33 Move the car, A34 Bounce off walls.
- **A4** splits into: A41 Collect coins, A42 Apply obstacles, A43 Count checkpoints, A44 Apply penalties.
