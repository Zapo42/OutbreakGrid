# Foundation 93 — box donation controls

Select a storage box in the simulation. Use **Block donations** to reserve it for local use or **Allow donations** to make its surplus available to couriers again. Editing pauses the simulation. Boxes allow donations by default.

The setting only controls outgoing box-to-box pickups. Local withdrawals, surplus deposits, incoming deliveries, inventory capacity and reserve targets keep their existing behavior. A courier approaching a newly blocked donor abandons collection and returns home. A courier already carrying food finishes the return with that food intact. Donors still protect their reserve when donations are allowed. Only known surplus can start a trip.

The setting is saved independently for each box, including map exports. A blocked box requires map schema 10 and encounter rules 74; older rules reject incompatible saved settings. Default boxes preserve the earlier compact map format. Older saves keep their previous rules; use **Enable delivery controls** to upgrade a running scenario. Restart restores the scenario checkpoint, including its original settings, rather than retaining mid-run edits.

## Manual checks when Unity testing resumes

1. Open **Inspect Supply Delivery Paused**, select the donor box at (12, 3), block donations, and step. No new delivery should start.
2. Allow donations, step until a courier departs, then block again before collection. The donor should retain its food.
3. Restart, let the courier collect first, then block donations. Its two surplus meals should still reach the recipient box at (4, 3).
4. Save and reload with a blocked donor. Confirm the setting persists. Confirm hungry residents can still withdraw meals and residents can deposit surplus locally.

Automated core and UI harness checks cover these rules, saved continuation, default compatibility and upgrade/restart energy preservation. Native Unity remains untested after Foundation 82, per user instruction. The rollback checkpoint remains `foundation-82-unity-checkpoint`.

Foundation 94 adds [assigned couriers and houses](ASSIGNED_COURIERS.md), retaining these donation controls.
