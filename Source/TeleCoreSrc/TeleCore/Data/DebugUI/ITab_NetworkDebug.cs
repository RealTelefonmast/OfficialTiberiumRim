using System.Collections.Generic;
using RimWorld;
using TeleCore.Defs;
using TeleCore.Network;
using TeleCore.Rendering.Widgets;
using TeleCore.ThingComps;
using UnityEngine;
using Verse;

namespace TeleCore;

public class ITab_NetworkDebug : ITab
{
    //
    private static readonly Vector2 WinSize = new(640, 480f);

    //
    private Vector2 scrollPos = Vector2.zero;
    private NetworkDef selTab;
    private List<TabRecord> tabs;

    public ITab_NetworkDebug()
    {
        size = WinSize;
        labelKey = "TELE.DEGBUG";
    }

    private CompNetwork NetworkStructure => SelThing.TryGetComp<CompNetwork>();
    public INetworkPart SelPart => NetworkStructure[selTab];

    //
    public override bool Hidden => !DebugSettings.godMode;

    public override void OnOpen()
    {
        base.OnOpen();

        tabs = new List<TabRecord>();
        foreach (var networkPart in NetworkStructure.NetworkParts)
        {
            var def = networkPart.Config.networkDef;
            tabs.Add(new TabRecord(def.defName, () => { selTab = def; }, false));
        }
    }

    public override void FillTab()
    {
        var inRect = TabRect.AtZero();
        var tabDrawerRect = inRect.TopPartPixels(TabDrawer.TabHeight);
        var contentRect = inRect.BottomPartPixels(inRect.height - tabDrawerRect.height);

        //
        TabDrawer.DrawTabs(contentRect, tabs);
        if (selTab == null) return;

        Widgets.BeginGroup(contentRect);
        {
            var scrollArea = new Rect(0, 0, TabRect.width, contentRect.height);
            var viewRect = new Rect(0, 0, scrollArea.width, scrollArea.height * 4);
            Widgets.BeginScrollView(scrollArea, ref scrollPos, viewRect);
            {
                var innerRect = new Rect(0, 0, TabRect.width, TabRect.height);

                Widgets.DrawHighlight(innerRect);

                //WidgetStackPanel readout = new WidgetStackPanel();
                WidgetStackPanel.Begin(innerRect);
                WidgetStackPanel.DrawHeader($"Debug '{SelThing}'");
                WidgetStackPanel.DrawDivider();

                WidgetStackPanel.DrawDivider();

                //
                if (SelPart.Network.Graph.TryGetAdjacencyList(SelPart, out var adjacencyList))
                {
                    WidgetStackPanel.DrawRow("AdjacencyList: ", $"{adjacencyList?.Count}");
                    foreach (var part in adjacencyList) WidgetStackPanel.DrawRow("", $"{part}");
                }

                WidgetStackPanel.End();
            }
            Widgets.EndScrollView();
        }
        Widgets.EndGroup();
    }
}