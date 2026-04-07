using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace TeleCore;

public class OrderedDictionary<TKey, TValue> : OrderedDictionary
{
    //
    public IEnumerable<TKey> Keys => base.Keys.Cast<TKey>();
    public IEnumerable<TValue> Values => base.Values.Cast<TValue>();

    public new TValue this[int index]
    {
        get => (TValue)base[index];
        set => base[index] = value;
    }

    public TValue this[TKey key]
    {
        get => Contains(key) ? (TValue)base[key] : default;
        set => base[key] = value;
    }

    public void Add(TKey key, TValue value)
    {
        base.Add(key, value);
    }

    public void Remove(TKey key)
    {
        base.Remove(key);
    }
}