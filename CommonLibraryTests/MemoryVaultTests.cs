/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/MemoryVaultTests.cs
 * PURPOSE:     Tests for the MemoryVault
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Threading;
using ExtendedSystemObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    [TestClass]
    public class MemoryVaultTests
    {
        private MemoryVault<string> _vault;

        [TestInitialize]
        public void TestInitialize()
        {
            _vault = MemoryVault<string>.Instance;
        }

        [TestMethod]
        public void AddData_ShouldReturnIdentifier()
        {
            // Arrange
            var data = "TestData";

            // Act
            var identifier = _vault.Add(data);

            // Assert
            Assert.AreNotEqual(-1, identifier);
        }

        [TestMethod]
        public void GetData_ShouldReturnCorrectData()
        {
            // Arrange
            var data = "TestData";
            var identifier = _vault.Add(data);

            // Act
            var retrievedData = _vault.Get(identifier);

            // Assert
            Assert.AreEqual(data, retrievedData);
        }

        [TestMethod]
        public void RemoveData_ShouldRemoveCorrectItem()
        {
            // Arrange
            var data = "TestData";
            var identifier = _vault.Add(data);

            // Act
            var removed = _vault.Remove(identifier);
            var retrievedData = _vault.Get(identifier);

            // Assert
            Assert.IsTrue(removed);
            Assert.IsNull(retrievedData);
        }

        /// <summary>
        /// Adds the metadata should store metadata correctly.
        /// </summary>
        [TestMethod]
        public void AddMetadata_ShouldStoreMetadataCorrectly()
        {
            // Arrange
            var data = "TestData";
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

        [TestMethod]
        public void Save_ShouldPersistData()
        {
            // Arrange
            var data = "TestData";
            var identifier = _vault.Add(data);

            // Act
            var filePath = "vault_test_data.json";
            _vault.SaveToDisk(identifier,filePath);

            // Simulate creating a new vault instance and loading the saved data
            var newVault = MemoryVault<string>.Instance;

            // Load the data from the file
            newVault.LoadFromDisk(filePath);

            // Assert
            var retrievedData = newVault.Get(identifier);
            Assert.AreEqual(data, retrievedData);
        }

        [TestMethod]
        public void Save_ShouldPersistExpiredDataCorrectly()
        {
            // Arrange
            var data = "TestData";
            var identifier = _vault.Add(data, TimeSpan.FromMilliseconds(100)); // Expiry after 100ms

            // Act
            var filePath = "vault_test_expired_data.json";
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
