# Art, audio and the 500MB budget

The code is free to write. A *realistic* 3D horror game is mostly assets, and those
either cost money or months of work. This is the honest breakdown.

## Budget split for a ~450MB APK (headroom for updates)

| Chunk | Budget | Notes |
|---|---|---|
| Environment textures + lightmaps | 150 MB | 2 baked maps, 2048px ASTC 6x6 |
| Character / monster meshes + textures | 60 MB | 4 survivors, 4 monsters, 4k-15k tris each |
| Props (furniture, doors, debris) | 40 MB | heavy atlas use, one 4K atlas per room set |
| Audio (music, ambience, SFX, VO) | 90 MB | Vorbis streaming music, ADPCM SFX |
| Animation clips | 25 MB | generic humanoid + monster rigs |
| UI art + fonts | 15 MB | one display font, one body font |
| Shaders / URP / engine code | 45 MB | IL2CPP ARM64 build |
| Code, scenes, data | 10 MB | this repo |
| Free space / patch headroom | 15 MB | |

## Where to get assets (realistic 3D, usable licence)

**Paid (fastest route, recommended):**
- Unity Asset Store - "Abandoned Hospital", "Horror Mansion Interior", "Realistic Zombie Pack".
  Typical 40-120 USD per pack; 3-4 packs cover both maps.
- Quixel Megascans / Fab - free with an Epic account for many surfaces, high quality PBR.

**Free (needs more assembly time):**
- Poly Haven (CC0 HDRIs, textures), ambientCG (CC0 PBR materials).
- Mixamo (free rigged humans + animations, needs a free Adobe account).
- Kenney / OpenGameArt for props and UI (CC0), lower fidelity.
- Freesound (CC0 / CC-BY) for ambience and stingers - check each licence.

**Must be licensed for commercial release:** every music track, every voice line,
every texture pack. Keep a `LICENSES.md` with source URL + licence per asset.

## What still has to be made by hand

1. Two playable maps with good flow (this is the real work - 3-6 weeks for one person).
2. Baked lighting passes per map (dark, cinematic, readable - the hardest art task in horror).
3. Monster animation sets: idle, patrol walk, run, attack, stun, death.
4. UI art in your visual identity (title, buttons, HUD icons, loading screens).
5. Audio mix pass (ducking, reverb zones, jump-scare timing).

## Realistic production options

| Route | Time | Cost |
|---|---|---|
| Solo dev, free assets | 3-5 months | ~0 (your time) |
| Solo dev, bought packs | 2-3 months | 200-600 USD |
| Small team (artist + dev) | 6-10 weeks | 3,000-8,000 USD |

A 500MB realistic multiplayer horror game is a 2-4 month project for one experienced
Unity developer, not a single-session build.
