using TeleCore.Comps;
using Verse;

namespace TeleCore.Unsorted;

public class EffecterLayer
{
    private readonly Effecter _effecter;
    private FXEffecterData _data;

    public EffecterLayer(CompFX fxComp, FXEffecterData data, int index)
    {
        CompFX = fxComp;
        _effecter = GetEffecter(data.effecterDef, fxComp);

        Args = new FXEffecterArgs
        {
            index = index,
            layerTag = data.layerTag,
            needsPower = data.needsPower,
            data = data
        };
    }

    public CompFX CompFX { get; }
    public FXEffecterArgs Args { get; }


    //FX Property Getters
    private bool HasPower => CompFX.HasPower(Args);
    private bool ShouldThrowEffects => CompFX.ShouldThrowEffects(Args);

    public TargetInfo TargetAOverride => CompFX.TargetAOverride(Args);
    public TargetInfo TargetBOverride => CompFX.TargetBOverride(Args);

    public void Tick()
    {
        Tick(TargetAOverride, TargetBOverride);
    }

    public void Tick(TargetInfo A, TargetInfo B)
    {
        if (HasPower && ShouldThrowEffects) _effecter.EffectTick(A, B);
    }

    //
    private static Effecter GetEffecter(EffecterDef def, CompFX fxComp)
    {
        return def is EffecterExtendedDef exDef ? exDef.SpawnWithFX(fxComp) : def.Spawn();
    }
}