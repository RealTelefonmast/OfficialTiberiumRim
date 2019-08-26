using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public static class TRUtils
    {

        public static void DrawTargeter(IntVec3 pos, Material mat, float size)
        {
            Vector3 vector = pos.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
            Graphics.DrawMesh(MeshMakerPlanes.NewPlaneMesh(size), vector, Quaternion.identity, mat, 0);
        }

        public static bool IsPlayerControlledMech(this Thing thing)
        {
            return thing is MechanicalPawn p && p.Faction.IsPlayer;
        }
        public static void GetTiberiumMutant(Pawn pawn, out Graphic Head, out Graphic Body)
        {
            Head = null;
            Body = null;
            if (pawn.def.defName != "Human")
            {
                PawnGraphicSet graphicSet = pawn.Drawer.renderer.graphics;
                string headPath = graphicSet.headGraphic.path + "_TibHead";
                string bodyPath = graphicSet.nakedGraphic.path + "_TibBody";
                Head = GraphicDatabase.Get(typeof(Graphic_Multi), headPath, ShaderDatabase.Cutout, Vector2.one, Color.white, Color.white);
                Body = GraphicDatabase.Get(typeof(Graphic_Multi), bodyPath, ShaderDatabase.Cutout, Vector2.one, Color.white, Color.white);
            }
            else
            {
                CrownType head = pawn.story.crownType;
                string headPath = pawn.story.HeadGraphicPath;
                string headResolved;
                BodyTypeDef body = pawn.story.bodyType;
                string bodyResolved;
                Gender gender = pawn.gender;

                string appendix = "";
                if (headPath.Contains("_Wide"))
                {
                    appendix = "_Wide";
                }
                if (headPath.Contains("_Normal"))
                {
                    appendix = "_Normal";
                }
                if (headPath.Contains("_Pointy"))
                {
                    appendix = "_Pointy";
                }
                headResolved = "Pawns/TiberiumMutant/Heads/" + gender + "_" + head + appendix;
                //Head = GraphicDatabase.Get(typeof(Graphic_Multi), headResolved, ShaderDatabase.MoteGlow, Vector2.one, Color.white, Color.white);
                Head = GraphicDatabase.Get(typeof(Graphic_Multi), "Pawns/TiberiumMutant/Heads/Mutant_head", ShaderDatabase.Cutout, Vector2.one, Color.white, Color.white);
                bodyResolved = "Pawns/TiberiumMutant/Bodies/" + body.defName;
                Body = GraphicDatabase.Get(typeof(Graphic_Multi), bodyResolved, ShaderDatabase.Cutout, Vector2.one, Color.white, Color.white);
            }
            
        }

        public static T RandomWeightedElement<T>(this IEnumerable<T> elements, Func<T, float> weightSelector)
        {
            var totalWeight = elements.Sum(weightSelector);
            var randWeight = TRUtils.Value * totalWeight;
            var curWeight = 0f;
            foreach (var e in elements)
            {
                float weight = weightSelector(e);
                if (weight <= 0) continue;
                curWeight += weight;
                if (curWeight >= randWeight)
                    return e;
            }
            return default(T);
        }

       public static Dictionary<T, T2> Copy<T, T2>(this Dictionary<T, T2> old)
        {
            Dictionary<T, T2> newDict = new Dictionary<T, T2>();
            foreach (T o in old.Keys)
                newDict.Add(o, old.TryGetValue(o));
            return newDict;
        }

        //Fucking Angular Math
        public static float AngNom(float angle)
        {
            float angle2 = angle;
            if (angle2 == 360)
                return 0;
            if (angle2 < 0)
            {
                while (angle2 < 0)
                    angle2 += 360;
            }

            if (angle > 360)
            {
                while (angle2 > 360)
                    angle2 -= 360;
            }

            return angle2;
        }

        public static List<IntVec3> SectorCells(IntVec3 center, Map map, float radius, float angle, float rotation)
        {
            var cells = new List<IntVec3>();
            var min = AngNom(rotation - angle * 0.5f);
            var max = AngNom(min + angle);
            var numCells = GenRadial.NumCellsInRadius(radius);

            for (int i = 1; i < numCells; i++)
            {
                IntVec3 cell = GenRadial.RadialPattern[i] + center;
                float cellAngle = AngNom(center.ToVector3().AngleToFlat(cell.ToVector3()) + 90);
                if (map != null &&
                    (!cell.InBounds(map) || cell.Roofed(map) && !GenSight.LineOfSight(center, cell, map)))
                    continue;
                if (min > max && (cellAngle >= min || cellAngle <= max))
                    cells.Add(cell);
                else if (cellAngle >= min && cellAngle <= max)
                    cells.Add(cell);
            }

            return cells;
        }

        //Randomizers
        public static float Range(FloatRange range)
        {
            return Range(range.min, range.max);
        }

        public static float Range(float min, float max)
        {
            if (max <= min)
            {
                return min;
            }
            Rand.PushState();
            float result = Rand.Value * (max - min) + min;
            Rand.PopState();
            return result;
        }

        public static int RangeInclusive(int min, int max)
        {
            if (max <= min)
            {
                return min;
            }
            return Range(min, max + 1);
        }

        public static int Range(IntRange range)
        {
            return Range(range.min, range.max);
        }

        public static int Range(int min, int max)
        {
            if (max <= min)
            {
                return min;
            }
            Rand.PushState();
            int result = min + Mathf.Abs(Rand.Int % (max - min));
            Rand.PopState();
            return result;
        }

        public static float Value
        {
            get
            {
                Rand.PushState();
                float value = Rand.Value;
                Rand.PopState();
                return value;
            }
        }

        public static bool Chance(float f)
        {
            Rand.PushState();
            bool result = Rand.Chance(f);
            Rand.PopState();
            return result;
        }

        public static Room GetRoomIndirect(this Thing thing)
        {
            var room = thing.GetRoom();
            if(room == null)
            {
                room = thing.CellsAdjacent8WayAndInside().Select(c => c.GetRoom(thing.Map)).First(r => r != null);
            }
            return room;
        }

        public static Pawn NewBorn(PawnKindDef kind, Faction faction = null, PawnGenerationContext context = PawnGenerationContext.NonPlayer)
        {
            PawnGenerationRequest request = new PawnGenerationRequest(kind, faction, context, -1, true, true);
            return PawnGenerator.GeneratePawn(request);
        }

        public static List<IntVec3> RemoveCorners(this CellRect rect, int[] range)
        {
            List<IntVec3> cells = rect.Cells.ToList();
            for (var i = 0; i < range.Count(); i++)
            {
                var j = range[i];
                switch (j)
                {
                    case 1:
                        cells.RemoveAll(c => c.x == rect.minX && c.z == rect.maxZ);
                        break;
                    case 2:
                        cells.RemoveAll(c => c.x == rect.maxX && c.z == rect.maxZ);
                        break;
                    case 3:
                        cells.RemoveAll(c => c.x == rect.maxX && c.z == rect.minZ);
                        break;
                    default:
                        cells.RemoveAll(c => c.x == rect.minX && c.z == rect.minZ);
                        break;
                }
            }
            return cells;
        }

        public static float AlignToBottomOffset(ThingDef def, GraphicData data)
        {
            float height = data.drawSize.y;
            float selectHeight = def.size.z;
            float diff = height - selectHeight;
            return diff / 2;
        }

        public static void Draw(Graphic graphic, Vector3 drawPos, Rot4 rot, float? rotation, ThingWithComps thing, FXThingDef fxDef)
        {
            GraphicDrawInfo info = new GraphicDrawInfo(graphic, thing, fxDef, drawPos, rot);
            Log.Message("DrawSize: " + info.drawSize + " Rotation: " + info.rotation);
            Graphics.DrawMesh(info.drawMesh, info.drawPos, info.rotation.ToQuat(), info.drawMat,0);
            //Graphics.DrawMesh(graphic.MeshAt(rot), new Vector3(info.drawPos.x, fxDef.altitudeLayer.AltitudeFor(), info.drawPos.z), rotation?.ToQuat() ?? info.rotation.ToQuat(), mat, 0);
        }

        public static void Print(SectionLayer layer, Graphic graphic, ThingWithComps thing, ThingDef fxDef)
        {
            if (graphic is Graphic_Linked || graphic is Graphic_Appearances)
            {
                graphic.Print(layer, thing);
                return;
            }
            GraphicDrawInfo info = new GraphicDrawInfo(graphic, thing, fxDef, thing.DrawPos, thing.Rotation);
            Printer_Plane.PrintPlane(layer, info.drawPos, info.drawSize, info.drawMat, info.rotation, info.flipUV, null, null, 0.01f, 0f);
            if (graphic.ShadowGraphic != null && thing != null)
            {
                graphic.ShadowGraphic.Print(layer, thing);
            }
            thing.AllComps.ForEach(c => c.PostPrintOnto(layer));
        }

        public static string ToString(this System.Xml.XmlNode node, int indentation)
        {
            using (var sw = new System.IO.StringWriter())
            {
                using (var xw = new System.Xml.XmlTextWriter(sw))
                {
                    xw.Formatting = System.Xml.Formatting.Indented;
                    xw.Indentation = indentation;
                    node.WriteContentTo(xw);
                }
                return sw.ToString();
            }
        }

        public static ThingDef MakeNewBluePrint(ThingDef def, bool isInstallBlueprint, ThingDef normalBlueprint = null)
        {
            Type type = typeof(ThingDefGenerator_Buildings);
            var NewBlueprint = type.GetMethod("NewBlueprintDef_Thing", BindingFlags.NonPublic | BindingFlags.Static);
            return (ThingDef)NewBlueprint.Invoke(null, new object[] { def, isInstallBlueprint, normalBlueprint });
        }

        public static ThingDef MakeNewFrame(ThingDef def)
        {
            Type type = typeof(ThingDefGenerator_Buildings);
            var NewFrame = type.GetMethod("NewFrameDef_Thing", BindingFlags.NonPublic | BindingFlags.Static);
            return (ThingDef)NewFrame.Invoke(null, new object[] { def });
        }

        public static Rot4 FromAngleFlat2(float angle)
        {
            angle = GenMath.PositiveMod(angle, 360f);
            if (angle <= 45f)
                return Rot4.North;
            if (angle <= 135f)
                return Rot4.East;
            if (angle < 225f)
                return Rot4.South;
            if (angle <= 315f)
                return Rot4.West;
            return Rot4.North;
        }

        public static IntVec3 PositionOffset(this IntVec3 fromCenter, IntVec3 toCenter)
        {
            Rot4 rotation = FromAngleFlat2((fromCenter - toCenter).AngleFlat);
            if (rotation == Rot4.North)
                return IntVec3.North;
            if (rotation == Rot4.East)
                return IntVec3.East;
            if (rotation == Rot4.South)
                return IntVec3.South;
            if (rotation == Rot4.West)
                return IntVec3.West;
            return IntVec3.Zero;
        }

        //Spawning Thing
        public static MoteThrown ThrowTiberiumLeak(Map map, IntVec3 cell, Rot4 rotation, Color color)
        {
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(TiberiumDefOf.Mote_TiberiumLeak, null);
            moteThrown.Scale = 0.55f;
            moteThrown.instanceColor = color;
            moteThrown.rotationRate = (float)TRUtils.RangeInclusive(-240, 240);
            moteThrown.exactPosition = cell.ToVector3Shifted();
            moteThrown.exactPosition += new Vector3(TRUtils.Range(-0.02f, 0.02f), 0f, TRUtils.Range(-0.02f, 0.02f));
            moteThrown.SetVelocity(rotation.AsAngle + TRUtils.Range(-16,16), TRUtils.Range(1.85f, 2.5f));
            GenSpawn.Spawn(moteThrown, cell, map, WipeMode.Vanish);
            return moteThrown;
        }

        //

        public static bool ThingExistsAt(Map map, IntVec3 pos, ThingDef def)
        {
            return !map.thingGrid.ThingAt(pos, def).DestroyedOrNull();
        }

        public static Thing GetAnyThingIn<T>(this CellRect cells, Map map)
        {
            CellRect.CellRectIterator iterator = cells.GetIterator();
            while (!iterator.Done())
            {
                IntVec3 c = iterator.Current;
                if (c.InBounds(map))
                {
                    var t = c.GetThingList(map).Find(x => x is T);
                    if(t != null)
                    {
                        return t;
                    }
                }
                iterator.MoveNext();
            }
            return null;
        }

        public static Color ColorForType(TiberiumValueType valueType)
        {
            Color color = Color.white;
            TiberiumControlDef def = MainTCD.Main;
            switch (valueType)
            {
                case TiberiumValueType.Green:
                    color = def.GreenColor;
                    break;
                case TiberiumValueType.Blue:
                    color = def.BlueColor;
                    break;
                case TiberiumValueType.Red:
                    color = def.RedColor;
                    break;
                case TiberiumValueType.Sludge:
                    color = def.SludgeColor;
                    break;
                case TiberiumValueType.Gas:
                    color = def.GasColor;
                    break;
            }
            return color;
        }

        public static Texture2D MaterialForType(TiberiumValueType valueType)
        {
            Texture2D tex = SolidColorMaterials.NewSolidColorTexture(Color.black);
            TiberiumControlDef def = MainTCD.Main;
            switch (valueType)
            {
                case TiberiumValueType.Green:
                    tex = TRMats.GreenType;
                    break;
                case TiberiumValueType.Blue:
                    tex = TRMats.BlueType;
                    break;
                case TiberiumValueType.Red:
                    tex = TRMats.RedType;
                    break;
                case TiberiumValueType.Sludge:
                    tex = TRMats.SludgeType;
                    break;
                case TiberiumValueType.Gas:
                    tex = TRMats.GasType;
                    break;
            }
            return tex;
        }

        public static float Cosine2(float yMin = 0f, float yMax = 1f, float xMax = 1f , float xOff = 0f, float curX = 0f)
        {
            float mltp = (yMax - yMin) / 2f;
            float height = mltp + yMin;
            float ang = (1f / (xMax * 2f)) * Mathf.PI * (curX - xOff);
            return (mltp * Mathf.Cos(ang)) + height;
        }

        public static float Cosine(float yMin = 0f, float yMax = 1f, float freq = 1f, float curX = 0f, float period = 2f)
        {
            float mltp = (yMax - yMin) / 2f;
            float height = mltp + yMin;
            float ang = (period * Mathf.PI) * curX * freq;
            return (mltp * Mathf.Cos(ang)) + height;
        }

        public static TiberiumCrystal ClosestTiberiumFor(Pawn seeker, TraverseParms parms, PathEndMode peMode = PathEndMode.Touch, HarvestMode hMode = HarvestMode.Nearest, TiberiumCrystalDef preferredDef = null)
        {
            TiberiumCrystal crystal = null;
            Predicate<IntVec3> predicate = x => x.IsValid && x.Standable(seeker.Map) && crystal == null;
            Action<IntVec3> action = delegate(IntVec3 c) {
                if (crystal == null)
                {
                    crystal = c.GetTiberium(seeker.Map);
                    if(!seeker.CanReserve(crystal) || !seeker.CanReach(c, peMode, Danger.Deadly))
                    {
                        crystal = null;
                    }
                }
            };
            seeker.Map.floodFiller.FloodFill(seeker.Position, predicate, action);    
            return crystal;
        }

        public static bool HarvestablyBy(this TiberiumCrystalDef def, Harvester harvester)
        {
            return true;
        }

        public static bool IsTiberiumTerrain(this TerrainDef def)
        {
            return def is TiberiumTerrainDef;
        }

        public static void CorruptArea(IntVec3 center, int radius, TiberiumValueType valueType, TiberiumCrystalDef def = null)
        {
            IEnumerable<IntVec3> cells = GenRadial.RadialCellsAround(center, radius, true);
            foreach(IntVec3 c in cells)
            {

            }
        }

        public static void CorruptThing(Thing thing, TiberiumCrystalDef def = null)
        {
            if(thing is Pawn)
            {

            }
            if(thing is Mineable)
            {

            }
        }

        public static bool IsCorruptableChunk(this Thing haulable)
        {
            return (haulable.def.thingCategories?.Contains(ThingCategoryDef.Named("StoneChunks")) ?? false) &&
                   !(haulable is TiberiumChunk);
        }

        public static TiberiumCrystalDef CrystalDefFromType(TiberiumValueType valueType, out bool isGas)
        {
            isGas = false;
            TiberiumCrystalDef crystalDef = null;
            switch (valueType)
            {
                case TiberiumValueType.Green: return TiberiumDefOf.TiberiumGreen;
                case TiberiumValueType.Blue: return TiberiumDefOf.TiberiumBlue;
                case TiberiumValueType.Red: return TiberiumDefOf.TiberiumRed;
                case TiberiumValueType.Sludge: return TiberiumDefOf.TiberiumMossGreen;
                case TiberiumValueType.Gas: isGas = true; return crystalDef;
                default: return crystalDef;
            }
        }

        public static CellRect ToCellRect(this List<IntVec3> cells)
        {
            int minZ = cells.Min(c => c.z);
            int maxZ = cells.Max(c => c.z);
            int minX = cells.Min(c => c.x);
            int maxX = cells.Max(c => c.x);
            int width = maxX - (minX - 1);
            int height = maxZ - (minZ - 1);
            return new CellRect(minX, minZ, width, height);
        }

        public static IEnumerable<IntVec3> CellsAdjacent8Way(this IntVec3 loc, bool andInside = false)
        {
            if (andInside)
            { yield return loc; }

            IntVec3 center = loc;
            int minX = center.x - (1 - 1) / 2 - 1;
            int minZ = center.z - (1 - 1) / 2 - 1;
            int maxX = minX + 1 + 1;
            int maxZ = minZ + 1 + 1;
            for (int i = minX; i <= maxX; i++)
            {
                for (int j = minZ; j <= maxZ; j++)
                {
                    yield return new IntVec3(i, 0, j);
                }
            }
            yield break;
        }
    }
}
