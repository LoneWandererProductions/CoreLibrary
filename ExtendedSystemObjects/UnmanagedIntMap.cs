using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ExtendedSystemObjects.Helper;

namespace ExtendedSystemObjects
{
    public sealed unsafe class UnmanagedIntMap : IEnumerable<Entry>
    {
        private const int Invalid = -1;
        private int _capacity;
        private Entry* _entries;
        private int _index;

        public UnmanagedIntMap(int capacityPowerOf2 = 8)
        {
            if (capacityPowerOf2 < 1 || capacityPowerOf2 > 30)
            {
                throw new ArgumentOutOfRangeException(nameof(capacityPowerOf2));
            }

            _capacity = 1 << capacityPowerOf2;
            var size = sizeof(Entry) * _capacity;
            _entries = (Entry*)Marshal.AllocHGlobal(size);
            Unsafe.InitBlock(_entries, 0, (uint)size);
            Count = 0;
            _index = -1;
        }

        public int Count { get; private set; }

        public bool ContainsKey(int key)
        {
            var mask = _capacity - 1;
            var index = key & mask;

            for (var i = 0; i < _capacity; i++)
            {
                ref var slot = ref _entries[(index + i) & mask];

                if (slot.Used == SharedResources.Empty)
                {
                    return false;
                }

                if (slot.Used == SharedResources.Occupied && slot.Key == key)
                {
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

        public void Set(int key, int value)
        {
            bool compactTried = false;

            while (true)
            {
                if (Count >= _capacity * 0.7f)
                {
                    if (!compactTried)
                    {
                        Compact();
                        compactTried = true;
                        continue; // Retry after compacting
                    }

                    Resize();
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

                // Should never get here since Resize doubles capacity
                throw new InvalidOperationException("UnmanagedIntMap full");
            }
        }


        public bool TryGetValue(int key, out int value)
        {
            var mask = _capacity - 1;
            var index = key & mask;

            for (var i = 0; i < _capacity; i++)
            {
                ref var slot = ref _entries[(index + i) & mask];

                if (slot.Used == SharedResources.Empty)
                {
                    break;
                }

                if (slot.Used == SharedResources.Occupied && slot.Key == key)
                {
                    value = slot.Value;
                    return true;
                }
            }

            value = Invalid;
            return false;
        }

        public bool TryRemove(int key)
        {
            var mask = _capacity - 1;
            var index = key & mask;

            for (var i = 0; i < _capacity; i++)
            {
                ref var slot = ref _entries[(index + i) & mask];

                if (slot.Used == SharedResources.Empty)
                {
                    break;
                }

                if (slot.Used != SharedResources.Occupied || slot.Key != key)
                {
                    continue;
                }

                slot.Used = SharedResources.Tombstone;
                Count--;
                return true;
            }

            return false;
        }

        public bool TryRemove(int key, out int index)
        {
            var mask = _capacity - 1;
            var startIndex = key & mask;

            for (var i = 0; i < _capacity; i++)
            {
                var probeIndex = (startIndex + i) & mask;
                ref var slot = ref _entries[probeIndex];

                if (slot.Used == SharedResources.Empty)
                {
                    break; // key not found, stop probing
                }

                if (slot.Used == SharedResources.Occupied && slot.Key == key)
                {
                    slot.Used = SharedResources.Tombstone;
                    Count--;
                    index = probeIndex;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        public void Resize()
        {
            var newCapacity = _capacity * 2;
            var newMap = new UnmanagedIntMap((int)Math.Log2(newCapacity));

            for (var i = 0; i < _capacity; i++)
            {
                ref var entry = ref _entries[i];
                if (entry.Used == SharedResources.Occupied)
                {
                    newMap.Set(entry.Key, entry.Value);
                }
            }

            Free(); // old buffer
            _entries = newMap._entries;
            _capacity = newMap._capacity;
            Count = newMap.Count;
        }

        public void Compact()
        {
            var targetCapacity = _capacity;
            var loadFactor = Count / (float)_capacity;

            // Optional: shrink if too sparse
            if (loadFactor < 0.25f && _capacity > 16)
            {
                targetCapacity = Math.Max(16, _capacity / 2);
            }

            var newMap = new UnmanagedIntMap((int)Math.Log2(targetCapacity));

            for (var i = 0; i < _capacity; i++)
            {
                ref var entry = ref _entries[i];
                if (entry.Used == SharedResources.Occupied)
                {
                    newMap.Set(entry.Key, entry.Value);
                }
            }

            Free(); // Dispose old entries
            _entries = newMap._entries;
            _capacity = newMap._capacity;
            Count = newMap.Count;
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

        ~UnmanagedIntMap()
        {
            Free();
        }
    }
}
