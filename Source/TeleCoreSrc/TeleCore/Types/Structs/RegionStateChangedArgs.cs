using Verse;

namespace TeleCore.Types.Structs;

public struct RegionStateChangedArgs
{
    public Map Map { get; set; }
    public IntVec3 Cell { get; set; }
    public Region Region { get; set; }
    public Room Room { get; set; }
}