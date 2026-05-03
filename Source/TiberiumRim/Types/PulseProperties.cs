using Verse;

namespace TiberiumRim;

public class PulseProperties
{
    public PulseMode mode = PulseMode.Opacity;
    public int opacityDuration = 60;
    public int opacityOffset;
    public FloatRange opacityRange = new(0f, 1f);
    public int sizeDuration = 60;
    public int sizeOffset;
    public FloatRange sizeRange = new(0.5f, 1f);
}

public enum PulseMode
{
    Opacity,
    Size,
    OpaSize
}