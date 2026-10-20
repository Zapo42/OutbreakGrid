# Foundation 28: coordinated shelf searches

Hungry humans notice other visible humans actively searching a shelf. If their preferred shelf is busy, they choose a different remembered or visible nonempty/unknown shelf with a complete local route. Among alternatives they retain the existing preference for confirmed food, then proximity and stable cell order. No unopened stock is revealed by coordination.

If there is no usable alternative, the original shelf stays available. Hidden, infected and inactive people do not reserve shelves. A person already partway through a search keeps working. During travel, a usable chosen shelf is retained when free; a visible searcher at that destination can prompt a switch to another shelf. There is no global reservation system or new saved state; choices use current personal sight, notebook entries and existing search progress.

Choose **Inspect Coordinated Search Paused**. Step **10 ticks**: human #1 works on the near shelf while human #2 moves to the other. Step **10 ticks** twice more: both have collected and eaten a meal, leaving three meals on each shelf and two in each inventory.

New encounters use encounter-prototype-27. Older saves retain their original selection until **Enable search coordination** (or the earlier-feature upgrade button) is clicked. All prior search duration, noise, risk and stock conservation rules remain in effect.

Native Unity rendering and input require local testing. Automated checks execute actual simulation and UI handlers with narrow Unity API doubles.

Validation: 202 simulation cases / 35,072 assertions; 508 legacy-input UI assertions and 3 NewOnly assertions. Project configuration and whitespace checks passed.
