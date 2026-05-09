# Advanced Counter-Strike Style Clone Roadmap

This repo started from the supplied GDD and now has a runtime foundation for:

- Character-controller movement with walk, run, crouch, jump and movement penalty hooks
- First-person look with recoil injection
- Hitscan weapons with recoil pattern support and simple wall penetration
- Surface-based footsteps and penetration resistance
- Health, damage, economy rewards, team membership and round states
- Bomb site and bomb state scaffolding

## Recommended build order

1. Build a graybox map with one mid lane, two bomb sites and clear spawn timings.
2. Wire a player prefab:
   - `CharacterController`
   - `PlayerMotor`
   - `FirstPersonLook`
   - `Health`
   - `PlayerEconomy`
   - `TeamMember`
   - `FootstepAudioController`
3. Create ScriptableObjects for:
   - `PlayerMovementSettings`
   - `WeaponDefinition`
   - `RecoilPattern`
   - `EconomyTable`
   - `RoundSettings`
4. Add a weapon prefab with `HitscanWeapon` and connect camera, muzzle and player references.
5. Add `SurfaceMaterial` to level geometry so footsteps and wallbang rules are data-driven.
6. Place `RoundManager`, `BombObjective` and `BombSite` objects in the scene.

## What "advanced" should mean for this clone

- Server authoritative multiplayer with lag compensation
- Deterministic recoil patterns per weapon
- Proper buy phase, armor, helmets, defuse kits and weapon pickups
- Damage zones, penetration tuning per surface and anti-wallhack visibility checks
- Audio propagation for footsteps and gunshots
- Match flow: freeze time, live round, planted bomb, defuse, end-round reset
- Spectator mode, kill feed, scoreboard and round economy UI

## Networking recommendation

The GDD mentions Photon Fusion or Mirror. For a Counter-Strike style game, prefer server authority and rollback-friendly hit validation:

- Photon Fusion if you want hosted sessions plus modern prediction tools
- Mirror if you want simpler open-source control and are comfortable building more systems yourself

## Next high-value milestones

1. Install one networking stack and convert `RoundManager`, `Health` and `HitscanWeapon` to server-authoritative flows.
2. Add a buy menu and inventory model.
3. Add map graybox plus nav blockers, audio zones and bomb-site triggers.
4. Replace the current penetration approximation with collider thickness sampling and material tables.
