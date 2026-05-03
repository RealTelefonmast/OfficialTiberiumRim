using System;
using Verse;

namespace TiberiumRim;

public class FluidBox : IExposable
{
    public TiberiumContainer Container = new(20, StoreMode.Pipe);
    private int lastFlow;
    public TiberiumNetworkBuilding parent;

    public FluidBox(TiberiumNetworkBuilding parent)
    {
        this.parent = parent;
    }

    public void ExposeData()
    {
    }

    public void TransferTo(FluidBox to)
    {
        if (Container.GetTotalStorage >= 2)
        {
            var num = ThroughPut(to);
            if (num > 0)
            {
                var value = num / Container.AllStoredTypes.Count;
                foreach (TiberiumType type in Container.AllStoredTypes)
                    Container.TryTransferTo(to.Container, type, value);
            }
        }
    }

    public int ThroughPut(FluidBox to)
    {
        var result = (int)Math.Round((Container.GetTotalStorage - (float)to.Container.GetTotalStorage) / 2f, 0,
            MidpointRounding
                .AwayFromZero); //(int)Mathf.Clamp(((this.Container.GetTotalStorage - to.Container.GetTotalStorage) / 2), 1, float.MaxValue);
        lastFlow = result;
        return result;
    }
}