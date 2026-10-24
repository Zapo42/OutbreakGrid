# Foundation 107: unfamiliar stores and alternatives

Target editor: **Unity 6000.6.3f1**. Native Unity testing remains paused after Foundation 82.

## Observe the new scenario

Open **Inspect Store Discovery Paused**. One resident starts with no store or shelf memories, an empty home chest with a two-meal target, and one personal meal. The nearer store has an unsearched empty shelf; a second store has six meals on a shelf and eight in protected back stock. Total food starts at 15 meals.

Run or step. The resident cautiously exits home, recognizes visible store entrances, enters the nearer shop and searches its shelf. After the unsuccessful visit it leaves that shop, checks the second store, collects food and brings two meals home. There are no clerks in this diagnostic, so the empty first store stays empty and the discovery/fallback behavior is easy to isolate. Earlier scenes still exercise clerk replenishment and distribution delivery.

The selected-item inspector distinguishes **Looking for store entrances**, **Investigating store #…**, **Collecting from store shelf**, and **Returning food home**. It shows personally known entrances, recent unsuccessful visits and any retry delay. Map target markers follow the current survey, investigation, shelf or home destination. Restart clears discoveries and restores the initial food, assignment and empty home.

## What humans know

A safe consumer with an observed home shortage can now begin a shopping survey without a preseeded shelf memory. Its existing home-door inspection provides the first outdoor lookout. Store entrances become known only when their nearby outer face is actually visible, or when the human is physically inside the store. A sign/entrance observation identifies a configured store, not its inventory.

Each human retains up to eight personally observed store entrances for six game hours, refreshed by later sightings. These notes are saved, detached from caller-owned arrays and cleared on conversion/death with other human memories. They are not a global list disclosed to the AI. Changing unseen food quantities cannot alter which entrance the human initially chooses.

Known usable shelf observations are still preferred. Otherwise the resident chooses a reachable remembered store entrance. Inside, ordinary shelf observations and searches reveal supplies. If no usable shelf is known, the resident checks reachable floor viewpoints, skipping cells only when they are currently visible. It does not inspect hidden food counts to decide which corner to visit. Search progress survives saving.

## Empty shops and retry behavior

Finding one empty shelf no longer automatically ends the trip. The shopper can investigate the rest of the store for another shelf. After an unsuccessful investigation it remembers that store briefly and tries another personally known store during the same shopping trip, keeping the original home-box claim.

Unsuccessful visits are remembered for 300 ticks (30 game minutes). This also covers unreachable entrances/viewpoints; it does not assert that every hidden shelf is certainly empty. A fresh observation of usable food can override a previous failure. If no usable known store remains, the human returns home and waits before trying again. The retry delay prevents an immediate home → empty-store loop. Discovery and failure memories have separate bounded lists, so routine hunger exploration history is preserved.

This is local, observation-driven discovery. A store beyond the human's sight is not automatically known. This increment does not add a city-wide search patrol, larger route range, roads or vehicles. With no visible/remembered candidate after the outdoor survey, the resident returns and retries later. It does not wander indefinitely or teleport to an unseen market.

## Existing rules remain active

Hunger, personal meals, carrying limits, energy, danger, infection, task expiry and finite inventory still apply. Store back stock and distribution storage remain protected from ordinary consumers. Trips preserve personal rations and deposit actual carried food into the assigned home box. Full home boxes retain surplus cargo. Household box claims prevent duplicate trips to the same destination. Clerks and couriers retain their current work routines; shifts, commutes and occupation outlines are still later increments.

The 5x/10x/20x/50x testing buttons, selected-only H/F/E bars, descending hunger display and home-supply knowledge remain available.

## Saves and validation

New scenarios use **encounter-prototype-86**. Rules 85 retain known-shelf-only shopping. Older runs offer **Enable store discovery**; save under another name to preserve prior behavior. An existing shopping trip can be upgraded without fabricating entrance observations. Remembered entrances, unsuccessful visits, survey/store/search stage and retry time are validated before publishing a restored world.

Automated coverage checks a complete no-memory → empty shop → alternative → home trip, food conservation, hidden-stock independence, empty-shop retry delays, blocked entrances, exhaustion, exact save continuation, detached and expiring memories, malformed saves, legacy behavior, active-trip upgrades and UI observation/restart/upgrade controls. Native Unity import, rendering, input and layout remain unverified while testing is paused.
