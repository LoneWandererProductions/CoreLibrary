// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Buffers;
using System.Collections.Generic;

namespace ExtendedSystemObjects
{
    public sealed class SortedKvStore : IDisposable
    {
        private IntArray _keys;
        private IntArray _values;
        private IntArray _occupied; // 0/1 flags

        public int Count { get; private set; }

        public SortedKvStore(int initialCapacity = 16)
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

        public void RemoveManyByKey(ReadOnlySpan<int> keysToRemove)
        {
            if (keysToRemove.Length == 0)
                return;

            var occSpan = _occupied.AsSpan()[..Count];
            var keysSpan = _keys.AsSpan()[..Count];

            // Optional: if keysToRemove is sorted, do a linear merge
            // This path is faster than HashSet-based if input is sorted and large
            bool isSorted = true;
            for (int i = 1; i < keysToRemove.Length; i++)
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
                    int currentKey = keysSpan[i];
                    int removeKey = keysToRemove[j];

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
                        if (occSpan[i] != 0)
                            occSpan[i] = 0;

                        i++;
                        j++;
                    }
                }
            }
            else
            {
                // Fall back to binary search (cheap because keys are sorted in store)
                for (int n = 0; n < keysToRemove.Length; n++)
                {
                    int idx = BinarySearch(keysToRemove[n]);
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
