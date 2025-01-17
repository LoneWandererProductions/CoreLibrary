/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/VaultItem.cs
 * PURPOSE:     Holds an Item that can expire
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;

namespace ExtendedSystemObjects
{
    /// <summary>
    /// Vault item with expiration and data tracking
    /// </summary>
    /// <typeparam name="U">Generic Type</typeparam>
    internal class VaultItem<U>
    {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public U Data { get; set; }

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
        /// Initializes a new instance of the <see cref="VaultItem{U}"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="expiryTime">The expiry time.</param>
        public VaultItem(U data, TimeSpan expiryTime)
        {
            Data = data;
            ExpiryTime = expiryTime;
            ExpiryDate = DateTime.UtcNow.Add(expiryTime);
        }

        /// <summary>
        /// HasExpired checks if the item has passed its expiration timed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has expired; otherwise, <c>false</c>.
        /// </value>
        public bool HasExpired => DateTime.UtcNow > ExpiryDate;
    }
}
