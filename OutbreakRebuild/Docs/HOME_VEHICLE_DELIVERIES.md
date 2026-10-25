# Foundation 125: vehicle home deliveries

Targets Unity 6000.6.3f1. Native Unity testing remains paused after Foundation 82.

Choose **Inspect Home Vehicle Deliveries Paused**. Courier #1 has a vehicle and two assigned home-box destinations, each with a six-meal reserve. Store phone orders take priority; home deliveries run afterward. The preview retains normal residents and finite food, so consumption can reopen shortages.

To configure a route, select an idle vehicle and unassign its courier. Select that NPC, choose **Edit courier assignment**, select each home storage box and **Add courier destination**. Set its food reserve or household target. Finish editing and assign that courier to the vehicle again. Up to eight destinations use the existing saved round-robin cursor. The source remains the courier's assigned distribution-center crate.

Each delivery physically collects food above the source reserve, carries it to the vehicle, drives to the curb, walks to the home box, and deposits only its current reserve deficit and available capacity. Pickup is capped at the destination reserve and 24 meals; the courier does not remotely read unknown destination stock. Stock learned firsthand prevents repeat visits to already satisfied boxes. Other workers respect an active vehicle's destination claim. Excess cargo returns to the source, including when a resident has already replenished the box or its target is reduced during transit.

Uses existing vehicle journey fields and courier destinations; no new inventory types or save fields. Pause, needs, schedules, incapacitation and shipment recovery retain their existing behavior. Assigned home routes are optional; vehicles without them continue serving store requests. No automatic assignment of every home, home phone requests, parking bays or traffic recovery in this step.
