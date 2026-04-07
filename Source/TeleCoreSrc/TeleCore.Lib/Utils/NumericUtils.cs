namespace TeleCore.Lib.Utils;

public struct NumericState
{
    private bool _isByte;
    private bool _isInt16;
    private bool _isInt32;
    private bool _isInt64;
}

public static class NumericUtils
{
    public static NumericState GetNumericState<T>(this T val) where T : unmanaged
    {
        NumericState state = new NumericState();
        return state;
    }
}