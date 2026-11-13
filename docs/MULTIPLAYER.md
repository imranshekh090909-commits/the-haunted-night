# Multiplayer architecture

## Why Relay, not raw sockets

Mobile carriers use CGNAT - a phone cannot accept inbound connections. Unity Relay gives the
host a public relay allocation, so room-code play works on 4G/5G with no port forwarding
and no dedicated server to pay for.

## Flow

```
HOST                                          CLIENT
  |-- UnityServices.InitializeAsync ----------> (same)
  |-- SignInAnonymouslyAsync ----------------> (same)
  |-- Relay.CreateAllocationAsync(3)          |
  |-- Relay.GetJoinCodeAsync  -> "K7QM2X"     |
  |-- Transport.SetRelayServerData(alloc)     |
  |-- NetworkManager.StartHost()              |
  |-- Lobby.CreateLobbyAsync(roomCode=K7QM2X) |
  |                                           |-- types K7QM2X
  |                                           |-- Relay.JoinAllocationAsync("K7QM2X")
  |                                           |-- Transport.SetRelayServerData(join)
  |                                           |-- NetworkManager.StartClient()
  |<-- ConnectionApproval (cap 4) ----------->|
```

`NetworkBootstrap.HostRoomAsync()` / `JoinRoomAsync()` implement exactly this.

## Room code rules

- Relay codes are 6 characters, uppercase, no vowels (avoids accidental words).
- Always `Trim().ToUpperInvariant()` before joining - users copy-paste with spaces.
- Code is per-session: a new host = a new code. Store it in the Lobby's `Data["roomCode"]`
  if you want friends to find the room later without typing.

## Authority model

| Thing | Authority |
|---|---|
| Movement, look, flashlight toggle | owner (client) |
| Health, death, keys, fuses, escape | server (host) |
| Enemy AI, spawning, jump scares | server |
| Room cap, approval | server |

Client-authoritative movement is standard for co-op horror (cheating is rare, and
rubber-banding ruins the feel). Everything that changes match outcome is server-side.

## Voice chat

Vivox group channel per room (`channelName = "room_" + roomCode`), 3D position updates every
frame from `VoiceChatManager.UpdateLocalPosition`. Free tier is fine for testing;
production needs a paid Vivox plan sized for concurrent users.

## Latency and low-end tuning

- Tick rate 30Hz (`NetworkManager.NetworkConfig.TickRate = 30`), interpolation ON.
- `NetworkTransform` on players and enemies only; props are static.
- Cap monsters at 3 on Low tier - AI is the biggest server cost.
- Never send per-frame RPCs for footsteps; use `EnemyAI.EmitNoise` (server-side only).

## Testing without a second phone

Install **Unity ParrelSync** (free, Asset Store) to open two editor instances of the same
project, host in one and join with the room code in the other.
