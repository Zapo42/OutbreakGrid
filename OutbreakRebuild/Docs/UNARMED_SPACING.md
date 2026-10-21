# Foundation 68 / unarmed spacing

Unarmed humans who are willing to fight no longer charge close zombies after becoming alarmed. They use the existing collision-aware backstep to increase separation from nearby visible threats. If a safe retreat is blocked, they hold instead of charging. At greater distances they avoid approaching just to shove.

A ready human still shoves defensively on contact. While that shove is recovering, an unarmed fighter tries to create space instead of standing in contact. Frozen and fleeing reactions, infection avoidance, food and shelter decisions retain their existing priorities. Humans with bats retain their existing fighting behavior.

Backsteps consider visible threats, respect walls, doors, solid corners and crowd limits, and use no extra random draw. This changes authoritative movement decisions and is versioned for exact older-save behavior.

Choose **Inspect Unarmed Spacing Paused**, then **Step 1 tick**. The alarmed, supported human should move away from the zombie, rather than advancing toward it as under the previous rules. Restart repeats the setup. Existing saves retain their original behavior until **Enable unarmed spacing** is selected.

Automated checks cover actual retreat, no random consumption, exact save continuation, older charging behavior, blocked retreat cells, ready shoves, recovery backsteps, upgrade and restart. Native Unity appearance and desktop input require local verification.

Validation: 322 core cases / 320,885 assertions; 1,114 UI harness assertions plus 3 with New Input only. Project configuration and whitespace checks passed.
