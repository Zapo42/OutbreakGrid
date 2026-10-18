Latest build: [Shelter after warnings](WARNING_SHELTER.md).

# Door bracing and door colors

Current build: **Foundation 16 / door bracing**, rules `encounter-prototype-15`.

Sheltered humans can now brace a recently attacked door from inside. This halves incoming damage while they physically hold it, buying time without repairing the door or making it invulnerable.

Doors also have the requested palette: **light muted brown when closed**, **dark muted brown when open**, an increasing **red tint with damage**, and **grey when broken**. Both open and closed damaged doors retain the red tint. Existing damage remains intact when opening or closing.

## Try it

1. On GitHub choose **Code → Download ZIP**, extract into a fresh folder and open **OutbreakRebuild** in Unity 6.
2. Choose **Outbreak → Rebuild → Open and Run Foundation**.
3. Choose **Inspect Door Bracing Paused** and select human **#1** inside the small room.
4. Click **Step 10 ticks**. The human's inspector shows **Bracing door at 3, 2**, and the nearby door shows **76/100 HP / BRACED**.
5. Two more **Step 10 ticks** reach tick 30. The door still has **44 HP**, where the original unbraced diagnostic would be almost broken. Watch its red tint increase as attacks continue.
6. Another **Step 50 ticks** passes the breach at tick 57: the door opens permanently, turns grey and loses its brace. Ordinary zombie pursuit and infection resume.

For an existing Foundation 15 save, use **Load Run → Enable door bracing**. The upgrade stays paused and preserves actors, damage, clock and random streams without changing the source file. New encounters enable bracing automatically. Earlier controlled breach/hearing diagnostics keep their original rules; this new scene demonstrates bracing.

## Rules and limits

- A human must be inside its chosen shelter, cardinally adjacent to one of that room's closed, unbroken doors, and must have recently heard an actual strike on that specific door. Warnings alone cannot trigger bracing.
- Holding uses the human's normal action and renews for at most five ticks. The human stays in place and cancels shelter departure while holding. Infection and a currently visible zombie prevent this defense, leaving ordinary shelter/escape behavior in control.
- Incoming damage is halved with a minimum of one HP. Extra defenders do not stack reductions. There are no construction materials, permanent barricades or repairs in this milestone.
- Quiet releases defense at the hearing deadline; opening, breaking, abandoning the shelter or turning clears it. Hearing a different door strike releases the previous brace until the human decides again.
- Defense and damage pause and save exactly. Starting map exports retain actual door damage; temporary human actions belong to running saves.

## Verification

The production-core checks cover first-hit timing, reduced later damage, eventual breach and contact, quiet recovery, pause and exact save continuation, distance/infection exclusions, changed sound sources, malformed saved actions and independent prior-release behavior. UI checks exercise the scenario, inspector, durability labels and explicit older-save upgrade while preserving its source file.

Validation passed: **122 core acceptance cases / 33,578 assertions**, **394 UI assertions** with legacy input plus **3** with New-only input, and the project configuration check.

Native Unity rendering, physical input and a Windows player still require local Unity testing, including visual review of the new palette.
