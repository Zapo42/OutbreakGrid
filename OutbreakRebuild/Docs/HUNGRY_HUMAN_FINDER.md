# Foundation 33: find hungry humans

**Next hungry human** appears in the live food overview. It pauses playback, selects the next human at hunger 40+, focuses the view on their cell and keeps the current zoom. It cycles in stable identity order, wrapping past fed humans and zombies. Infected humans remain eligible while human, matching the overview's hunger totals.

The command cancels human/zombie/shelf placement and clears shelf selection so the next map click is safe for inspection. It changes presentation state only: no ticks, food, personal memories or random streams advance. The control disables when no hungry humans remain.

Test with **Inspect Coordinated Search Paused**, then click **Next hungry human** repeatedly. The selected human IDs cycle between #1 and #2. Their positions initially overlap, so use the inspector or status line to distinguish them. In larger runs the view moves to each person's location.

Existing saves work without an upgrade; simulation rules remain encounter-prototype-30. Native Unity layout, camera appearance and input require local testing. Automated UI checks execute actual production handlers with narrow Unity API doubles, covering deferred dispatch, pausing, camera focus, cycling, placement cancellation and the no-hungry state.

Validation: 538 legacy-input UI assertions and 3 NewOnly assertions; project configuration and whitespace checks passed. Simulation code is unchanged.
