/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        MemoryHandle.cs
 * PURPOSE:     Handle for manual memory Allocation
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;

namespace ExtendedSystemObjects
{
    public readonly struct MemoryHandle
    {
        public int Id { get; }
        private readonly MemoryManager _manager;

        public MemoryHandle(int id, MemoryManager manager)
        {
            Id = id;
            _manager = manager;
        }

        public IntPtr GetPointer()
        {
            return _manager.Resolve(this);
        }
    }
}
