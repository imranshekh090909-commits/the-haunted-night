# Scene setup - copy this exactly

Unity cannot ship a scene from text, so build three scenes by hand. Each step is small.

## Scene 1: `MainMenu`

Hierarchy:

```
MainMenu
├── Systems
│   ├── NetworkManager            (NetworkManager + UnityTransport + ConnectionApproval)
│   └── NetworkBootstrap          (NetworkBootstrap)
├── EventSystem                   (Input System UI module)
├── Canvas (Screen Space - Overlay, CanvasScaler: Scale With Screen Size 1920x1080)
│   ├── MainPanel                 (title art, PLAY, JOIN, PRACTICE, SETTINGS, QUIT)
│   ├── HostPanel                 (name field, CREATE ROOM button, roomCodeLabel)
│   ├── JoinPanel                 (name field, roomCodeField, JOIN button)
│   ├── SettingsPanel             (sensitivity, volumes, quality dropdown, vibration)
│   └── LoadingPanel              (spinner + "Connecting...")
└── AudioManager                  (AudioManager, menu music clip)
```

Wiring:

1. `MainMenuUI` on `Canvas`, drag every panel + field into its slot, set `lobbyScene = Map_HauntedHouse`.
2. Buttons: `OnClick` -> `OnPlayPressed`, `OnJoinPressed`, `OnCreateRoomPressed`, `OnJoinRoomPressed`, `OnPracticePressed`, `OnQuitPressed`, `OnBackPressed`.
3. `NetworkManager`: add `UnityTransport`, then `ConnectionApproval` component.
4. Add both scenes to `File > Build Settings` and put `MainMenu` at index 0.

## Scene 2: `Map_HauntedHouse`

```
Map_HauntedHouse
├── Systems
│   ├── GameManager               (GameManager - prefab, DontDestroyOnLoad)
│   ├── MissionManager            (MissionManager, escapeZone -> EscapeGate)
│   ├── PlayerSpawner             (PlayerSpawner, playerPrefab, 4 spawn points)
│   ├── EnemySpawner              (EnemySpawner, Ghost + Zombie prefabs, 6 spawn points)
│   ├── HudController             (HudController + Canvas)
│   ├── TouchControls             (full-screen touch area + buttons)
│   └── LowEndOptimizer
├── Environment
│   ├── HouseShell                (walls, floors, baked lightmaps)
│   ├── NavMeshSurface             (bake here after building geometry)
│   ├── PatrolPoints              (empty transforms, 10-14 of them)
│   ├── SpawnPoints               (4 player, 6 enemy)
│   └── PhaseAnchors              (ghost teleport targets, 6-8)
├── Objectives
│   ├── Key_01 / Key_02 / Key_03  (KeyPickup prefabs)
│   ├── FuseBox                   (FuseBox)
│   ├── PuzzleSwitches            (PuzzleButton x4, correctOrder 2-0-3-1)
│   └── EscapeGate                (EscapeZone + trigger collider)
├── HidingSpots
│   ├── Cupboard_01..06           (HidingSpot, cameraAnchor inside)
│   └── Bed_01..03                (HidingSpot, cameraAnchor under)
├── Scares
│   └── Scare_01..05              (JumpScareTrigger, trigger collider)
└── Lighting                      (baked mixed lights, fog on, ambient near-black)
```

Player prefab (`Player`) - put this in `Assets/Prefabs`:

```
Player                              NetworkObject, CharacterController, PlayerController,
│                                   PlayerHealth, PlayerInteractor, Flashlight, AudioSource
├── CameraPivot                     (assign to PlayerController.cameraPivot)
│   └── Main Camera                 (assign to HudController + PlayerInteractor.rayOrigin)
├── FlashlightLight                 (Light: Spot, range 22, angle 55, intensity 2.4)
└── Body                            (mesh + Animator for remote players)
```

Assign `playerPrefab` on `PlayerSpawner`, and drag the prefab into `NetworkManager > Network Prefabs`
(along with enemy prefabs, key prefab, fuse prefab).

Bake the NavMesh: select `NavMeshSurface` -> Bake. Set agent radius 0.4, height 1.8.

## Scene 3: `Map_AbandonedHospital`

Same structure, different geometry, `requiredKeys = 4`, longer corridors, more zombies
(`baseCount = 3`), flickering fluorescent lights instead of candles.

## Layer setup

| Layer | Used for |
|---|---|
| `Player` | player bodies |
| `Enemy` | monsters (flashlight raycast) |
| `Interactable` | keys, doors, hiding spots (set `interactMask` to this) |
| `Environment` | walls/floors (`sightBlockers` = Environment + Interactable) |

## Fast smoke test (Editor)

1. Press Play in `Map_HauntedHouse` with `NetworkManager` present -> `Start Host`.
2. Walk with WASD (add a tiny editor-only input fallback), press E to interact.
3. Confirm: key pickup increments the HUD counter, a monster chases on noise,
   hiding stops the chase, the escape gate only fires with 3 keys + power.
