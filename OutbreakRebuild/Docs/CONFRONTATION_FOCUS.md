# Foundation 61 / confrontation focus

**Next active fight** pauses and focuses on a human within two cells of a visible, living zombie. It centers the camera between the human and nearest close zombie, selects the human and shows the zombie's identity and exact health in the sidebar. Repeated clicks cycle through eligible humans in stable identity order and wrap around.

A confrontation means close visible opponents; the human may fight, freeze or flee according to its existing behavior. Distant zombies, defeated zombies and opponents hidden behind walls or closed doors do not qualify. The button is disabled when no confrontation exists. This inspection tool does not change actor decisions, saved state or random streams.

Choose **Inspect Combat Footwork Paused**, then **Next active fight**. Step one tick and inspect the zombie's updated health. You can also use the control in a larger encounter while running; it pauses automatically. Existing saves require no upgrade.

Automated checks cover close sight, barriers, range, defeated opponents, stable opponent selection, camera framing, cycling, pausing, exact saved-state preservation and health changes after real combat. Native Unity appearance and desktop input require local verification.

Validation: 311 core cases / 320,873 assertions; 915 UI harness assertions plus 3 with New Input only. Project configuration and whitespace checks passed.
