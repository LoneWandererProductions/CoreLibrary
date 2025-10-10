/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/TransactionLogs.cs
 * PURPOSE:     Basic Transaction Log, log Changes
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 * DESCRIPTION:
 *   Basic Transaction Log for tracking Add, Remove, and Change operations with unique entries.
 *   Designed as a minimal, thread-safe utility for logging changes in collections or objects.
 *   
 *   This class provides a bare-bones implementation intended for adaptation to specific use cases.
 *   It focuses on tracking state changes without assumptions about persistence or complex undo-redo mechanisms.
 *   
 *   Usage Notes:
 *   - Thread-safe via internal locking and concurrent dictionary.
 *   - Uses integer keys managed internally; may need customization for key management.
 *   - Does not enforce validation beyond basic checks; users should adapt as needed.
 *   - Intended primarily for lightweight change tracking and simple transaction logging scenarios.
 *   
 *   Future improvements could include:
 *   - Persistence support
 *   - Undo/Redo operations
 *   - More advanced query/filtering of logs
 *   - Custom key strategies or identifiers
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ExtendedSystemObjects;

/// <summary>
///     Basic Transaction Log with unique entries and generic entries.
///     Supports Add, Change, and Remove logging.
/// </summary>
public sealed class TransactionLogs
{
    /// <summary>
    ///     The lock used to synchronize operations that require thread safety beyond what the ConcurrentDictionary provides.
    /// </summary>
    private readonly Lock _lock = new();

    /// <summary>
    ///     Flag used to track whether the changelog has been modified.
    /// </summary>
    private int _changedFlag;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionLogs" /> class.
    /// </summary>
    public TransactionLogs()
    {
        Changelog = new ConcurrentDictionary<int, LogEntry>();
    }

    /// <summary>
    ///     Gets the changelog containing all tracked operations.
    /// </summary>
    public ConcurrentDictionary<int, LogEntry> Changelog { get; }

    /// <summary>
    ///     Gets a value indicating whether any changes have been logged.
    /// </summary>
    public bool Changed
    {
        get => Interlocked.CompareExchange(ref _changedFlag, 0, 0) == 1;
        private set => Interlocked.Exchange(ref _changedFlag, value ? 1 : 0);
    }

    /// <summary>
    ///     Adds a new log entry for an object.
    /// </summary>
    /// <param name="uniqueIdentifier">The unique identifier of the object.</param>
    /// <param name="item">The object being added.</param>
    /// <param name="startData">Whether this is initial state data (i.e., pre-existing before logging).</param>
    public void Add(int uniqueIdentifier, object item, bool startData)
    {
        lock (_lock)
        {
            var log = new LogEntry
            {
                State = LogState.Add,
                Data = item,
                UniqueIdentifier = uniqueIdentifier,
                StartData = startData
            };

            Changelog[GetNewKey()] = log;
            Changed = true;
        }
    }

    /// <summary>
    ///     Adds a remove log entry for an object if an 'Add' entry exists for it.
    /// </summary>
    /// <param name="uniqueIdentifier">The unique identifier of the object.</param>
    public void Remove(int uniqueIdentifier)
    {
        lock (_lock)
        {
            var id = GetItem(uniqueIdentifier, LogState.Add);
            if (id == -1)
            {
                return;
            }

            var item = Changelog[id].Data;

            Changelog[GetNewKey()] = new LogEntry
            {
                State = LogState.Remove,
                Data = item,
                UniqueIdentifier = uniqueIdentifier
            };

            Changed = true;
        }
    }

    /// <summary>
    ///     Logs a change to an object, if the object has changed.
    ///     Updates an existing change entry if one exists and differs.
    /// </summary>
    /// <param name="uniqueIdentifier">The unique identifier of the object.</param>
    /// <param name="item">The updated object.</param>
    public void Change(int uniqueIdentifier, object item)
    {
        lock (_lock)
        {
            var entry = GetItem(uniqueIdentifier, LogState.Change);

            if (entry != -1 && !Changelog[entry].Data.Equals(item))
            {
                Changelog[entry] = new LogEntry
                {
                    State = LogState.Change,
                    Data = item,
                    UniqueIdentifier = uniqueIdentifier
                };
                Changed = true;
            }
            else if (entry == -1)
            {
                Changelog[GetNewKey()] = new LogEntry
                {
                    State = LogState.Change,
                    Data = item,
                    UniqueIdentifier = uniqueIdentifier
                };
                Changed = true;
            }
        }
    }

    /// <summary>
    ///     Gets the predecessor Add entry key for a given log entry key, if available.
    /// </summary>
    /// <param name="id">The key of the log entry to search from.</param>
    /// <returns>The key of the matching Add entry, or -1 if not found.</returns>
    public int GetPredecessor(int id)
    {
        lock (_lock)
        {
            if (!Changelog.TryGetValue(id, out var reference))
            {
                return -1;
            }

            var unique = reference.UniqueIdentifier;

            foreach (var (key, logEntry) in Changelog.Reverse())
            {
                if (key >= id)
                {
                    continue;
                }

                if (logEntry.UniqueIdentifier == unique && logEntry.State == LogState.Add)
                {
                    return key;
                }
            }

            return -1;
        }
    }

    /// <summary>
    ///     Gets all newly added items (i.e., those not marked as StartData).
    /// </summary>
    /// <returns>A dictionary of new items or null if none exist.</returns>
    public Dictionary<int, LogEntry> GetNewItems()
    {
        lock (_lock)
        {
            if (Changelog.IsEmpty)
            {
                return null;
            }

            return Changelog
                .Where(entry => !entry.Value.StartData)
                .ToDictionary(entry => entry.Key, entry => entry.Value);
        }
    }

    /// <summary>
    ///     Gets the most recent entry matching the specified unique identifier and state.
    /// </summary>
    /// <param name="uniqueIdentifier">The unique identifier of the object.</param>
    /// <param name="state">The state to match (Add, Remove, Change).</param>
    /// <returns>The key of the matching entry or -1 if not found.</returns>
    internal int GetItem(int uniqueIdentifier, LogState state)
    {
        lock (_lock)
        {
            if (Changelog.IsEmpty)
            {
                return -1;
            }

            foreach (var (key, value) in Changelog.Reverse())
            {
                if (value.UniqueIdentifier == uniqueIdentifier && value.State == state)
                {
                    return key;
                }
            }

            return -1;
        }
    }

    /// <summary>
    ///     Gets the next available key in the changelog.
    /// </summary>
    /// <returns>The first unused integer key.</returns>
    public int GetNewKey()
    {
        lock (_lock)
        {
            if (Changelog.IsEmpty)
            {
                return 0;
            }

            var keys = Changelog.Keys.ToList();
            return Utility.GetFirstAvailableIndex(keys);
        }
    }
}
