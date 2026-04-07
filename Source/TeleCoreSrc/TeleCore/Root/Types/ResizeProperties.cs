using Verse;

namespace TeleCore.Types;

public class ResizeProperties
{
    public int initialSizeOffset;
    public int sizeDuration = 60;
    public FloatRange sizeRange = FloatRange.One;
}