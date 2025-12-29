/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        CommonExtendedObjectsTests/MemoryVaultTests.cs
 * PURPOSE:     Tests for the MemoryVault
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Threading;
using ExtendedSystemObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonExtendedObjectsTests
{
    /// <summary>
    ///     Some basic tests for Memory Vault
    /// </summary>
    [TestClass]
    public class MemoryVaultTests
    {
        private MemoryVault<string> _vault;

        [TestInitialize]
        public void TestInitialize()
        {
            _vault = MemoryVault<string>.Instance;
        }

        /// <summary>
        ///     Adds the data should return identifier.
        /// </summary>
        [TestMethod]
        public void AddDataShouldReturnIdentifier()
        {
            // Arrange
            const string data = "TestData";

            // Act
            var identifier = _vault.Add(data);

            // Assert
            Assert.AreNotEqual(-1, identifier);
        }

        /// <summary>
        ///     Gets the data should return correct data.
        /// </summary>
        [TestMethod]
        public void GetDataShouldReturnCorrectData()
        {
            // Arrange
            const string data = "TestData";
            var identifier = _vault.Add(data);

            // Act
            var retrievedData = _vault.Get(identifier);

            // Assert
            Assert.AreEqual(data, retrievedData);
        }

        /// <summary>
        ///     Removes the data should remove correct item.
        /// </summary>
        [TestMethod]
        public void RemoveDataShouldRemoveCorrectItem()
        {
            // Arrange
            const string data = "TestData";
            var identifier = _vault.Add(data);

            // Act
            var removed = _vault.Remove(identifier);
            var retrievedData = _vault.Get(identifier);

            // Assert
            Assert.IsTrue(removed);
            Assert.IsNull(retrievedData);
        }

        /// <summary>
        ///     Adds the metadata should store metadata correctly.
        /// </summary>
        [TestMethod]
        public void AddMetadataShouldStoreMetadataCorrectly()
        {
            // Arrange
            const string data = "TestData";
            var identifier = _vault.Add(data);
            var metadata = new VaultMetadata
            {
                Description = "A test item", CreationDate = DateTime.UtcNow, Identifier = identifier
            };

            // Act
            _vault.AddMetadata(identifier, metadata);
            var retrievedMetadata = _vault.GetMetadata(identifier);

            // Assert
            Assert.IsNotNull(retrievedMetadata);
            Assert.AreEqual(metadata.Description, retrievedMetadata.Description);
            Assert.AreEqual(metadata.Identifier, retrievedMetadata.Identifier);
        }

        /// <summary>
        ///     Saves the should persist data.
        /// </summary>
        [TestMethod]
        public void SaveShouldPersistData()
        {
            // Arrange
            const string data = "TestData";
            var identifier = _vault.Add(data);

            // Act
            const string filePath = "vault_test_data.json";
            _vault.SaveToDisk(identifier, filePath);

            // Simulate creating a new vault instance and loading the saved data
            var newVault = MemoryVault<string>.Instance;

            // Load the data from the file
            newVault.LoadFromDisk(filePath);

            // Assert
            var retrievedData = newVault.Get(identifier);
            Assert.AreEqual(data, retrievedData);
        }

        /// <summary>
        ///     Saves the should persist expired data correctly.
        /// </summary>
        [TestMethod]
        public void SaveShouldPersistExpiredDataCorrectly()
        {
            // Arrange
            const string data = "TestData";
            var identifier = _vault.Add(data, TimeSpan.FromMilliseconds(100)); // Expiry after 100ms

            // Act
            const string filePath = "vault_test_expired_data.json";
            _vault.SaveToDisk(identifier, filePath);

            // Simulate creating a new vault instance and loading the saved data
            var newVault = MemoryVault<string>.Instance;

            // Load the data from the file
            identifier = newVault.LoadFromDisk(filePath);

            // Wait for the item to expire
            Thread.Sleep(200);

            // Act
            var retrievedData = newVault.Get(identifier);

            // Assert
            Assert.IsNull(retrievedData); // Data should be null because it's expired
        }
    }
}
