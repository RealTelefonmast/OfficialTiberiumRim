using System;
using System.Collections;
using System.Collections.Generic;
using Verse;

// ReSharper disable HeapView.BoxingAllocation

namespace TeleCore.Unsorted;

/// <summary>
///     A wrapper to expose generic lists as values. I.e: A <see cref="List{T}" /> of <see cref="ScribeList{T}" />
/// </summary>
/// <typeparam name="T"></typeparam>
public class ScribeList<T> : IList<T>, IExposable
{
    private LookMode _lookMode;
    private List<T> _savedList;

    public ScribeList()
    {
    }

    public ScribeList(LookMode lookMode)
    {
        _savedList = new List<T>();
        _lookMode = lookMode;
    }

    public ScribeList(List<T> list, LookMode lookMode)
    {
        _savedList = list;
        _lookMode = lookMode;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref _lookMode, "lookMode");
        Scribe_Collections.Look(ref _savedList, "savedList", _lookMode);
    }

    public int Count => _savedList.Count;
    public bool IsReadOnly => false;

    public IEnumerator<T> GetEnumerator()
    {
        return _savedList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(T item)
    {
        _savedList.Add(item);
    }

    public void Clear()
    {
        _savedList.Clear();
    }

    public bool Contains(T item)
    {
        return _savedList.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _savedList.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        return _savedList.Remove(item);
    }

    public int IndexOf(T item)
    {
        return _savedList.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        _savedList.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _savedList.RemoveAt(index);
    }

    public T this[int index]
    {
        get => _savedList[index];
        set => _savedList[index] = value;
    }

    public void ForEach(Action<T> action)
    {
        foreach (T variable in _savedList) action.Invoke(variable);
    }

    public void SortBy<TSortBy>(Func<T, TSortBy> selector) where TSortBy : IComparable<TSortBy>
    {
        if (_savedList.Count <= 1) return;
        _savedList.Sort(delegate(T a, T b)
        {
            TSortBy tsortBy = selector(a);
            return tsortBy.CompareTo(selector(b));
        });
    }
}