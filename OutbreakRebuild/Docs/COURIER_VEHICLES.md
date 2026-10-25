Foundation 125 adds [assigned home deliveries](HOME_VEHICLE_DELIVERIES.md) after store orders. The cycle below also applies to home boxes.

# Courier driving and vehicle deliveries — Foundation 124

Choose **Inspect Vehicle Deliveries Paused**. The preview has five residents, two stores, a distribution center and one vehicle assigned to courier #1. Run at 1×/5× to watch movement, or step 100 ticks at a time. The selected vehicle shows its driver, cargo, delivery phase and normal traffic status. Around tick 200 the courier is aboard; both initial 12-meal store requests finish within 2,500 ticks in the checked seed-214 preview.

For an existing city, enable current behavior if offered, place a vehicle on a road near the center crate, select it and choose **Assign courier #…**. The courier must already work at that distribution center and be free of an on-foot restocking/order task. The vehicle must be within 64 cells of the source crate when first assigned. Prefer an ordinary curb beside a sidewalk, away from a crossing/junction. Existing generated cities still begin without vehicles unless you use the dedicated delivery preview.

## Physical delivery cycle

A center courier selects an unclaimed phone replenishment order, walks to the source crate and collects only stock above its reserve. Personal provisions stay in the existing three-meal inventory. The shipment is separate, with a maximum of **24 meals**. The courier carries that crate to the vehicle, boards from an adjacent cell, drives along legal lanes to a reachable curb near the store, then steps out after the arrival movement finishes. The courier physically carries the shipment to the store crate before food and order credit transfer. It returns to the vehicle and drives back to the source before taking another order.

The visible brown crate follows the courier on foot and appears on the vehicle while loaded. A seated courier is not drawn as a second pedestrian. Actor identity, home assignment, health and needs remain attached to the same NPC. Lanes, following gaps, pedestrian yielding, junction priority and smooth vehicle rotation remain active. Normal foot movement still uses existing collision and door rules, including closing the departure door after clearing it.

Orders may serve different stores across successive trips. This version handles phone orders from a distribution center; it does not yet drive consumers, carry passengers, or convert manual home-box delivery routes into vehicle jobs. It makes one store trip at a time rather than touring multiple stops in one load.

## Inventory, needs and interruptions

Food moves source → courier's shipment → vehicle cargo → courier's shipment → destination. It is never credited just because the vehicle reaches a curb. Loading respects source reserves; unloading respects capacity and the order's remaining quantity. Excess cargo returns physically to its source. A full destination is remembered rather than repeatedly treating it as empty. Cancelled/reconfigured requests release their claim and retain the shipment for return. No weapons, medical or ammo cargo was introduced.

Food Overview includes vehicle, carried and dropped reserved shipments in its remaining-meal ledger. Personal meals and any shipment food eaten by a courier reduce the total normally. Couriers eat their own provisions first; when personally carrying the shipment or seated with cargo, they can use it if necessary. Only food actually delivered counts against an order.

Driving costs one energy point per ten road cells, with partial effort saved between steps. Exhausted driving adds hunger instead, under the existing needs model. Seated couriers do not receive bed/rest recovery. Jobs already underway finish their return cycle after shift end; idle couriers revert to normal work/home/rest behavior. Vehicles currently cover deliveries, not the commute home.

**Stop vehicle** pauses the assigned delivery and retains its rider, food and order claim. Use **Resume courier deliveries** to continue. This does not pause the whole simulation: hunger and health continue. Manual destination commands are unavailable while a courier owns the vehicle. Unassign it when idle, empty and unoccupied before changing the courier's house, route or schedule.

If a courier becomes infected/incapacitated, the vehicle stops. Hand-carried shipments drop at that actual position and remain reserved, visible and saved; loaded cargo remains in the vehicle. A seated incapacitated driver exits when an adjacent cell is available. Assign a healthy replacement courier from the **same source crate** to recover the dropped shipment or return retained cargo. A blocked exit waits instead of overlapping pedestrians. Ground shipments are currently reserved recovery crates, not general consumer loot.

## Saves, routing and performance

New running saves use **encounter-prototype-95**. Rules-94 saves retain their previous behavior until **Enable courier vehicles** is selected. Driver/occupant relationships, cargo/hand/dropped quantities, order references, phase, pauses, effort, meal and exit/retry clocks all persist. Malformed roles, quantities, duplicate assignments/claims, legacy vehicle-job data and overlapping occupancy are rejected before loading. Restart restores the original scenario and assignment. Save Run retains shipments; Save Map retains terrain and stationary inventories, not NPC/vehicle cargo.

Assigned drivers share bounded reverse road-distance fields instead of searching the entire road network on every step. These plans are derived, not serialized, and rebuild for road/lane/furniture changes. Live vehicles and pedestrians remain checked every move. There are at most eight retained goal fields, each bounded to 32,768 road cells and radius 512; normal manual/legacy vehicles retain their existing routing behavior. Failed curb searches retry at a bounded interval.

Reproduce core benchmarks from OutbreakRebuild:

```sh
dotnet run -c Release --project Tests/CityPerformance.csproj -- --vehicle
dotnet run -c Release --project Tests/CityPerformance.csproj
```

See COURIER_VEHICLE_PERFORMANCE.txt for this session's results. Automated checks cover physical source-to-store credit at two destinations, food conservation, saved phase continuation, pause/resume, full/cancelled returns, driver incapacity, replacement recovery, departure/arrival handling, energy cost and UI controls.

The prototype retains a one-cell vehicle footprint, 16 vehicles and current city limits. Parking occupies a road cell: there are no loading bays, overtaking/deadlock recovery, fuel, multi-cell physics or NPC passenger commuting yet. Vehicles have no damage/combat system in this step; seated occupants are excluded from pedestrian targeting, while couriers on foot use existing danger responses. Native Unity 6000.6.3f1 rendering/play testing and frame rate remain unverified under the standing instruction; the Foundation 82 rollback checkpoint is unchanged.
