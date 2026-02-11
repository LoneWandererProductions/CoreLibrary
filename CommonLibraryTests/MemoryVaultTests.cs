/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        MemoryVaultTests.cs
 * PURPOSE:     We try to test my MemoryVault class to ensure that it is properly tracking memory usage and that items are being added and removed correctly.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using ExtendedSystemObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    /// Memory Tests for the MemoryVault class. We want to ensure that memory tracking is accurate and that items are properly added and removed from the vault.
    /// </summary>
    [TestClass]
    public  class MemoryVaultTests
    {
        /// <summary>
        /// Vaults the memory tracking stays in synchronize.
        /// </summary>
        [TestMethod]
        public void Vault_MemoryTracking_StaysInSync()
        {
            var vault = MemoryVault<byte[]>.Instance;
            vault.Clear(); // Ensure a clean state

            byte[] largeData = new byte[1024 * 1024]; // 1MB
            var id = vault.Add(largeData, TimeSpan.FromHours(1), "Test Item");

            long sizeAfterAdd = vault.UsedMemory;
            Assert.IsTrue(sizeAfterAdd >= 1024 * 1024, "Memory should increase.");

            vault.Remove(id);
            Assert.AreEqual(0, vault.UsedMemory, "Memory should return to zero after removal.");
        }

    }
}
