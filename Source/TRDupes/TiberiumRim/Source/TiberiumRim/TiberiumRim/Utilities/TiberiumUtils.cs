using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public static class TRUtils
    {
        public static DiscoveryTable DiscoveryTable()
        {
            return Tiberium(Find.World).DiscoveryTable;
        }

        public static EventManager EventManager()
        {
            return Find.World.GetComponent<EventManager>();
        }
        public static TResearchManager ResearchManager()
        {
            return Find.World.GetComponent<TResearchManager>();
        }

        public static MapComponent_Tiberium Tiberium(this Map map)
        {
            return map.GetComponent<MapComponent_Tiberium>();
        }

        public static WorldComponent_TR Tiberium(this World world)
        {
            return world.GetComponent<WorldComponent_TR>();
        }

        public static ThingDef VeinCorpseDef(this Pawn pawn)
        {
            ThingDef raceDef = pawn.def;
            ThingDef d = new ThingDef();
            d.category = ThingCategory.Item;
            d.thingClass = typeof(VeinholeFood);
            d.selectable = true;
            d.tickerType = TickerType.Normal;
            d.altitudeLayer = AltitudeLayer.ItemImportant;
            d.scatterableOnMapGen = false;
            d.drawerType = DrawerType.RealtimeOnly;
            d.SetStatBaseValue(StatDefOf.Beauty, -50f);
            d.SetStatBaseValue(StatDefOf.DeteriorationRate, 1f);
            d.SetStatBaseValue(StatDefOf.FoodPoisonChanceFixedHuman, 0.05f);
            d.alwaysHaulable = true;
            d.soundDrop = SoundDefOf.Corpse_Drop;
            d.pathCost = 15;
            d.socialPropernessMatters = false;
            d.tradeability = Tradeability.None;
            d.inspectorTabs = new List<Type>();
            d.inspectorTabs.Add(typeof(ITab_Pawn_Health));
            d.inspectorTabs.Add(typeof(ITab_Pawn_Character));
            d.inspectorTabs.Add(typeof(ITab_Pawn_Gear));
            d.inspectorTabs.Add(typeof(ITab_Pawn_Social));
            d.inspectorTabs.Add(typeof(ITab_Pawn_Log));
            d.comps.Add(new CompProperties_Forbiddable());
            d.recipes = new List<RecipeDef>();
            if (!raceDef.race.IsMechanoid)
            {
                d.recipes.Add(RecipeDefOf.RemoveBodyPart);
            }
            d.defName = "VeinCorpse_" + raceDef.defName;
            d.label = "CorpseLabel".Translate(raceDef.label);
            d.description = "CorpseDesc".Translate(raceDef.label);
            d.soundImpactDefault = raceDef.soundImpactDefault;
            d.SetStatBaseValue(StatDefOf.MarketValue, raceDef.race.corpseDef.statBases.GetStatValueFromList(StatDefOf.MarketValue, 0));
            d.SetStatBaseValue(StatDefOf.Flammability, raceDef.GetStatValueAbstract(StatDefOf.Flammability, null));
            d.SetStatBaseValue(StatDefOf.MaxHitPoints, (float)raceDef.BaseMaxHitPoints);
            d.SetStatBaseValue(StatDefOf.Mass, raceDef.statBases.GetStatOffsetFromList(StatDefOf.Mass));
            d.SetStatBaseValue(StatDefOf.Nutrition, 5.2f);
            d.modContentPack = raceDef.modContentPack;
            d.ingestible = new IngestibleProperties();
            d.ingestible.parent = d;
            IngestibleProperties ing = d.ingestible;
            ing.foodType = FoodTypeFlags.Corpse;
            ing.sourceDef = raceDef;
            ing.preferability = ((!raceDef.race.IsFlesh) ? FoodPreferability.NeverForNutrition : FoodPreferability.DesperateOnly);
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(ing, "tasteThought", ThoughtDefOf.AteCorpse.defName);
            ing.maxNumToIngestAtOnce = 1;
            ing.ingestEffect = EffecterDefOf.EatMeat;
            ing.ingestSound = SoundDefOf.RawMeat_Eat;
            ing.specialThoughtDirect = raceDef.race.FleshType.ateDirect;
            if (raceDef.race.IsFlesh)
            {
                CompProperties_Rottable compProperties_Rottable = new CompProperties_Rottable();
                compProperties_Rottable.daysToRotStart = 2.5f;
                compProperties_Rottable.daysToDessicated = 5f;
                compProperties_Rottable.rotDamagePerDay = 2f;
                compProperties_Rottable.dessicatedDamagePerDay = 0.7f;
                d.comps.Add(compProperties_Rottable);
                CompProperties_SpawnerFilth compProperties_SpawnerFilth = new CompProperties_SpawnerFilth();
                compProperties_SpawnerFilth.filthDef = ThingDefOf.Filth_CorpseBile;
                compProperties_SpawnerFilth.spawnCountOnSpawn = 0;
                compProperties_SpawnerFilth.spawnMtbHours = 0f;
                compProperties_SpawnerFilth.spawnRadius = 0.1f;
                compProperties_SpawnerFilth.spawnEveryDays = 1f;
                compProperties_SpawnerFilth.requiredRotStage = new RotStage?(RotStage.Rotting);
                d.comps.Add(compProperties_SpawnerFilth);
            }
            if (d.thingCategories == null)
            {
                d.thingCategories = new List<ThingCategoryDef>();
            }
            return d;
        }

        public static bool IsWall(this ThingDef def)
        {
            return (def.graphicData.linkFlags & LinkFlags.Wall) != LinkFlags.None &&
                   def.graphicData.linkType == LinkDrawerType.CornerFiller &&
                   def.fillPercent >= 1f &&
                   def.blockWind         &&
                   def.coversFloor       &&
                   def.castEdgeShadows   &&
                   def.holdsRoof         &&
                   def.blockLight;
        }

        public static Material GetColoredVersion(this Material mat, Color color)
        {
            Material material = new Material(mat);
            material.color = color;
            return material;
        }

        public static void DrawTargeter(IntVec3 pos, Material mat, float size)
        {
            Vector3 vector = pos.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
            Matrix4x4 matrix = default;
            matrix.SetTRS(vector, Quaternion.Euler(0f, 0f, 0f), new Vector3(size, 1f, size));
            Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0, null, 0);
        }

        public static bool IsPlayerControlledMech(this Thing thing)
        {
            return thing is MechanicalPawn p && (p.Faction?.IsPlayer ?? false);
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
            var randWeight = TRUtils.RandValue * totalWeight;
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

        //Fucking Math
        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            Vector3 AB = b - a;
            Vector3 AV = value - a;
            return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
        }

        public static float AngleWrapped(this float angle)
        {
            while (angle > 360)
            {
                angle -= 360;
            }
            while (angle < 0)
            {
                angle += 360;
            }
            return angle == 360 ? 0f : angle;
        }

        //
        public static IEnumerable<IntVec3> SectorCells(IntVec3 center, Map map, float radius, float angle, float rotation, bool useCenter = false, Predicate<IntVec3> validator = null)
        {
            int cellCount = GenRadial.NumCellsInRadius(radius);
            int startCell = useCenter ? 0 : 1;
            var angleMin = AngleWrapped(rotation - angle * 0.5f);
            var angleMax = AngleWrapped(angleMin + angle);
            for (int i = startCell; i < cellCount; i++)
            {
                IntVec3 cell = GenRadial.RadialPattern[i] + center;
                float curAngle = (cell.ToVector3Shifted() - center.ToVector3Shifted()).AngleFlat();
                var invert = angleMin > angleMax;
                var flag = invert ? (curAngle >= angleMin || curAngle <= angleMax) : (curAngle >= angleMin && curAngle <= angleMax);
                if (!cell.InBounds(map) || !flag || (validator != null && !validator(cell)))
                    continue;
                yield return cell;
            }
        }

        /*
        public static List<IntVec3> SectorCells(IntVec3 center, Map map, float radius, float angle, float rotation)
        {
            var cells = new List<IntVec3>();
            var min = AngleWrapped(rotation - angle * 0.5f);
            var max = AngNom(min + angle);
            var numCells = GenRadial.NumCellsInRadius(radius);

            for (int i = 1; i < numCells; i++)
            {
                IntVec3 cell = GenRadial.RadialPattern[i] + center;
                float cellAngle = AngNom(center.ToVector3().AngleToFlat(cell.ToVector3()) + 90);
                if (map != null && (!cell.InBounds(map) || cell.Roofed(map) && !GenSight.LineOfSight(center, cell, map)))
                    continue;
                if (min > max && (cellAngle >= min || cellAngle <= max))
                    cells.Add(cell);
                else if (cellAngle >= min && cellAngle <= max)
                    cells.Add(cell);
            }
            return cells;
        }
        */

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

        public static float RandValue
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

        public static void Draw(Graphic graphic, Vector3 drawPos, Rot4 rot, float? rotation, FXThingDef fxDef)
        {
            GraphicDrawInfo info = new GraphicDrawInfo(graphic, drawPos, rot, fxDef?.extraData, fxDef);
            Graphics.DrawMesh(info.drawMesh, info.drawPos, rotation?.ToQuat() ?? info.rotation.ToQuat(), info.drawMat,0);
            //Graphics.DrawMesh(graphic.MeshAt(rot), new Vector3(info.drawPos.x, fxDef.altitudeLayer.AltitudeFor(), info.drawPos.z), rotation?.ToQuat() ?? info.rotation.ToQuat(), mat, 0);
        }

        public static void Print(SectionLayer layer, Graphic graphic, ThingWithComps thing, FXThingDef fxDef)
        {
            if (graphic is Graphic_Linked || graphic is Graphic_Appearances)
            {
                graphic.Print(layer, thing);
                return;
            }
            if (graphic is Graphic_Random rand)
                graphic = rand.SubGraphicFor(thing);
            GraphicDrawInfo info = new GraphicDrawInfo(graphic, thing.DrawPos, thing.Rotation, fxDef.extraData, fxDef, thing);
            //Log.Message("Printing: " + thing + " with drawsize: " + info.drawSize + " rotation: " + info.rotation + " flipUV: " + info.flipUV);
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
        public static bool IsPowered(this ThingWithComps thing, out bool usesPower)
        {
            var comp = thing.GetComp<CompPowerTrader>();
            usesPower = comp != null;
            if (!usesPower)
                return false;
            return usesPower ? comp.PowerOn : false;
        }

        public static bool ThingExistsAt(Map map, IntVec3 pos, ThingDef def)
        {
            return !map.thingGrid.ThingAt(pos, def).DestroyedOrNull();
        }

        public static Thing GetAnyThingIn<T>(this CellRect cells, Map map)
        {
            foreach (var c in cells)
            {
                if (!c.InBounds(map)) continue;
                var t = c.GetThingList(map).Find(x => x is T);
                if(t != null)
                {
                    return t;
                }
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


        public static bool ThingFitsAt(this ThingDef thing, Map map, IntVec3 cell)
        {
            foreach (var c in GenAdj.OccupiedRect(cell, Rot4.North, thing.size))
            {
                if (!c.InBounds(map) || c.Fogged(map) || !c.Standable(map) || (c.Roofed(map) && c.GetRoof(map).isThickRoof))
                {
                    return false;
                }
            }
            return true;
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
