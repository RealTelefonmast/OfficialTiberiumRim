using System;
using System.Collections.Generic;
using Verse;

namespace TR.TiberiumObjects;

public class CellCrawler : IExposable
{
    private readonly List<IntVec3> Points = new();
    private Map map;

    private int numCellsLeft = -1;
    private IntVec3 origin;

    private Predicate<IntVec3> Pattern;

    public CellCrawler(Map map)
    {
    }

    public void ExposeData()
    {
    }

    private void GeneratePoints()
    {
    }

    public void DrawPoints()
    {
        if (Points == null) return;
        for (var i = 0; i < Points.Count; i++)
            GenDraw.DrawCircleOutline(Points[i].ToVector3Shifted(), 1, SimpleColor.Red);
    }
}