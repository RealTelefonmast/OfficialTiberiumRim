using TeleCore.Events.Args;
using TeleCore.ThingComps;
using TeleCore.Visual.VFX.Effecters;
using Verse;

namespace TeleCore.Visual.VFX.FX.Effecter;

public class EffecterLayer
{
    private FXEffecterData _data;
    private readonly Verse.Effecter _effecter;

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
    private static Verse.Effecter GetEffecter(EffecterDef def, CompFX fxComp)
    {
        return def is EffecterExtendedDef exDef ? exDef.SpawnWithFX(fxComp) : def.Spawn();
    }
}