using System;
using System.Collections.Generic;
using System.Linq;
using TeleCore.Defs;
using TeleCore.Types.Exposables;
using TeleCore.Types.Utils;
using UnityEngine;
using Verse;

namespace TeleCore.UI;

public class Gizmo_NetworkStorage : Gizmo
{
    private static bool optionToggled = false;
    public NetworkContainer container;

    public Gizmo_NetworkStorage()
    {
        order = -200f;
    }

    public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
    {
        get
        {
            var part = container.Capacity / container.AcceptedTypes.Count;
            yield return new FloatMenuOption("Add ALL", delegate { Debug_AddAll(part); });

            yield return new FloatMenuOption("Remove ALL", Debug_Clear);

            foreach (var type in container.AcceptedTypes)
                yield return new FloatMenuOption($"Add {type}", delegate { Debug_AddType(type, part); });
        }
    }

    public override float GetWidth(float maxWidth)
    {
        return 150; //optionToggled ? 310 : 150f;
    }

    [SyncWorker]
    private static void SyncWorkerGizNS(SyncWorker sync, ref Gizmo_NetworkStorage type)
    {
        if (sync.isWriting)
        {
            var netComp = (NetworkComponent)type.container.Parent;
            var comp = (Comp_NetworkStructure)netComp.Parent;
            sync.Write(comp);
            sync.Write(netComp.NetworkDef);
        }
        else
        {
            var comp = sync.Read<Comp_NetworkStructure>();
            var def = sync.Read<NetworkDef>();
            type = comp[def].Container.ContainerGizmo;
        }
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        var MainRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
        Find.WindowStack.ImmediateWindow(container.GetHashCode(), MainRect, WindowLayer.GameUI, delegate
        {
            var rect = MainRect.AtZero().ContractedBy(5f);
            var optionRect = new Rect(rect.xMax - 15, rect.y, 15, 15);
            var mouseOver = Mouse.IsOver(rect);
            GUI.color = mouseOver ? Color.cyan : Color.white;
            Widgets.DrawTextureFitted(optionRect, TeleContent.InfoButton, 1f);
            GUI.color = Color.white;
            /*
            if (Widgets.ButtonInvisible(rect))
                optionToggled = !optionToggled;
            */
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, container.Title);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Small;
            Widgets.Label(rect, $"{Math.Round(container.TotalStored, 0)}/{container.Capacity}");
            Text.Anchor = 0;
            var rect2 = rect.BottomHalf();
            var rect3 = rect2.BottomHalf();
            Widgets.BeginGroup(rect3);
            var BGRect = new Rect(0, 0, rect3.width, rect3.height);
            var BarRect = BGRect.ContractedBy(2.5f);
            var xPos = 0f;
            Widgets.DrawBoxSolid(BGRect, new Color(0.05f, 0.05f, 0.05f));
            Widgets.DrawBoxSolid(BarRect, new Color(0.25f, 0.25f, 0.25f));
            foreach (var type in container.AllStoredTypes)
            {
                var percent = container.ValueForType(type) / container.Capacity;
                var typeRect = new Rect(2.5f + xPos, BarRect.y, BarRect.width * percent, BarRect.height);
                var color = type.valueColor;
                xPos += BarRect.width * percent;
                Widgets.DrawBoxSolid(typeRect, color);
            }

            Widgets.EndGroup();
            /*
            if (optionToggled)
            {
                Rect Main2 = new Rect(topLeft.x + 160, topLeft.y, 150, 75f);
                DrawOptions(Main2);
            }
            */

            //Right Click Input
            var curEvent = Event.current;
            if (Mouse.IsOver(rect) && curEvent.type == EventType.MouseDown && curEvent.button == 1)
                if (DebugSettings.godMode)
                {
                    var menu = new FloatMenu(RightClickFloatMenuOptions.ToList(), "Add NetworkValue", true);
                    menu.vanishIfMouseDistant = true;
                    Find.WindowStack.Add(menu);
                }
        });
        return new GizmoResult(GizmoState.Clear);
    }

    [SyncMethod]
    private void Debug_AddAll(float part)
    {
        foreach (var type in container.AcceptedTypes) container.TryAddValue(type, part, out _);
    }

    [SyncMethod]
    private void Debug_Clear()
    {
        container.Clear();
    }

    [SyncMethod]
    private void Debug_AddType(NetworkValueDef type, float part)
    {
        container.TryAddValue(type, part, out _);
    }

    public void DrawOptions(Rect inRect)
    {
        Find.WindowStack.ImmediateWindow(1453564358, inRect, WindowLayer.GameUI, delegate { });
    }
}