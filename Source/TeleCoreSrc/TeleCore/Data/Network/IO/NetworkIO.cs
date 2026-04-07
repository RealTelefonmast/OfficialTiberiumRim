using System.Collections.Generic;
using TeleCore.Network.Utility;
using TeleCore.Primitive;
using Verse;

namespace TeleCore.Network.IO;

/// <summary>
/// </summary>
public class NetworkIO
{
    public NetworkIO(NetIOConfig config, IntVec3 refPos, Rot4 parentRotation)
    {
        Connections = new List<IOCell>();
        VisualCells = new List<IOCell>();

        var cells = config.GetCellsFor(parentRotation);
        foreach (var cell in cells)
        {
            if (cell.mode == NetworkIOMode.None) continue;
            if ((cell.mode & NetworkIOMode.Visual) == NetworkIOMode.Visual)
            {
                VisualCells.Add(new IOCell(new IntVec3Rot(cell.offset + refPos, cell.direction), cell.mode));
                continue;
            }

            Connections.Add(new IOCell(new IntVec3Rot(cell.offset + refPos, cell.direction), cell.mode));
        }
    }

    public List<IOCell> Connections { get; }
    public List<IOCell> VisualCells { get; }

    public NetworkIOMode IOModeAt(IntVec3 pos)
    {
        foreach (var cell in Connections)
            if (cell.Pos.Pos == pos)
                return cell.Mode;
        return 0;
    }
}