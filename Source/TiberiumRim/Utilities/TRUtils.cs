using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using TeleCore;
using UnityEngine;
using Verse;
using static System.String;

namespace TiberiumRim
{
    public static class TRUtils
    {
        public static NetworkValueDef[] MainValueTypes = new[] {TiberiumDefOf.TibGreen, TiberiumDefOf.TibBlue, TiberiumDefOf.TibRed }; 

        public static GameComponent_CameraPanAndLock CameraPanNLock()
        {
            return Current.Game.GetComponent<GameComponent_CameraPanAndLock>();
        }

        public static DiscoveryTable DiscoveryTable()
        {
            return Tiberium().DiscoveryTable;
        }

        public static EventManager EventManager()
        {
            return Find.World.GetComponent<EventManager>();
        }

        public static ResearchCreationTable ResearchCreationTable()
        {
            return Find.World.GetComponent<TResearchManager>().creationTable;
        }

        public static ResearchTargetTable ResearchTargetTable()
        {
            return Find.World.GetComponent<TResearchManager>().researchTargets;
        }

        public static TResearchManager ResearchManager()
        {
            return Find.World.GetComponent<TResearchManager>();
        }

        public static MapComponent_Tiberium Tiberium(this Map map)
        {
            if (map == null)
            {
                TRLog.Warning("Map is null for Tiberium MapComp getter");
                return null;
            }
            return StaticData.TiberiumMapComp[map.uniqueID];
        }

        public static WorldComponent_TR Tiberium()
        {
            return Find.World.GetComponent<WorldComponent_TR>();
        }

        public static GameSettingsInfo GameSettings()
        {
            return Tiberium().GameSettings;
        }

        public static MainTabWindow WindowFor(MainButtonDef def)
        {
            return def.TabWindow;
        }

        public static EventLetter SendEventLetter(this LetterStack stack, TaggedString eventLabel, TaggedString eventDesc, EventDef eventDef, LookTargets targets = null)
        {
            EventLetter letter = (EventLetter)LetterMaker.MakeLetter(eventLabel, eventDesc, TiberiumDefOf.EventLetter, targets);
            letter.AddEvent(eventDef);
            stack.ReceiveLetter(letter);
            return letter;
        }

        public static string GetTextureDirectory()
        {
            return GetModRootDirectory() + Path.DirectorySeparatorChar + "Textures" + Path.DirectorySeparatorChar;
        }

        public static string GetModRootDirectory()
        {
            TiberiumRimMod mod = LoadedModManager.GetMod<TiberiumRimMod>();
            if (mod == null)
            {
                TRLog.Error("LoadedModManager.GetMod<TiberiumRimMod>() failed");
                return "";
            }
            return mod.Content.RootDir;
        }

        public static string SizeTo(this string label, float newSize)
        {
            while (Text.CalcSize(label).x < newSize)
            {
                label += " ";
            }
            return label;
        }

        public static ThingDef VeinCorpseDef(this Pawn pawn)
        {
            ThingDef raceDef = pawn.def;
            ThingDef d = new ThingDef
            {
                category = ThingCategory.Item,
                thingClass = typeof(VeinholeFood),
                selectable = true,
                tickerType = TickerType.Normal,
                altitudeLayer = AltitudeLayer.ItemImportant,
                scatterableOnMapGen = false,
                drawerType = DrawerType.RealtimeOnly
            };
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

        public static bool IsConductive(this Thing thing)
        {
            if (thing.IsMetallic()) return true;
            if (thing.def.IsConductive()) return true;
            return false;
        }

        public static bool IsConductive(this ThingDef def)
        {
            return def.race != null;
        }

        public static bool IsMetallic(this Thing thing)
        {
            if (thing.def.MadeFromStuff && thing.Stuff.IsMetal) return true;
            return thing.def.IsMetallic();
        }

        public static bool IsMetallic(this ThingDef def)
        {
            if (def.costList.NullOrEmpty()) return false;
            float totalCost = def.costList.Sum(t => t.count);
            float metalCost = def.costList.Find(t => t.thingDef.IsMetal)?.count ?? 0;
            return metalCost / totalCost > 0.5f;
        }

        public static bool IsBuilding(this ThingDef def)
        {
            return def.category == ThingCategory.Building;
        }

        public static bool IsWall(this ThingDef def)
        {
            if (def.category != ThingCategory.Building) return false;
            if (!def.graphicData?.Linked ?? true) return false;
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

        public static float GetStatOffsetFromList(this List<ConditionalStatModifier> list, StatDef stat, Pawn pawn)
        {
            if (list == null) return 0;
            return list.Select(t => t.StatOffsetForStat(stat, pawn)).Sum();
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

        public static Matrix4x4 MatrixFor(Vector3 pos, float rotation, Vector3 size)
        {
            Matrix4x4 matrix = default;
            matrix.SetTRS(pos, rotation.ToQuat(), size);
            return matrix;
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

        //
        public static bool IsConstructible(this ThingDef def)
        {
            if (!def.IsBuildingArtificial) return false;
            if (def is TRThingDef trThing && trThing.isNatural) return false;
            return true;
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
        
        public static IEnumerable<T> AllFlags<T>(this T enumType) where T : Enum
        {
            return enumType.GetAllSelectedItems<T>();
        }

        public static string ShortLabel(this Enum enumType)
        {
            if (enumType is TiberiumValueType tibVal)
            {
                return ShortLabel(tibVal);
            }

            if (enumType is AtmosphericValueType atmosVal)
            {
                return ShortLabel(atmosVal);
            }

            return Empty;
        }

        public static string ShortLabel(this AtmosphericValueType valueType)
        {
            string label = valueType switch
            {
                AtmosphericValueType.Air => "Air",
                AtmosphericValueType.Pollution => "Poll",
                _ => Empty
            };

            return label;
        }

        public static string ShortLabel(this TiberiumValueType valueType)
        {
            string label = valueType switch
            {
                TiberiumValueType.Green => "G",
                TiberiumValueType.Blue => "B",
                TiberiumValueType.Red => "R",
                TiberiumValueType.Sludge => "Slg",
                TiberiumValueType.Gas => "Gs",
                _ => Empty
            };

            return label;
        }

        public static Color GetColor(this Enum valueType)
        {
            Color color = Color.white;
            if (valueType is TiberiumValueType tibType)
            {
                TiberiumControlDef def = MainTCD.Main;
                color = tibType switch
                {
                    TiberiumValueType.Green => def.GreenColor,
                    TiberiumValueType.Blue => def.BlueColor,
                    TiberiumValueType.Red => def.RedColor,
                    TiberiumValueType.Sludge => def.SludgeColor,
                    TiberiumValueType.Gas => def.GasColor,
                    _ => color
                };
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



        public static bool IsTiberiumTerrain(this TerrainDef def)
        {
            return def is TiberiumTerrainDef;
        }

        public static TiberiumCrystalDef CrystalDefFromType(NetworkValueDef valueDef, out bool isGas)
        {
            isGas = false;
            //TODO: Adjust drops
            if (valueDef == TiberiumDefOf.TibGreen)
            {
                return TiberiumDefOf.TiberiumGreen;
            }
            if (valueDef == TiberiumDefOf.TibBlue)
            {
                return TiberiumDefOf.TiberiumBlue;
            }
            if (valueDef == TiberiumDefOf.TibRed)
            {
                return TiberiumDefOf.TiberiumRed;
            }
            if (valueDef == TiberiumDefOf.TibSludge)
            {
                return TiberiumDefOf.TiberiumMossGreen;
            }
            if (valueDef == TiberiumDefOf.TibGas)
            {
                isGas = true;
            }
            return null;
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

        public static bool IsBlocked(this IntVec3 cell, Map map, out bool byPlant)
        {
            byPlant = false;
            if (!cell.Walkable(map))
            {
                return true;
            }
            List<Thing> list = map.thingGrid.ThingsListAt(cell);
            foreach (var thing in list)
            {
                if (thing.def.passability != Traversability.Standable)
                {
                    byPlant = thing is Plant;
                    return true;
                }
            }
            return false;
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

        public static string ToStringDictListing<K,V>(this Dictionary<K,V> dict)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var v in dict)
            {
                sb.AppendLine($"{v.Key}: {v.Value}");
            }

            return sb.ToString().TrimEndNewlines();
        }
    }
}
