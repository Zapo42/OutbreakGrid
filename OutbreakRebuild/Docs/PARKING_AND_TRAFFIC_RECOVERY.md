# Foundation 126: parking bays and traffic recovery

Target: Unity 6000.6.3f1. Native Unity validation remains paused after Foundation 82.

## Try it

- **Inspect Courier Parking Paused**: the home-delivery city has marked off-lane bays near its storage boxes. Store orders retain priority, then assigned home deliveries run. Follow the courier collecting food, driving into a bay, walking to the crate, returning and merging into traffic. The preview keeps normal residents and finite supplies.
- **Inspect Traffic Recovery Paused**: two stationary cars block the upper road of a loop. The selected car waits, then follows the available alternate road. Step 100 ticks or Run; no blockers are removed.
- In **Vehicles**, choose **Place parking bay**, then click clear grass or sidewalk next to an ordinary road outside an intersection. A bay is a single curb pullout with exactly one road connection, drawn as a dark space with pale side markings. Existing terrain and pedestrian walkability are retained; an occupied bay blocks that cell for pedestrians, so place bays with walking space around them. With a storage box selected, **Add parking near selected box** chooses a nearby clear curb cell. Generated cities do not automatically acquire bays in this increment.

## Rules

Couriers prefer declared bays within 48 cells of their destination crate. A bay can be occupied or reserved by only one vehicle. If nearby bays exist but are all occupied, reserved or unreachable, the courier waits and retries every 30 ticks rather than stopping in the travel lane. Maps without nearby bays retain their earlier curb-stop fallback. Manual vehicles can use a bay as a destination too.

A bay connects only to its declared adjacent road cell and cannot become a shortcut between lanes. The car checks for pedestrians, vehicles and nearby traffic before merging back into the lane. Crossing and intersection admission rules still apply. Boarding and unloading remain physical; cargo and reserves retain their existing limits.

After 30 ticks blocked by vehicles, following distance or a blocked intersection exit, a car tries a deterministic alternate road route. The search obeys lane directions, excludes occupied car cells, and is capped at a radius of 64 cells and 4,096 visited cells. An available detour is followed for up to 120 ticks before ordinary routing resumes. Failed searches retry at most every 30 ticks. All movement still passes pedestrian, following-gap and junction checks. Recovery never teleports, moves parked blockers, drives through people or travels against a lane. If there is no legal alternative, waiting is the expected behavior; this is not a general deadlock solver or overtaking system.

Map schema 13 adds a bounded, sorted, defensively copied parking catalog (up to 512 bays). Run rules `encounter-prototype-96` add saved block/retry/detour clocks. Previous rules 95 keep their original traffic behavior until explicitly upgraded through **Enable parking bays and traffic recovery**. Save/load and restart retain authored bays and active jobs. Changing a bay's supporting terrain removes invalid map metadata. Furniture placement cannot overwrite a marked bay in the simulation editor.

Vehicle speed, fuel, bicycles and cosmetic variants are unchanged in this increment. The full requested sequence, including the shopping regression, happiness, leisure and graphics, is recorded in [the city-life workflow](CITY_LIFE_AND_TRANSPORT_PLAN.md).
