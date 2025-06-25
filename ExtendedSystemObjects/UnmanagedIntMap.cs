using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ExtendedSystemObjects.Helper;

namespace ExtendedSystemObjects
{
    public sealed unsafe class UnmanagedIntMap : IEnumerable<Entry>, IDisposable
    {
        private const int Invalid = -1;
        private int _capacity;
        private Entry* _entries;
        private int _index;

        private int _capacityPowerOf2;

        private const int MinPowerOf2 = 4;  // 16 entries minimum
        private const int MaxPowerOf2 = 20; // ~1 million entries max

        public UnmanagedIntMap(int? capacityPowerOf2 = null)
        {
            var power = capacityPowerOf2 ?? 8;
            if (power < MinPowerOf2)
                power = MinPowerOf2;

            _capacity = 1 << power;
            _capacityPowerOf2 = power;

            var size = sizeof(Entry) * _capacity;
            _entries = (Entry*)Marshal.AllocHGlobal(size);
            Unsafe.InitBlock(_entries, 0, (uint)size);
            Count = 0;
            _index = -1;
        }

        public int Count { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(int key)
        {
            var mask = _capacity - 1;
            var index = key & mask;

            for (var i = 0; i < _capacity; i++)
            {
                ref var slot = ref _entries[(index + i) & mask];

                switch (slot.Used)
                {
                    case SharedResources.Empty:
                        return false;
                    case SharedResources.Occupied when slot.Key == key:
                        return true;
                }
            }

            return false;
        }

        public int this[int key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        public IEnumerable<int> Keys
        {
            get
            {
                foreach (var key in GetKeysSnapshot())
                    yield return key;
            }
        }

        public int Get(int key)
        {
            var mask = _capacity - 1;
            var index = key & mask;

            for (var i = 0; i < _capacity; i++)
            {
                ref var slot = ref _entries[(index + i) & mask];

                if (slot.Used == SharedResources.Empty)
                    break;

                if (slot.Used == SharedResources.Occupied && slot.Key == key)
                    return slot.Value;
            }

            throw new KeyNotFoundException($"Key {key} not found.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int key, int value)
        {
            var compactTried = false;
            var resizeTried = false;

            while (true)
            {
                if (Count >= _capacity * 0.7f)
                {
                    if (!compactTried)
                    {
                        Compact();
                        compactTried = true;

                        // Retry after compaction only if it helped
                        if (Count < _capacity * 0.7f)
                            continue;
                    }

                    if (!resizeTried)
                    {
                        Resize();
                        resizeTried = true;
                        compactTried = false; // allow compact again later
                        continue;
                    }

                    throw new InvalidOperationException("UnmanagedIntMap full after compact and resize");
                }

                var mask = _capacity - 1;
                var index = key & mask;
                var firstTombstone = -1;

                for (var i = 0; i < _capacity; i++)
                {
                    ref var slot = ref _entries[(index + i) & mask];

                    if (slot.Used == SharedResources.Empty)
                    {
                        if (firstTombstone != -1)
                        {
                            ref var tomb = ref _entries[firstTombstone];
                            tomb.Key = key;
                            tomb.Value = value;
                            tomb.Used = SharedResources.Occupied;
                        }
                        else
                        {
                            slot.Key = key;
                            slot.Value = value;
                            slot.Used = SharedResources.Occupied;
                        }

                        Count++;
                        return;
                    }

                    if (slot.Used == SharedResources.Tombstone && firstTombstone == -1)
                        firstTombstone = (index + i) & mask;
                    else if (slot.Key == key)
                    {
                        slot.Value = value;
                        return;
                    }
                }

                // Should not be reached, retry
                continue;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(int key, out int value)
        {
            var mask = _capacity - 1;
            var index = key & mask;

            for (var i = 0; i < _capacity; i++)
            {
                ref var slot = ref _entries[(index + i) & mask];

                if (slot.Used == SharedResources.Empty)
                    break;

                if (slot.Used == SharedResources.Occupied && slot.Key == key)
                {
                    value = slot.Value;
                    return true;
                }
            }

            value = Invalid;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(int key)
        {
            var mask = _capacity - 1;
            var index = key & mask;

            for (var i = 0; i < _capacity; i++)
            {
                ref var slot = ref _entries[(index + i) & mask];

                if (slot.Used == SharedResources.Empty)
                    break;

                if (slot.Used != SharedResources.Occupied || slot.Key != key)
                    continue;

                slot.Used = SharedResources.Tombstone;
                Count--;
                return true;
            }

            return false;
        }

        public bool TryRemove(int key, out int value)
        {
            var mask = _capacity - 1;
            var startIndex = key & mask;

            for (var i = 0; i < _capacity; i++)
            {
                var probeIndex = (startIndex + i) & mask;
                ref var slot = ref _entries[probeIndex];

                if (slot.Used == SharedResources.Empty)
                    break;

                if (slot.Used == SharedResources.Occupied && slot.Key == key)
                {
                    value = slot.Value;
                    slot.Used = SharedResources.Tombstone;
                    Count--;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public void Resize()
        {
            if (_capacityPowerOf2 >= MaxPowerOf2)
                return;

            _capacityPowerOf2++;
            var newMap = new UnmanagedIntMap(_capacityPowerOf2);

            for (var i = 0; i < _capacity; i++)
            {
                ref var entry = ref _entries[i];
                if (entry.Used == SharedResources.Occupied)
                    newMap.Set(entry.Key, entry.Value);
            }

            Free();

            _entries = newMap._entries;
            _capacity = newMap._capacity;
            _capacityPowerOf2 = newMap._capacityPowerOf2;
            Count = newMap.Count;

            // Prevent double free
            newMap._entries = null;
        }

        public void Compact()
        {
            var loadFactor = Count / (float)_capacity;

            if (loadFactor >= 0.25f || _capacity <= (1 << MinPowerOf2))
                return;

            int targetPowerOf2 = Math.Max(MinPowerOf2, _capacityPowerOf2 - 1);
            var newMap = new UnmanagedIntMap(targetPowerOf2);

            for (var i = 0; i < _capacity; i++)
            {
                ref var entry = ref _entries[i];
                if (entry.Used == SharedResources.Occupied)
                    newMap.Set(entry.Key, entry.Value);
            }

            Free();
            _entries = newMap._entries;
            _capacity = newMap._capacity;
            _capacityPowerOf2 = targetPowerOf2;
            Count = newMap.Count;

            newMap._entries = null; // prevent double free
        }

        public void EnsureCapacity(int expectedCount)
        {
            while (expectedCount > _capacity * 0.7f && _capacityPowerOf2 < MaxPowerOf2)
                Resize();
        }

        public void Free()
        {
            if (_entries != null)
            {
                Marshal.FreeHGlobal((IntPtr)_entries);
                _entries = null;
            }

            _capacity = 0;
            Count = 0;
        }

        public void Clear()
        {
            if (_entries != null)
            {
                var size = sizeof(Entry) * _capacity;
                Unsafe.InitBlock(_entries, 0, (uint)size);
            }

            Count = 0;
        }

        public IEnumerator<Entry> GetEnumerator()
        {
            return new EntryEnumerator(_entries, _capacity);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            Free();
            GC.SuppressFinalize(this);
        }

        [Conditional("DEBUG")]
        public void DebugValidate()
        {
            var seen = new HashSet<int>();
            for (int i = 0; i < _capacity; i++)
            {
                var entry = _entries[i];
                if (entry.Used == SharedResources.Occupied)
                {
                    Debug.Assert(seen.Add(entry.Key), $"Duplicate key {entry.Key} found.");
                }
            }
        }

        private List<int> GetKeysSnapshot()
        {
            var keys = new List<int>(Count);
            for (var i = 0; i < _capacity; i++)
            {
                var entry = _entries[i];
                if (entry.Used == SharedResources.Occupied)
                    keys.Add(entry.Key);
            }

            return keys;
        }

        ~UnmanagedIntMap()
        {
            if (_entries != null)
                Free();
        }
    }
}
