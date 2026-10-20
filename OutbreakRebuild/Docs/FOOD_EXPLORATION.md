# Foundation 42: deliberate food exploration

When a hungry, uninfected human has no usable food targets in memory, it can investigate a room instead of depending entirely on random wandering. Known food keeps priority. Hunger and bravery must still permit the trip under the existing perceived-risk rules.

The human selects the nearest reachable room inspection point among rooms whose entrances it can currently observe, or its current room. A closed door's outer approach must be visible; the decision never reads hidden shelf locations or stock. Rejected, visibly unsafe and full rooms are excluded. Routes use the guarded-food clearance rules. An occupied refuge is investigated before starting another hungry departure.

Room admission, physical movement, opening doors and closing them after entry use the existing shelter behavior. An active investigation postpones ordinary hungry departure while walking toward the inspection point. Discovering a usable shelf switches to normal food searching. If that happens in the doorway, the human retains its room assignment through entry so it can close the door behind itself.

Each human saves up to eight recently investigated or abandoned rooms. That history lasts 300 ticks and prevents immediate repeated visits to the same empty room. An investigation has a 150-tick time limit; expiry releases an outdoor reservation even between movement opportunities. Excessive risk, infection, acquiring food or becoming fed cancels exploration at the next food decision. An indoor human retains its refuge; an outdoor human releases the exploration reservation and can use the existing food-trip retreat behavior. Turning clears all human exploration state.

This first exploration pass investigates one interior observation point per room visit. It does not certify an entire large or irregular room empty, sweep every hidden corner, or plan travel to unseen buildings. Ordinary local observation discovers shelves along the trip, and existing shelf memory and reporting remain authoritative.

## Try it

1. Choose **Inspect Food Exploration Paused**.
2. Click **Step 1 tick**, then **Next hungry human**. The outdoor shelf is known to be empty. A purple **Room investigation** marker points into the left room, whose visible entrance is closer.
3. Click **Step 100 ticks**, then **Step 50 ticks**. The human has investigated and left the empty left room, discovered an unopened shelf in the right room, searched it, and eaten. The right door closes after entry. Normal pantry storage may put a spare meal back on the shelf.
4. The inspector shows **Recently investigated rooms: 2**. No food is created: five of the six original meals remain across shelf and inventory after one meal is eaten.

New encounters use encounter-prototype-37. Foundation 41 runs keep their prior choices until **Enable food exploration** is clicked. Earlier runs retain their first missing feature's upgrade button. The upgrade stays paused and preserves actors, clocks, terrain, inventory and random streams. Old saves omit the new optional fields; their original behavior is preserved. New saves retain active investigation deadlines and detached, bounded room history.

Automated checks execute production simulation and UI handlers with narrow Unity API doubles. Native Unity rendering and input still require local testing.

Validation: 247 simulation cases / 35,343 assertions; 618 legacy-input UI assertions and 3 NewOnly assertions. Checks cover empty-room progression, hidden-stock independence, guarded risk, admission, visible entrances, exact deadlines, bounded history eviction, defensive copies, corrupt saves, infection cleanup and prior-rule compatibility. Project configuration and whitespace checks passed.
