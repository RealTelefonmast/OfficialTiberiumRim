using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TeleCore.FlowCore;

namespace TeleCore.Generics;

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


public class TwoWayDictionary<TKey, TValue> : IReadOnlyTwoWayDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
{
    private Dictionary<TKey, TValue> _keyToValue;
    private Dictionary<TValue, List<TKey>> _valueToKey;

    public int Count => _keyToValue.Count;

    public IEnumerable<TKey> Keys => _keyToValue.Keys;
    public IEnumerable<TValue> Values => _valueToKey.Keys;

    public KeyValuePair<TKey, TValue> this[int index] => _keyToValue.ElementAt(index);
    public TValue this[TKey key] => _keyToValue[key];
    public List<TKey> this[TValue value] => _valueToKey[value];
    
    public TwoWayDictionary()
    {
        _keyToValue = new();
        _valueToKey = new();
    }

    public bool TryAdd(TKey key, TValue value)
    {
        if (_keyToValue.ContainsKey(key))
        {
            return false;
        }

        _keyToValue.Add(key, value);
        if (!_valueToKey.ContainsKey(value))
        {
            _valueToKey.Add(value, new List<TKey>());
        }
        _valueToKey[value].Add(key);
        return true;
    }
    
    public bool Remove(TKey key, out TValue value)
    {
        if (!_keyToValue.ContainsKey(key))
        {
            value = default!;
            return false;
        }
        
        value = _keyToValue[key];
        _keyToValue.Remove(key);
        _valueToKey.Remove(value);
        return true;
    }

    public bool TryRemove(TKey key, TValue value)
    {
        if (!_keyToValue.ContainsKey(key) || !_valueToKey.ContainsKey(value))
        {
            return false;
        }
        _keyToValue.Remove(key);
        _valueToKey.Remove(value);
        return true;   
    }
    
    public bool TryGetKeyFromValue(TValue value, out List<TKey> key)
    {
        return _valueToKey.TryGetValue(value, out key);
    }
    
    public bool TryGetValueFromKey(TKey key, out TValue value)
    {
        return _keyToValue.TryGetValue(key, out value);
    }

    public int IndexOf(TKey value)
    {
        return  _keyToValue.Keys.ToList().IndexOf(value);
    }

    public int IndexOf(TValue value)
    {
        return  _valueToKey.Keys.ToList().IndexOf(value);
    }

    public bool ContainsKey(TKey key)
    {
        return _keyToValue.ContainsKey(key);
    }
    
    public bool ContainsValue(TValue key)
    {
        return _valueToKey.ContainsKey(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return _keyToValue.TryGetValue(key, out value);
    }
    
    //
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _keyToValue.GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Clear()
    {
        _keyToValue.Clear();
        _valueToKey.Clear();
    }
}