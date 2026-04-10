using System;
using System.Collections.Generic;
using LudeonTK;
using TeleCore.Atmosphere.Grid;
using TeleCore.Mod.Loader.Update;
using UnityEngine;
using Verse;

namespace TeleCore.Utils;

internal static class DEBUG_Tools
{
    [DebugAction("General", "[TAE]Fill Gas (Map)", false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
    private static void TeleCore_FillAllGas()
    {
        Find.CurrentMap.GetMapInfo<SpreadingGasGrid>().Debug_FillAll();
    }

    private static bool Holding_Button;

    [DebugAction("General", "[TAE]Fill Gas (AdjacentFill)", false, false, false, 0, false,
        actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap,
        displayPriority = -1000)]
    private static List<DebugActionNode> TeleCore_FillAllGasClickAdjacent()
    {
        List<DebugActionNode> list = new List<DebugActionNode>();
        foreach (var def in DefDatabase<SpreadingGasTypeDef>.AllDefsListForReading)
        {
            list.Add(new DebugActionNode(def.LabelCap, DebugActionType.ToolMap, delegate()
            {
                Find.CurrentMap.GetMapInfo<SpreadingGasGrid>().Debug_PushRadialAdjacent(UI.MouseCell(), def);
            }));
        }

        return list;
    }

    [DebugAction("General", "[TAE]Fill Gas (Smooth)", false, false, false, 0, false,
        actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap,
        displayPriority = -1000)]
    private static List<DebugActionNode> TeleCore_FillAllGasSmooth()
    {
        List<DebugActionNode> list = new List<DebugActionNode>();
        foreach (var def in DefDatabase<SpreadingGasTypeDef>.AllDefsListForReading)
        {
            list.Add(new DebugActionNode(def.LabelCap, DebugActionType.ToolMap, delegate()
            {
                var id = $"GasSmoothMaker_{def}";
                Action action = delegate
                {
                    Event curEvent = Event.current;
                    if (curEvent.type is EventType.MouseDown && curEvent.button == 0)
                    {
                        Holding_Button = true;
                    }

                    if (curEvent.type is EventType.MouseUp && curEvent.button == 0)
                    {
                        Holding_Button = false;
                    }

                    if (Holding_Button)
                    {
                        Find.CurrentMap.GetMapInfo<SpreadingGasGrid>().Debug_PushTypeRadial(UI.MouseCell(), def);
                    }

                    if (curEvent.type == EventType.MouseDown && curEvent.button == 1)
                    {
                        TeleUpdateManager.Remove_TaggedAction(TeleUpdateManager.TaggedActionType.OnGUI, id);
                    }
                };
                action.AddTaggedAction(TeleUpdateManager.TaggedActionType.OnGUI, id);
            }, null));
        }

        return list;
    }

    [DebugAction("General", "[TAE]Fill Gas (Rect)", false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100)]
    private static void ClearArea()
    {
        DebugToolsGeneral.GenericRectTool("Fill Gas", delegate(CellRect rect)
        {
            var map = Find.CurrentMap;
            rect.ClipInsideMap(map);
            foreach (IntVec3 c2 in rect)
            {
                Find.CurrentMap.GetMapInfo<SpreadingGasGrid>().Debug_AddAllAt(c2);
            }
        });
    }
}