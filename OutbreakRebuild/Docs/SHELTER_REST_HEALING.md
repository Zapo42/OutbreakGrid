# Foundation 65 / shelter rest healing

Injured, uninfected humans now seek shelter even when calm. Once at their indoor resting position, they recover 2 health per normal action and gain 1 hunger point. Healing caps at 100 health and consumes that action, so it cannot be combined with movement or a weapon action.

Rest requires zero stress, no visible zombie, no recent warning or door impact, and hunger below 40. Existing food, shelter evacuation, door bracing and repair decisions retain priority. Hungry humans stop healing and use their normal food behavior. Infection prevents healing; resting never cures a bite or changes its deadline.

Choose **Inspect Shelter Healing Paused**, then **Step 50 ticks**. Health should rise from 60 to 80 while hunger rises from 10 to 21, including ordinary hunger gain. Another 50 ticks reaches full health. **Restart scenario** restores the original injury. Existing saves keep their old behavior until **Enable shelter healing** is selected.

Automated checks cover real healing actions, hunger cost and threshold, health cap, indoor shelter seeking, danger and infection interruption, exact save continuation, earlier rules, upgrade and restart. Native Unity appearance and desktop input require local verification.

Validation: 314 core cases / 320,887 assertions; 1,091 UI harness assertions plus 3 with New Input only. Project configuration and whitespace checks passed.
