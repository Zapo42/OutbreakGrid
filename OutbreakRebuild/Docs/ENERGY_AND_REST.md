# Foundation 71 / energy and shelter rest

Humans now have a shared 0–100 energy reserve instead of the latest rules' shove-fatigue counter. New humans begin with full energy.

- Every committed human step costs energy: 1 for a straight step, 2 for a diagonal step. This includes fleeing, exploration, shelter entry and food trips. Blocked moves, idle decisions and visual interpolation cost nothing.
- A shove costs 30 energy. A bat swing costs 10, including a miss. Humans lacking the energy for their current defensive action prioritize escape.
- At zero energy, humans act at half their normal opportunity rate. They can still move toward safety or food; movement then increases hunger instead of producing negative energy. A step that exhausts the last energy also charges any remaining cost to hunger.
- Safe shelter rest restores 10 energy per resting action and adds 1 hunger. Hungry, infected or threatened humans cannot use this recovery. Existing food behavior can eat carried meals and then resume rest. Health recovery and energy recovery may share a resting action and its hunger cost.

Low-energy humans seek shelter at 40 energy or below. An occupant already resting can refill completely. Energy recovery waits for shove and swing cooldowns to finish. This build uses existing safe shelters as resting places.

Choose **Inspect Energy Rest Paused**, then **Step 10 ticks**. Energy rises from 20 to 40 and hunger from 10 to 12. Restart restores the initial reserve. Use the combat and movement scenarios to watch energy expenditure.

Existing runs retain their previous rules until **Enable energy and rest** is selected. Existing shove fatigue converts into used energy at 30 per point; two fatigue points become 40 remaining energy. Restart preserves that converted starting state. Energy is saved exactly, and infection conversion clears the human-only resource.

Automated checks cover movement costs, blocked moves, exhausted movement, real shoves and swings, hunger-funded rest, energy bounds, exact save continuation, fatigue migration and upgraded restart. Native Unity appearance and desktop input require local verification.

Validation: 330 core cases / 320,932 assertions; 1,142 UI harness assertions plus 3 with New Input only. Project configuration and whitespace checks passed.
