/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/VaultItem.cs
 * PURPOSE:     Holds an Item that can expire
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global

using System;

namespace ExtendedSystemObjects
{
    /// <summary>
    /// Vault item with expiration and data tracking
    /// </summary>
    /// <typeparam name="TU">Generic Type</typeparam>
    internal sealed class VaultItem<TU>
    {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public TU Data { get; set; }

        /// <summary>
        /// Gets the expiry date.
        /// </summary>
        /// <value>
        /// The expiry date.
        /// </value>
        public DateTime ExpiryDate { get; }

        /// <summary>
        /// Gets or sets the expiry time.
        /// </summary>
        /// <value>
        /// The expiry time.
        /// </value>
        public TimeSpan ExpiryTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VaultItem{TU}"/> class.
        /// Needed for Json serialization.
        /// </summary>
        public VaultItem() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VaultItem{U}"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="expiryTime">The expiry time.</param>
        /// <param name="description">A short description of the item, optional.</param>
        public VaultItem(TU data, TimeSpan expiryTime, string description = "")
        {
            Data = data;
            ExpiryTime = expiryTime;
            ExpiryDate = DateTime.UtcNow.Add(expiryTime);
            Description = description;
            CreationDate = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        /// <value>
        /// The creation date.
        /// </value>
        public DateTime CreationDate { get; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// HasExpired checks if the item has passed its expiration timed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has expired; otherwise, <c>false</c>.
        /// </value>
        public bool HasExpired => DateTime.UtcNow > ExpiryDate;
    }
}
