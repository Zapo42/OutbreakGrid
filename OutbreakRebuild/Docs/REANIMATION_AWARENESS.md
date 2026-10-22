# Foundation 82: reanimation awareness

A mobile, unarmed survivor whose nearest visible zombie is getting up within three cells prioritizes making space. This takes precedence over supply pickup, food tasks, rest and normal courage decisions. Each observation adds four stress, capped at 100, and uses the existing collision-safe retreat. If cornered, the survivor can use the existing defensive shove, subject to energy and cooldown limits. Escape movement consumes normal movement energy.

Walls block sight. Existing freezes remain in effect. Armed survivors retain their combat decisions and can strike a rising zombie. Infected survivors retain their close-danger escape behavior. Humans act on their normal staggered decision ticks; there is no instant extra movement. Once the zombie finishes getting up, normal threat decisions resume.

## Try it

1. Open and run Foundation, then choose **Inspect Reanimation Escape Paused**.
2. Step twice: the corpse starts its grey-to-green transition and the adjacent survivor backs away.
3. Run to continue, or choose **Restart scenario** to restore the paused starting setup.

**Inspect Rising Zombie Paused** still demonstrates a frozen survivor who cannot immediately escape.

## Existing saves and validation

Foundation 81 saves retain their previous decisions until **Enable reanimation awareness** is selected. Current rules: `encounter-prototype-67`. Save/resume and restart retain existing energy and infection state; no new actor fields or random streams are required.

Automated checks cover escape distance and energy cost, save continuation, wall occlusion, existing freeze behavior, old rules, demo UI, upgrade and restart. Native Unity rendering and Windows builds remain unverified here.
