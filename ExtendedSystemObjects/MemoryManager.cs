/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        MemoryManager.cs
 * PURPOSE:     Really stupid simple Memory Allocator. More an Experiment than real world value.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ExtendedSystemObjects
{
    public class MemoryManager : IDisposable
    {
        private IntPtr _buffer;
        private int _capacity;
        private List<AllocationEntry> _entries = new();
        private Dictionary<int, AllocationEntry> _handleMap = new();
        private int _nextHandleId = 1;

        public MemoryManager(int size)
        {
            _capacity = size;
            _buffer = Marshal.AllocHGlobal(size);
        }

        public MemoryHandle Allocate(int size)
        {
            if (size <= 0) throw new ArgumentException("Size must be greater than 0.");

            var offset = FindFreeSpot(size);
            if (offset + size > _capacity)
                throw new OutOfMemoryException("Insufficient memory.");

            var handleId = _nextHandleId++;
            var entry = new AllocationEntry
            {
                Offset = offset,
                Size = size,
                HandleId = handleId
            };

            _entries.Add(entry);
            _entries.Sort((a, b) => a.Offset.CompareTo(b.Offset));
            _handleMap[handleId] = entry;

            return new MemoryHandle(handleId, this);
        }

        private int FindFreeSpot(int size)
        {
            var offset = 0;
            foreach (var entry in _entries)
            {
                if (offset + size <= entry.Offset)
                    return offset;

                offset = entry.Offset + entry.Size;
            }
            return offset;
        }

        public IntPtr Resolve(MemoryHandle handle)
        {
            if (!_handleMap.TryGetValue(handle.Id, out var entry))
                throw new InvalidOperationException("Invalid handle.");

            return _buffer + entry.Offset;
        }

        public void Free(MemoryHandle handle)
        {
            if (_handleMap.Remove(handle.Id, out var entry))
                _entries.Remove(entry);
        }

        public unsafe void Compact()
        {
            var newBuffer = Marshal.AllocHGlobal(_capacity);
            var offset = 0;

            var newMap = new Dictionary<int, AllocationEntry>();

            foreach (var entry in _entries)
            {
                Buffer.MemoryCopy(
                    (void*)(_buffer + entry.Offset),
                    (void*)(newBuffer + offset),
                    entry.Size,
                    entry.Size
                );

                entry.Offset = offset;
                offset += entry.Size;

                newMap[entry.HandleId] = entry;
            }

            Marshal.FreeHGlobal(_buffer);
            _buffer = newBuffer;
            _handleMap = newMap;
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(_buffer);
            _entries.Clear();
            _handleMap.Clear();
        }

        private class AllocationEntry
        {
            public int Offset { get; set; }
            public int Size { get; set; }
            public int HandleId { get; set; }
        }
    }
}
