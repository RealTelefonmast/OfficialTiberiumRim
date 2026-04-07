using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using TeleCore;
using TR.Defs;
using TR.GameParts.EVA;
using TR.Rendering.TextureContent;
using TR.ThingData.Pawns.MechanicalPawns;
using TR.Util;
using UnityEngine;
using Verse;
using TRMainButtonDef = TR.Research.TRMainButtonDef;

namespace TR.Patches;

[StaticConstructorOnStartup]
internal static class TRPatches
{
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
                GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.OnHold, blueprint);
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
            if (__instance.FleshType == TRCDefOf.Mechanical)
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
    
    //Pawn Patches
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch(nameof(Pawn.TryGetAttackVerb))]
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
    
    //Deprecated?
    /*[HarmonyPatch(typeof(Hediff))]
    [HarmonyPatch("PostAdd")]
    public static class Hediff_PostAddPatch
    {
        public static void Postfix(Hediff __instance)
        {
            //General hediff patch
            if (__instance.pawn.Faction?.IsPlayer ?? false)
            {
                TRUtils.EventManager().(__instance);
            }
        }
    }*/
    
    [HarmonyPatch(typeof(MainButtonWorker))]
    [HarmonyPatch("DoButton")]
    public static class MainButtonWorker_DoButtonPatch
    {
        /*
        public static bool Prefix(MainButtonWorker __instance)
        {
            if (__instance.props == TiberiumDefOf.TiberiumTab)
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
            return value + p.GetStatValue(TRCDefOf.ExtraCarryWeight, true);
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
            return value + StatUtility.GetStatOffsetFromList(trDef.conditionalStatOffsets, stat, pawn);
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
    
    #region EVA
    
    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch("Kill")]
    static class KillThingPatch
    {
        private static IntVec3 lastPos;

        public static bool Prefix(Thing __instance, DamageInfo? dinfo)
        {
            lastPos = __instance.Position;
            return true;
        }

        public static void Postfix(Thing __instance, DamageInfo? dinfo)
        {
            if (__instance.Faction == null) return;
            if (!__instance.Faction.IsPlayer) return;
            if (__instance is Building)
                GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.BuildingLost, lastPos);
            if (__instance is Pawn)
                GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.UnitLost, lastPos);

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
                GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.BaseUnderAttack, __instance);
            if (__instance is Pawn)
                GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.UnitUnderAttack,  __instance);
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
                GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.Cancelled, null);
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
                GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.CantDeploy, null);
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
    
    #endregion
    
    
    [HarmonyPatch(typeof(AutoHomeAreaMaker))]
    [HarmonyPatch(nameof(AutoHomeAreaMaker.Notify_BuildingClaimed))]
    static class AutoHomeAreaMakerNotify_BuildingClaimedPatch
    {
        public static void Postfix(Thing b)
        {
            if (b != null)
            {
                TRUtils.ResearchCreationTable().TryTrackConstructedOrClaimedBuilding(b.def);
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
}