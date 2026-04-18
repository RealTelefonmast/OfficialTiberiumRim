using RimWorld;

namespace TeleCore.Stats;

public class StatWorker_GasPermeability : StatWorker
{
    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        return base.GetValueUnfinalized(req, applyPostProcess);
    }

    public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
    {
        base.FinalizeValue(req, ref val, applyPostProcess);
    }
}