using System.Linq;
using TR;

namespace TiberiumRim;

public class CompTNW_Power : CompTNW
{
    public CompPower_Tiberium power;

    public override bool[] DrawBools => new[]
        { true, StructureSet.Pipes.Any(), power.GeneratesPowerNow, power.GeneratesPowerNow };

    public override bool ShouldDoEffecters => power.GeneratesPowerNow;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        power = parent.GetComp<CompPower_Tiberium>();
    }
}