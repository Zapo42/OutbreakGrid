using System;
using Outbreak.Contracts;
using Outbreak.Core;
using UnityEngine;

namespace Outbreak.UnityAdapter
{
    // Bit zero is the top-left logical pixel. Masks are presentation, never collision or decision geometry.
    public static class NpcGlyphPresentation
    {
        public const int Size = 3;
        public const int Human = 2 | 56 | 320;       // .#. / ### / #.#
        public const int Zombie = 2 | 24 | 384;     // .#. / ##. / .##
        public const int Corpse = 56;              // ... / ### / ...
        public const int Rising = 2 | 56;          // .#. / ### / ...

        public static int Mask(ActorState actor, double tick, bool riseEnabled)
        {
            if (actor.health <= 0) return Corpse;
            if (actor.species == Species.Human) return Human;
            if (riseEnabled && actor.riseUntilTick > tick)
            {
                double progress = RiseRules.Progress(actor, tick);
                return progress < 1.0 / 3 ? Corpse : progress < 2.0 / 3 ? Rising : Zombie;
            }
            return Zombie;
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

        public static Rect PixelBounds(Rect logicalBody)
        {
            // The mask scales at integer pitch; an undersampled view retains one visible presence mark.
            float available = Math.Min(logicalBody.width, logicalBody.height);
            float side = available < Size ? 1 : Size * (float)Math.Floor(available / Size);
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
                int mask = Mask(actor, tick, riseEnabled);
                float pitch = pixels.width / Size;
                for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    if ((mask & (1 << (y * Size + x))) != 0)
                        GUI.DrawTexture(new Rect(pixels.x + x * pitch, pixels.y + y * pitch, pitch, pitch), Texture2D.whiteTexture);
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
            World preview = new World(new WorldMap(64, 24, "3x3 NPC symbols", "glyph-3x3"), 214, encounter: true, hunger: true);
            int id;
            preview.TrySpawn(Species.Human, 8, 8, out id);
            preview.TrySpawn(Species.Zombie, 16, 8, out id);
            preview.TrySpawn(Species.Human, 24, 8, out id);
            preview.TrySpawn(Species.Human, 32, 8, out id);
            SaveDocument saved = preview.Capture();
            saved.actors[2].health = 0;
            saved.actors[3].turnAtTick = saved.tick + InfectionRules.Delay(saved.actors[3], 1);
            UseWorld(World.Restore(saved)); selected = 0;
            status = "Left to right: human, zombie, corpse, infected human with existing marker. 3x3 symbols; zoom out for distant dots. Starts paused. Glow off.";
        }
    }
}
