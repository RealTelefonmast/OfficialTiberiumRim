using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public class PositionOffsets
{
    public List<Vector3> east;
    public List<Vector3> north;
    public List<Vector3> south;
    public List<Vector3> west;

    public List<Vector3> PositionsSingle => north;

    public List<Vector3> this[Rot4 rot]
    {
        get
        {
            if (rot == Rot4.North) return north;
            if (rot == Rot4.East && !east.NullOrEmpty()) return east;
            if (rot == Rot4.South && !south.NullOrEmpty()) return south;
            if (rot == Rot4.West && !west.NullOrEmpty()) return west;
            return PositionsSingle;
        }
    }
}