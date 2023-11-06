/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/Transactions.cs
 * PURPOSE:     Test the Transaction Log
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using CommonControls;
using ExtendedSystemObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
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

            var data = new DataItem
            {
                Id = key,
                Name = "start"
            };

            //start of the event, nothing was done yet

            log.Add(0, data, true);

            key = log.GetNewKey();

            data = new DataItem
            {
                Id = key,
                Name = "added"
            };
            log.Add(data.Id, data, false);

            data = new DataItem
            {
                Id = key,
                Name = "added Change"
            };
            log.Change(data.Id, data);

            data = new DataItem
            {
                Id = key,
                Name = "another Change"
            };
            log.Change(data.Id, data);

            var logEntry = log.Changelog[1];

            if (logEntry.Data is DataItem obj) Assert.AreNotEqual((object)obj.Name, data.Name, "Overwrite Data");

            var dct = log.GetNewItems();

            Assert.AreNotEqual(dct.Count, 1, "Correct amount");

            var item = log.GetPredecessor(1);

            Assert.AreNotEqual(item, 0, "Correct id");

            log.Remove(0);

            Assert.AreNotEqual(log.Changelog.Count, 0, "Correct amount");
        }
    }
}
