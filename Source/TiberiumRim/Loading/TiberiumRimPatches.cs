using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using TiberiumRim.Utilities;
using UnityEngine;
using Verse;
using Verse.AI;
using MapInterface = RimWorld.MapInterface;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public static class TiberiumRimPatches
    {
        static TiberiumRimPatches()
        {
            Log.Message("[TiberiumRim] - Startup Init");
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

            var allinci = DefDatabase<IncidentDef>.AllDefs;
            allinci.Where(c => c.letterDef == LetterDefOf.NeutralEvent);
            allinci.Where(c => c.letterDef == LetterDefOf.NeutralEvent);

            foreach (var incidentDef in DefDatabase<IncidentDef>.AllDefs)
            {
                
            }
        }

        [HarmonyPatch(typeof(GenConstruct)), HarmonyPatch("CanPlaceBlueprintOver")]
        public static class TestPatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                MethodInfo helper = AccessTools.Method(typeof(TestPatch), nameof(GibHelp));

                bool patched = false;
                int i = 0;
                foreach (var code in instructions)
                {
                    if (code.opcode == OpCodes.Ret)
                    {
                        yield return new CodeInstruction(OpCodes.Ldc_I4, i);
                        yield return new CodeInstruction(OpCodes.Call, helper);

                        i++;
                    }
                    yield return code;
                }

            }

            public static int GibHelp(int retval , int count)
            {
                Log.Message("Returning " + (retval != 0) + " at " + count, true);
                return retval;
            }

        }

        [HarmonyPatch(typeof(Pawn_PathFollower)), HarmonyPatch("TrySetNewPath")]
        public static class TrySetNewPathPatch
        {
            //Patch the new path setter method to hook into new logic depending on the path
            public static void Postfix(Pawn_PathFollower __instance, ref bool __result, Pawn ___pawn)
            {
                //Bad path... return
                if (!__result) return;

                //If already using an airlock..
                if (___pawn.CurJobDef == TiberiumDefOf.UseAirlock) return;

                //If already in airlock which is cleaned, return
                bool airlockCondition = true;
                if (___pawn.GetRoom().Role == TiberiumDefOf.TR_AirLock && airlockCondition) return;

                //Check path for airlocks
                var rooms = __instance.curPath.RoomsAlongPath(___pawn.Map);
                Room airLock = rooms?.Find(r => r.Role == TiberiumDefOf.TR_AirLock);
                //No airlocks, no job
                if (airLock == null) return;

                //Start the airlock job and set the current job to be resumed
                Job airlockJob = JobMaker.MakeJob(TiberiumDefOf.UseAirlock, airLock.GeneralCenter());
                ___pawn.jobs.StartJob(airlockJob, JobCondition.Ongoing, null, true);
            }
        }

        /*
        [HarmonyPatch(typeof(JobDriver_Goto)), HarmonyPatch("MakeNewToils")]
        public static class GotoPatch
        {
            public static IEnumerable<Toil> Postfix(IEnumerable<Toil> values, JobDriver_Goto __instance)
            {
                Log.Message("Postfixing goto toils");
                var gotoToil = values.First();
                var endToil = values.Last();

                var endPos = __instance.pawn.CurJob.targetA;

                //Force path
                PawnPath path = __instance.pawn.Map.pathFinder.FindPath(__instance.pawn.Position, endPos,
                    __instance.pawn, PathEndMode.OnCell);
                __instance.pawn.pather.StartPath(endPos, PathEndMode.OnCell);

                //var curPath = __instance.pawn.pather.curPath;
                var pathRooms = path?.RoomsAlongPath(__instance.pawn.Map);
                Room airLock = pathRooms?.Find(r => r.Role == TiberiumDefOf.TR_AirLock);
                if (airLock == null)
                {
                    Log.Message("Found no airlock... normal goto");
                    yield return gotoToil;
                    yield return endToil;
                    yield break;
                }

                yield return Toils_Goto.GotoCell(airLock.GeneralCenter(), PathEndMode.OnCell);

                yield return Toils_General.Wait(10f.SecondsToTicks());

                yield return Toils_Goto.GotoCell(endPos.Cell, PathEndMode.OnCell);

                yield return endToil;
            }
        }
        */

        //First Startup
        [HarmonyPatch(typeof(MainMenuDrawer)), HarmonyPatch("MainMenuOnGUI"), StaticConstructorOnStartup]
        class FirstGame
        {
            [HarmonyPostfix]
            static void Fix()
            {
                TiberiumRimMod mod = TiberiumRimMod.mod; //LoadedModManager.ModHandles.FirstOrDefault(x => x is TiberiumRimMod) as TiberiumRimMod;
                if (!mod.settings.FirstStartUp) return;
                mod.settings.FirstStartUp = false;
                return; //TODO: Implement difficulty dialog
                Find.WindowStack.Add(new Dialog_Difficulty(delegate
                    {
                        mod.settings.SetEasy();
                        mod.WriteSettings();
                    }, delegate
                    {
                        mod.settings.ResetToDefault();
                        mod.WriteSettings();
                    },
                    delegate
                    {
                        mod.settings.SetHard();
                        mod.WriteSettings();
                    }));
            }
        }

        //Control Patches
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

        [HarmonyPatch(typeof(Command_Toggle))]
        [HarmonyPatch("ProcessInput")]
        static class ToggleInputPatch
        {
            public static void Postfix(Command_Toggle __instance)
            {
                var blueprint = (Thing)Find.Selector.SelectedObjects.Find(b => b is Blueprint || b is Frame);
                var forbid = blueprint?.TryGetComp<CompForbiddable>();
                if (blueprint == null || forbid == null) return;
                if (blueprint.Faction.IsPlayer && forbid.Forbidden)
                    GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.OnHold);
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
                foreach (var verb in __instance.AllVerbs)
                {
                    if (verb is Verb_TR verbTR)
                    {
                        if (verbTR.Props.secondaryProjectile != null)
                        {
                            yield return new Command_Action
                            {
                                defaultLabel = "Switch Projectile",
                                defaultDesc = "Current projectile: " + verbTR.Projectile.defName,
                                action = delegate () { verbTR.SwitchProjectile(); },
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/Controls/Weapon_SwitchAmmo")
                            };
                        }
                    }
                }
                yield break;
            }
        }


        //### Mech Patches
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
                if (__instance is MechanicalPawn)
                {
                    __result = __instance.Spawned && (__instance.Faction != null && __instance.Faction.IsPlayer) && __instance.MentalStateDef == null && __instance.HostFaction == null;
                }
            }
        }

        //World Patches
        [HarmonyPatch(typeof(WorldInspectPane))]
        [HarmonyPatch("TileInspectString", MethodType.Getter)]
        public static class TileInspectString_Patch
        {
            public static void Postfix(ref string __result)
            {
                float level = TRUtils.Tiberium().TiberiumInfo.CoverageAt(Find.WorldSelector.selectedTile);
                if (level > 0)
                {
                    __result += "\n" + "TR_WorldInfestationLevel".Translate(level.ToStringPercent());
                }
            }
        }

        //Pawn Patches
        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch("TryGetAttackVerb")]
        public static class Pawn_TryGetAttackVerbPatch
        {
            public static void Postfix(ref Verb __result, Pawn __instance)
            {
                if (!__result.WarmingUp && !__result.IsMeleeAttack) return;
                var bestHediff = __instance.BestHediffVerbFor();
                if(bestHediff != null)
                    __result = bestHediff;
            }
        }

        [HarmonyPatch(typeof(Hediff))]
        [HarmonyPatch("PostAdd")]
        public static class Hediff_PostAddPatch
        {
            public static void Postfix(Hediff __instance)
            {
                //General hediff patch
                if (__instance.pawn.Faction?.IsPlayer ?? false)
                {
                    TRUtils.EventManager().CheckForEventStart(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(Hediff_Implant))]
        [HarmonyPatch("PostAdd")]
        public static class Hediff_Implant_PostAddPatch
        {
            public static void Postfix(Hediff_Implant __instance)
            {
                __instance?.pawn.HealthComp().UpdateParts();
            }
        }

        [HarmonyPatch(typeof(Hediff_MissingPart))]
        [HarmonyPatch("PostAdd")]
        public static class Hediff_MissingPart_PostAddPatch
        {
            public static void Postfix(Hediff_Implant __instance)
            {
                __instance?.pawn.HealthComp().UpdateParts();
            }
        }

        //This adds gizmos to the pawn
        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch("GetGizmos")]
        public static class Pawn_GetGizmoPatch
        {
            public static void Postfix(ref IEnumerable<Gizmo> __result, Pawn __instance)
            {
                List<Gizmo> gizmos = new List<Gizmo>(__result);
                
                foreach (var hediff in __instance.health.hediffSet.hediffs)
                {
                    if (hediff is HediffWithGizmos gizmoDiff)
                    {
                        gizmos.AddRange(gizmoDiff.GetGizmos());
                    }
                    var gizmoComp = hediff.TryGetComp<HediffComp_Gizmo>();
                    if(gizmoComp != null)
                        gizmos.AddRange(gizmoComp.GetGizmos());
                }
                __result = gizmos;
            }
        }

        [HarmonyPatch(typeof(JobGiver_Manhunter))]
        [HarmonyPatch("TryGiveJob")]
        public class JobGiver_Manhunter_TryGiveJobPatch
        {
            public static Pawn CURRENT_ATTACKING_PAWN;
            public static bool Prefix(Pawn pawn)
            {
                CURRENT_ATTACKING_PAWN = pawn;
                return true;
            }

            public static void Postfix()
            {
                CURRENT_ATTACKING_PAWN = null;
            }
        }

        [HarmonyPatch(typeof(JobGiver_Manhunter))]
        [HarmonyPatch("MeleeAttackJob")]
        public class JobGiver_Manhunter_MeleeAttackJobPatch
        {
            private static bool Prefix(Thing target, ref Job __result)
            {
                Pawn pawn = JobGiver_Manhunter_TryGiveJobPatch.CURRENT_ATTACKING_PAWN;
                if (pawn == null) return true;
                if (!pawn.PawnHasRangedHediffVerb()) return true;
                Job job = JobMaker.MakeJob(JobDefOf.AttackStatic, target);
                //job.maxNumMeleeAttacks = 1;
                job.expiryInterval = Rand.Range(420, 900);
                job.attackDoorIfTargetLost = true; 
                __result = job;
                return false;
            }
        }

        [HarmonyPatch(typeof(JobDriver_Wait))]
        [HarmonyPatch("CheckForAutoAttack")]
        public class JobDriverWait_CheckForAutoAttackPatch
        {
            private static void Postfix(JobDriver_Wait __instance)
            {
                if (__instance.pawn.Downed)
                {
                    return;
                }
                if (__instance.pawn.Faction != null && __instance.job.def == JobDefOf.Wait_Combat && (__instance.pawn.drafter == null || __instance.pawn.drafter.FireAtWill))
                {
                    //TiberiumRimMod.CheckForAutoAttack(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(FloatMenuMakerMap))]
        [HarmonyPatch("AddDraftedOrders")]
        public static class Pawn_AddDraftedOrdersPatch
        {
            public static void Postfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
            {
                if (!pawn.PawnHasRangedHediffVerb()) return;
                foreach (LocalTargetInfo attackTarg in GenUI.TargetsAt(clickPos, TargetingParameters.ForAttackHostile(), true, null))
                {
                    Action rangedAct = HediffRangedHelper.GetRangedAttackAction(pawn, attackTarg, out string str);
                    string text = "FireAt".Translate(attackTarg.Thing.Label, attackTarg.Thing);
                    FloatMenuOption floatMenuOption = new FloatMenuOption("", null, MenuOptionPriority.High, null, attackTarg.Thing, 0f, null, null);
                    if (rangedAct == null)
                        text += ": " + str;
                    else
                    {
                        floatMenuOption.autoTakeable = (attackTarg.HasThing || attackTarg.Thing.HostileTo(Faction.OfPlayer));
                        floatMenuOption.autoTakeablePriority = 40f;
                        floatMenuOption.action = delegate ()
                        {
                            FleckMaker.Static(attackTarg.Thing.DrawPos, attackTarg.Thing.Map, FleckDefOf.FeedbackShoot, 1f);
                            rangedAct();
                        };
                    }
                    floatMenuOption.Label = text;
                    opts.Add(floatMenuOption);
                }
            }
        }

        /*
        [HarmonyPatch(typeof(Pawn_DraftController))]
        [HarmonyPatch("GetGizmos")]
        public static class DraftController_GetGizmoPatch
        {
            public static void Postfix(ref IEnumerable<Gizmo> __result, Pawn_DraftController __instance)
            {
                List<Gizmo> gizmos = new List<Gizmo>(__result);
                foreach (var hediff in __instance.health.hediffSet.hediffs)
                {
                    var gizmoComp = hediff.TryGetComp<HediffComp_Gizmo>();
                    gizmos.AddRange(gizmoComp.GetGizmos());
                }
                __result = gizmos;
            }
        }
        */

        //Render Patches
        public static bool BackgroundOnGUIPatch()
        {
            if (!TRUtils.TiberiumSettings().CustomBackground) return true;
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

        //Draw
        [HarmonyPatch(typeof(HealthCardUtility))]
        [HarmonyPatch("DrawHediffRow")]
        public static class HediffDrawerPatch
        {
            [TweakValue("Hediff_MutationHelper_Bar_contracter", -50f, 50f)]
            private static float contracted = 3f;

            [HarmonyPostfix]
            public static void Fix(Rect rect, Pawn pawn, IEnumerable<Hediff> diffs, ref float curY)
            {
                Hediff_TiberiumMutation mutation = (Hediff_TiberiumMutation)diffs.FirstOrDefault(h => h is Hediff_TiberiumMutation);
                mutation?.DrawMutation(rect, ref curY);
            }
        }

        [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal")]
        [HarmonyPatch(new Type[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(RotDrawMode), typeof(PawnRenderFlags)})]
        public static class PawnRenderer_RenderPawnInternalPatch
        {
            [HarmonyPostfix]
            public static void Fix(PawnRenderer __instance, Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
            {
                //TODO: Fix up rendering patch
                if (!renderBody || bodyDrawType == RotDrawMode.Dessicated)
                    return;

                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                var renderComp = pawn.GetComp<Comp_CrystalDrawer>();
                var drawerComp = pawn.GetComp<Comp_PawnExtraDrawer>();
                if (drawerComp == null)
                {
                    Log.ErrorOnce("Comp_PawnExtraDrawer not applied!", 2803974);
                    return;
                }
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                drawerComp.DrawExtraLayers(pawn, rootLoc, rotation, renderBody, bodyFacing, bodyDrawType, flags);

                if (renderComp == null)
                {
                    Log.ErrorOnce("Comp_CrystalDrawer Not Applied!", 87348728);
                    return;
                }

                Vector3 drawLoc = rootLoc;
                drawLoc.y += 0.01953125f;
                Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
                renderComp.Drawer.RenderOverlay(pawn, drawLoc, bodyFacing, quaternion, flags.FlagSet(PawnRenderFlags.Portrait));
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
                if (def is FXThingDef fx)
                {
                    TRUtils.Print(layer, __instance.Graphic, __instance, fx);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(GhostDrawer))]
        [HarmonyPatch("DrawGhostThing")]
        public static class DrawGhostThingPatch
        {
            public static bool Prefix(IntVec3 center, Rot4 rot, ThingDef thingDef, Graphic baseGraphic, Color ghostCol, AltitudeLayer drawAltitude)
            {
                if (!(thingDef is FXThingDef fxDef)) return true;
                if (baseGraphic == null)
                {
                    baseGraphic = thingDef.graphic;
                }
                Graphic graphic = GhostUtility.GhostGraphicFor(baseGraphic, thingDef, ghostCol);
                Vector3 loc = GenThing.TrueCenter(center, rot, thingDef.Size, drawAltitude.AltitudeFor());
                TRUtils.Draw(graphic, loc, rot, null, fxDef);

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

        // GUI.color = new Color(1f, 1f, 1f, Pulser.PulseBrightness(1.2f, 0.7f));
        // Widgets.DrawAtlas(rect2, UIHighlighter.TutorHighlightAtlas);
        // GUI.color = Color.white;

        [HarmonyPatch(typeof(MainButtonWorker))]
        [HarmonyPatch("DoButton")]
        public static class MainButtonWorker_DoButtonPatch
        {
            /*
            public static bool Prefix(MainButtonWorker __instance)
            {
                if (__instance.def == TiberiumDefOf.TiberiumTab)
                {
                    GUI.color = new Color(0f, 200f, 40f, Pulser.PulseBrightness(1.2f, 0.7f));
                }
                return true;
            }
            */

            public static void Postfix(Rect rect, MainButtonWorker __instance)
            {
                if (__instance.def.TabWindow is MainTabWindow_TibResearch tibRes && tibRes.HasUnseenProjects)
                {
                    GUI.color = new Color(0f, 200, 40f, Pulser.PulseBrightness(0.8f, 0.7f) - 0.2f);
                    Widgets.DrawAtlas(rect.ContractedBy(-10), TiberiumContent.HighlightAtlas);
                    GUI.color = Color.white;
                }
            }
        }

        //
        [HarmonyPatch(typeof(MainButtonDef))]
        [HarmonyPatch("Icon", MethodType.Getter)]
        public static class MainButtonDef_IconPatch
        {
            public static void Postfix(MainButtonDef __instance, ref Texture2D __result)
            {
                //Custom detour if its the tiberium window
                if (!(__instance is TRMainButtonDef trButton)) return;
                if (trButton.SpecialIcon == null) return;
                if (trButton.TabWindow is MainTabWindow_TibResearch research && research.HasUnseenProjects)
                {
                    __result = trButton.SpecialIcon;
                }
            }
        }

        //
        [HarmonyPatch(typeof(MassUtility))]
        [HarmonyPatch("Capacity")]
        public static class MassUtility_CapacityPatch
        {
            public static float Postfix(float value, Pawn p)
            {
                return value + p.GetStatValue(TiberiumDefOf.ExtraCarryWeight, true);
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
                        //TODO: Show extra verb attack stats
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


        //EVA NOTIFICATION PATCHES
        [HarmonyPatch(typeof(Designator))]
        [HarmonyPatch("FinalizeDesignationSucceeded")]
        static class Designator_Build_FinalizeSuccPatch
        {
            public static void Postfix(Designator __instance)
            {
                if (__instance is Designator_Cancel)
                {
                    GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.Cancelled);
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
                var suppression = map.Tiberium().SuppressionInfo;
                if (suppression.IsInSuppressionCoverage(c, out List<Comp_Suppression> sups))
                {
                    suppression.MarkDirty(sups);
                }
            }
        }

        //Core Parent Addons
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
                //Register For DataBase
                __instance.Map.Tiberium().RegisterNewThing(__instance);

                //Updates On Structure Spawn
                if (__instance is Building building)
                {
                    //Radiation Logic
                    var radiation = building.Map.Tiberium().TiberiumAffecter.HediffGrid;
                    if (radiation.IsInRadiationSourceRange(__instance.Position))
                    {
                        List<IRadiationSource> sources = radiation.RadiationSourcesAt(building.Position);
                        foreach (IRadiationSource source in sources)
                        {
                            source.Notify_BuildingSpawned(building);
                        }
                    }

                    if (!building.CanBeSeenOver())
                    {
                        var suppression = building.Map.Tiberium().SuppressionInfo;
                        if (suppression.IsInSuppressionCoverage(building.Position, out List<Comp_Suppression> sups))
                        {
                            suppression.MarkDirty(sups);
                        }
                    }

                    if (building.def.IsEdifice())
                    {
                        foreach (var cell in __instance.OccupiedRect())
                        {
                            var tib = cell.GetTiberium(__instance.Map);
                            tib?.Destroy();
                        }
                    }
                }

                //Research
                TRUtils.ResearchTargetTable().RegisterNewTarget(__instance);
                TRUtils.EventManager().CheckForEventStart(__instance);
            }
        }

        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("DeSpawn")]
        public static class DeSpawnPatch
        {
            private static IntVec3 instancePos;
            private static Map instanceMap;
            private static bool updateSuppressionGrid;
            private static bool updateRadiationGrid;

            //Radiation - On Despawn:   First Reset | Despawn | Set New

            public static bool Prefix(Thing __instance)
            {
                instancePos = __instance.Position;
                instanceMap = __instance.Map;

                Building building = __instance as Building;
                updateRadiationGrid = building != null;
                updateSuppressionGrid = updateRadiationGrid && !building.CanBeSeenOver();

                if (updateRadiationGrid)
                {
                    //Radiation Logic
                    var radiation = building.Map.Tiberium().TiberiumAffecter.HediffGrid;
                    if (radiation.IsInRadiationSourceRange(instancePos))
                    {
                        List<IRadiationSource> sources = radiation.RadiationSourcesAt(building.Position);
                        foreach (IRadiationSource source in sources)
                        {
                            if (source.SourceThing == building || !source.SourceThing.Spawned) continue;
                            source.Notify_BuildingDespawning(building);
                        }
                    }
                }

                //Research
                TRUtils.ResearchTargetTable().DeregisterTarget(__instance);

                //Register For DataBase
                instanceMap.Tiberium().DeregisterThing(__instance);
                return true;
            }

            public static void Postfix(Thing __instance)
            {
                if (updateSuppressionGrid)
                {
                    var suppression = instanceMap.Tiberium().SuppressionInfo;
                    if (suppression.IsInSuppressionCoverage(instancePos, out List<Comp_Suppression> sups))
                    {
                        suppression.MarkDirty(sups);
                    }
                }

                if (updateRadiationGrid)
                {
                    //Radiation Logic
                    var radiation = instanceMap.Tiberium().TiberiumAffecter.HediffGrid;
                    if (radiation.IsInRadiationSourceRange(instancePos))
                    {
                        List<IRadiationSource> sources = radiation.RadiationSourcesAt(instancePos);
                        foreach (IRadiationSource source in sources)
                        {
                            if (source.SourceThing == __instance || !source.SourceThing.Spawned) continue;
                            source.Notify_UpdateRadiation();
                        }
                    }
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
                    row.ToggleableIcon(ref TRUtils.GameSettings().EVASystem, TiberiumContent.Icon_EVA, "Enable or disable the EVA", SoundDefOf.Mouseover_ButtonToggle);
                    row.ToggleableIcon(ref TRUtils.GameSettings().RadiationOverlay, TiberiumContent.Hediff_Radiation, "Toggle the Tiberium Radiation overlay.", SoundDefOf.Mouseover_ButtonToggle);
                }
            }
        }

        [HarmonyPatch(typeof(BillUtility))]
        [HarmonyPatch("MakeNewBill")]
        public static class MakeNewBillPatch
        {
            public static void Postfix(ref Bill __result)
            {
                if(__result.recipe is TRecipeDef)
                {
                    TiberiumBill tibBill = new TiberiumBill(__result.recipe as TRecipeDef);
                    __result = tibBill;
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

        [HarmonyPatch(typeof(ThingSetMaker_Meteorite))]
        [HarmonyPatch("Reset")]
        public static class ThingSetMaker_Meteorite_ResetPatch
        {
            public static void Postfix()
            {
                Traverse.Create(typeof(ThingSetMaker_Meteorite)).Field("nonSmoothedMineables").GetValue<List<ThingDef>>().RemoveAll(t => t is TRThingDef);
            }
        }

        [HarmonyPatch(typeof(ThingDefGenerator_Buildings))]
        [HarmonyPatch("NewFrameDef_Thing")]
        public static class NewFrameDef_ThingPatch
        {
            public static void Postfix(ThingDef def, ref ThingDef __result)
            {
                if (def is TRThingDef trDef)
                {
                    foreach (var stat in trDef.statBases)
                    {
                        //if(__result.StatBaseDefined(stat.stat)) continue;
                        __result.SetStatBaseValue(stat.stat, stat.value);
                    }
                }
            }
        }

        //Incidents
        [HarmonyPatch(typeof(IncidentWorker_SelfTame))]
        [HarmonyPatch("Candidates")]
        public static class IncidentWorker_SelfTamePatch
        {
            public static IEnumerable<Pawn> Postfix(IEnumerable<Pawn> values, ThingDef __instance)
            {
                foreach (var entry in values)
                {
                    if (entry.kindDef is TiberiumKindDef) continue;

                    yield return entry;
                }
            }
        }

        //### TRANSPILER AREA - WE GOING DEEP BOY

        //### Update Call Injections
        //# Tick Update

        //# Render Update
        [HarmonyPatch(typeof(MapInterface))]
        [HarmonyPatch("MapInterfaceUpdate")]
        public static class MapInterfaceUpdatePatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                MethodInfo helper = AccessTools.Method(typeof(MapInterfaceUpdatePatch), nameof(TiberiumMapInterfaceUpdate));
                MethodInfo updateCall = AccessTools.Method(typeof(DeepResourceGrid), "DeepResourceGridUpdate");

                bool patched = false;
                foreach (var code in instructions)
                {
                    yield return code;
                    if (code.Calls(updateCall) && !patched)
                    {
                        yield return new CodeInstruction(OpCodes.Call, helper); //Consumes 1 and returns Rect
                        patched = true;
                    }
                }
            }

            public static void TiberiumMapInterfaceUpdate()
            {
                Find.CurrentMap.Tiberium().TiberiumMapInterfaceUpdate();
            }
        }

        //MessageReadOut
        [HarmonyPatch(typeof(Message))]
        [HarmonyPatch("Draw")]
        public static class MessageDrawPatch
        {

        }

        //Readout
        [HarmonyPatch(typeof(ResourceReadout))]
        [HarmonyPatch("ResourceReadoutOnGUI")]
        public static class ResourceReadoutOnGUIPatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                MethodInfo helper = AccessTools.Method(typeof(ResourceReadoutOnGUIPatch), nameof(AdjustResourceReadoutDownwards));

                bool patched = false;
                foreach (var code in instructions)
                {
                    yield return code;
                    if (code.opcode == OpCodes.Stloc_0 && !patched)
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_0); //Rect on stack  
                        yield return new CodeInstruction(OpCodes.Call, helper); //Consumes 1 and returns Rect
                        yield return new CodeInstruction(OpCodes.Stloc_0);
                        patched = true;
                    }
                }

            }

            public static void Postfix(ResourceReadout __instance)
            {
                if (ShouldFix)
                    DrawCredits();
            }

            static Rect AdjustResourceReadoutDownwards(Rect rect)
            {
                if (!ShouldFix) return rect;

                Rect newRect = new Rect(rect);
                newRect.yMin += TotalHeight.Value + 10f;
                return newRect;
            }

            private static bool ShouldFix => GetTiberiumCredits(Find.CurrentMap) > 0;

            static float GetTiberiumCredits(Map map)
            {
                return GetNetwork(map)?.TotalStorageNetworkValue ?? 0;
            }

            private static float? TotalHeight = 120;
            private static float? ResourceReadoutHeight = 60f;

            static Network GetNetwork(Map map)
            {
                return map.Tiberium()?.NetworkInfo[NetworkType.TiberiumProcessing]?.MainNetworkStructure?.Network ?? null;
            }
            
            static void DrawCredits()
            {
                //
                Rect mainRect = new Rect(5f, 10, 120f, TotalHeight.Value);
                string creditLabel = "TR_Credits".Translate();
                Vector2 creditLabelSize = Text.CalcSize(creditLabel);
                Rect creditLabelRect = new Rect(5, mainRect.y, mainRect.width, creditLabelSize.y + 8);
                Rect readoutRect = new Rect(5, creditLabelRect.yMax, mainRect.width, ResourceReadoutHeight.Value);

                //Main
                //Widgets.DrawWindowBackground(mainRect);
                TRWidgets.DrawColoredBox(mainRect, new Color(1,1,1,0.125f), Color.white, 1);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(creditLabelRect, creditLabel); 
                Text.Anchor = default;
                Widgets.DrawLine(new Vector2(5f, creditLabelRect.yMax), new Vector2(125f, creditLabelRect.yMax), Color.white, 1f);

                ResourceReadoutHeight = TRWidgets.DrawNetworkValueTypeReadout(readoutRect, GameFont.Tiny, -2, GetNetwork(Find.CurrentMap).ContainerSet.TotalValueByType);

                Text.Font = GameFont.Tiny;
                string totalLabel = "TR_CreditsTotal".Translate(Math.Round(GetTiberiumCredits(Find.CurrentMap)));
                Vector2 totalLabelSize = Text.CalcSize(totalLabel);
                Rect totalLabelRect = new Rect(10f, readoutRect.yMax, mainRect.width, totalLabelSize.y);
                Widgets.Label(totalLabelRect, totalLabel);
                Text.Font = default;

                TotalHeight = totalLabelRect.yMax - mainRect.y;
                //
            }
        }
    }

    [HarmonyPatch(typeof(Room))]
    [HarmonyPatch("Notify_RoofChanged")]
    public static class Notify_RoofChangedPatch
    {
        public static void Postfix(Room __instance)
        {
            __instance.Map.Tiberium().RoomInfo.updater.Notify_RoofChanged(__instance);
        }
    }

    [HarmonyPatch(typeof(RegionAndRoomUpdater))]
    [HarmonyPatch("NotifyAffectedDistrictsAndRoomsAndUpdateTemperature")]
    public static class NotifyAffectedRoomsAndRoomGroupsAndUpdateTemperaturePatch
    {
        public static bool Prefix(Map ___map, List<Room> ___newRooms, HashSet<Room> ___reusedOldRooms)
        {
            ___map.Tiberium().RoomInfo.updater.Notify_SetNewRoomData(___newRooms, ___reusedOldRooms);
            return true;
        }
    }

    [HarmonyPatch(typeof(RegionAndRoomUpdater))]
    [HarmonyPatch("CreateOrUpdateRooms")]
    public static class CreateOrUpdateRoomsPatch
    {
        public static bool Prefix(Map ___map)
        {
            ___map.Tiberium().RoomInfo.updater.Notify_RoomUpdatePrefix();
            return true;
        }

        
        public static void Postfix(Map ___map)
        {
            ___map.Tiberium().RoomInfo.updater.Notify_RoomUpdatePostfix();
        }

    }

    [HarmonyPatch(typeof(TemperatureCache))]
    [HarmonyPatch("TryCacheRegionTempInfo")]
    public static class TryCacheRegionTempInfoPatch
    {
        public static void Postfix(IntVec3 c, Region reg, Map ___map)
        {
            ___map.Tiberium().PollutionInfo.Cache.TryCacheRegionPollutionInfo(c, reg);
        }

    }

    [HarmonyPatch(typeof(TemperatureCache))]
    [HarmonyPatch("ResetCachedCellInfo")]
    public static class ResetCachedCellInfoPatch
    {
        public static void Postfix(IntVec3 c, Map ___map)
        {
            ___map.Tiberium().PollutionInfo.Cache.ResetInfo(c);
        }
    }

    [HarmonyPatch(typeof(TemperatureSaveLoad))]
    [HarmonyPatch("ApplyLoadedDataToRegions")]
    public static class ApplyLoadedDataToRegionsPatch
    {
        public static void Postfix(Map ___map)
        {
            ___map.Tiberium().PollutionInfo.Cache.pollutionSaveLoad.ApplyLoadedDataToRegions();
        }
    }



    /*
    [HarmonyPatch(typeof(RoomGroup))]
    [HarmonyPatch("AddRoom")]
    public static class AddRoomPatch
    {
        public static void Postfix(RoomGroup __instance, Room room)
        {
            room.Map.Tiberium().PollutionInfo.RoomGroupAddedRoom(__instance, room);
        }
    }

    [HarmonyPatch(typeof(RoomGroup))]
    [HarmonyPatch("RemoveRoom")]
    public static class RemoveRoomPatch
    {
        public static void Postfix(RoomGroup __instance, Room room)
        {
            room.Map.Tiberium().PollutionInfo.RoomGroupRemovedRoom(__instance, room);
        }
    }
    */
    /*
    [HarmonyPatch(typeof(RegionAndRoomUpdater))]
    [HarmonyPatch("RegenerateNewRegionsFromDirtyCells")]
    public static class RegionPatch
    {
        private static readonly List<Region> oldRegions = new List<Region>(); 
        private static readonly List<Region> newRegions = new List<Region>();

        public static bool Prefix(RegionAndRoomUpdater __instance)
        {
            Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
            RegionGrid grid = Traverse.Create(map).Field("regionGrid").GetValue<RegionGrid>();

            oldRegions.Clear();
            map.Tiberium().TiberiumInfo.regionGrid.RemoveDirtyGrids(map.regionDirtyer.DirtyCells);
            foreach (IntVec3 dirty in map.regionDirtyer.DirtyCells)
            {
                oldRegions.AddDistinct(grid.GetValidRegionAt(dirty));
            }
            return true;
        }

        public static void Postfix(RegionAndRoomUpdater __instance)
        {
            var instance = Traverse.Create(__instance);
            Map map = instance.Field("map").GetValue<Map>();
            var tiberium = map.Tiberium();
            var info = tiberium.TiberiumInfo;
            List<Region> regions = instance.Field("newRegions").GetValue<List<Region>>();

            newRegions.Clear();
            newRegions.AddRange(regions);

            info.regionGrid.Notify_RegionSplit(oldRegions,newRegions);
        }
    }
    */

}
