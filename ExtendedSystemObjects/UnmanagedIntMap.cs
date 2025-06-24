using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ExtendedSystemObjects
{

    public unsafe partial struct UnmanagedIntMap
    {
        private const int INVALID = -1;

        private Entry* _entries;
        private int _capacity;
        private int _count;

        public int Count => _count;

        public void Init(int capacityPowerOf2 = 8)
        {
            _capacity = 1 << capacityPowerOf2; // must be power of 2
            int size = sizeof(Entry) * _capacity;
            _entries = (Entry*)Marshal.AllocHGlobal(size);
            Unsafe.InitBlock(_entries, 0, (uint)size);
            _count = 0;
        }

        public void Set(int key, int value)
        {
            if (_count >= (_capacity * 0.7f))
                Resize();

            int mask = _capacity - 1;
            int index = key & mask;

            for (int i = 0; i < _capacity; i++)
            {
                ref Entry slot = ref _entries[(index + i) & mask];

                if (slot.Used == 0)
                {
                    // Insert new entry
                    slot.Key = key;
                    slot.Value = value;
                    slot.Used = 1;
                    _count++;
                    return;
                }
                else if (slot.Key == key)
                {
                    // Update existing entry
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
                if (slot.Used == 0) break;
                if (slot.Key == key)
                    return true;
            }

            return false;
        }

        public bool TryGet(int key, out int value)
        {
            int mask = _capacity - 1;
            int index = key & mask;

            for (int i = 0; i < _capacity; i++)
            {
                ref Entry slot = ref _entries[(index + i) & mask];
                if (slot.Used == 0) break;
                if (slot.Key == key)
                {
                    value = slot.Value;
                    return true;
                }
            }

            value = INVALID;
            return false;
        }

        public bool TryRemove(int key)
        {
            int mask = _capacity - 1;
            int index = key & mask;

            for (int i = 0; i < _capacity; i++)
            {
                ref Entry slot = ref _entries[(index + i) & mask];
                if (slot.Used == 0) break;
                if (slot.Key == key)
                {
                    slot.Used = 0;
                    slot.Key = INVALID;
                    slot.Value = INVALID;
                    _count--;
                    return true;
                }
            }

            return false;
        }

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
            var newMap = new UnmanagedIntMap();

            newMap.Init((int)Math.Log2(newCapacity)); // power-of-2

            for (int i = 0; i < _capacity; i++)
            {
                ref var entry = ref _entries[i];
                if (entry.Used == 1)
                    newMap.Set(entry.Key, entry.Value);
            }

            Free(); // free old buffer
            _entries = newMap._entries;
            _capacity = newMap._capacity;
            _count = newMap._count;
        }
    }
}
