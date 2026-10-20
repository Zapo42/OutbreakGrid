# Foundation 32: live food overview

The simulation inspector now starts with a live food summary for runs with hunger enabled:

- Human count and infected humans (an infected human remains in the human total until conversion).
- Hungry humans at hunger 40+, and the very hungry subset at 80+.
- Active food trips and partial shelf searches.
- Meals carried by humans and meals remaining on all shelves.
- Stocked, empty and unsearched shelf counts.

These are player-wide map totals, including unopened shelf stock. They do not reveal food or humans to the simulated actors or alter their personal memories. Food trips include uninfected hungry humans with no carried food and an active shelf target; searching is the subset with partial search progress. Infected humans are included in the hunger counts but not active food tasks.

Choose **Inspect Exposed Food Paused**. Initially there are twelve shelf meals across two shelves. **Step 1 tick** changes that to three carried and nine on shelves. **Step 10 ticks** shows the human fed. The summary updates while running and after manual steps, placement and loading; scroll the inspector for controls and individual details below it.

This build changes observation only. Simulation rules remain encounter-prototype-30, saves need no upgrade, and older hunger-enabled runs also show the summary. Runs without hunger keep their existing inspector.

Native Unity layout and input require local testing. Automated checks execute the actual summary and UI handlers with narrow Unity API doubles.

Validation: 214 simulation cases / 35,107 assertions; 527 legacy-input UI assertions and 3 NewOnly assertions. Project configuration and whitespace checks passed.
