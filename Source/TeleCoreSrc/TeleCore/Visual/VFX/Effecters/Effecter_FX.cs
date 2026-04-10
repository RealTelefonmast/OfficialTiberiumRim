using TeleCore.Events.Args;
using TeleCore.ThingComps;
using Verse;

namespace TeleCore.Visual.VFX.Effecters;

public class Effecter_FX : Effecter
{
    private readonly CompFX fxComp;

    public Effecter_FX(CompFX fxComp, EffecterDef def) : base(def)
    {
        this.fxComp = fxComp;
    }

    public Effecter_FX(EffecterDef def) : base(def)
    {
    }

    internal void SpawnedEffect(FXEffecterSpawnedEventArgs spawnedEventArgs)
    {
        fxComp?.OnEffectSpawned(spawnedEventArgs);
    }
}