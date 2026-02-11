/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/MemoryVault.cs
 * PURPOSE:     In Memory Storage
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable EventNeverSubscribedTo.Global

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using ExtendedSystemObjects.Helper;

namespace ExtendedSystemObjects
{
    /// <inheritdoc />
    /// <summary>
    ///     A thread-safe memory vault for managing data with expiration and metadata enrichment.
    /// </summary>
    /// <typeparam name="TU">Generic type of the data being stored.</typeparam>
    [DebuggerDisplay("Count = {Count}, UsedMemory = {UsedMemory} bytes, Threshold = {MemoryThreshold}")]
    public sealed class MemoryVault<TU> : IDisposable
    {
        /// <summary>
        ///     The singleton instance.
        /// </summary>
        private static MemoryVault<TU>? _instance;

        /// <summary>
        /// Lock for singleton instance initialization.
        /// </summary>
        private static readonly Lock InstanceLock = new();

        /// <summary>
        /// Thread-safe dictionary for storing items.
        /// </summary>
        private readonly ConcurrentDictionary<long, VaultItem<TU>> _vault;

        /// <summary>
        ///     Timer for periodic cleanup of expired items.
        /// </summary>
        private readonly Timer _cleanupTimer;

        /// <summary>
        ///     Atomic counter for generating unique identifiers.
        /// </summary>
        private long _nextId;

        /// <summary>
        ///     Indicates whether the vault has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The total bytes
        /// </summary>
        private long _totalBytes;

        /// <summary>
        /// The cleanup interval
        /// </summary>
        private TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets the approximate total memory used by all items in the vault in bytes.
        /// </summary>
        /// <value>
        /// The used memory.
        /// </value>
        public long UsedMemory => Interlocked.Read(ref _totalBytes);

        /// <summary>
        /// Gets the number of items currently in the vault.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _vault.Count;

        /// <summary>
        /// Interval at which expired items are cleaned up.
        /// Changing this value reschedules the background timer immediately.
        /// </summary>
        /// <value>
        /// The cleanup interval.
        /// </value>
        public TimeSpan CleanupInterval
        {
            get => _cleanupInterval;
            set
            {
                _cleanupInterval = value;
                // Reschedule the timer to use the new interval
                _cleanupTimer?.Change(value, value);
            }
        }

        /// <summary>
        ///     Public static property to access the Singleton instance.
        /// </summary>
        public static MemoryVault<TU> Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    return _instance ??= new MemoryVault<TU>();
                }
            }
        }

        /// <summary>
        /// Memory usage threshold in bytes. Triggers event when exceeded.
        /// </summary>
        /// <value>
        /// The memory threshold.
        /// </value>
        public long MemoryThreshold { get; set; } = 10 * 1024 * 1024; // Default 10 MB

        /// <summary>
        ///     Event triggered when memory usage exceeds the threshold.
        /// </summary>
        public event EventHandler<VaultMemoryThresholdExceededEventArgs>? MemoryThresholdExceeded;

        /// <summary>
        ///     Private constructor for singleton pattern.
        /// </summary>
        private MemoryVault()
        {
            _vault = new ConcurrentDictionary<long, VaultItem<TU>>();
            _nextId = 0;

            // Initialize cleanup timer with configurable interval
            _cleanupTimer = new Timer(CleanupExpiredItems, null, CleanupInterval, CleanupInterval);
        }

        /// <summary>
        /// Adds data to the vault with optional expiration and description.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="expiryTime">The expiry time.</param>
        /// <param name="description">The description.</param>
        /// <returns>Address of memory</returns>
        public long Add(TU data, TimeSpan? expiryTime = null, string description = "")
        {
            EnsureNotDisposed();

            // Generate next available unique ID atomically
            var identifier = Interlocked.Increment(ref _nextId);

            var vaultItem = new VaultItem<TU>(data, expiryTime, description);

            _vault[identifier] = vaultItem;

            // Increment total bytes atomically
            long itemSize = vaultItem.DataSize + (description?.Length * 2 ?? 0);
            Interlocked.Add(ref _totalBytes, itemSize);

            if (Interlocked.Read(ref _totalBytes) > MemoryThreshold)
            {
                MemoryThresholdExceeded?.Invoke(this, new VaultMemoryThresholdExceededEventArgs(_totalBytes));
            }
            return identifier;
        }

        /// <summary>
        ///     Retrieves an item by its identifier. Returns default if not found or expired.
        /// </summary>
        public TU? Get(long identifier)
        {
            EnsureNotDisposed();

            if (!_vault.TryGetValue(identifier, out var item))
            {
                return default;
            }

            if (!item.HasExpireTime || !item.HasExpired)
            {
                return item.Data;
            }

            _vault.TryRemove(identifier, out _);
            return default;
        }

        /// <summary>
        /// Removes an item from the vault.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <returns>Status of operation.</returns>
        public bool Remove(long identifier)
        {
            EnsureNotDisposed();
            if (_vault.TryRemove(identifier, out var item))
            {
                DecrementMemory(item);
                return true;
            }
            return false;
        }

        /// <summary>
        ///  Clears all items from the vault and resets the memory counter.
        /// </summary>
        public void Clear()
        {
            EnsureNotDisposed();
            _vault.Clear();
            Interlocked.Exchange(ref _totalBytes, 0);
        }

        /// <summary>
        /// Returns all non-expired items in the vault.
        /// </summary>
        /// <returns>List with stored data.</returns>
        public List<TU> GetAll()
        {
            EnsureNotDisposed();

            var results = new List<TU>();
            var expiredKeys = new List<long>();

            foreach (var kvp in _vault)
            {
                if (kvp.Value.HasExpireTime && kvp.Value.HasExpired)
                {
                    expiredKeys.Add(kvp.Key);
                }
                else
                {
                    results.Add(kvp.Value.Data);
                }
            }

            foreach (var key in expiredKeys)
            {
                // Use the same pattern to keep _totalBytes accurate
                if (_vault.TryRemove(key, out var item))
                {
                    DecrementMemory(item);
                }
            }

            return results;
        }

        /// <summary>
        /// Retrieves metadata for a specific item.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <returns></returns>
        public VaultMetadata? GetMetadata(long identifier)
        {
            EnsureNotDisposed();

            if (!_vault.TryGetValue(identifier, out var item))
            {
                return default;
            }

            if (item.HasExpireTime && item.HasExpired)
            {
                _vault.TryRemove(identifier, out _);
                return default;
            }

            return new VaultMetadata
            {
                Identifier = identifier,
                Description = item.Description,
                CreationDate = item.CreationDate,
                HasExpireTime = item.HasExpireTime,
                AdditionalMetadata = item.AdditionalMetadata
            };
        }

        /// <summary>
        /// Updates metadata for a specific item.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="metaData">The meta data.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void AddMetadata(long identifier, VaultMetadata metaData)
        {
            EnsureNotDisposed();

            ArgumentNullException.ThrowIfNull(metaData);

            if (!_vault.TryGetValue(identifier, out var item))
            {
                return;
            }

            item.Description = metaData.Description;
            item.HasExpireTime = metaData.HasExpireTime;
            item.AdditionalMetadata = metaData.AdditionalMetadata;
        }

        /// <summary>
        /// Saves an item to disk in JSON format.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns>Success status.</returns>
        public bool SaveToDisk(long identifier, string filePath)
        {
            EnsureNotDisposed();

            try
            {
                if (!_vault.TryGetValue(identifier, out var item))
                    return false;

                var json = JsonSerializer.Serialize(item);
                File.WriteAllText(filePath, json);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Loads an item from disk and stores it in the vault.
        /// </summary>
        public long LoadFromDisk(string filePath)
        {
            EnsureNotDisposed();

            try
            {
                var json = File.ReadAllText(filePath);
                var item = JsonSerializer.Deserialize<VaultItem<TU>>(json);

                if (item != null)
                {
                    // Add item to vault and preserve metadata via init-only constructor
                    var vaultItem = new VaultItem<TU>(item.Data, item.ExpiryTime, item.Description)
                    {
                        AdditionalMetadata = item.AdditionalMetadata
                    };

                    return Add(vaultItem.Data, vaultItem.ExpiryTime, vaultItem.Description);
                }
            }
            catch
            {
                return -1;
            }

            return -1;
        }

        /// <summary>
        /// Periodic cleanup of expired items.
        /// </summary>
        /// <param name="state">The state.</param>
        private void CleanupExpiredItems(object? state)
        {
            foreach (var kvp in _vault)
            {
                if (kvp.Value.HasExpireTime && kvp.Value.HasExpired)
                {
                    if (!_vault.TryRemove(kvp.Key, out var item))
                    {
                        continue;
                    }

                    DecrementMemory(item);
                }
            }
        }
        /// <summary>
        /// Ensures the vault has not been disposed.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">TU</exception>
        private void EnsureNotDisposed()
        {
            if (!_disposed)
            {
                return;
            }

            throw new ObjectDisposedException(nameof(MemoryVault<TU>));
        }

        /// <summary>
        /// Decrements the memory.
        /// </summary>
        /// <param name="item">The item.</param>
        private void DecrementMemory(VaultItem<TU> item)
        {
            long size = item.DataSize + (item.Description?.Length * 2 ?? 0);
            // Add additional metadata estimate if it exists
            if (item.AdditionalMetadata != null) size += item.AdditionalMetadata.Count * 64;

            Interlocked.Add(ref _totalBytes, -size);
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"MemoryVault<{typeof(TU).Name}>: {Count} items, {UsedMemory / 1024.0:F2} KB used (Threshold: {MemoryThreshold / 1024.0:F2} KB)";
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;
            lock (InstanceLock)
            {
                _cleanupTimer.Dispose();
                _vault.Clear();
                _totalBytes = 0;
                _instance = null;
                _disposed = true;
            }
        }
    }
}
