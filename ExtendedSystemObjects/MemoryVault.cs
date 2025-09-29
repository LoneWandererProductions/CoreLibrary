/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/MemoryVault.cs
 * PURPOSE:     In Memory Storage
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using ExtendedSystemObjects.Helper;

namespace ExtendedSystemObjects;

/// <inheritdoc />
/// <summary>
///     A thread-safe memory vault for managing data with expiration and metadata enrichment.
/// </summary>
/// <typeparam name="TU">Generic type of the data being stored.</typeparam>
public sealed class MemoryVault<TU> : IDisposable
{
    /// <summary>
    ///     The singleton instance.
    /// </summary>
    private static MemoryVault<TU>? _instance;

    /// <summary>
    ///     Lock for singleton instance initialization.
    /// </summary>
    private static readonly Lock InstanceLock = new();

    /// <summary>
    ///     Thread-safe dictionary for storing items.
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
    ///     Interval at which expired items are cleaned up.
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(5);

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
    ///     Memory usage threshold in bytes. Triggers event when exceeded.
    /// </summary>
    public long MemoryThreshold { get; set; } = 10 * 1024 * 1024; // Default 10 MB

    /// <summary>
    ///     Event triggered when memory usage exceeds the threshold.
    /// </summary>
    public event EventHandler<VaultMemoryThresholdExceededEventArgs> MemoryThresholdExceeded;

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
    ///     Adds data to the vault with optional expiration and description.
    /// </summary>
    public long Add(TU data, TimeSpan? expiryTime = null, string description = "")
    {
        EnsureNotDisposed();

        // Generate next available unique ID atomically
        var identifier = Interlocked.Increment(ref _nextId);

        var vaultItem = new VaultItem<TU>(data, expiryTime, description);

        _vault[identifier] = vaultItem;

        // Calculate memory usage after adding
        var currentMemoryUsage = CalculateMemoryUsage();
        if (currentMemoryUsage > MemoryThreshold)
        {
            MemoryThresholdExceeded?.Invoke(this, new VaultMemoryThresholdExceededEventArgs(currentMemoryUsage));
        }

        return identifier;
    }

    /// <summary>
    ///     Retrieves an item by its identifier. Returns default if not found or expired.
    /// </summary>
    public TU? Get(long identifier)
    {
        EnsureNotDisposed();

        if (_vault.TryGetValue(identifier, out var item))
        {
            if (item.HasExpireTime && item.HasExpired)
            {
                _vault.TryRemove(identifier, out _);
                return default;
            }

            return item.Data;
        }

        return default;
    }

    /// <summary>
    ///     Removes an item from the vault.
    /// </summary>
    public bool Remove(long identifier)
    {
        EnsureNotDisposed();
        return _vault.TryRemove(identifier, out _);
    }

    /// <summary>
    ///     Returns all non-expired items in the vault.
    /// </summary>
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
            _vault.TryRemove(key, out _);
        }

        return results;
    }

    /// <summary>
    ///     Retrieves metadata for a specific item.
    /// </summary>
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
    ///     Updates metadata for a specific item.
    /// </summary>
    public void AddMetadata(long identifier, VaultMetadata metaData)
    {
        EnsureNotDisposed();

        ArgumentNullException.ThrowIfNull(metaData);

        if (_vault.TryGetValue(identifier, out var item))
        {
            item.Description = metaData.Description;
            item.HasExpireTime = metaData.HasExpireTime;
            item.AdditionalMetadata = metaData.AdditionalMetadata;
        }
    }

    /// <summary>
    ///     Saves an item to disk in JSON format.
    /// </summary>
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
    ///     Periodic cleanup of expired items.
    /// </summary>
    private void CleanupExpiredItems(object? state)
    {
        foreach (var kvp in _vault)
        {
            if (kvp.Value.HasExpireTime && kvp.Value.HasExpired)
            {
                _vault.TryRemove(kvp.Key, out _);
            }
        }
    }

    /// <summary>
    ///     Calculates approximate memory usage of the vault including metadata.
    /// </summary>
    private long CalculateMemoryUsage()
    {
        long total = 0;

        foreach (var item in _vault.Values)
        {
            total += item.DataSize;

            // Rough estimate of metadata size
            if (!string.IsNullOrEmpty(item.Description))
                total += item.Description.Length * sizeof(char);

            if (item.AdditionalMetadata != null)
                total += item.AdditionalMetadata.Count * 64; // rough estimate per entry
        }

        // Include dictionary overhead (approximation)
        total += _vault.Count * 32;

        return total;
    }

    /// <summary>
    ///     Ensures the vault has not been disposed.
    /// </summary>
    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MemoryVault<TU>));
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        _cleanupTimer.Dispose();
        _vault.Clear();
        _disposed = true;
    }
}
