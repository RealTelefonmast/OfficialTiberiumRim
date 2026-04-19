using System.Collections;
using System.Collections.Generic;
using Verse;

namespace TiberiumRim;

public class MapComponent_TiberiumEntityManager : MapComponent
{
    public TiberiumEntityGrid EntityGrid;
    private int masterID = -1;
    public List<TiberiumEntity> TiberiumEntities = new();

    private int ticksPassed;

    public MapComponent_TiberiumEntityManager(Map map) : base(map)
    {
        EntityGrid = new TiberiumEntityGrid(map);
    }

    public bool CanTick
    {
        get
        {
            if (ticksPassed >= 500)
            {
                ticksPassed = 0;
                return true;
            }

            ticksPassed++;
            return false;
        }
    }

    public IEnumerator Ticker()
    {
        yield return null;
        if (CanTick)
        {
            Log.Message("Ticking Tib Entities: " + TiberiumEntities.Count);
            foreach (var entity in TiberiumEntities) entity.Tick();
        }
    }

    public void RegisterEntity(TiberiumEntity entity)
    {
        entity.ID = masterID += 1;
        EntityGrid.Set(entity.Position, true, entity);
        TiberiumEntities.Add(entity);
        Log.Message("Registering entity with ID " + entity.ID);
    }

    public void DeRegisterEntity(TiberiumEntity entity)
    {
        EntityGrid.Set(entity.Position, false, null);
        TiberiumEntities.Remove(entity);
    }
}