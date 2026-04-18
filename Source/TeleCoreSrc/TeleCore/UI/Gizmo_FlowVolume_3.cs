using System.Collections.Generic;
using System.Linq;
using TeleCore.Defs;
using TeleCore.Unsorted;
using UnityEngine;
using Verse;

//using Multiplayer.API;

namespace TeleCore.UI;

/// <summary>
/// A simple overview of a <see cref="FlowVolume{T}"/>'s contents.
/// </summary>
public class Gizmo_FlowVolume<T> : Gizmo where T : FlowValueDef
{
    protected readonly FlowVolume<T> _volume;

    public string Label { get; set; }

    public sealed override float GetWidth(float maxWidth)
    {
        return 150; //optionToggled ? 310 : 150f;
    }

    public sealed override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        var inRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
        var hash = _volume.GetHashCode();
        Find.WindowStack.ImmediateWindow(hash, inRect, WindowLayer.GameUI, delegate
        {
            var rect = inRect.AtZero().ContractedBy(5f);
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
            Widgets.Label(rect, Label);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Small;
            Widgets.Label(rect, $"{_volume.TotalValue}/{_volume.MaxCapacity}");
            Text.Anchor = 0;
            var rect2 = rect.BottomHalf();
            var rect3 = rect2.BottomHalf();
            //
            Widgets.BeginGroup(rect3);
            {
                var BGRect = new Rect(0, 0, rect3.width, rect3.height);
                var BarRect = BGRect.ContractedBy(2.5f);
                var xPos = 0f;
                Widgets.DrawBoxSolid(BGRect, new Color(0.05f, 0.05f, 0.05f));
                Widgets.DrawBoxSolid(BarRect, new Color(0.25f, 0.25f, 0.25f));
                foreach (var type in _volume.Stack)
                {
                    var percent = _volume.StoredPercentOf(type);
                    var typeRect = new Rect(2.5f + xPos, BarRect.y, BarRect.width * percent, BarRect.height);
                    var color = type.Def.valueColor;
                    xPos += BarRect.width * percent;
                    Widgets.DrawBoxSolid(typeRect, color);
                }
            }
            Widgets.EndGroup();

            //Right Click Input
            var curEvent = Event.current;
            if (Mouse.IsOver(rect) && curEvent.type == EventType.MouseDown && curEvent.button == 1)
            {
                if (DebugSettings.godMode)
                {
                    var menu = new FloatMenu(RightClickFloatMenuOptions.ToList(), "Add NetworkValue", true);
                    menu.vanishIfMouseDistant = true;
                    Find.WindowStack.Add(menu);
                }
            }
        });
        return new GizmoResult(GizmoState.Clear);
    }

    public sealed override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
    {
        get
        {
            var part = _volume.MaxCapacity / (float)_volume.AllowedValues.Count;
            yield return new FloatMenuOption("Add ALL", delegate { Debug_AddAll((int)part); });

            yield return new FloatMenuOption("Remove ALL", Debug_Clear);

            foreach (var type in _volume.AllowedValues)
                yield return new FloatMenuOption($"Add {type}", delegate { Debug_AddType(type, (int)part); });
        }
    }

    protected virtual void Debug_AddAll(int part)
    {
    }

    protected virtual void Debug_Clear()
    {
    }

    protected virtual void Debug_AddType(T type, int part)
    {
    }
}

//This needs to be done because otherwise sync methods wont work due to generic class BS
public class Gizmo_NetworkVolume : Gizmo_FlowVolume<NetworkValueDef>
{
    //[SyncMethod]
    protected override void Debug_AddAll(int part)
    {
        foreach (var type in _volume.AllowedValues)
            _volume.TryAdd(type, part);
    }

    //[SyncMethod]
    protected override void Debug_Clear()
    {
        _volume.Clear();
    }

    //[SyncMethod]
    protected override void Debug_AddType(NetworkValueDef type, int part)
    {
        _volume.TryAdd(type, part);
    }
}