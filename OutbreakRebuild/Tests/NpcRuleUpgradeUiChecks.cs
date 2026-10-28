using System;
using System.IO;
using System.Reflection;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
using Outbreak.UnityAdapter;

// Exercise queued production UI commands using the existing doubles, without altering NPC behavior tests.
public static class NpcRuleUpgradeUiChecks
{
    private static int assertions;
    private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.NonPublic;
    private const BindingFlags Static = BindingFlags.Static | BindingFlags.NonPublic;
    private static void Require(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new InvalidOperationException(message);
    }
    private static void Call(FoundationApp app, string method)
    {
        typeof(FoundationApp).GetMethod(method, Instance).Invoke(app, null);
    }
    private static World Current(FoundationApp app)
    {
        return (World)typeof(FoundationApp).GetField("world", Instance).GetValue(app);
    }
    private static bool Paused(FoundationApp app)
    {
        return ((TickClock)typeof(FoundationApp).GetField("clock", Instance).GetValue(app)).Paused;
    }
    private static void Render(FoundationApp app, string click = "")
    {
        typeof(UiHarness).GetMethod("Render", Static).Invoke(null, new object[] { app, click, null });
    }
    private static void Press(FoundationApp app, string label)
    {
        typeof(UiHarness).GetMethod("Press", Static).Invoke(null, new object[] { app, label });
    }
    private static string State(World world) { return Documents.Encode(world.Capture()); }

    public static int Main(string[] args)
    {
        string path = args.Length > 0 ? args[0] : "OutbreakRebuild/CityReferences/districts/houses/suburban-grid-214-9cea487c8d68.obmap2";
        string mapJson = File.ReadAllText(path);
        string[] versions = { EncounterRules.LeisureVersion, EncounterRules.HappinessVersion, EncounterRules.OffDutyConsumersVersion,
            EncounterRules.ParkVisitsVersion, EncounterRules.SocialInteractionsVersion, EncounterRules.ParkPreferencesVersion,
            EncounterRules.SocialFamiliarityVersion };
        string[] labels = { "Enable happiness", "Enable off-duty consumer routines", "Enable known park visits",
            "Enable social greetings", "Enable park preferences", "Enable familiar greetings", "Enable needs priority" };
        for (int n = 0; n < versions.Length; n++)
        {
            WorldMap map = Documents.ParseMap(mapJson);
            World authored = new World(map, 214, encounter: true, hunger: true);
            BuildingRoomDocument anchor = map.BuildingRoomGroups()[0].rooms[0];
            int actorId;
            Require(authored.TrySpawn(Species.Human, anchor.x, anchor.y, out actorId), "Could not place house resident.");
            Require(authored.TryAssignHouse(0, anchor.x, anchor.y), "Could not assign grouped house.");
            SaveDocument initial = authored.Capture();
            initial.rulesVersion = versions[n];
            initial.actors[0].energySpent = 37; // A real modern energy value must survive restart, not be converted from zero shove fatigue.
            World legacy = World.Restore(initial);
            string initialState = State(legacy);
            FoundationApp app = new FoundationApp();
            Call(app, "Awake");
            try
            {
                typeof(FoundationApp).GetMethod("UseWorld", Instance).Invoke(app, new object[] { legacy });
                Render(app);
                Require(State(Current(app)) == initialState && !Current(app).NeedsPriorityEnabled, "Loading/rendering silently upgraded old rules.");
                Require(UnityEngine.GUILayout.controls.ContainsKey(labels[n]), "Missing explicit upgrade for " + versions[n]);
                Press(app, "Step 1 tick");
                Press(app, "Restart scenario");
                Require(State(Current(app)) == initialState && Paused(app), "Legacy restart changed energy, map or saved facts for " + versions[n]);
                // Retain an old-rule scenario checkpoint while upgrading an already advanced run.
                Press(app, "Step 1 tick");
                string before = State(Current(app));
                SaveDocument expected = Current(app).Capture();
                expected.rulesVersion = EncounterRules.Version;
                Render(app, labels[n]);
                Require(State(Current(app)) == before, "Upgrade mutated simulation during GUI traversal.");
                Call(app, "Update");
                Require(Current(app).NeedsPriorityEnabled && Paused(app), "Queued upgrade did not enable rules107 while paused.");
                Require(State(Current(app)) == Documents.Encode(World.Restore(expected).Capture()), "Upgrade changed inventory, energy, map, clock or RNG.");
                Press(app, "Step 1 tick");
                Press(app, "Restart scenario");
                initial.rulesVersion = EncounterRules.Version;
                Require(State(Current(app)) == Documents.Encode(World.Restore(initial).Capture()) && Paused(app), "Upgraded restart converted modern energy or lost original house state.");
                Render(app);
                Require(!UnityEngine.GUILayout.controls.ContainsKey("Enable needs priority"), "Current rules still offer needs upgrade.");
            }
            finally { Call(app, "OnDestroy"); }
        }
        FoundationApp preview = new FoundationApp();
        Call(preview, "Awake");
        try
        {
            Press(preview, "Inspect Needs Priority Paused");
            World world = Current(preview);
            string start = State(world);
            Require(world.NeedsPriorityEnabled && Paused(preview) && world.Tick == 600, "Needs preview did not start paused at its authored shift boundary.");
            Require(world.ActorAt(0).carriedFood == 1 && world.ActorAt(1).carriedFood == 1 && world.ActorAt(0).hunger == 60, "Needs preview changed physical meal inputs.");
            Render(preview);
            Require(State(world) == start, "Preview rendering advanced NPC decisions.");
            Press(preview, "Step 10 ticks");
            Require(world.ActorAt(0).carriedFood == 0 && world.ActorAt(1).carriedFood == 0 && world.ActorAt(0).hunger < 60 && world.ActorAt(1).hunger < 60,
                "Preview tick commands did not consume the existing carried meals.");
            Press(preview, "Restart scenario");
            Require(State(Current(preview)) == start && Paused(preview), "Needs preview restart lost its authored finite inventory.");
        }
        finally { Call(preview, "OnDestroy"); }
        Console.WriteLine("PASS rules100-106 explicit upgrades/restarts with schema14 houses and needs preview: " + assertions + " assertions. UI API doubles; native Unity NOT RUN.");
        return 0;
    }
}
