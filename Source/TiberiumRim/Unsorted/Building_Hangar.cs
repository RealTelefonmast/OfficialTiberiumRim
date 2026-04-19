using RimWorld;
using Verse;

namespace TR;

public class Building_Hangar : TRBuildingPrototype
{
    public MechConstructionBillStack Bills { get; private set; }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        Bills = new MechConstructionBillStack(this);
    }

    public override void Tick()
    {
        if (PowerComp is CompPowerTrader { PowerOn: false })
            return;
        Bills.Tick();
        base.Tick();
    }

    public void AddMechConstructionBill(MechRecipeDef recipe)
    {
        Bills.Add(recipe);
    }

    internal void MechConstructionFinished(MechConstructionBill bill)
    {
    }
}