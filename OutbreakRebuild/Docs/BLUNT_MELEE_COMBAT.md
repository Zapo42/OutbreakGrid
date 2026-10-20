# Foundation 44: blunt melee combat

Humans can pick up a finite ground bat and use it for contact defense when their existing courage decision is Fight. A bat adds real zombie damage, timed swings, accuracy, stagger and collision-safe knockback. It does not turn a fleeing, frozen or infected human into an attacker. Unarmed defense remains a nonlethal shove.

## Try it

1. Choose **Inspect Blunt Combat Paused**.
2. Click **Step 1 tick**. The human picks up the bat at its feet, consuming that action. Select the human to see **Melee weapon: Bat** and its blunt skill.
3. Click **Step 50 ticks**. In this controlled scene, the human has landed three strikes and the zombie is defeated. An unselected defeated body is grey; selecting it shows **DEFEATED** and health **0.0**.
4. To arm a human in another current encounter, select an unarmed, uninfected human and click **Place bat at feet**. This pauses and authors one ground pickup; the human still has to spend an action collecting it when safe.

The demonstration uses a brave human with STR 10, PER 8, AGI 8 and blunt skill 5, backed into a narrow corridor. It demonstrates a successful defense rather than typical civilian performance. Ground bats and equipped bats use brown debug markers; directional weapon/death artwork is not bound yet.

## Provisional versioned rules

- Pickup is allowed at the human's cell or a visible cardinal neighbor. There is no long-distance weapon seeking yet. Humans do not stop to collect a bat with a visible zombie within two cells, during Flight, while frozen or after infection.
- Only an existing Fight decision at contact range can swing. Swings against a visible zombie within one cell use `8 + 2 × STR + 2 × blunt skill` internal damage. Health remains internally 0–100 and displays as 0.0–10.0.
- Hit chance is `55 + 2 × (STR + PER + AGI) + 3 × blunt skill − 2 × target AGI − stress / 5`, clamped to 10–95 percent. The saved reaction random stream determines hits. A miss consumes the action and the same cooldown.
- Swing cooldown is ten ticks. A successful nonfatal hit staggers for three ticks and tries to push the zombie one cell directly away. Walls, diagonal corner clearance and body capacity can stop displacement without cancelling damage or stagger.
- Defeated zombies have zero health and retain their identity and selection. They stop moving, biting, hearing, blocking as a live zombie, and contributing visible threat or room danger. Their bodies remain in ordinary occupancy and population counts, so existing cell/body limits still apply. No corpse cleanup is added.

Infected humans retain their equipment; on turning it remains attached but inactive, and the human swing cooldown clears. Weapons are not dropped or looted from zombies/corpses in this first pass. There is no durability, skill growth, firearm, blade, critical-hit, grab/pile-on or melee sound/animation system in this build.

## Saves and maps

New encounters use encounter-prototype-38. Foundation 42/43 runs retain their prior rules until **Enable melee combat** is clicked. The upgrade stays paused and preserves actors, clocks, terrain, food, memories and random streams. Earlier runs retain their first missing feature's upgrade button.

Version-four maps add sparse ground weapon records while preserving shelves, door damage and authoring metadata. Maps without ground weapons retain their earlier schema; equipped weapons, skill, swing cooldown and defeated bodies are saved in the running actors. Old saved rules reject weapon equipment and defeated-zombie states rather than silently accepting new behavior. Optional actor/map fields stay omitted from earlier saves.

Automated checks execute production simulation and UI handlers with narrow Unity API doubles. Native Unity rendering, input and directional animation still require local testing.

Validation: 261 simulation cases / 317,579 assertions; 649 legacy-input UI assertions and 3 NewOnly assertions. Checks cover finite competing pickups, actual stat-scaled damage, misses, exact cooldown expiry, blocked knockback, Fight/Flight/Freeze/infection priorities, defeated-body movement and hearing, version-four map ownership/validation, save/resume, old rules and the unchanged long food-scarcity baseline. Project configuration and whitespace checks passed.
