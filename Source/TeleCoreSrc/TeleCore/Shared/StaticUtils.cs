using System;
using System.Collections.Generic;

namespace TeleCore.Shared;

public static class EmptyList<T>
{
    private static readonly Dictionary<Type, object> Empty = new();
    
    
    public static IList<T> Get()
    {
        var type = typeof(T);
        if(Empty.TryGetValue(type, out var listT))
        {
            return (listT as IList<T>)!;
        }
        return (IList<T>)(Empty[type] = new List<T>());
    }
}