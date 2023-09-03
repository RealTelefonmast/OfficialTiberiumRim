using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace TR
{
    public static class MapWorldPatches
    {
        [HarmonyPatch(typeof(MapParent), nameof(MapParent.ShouldRemoveMapNow))]
        [HarmonyPatch(typeof(Settlement), nameof(MapParent.ShouldRemoveMapNow))]
        public static class MapParentShouldRemoveMapNow_Patch
        {
            public static void Postfix(MapParent __instance, ref bool alsoRemoveWorldObject, ref bool __result)
            {
                var isWatched = TRUtils.Tiberium().WorldDataInfo.IsSpiedOn(__instance.Map);
                if (isWatched)
                {
                    __result = false;
                    alsoRemoveWorldObject = false;
                }
            }
        }

        /*
        //Map/Game Load Patch
        [HarmonyPatch(typeof(Root_Play), nameof(Root_Play.Start))]
        public static class Root_Play_Start_Patch
        {
            static readonly MethodInfo postLoader = AccessTools.Method(typeof(Root_Play_Start_Patch), nameof(DoPostLoadFinalize));
            static readonly MethodInfo callOperand = AccessTools.Method(typeof(LongEventHandler), nameof(LongEventHandler.QueueLongEvent), new []{typeof(Action), typeof(string), typeof(bool), typeof(Action<Exception>), typeof(bool)});

            private static MethodInfo baseMethod = AccessTools.Method(typeof(Root), nameof(Root.Start));

            [HarmonyPrefix]
            public static bool Prefix(Root_Play __instance)
            {
                return true;
            }

            private static void BaseCopy(Root_Play __instance)
            {
                try
                {
                    CultureInfoUtility.EnsureEnglish();
                    Current.Notify_LoadedSceneChanged();
                    GlobalTextureAtlasManager.FreeAllRuntimeAtlases();
                    Root.CheckGlobalInit();
                    Action action = delegate ()
                    {
                        DeepProfiler.Start("Misc Init (InitializingInterface)");
                        try
                        {
                            __instance.soundRoot = new SoundRoot();
                            if (GenScene.InPlayScene)
                            {
                                __instance.uiRoot = new UIRoot_Play();
                            }
                            else if (GenScene.InEntryScene)
                            {
                                __instance.uiRoot = new UIRoot_Entry();
                            }
                            __instance.uiRoot.Init();
                            Messages.Notify_LoadedLevelChanged();
                            if (Current.SubcameraDriver != null)
                            {
                                Current.SubcameraDriver.Init();
                            }
                        }
                        finally
                        {
                            DeepProfiler.End();
                        }
                    };
                    if (!PlayDataLoader.Loaded)
                    {
                        Application.runInBackground = true;
                        LongEventHandler.QueueLongEvent(delegate ()
                        {
                            PlayDataLoader.LoadAllPlayData(false);
                        }, null, true, null, true);
                        LongEventHandler.QueueLongEvent(action, "InitializingInterface", false, null, true);
                    }
                    else
                    {
                        action();
                    }
                }
                catch (Exception arg)
                {
                    Log.Error("Critical error in root Start(): " + arg);
                }
            }

            //
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                Log.Message("Patching Root_Play Start...");
                uint callCounter = 0;
                bool added = false;
                int i = 0;
                foreach (var code in instructions)
                {
                    if (code.opcode == OpCodes.Pop)
                    {
                        callCounter++;
                    }

                    Log.Message($"[{i}][{callCounter}] {code.opcode.Name} {code.operand}");
                    yield return code;

                    if (callCounter == 3 && !added)
                    {
                        Log.Message("INSERTING POSTLOADER");
                        added = true;
                        yield return new CodeInstruction(OpCodes.Call, postLoader);
                    }
                    i++;
                }
            }
            //

            //TODO: Add potential finalizer interfaces
            static void DoPostLoadFinalize()
            {
                Log.Message($"Calling PostLoader... Current? {Current.Game?.Maps?.Count} Find?: {Find.Maps?.Count}");
                
                for (var i = 0; i < Find.Maps.Count; i++)
                {
                    var map = Find.Maps[i];
                    map.Tiberium().ThreadSafeFinalize();
                }
                
            }
        }
        */

        [HarmonyPatch(typeof(SteadyEnvironmentEffects))]
        [HarmonyPatch("DoCellSteadyEffects")]
        public static class DoCellSteadyEffectsPatch
        {
            public static void Postfix(SteadyEnvironmentEffects __instance, IntVec3 c)
            {
                __instance.map.Tiberium().CustomCellSteadyEffect(c);
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

    }
}
