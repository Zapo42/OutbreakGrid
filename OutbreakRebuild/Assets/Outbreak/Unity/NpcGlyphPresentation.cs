using System;
using Outbreak.Contracts;
using Outbreak.Core;
using UnityEngine;

namespace Outbreak.UnityAdapter
{
    // Bit zero is the top-left logical pixel. Masks are presentation, never collision or decision geometry.
    public static class NpcGlyphPresentation
    {
        public const int Width = 5;
        public const int Height = 5;
        public const int Human = 14 | (14 << 5) | (31 << 10) | (14 << 15) | (10 << 20); // .###./.###./#####/.###./.#.#.
        public const int Zombie = Human;
        public const int Corpse = 14 << 15;
        public const int Rising = (4 << 5) | (14 << 15) | (10 << 20);

        public static int Tier(Rect logicalBody)
        {
            // Select a raster that fits at unit pitch, then enlarge its square pixels at close zoom.
            float available = Math.Min(logicalBody.width, logicalBody.height);
            return available < 3 ? 1 : available < 5 ? 3 : 5;
        }

        public static int Mask(ActorState actor, double tick, bool riseEnabled, int size = 5)
        {
            if (size == 1) return 1; // Species/state ink still distinguishes distant actors.
            int corpse = size == 3 ? 7 << 3 : Corpse;
            int standing = size == 3 ? 2 | (2 << 3) | (5 << 6) : Human;
            int rising = size == 3 ? (2 << 3) | (5 << 6) : Rising;
            if (actor.health <= 0) return corpse;
            if (actor.species == Species.Human) return standing;
            if (riseEnabled && actor.riseUntilTick > tick)
            {
                double progress = RiseRules.Progress(actor, tick);
                return progress < 1.0 / 3 ? corpse : progress < 2.0 / 3 ? rising : standing;
            }
            return standing;
        }

        public static Color Ink(ActorState actor, double tick, bool riseEnabled)
        {
            // Retain current species/death colors and the existing grey-to-green rise feedback.
            if (actor.health <= 0) return new Color(0.4f, 0.4f, 0.4f);
            if (actor.species == Species.Human) return new Color(0.75f, 0.8f, 0.92f);
            if (!riseEnabled || actor.riseUntilTick <= tick) return new Color(0.23f, 0.29f, 0.14f);
            float progress = (float)RiseRules.Progress(actor, tick);
            return new Color(Mathf.Lerp(0.4f, 0.23f, progress), Mathf.Lerp(0.4f, 0.29f, progress), Mathf.Lerp(0.4f, 0.14f, progress));
        }

        public static Color PixelInk(ActorState actor, double tick, bool riseEnabled, int size, int mask, int x, int y)
        {
            Color body = Ink(actor, tick, riseEnabled);
            // Only the close standing raster carries face details; distant/corpse/crouched masks keep state ink.
            if (size != 5 || mask != Human || y > 1) return body;
            Color detail = y == 1 && x != 2 ? new Color(38 / 255f, 51 / 255f, 59 / 255f)
                : actor.species == Species.Human ? new Color(198 / 255f, 199 / 255f, 174 / 255f)
                : new Color(135 / 255f, 148 / 255f, 77 / 255f);
            if (actor.species == Species.Zombie && riseEnabled && actor.riseUntilTick > tick)
            {
                float progress = (float)RiseRules.Progress(actor, tick);
                return new Color(Mathf.Lerp(0.4f, detail.r, progress), Mathf.Lerp(0.4f, detail.g, progress), Mathf.Lerp(0.4f, detail.b, progress));
            }
            return detail;
        }

        public static Rect PixelBounds(Rect logicalBody)
        {
            int size = Tier(logicalBody);
            float pitch = size == 1 ? 1 : (float)Math.Floor(Math.Min(logicalBody.width, logicalBody.height) / size);
            float side = size * pitch;
            return new Rect((float)Math.Floor(logicalBody.x + (logicalBody.width - side) / 2 + 0.5f),
                (float)Math.Floor(logicalBody.y + (logicalBody.height - side) / 2 + 0.5f), side, side);
        }

        public static void Draw(Rect logicalBody, ActorState actor, double tick, bool riseEnabled, Color? flash = null)
        {
            Rect pixels = PixelBounds(logicalBody);
            Color previous = GUI.color;
            GUI.color = flash ?? Ink(actor, tick, riseEnabled);
            if (pixels.width == 1) GUI.DrawTexture(pixels, Texture2D.whiteTexture);
            else
            {
                int size = Tier(logicalBody);
                int mask = Mask(actor, tick, riseEnabled, size);
                float pitch = pixels.width / size;
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    if ((mask & (1 << (y * size + x))) != 0)
                    {
                        GUI.color = flash ?? PixelInk(actor, tick, riseEnabled, size, mask, x, y);
                        GUI.DrawTexture(new Rect(pixels.x + x * pitch, pixels.y + y * pitch, pitch, pitch), Texture2D.whiteTexture);
                    }
            }
            GUI.color = previous; // Do not leak actor ink into terrain, occupations, bars or subsequent actors.
        }

        public static float SelectionPadding(float cell) { return Mathf.Clamp(cell * 0.035f, 1, 2) + 1; }
        public static void DrawSelection(Rect body, float cell)
        {
            // Leave the existing occupation band intact between the body and selection frame.
            float pad = SelectionPadding(cell);
            Rect frame = new Rect(body.x - pad, body.y - pad, body.width + 2 * pad, body.height + 2 * pad);
            Color previous = GUI.color; GUI.color = Color.yellow;
            GUI.DrawTexture(new Rect(frame.x, frame.y, frame.width, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(frame.x, frame.y + frame.height - 1, frame.width, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(frame.x, frame.y + 1, 1, frame.height - 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(frame.x + frame.width - 1, frame.y + 1, 1, frame.height - 2), Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }

    public sealed partial class FoundationApp
    {
        private void StartNpcGlyphPreview()
        {
            // This creates finite paused test data. Core alone decides all subsequent behavior.
            World preview = new World(new WorldMap(64, 24, "NPC zoom sprites", "glyph-3x3"), 214, encounter: true, hunger: true);
            int id;
            preview.TrySpawn(Species.Human, 8, 8, out id);
            preview.TrySpawn(Species.Zombie, 16, 8, out id);
            preview.TrySpawn(Species.Human, 24, 8, out id);
            preview.TrySpawn(Species.Human, 32, 8, out id);
            SaveDocument saved = preview.Capture();
            saved.actors[2].health = 0;
            saved.actors[3].turnAtTick = saved.tick + InfectionRules.Delay(saved.actors[3], 1);
            UseWorld(World.Restore(saved)); selected = 0;
            status = "Left to right: human, zombie, corpse, infected human with existing marker. 5x5 close / 3x3 medium / 1x1 distant. Zoom to compare. Starts paused. Glow off.";
        }
    }
}
