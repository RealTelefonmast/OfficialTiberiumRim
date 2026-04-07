using Verse;

namespace TeleCore.Types;

public class FadeProperties
{
    public int initialOpacityOffset;
    public int duration = 60;
    public FloatRange opacityRange = FloatRange.One;
}