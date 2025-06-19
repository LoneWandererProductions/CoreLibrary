/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects.Helper
 * FILE:        ExtendedSystemObjects.Helper/Enumerator.cs
 * PURPOSE:     Since I use an older .etn Version I need to use this helper
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ExtendedSystemObjects.Helper
{
    internal unsafe struct Enumerator<T> : IEnumerator<T> where T : unmanaged
    {
        private readonly T* _data;
        private readonly int _length;
        private int _index;

        public Enumerator(T* data, int length)
        {
            _data = data;
            _length = length;
            _index = -1;
        }

        public T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data[_index];
        }

        object IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _index = -1;

        public void Dispose() { /* no resources to clean */ }
    }
}
