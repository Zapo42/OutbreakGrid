# Retained scope register

This register carries the established project into the rebuild. Only the foundation behavior identified in the README is implemented in this package. Everything else below remains a requirement, not a claim about this stage.

## Platform, scale and time

- Unity/C#, Windows EXE first. Retain Android as a later platform request; it is not part of this foundation's player adapter.
- Observational top-down grid simulation: Pre-Encounter → Encounter → Post-Encounter survivors.
- Default 5 TPS, a 0.20-second logical tick, and one in-game hour per two real minutes. Half/Normal/Double/Triple and step ten ticks. Pause governs all systems and visuals.
- Target about 1,000 active humans plus zombies combined, with one starting zombie. Keep larger population experiments separate from changing the default cap.
- The 100×100 prototype is a test arena, not the permanent simulation dimension. Retain the 1,000×1,000-cell simulation target and 100/250/500/1,000 dimension ladder. Workshop authoring must support up to 10,000×10,000 through sparse/chunked storage.
- Use small named authored scenarios for behavior acceptance. Preserve existing art, projects, palettes, brushes, stamps and layouts. Keep code commented in plain English.

## People, perception and decisions

- Seven SPECIAL statistics: Strength, Perception, Endurance, Charisma, Intelligence, Agility, Luck. Natural values 0–5; modifiers may reach 10. Manual spawning warns above 5 and rejects above 10.
- Weighted natural draws: 0=1%, 1=7%, 2=22%, 3=40%, 4=22%, 5=8%. Total natural points: 17=5%, 18=89%, 19=5%, 20=1%. Generation must satisfy both the total and a deliberately validated distribution, not silently assume independent draws will do so.
- Keep Morality and Bravery separate. Bravery: `SP=(STR+PER)/2`; `Base=round((SP−3)×1.5)`; `Final=clamp(Base+(CHA−3),−3,+3)`. Foundation uses midpoint rounding away from zero; this makes the previously unspecified tie behavior explicit. The recovered source permits Morality −5…+5, retained in the new save boundary until the trait system is migrated.
- Group support combines −1…+3 contributions; cowards subtract at most one, brave people add up to three, and departures are not noticed instantly.
- Courage Margin = individual bravery + group support − stress penalty − threat pressure. ≥+1 Fight; 0 or −1 Freeze; ≤−2 Flight. Freeze has variable duration and resolves into Fight or Flight.
- Stress: Normal, Aware, Cautious, Alarmed, Afraid, Panicked; hiding provides recovery. Aware watches/backs away before automatic fleeing. Witnesses face a threat while retreating. Knowledge and witnessed behavior change recognition and response.
- Knowledge begins with small 0.01 gains, doubles with repeated discovery, and caps at 10.00. INT affects first discovery; PER+INT affects personal recognition. Witnessing attack behavior grants knowledge of that behavior; warnings propagate through actual communication/perception.
- Vision cones, sensory bubbles, walls, doors, windows and room bounds determine knowledge. Indoor connected rooms extend awareness while walls block LOS. Exact outdoor distances/angles must be reconciled against the latest accepted revisions during perception migration rather than copying old pixel constants into logical cells.
- Four humans per ordinary cell; up to eight total bodies for attack crowding. Humans cannot walk through an unstaggered zombie. Diagonal movement must not cut solid corners.
- Flight compares reachable buildings and groups using projected safety minus travel risk, with destination stickiness 0.75. Look for unlocked doors, prefer shelter, break LOS when panicked, and avoid pointless corner piles.
- Hiding chooses interior space with few openings and comfortable capacity, away from doors/windows and their sightlines. Recover, inspect openings, and confirm clearance before resuming routine.

## Zombies and infection

- Very durable against unarmed attacks. Human-derived SPECIAL has strain changes: STR decays after successful grabs/lunges by an END-dependent percentage; PER decays slowly; END is inherited; CHA/INT become zero; Luck stays; STR/PER floor at one.
- Zombie movement distribution: 90% speed 1, 8% speed 2, 2% speed 3, with 40% diagonal chance. Calibrate the exact speed unit during movement migration.
- Catatonic/Roaming idle behavior. Hordes form at six within a 5×5 area, choose highest aggression as leader, let followers catch up and abandon a stuck leader after 15 seconds.
- Use sight/sound/occlusion, shortest reachable paths, last-known locations and closer visible target switching. Retain toned-down lunges and room/exit tracking.
- Multiple zombies can grab one human. More grabbers reduce escape odds and raise bite odds; nearby human support resists grabs. Escape strongly favors staggering the zombie. A staggered human can be piled on, extending stagger.
- Bites cause END-dependent delayed turning. Infected humans retain their weapons and identity until turning, slow according to bite damage, and do not transmit before turning by default. Wildfire remains an optional pre-turn transmission trait.
- Infected pale-green display is a debug option, initially off. Zombies use dark green-brown. Follow/selection persists through conversion.
- Retain exceptional fictional opt-out and mercy-kill behavior under the previously agreed rare infected-character conditions; do not make it the default response.

## Combat and equipment

- Hands shove; shove can stagger. Detection/evasion uses PER+AGI and Luck can aid counters. Preemptive attacks use PER, knowledge and bravery. Successful escape and observed escapes improve future avoidance.
- Separate Blunt, Bladed, Short and Long skills. Damage uses STR+skill. Accuracy uses STR/PER/AGI+skill. Stress can increase indiscriminate firing; high INT discourages unsafe indoor/friendly-line shots.
- Visible bullet pixel and correct muzzle origin. Preserve the increased projectile speed, then calibrate units explicitly in the new movement model.
- Handgun: 20% penetration; a secondary target takes 30% damage with 5% secondary crit. Failed penetration gives minor knockback.
- Shotgun: five shells, five independently scattered projectiles, wider cone and a minimum cone width. Revolver: six, slower and somewhat stronger. Glock: fifteen, somewhat quicker. Long rifle: ten, accurate/powerful/slow. AR: thirty, full auto, Glock-class damage.
- Bats/blunt weapons can stagger, knock back and cripple. Blades slash/stab for more damage with little knockback. Bind each weapon's idle/movement/attack/death states to the actual supplied directional art.
- Numeric damage, cadence, decay, terrain speed and chance constants not explicitly fixed here require a versioned rule definition and scenario calibration; do not turn qualitative requests into undocumented permanent numbers.

## Buildings, population and normal life

- Tile → Room → Building → Scenario, with separate logical footprints, artwork and metadata. Door/window state affects collision, senses, condition and shelter. Door 100 HP, window 10 HP; both lockable. Prefer a viable door to breaking glass. Broken glass emits real sound.
- Condition 0…5: doors weight two, windows one, scaled by remaining HP and lock protection. Shelter also accounts for occupancy. Comfortable capacity defaults to one person per four usable interior cells; aggregate room/building occupancy ratios.
- Buildings own population profiles: Residents, Workers, Visitors/Consumers, Special. Gun store one clerk; grocery/pharmacy/warehouse two each. LEOs from police barracks. Small/large DC: four/eight couriers and two/four terminals.
- Homes, work schedules and real routeable doors precede needs/economy. Consumer shopping uses hunger >99. Civilian home storage 70: Food20, Medical10, Construction20, Ammo20. Retain theft and PER-vs-AGI apprehension.
- Food consumption baseline ten seconds modified by STR, one second per point from three. Medicine consumption baseline ten seconds modified by PER the same way. Internal health 0…100 displays as 0.0…10.0. One Medical heals 15 internal. Medic healing scales with PER: about 3.0 displayed at PER3 and at least 1.6 at PER0. Non-medics can heal self/others normally.
- Stores reorder at computers; clerks restock from backroom; DC workers pick and deliver. Preserve the courier clue to the hidden/unlabeled laboratory. LEOs have barracks standby, patrol, apprehension and alert relay.

## Laboratory and emission

- Preserve automatic research's exact approved family/confirmation/mutation/reservation rules by extracting a migration specification and fixtures from the recovered current implementation. Precursor identity cannot switch within its allowed solvent permutations: for precursor1 and parts2/3, allow123 or132, not a permutation that moves1. Do not re-create the whole research design from an abbreviated memory.
- Three linked station roles: precursor, mixing/centrifuge and computer; Scientists own actual tasks and private research knowledge; shared reservations prevent duplicate submissions. Keep bounded diagnostic history separate from authoritative discoveries/materials/experiments.
- Virus families G/T/X/Y/Z with three-digit identities; ordered parts carry identity/position, even where visible letters repeat. Each human keeps the latest four received pieces. A matching latest-three sequence infects, with a leading fourth part retained as modifier.
- Place emitters with radius, interval, enable state and rotating ordered recipes. Support single pulses, dual-then-single, quantities and per-part percentage random emission. Preserve recipe settings while switching modes.
- Debug toggles show a hazy green range and next-pulse letters drifting away for the interval. Computer tube colors and screen letters are modular overlays, not a factorial collection of full tables. Keep letter sizes inside the screen.

## Survivor world

- Scavenger, Loner, Shelterer, Hoarder, Raider. Raiders slightly most common; Shelterers close; Scavengers and Hoarders middle; Loners least common. Preserve their distinct risk/fight choices.
- Claim only a cleared and verified base; use build radius and storage bonuses. Shelterer/Raider may expand to four adjacent structures; Hoarder one cache/base; Scavenger one small base; Loner exactly two fallbacks.
- Settled daily planning evaluates household needs together: scavenge, repair/build, guard/patrol, with guard as idle fallback. Scavenger priorities Food > Medical > Ammo > Supplies > Fuel and two-cell-per-person temporary comfort; vehicles opportunistic. Raiders use vehicles for staging and remain cautious about overwhelming hordes.
- Base defense uses Perimeter Patrol and Sector Patrol, with temporary observation stops. No default permanent stationary guard-post subsystem.
- Alerts travel as real shouting/sound, giving nearby allies approximate incident knowledge. Raiders may use a safe-direction warning shot, with its normal attraction/revelation cost. Do not instantly inform an entire faction.
- Further class-specific supply weights remain future design; do not invent them as already agreed.

## Workshop, art and product controls

- DragonGlassPixelWorkshop branding, obsidian-glass appearance/tint, optional modern glass, pixel title, preloaded art and Open Workshop flow. Preserve user assets, palettes, brushes, projects and workspace layouts.
- Shared authoring operations and catalogs between apps, with separate standalone building/sprite workflows. Simulator and Workshop must render the same terrain and metadata; a shared format is not a claim that their whole interfaces are identical.
- Visible **Load Map** must open a real navigable file browser in the Windows player. Support legacy `.obmap`, `.dgproject` Map and `.obworld` through explicit tested migration, retaining source art/metadata and avoiding silent cropping/resizing.
- New/Open/Save/Save As/Undo/Redo/Project Settings/Canvas Background. Define name, dimensions and save location when creating. Center and fit canvases on open. Background swatch/wheel, white/dark/checker choices and per-workspace persistence.
- Dockable/resizable/movable/pinnable tools and panels, saved layouts, hover explanations and optional tutorial actions. Project/layer controls retain words where readable; drawing tools use clear icons and active states.
- Pointer with optional snap-back; square/circle brushes and sizes; line/rectangle/ellipse with hold-to-select outline/fill; bucket; tolerance magic select; eraser; true-pixel eyedropper; brush-sized color replacement; rectangle/lasso selection; copy/paste/deselect; move/rotate/flip/warp; footprint previews. Metadata supports paint tools too.
- Named layers, clear active layer, opacity, reorder, multiselect and flatten. Paste goes to the currently selected target layer. Selected-pixel viewer and Define Selection; no redundant duplicate selection toolbar.
- Bright 24-swatch default palette, custom persistent brushes. Animator bottom timeline, visible blank thumbnails, onion skin and playback. Building editor grid, pan/zoom, large canvases, outline, doors/windows, independent asset saving.
- Five grass variants, light/medium/heavy terrain, sidewalk width two, crosswalk, fence, water, open/closed gate, and custom tile types with artwork/passability/variables. Preserve art rather than substituting solid-color cells in the finished editor.
- Stock human clothing variants, green zombie head/hands, correct unarmed and weapon hands, non-career NPCs, Scientists with directional work/death/shove, shelves searched/unsearched/searching, modular lab stations/centrifuge, storage box, gas pump, counter/emergency phone, buildings and vehicles.
- Scenario menu chooses named map, starting counts, caps and spawn regions. Manual spawner configures species/stats or randomizes, then places on next click with repeat toggle and cap enforcement.
- Zoom buttons/wheel, free/follow camera, draggable minimap and camera navigation. Human/zombie minimap markers initially hidden. Stat panel visibility/pinning, debug vision/senses/infected toggles and smooth soft-edged distant population masses colored by local average human stress.
