using System.Collections.Generic;

namespace TeleCore.Lib.Factories;

public static class TempParams<T>
{
    private static readonly T[] _temp1 = new T[1];
    private static readonly T[] _temp2 = new T[2];
    private static readonly T[] _temp3 = new T[3];
    private static readonly T[] _temp4 = new T[4];

    public static T[] Get(T param1)
    {
        _temp1[0] = param1;
        return _temp1;
    }
    
    public static T[] Get(T param1, T param2)
    {
        _temp2[0] = param1;
        _temp2[1] = param2;
        return _temp2;
    }
    
    public static T[] Get(T param1, T param2, T param3)
    {
        _temp3[0] = param1;
        _temp3[1] = param2;
        _temp3[2] = param3;
        return _temp3;
    }
    
    public static T[] Get(T param1, T param2, T param3, T param4)
    {
        _temp4[0] = param1;
        _temp4[1] = param2;
        _temp4[2] = param3;
        _temp4[3] = param4;
        return _temp4;
    }
}