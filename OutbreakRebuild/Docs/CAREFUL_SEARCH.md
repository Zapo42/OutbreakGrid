# Foundation 27: cautious searching

Shelf searches take three normal human actions. The first two actions make search noise but reveal no stock and transfer no food. The third transfers the available food, marks the shelf searched, records the actual remaining stock and clears progress. Eating remains a later action.

The existing personal risk assessment applies before every action: perceived stress, recent warnings, door impacts and visible zombies are compared with hunger and bravery. Close visible zombies always prevent searching. When the human cannot accept the risk, its partial work is cancelled and survival behavior takes over. Once risk is acceptable again, searching starts from zero. No hidden zombie locations or unopened shelf quantities affect this decision.

Search progress is saved against the existing shelf target, validates at restore, clears when the target changes or expires, and clears on infection conversion. Progress is shown in the human inspector.

Choose **Inspect Careful Search Paused**, select human #1, then **Step 1 tick**: progress is 1/3 with no food in inventory. **Step 10 ticks** completes the third action and collects three meals. A further eating action reduces hunger.

New encounters use encounter-prototype-26. Old saves retain their prior timing until **Enable cautious searching** (or an earlier-feature upgrade button) is clicked. Earlier inspection scenarios and their single-action acceptance controls retain their authored rules to isolate their original milestones; use the new scenario for multi-action searching.

Native Unity visuals and input require local testing. Automated checks execute production simulation and UI handlers with narrow Unity API doubles.

Validation: 199 simulation cases / 35,064 assertions; 501 legacy-input UI assertions and 3 NewOnly assertions. Project configuration and whitespace checks passed.
