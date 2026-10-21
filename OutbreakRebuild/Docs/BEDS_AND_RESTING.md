# Foundation 72: Beds and resting

Use **Place bed** during a paused simulation, then click a free floor cell inside an enclosed shelter. **Remove bed** removes it without changing the floor. Beds have a brown frame, teal mattress and pale pillow. They remain walkable.

Humans with 40 energy or less prefer an available, visible, reachable bed in their current shelter. They reserve separate beds and fall back to ordinary shelter rest when none is available. Rest requires the same safe, calm, uninfected and sufficiently fed conditions as before. A bed restores up to 20 energy per rest action instead of 10, at the same one-point hunger cost. Movement still costs energy. Full humans release their bed and move to an ordinary shelter position.

To try it, choose **Inspect Bed Rest Paused** from the main menu, then **Run**. The tired human walks to the bed, replenishes energy and leaves the bed available. Restart restores the initial bed and energy. New configured encounters begin without beds; place them where you want people to recover.

Beds persist in maps and saved runs (map schema 7, encounter rules 57). Existing Foundation 71 runs retain their old behavior until **Enable beds** is clicked. Earlier upgrade buttons also enable current behavior. Saves without beds retain their existing map format.

Automated checks cover bed travel and energy costs, accelerated recovery and hunger cost, release, occupied-bed fallback, placement/removal, invalid map data, save continuation and old energy-save upgrades/restarts. UI checks use narrow Unity API doubles; native Unity appearance and input require testing in the editor.
