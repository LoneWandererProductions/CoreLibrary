/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        UnmanagedMap.cs
 * PURPOSE:     A high-performance unmanaged key-value store similar to a Dictionary.
 *              Unlike typical dictionaries, entries are marked as deleted (tombstoned)
 *              and only physically removed during explicit compaction, improving
 *              insertion and deletion performance by avoiding frequent reallocations.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */
// ReSharper disable MemberCanBeInternal

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ExtendedSystemObjects.Helper;

namespace ExtendedSystemObjects;

/// <summary>
///     Represents a high-performance, unmanaged hash map with integer keys and unmanaged values.
///     Uses open addressing with linear probing for collision resolution and
///     supports lazy deletion (tombstoning) to improve performance on removals.
/// </summary>
/// <typeparam name="TValue">The type of values stored, must be unmanaged.</typeparam>
/// <seealso cref="System.Collections.Generic.IEnumerable&lt;(System.Int32, TValue)&gt;" />
/// <seealso cref="System.IDisposable" />
[DebuggerDisplay("{ToString()}")]
public sealed unsafe class UnmanagedMap<TValue> : IEnumerable<(int, TValue)>, IDisposable where TValue : unmanaged
{
    /// <summary>
    ///     The minimum exponent for capacity (2^MinPowerOf2 entries minimum).
    /// </summary>
    private const int MinPowerOf2 = 4;

    /// <summary>
    ///     The maximum exponent for capacity (~2^MaxPowerOf2 entries maximum).
    /// </summary>
    private const int MaxPowerOf2 = 20;

    /// <summary>
    ///     Current capacity expressed as a power of two exponent.
    /// </summary>
    private int _capacityPowerOf2;

    /// <summary>
    ///     Pointer to the unmanaged array of entries.
    /// </summary>
    private EntryGeneric<TValue>* _entries;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UnmanagedMap{TValue}" /> class.
    /// </summary>
    /// <param name="capacityPowerOf2">
    ///     Optional initial capacity as a power of two exponent.
    ///     For example, 8 means initial capacity is 256 entries (2^8).
    ///     Values less than 4 are clamped to 4, and max allowed is 20.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown if capacityPowerOf2 is less than MinPowerOf2 or greater than MaxPowerOf2.
    /// </exception>
    public UnmanagedMap(int? capacityPowerOf2 = null)
    {
        var power = capacityPowerOf2 ?? 8;
        if (power < MinPowerOf2)
        {
            power = MinPowerOf2;
        }
        else if (power > MaxPowerOf2)
        {
            power = MaxPowerOf2;
        }

        Capacity = 1 << power;
        _capacityPowerOf2 = power;

        _entries = (EntryGeneric<TValue>*)Marshal.AllocHGlobal(sizeof(EntryGeneric<TValue>) * Capacity);
        Unsafe.InitBlock(_entries, 0, (uint)(sizeof(EntryGeneric<TValue>) * Capacity));
    }

    /// <summary>
    ///     Gets the number of occupied entries in the map.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    ///     Gets the total capacity of the map (number of slots available).
    ///     This is always a power of two.
    /// </summary>
    public int Capacity { get; private set; }

    /// <summary>
    ///     Gets a snapshot of all keys currently stored in the map.
    /// </summary>
    public IEnumerable<int> Keys => GetKeysSnapshot();

    /// <summary>
    ///     Gets or sets the value associated with the specified key.
    ///     Setting a value adds or updates the key-value pair.
    ///     Getting a value throws <see cref="KeyNotFoundException" /> if the key does not exist.
    /// </summary>
    /// <param name="key">The integer key.</param>
    /// <returns>The value associated with the key.</returns>
    /// <exception cref="KeyNotFoundException">If the key is not found when getting.</exception>
    public TValue this[int key]
    {
        get => Get(key);
        set => Set(key, value);
    }

    /// <summary>
    ///     Releases all resources used by the <see cref="UnmanagedMap{TValue}" />.
    /// </summary>
    public void Dispose()
    {
        Free();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Returns an enumerator that iterates over the occupied key-value pairs in the map.
    /// </summary>
    /// <returns>An enumerator of (key, value) tuples.</returns>
    IEnumerator<(int, TValue)> IEnumerable<(int, TValue)>.GetEnumerator()
    {
        return GetEnumerator();
    }

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

    /// <summary>
    ///     Determines whether the map contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <returns><c>true</c> if the map contains an element with the specified key; otherwise, <c>false</c>.</returns>
    public bool ContainsKey(int key)
    {
        return FindIndex(key) >= 0;
    }

    /// <summary>
    ///     Adds or updates the value for the specified key.
    ///     If the load factor exceeds 70%, compaction and/or resizing are triggered automatically.
    /// </summary>
    /// <param name="key">The key to add or update.</param>
    /// <param name="value">The value to associate with the key.</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the map is full after compaction and resizing attempts.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int key, TValue value)
    {
        var compactTried = false;
        var resizeTried = false;

        while (true)
        {
            if (Count >= Capacity * 0.7f)
            {
                if (!compactTried)
                {
                    Compact();
                    compactTried = true;

                    // Retry after compaction only if it helped
                    if (Count < Capacity * 0.7f)
                    {
                        continue;
                    }
                }

                if (!resizeTried)
                {
                    Resize();
                    resizeTried = true;
                    compactTried = false; // allow compact again later
                    continue;
                }

                throw new InvalidOperationException("UnmanagedIntMap full after compact and resize");
            }

            var mask = Capacity - 1;
            var index = key & mask;
            var firstTombstone = -1;

            for (var i = 0; i < Capacity; i++)
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

                if (slot.Used == SharedResources.Tombstone && firstTombstone == -1)
                {
                    firstTombstone = (index + i) & mask;
                }
                else if (slot.Key == key)
                {
                    slot.Value = value;
                    return;
                }
            }

            // Should not be reached, retry
        }
    }

    /// <summary>
    ///     Retrieves the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <returns>The value associated with the specified key.</returns>
    /// <exception cref="KeyNotFoundException">If the key is not found.</exception>
    public TValue Get(int key)
    {
        var mask = Capacity - 1;
        var index = key & mask;

        for (var i = 0; i < Capacity; i++)
        {
            ref var slot = ref _entries[(index + i) & mask];

            if (slot.Used == SharedResources.Empty)
            {
                break;
            }

            if (slot.Used == SharedResources.Occupied && slot.Key == key)
            {
                return slot.Value;
            }
        }

        throw new KeyNotFoundException($"Key {key} not found.");
    }

    /// <summary>
    ///     Attempts to get the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <param name="value">
    ///     When this method returns, contains the value associated with the key, if found; otherwise, the
    ///     default value of <typeparamref name="TValue" />.
    /// </param>
    /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(int key, out TValue value)
    {
        var idx = FindIndex(key);
        if (idx >= 0)
        {
            value = _entries[idx].Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    ///     Attempts to remove the key and its associated value from the map.
    ///     Marks the entry as deleted (tombstone) but does not physically remove it until compaction.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <param name="value">
    ///     When this method returns, contains the value removed if found; otherwise, the default value of
    ///     <typeparamref name="TValue" />.
    /// </param>
    /// <returns><c>true</c> if the key was found and removed; otherwise, <c>false</c>.</returns>
    public bool TryRemove(int key, out TValue value)
    {
        var mask = Capacity - 1;
        var startIndex = key & mask;

        for (var i = 0; i < Capacity; i++)
        {
            var probeIndex = (startIndex + i) & mask;
            ref var slot = ref _entries[probeIndex];

            if (slot.Used == SharedResources.Empty)
            {
                break;
            }

            if (slot.Used == SharedResources.Occupied && slot.Key == key)
            {
                value = slot.Value;
                slot.Used = SharedResources.Tombstone;
                Count--;
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    ///     Attempts to remove the key and its associated value from the map.
    ///     Marks the entry as deleted (tombstone) but does not physically remove it until compaction.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <returns><c>true</c> if the key was found and removed; otherwise, <c>false</c>.</returns>
    public bool TryRemove(int key)
    {
        var mask = Capacity - 1;
        for (var i = 0; i < Capacity; i++)
        {
            var idx = (key + i) & mask;
            ref var slot = ref _entries[idx];

            if (slot.Used == SharedResources.Empty)
            {
                break;
            }

            if (slot.Used == SharedResources.Occupied && slot.Key == key)
            {
                slot.Used = SharedResources.Tombstone;
                Count--;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Gets the enumerator.
    /// </summary>
    /// <returns>Enumerator for the key value pair.</returns>
    public EntryGenericEnumerator<TValue> GetEnumerator()
    {
        return new EntryGenericEnumerator<TValue>(_entries, Capacity);
    }

    /// <summary>
    ///     Resizes the map to the next larger capacity (doubling size),
    ///     rehashing all occupied entries. No operation if max capacity reached.
    /// </summary>
    public void Resize()
    {
        if (_capacityPowerOf2 >= MaxPowerOf2)
        {
            return;
        }

        _capacityPowerOf2++;
        var newMap = new UnmanagedMap<TValue>(_capacityPowerOf2);

        for (var i = 0; i < Capacity; i++)
        {
            ref var entry = ref _entries[i];
            if (entry.Used == SharedResources.Occupied)
            {
                newMap.Set(entry.Key, entry.Value);
            }
        }

        Free();

        _entries = newMap._entries;
        Capacity = newMap.Capacity;
        _capacityPowerOf2 = newMap._capacityPowerOf2;
        Count = newMap.Count;

        // Prevent double free
        newMap._entries = null;
    }

    /// <summary>
    ///     Ensures the map capacity is sufficient for the expected number of entries,
    ///     resizing as needed to maintain a load factor below 70%.
    /// </summary>
    /// <param name="expectedCount">The expected number of entries to accommodate.</param>
    public void EnsureCapacity(int expectedCount)
    {
        while (expectedCount > Capacity * 0.7f && _capacityPowerOf2 < MaxPowerOf2)
        {
            Resize();
        }
    }

    /// <summary>
    ///     Compacts the map by creating a smaller map if the load factor falls below 25%,
    ///     physically removing tombstoned entries and freeing memory.
    /// </summary>
    public void Compact()
    {
        var loadFactor = Count / (float)Capacity;

        if (loadFactor >= 0.25f || Capacity <= 1 << MinPowerOf2)
        {
            return;
        }

        var targetPowerOf2 = Math.Max(MinPowerOf2, _capacityPowerOf2 - 1);
        var newMap = new UnmanagedMap<TValue>(targetPowerOf2);

        for (var i = 0; i < Capacity; i++)
        {
            ref var entry = ref _entries[i];
            if (entry.Used == SharedResources.Occupied)
            {
                newMap.Set(entry.Key, entry.Value);
            }
        }

        Free();
        _entries = newMap._entries;
        Capacity = newMap.Capacity;
        _capacityPowerOf2 = targetPowerOf2;
        Count = newMap.Count;

        newMap._entries = null; // prevent double free
    }

    /// <summary>
    ///     Clears all entries from the map, resetting all slots to empty.
    ///     Does not release allocated memory.
    /// </summary>
    public void Clear()
    {
        if (_entries != null)
        {
            Unsafe.InitBlock(_entries, 0, (uint)(sizeof(EntryGeneric<TValue>) * Capacity));
            Count = 0;
        }
    }

    /// <summary>
    ///     Frees unmanaged memory allocated for the map entries.
    ///     After calling this, the map is unusable until reinitialized.
    /// </summary>
    public void Free()
    {
        if (_entries != null)
        {
            Marshal.FreeHGlobal((IntPtr)_entries);
            _entries = null;
        }

        Capacity = 0;
        Count = 0;
    }

    /// <summary>
    ///     Converts to string.
    /// </summary>
    /// <returns>
    ///     A <see cref="System.String" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"Count = {Count}, Capacity = {Capacity}, LoadFactor = {Count / (float)Capacity:P1}";
    }

    /// <summary>
    ///     Validates internal state for debugging purposes.
    ///     Checks for duplicate keys in the map and asserts failure if any are found.
    ///     This method is only compiled in Debug builds.
    /// </summary>
    [Conditional("DEBUG")]
    public void DebugValidate()
    {
        var seen = new HashSet<int>();
        for (var i = 0; i < Capacity; i++)
        {
            var entry = _entries[i];
            if (entry.Used == SharedResources.Occupied)
            {
                Debug.Assert(seen.Add(entry.Key), $"Duplicate key {entry.Key} found.");
            }
        }
    }

    /// <summary>
    ///     Finalizes the instance and frees unmanaged resources if not already disposed.
    /// </summary>
    ~UnmanagedMap()
    {
        if (_entries != null)
        {
            Free();
        }
    }

    /// <summary>
    ///     Determines whether the map contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <returns><c>true</c> if the map contains an element with the specified key; otherwise, <c>false</c>.</returns>
    private int FindIndex(int key)
    {
        var mask = Capacity - 1;
        var index = key & mask;

        for (var i = 0; i < Capacity; i++)
        {
            var idx = (index + i) & mask;
            ref var slot = ref _entries[idx];

            if (slot.Used == SharedResources.Empty)
            {
                break;
            }

            if (slot.Used == SharedResources.Occupied && slot.Key == key)
            {
                return idx;
            }
        }

        return -1;
    }

    /// <summary>
    ///     Returns a snapshot list of all keys currently occupied in the map.
    /// </summary>
    /// <returns>A list of keys.</returns>
    private List<int> GetKeysSnapshot()
    {
        var keys = new List<int>(Count);
        for (var i = 0; i < Capacity; i++)
        {
            var entry = _entries[i];
            if (entry.Used == SharedResources.Occupied)
            {
                keys.Add(entry.Key);
            }
        }

        return keys;
    }
}
