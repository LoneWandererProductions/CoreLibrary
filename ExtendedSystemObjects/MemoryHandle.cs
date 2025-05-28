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

        public IntPtr GetPointer() => _manager.Resolve(this);
    }
}
