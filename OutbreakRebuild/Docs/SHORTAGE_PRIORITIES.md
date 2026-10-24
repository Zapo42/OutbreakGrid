# Foundation 103: shortage priorities

Open **Inspect Multi-Store Supply Paused** and step forward. Expand store management to see orders in dispatch order.

Couriers now choose the eligible, unclaimed request with the smallest delivered share. For example, an order with 0/4 delivered precedes one with 2/4; 2/8 also precedes 2/4. Equal shares use oldest request, then lowest order ID. This compares the clerk’s reported request and recorded deliveries, not hidden live shelf inventory, consumer hunger or travel distance.

Existing trips finish unchanged. Off-duty, threatened or exhausted workers still cannot start; source reserves and donation settings remain protected. A partial order stays pending when the center runs out. No supplies are generated.

This is request-level priority: a fresh repeat request also starts at zero delivered. It does not guarantee equal lifetime deliveries to stores or starvation prevention. Live shortage reports and aging could refine that later.

New scenarios use encounter rules 83. Rules 82 saves preserve oldest-first dispatch until explicitly upgraded with **Enable shortage priorities**. Save under a new name to preserve the old behavior. Restarts retain the loaded rule generation.

Automated coverage checks actual courier claims against legacy behavior, different request sizes, stable ties, saving/resuming, active-trip continuity, three-seed finite-food trials, and the UI policy label. Native Unity testing remains paused after Foundation 82.
