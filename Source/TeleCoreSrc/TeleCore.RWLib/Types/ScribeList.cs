using System;
using System.Collections;
using System.Collections.Generic;
using Verse;
// ReSharper disable HeapView.BoxingAllocation

namespace TeleCore.RWLib;

/// <summary>
///     A wrapper to expose generic lists as values. I.e: A <see cref="List{T}" /> of <see cref="ScribeList{T}" />
/// </summary>
/// <typeparam name="T"></typeparam>
public class ScribeList<T> : IList<T>, IExposable
{
    public LookMode lookMode;
    public List<T> savedList;

    public ScribeList()
    {
    }

    public ScribeList(LookMode lookMode)
    {
        savedList = new List<T>();
        this.lookMode = lookMode;
    }

    public ScribeList(List<T> list, LookMode lookMode)
    {
        savedList = list;
        this.lookMode = lookMode;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref lookMode, "lookMode");
        Scribe_Collections.Look(ref savedList, "savedList", lookMode);
    }

    public int Count => savedList.Count;
    public bool IsReadOnly => false;

    public IEnumerator<T> GetEnumerator()
    {
        return savedList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(T item)
    {
        savedList.Add(item);
    }

    public void Clear()
    {
        savedList.Clear();
    }

    public bool Contains(T item)
    {
        return savedList.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        savedList.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        return savedList.Remove(item);
    }

    public int IndexOf(T item)
    {
        return savedList.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        savedList.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        savedList.RemoveAt(index);
    }

    public T this[int index]
    {
        get => savedList[index];
        set => savedList[index] = value;
    }

    public void ForEach(Action<T> action)
    {
        foreach (var variable in savedList) action.Invoke(variable);
    }

    public void SortBy<TSortBy>(Func<T, TSortBy> selector) where TSortBy : IComparable<TSortBy>
    {
        if (savedList.Count <= 1) return;
        savedList.Sort(delegate (T a, T b)
        {
            var tsortBy = selector(a);
            return tsortBy.CompareTo(selector(b));
        });
    }
}