# Foundation 35: food route choices

When choosing a new shelf, humans compare bounded collision-safe routes to adjacent search positions. A locally reachable shelf takes priority over one with no complete local route. Among reachable shelves, confirmed food still takes priority over unknown supplies; equal-confidence shelves are ranked by route steps, with stable coordinate order resolving ties.

This uses the existing 33-by-33 local pathfinder, including terrain, crowding, doors and observed-threat clearance. Diagonal moves count as one step. Opening a closed door still consumes an additional action, so this is route-length comparison rather than a full travel-time forecast. If no known shelf has a complete local route, the earlier remembered-distance and waypoint fallback remains available, together with blocked-route memory. Ongoing usable trips retain their target to avoid repeated switching.

The choice reads personal shelf notes, never unopened quantities. Search coordination also compares reachable alternative path lengths. No extra actor state or random draws are introduced.

Choose **Inspect Food Route Choices Paused**. The remembered shelf at 8,3 is physically closer but needs a long detour around a wall. **Step 1 tick** chooses the shorter walk to 3,7. **Step 50 ticks** shows food collected and eaten from that shelf while the other remains stocked.

New encounters use encounter-prototype-31. Earlier saves retain their prior distance preference until **Enable food route choices** (or an earlier-feature upgrade button) is clicked. The previous blocked-route inspection scenario keeps its authored earlier rules to isolate actual route-failure memory; the new scenario demonstrates planning before committing to a trip.

Native Unity rendering and input require local testing. Automated checks execute production simulation and UI handlers with narrow Unity API doubles.

Validation: 217 simulation cases / 35,132 assertions; 558 legacy-input UI assertions and 3 NewOnly assertions. Project configuration and whitespace checks passed.
