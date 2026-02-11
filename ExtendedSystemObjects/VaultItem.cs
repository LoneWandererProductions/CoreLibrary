/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:      ExtendedSystemObjects
 * FILE:         ExtendedSystemObjects/VaultItem.cs
 * PURPOSE:      Holds an Item that can expire
 * PROGRAMER:    Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ExtendedSystemObjects
{
    /// <summary>
    ///      Vault item with expiration and data tracking
    /// </summary>
    /// <typeparam name="TU">Generic Type</typeparam>
    [DebuggerDisplay("Size: {DataSize} bytes, Expired: {HasExpired}, Description: {Description}")]
    internal sealed class VaultItem<TU>
    {
        /// <summary>
        ///      Initializes a new instance of the <see cref="VaultItem{TU}" /> class.
        ///      Needed for Json serialization.
        /// </summary>
        public VaultItem() { }

        /// <summary>
        ///      Initializes a new instance of the <see cref="VaultItem{U}" /> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="expiryTime">The expiry time.</param>
        /// <param name="description">A short description of the item, optional.</param>
        public VaultItem(TU data, TimeSpan? expiryTime, string description = "")
        {
            Data = data;
            ExpiryTime = expiryTime;

            // Use UtcNow for all internal calculations to ensure consistency
            CreationDate = DateTime.UtcNow;

            if (expiryTime != null)
            {
                HasExpireTime = true;
                ExpiryDate = CreationDate.Add((TimeSpan)expiryTime);
            }
            else
            {
                HasExpireTime = false;
                // Default to MaxValue if no expiry is set to prevent premature cleanup
                ExpiryDate = DateTime.MaxValue;
            }

            Description = description;
            DataSize = MeasureMemoryUsage(data);
        }

        /// <summary>
        ///      Gets or sets a value indicating whether this instance has expire time.
        /// </summary>
        /// <value>
        ///      <c>true</c> if this instance has expire time; otherwise, <c>false</c>.
        /// </value>
        public bool HasExpireTime { get; set; }

        /// <summary>
        ///      Gets or sets the size of the data.
        /// </summary>
        /// <value>
        ///      The size of the data.
        /// </value>
        public long DataSize { get; init; }

        /// <summary>
        ///      Gets or sets the data.
        /// </summary>
        /// <value>
        ///      The data.
        /// </value>
        public TU Data { get; init; }

        /// <summary>
        ///      Gets the expiry date.
        /// </summary>
        /// <value>
        ///      The expiry date.
        /// </value>
        public DateTime ExpiryDate { get; init; }

        /// <summary>
        ///      Gets or sets the expiry time.
        /// </summary>
        /// <value>
        ///      The expiry time.
        /// </value>
        public TimeSpan? ExpiryTime { get; init; }

        /// <summary>
        ///      Gets or sets the creation date.
        /// </summary>
        /// <value>
        ///      The creation date.
        /// </value>
        public DateTime CreationDate { get; init; }

        /// <summary>
        ///      Gets or sets the description.
        /// </summary>
        /// <value>
        ///      The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        ///      HasExpired checks if the item has passed its expiration timed.
        /// </summary>
        /// <value>
        ///      <c>true</c> if this instance has expired; otherwise, <c>false</c>.
        /// </value>
        public bool HasExpired => HasExpireTime && DateTime.UtcNow > ExpiryDate;

        /// <summary>
        ///      Gets or sets the additional metadata.
        ///      Future Proving for custom user data.
        /// </summary>
        /// <value>
        ///      The additional metadata.
        /// </value>
        public Dictionary<string, object> AdditionalMetadata { get; set; } = new();

        /// <summary>
        ///      Calculates the size of an object using deterministic estimation.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="data">The object to calculate the size of.</param>
        /// <returns>The size in bytes.</returns>
        private static long MeasureMemoryUsage<T>(T data)
        {
            if (data == null) return 0;

            // 1. Handle Strings (Existing)
            if (data is string s) return (s.Length * sizeof(char)) + 24;

            // 2. NEW: Handle Arrays (Very important for byte[] tests!)
            if (data is Array array)
            {
                // Get the length of the array and multiply by the size of the element type
                long elementSize = Marshal.SizeOf(array.GetType().GetElementType() ?? typeof(byte));
                return (array.Length * elementSize) + 24;
            }

            // 3. Handle Value Types (Existing)
            if (typeof(T).IsValueType)
            {
                try { return Marshal.SizeOf(typeof(T)); }
                catch { return IntPtr.Size; }
            }

            // 4. Handle Reference Types (Existing)
            return IntPtr.Size + 16;
        }

        /// <summary>
        /// Returns a string that represents the current item.
        /// </summary>
        public override string ToString()
        {
            string status = HasExpired ? "EXPIRED" : (HasExpireTime ? $"Expires: {ExpiryDate}" : "Persistent");
            return $"VaultItem<{typeof(TU).Name}> | {status} | Size: {DataSize} bytes | Desc: {Description}";
        }
    }
}
