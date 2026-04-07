using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TeleCore.Lib.Factories;
using Verse;

namespace TeleCore.Loader;

internal static class TeleStartUtils
{
    internal static void DefIDValidation()
    {
        Type defDataBase = typeof(DefDatabase<>);
        Type idStack = typeof(DefIDStack);
        var is_ToID = idStack.GetMethod("ToID");
        var is_ToDef = idStack.GetMethod("ToDef", BindingFlags.NonPublic | BindingFlags.Static);
        var is_RegisterNew = idStack.GetMethod("RegisterNew", BindingFlags.NonPublic | BindingFlags.Static);

        foreach (var defType in typeof(Def).AllSubclasses())
        {
            try
            {
                var defTypeParams = TempParams<Type>.Get(defType);
                var defDataBase_OfDef = defDataBase.MakeGenericType(defTypeParams);
                var genericList = typeof(List<>).MakeGenericType(defTypeParams);
                var defDataBase_AllDefsListForReading = defDataBase_OfDef.GetProperty("AllDefsListForReading", BindingFlags.Static | BindingFlags.Public);
                //var genericList_Count = genericList.GetProperty("Count");

                var toIDMethod = is_ToID.MakeGenericMethod(defTypeParams);
                var toDefMethod = is_ToDef.MakeGenericMethod(defTypeParams);
                var registerNewMethod = is_RegisterNew.MakeGenericMethod(defTypeParams);

                var invokedData = defDataBase_AllDefsListForReading.GetValue(null, null);
                if (invokedData is not IEnumerable data) continue;
                foreach (var defObject in data)
                {   
                    try
                    {
                        var defObjectParam = TempParams<object>.Get(defObject);
                        registerNewMethod.Invoke(null, defObjectParam);
                        var defID = (int)toIDMethod.Invoke(null, defObjectParam);
                        // ReSharper disable once HeapView.BoxingAllocation
                        var defObjectFromID = toDefMethod.Invoke(null, TempParams<object>.Get(defID));
                        if (defObject != defObjectFromID)
                        {
                            var subDefID = (int)toIDMethod.Invoke(null, TempParams<object>.Get(defObjectFromID));
                            TLog.Warning($"Checking {defObject} failed: ({defObject}){defID} != ({defObjectFromID}){subDefID}");
                        }
                    }
                    catch (Exception e)
                    {
                        TLog.Error($"Failed to validate Def ID for {defObject}: {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                TLog.Error($"Failed to validate Def IDs for {defType}: {e.Message}");
            }
        }
    }

    internal static void ExecuteDefInjectors()
    {
        var injectorTypes = typeof(DefInjectBase).AllSubclassesNonAbstract();
        if (injectorTypes.NullOrEmpty()) return;
        
        var injectors = new DefInjectBase[injectorTypes.Count];
        TLog.Debug("Found def injectors:");
        foreach (var injectorType in injectorTypes)
        {
            TLog.Debug(" - " + injectorType.Name);
        }
        for (var i = 0; i < injectorTypes.Count; i++)
            injectors[i] = (DefInjectBase)Activator.CreateInstance(injectorTypes[i]);
        
        TLog.Debug("Executing Def Injectors...");
        var allDefs = LoadedModManager.RunningMods.SelectMany(s => s.AllDefs);
        
        //TODO: Use RW's UI tips during loading to show progress
        
        foreach (var def in allDefs)
        {
            for (var i = 0; i < injectors.Length; i++)
            {
                var injector = injectors[i];
                if (injector.AcceptsSpecial(def)) injector.OnDefSpecialInjected(def);
                if (def is BuildableDef bDef) injector.OnBuildableDefInject(bDef);
                if (def is ThingDef tDef)
                {
                    injector.OnThingDefInject(tDef);
                    if (tDef.thingClass != null &&
                        (tDef.thingClass == typeof(Pawn) || tDef.thingClass.IsSubclassOf(typeof(Pawn))))
                    {
                        //tDef.comps ??= new List<CompProperties>();
                        injector.OnPawnInject(tDef);
                    }
                }
            }
        }

        injectors.Do(i => i.Dispose());
    }
}