using System;
using System.Collections.Generic;
using TeleCore.Defs;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

/// <summary>
///     Handles additional pathing data, such as avoid weights, walkable/wander cells and danger
/// </summary>
public class PathHelperInfo : MapInformation
{
    private static readonly int SampleNumCells = GenRadial.NumCellsInRadius(8.9f);
    private readonly HashSet<AvoidGridWorker> _avoidGrids;

    public IEnumerable<AvoidGridWorker> AvoidGrids => _avoidGrids;

    public PathHelperInfo(Verse.Map map) : base(map)
    {
        _avoidGrids = new HashSet<AvoidGridWorker>();
        GlobalEventHandler.Terrain.CellChanged += Notify_CellChanged;
    }

    public override void InfoInit(bool initAfterReload = false)
    {
        base.InfoInit(initAfterReload);
        foreach (var def in DefDatabase<AvoidGridDef>.AllDefsListForReading)
        {
            var worker = (AvoidGridWorker) Activator.CreateInstance(def.avoidGridClass, map, def);
            _avoidGrids.Add(worker);
        }
    }

    private void Notify_CellChanged(CellChangedEventArgs args)
    {
        foreach (var worker in _avoidGrids)
            worker.Notify_CellChanged(args);
    }

    public override void UpdateOnGUI()
    {
        //Debug Rendering
        if (!TeleCoreDebugViewSettings.DrawAvoidGrid) return;

        var root = Verse.UI.MouseCell();
        var map = Find.CurrentMap;
        for (var i = 0; i < SampleNumCells; i++)
        {
            var intVec = root + GenRadial.RadialPattern[i];
            if (intVec.InBounds(map) && !intVec.Fogged(map))
            {
                var index = CellUtility.Index(intVec, map);
                var pathGrid = map.pathFinder.pathGrid.pathGrid[index];
                var baseVal = map.avoidGrid.grid[index];
                var newVal = 0;
                foreach (var worker in _avoidGrids) newVal += worker.Grid[index];

                var pos = GenMapUI.LabelDrawPosFor(intVec);
                GenMapUI.DrawThingLabel(pos + new Vector2(0, 32), newVal.ToString(), Color.cyan);
                GenMapUI.DrawThingLabel(pos + new Vector2(0, 0f), baseVal.ToString(), Color.green);
                GenMapUI.DrawThingLabel(pos + new Vector2(0, -32f), pathGrid.ToString(), Color.red);
            }
        }
    }
}