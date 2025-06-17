/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/IUnmanagedArray.cs
 * PURPOSE:     An Abstraction for UnmanagedArray and IntArray to make both exchangeable.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;

namespace ExtendedSystemObjects
{
    /// <summary>
    /// Interface to make unmanaged arrays interchangeable.
    /// </summary>
    /// <typeparam name="T">Generic Type</typeparam>
    /// <seealso cref="System.IDisposable" />
    public interface IUnmanagedArray<T> : IDisposable
    {
        int Length { get; }
        T this[int index] { get; set; }
        void Resize(int newSize);
        void Clear();
    }
}
