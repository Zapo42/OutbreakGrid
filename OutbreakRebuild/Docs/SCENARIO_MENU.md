# Foundation 128: organized test menu

Target: Unity 6000.6.3f1. Native Unity testing remains paused after Foundation 82.

The Main Menu keeps city/encounter setup, quick-start Encounter, load actions and Resume current run directly accessible. **Current Tests** lists these recent previews:

- Large City Shopping
- Courier Parking
- Traffic Recovery
- Home Vehicle Deliveries
- Vehicle Deliveries
- Expanded City

Earlier behavior previews and the original Movement Test are under **Show Old Test Scenarios**. This collection starts collapsed. Open it to launch a historical scenario, or choose **Hide Old Test Scenarios** to shorten the menu again. Its open/closed state is retained while using the app; it is presentation state and does not enter a saved run.

All scenario launch handlers, authored setups, starting pause states, replacement confirmation, restart behavior and labels are retained. Historical previews are moved, not removed. Opening or closing the collection does not replace, advance or edit a running simulation. Current tests remain visible in either state. Existing Load Map/Load Run actions keep their native dialog dispatch behavior.

This build changes menu presentation only. Consumer vehicle shopping with larger cargo and finite carts remains the next transport task in [the development workflow](CITY_LIFE_AND_TRANSPORT_PLAN.md).

Automated validation: 2284 UI assertions and 3 NewOnly input assertions passed. All literal button labels were checked against the original menu during extraction. Core behavior is unchanged; native Unity layout remains unverified.

Foundation 129 adds **Inspect Vehicle Shopping Paused** to Current Tests. Historical previews remain in the same collapsed collection.
