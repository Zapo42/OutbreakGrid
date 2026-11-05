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
        // Only the feet move; head, body, colors and logical anchors remain shared across species.
        public const int WalkLeft = (Human & ~(31 << 20)) | (6 << 20);  // .##..
        public const int WalkRight = (Human & ~(31 << 20)) | (12 << 20); // ..##.
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

        private static double Linear(float v) { return v <= 0.04045f ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4); }
        private static double Luminance(Color c) { return 0.2126 * Linear(c.r) + 0.7152 * Linear(c.g) + 0.0722 * Linear(c.b); }
        private static readonly double blueLight=Luminance(TerrainPalette.WaterBlue),blueDark=Luminance(TerrainPalette.StoreBlue),
            greenLight=Luminance(TerrainPalette.Olive),greenDark=Luminance(TerrainPalette.Grass),grayLight=Luminance(TerrainPalette.Stone),grayDark=Luminance(TerrainPalette.Charcoal),brownLight=Luminance(TerrainPalette.Sand),brownDark=Luminance(TerrainPalette.Wood);
        private static double Contrast(double a,double b) { return (Math.Max(a,b)+0.05)/(Math.Min(a,b)+0.05); }
        private static Color Choose(Color light,Color dark,double lightLuma,double darkLuma,double surface)
        {
            return Contrast(lightLuma,surface)>=Contrast(darkLuma,surface)?light:dark;
        }
        private static Color CorpseInk(Color background) { return Choose(TerrainPalette.Stone,TerrainPalette.Charcoal,grayLight,grayDark,Luminance(background)); }
        private static Color CourierInk(Color background) { return Choose(TerrainPalette.Sand,TerrainPalette.Wood,brownLight,brownDark,Luminance(background)); }
        public static Color Ink(ActorState actor, double tick, bool riseEnabled, Color? background = null)
        {
            // Only presentation reads the rendered surface. Both shades retain a stable species color family.
            double surface=Luminance(background ?? TerrainPalette.Ink);
            Color corpse=Choose(TerrainPalette.Stone,TerrainPalette.Charcoal,grayLight,grayDark,surface);
            if(actor.health<=0)return corpse;
            if(actor.species==Species.Human)return Choose(TerrainPalette.WaterBlue,TerrainPalette.StoreBlue,blueLight,blueDark,surface);
            Color green=Choose(TerrainPalette.Olive,TerrainPalette.Grass,greenLight,greenDark,surface);
            if(!riseEnabled || actor.riseUntilTick<=tick)return green;
            float progress=(float)RiseRules.Progress(actor,tick);
            return new Color(Mathf.Lerp(corpse.r,green.r,progress),Mathf.Lerp(corpse.g,green.g,progress),Mathf.Lerp(corpse.b,green.b,progress));
        }
        private static Color CellInk(ActorState actor,double tick,bool riseEnabled,int size,int mask,int x,int y,int facing,Color body,Color corpse,HumanOccupation occupation,Color clothes)
        {
            if(size!=5 || (mask!=Human && mask!=WalkLeft && mask!=WalkRight))return body;
            if(y>1)
            {
                // Existing job identity styles only the close sprite; no behavior or saved uniform state.
                if(actor.species==Species.Human)
                {
                    if(occupation==HumanOccupation.Courier)return clothes;
                    if(occupation==HumanOccupation.Clerk && (y==3 || (y==2 && x==2)))return TerrainPalette.Gold;
                }
                return body;
            }
            bool eye=y==1 && (facing==0?x!=2:x==(facing<0?1:3));
            if(!eye)return actor.species==Species.Human?TerrainPalette.Cream:body;
            if(actor.species==Species.Zombie && riseEnabled && actor.riseUntilTick>tick)
            {
                float progress=(float)RiseRules.Progress(actor,tick);
                return new Color(Mathf.Lerp(corpse.r,TerrainPalette.Structure.r,progress),Mathf.Lerp(corpse.g,TerrainPalette.Structure.g,progress),Mathf.Lerp(corpse.b,TerrainPalette.Structure.b,progress));
            }
            return TerrainPalette.Structure;
        }
        public static Color PixelInk(ActorState actor,double tick,bool riseEnabled,int size,int mask,int x,int y,int facing=0,Color? background=null,HumanOccupation occupation=HumanOccupation.Consumer)
        {
            return CellInk(actor,tick,riseEnabled,size,mask,x,y,facing,Ink(actor,tick,riseEnabled,background),CorpseInk(background ?? TerrainPalette.Ink),occupation,occupation==HumanOccupation.Courier?CourierInk(background ?? TerrainPalette.Ink):Ink(actor,tick,riseEnabled,background));
        }

        public static Rect PixelBounds(Rect logicalBody)
        {
            int size = Tier(logicalBody);
            float pitch = size == 1 ? 1 : (float)Math.Floor(Math.Min(logicalBody.width, logicalBody.height) / size);
            float side = size * pitch;
            return new Rect((float)Math.Floor(logicalBody.x + (logicalBody.width - side) / 2 + 0.5f),
                (float)Math.Floor(logicalBody.y + (logicalBody.height - side) / 2 + 0.5f), side, side);
        }

        public static void Draw(Rect logicalBody, ActorState actor, double tick, bool riseEnabled, Color? flash = null, int walkFrame = -1, int facing = 0, Color? background = null, HumanOccupation occupation = HumanOccupation.Consumer)
        {
            Rect pixels = PixelBounds(logicalBody);
            Color previous = GUI.color;
            // Pick once per body, rather than recomputing surface contrast for every occupied pixel.
            Color body = Ink(actor,tick,riseEnabled,background);
            Color corpse = actor.species==Species.Zombie && riseEnabled && actor.riseUntilTick>tick ? CorpseInk(background ?? TerrainPalette.Ink) : TerrainPalette.Stone;
            Color clothes = occupation==HumanOccupation.Courier ? CourierInk(background ?? TerrainPalette.Ink) : body;
            GUI.color = flash ?? body;
            if (pixels.width == 1) GUI.DrawTexture(pixels, Texture2D.whiteTexture);
            else
            {
                int size = Tier(logicalBody);
                int mask = Mask(actor, tick, riseEnabled, size);
                if (size == 5 && mask == Human && walkFrame >= 0 && (!riseEnabled || actor.riseUntilTick <= tick))
                    mask = (walkFrame & 1) == 0 ? WalkLeft : WalkRight;
                // Only live close-tier heads turn. Shift within the same canvas; body/feet/hitboxes never move.
                int poseFacing = size == 5 && (mask == Human || mask == WalkLeft || mask == WalkRight) &&
                    (!riseEnabled || actor.riseUntilTick <= tick) ? Math.Sign(facing) : 0;
                float pitch = pixels.width / size;
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    if ((mask & (1 << (y * size + x))) != 0)
                    {
                        GUI.color = flash ?? CellInk(actor, tick, riseEnabled, size, mask, x, y, poseFacing, body, corpse, occupation, clothes);
                        GUI.DrawTexture(new Rect(pixels.x + (x + (y < 2 ? poseFacing : 0)) * pitch, pixels.y + y * pitch, pitch, pitch), Texture2D.whiteTexture);
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
        private void StartWorkerUniformPreview()
        {
            // Reuse the existing finite worker-shift fixture; the UI only presents its assigned jobs.
            UseWorld(EncounterScenarios.WorkerShiftTest()); selected=1;
            status="Brown courier, blue-shirt/pale-yellow-apron clerk, and ordinary resident. Close zoom shows uniforms; smaller views keep species colors. Existing shifts and gray off-duty role bands remain.";
        }
        private void StartNpcWalkPreview()
        {
            // Use the existing seeded non-encounter movement fixture; rendering never chooses a destination.
            World preview = new World(new WorldMap(32, 16, "NPC walk cycle", "npc-walk-cycle"), 214);
            int id; preview.TrySpawn(Species.Human, 10, 8, out id); preview.TrySpawn(Species.Zombie, 20, 8, out id);
            UseWorld(preview); selected = 0;
            status = "Run to see both species walk and face their horizontal movement. Pause holds feet and facing; smaller zoom tiers stay static. Click map, then WASD/arrows to pan.";
        }
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
