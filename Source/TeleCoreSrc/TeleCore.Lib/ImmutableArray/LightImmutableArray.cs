using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Verse;

namespace TeleCore.Lib;

//For the enumerator
public partial struct LightImmutableArray<T>
{
    /// <summary>
    ///     An array enumerator.
    /// </summary>
    /// <remarks>
    ///     It is important that this enumerator does NOT implement <see cref="IDisposable" />.
    ///     We want the iterator to inline when we do foreach and to not result in
    ///     a try/finally frame in the client.
    /// </remarks>
    public struct ImmutableArrayEnumerator
    {
        private readonly T[] _array;

        /// <summary>
        ///     The currently enumerated position.
        /// </summary>
        /// <value>
        ///     -1 before the first call to <see cref="MoveNext" />.
        ///     >= this.array.Length after <see cref="MoveNext" /> returns false.
        /// </value>
        private int _index;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImmutableArrayEnumerator" /> struct.
        /// </summary>
        /// <param name="array">The array to enumerate.</param>
        internal ImmutableArrayEnumerator(T[] array)
        {
            _array = array;
            _index = -1;
        }

        /// <summary>
        ///     Gets the currently enumerated value.
        /// </summary>
        public T Current =>
            // PERF: no need to do a range check, we already did in MoveNext.
            // if user did not call MoveNext or ignored its result (incorrect use)
            // they will still get an exception from the array access range check.
            _array[_index];

        /// <summary>
        ///     Advances to the next value to be enumerated.
        /// </summary>
        /// <returns><c>true</c> if another item exists in the array; <c>false</c> otherwise.</returns>
        public bool MoveNext()
        {
            return ++_index < _array.Length;
        }
    }

    /// <summary>
    /// An array enumerator that implements <see cref="IEnumerator{T}"/> pattern (including <see cref="IDisposable"/>).
    /// </summary>
    private sealed class ImmutableArrayEnumeratorObject : IEnumerator<T>
    {
        /// <summary>
        /// A shareable singleton for enumerating empty arrays.
        /// </summary>
        private static readonly IEnumerator<T> s_EmptyEnumerator = new ImmutableArrayEnumeratorObject(Empty.Array!);

        /// <summary>
        /// The array being enumerated.
        /// </summary>
        private readonly T[] _array;

        /// <summary>
        /// The currently enumerated position.
        /// </summary>
        /// <value>
        /// -1 before the first call to <see cref="MoveNext"/>.
        /// this.array.Length - 1 after MoveNext returns false.
        /// </value>
        private int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="Enumerator"/> class.
        /// </summary>
        private ImmutableArrayEnumeratorObject(T[] array)
        {
            _index = -1;
            _array = array;
        }

        /// <summary>
        /// Gets the currently enumerated value.
        /// </summary>
        public T Current
        {
            get
            {
                // this.index >= 0 && this.index < this.array.Length
                // unsigned compare performs the range check above in one compare
                if (unchecked((uint)_index) < (uint)_array.Length)
                {
                    return _array[_index];
                }

                // Before first or after last MoveNext.
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Gets the currently enumerated value.
        /// </summary>
        object? IEnumerator.Current
        {
            get { return this.Current; }
        }

        /// <summary>
        /// If another item exists in the array, advances to the next value to be enumerated.
        /// </summary>
        /// <returns><c>true</c> if another item exists in the array; <c>false</c> otherwise.</returns>
        public bool MoveNext()
        {
            int newIndex = _index + 1;
            int length = _array.Length;

            // unsigned math is used to prevent false positive if index + 1 overflows.
            if ((uint)newIndex <= (uint)length)
            {
                _index = newIndex;
                return (uint)newIndex < (uint)length;
            }

            return false;
        }

        /// <summary>
        /// Resets enumeration to the start of the array.
        /// </summary>
        void IEnumerator.Reset()
        {
            _index = -1;
        }

        /// <summary>
        /// Disposes this enumerator.
        /// </summary>
        /// <remarks>
        /// Currently has no action.
        /// </remarks>
        public void Dispose()
        {
            // we do not have any native or disposable resources.
            // nothing to do here.
        }

        /// <summary>
        /// Creates an enumerator for the specified array.
        /// </summary>
        internal static IEnumerator<T> Create(T[] array)
        {
            if (array.Length != 0)
            {
                return new ImmutableArrayEnumeratorObject(array);
            }
            else
            {
                return s_EmptyEnumerator;
            }
        }
    }
}

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public partial struct LightImmutableArray<T> : IImmutableArray<T>, ICollection<T>, IEnumerable<T>, IStructuralComparable, IStructuralEquatable
{
    //TODO: STRUCT NEEDS TO BE READONLY
    //MANAGE NULL ARR BY GUARD - FU
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private T[] _arr;
    private bool defaultInit = false;

    public T[] Array
    {
        get
        {
            if (_arr == null)
            {
                //TLog.Warning("ImmutableArray had to be default instantiated!!");
                defaultInit = true;
                _arr = new T[] { };
            }
            return _arr;
        }
    }

    public int Length => Array.Length;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsEmpty => Array.Length == 0;

    public bool IsNull => _arr == null;
    public bool IsNullOrEmpty => _arr == null || _arr.Length == 0;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            var self = this;
            return self.IsNullOrEmpty ? "Uninitialized" : $"Length = {self.Length}";
        }
    }

    public T this[int i]
    {
        get
        {
            if (i < 0 || i >= Length)
            {
                throw new ArgumentOutOfRangeException(nameof(i));
            }
            return Array[i];
        }
        set
        {
            Log.Error($"Cannot set ImmutableArray directly, use {nameof(SetAt)} instead!");
        }
    }

    public static readonly LightImmutableArray<T> Empty = new LightImmutableArray<T>(System.Array.Empty<T>());

    public LightImmutableArray(T item)
    {
        _arr = new T[1];
        _arr[0] = item;
    }

    public LightImmutableArray(T[] array)
    {
        _arr = array;
    }

    void ICollection<T>.Add(T item)
    {
        throw new NotSupportedException();
    }

    void ICollection<T>.Clear()
    {
        throw new NotSupportedException();
    }

    public bool Contains(T item)
    {
        throw new NotSupportedException();
    }

    public void CopyTo(T[] array, int index)
    {
        var self = this;
        System.Array.Copy(self.Array!, 0, array, index, self.Length);

    }

    public bool Remove(T item)
    {
        throw new NotSupportedException();
    }

    public int Count => Length;
    public bool IsReadOnly => true;

    public LightImmutableArray<T> Clear()
    {
        return Empty;
    }

    public LightImmutableArray<T> Replace(T oldValue, T newValue)
    {
        return this.Replace(oldValue, newValue, EqualityComparer<T>.Default);
    }


    public LightImmutableArray<T> SetAt(int index, T item)
    {
        Requires.Range(index >= 0 && index < Length, nameof(index));
        var tmp = new T[Length];
        System.Array.Copy(Array!, tmp, Length);
        tmp[index] = item;
        return new LightImmutableArray<T>(tmp);
    }

    public LightImmutableArray<T> Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer)
    {
        var self = this;
        int index = self.IndexOf(oldValue, 0, self.Length, equalityComparer);
        if (index < 0)
        {
            throw new ArgumentException("SR.CannotFindOldValue", nameof(oldValue));
        }

        return self.SetAt(index, newValue);
    }


    public LightImmutableArray<T> Add(T item)
    {
        var self = this;
        if (self.IsEmpty)
        {
            return new LightImmutableArray<T>(item);
        }

        return self.Insert(self.Length, item);
    }

    public LightImmutableArray<T> Insert(int index, T item)
    {
        var self = this;
        Requires.Range(index >= 0 && index <= self.Length, nameof(index));

        if (self.IsEmpty)
        {
            return new LightImmutableArray<T>(item);
        }

        var tmp = new T[self.Length + 1];
        tmp[index] = item;

        if (index != 0)
        {
            System.Array.Copy(self.Array, tmp, index);
        }

        if (index != self.Length)
        {
            System.Array.Copy(self.Array, index, tmp, index + 1, self.Length - index);
        }

        return new LightImmutableArray<T>(tmp);
    }

    public int IndexOf(T item, int startIndex, int count, IEqualityComparer<T>? equalityComparer)
    {
        if (count == 0 && startIndex == 0) return -1;

        Requires.Range(startIndex >= 0 && startIndex < Length, nameof(startIndex));
        Requires.Range(count >= 0 && startIndex + count <= Length, nameof(count));

        equalityComparer ??= EqualityComparer<T>.Default;
        if (Equals(equalityComparer, EqualityComparer<T>.Default)) return System.Array.IndexOf(Array, item, startIndex, count);

        for (var i = startIndex; i < startIndex + count; i++)
        {
            if (equalityComparer.Equals(Array[i], item))
                return i;
        }

        return -1;
    }

    private void AsserIllegalState()
    {
        if (IsNullOrEmpty)
        {
            var stackTrace = new StackTrace();
            Log.Error($"Illegal state! {stackTrace}");
        }
    }

    #region Operators

    #region Equality

    public static bool operator !=(LightImmutableArray<T> arr, Array? obj)
    {
        return arr.Array != obj;
    }

    public static bool operator ==(LightImmutableArray<T> arr, Array? obj)
    {
        return !(arr != obj);
    }

    #endregion

    #endregion

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return ImmutableArrayEnumeratorObject.Create(Array);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ImmutableArrayEnumeratorObject.Create(Array);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableArrayEnumerator GetEnumerator()
    {
        return new ImmutableArrayEnumerator(Array);
    }

    //
    public int CompareTo(object other, IComparer comparer)
    {
        Array? otherArray = other as Array;
        if (otherArray == null)
        {
            if (other is IImmutableArray<T> theirs)
            {
                otherArray = theirs.Array;

                if (Array == null && otherArray == null)
                {
                    return 0;
                }

                if (Array == null ^ otherArray == null)
                {
                    throw new ArgumentException("SR.ArrayInitializedStateNotEqual", nameof(other));
                }
            }
        }

        if (otherArray != null)
        {
            IStructuralComparable? ours = Array;
            if (ours == null)
            {
                throw new ArgumentException("SR.ArrayInitializedStateNotEqual", nameof(other));
            }

            return ours.CompareTo(otherArray, comparer);
        }

        throw new ArgumentException("SR.ArrayLengthsNotEqual", nameof(other));
    }

    bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
    {
        var otherArray = other as Array;
        if (otherArray == null)
        {
            if (other is IImmutableArray<T> theirs)
            {
                otherArray = theirs.Array;

                if (Array == null && otherArray == null)
                    return true;
                if (Array == null) return false;
            }
        }

        IStructuralEquatable ours = Array!;
        return ours.Equals(otherArray, comparer);
    }

    int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
    {
        IStructuralEquatable? ours = Array;
        return ours != null ? ours.GetHashCode(comparer) : GetHashCode();
    }

    public override int GetHashCode()
    {
        // This case should not happen a lot anyway
        // ReSharper disable once HeapView.BoxingAllocation
        return base.GetHashCode();
    }
}