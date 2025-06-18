// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global

using System;
using System.Buffers;
using System.Collections.Generic;

namespace ExtendedSystemObjects
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class SortedKvStore : IDisposable
    {
        private readonly IntArray _keys;
        private readonly IntArray _occupied; // 0/1 flags
        private readonly IntArray _values;

        public SortedKvStore(int initialCapacity = 16)
        {
            _keys = new IntArray(initialCapacity);
            _values = new IntArray(initialCapacity);
            _occupied = new IntArray(initialCapacity);
        }

        public int Count { get; private set; }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>
        /// The keys.
        /// </value>
        public IEnumerable<int> Keys
        {
            get
            {
                for (int i = 0; i < Count; i++)
                {
                    if (_occupied[i] != 0)
                        yield return _keys[i];
                }
            }
        }

        public int this[int key]
        {
            get
            {
                if (TryGet(key, out var value))
                    return value;

                throw new KeyNotFoundException($"Key {key} not found.");
            }
            set
            {
                Add(key, value); // Add already handles update or insert
            }
        }

        public void Add(int key, int value)
        {
            var idx = BinarySearch(key);

            if (idx >= 0)
            {
                if (_occupied[idx] == 0)
                {
                    _occupied[idx] = 1;
                    _values[idx] = value;
                    Count++;
                }
                else
                {
                    _values[idx] = value;
                }

                return;
            }

            idx = ~idx;

            EnsureCapacity();

            _keys.InsertAt(idx, key);
            _values.InsertAt(idx, value);
            _occupied.InsertAt(idx, 1);

            Count++;
        }

        public bool TryGet(int key, out int value)
        {
            int left = 0, right = Count - 1;
            var keysSpan = _keys.AsSpan()[..Count];
            var occupiedSpan = _occupied.AsSpan()[..Count];
            var valuesSpan = _values.AsSpan()[..Count];

            while (left <= right)
            {
                var mid = left + ((right - left) >> 1);
                var midKey = keysSpan[mid];
                if (midKey == key)
                {
                    if (occupiedSpan[mid] != 0)
                    {
                        value = valuesSpan[mid];
                        return true;
                    }

                    break;
                }

                if (midKey < key)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            value = default;
            return false;
        }

        public void Remove(int key)
        {
            var idx = BinarySearch(key);
            if (idx >= 0 && _occupied[idx] != 0)
            {
                _occupied[idx] = 0;
            }
        }

        public bool TryRemove(int key, out int index)
        {
            index = BinarySearch(key);
            if (index >= 0 && _occupied[index] != 0)
            {
                _occupied[index] = 0;
                return true;
            }

            index = -1;
            return false;
        }

        public void RemoveMany(ReadOnlySpan<int> keysToRemove)
        {
            if (keysToRemove.Length == 0)
            {
                return;
            }

            var occSpan = _occupied.AsSpan()[..Count];
            var keysSpan = _keys.AsSpan()[..Count];

            // Optional: if keysToRemove is sorted, do a linear merge
            // This path is faster than HashSet-based if input is sorted and large
            var isSorted = true;
            for (var i = 1; i < keysToRemove.Length; i++)
            {
                if (keysToRemove[i] >= keysToRemove[i - 1])
                {
                    continue;
                }

                isSorted = false;
                break;
            }

            if (isSorted)
            {
                int i = 0, j = 0;
                while (i < Count && j < keysToRemove.Length)
                {
                    var currentKey = keysSpan[i];
                    var removeKey = keysToRemove[j];

                    if (currentKey < removeKey)
                    {
                        i++;
                    }
                    else if (currentKey > removeKey)
                    {
                        j++;
                    }
                    else
                    {
                        occSpan[i] = 0;

                        i++;
                        j++;
                    }
                }
            }
            else
            {
                // Fall back to binary search (cheap because keys are sorted in store)
                foreach (var t in keysToRemove)
                {
                    var idx = BinarySearch(t);
                    if (idx >= 0 && occSpan[idx] != 0)
                    {
                        occSpan[idx] = 0;
                    }
                }
            }
        }

        public void Compact()
        {
            var occSpan = _occupied.AsSpan()[..Count];
            var maxRemoved = Count; // worst case: all are removed

            var rented = ArrayPool<int>.Shared.Rent(maxRemoved);
            var removedCount = 0;

            for (var i = 0; i < Count; i++)
            {
                if (occSpan[i] == 0)
                {
                    rented[removedCount++] = i;
                }
            }

            if (removedCount == 0)
            {
                ArrayPool<int>.Shared.Return(rented);
                return;
            }

            var indicesToRemove = new Span<int>(rented, 0, removedCount);

            _keys.RemoveMultiple(indicesToRemove);
            _values.RemoveMultiple(indicesToRemove);
            _occupied.RemoveMultiple(indicesToRemove);

            Count -= removedCount;

            ArrayPool<int>.Shared.Return(rented);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _keys.Clear();
            _values.Clear();
            _occupied.Clear();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SortedKvStore"/> class.
        /// </summary>
        ~SortedKvStore()
        {
            Dispose();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _keys.Dispose();
            _values.Dispose();
            _occupied.Dispose();
        }

        private void EnsureCapacity()
        {
            // Delegate to IntArray.EnsureCapacity, using Count + 1 since we add one item
            _keys.EnsureCapacity(Count + 1);
            _values.EnsureCapacity(Count + 1);
            _occupied.EnsureCapacity(Count + 1);
        }

        private int BinarySearch(int key)
        {
            return Utility.BinarySearch(_keys.AsSpan(), Count, key);
        }

    }
}
