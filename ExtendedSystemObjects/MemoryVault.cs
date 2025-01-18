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
using System.Linq;
using System.Threading;

namespace ExtendedSystemObjects
{
    /// <inheritdoc />
    /// <summary>
    ///     A thread-safe memory vault for managing data with expiration and metadata enrichment.
    /// </summary>
    /// <typeparam name="TU">Generic type of the data being stored.</typeparam>
    public sealed class MemoryVault<TU> : IDisposable
    {
        /// <summary>
        ///     The instance
        /// </summary>
        private static MemoryVault<TU> _instance;

        /// <summary>
        ///     The instance lock
        /// </summary>
        private static readonly object InstanceLock = new();

        /// <summary>
        ///     The lock
        /// </summary>
        private readonly ReaderWriterLockSlim _lock = new();

        /// <summary>
        ///     The vault
        /// </summary>
        private readonly Dictionary<long, VaultItem<TU>> _vault;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MemoryVault{TU}" /> class.
        /// </summary>
        private MemoryVault()
        {
            _vault = new Dictionary<long, VaultItem<TU>>();
        }

        /// <summary>
        ///     Public static property to access the Singleton instance
        /// </summary>
        /// <value>
        ///     The instance.
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

        /// <inheritdoc />
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _lock?.Dispose();
        }

        /// <summary>
        ///     Adds data to the vault with an optional expiration time.
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
        ///     Gets the data by its identifier.
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
        ///     Removes an item from the vault by its identifier.
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
        ///     Gets all non-expired items in the vault.
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
        ///     Adds metadata to an item.
        /// </summary>
        public void AddMetadata(long identifier, VaultMetadata metaData)
        {
            if (metaData == null)
            {
                throw new ArgumentNullException(nameof(metaData));
            }

            _lock.EnterWriteLock();
            try
            {
                if (_vault.ContainsKey(identifier))
                {
                    _vault[identifier].Description = metaData.Description;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }


        /// <summary>
        ///     Retrieves metadata for an item.
        /// </summary>
        public VaultMetadata GetMetadata(long identifier)
        {
            _lock.EnterReadLock();
            try
            {
                if (_vault.TryGetValue(identifier, out var item) && !item.HasExpired)
                {
                    return new VaultMetadata
                    {
                        Description = item.Description, CreationDate = item.CreationDate, Identifier = identifier
                    };
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return null;
        }
    }
}
