using System.Collections;
using Verse;

namespace TR.Components;

public class MapComponent_TiberiumTickManager : MapComponent
{
    public MapComponent_TiberiumTickManager(Map map) : base(map)
    {
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();
    }

    private IEnumerator TickAll()
    {
        if (Find.TickManager.Paused)
            yield return null;
    }
}