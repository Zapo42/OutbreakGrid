# Foundation 59 / visible combat health

Living injured humans and zombies now have a small health bar above their body. Its fill follows actual health, and its color becomes redder as health decreases. Full-health actors and defeated zombies have no bar. Selection also shows exact health out of 100 in the inspector.

Choose **Inspect Health Bars Paused** to compare an injured human, injured zombie, healthy human and defeated zombie. Choose **Inspect Blunt Combat Paused** and step or run to see bars change during actual combat. Existing hit flashes and weapon motion remain visible.

This is a presentation update. Combat damage, infection, movement and saved rules retain their existing behavior. No save upgrade is required; health bars also reflect damage restored from saved runs.

Automated UI checks verify fill proportions and placement, injured-only visibility, exact selected health, unchanged simulation state while rendering, actual combat damage and saved damage restoration. Native Unity appearance and desktop input require local verification.

Validation: 310 core cases / 320,868 assertions; 883 UI harness assertions plus 3 with New Input only. Project configuration and whitespace checks passed.
