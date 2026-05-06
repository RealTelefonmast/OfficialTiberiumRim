using TeleCore.Types.Structs;
using Verse;

namespace TeleCore.Rendering;

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