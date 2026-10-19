# Quiet door repair

Current build: **Foundation 18 / quiet door repair**, rules `encounter-prototype-17`.

Calm sheltered humans now repair damaged doors from inside once recent danger has passed. Each normal human action restores five HP while the worker stays beside the closed door. Broken doors remain broken.

## Try it

1. On GitHub choose **Code → Download ZIP**, extract into a fresh folder and open **OutbreakRebuild** in Unity 6.
2. Choose **Outbreak → Rebuild → Open and Run Foundation**.
3. Choose **Inspect Door Repair Paused** and select human **#1** inside the room. The nearby closed door starts at **40/100 HP**.
4. Click **Step 10 ticks**: the door reaches **50/100 HP**. Its red tint fades as damage is repaired.
5. Click **Step 50 ticks**: the door reaches **100/100 HP**, returning to its light muted brown.
6. Click **Step 50 ticks** again: after the normal calm wait and checked exit, the human has left the shelter.

Existing Foundation 17 saves retain their previous behavior until **Load Run → Enable door repair**. The upgrade remains paused and preserves actors, terrain, door health, clock and random streams without overwriting its source file. New encounters enable repairs automatically.

## Rules and limits

- Work requires an uninfected human with zero stress, inside its chosen shelter and cardinally adjacent to one of that room's closed doors. A visible zombie, recent human warning or recent door impact prevents repair.
- Repair uses a normal human opportunity instead of movement, bracing or shelter departure. Recent attacks keep bracing in control; quiet maintenance resumes only after hearing expires and stress recovers.
- Repair adds five HP up to 100, with no random draws. If several doors are eligible, canonical room order selects one per opportunity. This prototype does not require tools or construction materials.
- Open doors must be closed in a separate normal action before work begins. Broken doors are never repaired or reclosed by this feature. Humans already reject a broken entrance they can actually see; this update preserves that behavior.
- Work delays shelter departure. After maintenance finishes, the human resumes the ordinary calm wait and checks the exit before leaving.
- Repaired health persists in both running saves and starting-map exports. Full restoration removes the sparse damage record. Repair changes condition feedback without changing collision or generating a zombie attack sound.
- Door bracing, warning responses and the muted brown/red/grey palette remain available.

## Verification

Production-core checks cover action timing and actual health gain, collision and sound stability, full restoration and canonical map state, later departure, broken/open doors, stress/infection/visible threat/distance exclusions, warning and impact deadlines, pause and exact saved continuation, and independently generated prototype-16 behavior. UI checks exercise the new scene, body selection, tick buttons, health inspector and explicit older-save upgrade with source-file preservation.

Validation passed: **136 core acceptance cases / 33,834 assertions**, **416 UI assertions** with legacy input plus **3** with New-only input, and the project configuration check.

Native Unity rendering, physical input and a Windows player still require local Unity testing.
