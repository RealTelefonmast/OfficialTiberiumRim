namespace TeleCore.Unsorted;

public static class MathT
{
    public static int NextPowerOfTwo(int value)
    {
        return 1 << (int)System.Math.Ceiling(System.Math.Log(value, 2));
    }
}