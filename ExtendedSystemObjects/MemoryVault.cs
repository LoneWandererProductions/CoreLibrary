/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/MemoryVault.cs
 * PURPOSE:     In Memory Storage
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace ExtendedSystemObjects
{
    /// <inheritdoc />
    /// <summary>
    /// A thread-safe memory vault for managing data with expiration and metadata enrichment.
    /// </summary>
    /// <typeparam name="TU">Generic type of the data being stored.</typeparam>
    public sealed class MemoryVault<TU> : IDisposable
    {
        /// <summary>
        /// The instance
        /// </summary>
        private static MemoryVault<TU> _instance;

        /// <summary>
        /// The instance lock
        /// </summary>
        private static readonly object InstanceLock = new();

        /// <summary>
        /// Public static property to access the Singleton instance
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
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
        /// The vault
        /// </summary>
        private readonly Dictionary<long, VaultItem<TU>> _vault;

        /// <summary>
        /// The metadata
        /// </summary>
        private readonly Dictionary<long, Dictionary<string, object>> _metadata;

        /// <summary>
        /// The lock
        /// </summary>
        private readonly ReaderWriterLockSlim _lock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryVault{TU}"/> class.
        /// </summary>
        public MemoryVault()
        {
            _metadata = new Dictionary<long, Dictionary<string, object>>();
            _vault = new Dictionary<long, VaultItem<TU>>();
        }

        /// <summary>
        /// Adds data to the vault with an optional expiration time.
        /// </summary>
        public long Add(TU data, TimeSpan? expiryTime = null, string description = "", long identifier = -1)
        {
            // If identifier is not provided (default -1), generate a new identifier
            if (identifier == -1)
            {
                identifier = Utility.GetFirstAvailableIndex(_vault.Keys.ToList());
            }

            var expiry = expiryTime ?? TimeSpan.FromHours(24);
            var vaultItem = new VaultItem<TU>(data, expiry, description);

            _lock.EnterWriteLock();
            try
            {
                _vault[identifier] = vaultItem;
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return identifier;
        }

        /// <summary>
        /// Gets the data by its identifier.
        /// </summary>
        public TU Get(long identifier)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_vault.TryGetValue(identifier, out var item) && !item.HasExpired)
                {
                    return item.Data;
                }

                // Remove expired item
                _lock.EnterWriteLock();
                try
                {
                    _vault.Remove(identifier);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }

            return default;
        }

        /// <summary>
        /// Removes an item from the vault by its identifier.
        /// </summary>
        public bool Remove(long identifier)
        {
            _lock.EnterWriteLock();
            try
            {
                return _vault.Remove(identifier);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets all non-expired items in the vault.
        /// </summary>
        public List<TU> GetAll()
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                var nonExpiredItems = _vault.Values
                    .Where(item => !item.HasExpired)
                    .Select(item => item.Data)
                    .ToList();

                // Remove expired items
                var expiredKeys = _vault.Where(kvp => kvp.Value.HasExpired).Select(kvp => kvp.Key).ToList();
                _lock.EnterWriteLock();
                try
                {
                    foreach (var key in expiredKeys)
                    {
                        _vault.Remove(key);
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }

                return nonExpiredItems;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Adds metadata to an item.
        /// </summary>
        public void AddMetadata(long identifier, string key, object value)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                _lock.EnterWriteLock();
                try
                {
                    _metadata[identifier][key] = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Retrieves metadata for an item.
        /// </summary>
        public object GetMetadata(long identifier, string key)
        {
            _lock.EnterReadLock();
            try
            {
                if (!_metadata.ContainsKey(identifier))
                    throw new ArgumentException("No item with this identifier exists.", nameof(identifier));

                return _metadata[identifier].TryGetValue(key, out var value) ? value : null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Save selected items (by identifier) to a JSON file
        /// </summary>
        /// <param name="identifiers">The identifiers.</param>
        /// <param name="path">The path.</param>
        /// <exception cref="System.InvalidOperationException">No valid items found to save.</exception>
        public void SaveSelectedItemsToFile(IEnumerable<long> identifiers, string path)
        {
            // Filter out non-expired items by identifiers
            var selectedItems = _vault
                .Where(kvp => identifiers.Contains(kvp.Key) && !kvp.Value.HasExpired)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (selectedItems.Count == 0)
            {
                throw new InvalidOperationException("No valid items found to save.");
            }

            var selectedMetadata = _metadata
                .Where(kvp => identifiers.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // Prepare the data to be saved
            var dataToSave = new
            {
                Vault = selectedItems,
                Metadata = selectedMetadata
            };

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(dataToSave, jsonOptions);

            // Write to the specified path
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Load items from a file and assign them new identifiers
        /// </summary>
        /// <param name="path">The path.</param>
        /// <exception cref="System.IO.FileNotFoundException">The specified file does not exist.</exception>
        /// <exception cref="System.InvalidOperationException">The file is invalid or does not contain expected data.</exception>
        public void LoadSelectedItemsFromFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("The specified file does not exist.");
            }

            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<dynamic>(json);

            if (data?.Vault is not JsonElement loadedVault || data?.Metadata is not JsonElement loadedMetadata)
            {
                throw new InvalidOperationException("The file is invalid or does not contain expected data.");
            }

            // Deserialize VaultItem<TU> objects
            var vaultItems = (from kvp in loadedVault.EnumerateObject() let identifier = long.Parse(kvp.Name) let item = JsonSerializer.Deserialize<VaultItem<TU>>(kvp.Value.GetRawText()) select (identifier, item)).ToList();

            // Add items to the vault with new identifiers
            foreach (var (_, item) in vaultItems)
            {
                var newIdentifier = Utility.GetFirstAvailableIndex(_vault.Keys.ToList());
                _vault[newIdentifier] = item;
            }

            // Deserialize metadata
            var metadataItems = (from kvp in loadedMetadata.EnumerateObject() let identifier = long.Parse(kvp.Name) let metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(kvp.Value.GetRawText()) select (identifier, metadata)).ToList();

            // Add metadata to the vault with the new identifiers
            foreach (var (identifier, objects) in metadataItems)
            {
                _metadata[identifier] = objects;
            }
        }


        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _lock?.Dispose();
        }
    }
}
