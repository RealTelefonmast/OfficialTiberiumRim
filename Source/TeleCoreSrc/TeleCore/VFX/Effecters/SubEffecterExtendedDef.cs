using Verse;

namespace TeleCore.VFX.Effecters;

public enum EffectThrowMode
{
    Burst,
    ChancePerTick,
    Continuous
}

public class SubEffecterExtendedDef : SubEffecterDef
{
    public bool affectedByWind;
    public IntRange burstInterval = new(0, 0);
    public EffectThrowMode effectMode = EffectThrowMode.Continuous;

    public string eventTag;
    public PositionOffsets originOffsets;
    public IntRange soundInterval = new(40, 100);
    public IntRange throwInterval = new(40, 100);

    public override string ToString()
    {
        return base.ToString();
    }
}