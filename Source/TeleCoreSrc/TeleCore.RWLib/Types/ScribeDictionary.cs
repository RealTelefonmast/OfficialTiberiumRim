using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Verse;

// ReSharper disable HeapView.BoxingAllocation

namespace TeleCore.RWLib;

internal class ScribeDictionary<T, D> : IDictionary<T, D>, IExposable
{
    public LookMode keyLookMode = LookMode.Value;
    public Dictionary<T, D> savedDict;
    public LookMode valueLookMode = LookMode.Value;

    public ScribeDictionary()
    {
        savedDict = new Dictionary<T, D>();
    }

    public ScribeDictionary(LookMode keyLookMode, LookMode valueLookMode)
    {
        savedDict = new Dictionary<T, D>();
        this.keyLookMode = keyLookMode;
        this.valueLookMode = valueLookMode;
    }

    public ScribeDictionary(Dictionary<T, D> list, LookMode keyLookMode, LookMode valueLookMode)
    {
        savedDict = list;
        this.keyLookMode = keyLookMode;
        this.valueLookMode = valueLookMode;
    }

    public int Count => savedDict.Count;
    public bool IsReadOnly => false;

    public IEnumerator<KeyValuePair<T, D>> GetEnumerator()
    {
        return savedDict.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(KeyValuePair<T, D> item)
    {
        savedDict.Add(item.Key, item.Value);
    }

    public void Clear()
    {
        savedDict.Clear();
    }

    public bool Contains(KeyValuePair<T, D> item)
    {
        return savedDict.Contains(item);
    }

    public void CopyTo(KeyValuePair<T, D>[] array, int arrayIndex)
    {
        savedDict.ToArray().CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<T, D> item)
    {
        return savedDict.Remove(item.Key);
    }

    public bool ContainsKey(T key)
    {
        return savedDict.ContainsKey(key);
    }

    public void Add(T key, D value)
    {
        savedDict.Add(key, value);
    }

    public bool Remove(T key)
    {
        return savedDict.Remove(key);
    }

    public bool TryGetValue(T key, out D value)
    {
        return savedDict.TryGetValue(key, out value);
    }

    public D this[T key]
    {
        get => savedDict[key];
        set => savedDict[key] = value;
    }

    public ICollection<T> Keys => savedDict.Keys;
    public ICollection<D> Values => savedDict.Values;

    public void ExposeData()
    {
        Scribe_Values.Look(ref keyLookMode, "keyLookMode");
        Scribe_Values.Look(ref valueLookMode, "valueLookMode");
        Scribe_Collections.Look(ref savedDict, "savedDict", keyLookMode, valueLookMode);
    }
}