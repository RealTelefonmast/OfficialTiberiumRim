using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    internal static class TRThingPatches
    {
        #region THING PATCHES

        //CREATION
        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("SpawnSetup")]
        public static class SpawnSetupPatch
        {
            public static void Postfix(Thing __instance)
            {
                var Tiberium = __instance.Map.Tiberium();
                //

                //Register For DataBase
                Tiberium.Notify_ThingSpawned(__instance);
                TRUtils.Tiberium().Notify_RegisterNewObject(__instance);

                //Updates On Structure Spawn
                if (__instance is Building building)
                {
                    //Radiation Logic
                    var radiation = Tiberium.TiberiumAffecter.HediffGrid;
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
                        var suppression = Tiberium.SuppressionInfo;
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

                var Tiberium = __instance.Map.Tiberium();

                Building building = __instance as Building;
                updateRadiationGrid = building != null;
                updateSuppressionGrid = updateRadiationGrid && !building.CanBeSeenOver();

                if (updateRadiationGrid)
                {
                    //Radiation Logic
                    var radiation = Tiberium.TiberiumAffecter.HediffGrid;
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
                instanceMap.Tiberium().Notify_DespawnedThing(__instance);
                return true;
            }

            public static void Postfix(Thing __instance)
            {
                var Tiberium = instanceMap.Tiberium();

                if (updateSuppressionGrid)
                {
                    var suppression = Tiberium.SuppressionInfo;
                    if (suppression.IsInSuppressionCoverage(instancePos, out List<Comp_Suppression> sups))
                    {
                        suppression.MarkDirty(sups);
                    }
                }

                if (updateRadiationGrid)
                {
                    //Radiation Logic
                    var radiation = Tiberium.TiberiumAffecter.HediffGrid;
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

        //
        [HarmonyPatch(typeof(BuildCopyCommandUtility), nameof(BuildCopyCommandUtility.FindAllowedDesignator))]
        public static class FindAllowedDesignatorPatch
        {
            public static bool Prefix(BuildableDef buildable, bool mustBeVisible, ref Designator_Build __result)
            {
                if (buildable is TRThingDef {devObject: false} trDef)
                {
                    __result = StaticData.GetDesignatorFor<Designator_Build>(trDef);
                    return false;
                }
                return true;
            }
        }

        /*
        [HarmonyPatch(typeof(BuildCopyCommandUtility), nameof(BuildCopyCommandUtility.BuildCommand))]
        public static class BuildCommandPatch
        {
            
            public static bool Prefix(BuildableDef buildable, ThingDef stuff, string label, string description, ref Command __result)
            {
                if (buildable is TRThingDef { devObject: false } trDef)
                {
                    Designator_Build des = BuildCopyCommandUtility.FindAllowedDesignator(buildable, true);

                    Command_Action buildAction = new Command_Action();
                    buildAction.defaultLabel = "Big Copy Test";
                    buildAction.defaultDesc = description;

                    buildAction.action = delegate ()
                    {
                        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
                        Find.DesignatorManager.Select(des);
                        des.SetStuffDefTemporary(stuff);
                        des.SetStuffDef(des.StuffDefRaw);
                    };

                    __result = buildAction;
                    return false;
                }
                return true;
            }
            
        }
        */
        #endregion

        #region PROJECTILE PATCHING

        /*
        [HarmonyPatch(typeof(Projectile), "DistanceCoveredFraction", MethodType.Getter)]
        public static class DistanceCoveredFractionPatch
        {
            public static void Postfix(Projectile __instance)
            {

            }
        }
        */

        [HarmonyPatch(typeof(Projectile), "Draw")]
        public static class ProjectileDrawPatch
        {
            private static MethodInfo methodToCall = AccessTools.Method(typeof(Graphics), nameof(Graphics.DrawMesh), new []{typeof(Mesh), typeof(Vector3) , typeof(Quaternion) , typeof(Material), typeof(int)});
            private static MethodInfo graphicGetter = AccessTools.PropertyGetter(typeof(Thing), "Graphic");
            private static MethodInfo injection = AccessTools.Method(typeof(ProjectileDrawPatch), nameof(GetRightMaterial));
            
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                int ignoreCount = 0;
                var instructionList = instructions.ToList();
                for (var i = 0; i < instructionList.Count; i++)
                {
                    var code = instructionList[i];
                    if (ignoreCount > 0)
                    {
                        ignoreCount--;
                        continue;
                    }

                    var codeAhead = i + 3 < instructionList.Count ? instructionList[i + 3] : null;
                    if (codeAhead != null && codeAhead.Calls(methodToCall))
                    {
                        yield return new CodeInstruction(OpCodes.Callvirt, graphicGetter);
                        yield return new CodeInstruction(OpCodes.Call, injection);
                        ignoreCount = 1;
                        continue;
                    }
                    yield return code;
                }
            }

            public static Material GetRightMaterial(Graphic graphic)
            {
                return graphic.MatSingle;
            }
        }

        #endregion
    }
}
