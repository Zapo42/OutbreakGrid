# Foundation 81: reanimation color transition

Reanimated corpses fade from grey to zombie green over five ticks (one simulation second). They cannot move or attack during this get-up delay. There is no pose animation. Living infected humans who turn while standing do not receive this delay.

The reanimated zombie starts with 50 health and counts as a zombie during the transition. Humans can damage or kill it during this time. When the delay ends it resumes its normal action cadence. Health bars remain visible; selection retains a thin yellow border.

## Try it

1. Open and run the Foundation project in Unity.
2. Choose **Inspect Rising Zombie Paused**.
3. Click **Step 1** to reanimate the corpse. The inspector shows **Reanimating: getting up (1.0s)**.
4. Click **Run** to see the fade, or step individual ticks. At tick six this test zombie finishes getting up and attacks its frozen neighbor.
5. **Restart scenario** restores the original corpse and paused setup.

The fade pauses with simulation time. Saving midway preserves the transition deadline. Death clears it.

## Existing saves

Foundation 80 saves retain immediate reanimation until **Enable reanimation transition** is selected. Existing active zombies do not receive a new delay. Current rules: `encounter-prototype-66`.

## Validation

Core checks cover delayed actions, standing conversions, mid-transition save/resume, damage and death, invalid saved deadlines, and previous-version compatibility. UI checks cover grey and intermediate colors, countdown, restart, and upgrading a prior run. These use API doubles; native Unity rendering and Windows builds have not been verified here.
