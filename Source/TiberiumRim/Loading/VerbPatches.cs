using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;

namespace TiberiumRim
{
    public static class VerbPatches
    {
        [HarmonyPatch(typeof(VerbTracker), "CreateVerbTargetCommand")]
        public static class CreateVerbTargetCommandPatch
        {
            public static void Postfix(Command_VerbTarget __result, Verb verb)
            {
                if (!verb.Available())
                    __result.Disable("Not available...");
            }
        }

        [HarmonyPatch(typeof(VerbProperties), "LaunchesProjectile", MethodType.Getter)]
        public static class VerbPropertiesLaunchesProjectilePatch
        {
            public static bool Prefix(VerbProperties __instance, ref bool __result)
            {
                if (__instance is VerbProperties_TR propsTR)
                {
                    __result = propsTR.isProjectile;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(VerbUtility), "GetProjectile")]
        public static class VerbUtilityGetProjectilePatch
        {
            public static bool Prefix(Verb verb, ref ThingDef __result)
            {
                if (verb is Verb_TR verbTR)
                {
                    __result = verbTR.Projectile;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(VerbUtility), "GetDamageDef")]
        public static class VerbUtilityGetDamageDefPatch
        {
            public static bool Prefix(Verb verb, ref DamageDef __result)
            {
                if (verb is Verb_TR verbTR)
                {
                    __result = verbTR.DamageDef;
                    return false;
                }
                return true;
            }
        }
    }
}
