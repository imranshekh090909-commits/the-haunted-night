# Building the APK

## 1. Prerequisites

- Unity **2022.3.40f1** with *Android Build Support* (Android SDK & NDK Tools, OpenJDK) ticked in Unity Hub.
- A phone on Android 8.0 (API 26) or newer, 3GB RAM recommended.
- Internet once, to resolve packages and Unity Gaming Services.

## 2. Player Settings (Edit > Project Settings > Player)

| Setting | Value |
|---|---|
| Company Name | your studio |
| Product Name | The Haunted Night |
| Package Name | `com.yourstudio.hauntednight` |
| Version | 0.1.0 |
| Default Orientation | Landscape Left |
| Minimum API Level | Android 8.0 (API 26) |
| Target API Level | Automatic (highest installed) |
| Scripting Backend | **IL2CPP** (required by Android, and faster) |
| Target Architectures | **ARM64 only** (drops ARMv7 - halves APK size, covers 99% of active phones) |
| Api Compatibility Level | .NET Standard 2.1 |
| Graphics API | **Vulkan** first, OpenGLES3 as fallback |
| Texture Compression | **ASTC** (best quality/size on modern Mali/Adreno) |
| Strip Engine Code | ON |
| Managed Stripping Level | Medium |
| Internet Access | Require |
| Active Input Handling | Both (Input System package is used by UI) |

Quality settings: create 3 levels (Low / Mid / High) matching `LowEndOptimizer.Apply()`.
URP asset: enable SRP Batcher, disable HDR on Low, shadow distance 18-32m.

## 3. Keystore (do this once, keep it safe)

`Player Settings > Publishing Settings > Keystore Manager > Create New`

- Alias: `hauntednight`
- Password: store it in a password manager, **never in this repo**
- Validity: 50 years

A lost keystore means you can never update the app on Play Store. Back it up offline.

## 4. Build

`File > Build Settings > Android > Build` -> produces `TheHauntedNight.apk`.

Command line equivalent (CI):

```bash
Unity -quit -batchmode -projectPath . \
  -executeMethod BuildScript.BuildAndroid \
  -logFile build.log
```

Add `Assets/Editor/BuildScript.cs` (sample below) if you want CI builds:

```csharp
public static class BuildScript
{
    public static void BuildAndroid()
    {
        UnityEditor.BuildPlayerOptions options = new()
        {
            target = UnityEditor.BuildTarget.Android,
            locationPathName = "Builds/TheHauntedNight.apk",
            options = UnityEditor.BuildOptions.None
        };
        UnityEditor.BuildPipeline.BuildPlayer(options);
    }
}
```

## 5. APK vs AAB

- **APK** (side-loading, itch.io, direct download): keep everything in one file. Must stay under 500MB; the Play Store limit for APK is 100MB, so only use APK for direct distribution.
- **AAB** (Play Store): 200MB base + Play Asset Delivery for the rest. If you plan Play Store, ship an AAB and use asset packs for the big map textures.

## 6. Size control checklist

1. Textures: max 2048px, ASTC 6x6 for environment, 4x4 for characters. No 4K textures.
2. Audio: music as Vorbis 128kbps streaming, SFX as ADPCM short clips.
3. Meshes: LOD groups on everything over 5k triangles; lightmaps baked, not realtime.
4. Remove unused engine modules (Strip Engine Code ON) and unused packages.
5. Compress build: `Build Settings > Compression Method = LZ4HC`.
6. Measure with `Build Report` window or the `build.log` size summary after each build.

## 7. First device test

```bash
adb install -r Builds/TheHauntedNight.apk
adb logcat -s Unity
```

Host on one phone, join on another with the 6-letter room code. Both phones need internet
(Relay is a cloud service; LAN-only play needs a different transport).
