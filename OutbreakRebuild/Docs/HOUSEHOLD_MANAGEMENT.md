# Foundation 95 — household management

Open **Household management** in the simulation. It pauses the run and lists every discovered house, including empty houses. Each row shows assigned residents, residents physically home, residents away, visitors, storage-box count and shelter capacity. Capacity remains admission guidance; it does not prevent a player from assigning a larger household.

## Change a resident's house

1. Click **Manage house #…** to focus that house and display its roster.
2. Click **Select resident #…** beside a roster member. This pauses and focuses the human at their actual location.
3. Click **Assign selected resident to house #…** beside the desired house.

Use **Next unassigned resident** to cycle through living humans without a home, then assign the selected human to a house. You may also select a human directly in the simulation and use the household list. Houses without storage boxes can still receive residents.

Home assignment changes do not teleport humans, move inventory, or change courier source/destination boxes. Residents travel normally under the existing hunger, energy and danger rules. A roster member remains assigned to their own house while visiting another house, travelling outdoors or making deliveries. Physical visitors are counted separately. The existing shelter food overview continues to count physical occupants rather than assigned residents.

Active collection/delivery or ordinary restocking tasks must finish before their human can be reassigned. Infected humans cannot be reassigned. Couriers retain their route when moved to another household. **Unassign selected resident** removes a home assignment for an idle resident; a courier's source and destination route must be cleared first. Removing a home while its resident is returning cancels travel forced by that old home assignment. Ordinary shelter and survival behavior remains available.

Membership uses the saved home assignments introduced in Foundation 94. It survives save/reload and the existing scenario restart checkpoint behavior. This build does not change the encounter rules version. Saves from before courier assignments still need **Enable courier assignments**. Household management is a player view; its counts and roster locations do not grant humans hidden supply or threat knowledge.

## Example and checks

Open **Inspect Households Paused**. House **#183** has two assigned residents: one home and a courier visiting house **#192**. House #192 has its own resident and counts that courier as a visitor. A fourth human outdoors is unassigned. House **#222** starts empty and is available for reassignment.

Try moving the courier to house #222: their source and two destination boxes remain intact, and their body stays where it is. Try selecting an unassigned resident and assigning a home. Save/reload, step, and restart to inspect checkpoint behavior. Completed tasks are required before further home changes.

Automated checks cover roster membership, home/away/visitor counts, independent snapshots, preserved cargo and routes, saved continuation, home removal, busy/infected editing restrictions, conversion cleanup, legacy rules and UI controls. Native Unity remains untested after Foundation 82; `foundation-82-unity-checkpoint` is unchanged.
