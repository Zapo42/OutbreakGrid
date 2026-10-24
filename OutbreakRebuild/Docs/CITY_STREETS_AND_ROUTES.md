# Foundation 110 — city streets and longer routes

Choose **Inspect Generated City Paused** (seed 214) or **Inspect Generated City Alternate Seed Paused** (seed 999), then Run or step game hours. The previews have the same small, finite supply-chain setup over different street layouts. Click the courier to follow the trip, or use the existing store/center observation controls. Restart restores the authored starting state.

## Longer walking routes

Assigned humans on current rules can plan beyond the previous 64-cell routing square. For safe trips with goals more than 16 cells away, a deterministic A* search favors direct travel with cardinal/diagonal costs of 10/14. This avoids unnecessary diagonal zigzags on straight routes. A failed local query can also try the extended search. It retains closed-door actions, diagonal door/corner restrictions, occupancy, zombie blocking and distribution-center entry permissions. Threat-aware and confined-room queries keep their existing local behavior.

Each extended query is bounded to 256 cells from the actor on either axis and at most 16,384 discovered cells. It uses a sparse frontier, not a world-sized navigation array. The planner recomputes from current geometry and uses no random draws or persistent route cache. It cannot guarantee a route through an arbitrarily large city or around an obstacle outside its budget. Travel still uses energy; rest, hunger, danger, deadlines and finite carried food can interrupt long journeys.

New runs use encounter-prototype-88. Saves at rules 87 or earlier retain their previous routing until explicitly upgraded using **Enable longer routes** (or the appropriate earlier feature upgrade). No saved fields were added. Generated terrain is serialized normally, so loading a save restores its actual streets without rerunning a generator.

## First city-generation stage

`CityGenerator.Streets(seed, columns, rows)` creates a connected rectangular network with seeded block spacing, two-cell roads, perimeter sidewalks and crosswalks at junctions. The same nonzero seed/settings recreate the same geometry. Each axis supports 1–6 blocks; the previews use four by two. A wall border contains the map.

Buildings in these previews are **fixed test fixtures**, with one home, one distribution center and one store. They occupy clear block interiors for both preview seeds. The courier and clerk have the separate assigned home; work begins at their respective workplaces. The courier supplies a store beyond the old 64-cell planning limit. Thirty center meals plus two personal rations make **32 meals** in total; the store starts empty and its clerk phones for deliveries. Watching several game hours allows for walking, rest, meals and repeated partial deliveries.

This is street-generation groundwork. It does not yet generate building lots, doors, residents or job assignments. Pedestrians can still cross road terrain away from crosswalks and walk across undeveloped grass. Sidewalk/crosswalk preference, vehicle lanes, traffic and driving are later steps. The street layout supplies geometry, not NPC knowledge of unseen shops or stock.

## Upcoming items

The user's requested future categories are **supplies**, **medical** and **ammo**, rather than food subtypes. They are recorded for the later inventory/store expansion; this increment keeps the existing food inventory and storage rules. Weapons are still not stored in boxes.

## Validation

Automated cases verify connected street/sidewalk/crosswalk geometry across three seeds, exact regeneration and serialization, bounded long-route reach, detours and blocked destinations, legacy routing, finite-food conservation, physical distant deliveries and exact saved continuation. UI API doubles cover both the current entry/upgrade path and scenario restart; they do not establish native Unity rendering, frame rate or input. Unity 6000.6.3f1 testing remains paused after Foundation 82.
