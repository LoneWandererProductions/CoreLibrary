using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ExtendedSystemObjects
{
    public sealed class UnmanagedKvStore : IDisposable
    {
        private IntArray _keys;
        private IntArray _values;
        private IntArray _occupied; // 0/1 flags

        public int Count { get; private set; }

        public UnmanagedKvStore(int initialCapacity = 16)
        {
            _keys = new IntArray(initialCapacity);
            _values = new IntArray(initialCapacity);
            _occupied = new IntArray(initialCapacity);
        }

        public void Add(int key, int value)
        {
            int idx = BinarySearch(key);

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

        public bool TryGetByKey(int key, out int value)
        {
            int left = 0, right = Count - 1;
            var keysSpan = _keys.AsSpan()[..Count];
            var occupiedSpan = _occupied.AsSpan()[..Count];
            var valuesSpan = _values.AsSpan()[..Count];

            while (left <= right)
            {
                int mid = left + ((right - left) >> 1);
                int midKey = keysSpan[mid];
                if (midKey == key)
                {
                    if (occupiedSpan[mid] != 0)
                    {
                        value = valuesSpan[mid];
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                else if (midKey < key)
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

        public void RemoveByKey(int key)
        {
            int idx = BinarySearch(key);
            if (idx >= 0 && _occupied[idx] != 0)
            {
                _occupied[idx] = 0;
            }
        }

        public void RemoveManyByKey(IEnumerable<int> keysToRemove)
        {
            var removalSet = new HashSet<int>(keysToRemove);

            var occSpan = _occupied.AsSpan()[..Count];
            var keysSpan = _keys.AsSpan()[..Count];

            for (int i = 0; i < Count; i++)
            {
                if (occSpan[i] != 0 && removalSet.Contains(keysSpan[i]))
                {
                    occSpan[i] = 0;
                }
            }
        }

        public void Compact()
        {
            var occSpan = _occupied.AsSpan()[..Count];
            int maxRemoved = Count; // worst case: all are removed

            int[] rented = ArrayPool<int>.Shared.Rent(maxRemoved);
            int removedCount = 0;

            for (int i = 0; i < Count; i++)
            {
                if (occSpan[i] == 0)
                    rented[removedCount++] = i;
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

        private void EnsureCapacity()
        {
            // Delegate to IntArray.EnsureCapacity, using Count + 1 since we add one item
            _keys.EnsureCapacity(Count + 1);
            _values.EnsureCapacity(Count + 1);
            _occupied.EnsureCapacity(Count + 1);
        }

        private int BinarySearch(int key)
        {
            int left = 0, right = Count - 1;
            var keysSpan = _keys.AsSpan()[..Count];

            while (left <= right)
            {
                int mid = left + ((right - left) >> 1);
                int midKey = keysSpan[mid];

                if (midKey == key)
                    return mid;

                if (midKey < key)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            return ~left;
        }

        public void Dispose()
        {
            _keys.Dispose();
            _values.Dispose();
            _occupied.Dispose();
        }
    }
}
