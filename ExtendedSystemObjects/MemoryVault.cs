/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/MemoryVault.cs
 * PURPOSE:     A Container to store persistent data and access it quick
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ExtendedSystemObjects
{
    /// <inheritdoc />
    /// <summary>
    /// In Memory Vault for data.
    /// </summary>
    /// <typeparam name="T">Generic data type.</typeparam>
    public sealed class MemoryVault<T> : IDisposable
    {
        /// <summary>
        /// The instance
        /// </summary>
        private static readonly Lazy<MemoryVault<T>> MemoryInstance =
            new(() => new MemoryVault<T>());

        /// <summary>
        /// The data store
        /// </summary>
        private readonly ConcurrentDictionary<long, VaultItem<T>> _dataStore = new();

        /// <summary>
        /// The lock
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        /// The cleanup timer
        /// </summary>
        private readonly Timer _cleanupTimer;

        /// <summary>
        /// Gets or sets the expiry time in milliseconds.
        /// </summary>
        /// <value>
        /// The expiry time in milliseconds.
        /// </value>
        public int ExpiryTimeInMilliseconds { get; set; } = 30000; // 30 seconds (adjustable)

        /// <summary>
        /// The current identifier
        /// </summary>
        private int _currentId; // ID counter


        /// <summary>
        /// The recycled ids
        /// </summary>
        private readonly ConcurrentQueue<long> _recycledIds = new();

        /// <summary>
        /// Gets the total memory usage.
        /// </summary>
        /// <value>
        /// The total memory usage.
        /// </value>
        public long TotalMemoryUsage { get; private set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="MemoryVault{T}" /> class from being created.
        /// </summary>
        private MemoryVault()
        {
            // Periodically clean up expired items (if any)
            _cleanupTimer = new Timer(CleanupExpiredItems, null, ExpiryTimeInMilliseconds, ExpiryTimeInMilliseconds);
        }

        /// <summary>
        /// Gets the instance. Singleton.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static MemoryVault<T> Instance => MemoryInstance.Value;

        /// <summary>
        /// Store data with an optional expiration time
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="expiryTime">The expiry time.</param>
        /// <returns>Your return ticket.</returns>
        public long Store(T data, TimeSpan? expiryTime = null)
        {
            // Thread-safe ID generation
            var id = _recycledIds.TryDequeue(out var recycledId)
                ? recycledId
                : Interlocked.Increment(ref _currentId);

            var vaultItem = new VaultItem<T>(data, expiryTime ?? TimeSpan.MaxValue);

            lock (_lock)
            {
                _dataStore[id] = vaultItem;
            }

            return id; // Return the ID to the caller
        }


        /// <summary>
        /// Retrieves the specified key.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Your object that you stored.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">
        /// Key {key} has expired.
        /// or
        /// Key {key} not found.
        /// </exception>
        public T Retrieve(int id)
        {
            lock (_lock)
            {
                if (!_dataStore.TryGetValue(id, out var vaultItem))
                {
                    throw new KeyNotFoundException($"ID {id} not found.");
                }

                if (!vaultItem.HasExpired)
                {
                    return vaultItem.Data;
                }

                Release(id);

                throw new KeyNotFoundException($"ID {id} has expired.");
            }
        }

        /// <summary>
        /// Releases the specified key.
        /// </summary>
        /// <param name="id">The id.</param>
        public void Release(long id)
        {
            lock (_lock)
            {
                _dataStore.TryRemove(id, out _);
            }
        }

        /// <summary>
        /// Clear all data from the vault
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _dataStore.Clear();
                TotalMemoryUsage = 0;
            }
        }

        /// <summary>
        /// Method to get memory usage (for byte arrays)
        /// Gets the memory usage.
        /// </summary>
        /// <returns>Memory used.</returns>
        public int GetMemoryUsage()
        {
            lock (_lock)
            {
                return _dataStore.Sum(item => item.Value is byte[] bytes ? bytes.Length : 0);
            }
        }

        /// <summary>
        /// Gets the memory usage.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Memory Consumption</returns>
        private static long GetMemoryUsage(T data)
        {
            // Handle byte arrays differently since they have a specific size
            return data switch
            {
                byte[] bytes => bytes.Length,
                string str => str.Length * sizeof(char),
                ICollection<object> collection => collection.Count * 1024, // Approximate per object
                _ => 1024 // Default estimate
            };
            // For other objects, we can only approximate memory usage. You could expand this for specific types.
        }

        /// <summary>
        /// Get the total memory usage for all data in the vault
        /// </summary>
        /// <returns>Memory used.</returns>
        public long GetTotalMemoryUsage()
        {
            lock (_lock)
            {
                return TotalMemoryUsage;
            }
        }

        /// <summary>
        /// Cleanup the expired items.
        /// </summary>
        /// <param name="state">The state.</param>
        private void CleanupExpiredItems(object state)
        {
            lock (_lock)
            {
                var expiredKeys = _dataStore
                    .Where(kv => kv.Value.HasExpired)
                    .Select(kv => kv.Key)
                    .ToList();

                foreach (var id in expiredKeys)
                {
                    Release(id);
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Releases all resources used by the <see cref="T:ExtendedSystemObjects.MemoryVault`1" />.
        /// </summary>
        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            _dataStore.Clear();
        }
    }
}
