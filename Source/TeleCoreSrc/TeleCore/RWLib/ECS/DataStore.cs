using System.Collections.Generic;
using Verse;

namespace TeleCore.RWLib.ECS;

public static class DataStore
{
    private static Dictionary<int, Thing> _fromId;
    
    static DataStore()
    {
    }
}