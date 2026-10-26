# Faster cars and saved colors — Foundation 130

Open **Inspect Faster Cars & Colors Paused** from the main menu. Eight cars show Blue, Red, Green, Sand, Silver, Charcoal, White and Yellow. The blue car has a road destination; the others occupy parking bays. Select a car and use **Next vehicle color** to cycle its saved paint. Selection adds a yellow outline and preserves the body color.

Current cars move one road cell every two simulation ticks, previously three: 50% greater road speed, or one-third less travel time over the same unobstructed route. Translation, turning and driver exit waits use the matching interval. Lane, junction, crosswalk, parking and traffic safety still apply; congestion can delay journeys.

Older runs retain their previous pace and blue paint until **Enable faster cars and colors** is selected. Upgrading clamps pending move/exit waits to the new interval, without resetting trips, inventories or assignments. Restarting an upgraded scenario retains the new rules. Paint is deterministic on placement, editable, and saved; it does not consume simulation randomness.

Vehicle shapes, bicycles and finite fuel remain separate future increments. Unity 6000.6.3f1 native testing remains paused; automated core and UI-double checks do not establish native rendering or Play mode results.
