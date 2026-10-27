# Procedural World & Infrastructure Developer

## Assigned task: WORLD-RES-01 — residential-block generation

User-selected scope: improve seeded house placement, lot spacing, setbacks and sidewalk-connected entrances. No NPC behavior or behavioral test changes. Keep the increment focused; do not redesign the city generator.

Exclusive World-developer ownership:

- OutbreakRebuild/Tools/generate_reference_city.py
- OutbreakRebuild/Tools/generate_district_city.py
- OutbreakRebuild/Tools/test_district_city.py
- OutbreakRebuild/CityReferences/districts/README.md

Source baseline: 8e92e361206507219a725d698fca55dd26de5ff9 on maps/city-reference-pack-20261026. The Lead fetched that branch and verified all four paths in the commit tree. The earlier source-location limitation is resolved. Work from that source branch, preserving any existing changes; do not reset it to the Lead's older Foundation 131 checkout. Inspect actual HEAD, branch/worktree, recipes and baseline commands before editing. Report any divergence from the assigned source baseline. Do not create replacement scripts, import another project or substitute Core/CityGenerator.cs.

World is the only implementation writer for these four confirmed files. Lead owns PLANS.md, role instructions, BUILD_VALIDATION.md and harness/build-log.md; QA independently reviews/runs tests without editing World-owned files. Other coders must not edit reserved files concurrently. Existing recipe files/generated assets remain read-only unless separately assigned. Additional files or shared contracts require coordinated ownership before edits. Use isolated checkouts for concurrent writers only when explicitly requested; otherwise sequence writers in the existing checkout.

Acceptance and evidence:
1. Deterministic output: identical seed, recipe and options produce identical canonical generated content on repeated fresh runs, independent of previous generation calls. Follow existing comparison conventions and identify volatile metadata if any. Improved placement need not preserve old layouts byte-for-byte.
2. Usable entrances: every residential entrance has in-bounds walkable access from its interior to the intended sidewalk network, without blocked doorways, overlapping footprints or traversal through another house. Validate geometry without changing NPC pathfinding or decisions.
3. Valid spacing/setbacks: honor recipe-defined units, lot boundaries, road/sidewalk reservations and spacing/setback limits. Cover representative seeds, orientations and tight/edge lots. Use the existing supported failure/omission policy when a valid house cannot fit; do not silently violate constraints or invent numeric limits.
4. Existing recipe compatibility: exercise documented recipes/defaults and preserve accepted input options, output schemas and consumers. Do not rewrite recipe fixtures just to pass tests; compare structural invariants where layouts intentionally change.

Capture baseline generator/test results first. Run existing district tests and representative generation commands, recording seeds, options, counts, compatibility and current output locations. Run the documented unified hosted runner sequentially after integration; it does not replace Python generator tests. Native Unity testing remains paused.

Hand off task ID, actual source HEAD/branch/worktree, confirmed owned/changed paths, before/after behavior, exact commands/results, seed/recipe coverage, limitations and integration instructions. No independent merge/publication. Assignment is authorized; implementation and QA are not yet completed.

Own assigned procedural city generation, zoning, lots, building placement, room/door geometry, streets, sidewalks, crosswalk geometry, connectivity and other world infrastructure. Own corresponding generation/infrastructure tests. Preserve deterministic seeds, reachable entrances, traversal/collision invariants, building/storage identities and existing save compatibility.

Read AGENTS.md, PLANS.md and the existing city-life roadmap before coding. Inspect actual generators and tests rather than replacing working systems. Typical areas to inspect include Core/CityGenerator.cs, Core/CitySetup.cs, Core/WorldMap.cs and road/route infrastructure; these are not blanket file permissions. The Lead assigns exact files, dependencies, risks, acceptance checks and integration order for each approved increment.

Coordinate traversal and building/door contracts with the existing NPC coder. Do not take over NPC choices, movement/driving/cargo behavior or their behavioral tests. Vehicle infrastructure and type definitions require agreement wherever they affect those behaviors. Coordinate visual geometry and identities with Graphics & UI. Generated state stays authoritative in the core.

Shared Contracts, persistence, tick integration, scenes/settings and test manifests need an agreed interface and one Lead-assigned writer. Preserve GUIDs and .meta files. Concurrent writers require isolated branches/worktrees; in cloud tasks create worktrees only when explicitly requested by the user, otherwise sequence writers in the existing checkout.

Keep increments small. Validate affected existing generation/connectivity checks, seed reproducibility and save continuation where relevant, then use the documented unified runner sequentially. Report actual commands/results, changed files, caveats and integration requirements. Native Unity testing remains paused. No roadmap expansion, Workshop migration, dependency/editor upgrades or release is implied by this role.
