using System;
using System.Reflection;
using Outbreak.Core;
using Outbreak.Persistence;
using Outbreak.UnityAdapter;
using UnityEngine;

// Reuse UiHarness's narrow API doubles to exercise the actual production renderer.
public static class BuildingRoomUiChecks
{
    private static int assertions;
    private static void Require(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new InvalidOperationException(message);
    }

    public static int Main()
    {
        foreach (bool area in new[] { false, true })
        foreach (bool bed in new[] { false, true })
        {
            WorldMap map = new WorldMap(12, 12);
            for (int y = 0; y < 12; y++)
            for (int x = 0; x < 12; x++)
                map.Set(x, y, x == 0 || y == 0 || x == 11 || y == 11 ? Terrain.Wall : Terrain.Floor);
            if (area) map.SetRestSpot(2, 2, true);
            if (bed) map.SetRestSpot(5, 5);
            World world = new World(map, 214, encounter: true, hunger: true);
            string before = Documents.Encode(world.Capture());
            FoundationApp app = new FoundationApp();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(FoundationApp).GetMethod("Awake", flags).Invoke(app, null);
            try
            {
                typeof(FoundationApp).GetMethod("UseWorld", flags).Invoke(app, new object[] { world });
                typeof(FoundationApp).GetField("span", flags).SetValue(app, 12f);
                GUI.draws.Clear();
                typeof(FoundationApp).GetMethod("DrawWorld", flags).Invoke(app, new object[] { new Rect(0, 0, 480, 480) });
                // Each ordinary bed draws three layers; recovery-area cells must draw none.
                foreach (Color color in new[] { new Color(0.38f, 0.30f, 0.23f), new Color(0.28f, 0.58f, 0.62f), new Color(0.86f, 0.87f, 0.82f) })
                {
                    int count = GUI.draws.FindAll(draw => draw.color.r == color.r && draw.color.g == color.g && draw.color.b == color.b).Count;
                    Require(count == (bed ? 1 : 0), "Recovery area drew bed furniture, or an ordinary bed lost a layer.");
                }
                Require(Documents.Encode(world.Capture()) == before, "Drawing changed authoritative map/save state.");
                Require(world.RestSpotSites().Length == (area ? 1 : 0) + (bed ? 1 : 0), "Rendering removed recovery markers.");
            }
            finally
            {
                typeof(FoundationApp).GetMethod("OnDestroy", flags).Invoke(app, null);
            }
        }
        Console.WriteLine("PASS house recovery-area/ordinary-bed production drawing: " + assertions + " assertions. API doubles only; native Unity rendering/input NOT RUN.");
        return 0;
    }
}
