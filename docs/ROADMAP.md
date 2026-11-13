# Roadmap to a shippable APK

## Done in this repo
- [x] Networking: Relay + Lobby room codes, approval, 4-player cap
- [x] Match flow, keys, fuses, escape gate, win/wipe
- [x] Mobile FPS controller, flashlight, interaction, hiding spots
- [x] Ghost + Zombie AI with sight, hearing, stun, patrol
- [x] HUD, main menu, settings, character select data model
- [x] Proximity voice chat integration (Vivox)
- [x] Low-end auto graphics tiering, audio pooling, object pool

## Next (in order)
1. Build the three scenes per `docs/SCENE_SETUP.md`, bake NavMesh + lighting.
2. Import environment/character packs, wire prefabs to `PlayerSpawner` / `EnemySpawner`.
3. Playtest on two real phones (host + client), tune enemy speeds and noise radii.
4. Add revive interaction on downed teammates (currently auto-bleed-out only).
5. Add a lobby screen showing the room code, player list and map vote.
6. Add save-data for unlocked characters, and a cosmetic-only shop if desired.
7. Audio pass: reverb zones, dynamic music layers, 20+ stingers.
8. Performance pass on a 2GB phone: target 30fps stable, check with `adb shell dumpsys gfxinfo`.
9. Sign with a keystore, build APK + AAB, test install on 3 device classes.
10. Store assets: icon, 8 screenshots, feature graphic, privacy policy (voice chat + data).

## Known gaps
- No offline bot AI for practice mode (practice currently just loads the map solo).
- No anti-cheat beyond server authority.
- No dedicated-server mode; host disconnecting ends the match.
- Voice chat needs Unity Gaming Services billing for production scale.
