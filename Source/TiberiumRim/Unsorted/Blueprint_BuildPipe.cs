using RimWorld;
using Verse;

namespace TiberiumRim;

public class Blueprint_BuildPipe : Blueprint_Build
{
    private NetworkMode networkMode;
    private StoreMode storeMode;

    public void SetModes(NetworkMode nMode, StoreMode sMode)
    {
        networkMode = nMode;
        storeMode = sMode;
    }

    protected override Thing MakeSolidThing()
    {
        return base.MakeSolidThing();
    }
}