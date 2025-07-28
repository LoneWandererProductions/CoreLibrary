/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects.Interfaces
 * FILE:        ExtendedSystemObjects.Interfaces/IUnmanagedArray.cs
 * PURPOSE:     An Abstraction for UnmanagedArray and IntArray to make both exchangeable.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMemberInSuper.Global

using System;

namespace ExtendedSystemObjects.Interfaces;

/// <inheritdoc />
/// <summary>
///     Interface to make unmanaged arrays interchangeable.
/// </summary>
/// <typeparam name="T">Generic Type</typeparam>
/// <seealso cref="T:System.IDisposable" />
public interface IUnmanagedArray<T> : IDisposable
{
    /// <summary>
    ///     Gets the length.
    /// </summary>
    /// <value>
    ///     The length.
    /// </value>
    int Length { get; }

    /// <summary>
    ///     Gets or sets the <see cref="T" /> at the specified index.
    /// </summary>
    /// <value>
    ///     The <see cref="T" />.
    /// </value>
    /// <param name="index">The index.</param>
    /// <returns>Value at index</returns>
    T this[int index] { get; set; }

    /// <summary>
    ///     Removes at.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="count">The count we want to remove. Optional.</param>
    void RemoveAt(int index, int count = 1);

    /// <summary>
    ///     Resizes the specified new size.
    /// </summary>
    /// <param name="newSize">The new size.</param>
    void Resize(int newSize);

    /// <summary>
    ///     Clears this instance.
    /// </summary>
    void Clear();
}
