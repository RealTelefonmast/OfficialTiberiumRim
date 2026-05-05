using System;
using UnityEngine;
using Verse;

namespace TeleCore.Types;

public class DataGrid
{
    private float _width;

    public DataGrid(Vector2 pos, float maxWidth)
    {
    }

    public void DoColumnHeaders(params string[] columns)
    {
        _width = 0;
        foreach (var column in columns)
        {
            var size = Text.CalcSize(column);
            _width += size.x;
        }
    }

    public void AddRow(Action renderAction)
    {
    }
}