# Shelter after warnings

Current build: **Foundation 17 / shelter after warnings**, rules `encounter-prototype-16`.

Outdoor humans now seek an available shelter when they hear a nearby human warning or door impact. They can respond to danger without personally seeing a zombie. Their hearing still records only the sound source; it never grants an unseen zombie's location or identity.

## Try it

1. On GitHub choose **Code → Download ZIP**, extract into a fresh folder and open **OutbreakRebuild** in Unity 6.
2. Choose **Outbreak → Rebuild → Open and Run Foundation**.
3. Choose **Inspect Warning Shelter Paused**, then select human **#3**, below the small room.
4. Click **Step 10 ticks**. Human #3 hears the warning at **6, 2**, reserves the available room and moves through its doorway. The inspector shows the shelter destination and heard warning without a zombie target.
5. Click **Step 10 ticks** again: the human is inside and closes the door behind itself. The partition prevents a personal zombie sighting throughout this example.

Existing Foundation 16 saves keep their previous behavior until **Load Run → Enable warning sheltering**. The upgrade stays paused, preserves actors, door damage, clock and random streams, and leaves the source file intact. New encounters enable this behavior automatically. Earlier controlled scenes retain their original behavior where needed to isolate their earlier milestone.

## Behavior and limits

- A recent human warning or door impact can prompt a shelter decision on the listener's next ordinary action. Stress by itself, or a warning that expired before that action, cannot start this response.
- Existing room admission limits, cell reservations, collision rules and bounded route planning apply. Full or unreachable shelters cannot accept the human; the normal alert behavior remains the fallback.
- Once a human chooses a valid shelter, it follows that goal even if the warning expires while traveling. This uses the existing shelter state without a new hidden target or a new random stream.
- The speaker need not remain in the world. Current personal sightings, infection and known danger inside a room continue to govern existing shelter/escape decisions.
- Quiet still allows stress recovery, the calm wait, a checked exit and ordinary shelter departure. Door bracing and the muted door palette from Foundation 16 remain available.

## Verification

The production-core suite covers actual warning-triggered travel, entry and door closing without sight of a zombie; listener-only continuation and quiet departure; absent/expired warnings; door-impact response; full/unreachable shelters; pause and exact saved travel; and independently recorded prototype-15 bracing behavior. The UI harness exercises scenario selection, body selection, the hearing inspector, tick controls and explicit older-save upgrade while preserving its source file.

Validation passed: **129 core acceptance cases / 33,703 assertions**, **404 UI assertions** with legacy input plus **3** with New-only input, and the project configuration check.

Native Unity rendering, physical input and a Windows player still require local Unity testing.
