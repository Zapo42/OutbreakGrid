# Foundation 66 / recovery food reserves

Uninfected humans below 50 health now reserve two carried meals instead of one. Sharing, pantry storage and delivery trips treat only food above that reserve as surplus. A badly injured human with three meals can still share or store one; at 50 health, the usual one-meal reserve resumes.

Eating already uses the normal hunger system. When a resting human becomes hungry, it eats one real carried meal in a separate action, then can resume healing when fed and safe. The reserve creates no food, increases no inventory capacity and does not prevent eating reserved meals. Infection still prevents healing.

Choose **Inspect Recovery Rations Paused**, then **Step 50 ticks**. The human keeps its two initial meals rather than storing one, rests, eats one when hunger reaches 40, and continues healing to 48 health. One meal remains carried and the adjacent shelf remains empty. **Restart scenario** restores the original injury and two meals. Existing Foundation 65 saves retain their previous reserve until **Enable recovery food reserves** is selected.

Automated checks cover finite food conservation, separate eating and healing actions, actual sharing and storage above the reserve, delivery decisions, the 50-health threshold, exact save continuation, earlier rules, upgrade and restart. Native Unity appearance and desktop input require local verification.

Validation: 318 core cases / 320,899 assertions; 1,098 UI harness assertions plus 3 with New Input only. Project configuration and whitespace checks passed.
