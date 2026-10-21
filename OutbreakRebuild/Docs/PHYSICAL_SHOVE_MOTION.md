# Foundation 69 / physical shove motion

Shoves now have a dedicated contact-and-recoil sequence during playback. The human leans forward with both hands; the zombie closes the visible gap before recoiling quickly. This replaces the slow walking interpolation previously used for a pushed zombie, which could make the push appear to happen before contact.

The visual approach lasts 0.6 ticks, followed by a 1.2-tick recoil. The human settles out of its lean within 2.4 ticks. Stable body offsets are included when aligning visible contact. Blocked pushes retain their verified cell position and still show the human's push. Compound moves retain the existing safe synchronization rather than animating an unverified path.

Choose **Inspect Shove Contact Paused**, then **Run** to watch the sequence. Pause and manual stepping show exact authoritative positions. Restart repeats the setup. No rules upgrade is required: shove range, cooldown, nonlethal health behavior, crowd limits and collision remain unchanged.

Shove events are transient presentation records. They are cleared each tick and are not replayed after loading. Animation state also clears on pause and restart. Automated checks cover actual contact events, no damage or added randomness, saved-state preservation, contact-before-recoil timing, the human lean, rendered contact alignment, hands, recoil, pause and restart. Native Unity appearance and desktop input require local verification.

Validation: 324 core cases / 320,893 assertions; 1,124 UI harness assertions plus 3 with New Input only. Project configuration and whitespace checks passed.
