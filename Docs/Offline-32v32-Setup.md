# Offline 32v32 Setup

This project now supports an offline Counter-Strike style match with one local player and up to 32 actors per team.

## Scene objects

Add these scene-level objects:

1. `RoundManager`
2. `OfflineMatchBootstrapper`
3. Multiple `TeamSpawnPoint` objects for both `Terrorists` and `CounterTerrorists`
4. A baked NavMesh if you want bots to path around geometry

## Local player prefab

Recommended core components:

- `CharacterController`
- `KeyboardMouseInputSource`
- `PlayerMotor`
- `FirstPersonLook`
- `Health`
- `PlayerEconomy`
- `TeamMember`
- `RoundActorLifecycle`
- `HitscanWeapon`

## Bot prefab

Recommended core components:

- `CharacterController`
- `BotInputSource`
- `PlayerMotor`
- `Health`
- `PlayerEconomy`
- `TeamMember`
- `RoundActorLifecycle`
- `HitscanWeapon`
- `BotController`
- `NavMeshAgent` for better movement

If the bot uses a separate head or weapon rig, set:

- `BotController.yawRoot` to the body root
- `BotController.pitchRoot` to the upper-body or camera pivot
- `BotController.eyePoint` to a head-level transform
- `HitscanWeapon.muzzle` to the gun barrel transform

## Bootstrap settings

Create an `OfflineMatchSettings` asset and configure:

- `Players Per Side = 32`
- `Include Local Player = true`
- `Local Player Side = CounterTerrorists` or `Terrorists`

Assign that asset to `OfflineMatchBootstrapper`, plus:

- local player prefab
- bot prefab
- round manager reference
- actor root transform if you want spawned actors grouped in hierarchy

## Notes

- If there are fewer spawn points than team size, the bootstrapper reuses them and adds radial scatter.
- Bots still work without a NavMesh, but they move much better when a NavMesh is baked.
- This is offline single-player logic only. There is no networking in this path.
