using System;
using System.Collections.Generic;

namespace TR;

public struct Identifier<T, T2>
{
    public IntPtr KeyPtr { get; }
    public IntPtr ValuePtr { get; }
}

public static class RW_Data
{
    private static Dictionary<(TeleCore.Map, Type), MapInformation> mapInformation = new();

    static RW_Data()
    {
        mapInformation = new Dictionary<(TeleCore.Map, Type), MapInformation>();
    }
}