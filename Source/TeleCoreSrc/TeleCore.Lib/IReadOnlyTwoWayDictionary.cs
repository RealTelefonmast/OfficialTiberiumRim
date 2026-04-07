using System.Collections.Generic;

namespace TeleCore.Lib;

public interface IReadOnlyTwoWayDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
{
    public int Count { get; }

    public KeyValuePair<TKey, TValue> this[int index] { get; }
    public TValue this[TKey key] { get; }
    public List<TKey> this[TValue value] { get; }

    public bool ContainsKey(TKey key);
    public bool ContainsValue(TValue key);

    public bool TryGetKeyFromValue(TValue value, out List<TKey> key);
    public bool TryGetValueFromKey(TKey key, out TValue value);

    int IndexOf(TKey value);
    int IndexOf(TValue value);
}
