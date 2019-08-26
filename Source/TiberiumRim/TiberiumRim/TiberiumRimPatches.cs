using Harmony;
using System;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;
using UnityEngine;
using System.Collections;
using System.Threading;
using System.IO;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public static class TiberiumRimPatches
    {
        static TiberiumRimPatches()
        {
            Log.Message("Constructing TiberiumRimPatches");
            HarmonyInstance Tiberium = TiberiumRimMod.Tiberium;
            Tiberium.Patch(typeof(UI_BackgroundMain).GetMethod("BackgroundOnGUI"),new HarmonyMethod(typeof(TiberiumRimPatches), "BackgroundOnGUIPatch"));
            Tiberium.Patch(
            typeof(SymbolResolver_RandomMechanoidGroup).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .First(mi => mi.HasAttribute<CompilerGeneratedAttribute>() && mi.ReturnType == typeof(bool) &&
                     mi.GetParameters().Count() == 1 &&
                     mi.GetParameters()[0].ParameterType == typeof(PawnKindDef)),
            null, new HarmonyMethod(typeof(TiberiumRimPatches),
            nameof(TiberiumRimPatches.MechanoidsFixerAncient)));

            Tiberium.Patch(
                typeof(CompSpawnerMechanoidsOnDamaged).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(
                    mi => mi.HasAttribute<CompilerGeneratedAttribute>() && mi.ReturnType == typeof(bool) &&
                          mi.GetParameters().Count() == 1 &&
                          mi.GetParameters()[0].ParameterType == typeof(PawnKindDef)), null, new HarmonyMethod(
                    typeof(TiberiumRimPatches), nameof(TiberiumRimPatches.MechanoidsFixer)));
            TiberiumRimMod.mod.LoadAssetBundles();
            TiberiumRimMod.mod.PatchPawnDefs();
        }

        public static void MechanoidsFixerAncient(ref bool __result, PawnKindDef kind)
        {
            if (typeof(MechanicalPawn).IsAssignableFrom(kind.race.thingClass)) __result = false;
        }

        public static void MechanoidsFixer(ref bool __result, PawnKindDef def)
        {
            if (typeof(MechanicalPawn).IsAssignableFrom(def.race.thingClass)) __result = false;
        }

        //Render Patches
        public static bool BackgroundOnGUIPatch()
        {
            if (!TiberiumRimSettings.settings.CustomBackground) return true;
            bool flag = !((float) UI.screenWidth > (float) UI.screenHeight * (2048f / 1280f));
            Rect position;
            if (flag)
            {
                float height = (float) UI.screenHeight;
                float num = (float) UI.screenHeight * (2048f / 1280f);
                position = new Rect((float) (UI.screenWidth / 2) - num / 2f, 0f, num, height);
            }
            else
            {
                float width = (float) UI.screenWidth;
                float num2 = (float) UI.screenWidth * (1280f / 2048f);
                position = new Rect(0f, (float) (UI.screenHeight / 2) - num2 / 2f, width, num2);
            }

            GUI.DrawTexture(position, TiberiumContent.BGPlanet, ScaleMode.ScaleToFit);
            return false;
        }

        [HarmonyPatch(typeof(HealthCardUtility))]
        [HarmonyPatch("DrawHediffRow")]
        public static class HediffDrawerPatch
        {
            [TweakValue("Hediff_MutationHelper_Bar_contracter", -50f, 50f)]
            private static float contracted = 3f;

            [HarmonyPostfix]
            public static void Fix(Rect rect, Pawn pawn, IEnumerable<Hediff> diffs, ref float curY)
            {
                Hediff_Mutation worker = (Hediff_Mutation)diffs.ToList().Find(h => h is Hediff_Mutation);
                if (worker != null)
                {
                    float viscNum = worker.VisceralCoverage;
                    float symbNum = worker.SymbioticCoverage;
                    float rev = 1f - viscNum;

                    string s2 = viscNum == symbNum ? " = " : (viscNum > symbNum ? " > " : " < ");
                    Vector2 vec = Text.CalcSize(s2);
                    string visc = "◀ " + "TR_Visceral".Translate() + " " + viscNum.ToStringPercent();
                    Vector2 vec1 = Text.CalcSize(visc);
                    string symb = symbNum.ToStringPercent() + " " + "TR_Symbiotic".Translate() + " ▶";
                    Vector2 vec2 = Text.CalcSize(symb);

                    Rect rectSide = new Rect((rect.width / 2f) - (vec.x / 2f), curY, vec.x, vec.y);
                    Rect rect1 = new Rect((rect.width / 2f) - vec1.x - rectSide.width / 2f, curY, vec1.x, vec1.y);
                    Rect rect2 = new Rect((rect.width / 2f) + rectSide.width / 2f, curY, vec2.x, vec2.y);
                    curY += rect1.height;

                    Rect fillRectTotal = new Rect(0f, curY, rect.width, 18f).ContractedBy(contracted);
                    Rect visceral = fillRectTotal.LeftHalf();
                    Rect symbiotic = fillRectTotal.RightHalf();
                    curY += fillRectTotal.ExpandedBy(contracted).height;

                    GUI.color = new ColorInt(155, 160, 75).ToColor;
                    Widgets.Label(rect1, visc);
                    GUI.color = Color.white;
                    Widgets.Label(rectSide, s2);
                    GUI.color = new ColorInt(138, 229, 226).ToColor;
                    Widgets.Label(rect2, symb);
                    GUI.color = Color.white;
                    Widgets.FillableBar(visceral, rev, TRMats.grey, TRMats.mutationVisceral, false);
                    Widgets.FillableBar(symbiotic, symbNum, TRMats.mutationBlue, TRMats.grey, false);
                }
            }
        }

        [HarmonyPatch(typeof(PawnRenderer))]
        [HarmonyPatch("RenderPawnInternal")]
        [HarmonyPatch(new Type[]
        {
            typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool),
            typeof(bool)
        })]

        public static class PawnRenderPatch
        {
            [HarmonyPostfix]
            public static void Fix(PawnRenderer __instance, Vector3 rootLoc, float angle, bool renderBody,
                Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
            {
                if (!renderBody || bodyDrawType == RotDrawMode.Dessicated)
                    return;

                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                var renderComp = pawn.GetComp<Comp_CrystalDrawer>();
                Vector3 drawLoc = rootLoc;
                drawLoc.y += 0.01953125f;
                Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
                renderComp.Drawer.RenderOverlay(pawn, drawLoc, headFacing, quaternion, portrait);
            }
        }

        [HarmonyPatch(typeof(DamageWorker_AddInjury))]
        [HarmonyPatch("PlayWoundedVoiceSound")]
        public static class PlayWoundedVoiceSoundPatch
        {
            public static bool Prefix(Pawn pawn)
            {
                return !(pawn.ParentHolder is VisceralPod);
            }
        }

        [HarmonyPatch(typeof(Hediff))]
        [HarmonyPatch("CapMods", MethodType.Getter)]
        public static class CapModsPatch
        {
            public static void Postfix(ref List<PawnCapacityModifier> __result, Hediff __instance)
            {
                if(__instance is Hediff_Relative relative && !relative.def.relativeCapMods.NullOrEmpty())
                {
                    __result = relative.RelativeCapMods;
                }
            }
        }       

        [HarmonyPatch(typeof(GhostDrawer))]
        [HarmonyPatch("DrawGhostThing")]
        public static class DrawGhostThingPatch
        {
            public static bool Prefix(IntVec3 center, Rot4 rot, ThingDef thingDef, Graphic baseGraphic, Color ghostCol, AltitudeLayer drawAltitude)
            {
                if (thingDef is FXThingDef fx)
                {
                    //Log.Message("Drawing Ghost - " + thingDef);
                    if (baseGraphic == null)
                    {
                        baseGraphic = thingDef.graphic;
                    }
                    Graphic graphic = GhostUtility.GhostGraphicFor(baseGraphic, thingDef, ghostCol);
                    Vector3 loc = GenThing.TrueCenter(center, rot, thingDef.Size, drawAltitude.AltitudeFor());
                    var extraData = fx.extraData;
                    GraphicDrawInfo info = new GraphicDrawInfo(graphic, null, fx, loc, rot);
                    if (extraData?.alignToBottom ?? false)
                    {
                        loc.z += TRUtils.AlignToBottomOffset(thingDef, baseGraphic.data);
                    }
                    loc += extraData?.drawOffset ?? Vector3.zero;
                    TRUtils.Draw(graphic, loc, rot, null, null, fx);
                    //graphic.DrawFromDef(loc, rot, thingDef, 0f);
                    for (int i = 0; i < thingDef.comps.Count; i++)
                    {
                        thingDef.comps[i].DrawGhost(center, rot, thingDef, ghostCol, drawAltitude);
                    }
                    if (thingDef.PlaceWorkers != null)
                    {
                        for (int j = 0; j < thingDef.PlaceWorkers.Count; j++)
                        {
                            thingDef.PlaceWorkers[j].DrawGhost(thingDef, center, rot, ghostCol);
                        }
                    }
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ThingWithComps))]
        [HarmonyPatch("Print")]
        public static class PrintPatch
        {
            public static bool Prefix(ThingWithComps __instance, SectionLayer layer)
            {
                ThingDef def = __instance.def;
                if (__instance is Blueprint b)
                {
                    if (b.def.entityDefToBuild is TerrainDef)
                        return true;

                    def = (ThingDef)b.def.entityDefToBuild;
                }
                if (def is FXThingDef)
                {
                    TRUtils.Print(layer, __instance.Graphic, __instance, def);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(RoofGrid))]
        [HarmonyPatch("SetRoof")]
        public static class SetRoofPatch
        {
            public static void Postfix(RoofGrid __instance, IntVec3 c)
            {
                Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
                var suppression = map.GetComponent<MapComponent_Suppression>();
                if (suppression.IsInSuppressorField(c, out List<ThingWithComps> sups))
                {
                    Log.Message("Updating " + sups.Count + " suppressors.");
                    sups.ForEach(s => s.GetComp<Comp_Suppression>().UpdateCells(false));
                }
            }
        }

        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("SpawnSetup")]
        public static class SpawnSetupPatch
        {
            public static void Postfix(Thing __instance)
            {
                if (__instance is Building b && !b.CanBeSeenOver())
                {
                    var suppression = b.Map.GetComponent<MapComponent_Suppression>();
                    if (suppression.IsInSuppressorField(b.Position, out List<ThingWithComps> sups))
                    {
                        Log.Message("Updating " + sups.Count + " suppressors.");
                        sups.ForEach(s => s.GetComp<Comp_Suppression>().UpdateCells(false));
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("DeSpawn")]
        public static class DeSpawnPatch
        {
            public static bool Prefix(Thing __instance)
            {
                if (__instance is Building b && !b.CanBeSeenOver())
                {
                    var suppression = b.Map.GetComponent<MapComponent_Suppression>();
                    if (suppression.IsInSuppressorField(b.Position, out List<ThingWithComps> sups))
                    {
                        Log.Message("Updating " + sups.Count + " suppressors.");
                        sups.ForEach(s => s.GetComp<Comp_Suppression>().UpdateCells(false));
                    }
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(GenConstruct))]
        [HarmonyPatch("CanPlaceBlueprintOver")]
        public static class CanPlaceBlueprintOverPatch
        {
            public static void Postfix(ref bool __result, BuildableDef newDef, ThingDef oldDef)
            {
                if(oldDef is TiberiumCrystalDef)
                {
                    if(newDef is TRThingDef tdef)
                    {
                        __result = tdef.destroyTiberium;
                    }
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(SteadyEnvironmentEffects))]
        [HarmonyPatch("FinalDeteriorationRate", new Type[]{ typeof(Thing), typeof(bool), typeof(bool), typeof(bool), typeof(TerrainDef), typeof(List<string>) })]
        public static class FinalDeteriorationRatePatch
        {
            public static void Postfix(Thing t, bool roofed, bool roomUsesOutdoorTemperature, bool protectedByEdifice, TerrainDef terrain, ref float __result, List<string> reasons)
            {
                if (t.def.CanEverDeteriorate && t.Position.GetTiberium(t.Map) != null)
                {
                    reasons?.Add("TR_Deterioration".Translate());
                    __result *= 2.25f;
                }
            }
        }
        

        [HarmonyPatch(typeof(PawnUtility))]
        [HarmonyPatch("ShouldSendNotificationAbout")]
        public static class ShouldSendNotificationPatch
        {
            public static bool Prefix(Pawn p)
            {
                return !(p is MechanicalPawn);
            }
        }

        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch("IsColonistPlayerControlled", MethodType.Getter)]
        public static class IsColonistPatch
        {
            public static void Postfix(Pawn __instance, ref bool __result)
            {
                if(__instance is MechanicalPawn)
                {
                    __result = __instance.Spawned && (__instance.Faction != null && __instance.Faction.IsPlayer) && __instance.MentalStateDef == null && __instance.HostFaction == null;
                }
            }
        }

        [HarmonyPatch(typeof(Selector))]
        [HarmonyPatch("HandleMapClicks")]
        public static class HandleMapClicksPatch
        {
            public static void Postfix(Selector __instance)
            {
                if (__instance.SingleSelectedThing.IsPlayerControlledMech())
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        List<object> selected = Traverse.Create(__instance).Field("selected").GetValue<List<object>>();
                        if (Event.current.button == 1)
                        {
                            //TODO: Mech needs to select what to do
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(DynamicDrawManager))]
        [HarmonyPatch("DrawDynamicThings")]
        public static class DynamicDrawerPatch
        {
            public static void Postfix()
            {
                var particles = Find.CurrentMap.GetComponent<MapComponent_Particles>().SavedParticles.ToArray();
                foreach (Particle p in particles)
                {
                    p.Draw();
                }
            }
        }

        [HarmonyPatch(typeof(TickManager))]
        [HarmonyPatch("TickManagerUpdate")]
        public static class TickManagerUpdatePatch
        {
            public static void Postfix(TickManager __instance)
            {
                if (!__instance.Paused)
                {
                    var particles = Find.Maps.Select(x => x.GetComponent<MapComponent_Particles>());
                    int num = 0;
                    var mltp = __instance.TickRateMultiplier;
                    foreach (MapComponent_Particles p in particles)
                    {
                        while (num < mltp)
                        {
                            Find.CameraDriver.StartCoroutine(p.Ticker());
                            num++;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CompGlower))]
        [HarmonyPatch("UpdateLit")]
        public static class GlowPatch
        {
            public static bool Prefix(CompGlower __instance, Map map)
            {
                if(__instance.parent is FXThing)
                {
                    //!GraphicsManager.Manager.CanGlow
                    if (__instance.parent is TiberiumCrystal crystal && crystal.Parent != null && crystal.Parent.turnOffLight || !__instance.parent.Spawned)
                    {
                        map.mapDrawer.MapMeshDirty(__instance.parent.Position, MapMeshFlag.Things);
                        map.glowGrid.DeRegisterGlower(__instance);
                    }
                    else
                    {
                        map.mapDrawer.MapMeshDirty(__instance.parent.Position, MapMeshFlag.Things);
                        map.glowGrid.RegisterGlower(__instance);
                    }
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlaySettings))]
        [HarmonyPatch("DoPlaySettingsGlobalControls")]
        public static class PlaySettingsPatch
        {
            public static void Postfix(WidgetRow row, bool worldView)
            {
                if (!worldView)
                {
                    row.ToggleableIcon(ref TiberiumRimSettings.settings.ShowNetworkValues, ContentFinder<Texture2D>.Get("UI/Icons/TiberiumNetIcon"), "TiberiumNetworkValues".Translate(), SoundDefOf.Mouseover_ButtonToggle);
                }
            }
        }

        [HarmonyPatch(typeof(BillUtility))]
        [HarmonyPatch("MakeNewBill")]
        public static class MakeNewBillPatch
        {
            public static void Postfix(ref Bill __result)
            {
                if(__result.recipe is RecipeDef_Tiberium)
                {
                    TiberiumBill tibBill = new TiberiumBill(__result.recipe as RecipeDef_Tiberium);
                    __result = tibBill;
                }
            }
        }



        #region RegionPatch
        /*
        [HarmonyPatch(typeof(RegionAndRoomUpdater))]
        [HarmonyPatch("RegenerateNewRegionsFromDirtyCells")]
        public static class RegionPatch
        {
            private static List<Region> regionsBefore = new List<Region>();
            private static List<Region> newRegions = new List<Region>();
            public static bool Prefix(RegionAndRoomUpdater __instance)
            {
                regionsBefore.Clear();
                Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
                foreach (IntVec3 dirty in map.regionDirtyer.DirtyCells)
                {
                    Region[] grid = Traverse.Create(map.regionGrid).Field("regionGrid").GetValue<Region[]>();
                    Region region = grid[map.cellIndices.CellToIndex(dirty)];
                    if (!regionsBefore.Contains(region))
                    {
                        regionsBefore.Add(region);
                    }
                }
                return true;
            }

            public static void Postfix(RegionAndRoomUpdater __instance)
            {
                newRegions.Clear();
                Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
                var tiberium = map.GetComponent<MapComponent_Tiberium>();
                var info = tiberium.TiberiumInfo;

                List<Region> regions = Traverse.Create(__instance).Field("newRegions").GetValue<List<Region>>();
                newRegions.AddRange(regions);
                foreach (Region region in regionsBefore)
                {
                    if (region != null)
                    {
                        if (info.TiberiumByRegion.ContainsKey(region))
                        {
                            CellRect rect = region.Cells.ToList().ToCellRect();
                            foreach (Region @new in newRegions)
                            {
                                if (@new != null)
                                {
                                    
                                    List<IntVec3> potentialSplit = cells.FindAll(c => rect.Cells.Contains(c));

                                    if (!tiberium.CurrentRegions.TryGetValue(@new, out List<IntVec3> value))
                                    {
                                        tiberium.CurrentRegions.Add(@new, potentialSplit);
                                    }
                                    else
                                    {
                                        foreach (IntVec3 cell in potentialSplit)
                                        {
                                            if (!value.Contains(cell))
                                            {
                                                value.Add(cell);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        */
        #endregion

    }
}
