using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public static class TRThingPatches
    {
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

        [HarmonyPatch(typeof(Projectile), "ImpactSomething")]
        public static class ProjectileImpactSomethingPatch
        {
            public static bool Prefix(Projectile __instance)
            {
                if (__instance is IPatchedProjectile patchedProj)
                {
                    return patchedProj.PreImpact();
                }

                return true;
            }

            public static void Postfix(Projectile __instance)
            {
                if (__instance is IPatchedProjectile patchedProj)
                {
                    patchedProj.PostImpact();
                }
            }
        }

        #endregion
    }
}
