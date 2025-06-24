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
        private Entry* _entries;
        private int _capacity;
        private int _count;
        private int _index;

        public int Count => _count;

        public UnmanagedIntMap(int capacityPowerOf2 = 8)
        {
            if (capacityPowerOf2 < 1 || capacityPowerOf2 > 30)
                throw new ArgumentOutOfRangeException(nameof(capacityPowerOf2));

            _capacity = 1 << capacityPowerOf2;
            int size = sizeof(Entry) * _capacity;
            _entries = (Entry*)Marshal.AllocHGlobal(size);
            Unsafe.InitBlock(_entries, 0, (uint)size);
            _count = 0;
            _index = -1;
        }

        public void Set(int key, int value)
        {
            if (_count >= (_capacity * 0.7f))
                Resize();

            int mask = _capacity - 1;
            int index = key & mask;
            int firstTombstone = -1;

            for (int i = 0; i < _capacity; i++)
            {
                ref Entry slot = ref _entries[(index + i) & mask];

                if (slot.Used == SharedResources.Empty)
                {
                    if (firstTombstone != -1)
                    {
                        ref Entry tomb = ref _entries[firstTombstone];
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

                    _count++;
                    return;
                }
                else if (slot.Used == SharedResources.Tombstone)
                {
                    if (firstTombstone == -1)
                        firstTombstone = (index + i) & mask;
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
            int mask = _capacity - 1;
            int index = key & mask;

            for (int i = 0; i < _capacity; i++)
            {
                ref Entry slot = ref _entries[(index + i) & mask];

                if (slot.Used == SharedResources.Empty)
                    return false;

                if (slot.Used == SharedResources.Occupied && slot.Key == key)
                    return true;
            }

            return false;
        }

        public bool TryGetValue(int key, out int value)
        {
            int mask = _capacity - 1;
            int index = key & mask;

            for (int i = 0; i < _capacity; i++)
            {
                ref Entry slot = ref _entries[(index + i) & mask];

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

        public bool TryRemove(int key)
        {
            int mask = _capacity - 1;
            int index = key & mask;

            for (int i = 0; i < _capacity; i++)
            {
                ref Entry slot = ref _entries[(index + i) & mask];

                if (slot.Used == SharedResources.Empty)
                    break;

                if (slot.Used != SharedResources.Occupied || slot.Key != key)
                {
                    continue;
                }

                slot.Used = SharedResources.Tombstone;
                _count--;
                return true;
            }

            return false;
        }

        public Entry Current
        {
            get
            {
                if (_index < 0 || _index >= _capacity)
                    throw new InvalidOperationException();
                return _entries[_index];
            }
        }

        object IEnumerator.Current => Current;

        public void Compact()
        {
            int targetCapacity = _capacity;
            float loadFactor = _count / (float)_capacity;

            // Optional: shrink if too sparse
            if (loadFactor < 0.25f && _capacity > 16)
            {
                targetCapacity = Math.Max(16, _capacity / 2);
            }

            var newMap = new UnmanagedIntMap((int)Math.Log2(targetCapacity));

            for (int i = 0; i < _capacity; i++)
            {
                ref Entry entry = ref _entries[i];
                if (entry.Used == SharedResources.Occupied)
                {
                    newMap.Set(entry.Key, entry.Value);
                }
            }

            Free(); // Dispose old entries
            _entries = newMap._entries;
            _capacity = newMap._capacity;
            _count = newMap._count;
        }


        public bool MoveNext()
        {
            while (++_index < _capacity)
            {
                if (_entries[_index].Used == SharedResources.Occupied)
                    return true;
            }

            return false;
        }

        public void Reset()
        {
            _index = -1;
        }


        public EntryEnumerator GetEnumerator() => new(_entries, _capacity);

        public void Free()
        {
            if (_entries != null)
            {
                Marshal.FreeHGlobal((IntPtr)_entries);
                _entries = null;
            }

            _capacity = 0;
            _count = 0;
        }

        public void Resize()
        {
            int newCapacity = _capacity * 2;
            var newMap = new UnmanagedIntMap((int)Math.Log2(newCapacity));

            for (int i = 0; i < _capacity; i++)
            {
                ref Entry entry = ref _entries[i];
                if (entry.Used == SharedResources.Occupied)
                    newMap.Set(entry.Key, entry.Value);
            }

            Free(); // old buffer
            _entries = newMap._entries;
            _capacity = newMap._capacity;
            _count = newMap._count;
        }

        public void Clear()
        {
            if (_entries != null)
            {
                int size = sizeof(Entry) * _capacity;
                Unsafe.InitBlock(_entries, 0, (uint)size);
            }

            _count = 0;
        }

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
