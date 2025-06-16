/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        CommonExtendedObjectsTests/Transactions.cs
 * PURPOSE:     Test the Transaction Log
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Linq;
using ExtendedSystemObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonExtendedObjectsTests
{
    /// <summary>
    ///     InterOps testing
    /// </summary>
    [TestClass]
    public class Transactions
    {
        /// <summary>
        ///     Test Transactions.
        /// </summary>
        [TestMethod]
        public void TransactionLog()
        {
            //TODO improve

            var log = new TransactionLogs();
            //Generate Test data for stuff to copy

            var key = log.GetNewKey();

            var data = new DataItem { Id = key, Name = "start" };

            //start of the event, nothing was done yet

            log.Add(0, data, true);

            key = log.GetNewKey();

            data = new DataItem { Id = key, Name = "added" };
            log.Add(data.Id, data, false);

            data = new DataItem { Id = key, Name = "added Change" };
            log.Change(data.Id, data);

            data = new DataItem { Id = key, Name = "another Change" };
            log.Change(data.Id, data);

            var logEntry = log.Changelog[1];

            if (logEntry.Data is DataItem obj)
            {
                Assert.AreNotEqual((object)obj.Name, data.Name, "Overwrite Data");
            }

            var dct = log.GetNewItems();

            Assert.AreNotEqual(dct.Count, 1, "Correct amount");

            var item = log.GetPredecessor(1);

            Assert.AreNotEqual(item, 0, "Correct id");

            log.Remove(0);

            Assert.AreNotEqual(log.Changelog.Count, 0, "Correct amount");
        }

        /// <summary>
        ///     Adds the item success.
        /// </summary>
        [TestMethod]
        public void AddItemSuccess()
        {
            var transactionLogs = new TransactionLogs();
            transactionLogs.Add(1, "Item1", false);

            Assert.IsTrue(transactionLogs.Changed);
            Assert.AreEqual(1, transactionLogs.Changelog.Count);
            Assert.AreEqual("Item1", transactionLogs.Changelog[0].Data);
        }

        /// <summary>
        ///     Removes the item success.
        /// </summary>
        [TestMethod]
        public void RemoveItemSuccess()
        {
            var transactionLogs = new TransactionLogs();
            transactionLogs.Add(1, "Item1", false);
            transactionLogs.Remove(1);

            Assert.IsTrue(transactionLogs.Changed);
            Assert.AreEqual(2, transactionLogs.Changelog.Count);
            Assert.AreEqual(LogState.Remove, transactionLogs.Changelog[1].State);
        }

        /// <summary>
        ///     Changes the item success.
        /// </summary>
        [TestMethod]
        public void ChangeItemSuccess()
        {
            var transactionLogs = new TransactionLogs();
            transactionLogs.Add(1, "Item1", false);
            transactionLogs.Change(1, "Item1_Changed");

            Assert.IsTrue(transactionLogs.Changed);
            Assert.AreEqual(2, transactionLogs.Changelog.Count);
            Assert.AreEqual("Item1_Changed", transactionLogs.Changelog[1].Data);
        }

        /// <summary>
        ///     Gets the predecessor success.
        /// </summary>
        [TestMethod]
        public void GetPredecessorSuccess()
        {
            var transactionLogs = new TransactionLogs();
            transactionLogs.Add(1, "Item1", false);
            transactionLogs.Add(2, "Item2", false);
            transactionLogs.Change(1, "Item1_Changed");

            var predecessor = transactionLogs.GetPredecessor(2);

            Assert.AreEqual(0, predecessor);
        }

        /// <summary>
        ///     Gets the new items success.
        /// </summary>
        [TestMethod]
        public void GetNewItemsSuccess()
        {
            var transactionLogs = new TransactionLogs();
            transactionLogs.Add(1, "Item1", true);
            transactionLogs.Add(2, "Item2", false);
            transactionLogs.Change(1, "Item1_Changed");

            var newItems = transactionLogs.GetNewItems();

            Assert.AreEqual(2, newItems.Count);
            Assert.IsFalse(newItems.Values.Any(item => item.StartData));
        }

        /// <summary>
        ///     Gets the index of the item item exists returns.
        /// </summary>
        [TestMethod]
        public void GetItemItemExistsReturnsIndex()
        {
            var transactionLogs = new TransactionLogs();
            transactionLogs.Add(1, "Item1", false);

            var index = transactionLogs.GetItem(1, LogState.Add);

            Assert.AreEqual(0, index);
        }

        /// <summary>
        ///     Gets the item item does not exist returns minus one.
        /// </summary>
        [TestMethod]
        public void GetItemItemDoesNotExistReturnsMinusOne()
        {
            var transactionLogs = new TransactionLogs();

            var index = transactionLogs.GetItem(1, LogState.Add);

            Assert.AreEqual(-1, index);
        }

        /// <summary>
        ///     Gets the new key returns correct key.
        /// </summary>
        [TestMethod]
        public void GetNewKeyReturnsCorrectKey()
        {
            var transactionLogs = new TransactionLogs();
            transactionLogs.Add(1, "Item1", false);

            var newKey = transactionLogs.GetNewKey();

            Assert.AreEqual(1, newKey);
        }
    }
}
