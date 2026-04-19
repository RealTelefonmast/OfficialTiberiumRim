using Verse;

namespace TiberiumRim;

public class Designator_SetStorageMode : Designator
{
    public StoreMode mode = StoreMode.RGB;

    public override int DraggableDimensions => 2;
    public override bool DragDrawMeasurements => true;

    public override AcceptanceReport CanDesignateCell(IntVec3 loc)
    {
        if (loc.InBounds(Map) && (DebugSettings.godMode || loc.Fogged(Map)) &&
            loc.GetThingList(Map).Any(t => CanDesignateThing(t).Accepted)) return true;
        return false;
    }

    public override AcceptanceReport CanDesignateThing(Thing t)
    {
        if (t is TNW_Pipe p && p.Container.mode != mode) return true;
        return false;
    }

    public override void DesignateThing(Thing t)
    {
        if (t is TNW_Pipe pipe) pipe.Container.mode = mode;
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        base.DesignateSingleCell(c);
    }
}