/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:      CommonExtendedObjectsTests
 * FILE:         CommonExtendedObjectsTests/Transactions.cs
 * PURPOSE:      Test the Transaction Log
 * PROGRAMER:    Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;
using ExtendedSystemObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonExtendedObjectsTests
{
    [TestClass]
    public class Transactions
    {
        /// <summary>
        /// Dummy data item for testing complex object logging.
        /// </summary>
        private class DataItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public override bool Equals(object obj) => obj is DataItem item && Name == item.Name;
            public override int GetHashCode() => Name.GetHashCode();
        }

        /// <summary>
        /// Transactions the log complex workflow.
        /// </summary>
        [TestMethod]
        public void TransactionLog_ComplexWorkflow()
        {
            var log = new TransactionLogs();

            // 1. Initial State (StartData) -> Key 1
            var initialData = new DataItem { Id = 100, Name = "start" };
            log.Add(100, initialData, true);

            // 2. New Addition -> Key 2
            var addedData = new DataItem { Id = 101, Name = "added" };
            log.Add(101, addedData, false);

            // 3. First Change for ID 101 -> Key 3
            // (Because no Change entry existed yet, only an Add entry)
            var change1 = new DataItem { Id = 101, Name = "added Change" };
            log.Change(101, change1);

            // 4. Second Change for ID 101 -> Still Key 3
            // (Our logic overwrites the existing Change entry for that ID)
            var change2 = new DataItem { Id = 101, Name = "another Change" };
            log.Change(101, change2);

            // Index 3 is now the "another Change" entry
            var logEntry = log.Changelog[3];
            Assert.AreEqual("another Change", ((DataItem)logEntry.Data).Name);

            // Check GetNewItems (ID 101 Add [Key 2] and ID 101 Change [Key 3])
            var newItems = log.GetNewItems();
            Assert.AreEqual(2, newItems.Count);

            // 5. Remove ID 100 -> Key 4
            log.Remove(100);
            Assert.AreEqual(4, log.Changelog.Count);
        }

        /// <summary>
        /// Adds the item success.
        /// </summary>
        [TestMethod]
        public void AddItemSuccess()
        {
            var transactionLogs = new TransactionLogs();
            transactionLogs.Add(1, "Item1", false);

            Assert.IsTrue(transactionLogs.Changed);
            Assert.AreEqual(1, transactionLogs.Changelog.Count);
            // In auto-increment, the first key is 1
            Assert.AreEqual("Item1", transactionLogs.Changelog[1].Data);
        }

        /// <summary>
        /// Removes the item success.
        /// </summary>
        [TestMethod]
        public void RemoveItemSuccess()
        {
            var transactionLogs = new TransactionLogs();
            transactionLogs.Add(1, "Item1", false);
            transactionLogs.Remove(1);

            Assert.IsTrue(transactionLogs.Changed);
            Assert.AreEqual(2, transactionLogs.Changelog.Count);
            // Last entry should be Remove
            Assert.AreEqual(LogState.Remove, transactionLogs.Changelog[2].State);
        }

        /// <summary>
        /// Changes the item success.
        /// </summary>
        [TestMethod]
        public void ChangeItemSuccess()
        {
            var transactionLogs = new TransactionLogs();
            transactionLogs.Add(1, "Item1", false);
            transactionLogs.Change(1, "Item1_Changed");

            Assert.IsTrue(transactionLogs.Changed);
            // Change overwrites existing Change entry if found,
            // but here we only have an Add, so it creates a new Change entry.
            Assert.AreEqual(2, transactionLogs.Changelog.Count);
            Assert.AreEqual("Item1_Changed", transactionLogs.Changelog[2].Data);
        }

        /// <summary>
        /// Gets the predecessor success.
        /// </summary>
        [TestMethod]
        public void GetPredecessorSuccess()
        {
            var transactionLogs = new TransactionLogs();
            transactionLogs.Add(1, "Item1", false); // Key 1
            transactionLogs.Change(1, "Changed"); // Key 2

            // Looking for the Add predecessor of the Change (Key 2)
            var predecessor = transactionLogs.GetPredecessor(2);

            Assert.AreEqual(1, predecessor);
        }

        /// <summary>
        /// Gets the new items success.
        /// </summary>
        [TestMethod]
        public void GetNewItemsSuccess()
        {
            var transactionLogs = new TransactionLogs();
            transactionLogs.Add(1, "Item1", true); // StartData
            transactionLogs.Add(2, "Item2", false); // New
            transactionLogs.Change(1, "Changed"); // New

            var newItems = transactionLogs.GetNewItems();

            // Item 2 (Add) and Item 1 (Change) are both "New" (StartData is false)
            Assert.AreEqual(2, newItems.Count);
        }

        /// <summary>
        /// Gets the index of the item item exists returns.
        /// </summary>
        [TestMethod]
        public void GetItemItemExistsReturnsIndex()
        {
            var transactionLogs = new TransactionLogs();
            transactionLogs.Add(55, "Data", false); // Key 1

            var key = transactionLogs.GetItem(55, LogState.Add);

            Assert.AreEqual(1, key);
        }

        /// <summary>
        /// Gets the item item does not exist returns minus one.
        /// </summary>
        [TestMethod]
        public void GetItemItemDoesNotExistReturnsMinusOne()
        {
            var transactionLogs = new TransactionLogs();
            var index = transactionLogs.GetItem(999, LogState.Add);
            Assert.AreEqual(-1, index);
        }

        /// <summary>
        /// Gets the new key returns correct key.
        /// </summary>
        [TestMethod]
        public void GetNewKeyReturnsCorrectKey()
        {
            var transactionLogs = new TransactionLogs();
            Assert.AreEqual(1, transactionLogs.GetNewKey());

            transactionLogs.Add(1, "Item1", false); // Consumes Key 1
            Assert.AreEqual(3, transactionLogs.GetNewKey());
        }
    }
}
