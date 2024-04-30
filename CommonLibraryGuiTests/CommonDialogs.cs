/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryGuiTests
 * FILE:        CommonLibraryGuiTests/CommonDialogs.cs
 * PURPOSE:     Tests for CommonCtrl some Controls, not all yet
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedVariable

using System;
using System.Threading;
using CommonDialogs;
using NUnit.Framework;

namespace CommonLibraryGuiTests
{
    /// <summary>
    ///     The common Controls unit test class.
    /// </summary>
    public sealed class CommonDialogs
    {
        /// <summary>
        ///     Creates and initiates all basic custom controls
        ///     It may sound stupid, but some may throw exceptions because of my own stupidity.
        /// </summary>
        [Test]
        [Apartment(ApartmentState.STA)]
        public void Basic()
        {
            _ = new FolderControl();

            Assert.Pass();
        }

        /// <summary>
        ///     Test of the ConnectionString Dialog.
        /// </summary>
        [Test]
        [Apartment(ApartmentState.STA)]
        public void ConnectionString()
        {
            var login = new SqlLogin();
            login.Show();
            login.View.Server = "SqlServer";
            login.View.Database = @"MyDB\Hello";
            login.View.ConnectCommand.Execute(null);
            var db = login.View.ConnectionStringDb;
            var server = login.View.ConnectionStringServer;
            login.Close();

            Assert.IsTrue(
                db.Equals(
                    @"PersistSecurityInfo=False;TrustServerCertificate=True;Integrated Security=True;Server=SqlServer;Database=MyDB\Hello;",
                    StringComparison.Ordinal),
                $"Wrong Connection string: {db}");

            Assert.IsTrue(
                server.Equals(
                    "PersistSecurityInfo=False;TrustServerCertificate=True;Integrated Security=True;Server=SqlServer;",
                    StringComparison.Ordinal),
                $"Wrong Connection string: {server}");
        }
    }
}
