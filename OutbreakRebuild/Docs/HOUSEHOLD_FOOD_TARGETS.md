# Foundation 96 — household food targets

A house can now reserve a chosen number of meals for each assigned living resident. Away residents and couriers count toward their own household. Visitors do not. Infected living residents count until death or conversion. Reassigning a resident changes both houses' targets immediately.

## Enable and adjust

1. Open **Household management**, then **Manage house #…**.
2. Click **Use 3 meals per resident** to enable the policy for that house.
3. Use **More meals per resident** or **Fewer meals per resident** to choose 1–12 meals per person. Edits pause the run.
4. **Use manual box targets** disables the household policy and restores the boxes' underlying reserve settings.

Household food policies are opt-in. Existing saves and houses keep their current box targets unless enabled. Saves from Foundation 94/95 need **Enable household food targets** first. The policy panel shows assigned residents, requested meals, allocated box reserves, stored meals, reserve deficits and unallocated meals.

## How boxes share the target

For two residents at three meals each, the requested household reserve is six meals total, not six meals in every box. Automatic boxes share the remaining target as evenly as capacities allow; earlier cells receive any remainder. Small boxes are capped and their unused share goes to other automatic boxes.

A box with a custom reserve remains a manual override. Manual reserves are subtracted from the household requirement before the remainder is shared among automatic boxes. A manual zero keeps deliberate restocking off for that box. Explicit manual targets can exceed the requested household total and are still protected.

Select a box and apply a reserve to override its automatic share. Even an explicit six-meal reserve is preserved as an override while the household policy is enabled. **Use household target** clears that override and returns the box to the automatic allocation. **Default reserve** sets the ordinary six-meal manual reserve; it does not clear an override while the policy is active.

If boxes cannot cover the requested reserve, **Unallocated meals** shows the uncovered amount. Add capacity or release manual overrides to let automatic allocation cover it. Houses with assigned residents remain in the shortage overview while everyone is away, including when the gap comes from insufficient usable capacity.

## Gameplay and persistence

Resident restocking and courier destination selection use the effective box reserves. Donor selection and actual pickup protect the same reserves, including changes made while a courier is travelling. **Block donations** continues to apply. Collected cargo is retained under the existing return rules. Reserve targets are minimum replenishment goals, not strict inventory ceilings: ordinary surplus deposits may exceed them up to box capacity.

This reserves food in storage boxes. Carried meals and food on shelves retain their existing behavior and do not reduce the household's box reserve requirement. Stock remains finite and food-only. Targets use household membership and settings, not hidden shelf or box inventories; humans still rely on their own observations for supply jobs. Player shortage totals do not enter human decisions.

Policies persist in running saves and scenario restart checkpoints under encounter rules 76. Household membership and policy settings belong to the running scenario; reusable terrain-map exports do not include those assignments or policies. Disabling the policy does not move food, remove residents, rewrite courier routes or erase manual box settings.

## Example and validation

Open **Inspect Household Food Targets Paused**. House #183 has two assigned residents, one away, and two automatic boxes: six requested meals are split three per box. House #192 has one assigned resident and ignores its visiting courier when computing its three-meal goal. Inspect either box to override its share, then restore its household target.

Automated checks cover allocation, stable remainders, capacity redistribution, overrides, membership changes, conversion, saved continuation, resident restocking, donor protection, invalid/legacy policies, shortages while residents are away, and UI adjustment/upgrade/restart paths. Native Unity remains untested after Foundation 82. The rollback tag remains `foundation-82-unity-checkpoint`.
