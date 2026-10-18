> Prototype 5 adds an 81-cell local escape query and fixed-capacity observation buffers. Bitten humans override ordinary courage decisions; short-lived last-observed threat coordinates are saved and cleared on expiry/turning. No hidden actor positions or unbounded query history are retained. See INFECTED_ESCAPE.md.

> Prototype 4 keeps bounded MovementPresentation segments in the Unity adapter, separate from saved actor positions. Ordinary moves blend over five ticks; unsafe compound transitions synchronize instead of cutting corners. Old prototype-three runs can explicitly upgrade rules without changing saved state. See MOVEMENT_UPDATE.md.

> Prototype 3 adds persisted human reactions and a separate named random stream for freeze duration. All deadlines use World.Step; reaction state clears on turning. The menu pauses and retains the existing world. See HUMAN_REACTIONS.md.

> Encounter prototype 2 extends the baseline below with opt-in pursuit memory, local sight/path queries and deadline-based infection. See ENCOUNTER_PROTOTYPE.md. Legacy rules remain available for exact save continuation.

# Foundation boundaries

| Layer | Owns | Allowed dependencies |
| --- | --- | --- |
| Contracts | Versioned map/save records, stable terrain/species identities and actor value records | C# base libraries |
| Core | Authoritative map, actors, random stream, occupancy, rules and tick scheduler | Contracts |
| Persistence | JSON encoding, candidate validation, bounded reads and atomic publication | Contracts, Core |
| Unity adapter | Screen, camera, interpolation, textures and operating-system file dialogs | Contracts, Core, Persistence, Unity |
| Editor adapter | Entry-scene preparation and Windows build action | Unity editor APIs and Unity adapter |

The first three assembly definitions disable engine references. This prevents camera or GameObject code from creeping into the simulation. Unity's assembly definition documentation describes that boundary: <https://docs.unity3d.com/6000.0/Documentation/Manual/assembly-definition-file-format.html>.

`World.Step()` is the sole authoritative time entry point. A frame may execute zero or several complete ticks. A work budget bounds catch-up per frame and retains excess tick debt rather than advancing the clock without executing systems. An explicit pause boundary clears fractional scheduling time; paused elapsed real time is excluded. Future cooldowns, needs, projectiles, infection and research must all use this clock.

The initial per-body array uses value records in stable identity order. Renderers receive copies. Authoring terrain is cloned into a running world, and snapshots clone arrays. The occupancy index drops empty buckets, so travel does not leave an ever-growing history of visited cells. No tick history is retained in the running world.

The current movement fixture is intentionally simple: every body gets one move opportunity each five ticks, staggered by identity, and chooses one of eight neighbors or idle. This is a diagnostic behavior, not the agreed final movement model. Terrain identities and solid collision are present; path costs, speed modifiers, doors, durability, water exhaustion and dynamic zombie blocking are not yet implemented.

The random algorithm is explicitly defined in production source and its current 32-bit state is saved. Rendering performs no random draws. Future systems must use named, independently captured streams when their order should not affect unrelated systems. Adding a system or changing draw order requires a new `rulesVersion`; old running saves must migrate explicitly or reject clearly.

Maps are sparse, row-major 32×32 terrain chunks with zero-valued ordinary grass as default. Capture sorts chunk coordinates before serialization. File coordinates are logical cells; sprite dimensions will remain separate. The opaque `authoringJson` field survives save/load byte-for-byte as a string, but it is not yet a functional building, household or laboratory model. It is a preservation boundary for the next migration stage.

Map and save loads allocate a candidate and validate the entire envelope before the caller swaps worlds. Unknown versions and old formats reject. The first slice does not silently reinterpret old numeric tile IDs or invent defaults for missing required records. The 32 MiB input/output limit is explicit; larger worlds need a separately designed chunked storage path before claiming full support.

The Unity shell builds a small texture of the viewed terrain, bounded to 256 cells per axis. It culls offscreen bodies, owns no per-body GameObjects, and interpolates committed positions. Its immediate-mode GUI is temporary. Native dialog execution and actual rendering remain unverified until Unity/Windows acceptance.

GUI commands execute outside their OnGUI traversal. In the Editor, a temporary editor-update subscription can drain an accepted command even while Unity's host pause prevents MonoBehaviour.Update. Player Update uses the same exactly-once pump. The component releases its subscription on execution or destruction. Startup scene preparation is explicit, and the diagnostic menu reads live counters without Unity Search. These dispatch safeguards are separately checked; they do not establish successful native mouse delivery or repair Unity's reported startup-indexing exception.

Future pathfinding uses revisioned terrain/collision input, bounded queries and reusable buffers. Hierarchical routing, perception buckets, simulation LOD, Jobs/Burst and entity removal/compaction are design work for the stage that demonstrates their need. This package does not claim those systems exist.
