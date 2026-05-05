using Verse;

namespace TeleCore.Types;

public class FadeProperties
{
    public int initialOpacityOffset;
    public int opacityDuration = 60;
    public FloatRange opacityRange = FloatRange.One;
}