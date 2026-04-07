using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TeleCore.Primitive.Immutable;

/// <summary>
/// Extension methods for immutable types.
/// </summary>
internal static partial class ImmutableExtensions
{
    internal static bool IsValueType<T>()
    {
        if (default(T) != null)
        {
            return true;
        }

        var t = typeof(T);
        return t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// Provides a known wrapper around a sequence of elements that provides the number of elements
    /// and an indexer into its contents.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="sequence">The collection.</param>
    /// <returns>An ordered collection.  May not be thread-safe.  Never null.</returns>
    internal static IOrderedCollection<T> AsOrderedCollection<T>(this IEnumerable<T> sequence)
    {
        Requires.NotNull(sequence, nameof(sequence));

        if (sequence is IOrderedCollection<T> orderedCollection)
        {
            return orderedCollection;
        }

        if (sequence is IList<T> listOfT)
        {
            return new ListOfTWrapper<T>(listOfT);
        }

        return new FallbackWrapper<T>(sequence);
    }

    /// <summary>
    /// Clears the specified stack.  For empty stacks, it avoids the call to <see cref="Stack{T}.Clear"/>, which
    /// avoids a call into the runtime's implementation of <see cref="Array.Clear(Array, int, int)"/>, helping performance,
    /// in particular around inlining.  <see cref="Stack{T}.Count"/> typically gets inlined by today's JIT, while
    /// <see cref="Stack{T}.Clear"/> and <see cref="Array.Clear(Array, int, int)"/> typically don't.
    /// </summary>
    /// <typeparam name="T">Specifies the type of data in the stack to be cleared.</typeparam>
    /// <param name="stack">The stack to clear.</param>
    internal static void ClearFastWhenEmpty<T>(this Stack<T> stack)
    {
        if (stack.Count > 0)
        {
            stack.Clear();
        }
    }

    /// <summary>
    /// Gets a disposable enumerable that can be used as the source for a C# foreach loop
    /// that will not box the enumerator if it is of a particular type.
    /// </summary>
    /// <typeparam name="T">The type of value to be enumerated.</typeparam>
    /// <typeparam name="TEnumerator">The type of the Enumerator struct.</typeparam>
    /// <param name="enumerable">The collection to be enumerated.</param>
    /// <returns>A struct that enumerates the collection.</returns>
    internal static DisposableEnumeratorAdapter<T, TEnumerator> GetEnumerableDisposable<T, TEnumerator>(
        this IEnumerable<T> enumerable)
        where TEnumerator : struct, IStrongEnumerator<T>, IEnumerator<T>
    {
        Requires.NotNull(enumerable, nameof(enumerable));

        if (enumerable is IStrongEnumerable<T, TEnumerator> strongEnumerable)
        {
            return new DisposableEnumeratorAdapter<T, TEnumerator>(strongEnumerable.GetEnumerator());
        }

        // Consider for future: we could add more special cases for common
        // mutable collection types like List<T>+Enumerator and such.
        return new DisposableEnumeratorAdapter<T, TEnumerator>(enumerable.GetEnumerator());
    }

    /// <summary>
    /// Wraps a <see cref="IList{T}"/> as an ordered collection.
    /// </summary>
    /// <typeparam name="T">The type of element in the collection.</typeparam>
    private sealed class ListOfTWrapper<T> : IOrderedCollection<T>
    {
        /// <summary>
        /// The list being exposed.
        /// </summary>
        private readonly IList<T> _collection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListOfTWrapper{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        internal ListOfTWrapper(IList<T> collection)
        {
            Requires.NotNull(collection, nameof(collection));
            _collection = collection;
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count => _collection.Count;

        /// <summary>
        /// Gets the <typeparamref name="T"/> at the specified index.
        /// </summary>
        public T this[int index] => _collection[index];

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    /// <summary>
    /// Wraps any <see cref="IEnumerable{T}"/> as an ordered, indexable list.
    /// </summary>
    /// <typeparam name="T">The type of element in the collection.</typeparam>
    private sealed class FallbackWrapper<T> : IOrderedCollection<T>
    {
        /// <summary>
        /// The original sequence.
        /// </summary>
        private readonly IEnumerable<T> _sequence;

        /// <summary>
        /// The list-ified sequence.
        /// </summary>
        private IList<T>? _collection;

        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackWrapper{T}"/> class.
        /// </summary>
        /// <param name="sequence">The sequence.</param>
        internal FallbackWrapper(IEnumerable<T> sequence)
        {
            Requires.NotNull(sequence, nameof(sequence));
            _sequence = sequence;
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count
        {
            get
            {
                if (_collection == null)
                {
                    int count;
                    if (_sequence.TryGetCount(out count))
                    {
                        return count;
                    }

                    _collection = _sequence.ToArray();
                }

                return _collection.Count;
            }
        }

        /// <summary>
        /// Gets the <typeparamref name="T"/> at the specified index.
        /// </summary>
        public T this[int index]
        {
            get
            {
                _collection ??= _sequence.ToArray();

                return _collection[index];
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _sequence.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}