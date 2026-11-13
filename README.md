# THE HAUNTED NIGHT

Realistic-ish 3D multiplayer horror survival for Android, 1-4 players with room-code matchmaking.
This repository is the **Unity 2022.3 LTS project scaffold**: all gameplay, networking,
AI, UI and optimization systems are written. Art, audio and level geometry are
plug-in work (see `docs/ASSETS_AND_SIZE_BUDGET.md`), because a 500MB horror game is
mostly bought or authored assets, not code.

> **No .apk is committed here.** An APK can only be produced by Unity's Android build
> pipeline on your machine (or a CI runner) - see `docs/BUILD_APK.md`.

## What is implemented

| Feature | Where |
|---|---|
| Room-code multiplayer (1-4), Unity Relay + Lobby | `Assets/Scripts/Networking/NetworkBootstrap.cs` |
| Connection approval / room cap | `Assets/Scripts/Networking/ConnectionApproval.cs` |
| Match flow: lobby -> mission -> escape / wipe | `Assets/Scripts/Core/GameManager.cs` |
| Keys, fuse box, escape gate | `Assets/Scripts/Objectives/*`, `Assets/Scripts/Interactables/*` |
| First-person mobile controller (stick + look pad) | `Assets/Scripts/Player/PlayerController.cs` |
| Flashlight with battery + blinding enemies | `Assets/Scripts/Player/Flashlight.cs` |
| Interaction system (keys, doors, hiding spots) | `Assets/Scripts/Player/PlayerInteractor.cs` |
| Hiding in cupboards / under beds | `Assets/Scripts/Interactables/HidingSpot.cs` |
| Monster AI: Ghost (phasing) + Zombie (bursts) | `Assets/Scripts/AI/*` |
| Health, downed state, revive | `Assets/Scripts/Player/PlayerHealth.cs` |
| HUD, main menu, settings, character select | `Assets/Scripts/UI/*` |
| Proximity voice chat (Vivox) | `Assets/Scripts/Voice/VoiceChatManager.cs` |
| Low-end auto graphics tier | `Assets/Scripts/Perf/LowEndOptimizer.cs` |

## Quick start

1. Install **Unity 2022.3.40f1** with Android Build Support (SDK + NDK + OpenJDK).
2. Open this folder as a Unity project. Let it resolve packages (`Packages/manifest.json`).
3. Link the project to Unity Gaming Services (Edit > Project Settings > Services):
   enable **Authentication**, **Relay**, **Lobby** and **Vivox**.
4. Build the scenes per `docs/SCENE_SETUP.md` (MainMenu + two map scenes).
5. Follow `docs/BUILD_APK.md` for the Android player settings and APK export.

## Docs

- `docs/GAME_DESIGN.md` - full feature spec, maps, monsters, progression
- `docs/SCENE_SETUP.md` - exact hierarchy + inspector wiring, step by step
- `docs/MULTIPLAYER.md` - relay/lobby architecture, room codes, cheating model
- `docs/BUILD_APK.md` - Android settings, keystore, APK vs AAB, size control
- `docs/ASSETS_AND_SIZE_BUDGET.md` - asset sources, 500MB budget breakdown, costs
- `docs/ROADMAP.md` - what is left to reach a shippable build

## Status

Code scaffold: complete. Art/audio/levels: not included. Playtesting: needed.
