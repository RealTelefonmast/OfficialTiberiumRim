using System.Collections.Generic;
using Verse;

namespace TiberiumRim;

public class TiberiumFloraManager
{
    public List<TiberiumGarden> Gardens;
    public Map map;
    public List<TiberiumPond> Ponds;

    public TiberiumFloraManager(Map map)
    {
        this.map = map;
    }

    public void ManagerTick()
    {
    }

    public void Notify_PlantSpawnedFromOutside(TiberiumPlant plant)
    {
    }
}