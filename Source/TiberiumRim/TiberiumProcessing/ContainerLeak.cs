using System.Collections.Generic;
using TR.GameParts.Networks.TiberiumNetwork;
using Verse;

namespace TR.TiberiumProcessing;

public class ContainerLeak
{
    private Comp_TiberiumNetworkStructure parent;
    private HashSet<IntVec3> radiationCells;

    public float Severity => 0f;

    public void Tick()
    {
    }

    public void SetRadiationRadius()
    {
    }
}
