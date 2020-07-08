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
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.IO;
using HarmonyLib;
using UnityEngine;
using Verse.Sound;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public static class TiberiumRimPatches
    {
        static TiberiumRimPatches()
        {
            TiberiumRimMod.Tiberium.Patch(typeof(UI_BackgroundMain).GetMethod("BackgroundOnGUI"),new HarmonyMethod(typeof(TiberiumRimPatches), "BackgroundOnGUIPatch"));
            
            /*
            TiberiumRimMod.Tiberium.Patch(
            typeof(SymbolResolver_RandomMechanoidGroup).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .First(mi => mi.HasAttribute<CompilerGeneratedAttribute>() && mi.ReturnType == typeof(bool) &&
                     mi.GetParameters().Count() == 1 &&
                     mi.GetParameters()[0].ParameterType == typeof(PawnKindDef)),
            null, new HarmonyMethod(typeof(TiberiumRimPatches),
            nameof(TiberiumRimPatches.MechanoidsFixerAncient)));
            */

            TiberiumRimMod.mod.LoadAssetBundles();
            TiberiumRimMod.mod.PatchPawnDefs();
            Log.Message("[TiberiumRim] Patches Done");
        }

        public static void MechanoidsFixerAncient(ref bool __result, PawnKindDef kind)
        {
            if (typeof(MechanicalPawn).IsAssignableFrom(kind.race.thingClass)) __result = false;
        }

        //Mech Patches
        [HarmonyPatch(typeof(RaceProperties))]
        [HarmonyPatch("IsFlesh", MethodType.Getter)]
        public static class IsFleshPatch
        {
            public static void Postfix(RaceProperties __instance, ref bool __result)
            {
                if (__instance.FleshType == TiberiumDefOf.Mechanical)
                    __result = false;
            }
        }

        [HarmonyPatch(typeof(TransferableUtility))]
        [HarmonyPatch("CanStack")]
        public static class CanStackPatch
        {
            public static bool Prefix(Thing thing, ref bool __result)
            {
                if (thing is MechanicalPawn pawn)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
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
                if (def is FXThingDef fx)
                {
                    TRUtils.Print(layer, __instance.Graphic, __instance, fx);
                    return false;
                }
                return true;
            }
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
                    float viscNum = 0.5f;
                    float symbNum = 0.5f;
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

        [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal")]
        [HarmonyPatch(new Type[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) })]
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
                if (renderComp == null)
                {
                    Log.ErrorOnce("Comp_CrystalDrawer Not Applied!", 87348728);
                    return;
                }

                Vector3 drawLoc = rootLoc;
                drawLoc.y += 0.01953125f;
                Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
                renderComp.Drawer.RenderOverlay(pawn, drawLoc, headFacing, quaternion, portrait);
            }
        }

        [HarmonyPatch(typeof(GhostDrawer))]
        [HarmonyPatch("DrawGhostThing")]
        public static class DrawGhostThingPatch
        {
            public static bool Prefix(IntVec3 center, Rot4 rot, ThingDef thingDef, Graphic baseGraphic, Color ghostCol, AltitudeLayer drawAltitude)
            {
                if (!(thingDef is FXThingDef fx)) return true;
                if (baseGraphic == null)
                {
                    baseGraphic = thingDef.graphic;
                }
                Graphic graphic = GhostUtility.GhostGraphicFor(baseGraphic, thingDef, ghostCol);
                Vector3 loc = GenThing.TrueCenter(center, rot, thingDef.Size, drawAltitude.AltitudeFor());
                TRUtils.Draw(graphic, loc, rot, null, fx);

                foreach (var t in thingDef.comps)
                {
                    t.DrawGhost(center, rot, thingDef, ghostCol, drawAltitude);
                }
                if (thingDef.PlaceWorkers != null)
                {
                    foreach (var p in thingDef.PlaceWorkers)
                    {
                        p.DrawGhost(thingDef, center, rot, ghostCol);
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(VerbTracker))]
        [HarmonyPatch("GetVerbsCommands")]
        public static class GetVerbsCommandsPatch
        {
            public static IEnumerable<Command> Postfix(IEnumerable<Command> values, VerbTracker __instance)
            {
                foreach (var command in values)
                {
                    yield return command;
                }
                foreach (var verb  in __instance.AllVerbs)
                {
                    if (verb is Verb_TR verbTR)
                    {
                        if (verbTR.Props.secondaryProjectile != null)
                        {
                            yield return new Command_Action
                            {
                                defaultLabel = "Switch Projectile",
                                defaultDesc = "Current projectile: " +  verbTR.Projectile.defName,
                                action = delegate() { verbTR.SwitchProjectile(); },
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/Controls/Weapon_SwitchAmmo")
                            };
                        }
                    }
                }
                yield break;
            }
        }

        //Adding Conditional Stats
        [HarmonyPatch(typeof(StatWorker))]
        [HarmonyPatch("StatOffsetFromGear")]
        public static class StatOffsetFromGearPatch
        {
            public static float Postfix(float value, Thing gear, StatDef stat)
            {
                if (!(gear.def is TRThingDef trDef)) return value;
                if (trDef.conditionalStatOffsets.NullOrEmpty()) return value;
                if (!trDef.conditionalStatOffsets.Any(s => s.AffectsStat(stat))) return value;
                Pawn pawn = null;
                if (gear is Apparel ap)
                    pawn = ap.Wearer;

                var compEquip = gear.TryGetComp<CompEquippable>();
                if (compEquip != null)
                    pawn = compEquip.PrimaryVerb.CasterPawn;
                
                if (pawn == null) return value;
                return value + trDef.conditionalStatOffsets.GetStatOffsetFromList(stat, pawn);
            }
        }

        [HarmonyPatch(typeof(StatWorker))]
        [HarmonyPatch("GearAffectsStat")]
        public static class GearAffectsStatPatch
        {
            public static bool Postfix(bool value, ThingDef gearDef, StatDef stat)
            {
                if (gearDef is TRThingDef trDef)
                {
                    if (trDef.conditionalStatOffsets.NullOrEmpty()) return value;
                    return value || trDef.conditionalStatOffsets.Any(c => c.AffectsStat(stat));
                }
                return value;
            }
        }

        [HarmonyPatch(typeof(Pawn_ApparelTracker))]
        [HarmonyPatch("Notify_ApparelAdded")]
        public static class Notify_ApparelAddedPatch
        {
            public static void Postfix(Apparel apparel, Pawn_ApparelTracker __instance)
            {
               if(apparel.def is TRThingDef trDef && !trDef.conditionalStatOffsets.NullOrEmpty())
                   __instance.pawn.health.capacities.Notify_CapacityLevelsDirty();
            }
        }

        [HarmonyPatch(typeof(Pawn_ApparelTracker))]
        [HarmonyPatch("Notify_ApparelRemoved")]
        public static class Notify_ApparelRemovedPatch
        {
            public static void Postfix(Apparel apparel, Pawn_ApparelTracker __instance)
            {
                if (apparel.def is TRThingDef trDef && !trDef.conditionalStatOffsets.NullOrEmpty())
                    __instance.pawn.health.capacities.Notify_CapacityLevelsDirty();
            }
        }

        [HarmonyPatch(typeof(ThingDef))]
        [HarmonyPatch("SpecialDisplayStats")]
        public static class SpecialDisplayStatsPatch
        {
            public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> values, ThingDef __instance)
            {
                foreach (var entry in values)
                {
                    yield return entry;
                }

                if (__instance is TRThingDef trDef)
                {
                    if (trDef.Verbs.Any(verb => verb is VerbProperties_TR))
                    {
                        VerbProperties_TR verb = (VerbProperties_TR)__instance.Verbs.First(x => x.isPrimary);

                    }
                }
                yield break;
            }
        }

        [HarmonyPatch(typeof(PawnRenderer))]
        [HarmonyPatch("DrawEquipmentAiming")]
        public static class DrawEquipmentAimingPatch
        {
            public static bool Prefix(Thing eq, Vector3 drawLoc, float aimAngle)
            {
                if (eq is FXThing thing)
                {
                    
                    return true;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("Kill")]
        static class KillThingPatch
        {
            public static void Postfix(Thing __instance, DamageInfo? dinfo)
            {
                if (!__instance.Spawned) return;
                if (__instance.Faction == null) return;
                if (!__instance.Faction.IsPlayer) return;
                if (__instance is Building)
                    GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.BuildingLost);
                if (__instance is Pawn)
                    GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.UnitLost);

            }
        }

        [HarmonyPatch(typeof(SampleOneShotManager))]
        [HarmonyPatch("TryAddPlayingOneShot")]
        static class TryAddPlayingOneShotDebugs
        {
            public static bool Prefix(SampleOneShot newSample)
            {
                //Log.Message("Adding One Shot: " + newSample.subDef.name + " from " + newSample.subDef.parentDef.defName);
                return true;
            }
        }

        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("TakeDamage")]
        static class TakeDamagePatch
        {
            public static void Postfix(Thing __instance, DamageInfo dinfo)
            {
                //EVA Patch
                if (__instance.Destroyed || !__instance.Spawned) return;
                if (__instance.Faction == null) return;
                if (!__instance.Faction.IsPlayer) return;

                if (__instance is Building)
                    GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.BaseUnderAttack);
                if (__instance is Pawn)
                    GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.UnitUnderAttack);
            }
        }

        [HarmonyPatch(typeof(Command_Toggle))]
        [HarmonyPatch("ProcessInput")]
        static class ToggleInputPatch
        {
            public static void Postfix(Command_Toggle __instance)
            {
                var blueprint = (Thing)Find.Selector.SelectedObjects.Find(b => b is Blueprint || b is Frame);
                var forbid = blueprint?.TryGetComp<CompForbiddable>();
                if(blueprint == null || forbid == null) return;
                if (blueprint.Faction.IsPlayer && forbid.Forbidden)
                    GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.OnHold);
            }
    }

        [HarmonyPatch(typeof(Designator))]
        [HarmonyPatch("FinalizeDesignationSucceeded")]
        static class Designator_Build_FinalizeSuccPatch
        {
            public static void Postfix(Designator __instance)
            {
                if (__instance is Designator_Cancel)
                {
                    GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.Canceled);
                }
            }
        }

        [HarmonyPatch(typeof(Designator))]
        [HarmonyPatch("FinalizeDesignationFailed")]
        static class Designator_Build_FinalizeFailPatch
        {
            public static void Postfix(Designator __instance)
            {
                if (__instance is Designator_Build)
                {
                    GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.CantDeploy);
                }
            }
        }

        [HarmonyPatch(typeof(DesignatorManager))]
        [HarmonyPatch("Deselect")]
        public static class DeselectPatch
        {
            public static bool Prefix(DesignatorManager __instance)
            {
                if (__instance.SelectedDesignator is Designator_Extended d && d.MustStaySelected)
                    return false;
                return true;
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

        [HarmonyPatch(typeof(RoofGrid))]
        [HarmonyPatch("SetRoof")]
        public static class SetRoofPatch
        {
            public static void Postfix(RoofGrid __instance, IntVec3 c)
            {
                //Suppression Field Logic
                Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
                var suppression = map.GetComponent<MapComponent_Suppression>();
                if (suppression.IsInSuppressorField(c, out List<Comp_Suppression> sups))
                {
                    suppression.MarkDirty(sups);
                }
            }
        }

        //Core Thing Addons
        [HarmonyPatch(typeof(Frame))]
        [HarmonyPatch("CompleteConstruction")]
        static class CompleteConstructionPatch
        {
            public static void Postfix(Frame __instance)
            {
                //Construction Task Logic
                if (__instance != null && (__instance.def.entityDefToBuild as TerrainDef) == null)
                {
                    TRUtils.ResearchCreationTable().TryTrackCreated((ThingDef)__instance.def.entityDefToBuild);
                }
            }
        }

        [HarmonyPatch(typeof(RecordsUtility))]
        [HarmonyPatch("Notify_BillDone")]
        static class BillDonePatch
        {
            public static void Postfix(Pawn billDoer, List<Thing> products)
            {
                //Construction Task Logic
                foreach (var product in products)
                {
                    TRUtils.ResearchCreationTable().TryTrackCreated(product);
                }
            }
        }

        [HarmonyPatch(typeof(RecipeDef))]
        [HarmonyPatch("AvailableNow", MethodType.Getter)]
        internal static class RecipeDef_AvailableNowPatch
        {
            public static void Postfix(RecipeDef __instance, ref bool __result)
            {
                bool TRRequisiteDone = __instance.products.All(t => (t.thingDef as TRThingDef)?.requisites?.FulFilled() ?? true);
                __result = __result && TRRequisiteDone;
            }
        }

        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("SpawnSetup")]
        public static class SpawnSetupPatch
        {
            public static void Postfix(Thing __instance)
            {
                //Suppressor Logic
                if (__instance is Building b && !b.CanBeSeenOver())
                {
                    var suppression = b.Map.GetComponent<MapComponent_Suppression>();
                    if (suppression.IsInSuppressorField(b.Position, out List<Comp_Suppression> sups))
                    {
                        suppression.MarkDirty(sups);
                    }
                }
                //Research
                TRUtils.ResearchTargetTable().RegisterNewTarget(__instance);

                Building building = __instance as Building;
                if (building?.def.IsEdifice() ?? false)
                {
                    foreach (var cell in __instance.OccupiedRect())
                    {
                        var tib = cell.GetTiberium(__instance.Map);
                        tib?.Destroy();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("DeSpawn")]
        public static class DeSpawnPatch
        {
            private static IntVec3 instancePos;
            private static Map instanceMap;
            private static bool updateSuppressionGrid;

            public static bool Prefix(Thing __instance)
            {
                instancePos = __instance.Position;
                instanceMap = __instance.Map;
                updateSuppressionGrid = __instance is Building b && !b.CanBeSeenOver();

                //Research
                TRUtils.ResearchTargetTable().DeregisterTarget(__instance);
                return true;
            }

            public static void Postfix()
            {
                if (updateSuppressionGrid)
                {
                    var suppression = instanceMap.GetComponent<MapComponent_Suppression>();
                    if (!suppression.IsInSuppressorField(instancePos, out List<Comp_Suppression> sups)) return;
                    suppression.MarkDirty(sups);
                }
            }
        }

        [HarmonyPatch(typeof(GenConstruct))]
        [HarmonyPatch("CanPlaceBlueprintOver")]
        public static class CanPlaceBlueprintOverPatch
        {
            //This Patch allows certain structures to be placed over Tiberium
            public static void Postfix(ref bool __result, BuildableDef newDef, ThingDef oldDef)
            {
                if (oldDef is TiberiumCrystalDef)
                    __result = oldDef is TRThingDef tDef && tDef.clearTiberium;
            }
        }

        [HarmonyPatch(typeof(SteadyEnvironmentEffects))]
        [HarmonyPatch("FinalDeteriorationRate", new Type[]{ typeof(Thing), typeof(bool), typeof(bool), typeof(bool), typeof(TerrainDef), typeof(List<string>) })]
        public static class FinalDeteriorationRatePatch
        {
            public static void Postfix(Thing t, bool roofed, bool roomUsesOutdoorTemperature, bool protectedByEdifice, TerrainDef terrain, ref float __result, List<string> reasons)
            {
                if (!t.Spawned) return;
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

        [TweakValue("Patches_MSThreshold", 0, 100)]
        public static int MSThreshold = 25;

        //Custom Tick Injection
        [HarmonyPatch(typeof(TickManager))]
        [HarmonyPatch("TickManagerUpdate")]
        public static class TickManagerUpdatePatch
        {
            public static bool Prefix()
            {
                return true;
            }
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
                if (!(__instance.parent is FXThing)) return true;
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
        }

        [HarmonyPatch(typeof(PlaySettings))]
        [HarmonyPatch("DoPlaySettingsGlobalControls")]
        public static class PlaySettingsPatch
        {
            public static void Postfix(WidgetRow row, bool worldView)
            {
                if (!worldView)
                {
                    row.ToggleableIcon(ref TiberiumRimSettings.settings.EVASystem, TiberiumContent.Icon_EVA, "Enable or disable the EVA", SoundDefOf.Mouseover_ButtonToggle);
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

        [HarmonyPatch(typeof(WorldSelector))]
        [HarmonyPatch("HandleWorldClicks")]
        public static class HandleWorldClicksPatch
        {
            public static bool Prefix(WorldSelector __instance)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 1 && __instance.NumSelectedObjects > 0)
                    {
                        WorldObject obj = __instance.FirstSelectedObject;
                        if (obj is AttackSatellite asat)
                        {
                            asat.SetDestination(GenWorld.MouseTile(false));
                            Event.current.Use();
                            return false;
                        }
                    }
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(StatWorker))]
        [HarmonyPatch("GetValueUnfinalized")]
        public static class GetValueUnfinalizedPatch
        {
            public static void Postfix(ref float __result, StatWorker __instance, StatRequest req)
            {
                Pawn pawn = req.Thing as Pawn;
                
                //Patching Mechs
                if (pawn is MechanicalPawn mech)
                {

                }
            }
        }

        //Scenario Chooser Patch
        [HarmonyPatch(typeof(Page_SelectScenario))]
        [HarmonyPatch("DoScenarioListEntry")]
        public static class DoScenarioListEntryPatch
        {
            //TODO: Transpiler to replace background method with custom background method
            public static bool Prefix(Page_SelectScenario __instance, Rect rect, Scenario scen)
            {
                return true;
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
