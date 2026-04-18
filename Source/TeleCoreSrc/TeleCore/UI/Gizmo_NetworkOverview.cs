using System;
using System.Collections.Generic;
using RimWorld;
using TeleCore.Comps;
using TeleCore.Defs;
using TeleCore.Unsorted;
using UnityEngine;
using Verse;

namespace TeleCore.UI;

public class Gizmo_NetworkOverview : Gizmo, IDisposable
{
    //Sizes
    private const float mainWidth = 200f;
    private const int gizmoPadding = 5;
    internal const int selSettingHeight = 22;
    private readonly Dictionary<NetworkPart, NetworkInfoView> _viewByPart;
    private readonly CompNetwork _compNetwork;

    //Part Extendo Consts
    private readonly Vector2 partSelectionSize;
    private float curExtendedPartX;

    //Extendo Consts
    private float currentExtendedY;
    private float desiredExtendedPartX;
    private float desiredExtendedY;
    private FloatRange partSelRange;

    //
    public NetworkPart SelectedPart { get; private set; }
    public NetworkInfoView SelectedView => _viewByPart[SelectedPart];

    public Gizmo_NetworkOverview(CompNetwork compParent)
    {
        order = -250f;
        _compNetwork = compParent;
        _viewByPart = new Dictionary<NetworkPart, NetworkInfoView>();

        //
        var maxTextSize = 50f;
        foreach (var part in _compNetwork.NetworkParts)
        {
            _viewByPart.Add(part, new NetworkInfoView(part));
            maxTextSize = Mathf.Max(maxTextSize, Text.CalcSize(part.Config.networkDef.labelShort).x + 10);
        }

        //Set First Selection
        SelectedPart = _compNetwork.NetworkParts[0];

        //
        partSelectionSize = new Vector2(maxTextSize, 25);
        partSelRange = new FloatRange(10, MaxPartSelX());

        //
        GlobalUpdateEventHandler.UITick += Tick;
    }

    ~Gizmo_NetworkOverview()
    {
        Dispose();
    }

    public void Dispose()
    {
        GlobalUpdateEventHandler.UITick -= Tick;
    }

    public override float GetWidth(float maxWidth)
    {
        return 0;
    }

    public float GetWidthSpecial()
    {
        return mainWidth + GetPartSelectionSize().x;
    }

    private void InspectorRef(Vector2 topLeft, out Vector2 newPos, out Vector2 size)
    {
        var inspector = Find.MainTabsRoot.OpenTab.TabWindow as MainTabWindow_Inspect;
        if (inspector == null)
        {
            newPos = topLeft;
            size = new Vector2(mainWidth, mainWidth);
            return;
        }

        var inspectorSize = inspector.RequestedTabSize;
        newPos = new Vector2(inspector.RequestedTabSize.x, inspector.PaneTopY);
        size = new Vector2(mainWidth, inspectorSize.y);
    }

    private Vector2 GetPartSelectionSize()
    {
        if (_viewByPart.Count <= 1) return Vector2.zero;
        var inspector = Find.MainTabsRoot.OpenTab.TabWindow as MainTabWindow_Inspect;
        var inspectorSize = inspector.RequestedTabSize;
        var maxY = inspectorSize.y - 10;
        var fitted = Mathf.FloorToInt(partSelectionSize.y * _viewByPart.Count / maxY);
        return new Vector2((fitted + 1) * curExtendedPartX, inspectorSize.y);
    }

    private float MaxPartSelX()
    {
        var inspector = Find.MainTabsRoot.OpenTab.TabWindow as MainTabWindow_Inspect;
        var inspectorSize = inspector.RequestedTabSize;
        var maxY = inspectorSize.y - 10;
        var fitted = Mathf.FloorToInt(partSelectionSize.y * _viewByPart.Count / maxY);
        return (fitted + 1) * partSelectionSize.x;
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        InspectorRef(topLeft, out var pos, out var size);
        var mainRect = new Rect(pos, size);

        //Draw Part Selection
        if (_viewByPart.Count >= 1)
            DrawPartSelection(mainRect);

        //Draw Extendo Tabs
        DrawExtendoTabs(mainRect);

        //Draw Extended Setting
        DrawExtendedTab(mainRect);

        //Draw Main
        SelectedView.DrawMainContent(mainRect);

        //
        var firstEv = Mouse.IsOver(mainRect) ? GizmoState.Mouseover : GizmoState.Clear;
        var eventRes = Event.current.isMouse && firstEv == GizmoState.Mouseover ? GizmoState.Interacted : firstEv;
        return new GizmoResult(eventRes);
    }

    private void DrawPartSelection(Rect mainRect)
    {
        var hoverArea = new Rect(mainRect.xMax, mainRect.y, Mathf.Max(15, curExtendedPartX), mainRect.height);
        var area = new Rect(mainRect.xMax, mainRect.y + 5, curExtendedPartX, mainRect.height - 10);
        Notify_PartSelHovered(Mouse.IsOver(hoverArea));

        var curPos = area.position;
        var t = Mathf.InverseLerp(partSelRange.min, partSelRange.max, curExtendedPartX);
        var partWidth = Mathf.Max(partSelectionSize.x * t, 10);
        var partSize = new Vector2(partWidth, partSelectionSize.y);
        foreach (var partView in _viewByPart)
        {
            var part = partView.Key;
            var partRect = new Rect(curPos, partSize);
            var textRect = new Rect(new Vector2(curPos.x - (partSelRange.max - curExtendedPartX), curPos.y),
                partSelectionSize);

            var isSelected = SelectedView == partView.Value;
            var colorBG = isSelected ? TColor.OptionSelectedBGFillColor : TColor.WindowBGFillColor;
            var colorBorder = isSelected ? TColor.OptionSelectedBGBorderColor : TColor.WindowBGBorderColor;

            TWidgets.DrawColoredBox(partRect, colorBG, colorBorder, 1);
            TWidgets.DrawHighlightIfMouseOverColor(partRect, TColor.White05);
            FlowUI<NetworkValueDef>.HoverFlowBoxReadout(partRect, part.Volume);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(textRect.ContractedBy(5, 0), part.Config.networkDef.labelShort);
            Text.Anchor = default;

            if (Widgets.ButtonInvisible(partRect))
                SelectedPart = partView.Key;

            //
            curPos.y += partSelectionSize.y;
            if (curPos.y >= area.yMax)
            {
                curPos.x += partSelectionSize.x;
                curPos.y = area.y;
            }
        }
    }

    private void DrawExtendoTabs(Rect mainRect)
    {
        if (!SelectedView.HasExtensions) return;

        var yMax = Mathf.Max(15, currentExtendedY) + 10;
        var extendTriggerArea = new Rect(mainRect.x, mainRect.y - (yMax - 5), mainRect.width, yMax);
        var extendedButton = new Rect(mainRect.x, mainRect.y - (currentExtendedY + 1), mainRect.width,
            currentExtendedY + 1);
        Notify_ExtendHovered(Mouse.IsOver(extendTriggerArea));

        Widgets.DrawWindowBackground(extendedButton);
        Text.Anchor = TextAnchor.MiddleCenter;
        var curY = extendedButton.y;
        foreach (var setting in SelectedView.Tabs)
        {
            if (curY > extendedButton.yMax) continue;
            var labelRect = new Rect(extendedButton.x, curY, extendedButton.width,
                Mathf.Min(extendedButton.height, selSettingHeight));
            Widgets.Label(labelRect, setting.Key);
            Widgets.DrawHighlightIfMouseover(labelRect);
            if (Widgets.ButtonInvisible(labelRect)) SelectedView.SetExtendoTab(setting.Key);

            curY += selSettingHeight;
        }

        Text.Anchor = default;
    }

    private void DrawExtendedTab(Rect mainRect)
    {
        if (SelectedView.CurrentTab == null) return;

        //Extend Rect
        var settingRect = new Rect(mainRect.x, mainRect.y - mainRect.height, mainRect.width, mainRect.height);
        var closeButtonSize = new Vector2(settingRect.width - gizmoPadding * 2, 16);
        var offset = settingRect.width / 2 - closeButtonSize.x / 2;
        var closeButtonRect = new Rect(settingRect.x + offset, settingRect.y - closeButtonSize.y, closeButtonSize.x,
            closeButtonSize.y + gizmoPadding);

        Widgets.DrawWindowBackground(closeButtonRect);
        Widgets.DrawHighlightIfMouseover(closeButtonRect);

        Text.Anchor = TextAnchor.UpperCenter;
        //var matrix = GUI.matrix;
        //UI.RotateAroundPivot(90, closeButtonRect.center);
        Widgets.Label(closeButtonRect, "<CLOSE>");
        //GUI.matrix = matrix;
        Text.Anchor = default;

        if (Widgets.ButtonInvisible(closeButtonRect))
        {
            SelectedView.SetExtendoTab(null);
            return;
        }

        //
        SelectedView.DrawExtendedTab(settingRect);
    }

    private void Notify_ExtendHovered(bool isHovered)
    {
        desiredExtendedY = isHovered ? SelectedView.ExtendableRange.TrueMax : SelectedView.ExtendableRange.TrueMin;
    }

    private void Notify_PartSelHovered(bool isHovered)
    {
        desiredExtendedPartX = isHovered ? partSelRange.TrueMax : partSelRange.TrueMin;
    }

    private void Tick()
    {
        if (!Visible) return;
        if (Mathf.Abs(currentExtendedY - desiredExtendedY) > 0.01)
        {
            var val = desiredExtendedY > currentExtendedY ? 1.5f : -1.5f;
            currentExtendedY = Mathf.Clamp(currentExtendedY + val * SelectedView.Tabs.Count,
                SelectedView.ExtendableRange.TrueMin, SelectedView.ExtendableRange.TrueMax);
        }

        if (Mathf.Abs(curExtendedPartX - desiredExtendedPartX) > 0.01)
        {
            var val = desiredExtendedPartX > curExtendedPartX ? 3f : -3f;
            curExtendedPartX = Mathf.Clamp(curExtendedPartX + val * (partSelRange.TrueMax / partSelectionSize.x),
                partSelRange.TrueMin, partSelRange.TrueMax);
        }
    }
}