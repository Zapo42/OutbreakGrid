# Foundation 90: shared box locations

A calm or aware, fed, uninfected human can spend one ordinary action telling an adjacent visible human about a storage box they personally observed. Hungry recipients are prioritized, followed by stocked boxes and fresher observations. Existing survival actions and actual meal transfers retain priority. Infection, freezing, recent warnings or observed zombies prevent the conversation. A recipient who can already see the box does not need the report.

## Dated information

A report copies the box location, last-seen food, free space and original observation tick. It records the speaker as the source; delivery does not refresh observation age. A reported box can guide the recipient's normal food, storage and restocking behavior through the existing memory system.

Fresher recipient knowledge wins. Unchanged information is not repeated unless the source observation is at least 300 ticks fresher; changed stock can be reported sooner. Empty-box reports are useful and can correct an older belief that food remains. A full eight-box notebook rejects a report it could not retain under the existing age/distance rules.

Only firsthand observations are shared. Hearsay cannot be relayed. When a recipient later sees or uses the box, their fresh personal observation replaces the report source. The same six-game-hour expiry is measured from the original observation, including for received reports. Hidden current stock is never substituted into a conversation.

## Inspect it

Choose **Inspect Box Sharing Paused**. Human #1 remembers a box behind the wall; adjacent hungry human #2 does not. Step one tick to deliver the report. The recipient inspector shows **Told by human #1** and the source observation's age. Run or step 100 ticks to watch #2 walk around the wall, collect three meals and eat one later. Its record then shows **Personally observed**. Restart restores the checkpoint before the conversation.

## Compatibility and validation

Foundation 89 runs retain private box memory until **Enable box reports** is selected. Rules are now `encounter-prototype-72`; generation 71 remains `BoxMemoryVersion`. Optional observation provenance is saved without changing map schema or inventory format. Older box-memory rules reject saves containing reported box knowledge.

Core checks cover actual information delivery and action use, hungry-recipient routing, exact save continuation, safety/adjacency restrictions, stale/empty/fresh reports, no hearsay relay or unnecessary repeats, hidden stock privacy, original-age expiry, bounded notebooks, malformed provenance and old rules. UI checks cover report source and age, read-only rendering, firsthand refresh, restart and upgrade without losing energy.

**Unity validation: untested.** Foundation 82 remains the [last user-tested checkpoint](BUILD_VALIDATION.md).

Storage remains food-only. Supply priorities are a separate possible milestone: assigning restocking effort preferentially to shelters with hungry occupants or fewer meals per person. That work has not been selected yet.
