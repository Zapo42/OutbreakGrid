# Foundation 34: find stocked shelves

**Next stocked shelf** appears beside the hungry-human finder in the food overview. It pauses playback, selects a shelf containing food and focuses the view on it without changing zoom. Repeated clicks cycle in coordinate order, skipping empty shelves and wrapping at the end. The existing shelf inspector shows its stock and search state.

Focusing a shelf clears body selection and cancels human, zombie or shelf placement. It changes presentation only: no ticks, stock, personal knowledge or random streams advance. Shelves with unsearched stock are included in this player-wide overview, without revealing them to simulated humans. The control disables when all shelves are empty.

Test with **Inspect Exposed Food Paused**, then click **Next stocked shelf** repeatedly. The view alternates between the outdoor shelf at 6,2 and the room shelf at 2,3. Pair it with **Next hungry human** to compare remaining supplies with people who need food.

Existing saves need no upgrade; rules remain encounter-prototype-30. Native Unity layout, camera appearance and input require local testing. Automated UI checks exercise actual handlers with narrow API doubles, covering selection, deferred dispatch, camera focus, unchanged world/zoom, placement cancellation, empty-shelf skipping, cycling and no-stock disabling.

Validation: 552 legacy-input UI assertions and 3 NewOnly assertions; project configuration and whitespace checks passed. Simulation code is unchanged.
