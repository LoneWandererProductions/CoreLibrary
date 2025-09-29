/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/UnmanagedIntList.cs
 * PURPOSE:     Provides a high-performance list implementation for integer values using unmanaged memory.
 *              Designed for scenarios requiring manual memory control.
 *              Not inherently thread-safe.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ExtendedSystemObjects.Helper;
using ExtendedSystemObjects.Interfaces;

namespace ExtendedSystemObjects;

/// <inheritdoc cref="IUnmanagedArray" />
/// <summary>
///     Represents a high-performance list of integers backed by unmanaged memory.
///     Offers fast insertion, removal, and random access with minimal overhead.
///     Useful for scenarios requiring fine-grained memory control, such as high-throughput data pipelines,
///     interop with native code, or avoiding garbage collection in performance-critical paths.
///     This class is not thread-safe. Consumers must implement external synchronization if needed.
/// </summary>
/// <remarks>
///     Manual disposal is required via <see cref="Dispose" /> to avoid memory leaks.
///     The internal buffer is allocated using <see cref="Marshal.AllocHGlobal" /> and must be explicitly freed.
/// </remarks>
/// <seealso cref="T:System.IDisposable" />
[DebuggerDisplay("{ToString()}")]
public sealed unsafe class UnmanagedIntList : IUnmanagedArray<int>, IEnumerable<int>
{
    /// <summary>
    ///     The buffer
    /// </summary>
    private IntPtr _buffer;

    /// <summary>
    ///     The disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    ///     Pointer to the unmanaged buffer holding the integer elements.
    /// </summary>
    private int* _ptr;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UnmanagedIntList" /> class with the specified initial capacity.
    /// </summary>
    /// <param name="initialCapacity">The initial number of elements the list can hold without resizing. Default is 16.</param>
    public UnmanagedIntList(int initialCapacity = 16)
    {
        Capacity = initialCapacity > 0 ? initialCapacity : 16;
        _buffer = UnmanagedMemoryHelper.Allocate<int>(Capacity);
        _ptr = (int*)_buffer;
        Clear();
    }

    /// <summary>
    ///     The capacity
    /// </summary>
    public int Capacity { get; private set; }

    /// <summary>
    ///     Returns a span representing a range of elements from the list.
    /// </summary>
    /// <value>
    ///     The <see cref="Span{System.Int32}" />.
    /// </value>
    /// <param name="range">The range of elements to include in the span.</param>
    /// <returns>
    ///     A <see cref="Span{Int32}" /> over the specified range.
    /// </returns>
    public Span<int> this[Range range] => AsSpan()[range];

    /// <inheritdoc />
    /// <summary>
    ///     Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    ///     An enumerator that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<int> GetEnumerator()
    {
        return new Enumerator<int>(_ptr, Length);
    }

    /// <inheritdoc />
    /// <summary>
    ///     Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    /// <summary>
    ///     Gets the number of elements currently stored in the list <see cref="UnmanagedIntList" />.
    /// </summary>
    public int Length { get; private set; }

    /// <inheritdoc />
    /// <summary>
    ///     Provides direct access to an element at the specified index.
    /// </summary>
    /// <param name="i">Zero-based index of the element.</param>
    /// <returns>The value at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///     Thrown in debug builds if the index is out of bounds.
    /// </exception>
    public int this[int i]
    {
        get
        {
#if DEBUG
            if (i < 0 || i >= Length)
            {
                throw new IndexOutOfRangeException();
            }
#endif
            return _ptr[i];
        }
        set
        {
#if DEBUG
            if (i < 0 || i >= Length)
            {
                throw new IndexOutOfRangeException();
            }
#endif
            _ptr[i] = value;
        }
    }

    /// <inheritdoc />
    /// <summary>
    ///     Removes one or more elements starting at the specified index.
    /// </summary>
    /// <param name="index">The starting index of the element(s) to remove.</param>
    /// <param name="count">The number of elements to remove. Default is 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if index is invalid in debug mode.</exception>
    public void RemoveAt(int index, int count = 1)
    {
#if DEBUG
        if (index < 0 || index + count > Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
#endif

        if (index + count < Length)
        {
            // Shift elements left by 'count' to fill the gap
            UnmanagedMemoryHelper.ShiftLeft(_ptr, index, count, Length);
        }

        Length -= count;
    }


    /// <inheritdoc />
    /// <summary>
    ///     Resizes the specified new size.
    /// </summary>
    /// <param name="newSize">The new size.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">newSize</exception>
    public void Resize(int newSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(newSize);

        EnsureCapacity(newSize);
        Length = newSize;
    }

    /// <inheritdoc />
    /// <summary>
    ///     Removes all elements from the list. The capacity remains unchanged.
    /// </summary>
    public void Clear()
    {
        Length = 0;

        // Clear the entire allocated capacity, not just Length items
        UnmanagedMemoryHelper.Clear<int>(_buffer, Capacity);
    }

    /// <inheritdoc />
    /// <summary>
    ///     Frees unmanaged resources used by the <see cref="T:ExtendedSystemObjects.IntList" />.
    ///     After calling this method, the instance should not be used.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Removes the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>Removes the called Value.</returns>
    public bool Remove(int value)
    {
        for (var i = 0; i < Length; i++)
        {
            if (_ptr[i] == value)
            {
                RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Creates a deep copy of the current <see cref="UnmanagedIntList" /> instance,
    ///     including its unmanaged buffer contents.
    /// </summary>
    /// <returns>A new <see cref="UnmanagedIntList" /> instance with the same values.</returns>
    public UnmanagedIntList Clone()
    {
        var clone = new UnmanagedIntList(Length);
        for (var i = 0; i < Length; i++)
        {
            clone._ptr[i] = _ptr[i];
        }

        clone.Length = Length;
        return clone;
    }

    /// <summary>
    ///     Binaries the search.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>Index of value in the list.</returns>
    public int BinarySearch(int value)
    {
        var span = AsSpan();
        return span.BinarySearch(value);
    }

    /// <summary>
    ///     Sorts this instance.
    /// </summary>
    public void Sort()
    {
        AsSpan()[..Length].Sort(); // Uses Array.Sort internally
    }

    /// <summary>
    ///     Pushes the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    public void Push(int value)
    {
        Add(value);
    }

    /// <summary>
    ///     Adds an integer value to the end of the list, resizing if necessary.
    /// </summary>
    /// <param name="value">The integer value to add.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(int value)
    {
        EnsureCapacity(Length + 1);
        _ptr[Length++] = value;
    }

    /// <summary>
    ///     Removes and returns the last element from the list.
    /// </summary>
    /// <returns>The last integer element in the list.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the list is empty.</exception>
    public int Pop()
    {
        if (Length == 0)
        {
            throw new InvalidOperationException("Stack empty");
        }

        return _ptr[--Length];
    }

    /// <summary>
    ///     Returns the last element without removing it from the list.
    /// </summary>
    /// <returns>The last integer element in the list.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the list is empty.</exception>
    public int Peek()
    {
        if (Length == 0)
        {
            throw new InvalidOperationException("Stack empty");
        }

        return _ptr[Length - 1];
    }

    /// <summary>
    ///     Inserts at.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="value">The value.</param>
    /// <param name="count">The count.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">index</exception>
    public void InsertAt(int index, int value, int count = 1)
    {
#if DEBUG
        if (index < 0 || index > Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
#endif
        if (count <= 0)
        {
            return;
        }

        EnsureCapacity(Length + count);

        // Shift elements to the right
        UnmanagedMemoryHelper.ShiftRight(_ptr, index, count, Length, Capacity);

        // Fill with value
        for (var i = 0; i < count; i++)
        {
            _ptr[index + i] = value;
        }

        Length += count;
    }

    /// <summary>
    ///     Returns a span over the valid elements of the list.
    ///     Allows fast, safe access to the underlying data.
    /// </summary>
    /// <returns>A <see cref="Span{Int32}" /> representing the list's contents.</returns>
    public Span<int> AsSpan()
    {
        return new Span<int>(_ptr, Capacity);
    }

    /// <summary>
    ///     Returns a new UnmanagedIntList that is a sorted copy of the current list.
    /// </summary>
    /// <returns>A new UnmanagedIntList instance with sorted values.</returns>
    public UnmanagedIntList Sorted()
    {
        var copy = new UnmanagedIntList(Length);
        for (var i = 0; i < Length; i++)
        {
            copy.Add(_ptr[i]);
        }

        copy.Sort(); // Uses internal AsSpan().Sort()
        return copy;
    }

    /// <summary>
    ///     Reduces the internal capacity to match the current number of elements,
    ///     releasing any unused memory.
    /// </summary>
    public void TrimExcess()
    {
        if (Length == Capacity)
        {
            return;
        }

        _buffer = UnmanagedMemoryHelper.Reallocate<int>(_buffer, Length);
        _ptr = (int*)_buffer;
        Capacity = Length;
    }

    /// <summary>
    ///     Copies the list contents into a new managed array.
    /// </summary>
    /// <returns>A managed <see cref="int[]" /> containing the current elements.</returns>
    public int[] ToArray()
    {
        var result = new int[Length];
        CopyTo(result);
        return result;
    }


    /// <summary>
    ///     Copies to Array.
    /// </summary>
    /// <param name="array">The array.</param>
    /// <param name="arrayIndex">Index of the array.</param>
    /// <exception cref="System.ArgumentNullException">array</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">arrayIndex</exception>
    public void CopyTo(int[] array, int arrayIndex = 0)
    {
#if DEBUG
        ArgumentNullException.ThrowIfNull(array);

        if (arrayIndex < 0 || arrayIndex + Length > array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        }
#endif
        for (var i = 0; i < Length; i++)
        {
            array[arrayIndex + i] = _ptr[i];
        }
    }


    /// <summary>
    ///     Converts to string.
    /// </summary>
    /// <returns>
    ///     A <see cref="System.String" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        var sb = new StringBuilder();

        for (var i = 0; i < Length; i++)
        {
            sb.Append(_ptr[i]);
            if (i < Length - 1)
            {
                sb.Append(", ");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Copies to.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <exception cref="System.ArgumentException">Target span too small</exception>
    public void CopyTo(Span<int> target)
    {
#if DEBUG
        if (target.Length < Length)
        {
            throw new ArgumentException("Target span too small");
        }
#endif
        AsSpan().Slice(0, Length).CopyTo(target);
    }

    /// <summary>
    ///     Releases the unmanaged resources used by the list.
    ///     After disposal, the instance should not be used.
    /// </summary>
    ~UnmanagedIntList()
    {
        Dispose(false);
    }

    /// <summary>
    ///     Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">
    ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
    ///     unmanaged resources.
    /// </param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        // Free unmanaged resources
        if (_buffer != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(_buffer);
            _buffer = IntPtr.Zero;
            _ptr = null;
            Capacity = 0;
            Length = 0;
        }

        // If you had managed disposable members and disposing is true,
        // dispose them here. None exist for now.

        _disposed = true; // Always set to true after dispose

        // Suppress unused parameter warning
        _ = disposing;
    }

    /// <summary>
    ///     Ensures the capacity of the internal buffer is at least the specified minimum size.
    ///     Resizes the buffer if necessary by doubling its capacity or setting it to the minimum required size.
    /// </summary>
    /// <param name="min">The minimum capacity required.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacity(int min)
    {
        if (min <= Capacity)
        {
            return;
        }

        var newCapacity = Capacity == 0 ? 4 : Capacity * 2;
        if (newCapacity < min)
        {
            newCapacity = min;
        }

        _buffer = UnmanagedMemoryHelper.Reallocate<int>(_buffer, newCapacity);
        _ptr = (int*)_buffer;
        Capacity = newCapacity;
    }
}
