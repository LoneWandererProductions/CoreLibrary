using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ExtendedSystemObjects.Helper;

namespace ExtendedSystemObjects
{
    public sealed unsafe class UnmanagedIntMap : IEnumerator<Entry>
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

        public Entry Current
        {
            get
            {
                if (_index < 0 || _index >= _capacity)
                {
                    throw new InvalidOperationException();
                }

                return _entries[_index];
            }
        }

        object IEnumerator.Current => Current;


        public bool MoveNext()
        {
            while (++_index < _capacity)
            {
                if (_entries[_index].Used == SharedResources.Occupied)
                {
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose()
        {
            Free();
            GC.SuppressFinalize(this);
        }

        public void Set(int key, int value)
        {
            if (Count >= _capacity * 0.7f)
            {
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

                if (slot.Used == SharedResources.Tombstone)
                {
                    if (firstTombstone == -1)
                    {
                        firstTombstone = (index + i) & mask;
                    }
                }
                else if (slot.Key == key)
                {
                    slot.Value = value;
                    return;
                }
            }

            throw new InvalidOperationException("UnmanagedIntMap full");
        }

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


        public EntryEnumerator GetEnumerator()
        {
            return new(_entries, _capacity);
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

        public void Clear()
        {
            if (_entries != null)
            {
                var size = sizeof(Entry) * _capacity;
                Unsafe.InitBlock(_entries, 0, (uint)size);
            }

            Count = 0;
        }

        ~UnmanagedIntMap()
        {
            Free();
        }
    }
}
