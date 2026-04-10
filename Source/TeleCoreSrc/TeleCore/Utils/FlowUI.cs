using System;
using System.Collections.Generic;
using TeleCore.FlowCore;
using TeleCore.Generics.Container;
using TeleCore.Primitive;
using TeleCore.Static;
using UnityEngine;
using Verse;
//using Multiplayer.API;

namespace TeleCore.Utils;

public static class FlowUI<T> where T : FlowValueDef
{
    /*public static Vector2 GetValueContainerReadoutSize<TValue>(ValueContainerBase<TValue> container)
        where TValue : FlowValueDef
    {
        var size = new Vector2(10, 10);
        foreach (var type in container.StoredDefs)
        {
            var typeSize =
                Text.CalcSize(
                    $"{type.labelShort}: {container.StoredValueOf(type)} ({container.StoredPercentOf(type).ToStringPercent()})");
            size.y += 10 + 2;
            var sizeX = typeSize.x + 20;
            if (size.x <= sizeX)
                size.x += sizeX;
        }

        return size;
    }*/

    public static Vector2 GetFlowBoxReadoutSize(FlowVolumeBase<T> fb)
    {
        var size = new Vector2(10, 10);
        var stack = fb.Stack;
        if(stack.IsEmpty) return Vector2.zero;
        foreach (var fv in stack.Values)
        {
            var type = fv.Def;
            //TODO: better percent calc
            var typeSize =
                Text.CalcSize(
                    $"{type.labelShort}: {fb.StoredValueOf(fv.Def)} ({fb.StoredPercentOf(type).ToStringPercent()})");
            size.y += 10 + 2;
            var sizeX = typeSize.x + 20;
            if (size.x <= sizeX)
                size.x += sizeX;
        }

        return size;
    }


    public static float DrawFlowValueStackReadout<TDef, TValue>(Rect rect, DefValueStack<TDef, TValue> stack) 
        where TDef : FlowValueDef
        where TValue : unmanaged
    {
        float height = 5;
        Widgets.DrawMenuSection(rect);
        Widgets.BeginGroup(rect);
        Text.Font = GameFont.Tiny;
        Text.Anchor = TextAnchor.UpperLeft;
        
        if (stack.IsValid)
        {
            foreach (var fv in stack.Values)
            {
                var type = fv.Def;
                var stored = MathG.Round(stack[type].Value,1);
                var label = $"{type.labelShort}: {stored}";
                var typeRect = new Rect(5, height, 10, 10);
                var typeSize = Text.CalcSize(label);
                var typeLabelRect = new Rect(20, height - 2, typeSize.x, typeSize.y);
                Widgets.DrawBoxSolid(typeRect, type.valueColor);
                Widgets.Label(typeLabelRect, label);
                height += 10 + 2;
            }
        }
        else
        {
            Widgets.Label(new Rect(5, height, rect.width - 10, rect.height - 10), "Invalid..");
            height += 10 + 2;
        }
        
        Text.Font = default;
        Text.Anchor = default;
        Widgets.EndGroup();
        
        return height;
    }
    
    public static float DrawFlowBoxReadout(Rect rect, FlowVolumeBase<T> fb)
    {
        float height = 5;
        Widgets.DrawMenuSection(rect);
        Widgets.BeginGroup(rect);
        Text.Font = GameFont.Tiny;
        Text.Anchor = TextAnchor.UpperLeft;
        if (fb.Stack.IsValid)
        {
            foreach (var fv in fb.Stack.Values)
            {
                var type = fv.Def;
                var label = $"{type.labelShort}: {Math.Round(fb.StoredValueOf(type),1)} ({fb.StoredPercentOf(type).ToStringPercent()})";
                var typeRect = new Rect(5, height, 10, 10);
                var typeSize = Text.CalcSize(label);
                var typeLabelRect = new Rect(20, height - 2, typeSize.x, typeSize.y);
                Widgets.DrawBoxSolid(typeRect, type.valueColor);
                Widgets.Label(typeLabelRect, label);
                height += 10 + 2;
            }
        }
        else
        {
            Widgets.Label(new Rect(5, height, rect.width - 10, rect.height - 10), "Invalid..");
            height += 10 + 2;
        }

        Text.Font = default;
        Text.Anchor = default;
        Widgets.EndGroup();

        if (DebugSettings.godMode)
        {
            if (TWidgets.MouseClickIn(rect, 1))
            {
                var menu = new FloatMenu(DebugFloatMenuOptions(fb), "", true);
                menu.vanishIfMouseDistant = true;
                Find.WindowStack.Add(menu);
            }
        }

        return height;
    }

    public static List<FloatMenuOption> DebugFloatMenuOptions(FlowVolumeBase<T> fv)
    {
        var tempList = StaticListHolder<FloatMenuOption>.RequestList($"FlowMenuOptions_{fv.GetHashCode()}");
        
        if (tempList.Count == 0)
        {
            var part = (int)(fv.MaxCapacity / fv.AllowedValues.Count);
            tempList.Add(new FloatMenuOption("Add ALL", delegate
            {
                Debug_AddAll(fv, part);
            }));

            tempList.Add(new FloatMenuOption("Remove ALL", () => Debug_Clear(fv)));

            foreach (var type in fv.AllowedValues)
            {
                tempList.Add(new FloatMenuOption($"Add {type}", delegate { Debug_AddType(fv, type, part); }));
            }
        }
        
        return tempList;
    }
    
    //[SyncMethod]
    private static void Debug_AddAll(FlowVolumeBase<T> fb, double part)
    {
        foreach (var type in fb.AllowedValues)
            fb.TryAdd(type, part);
    }

    //[SyncMethod]
    private static void Debug_Clear(FlowVolumeBase<T> fv)
    {
        fv.Clear();
    }

    //[SyncMethod]
    private static void Debug_AddType(FlowVolumeBase<T> fv, T type, double part)
    {
        fv.TryAdd(type, part);
    }


    /*
    public static float DrawNetworkValueTypeReadout(Rect rect, GameFont font, float textYOffset, NetworkContainerSet containerSet)
    {
        float height = 5;

        Widgets.BeginGroup(rect);
        Text.Font = font;
        Text.Anchor = TextAnchor.UpperLeft;
        foreach (var type in containerSet.AllTypes)
        {
            // float value = GetNetwork(Find.CurrentMap).NetworkValueFor(type);
            //if(value <= 0) continue;
            string label = $"{type}: {containerSet.GetValueByType(type)}";
            Rect typeRect = new Rect(5, height, 10, 10);
            Vector2 typeSize = Text.CalcSize(label);
            Rect typeLabelRect = new Rect(20, height + textYOffset, typeSize.x, typeSize.y);
            Widgets.DrawBoxSolid(typeRect, type.valueColor);
            Widgets.Label(typeLabelRect, label);
            height += 10 + 2;
        }
        Text.Font = default;
        Text.Anchor = default;
        Widgets.EndGroup();

        return height;
    }
    */

    /*
    public static void DrawValueContainerReadout<TValue>(Rect rect, ValueContainerBase<TValue> container)
        where TValue : FlowValueDef
    {
        float height = 5;
        Widgets.DrawMenuSection(rect);
        Widgets.BeginGroup(rect);
        Text.Font = GameFont.Tiny;
        Text.Anchor = TextAnchor.UpperLeft;
        foreach (var type in container.StoredDefs)
        {
            var label =
                $"{type.labelShort}: {container.StoredValueOf(type)} ({container.StoredPercentOf(type).ToStringPercent()})";
            var typeRect = new Rect(5, height, 10, 10);
            var typeSize = Text.CalcSize(label);
            var typeLabelRect = new Rect(20, height - 2, typeSize.x, typeSize.y);
            Widgets.DrawBoxSolid(typeRect, type.valueColor);
            Widgets.Label(typeLabelRect, label);
            height += 10 + 2;
        }

        Text.Font = default;
        Text.Anchor = default;
        Widgets.EndGroup();

        if (DebugSettings.godMode)
            if (TWidgets.MouseClickIn(rect, 1))
            {
                var menu = new FloatMenu(container.DebugFloatMenuOptions, "", true);
                menu.vanishIfMouseDistant = true;
                Find.WindowStack.Add(menu);
            }
    }

    public static void HoverContainerReadout<TValue>(Rect hoverArea, ValueContainerBase<TValue> container)
        where TValue : FlowValueDef
    {
        if (container == null) return;

        //Draw Hovered Readout
        if (container.FillState != ContainerFillState.Empty && Mouse.IsOver(hoverArea))
        {
            var mousePos = Event.current.mousePosition;
            var containerReadoutSize = GetValueContainerReadoutSize(container);
            var rectAtMouse = new Rect(mousePos.x, mousePos.y - containerReadoutSize.y, containerReadoutSize.x,
                containerReadoutSize.y);
            DrawValueContainerReadout(rectAtMouse, container);
        }
    }
    */


    public static void HoverFlowBoxReadout(Rect hoverArea, FlowVolume<T> networkVolume)
    {
        if (networkVolume == null) return;

        if (networkVolume.FillState != ContainerFillState.Empty && Mouse.IsOver(hoverArea))
        {
            var mousePos = Event.current.mousePosition;
            var containerReadoutSize = GetFlowBoxReadoutSize(networkVolume);
            var rectAtMouse = new Rect(mousePos.x, mousePos.y - containerReadoutSize.y, containerReadoutSize.x,
                containerReadoutSize.y);
            DrawFlowBoxReadout(rectAtMouse, networkVolume);
        }
    }
}