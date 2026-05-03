using System;

namespace TeleCore.Unsorted;

public static class MathT
{
    public static int NextPowerOfTwo(int value)
    {
        return 1 << (int)Math.Ceiling(Math.Log(value, 2));
    }
}