# Foundation 100 — encounter navigation and inspection

The simulation sidebar is reorganized into collapsible sections. **Expand all sections** opens everything, including management panels; **Collapse all sections** closes it. Essential playback, stepping, file, restart and menu controls remain on the top toolbar.

## Clicked items first

The **Selected item** section appears first. Clicking a different human, zombie, shelf, crate, phone or ground weapon brings its inspection back to the top of the sidebar and opens the relevant item details. It does not advance simulation time.

Human/zombie identity and position appear first, followed by a concise health/energy, activity and target summary. Clerk phone activity and pending orders are visible there. Further actor information is grouped under:

- **Vitals & needs** — health, infection, hunger, carried food, energy and recovery.
- **Combat & contact** — weapon condition, bite/shove feedback and combat experience.
- **Assignments & actions** — courier routes/shifts, clerk and distribution-center actions, active supply tasks.
- **Memories & traits** — remembered supply locations, reports, exploration history and saved traits.
- **Recent signals** — warnings, heard impacts and nearby door conditions.

Crate inventory and reserve controls appear with the clicked crate. Store/center configuration actions are in its **Assignments & actions** section. Shelves show stock/search state. Phones show their location and any pending order made from that phone.

## Encounter and observation sections

**Encounter summary** holds population totals, conversion/corpse counts and population history. **Event observation** holds next fight, injured/infected human focus, soonest turn and turn-watch tools. Focusing an observed event reveals relevant actor inspection sections rather than leaving its details buried.

Other groups separate **Food & shelves**, **Shelter supplies**, household/store/distribution-center management, **Placement tools**, **Weapon supplies**, **Map markers & navigation**, and **Technical details**. Existing management toggles remain their collapsible entry points. Placement cancellation remains accessible even if its tools section is closed.

Section choices last for the current app session and do not alter saved simulation state. A new selection reopens its item summary and vitals; event focus can reveal combat/recent-signal details. Player inspection does not grant NPCs hidden knowledge or change their inventories, random streams or timers.

The UI harness checks compact defaults, individual folds, expand/collapse-all, selection order, phone/item clicks, order observation, placement dispatch and read-only folding. Existing interaction checks run in expanded sections to verify that prior controls remain functional after relocation. These use API doubles; native Unity layout, scrolling and visual readability still require a later Unity pass under the user's paused-testing instruction.
