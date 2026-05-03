using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public static class XMLPatches
{
    private static float CalculateMarketValue(ThingDef raceDef)
    {
        var num = 0f;
        if (raceDef == null)
        {
            TLog.Error("raceDef is null");
            return 0;
        }

        if (raceDef.race == null)
        {
            TLog.Error($"raceDef.race is null on {raceDef}");
            return 0;
        }

        if (raceDef.race.meatDef != null)
        {
            var num2 = Mathf.RoundToInt(raceDef.GetStatValueAbstract(StatDefOf.MeatAmount));
            num += num2 * raceDef.race.meatDef.GetStatValueAbstract(StatDefOf.MarketValue);
        }

        if (raceDef.race.leatherDef != null)
        {
            var num3 = Mathf.RoundToInt(raceDef.GetStatValueAbstract(StatDefOf.LeatherAmount));
            num += num3 * raceDef.race.leatherDef.GetStatValueAbstract(StatDefOf.MarketValue);
        }

        if (raceDef.butcherProducts != null && raceDef.butcherProducts.Count > 0)
            for (var i = 0; i < raceDef.butcherProducts.Count; i++)
            {
                var product = raceDef.butcherProducts[i];
                if (product?.thingDef == null)
                {
                    TLog.Error($"{raceDef}.Product: {product} | Product.thingDef: {product?.thingDef}");
                    continue;
                }

                num += product.thingDef.BaseMarketValue * product.count;
            }

        return num * 0.6f;
    }

    [HarmonyPatch(typeof(ThingDefGenerator_Corpses), nameof(ThingDefGenerator_Corpses.CalculateMarketValue))]
    public static class DefGeneratorInsight
    {
        public static bool Prefix(ThingDef raceDef, ref float __result)
        {
            __result = CalculateMarketValue(raceDef);
            return false;
        }
    }

    /*
    [HarmonyPatch]
    internal static class MonoMethod_MakeGenericMethod
    {
        [HarmonyTargetMethod]
        static MethodInfo TargetMethod()
        {
            var type = AccessTools.TypeByName("MonoMethod");
            return AccessTools.Method(type, "MakeGenericMethod", new[] {typeof(Type[])});
        }

        [HarmonyPostfix]
        public static void Postfix(Type[] methodInstantiation, ref MethodInfo __result)
        {
            if (__result.Name == nameof(DirectXmlToObject.ListFromXmlReflection))
            {
                TLog.Debug($"Creating GenericMethod [{__result.DeclaringType}]{__result.Name}: ({__result.GetGenericArguments().ToStringSafeEnumerable()} ..|.. {methodInstantiation.ToStringSafeEnumerable()})");
                var genericType = __result.GetGenericArguments()[0];
                if (DirectXmlToObject.listFromXmlMethods.TryGetValue(genericType, out var func))
                {
                    MethodInfo method = typeof(MonoMethod_MakeGenericMethod).GetMethod("ListRootChanger", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    Type[] genericArguments = genericType.GetGenericArguments();
                    //method.MakeGenericMethod(genericArguments)
                    TeleCoreMod.TeleCore.Patch(func.Method, null, new HarmonyMethod(method.MakeGenericMethod(genericArguments)));
                    //TeleCoreMod.TeleCore.Patch(func.Method, null, new HarmonyMethod(typeof(MonoMethod_MakeGenericMethod), nameof(ListRootChanger), new Type[]{typeof(XmlNode)}));
                }
            }
        }

        public static void ListRootChanger<T>(XmlNode listRootNode)
        {
            TLog.Debug("Postfixing list node");
            return;
        }
    }
    */
}