# Foundation 86: shared storage boxes

Storage boxes are separate from searchable shelves. Each box owns its own saved inventory of typed item stacks and a capacity. This first build supports food; the container contract is independent of room type so other buildings can use it later. Other item types and equipment storage are future work.

## Using a box

- Choose **Place storage box**, then click a free non-door cell. New boxes are empty and hold up to 30 items. They can be placed in any building or outdoors; a shelter is not required for placement.
- Boxes occupy a solid cell. They cannot overlap bodies, reserved shelter destinations, shelves, beds, ground weapons, or doors.
- Click a box or use **Next storage box** to pause, center the camera and inspect its item list and used capacity. Blue-grey bodies with a horizontal stripe distinguish boxes from shelves; selection is yellow.
- The food overview includes stored meals in the remaining total, without counting them as shelf or carried food.
- Save Run and Save Map both retain each box's inventory independently. Restart restores the scenario's original boxes and stock.

## Human behavior

A calm, uninfected shelter occupant carrying surplus meals walks to a visible box in the same room and deposits excess food. The human keeps its normal personal reserve: one meal, or two when badly injured. Existing sharing and direct food delivery still take priority. Box capacity limits leave excess meals in the human's inventory. If no usable box is available, the existing shelf pantry behavior remains available.

Hungry humans with no carried food can walk to a visible stocked box and withdraw up to their carry limit. They eat on a later ordinary action. A shelter occupant uses boxes in the current room; a human outside a shelter can use a visible outdoor box. Boxes do not create food. Transfers use an ordinary action, movement costs normal energy, and threats, infection, unresolved freezes and recent danger warnings interrupt the work. Existing food-risk and rest checks still apply before taking food.

This increment uses locally visible boxes. Persistent box-location memory is a later milestone. [Foundation 87](STORAGE_RESTOCKING.md) adds deliberate supply runs to refill low reserves. Seeing a box does not reveal containers through walls. Empty boxes are not searchable loot sources.

## Controlled examples

**Inspect Storage Deposit Paused** starts with an empty box and a human carrying three meals. Step one tick: the box receives two and the human keeps one.

**Inspect Storage Meal Paused** starts with six meals in a box and a hungry human. Step one tick: three meals move into the human's inventory. Step five more ticks: the human eats one.

## Compatibility and validation

Previous runs retain their rules until **Enable storage boxes** is selected. Rules are now `encounter-prototype-68`; rules 67 are retained as `RiseAwarenessVersion`. Maps containing boxes use schema 8; maps without boxes keep their earlier schema. Earlier builds cannot read box-bearing maps/saves. Keep original saves when using the Foundation 82 rollback checkpoint.

Core checks cover independent inventories, defensive copies, finite capacity, outdoor placement and collision, deposit reserves, withdrawal and eating, competing users, mid-route save/resume, interruption by infection/freeze/danger, malformed inventories and previous rules. UI checks cover placement, inspection and overview totals, focus, tool cancellation, restart, withdrawal, and upgrade without resetting energy.

**Unity validation: untested.** Foundation 82 remains the [last user-tested checkpoint](BUILD_VALIDATION.md).
