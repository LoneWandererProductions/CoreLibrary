﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/SqlConnect.cs
 * PURPOSE:     Class that will build a sql Connection string in the future
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace CommonControls
{
    /// <summary>
    /// The Sql connection string class
    /// </summary>
    public class SqlConnect
    {
        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [integrated security].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [integrated security]; otherwise, <c>false</c>.
        /// </value>
        public bool IntegratedSecurity { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether [trust server certificate].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [trust server certificate]; otherwise, <c>false</c>.
        /// </value>
        public bool TrustServerCertificate { get; set; } = true;

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }

        /// <summary>
        /// The persist security information, for Security reasons always deactivated
        /// Until I know of a case where it is necessary it stays that way.
        /// </summary>
        public const bool PersistSecurityInfo = false;

        /// <summary>
        /// The persist information string configuration for the connection string.
        /// </summary>
        private readonly string _persistInfo = string.Concat("PersistSecurity Info= ", PersistSecurityInfo.ToString(), ";");

        /// <summary>
        /// The IntegratedSecurity string for the connection string
        /// </summary>
        private string _security;

        /// <summary>
        /// The TrustServerCertificate string for the connection string
        /// </summary>
        private string _trust;

        /// <summary>
        /// Gets the connection string to a SQL Server.
        /// </summary>
        /// <returns>Complete Connection string based on chosen Connection Typ</returns>
        public string GetConnectionString()
        {
            _security = IntegratedSecurity ? @"Integrated Security=True;" : @"Integrated Security=False;";
            _trust = TrustServerCertificate ? @"TrustServerCertificate=True;" : @"TrustServerCertificate=False;";
            return IntegratedSecurity ? SqlWindowsAuthentication() : SqlAuthentication();
        }

        /// <summary>
        /// Authentication with Windows Authentication.
        /// </summary>
        /// <returns>Connection string</returns>
        private string SqlWindowsAuthentication()
        {
            if (string.IsNullOrEmpty(Server)) return "Error: Server Name";
            if (string.IsNullOrEmpty(Database)) return "Error: Database Name";

            return string.Concat(_persistInfo, _trust, _security, Server, ";", Database);
        }

        /// <summary>
        /// Authentication with SqlClient and Password.
        /// </summary>
        /// <returns>Connection string</returns>
        private string SqlAuthentication()
        {
            if (string.IsNullOrEmpty(UserId)) return "Error: UserId";
            if (string.IsNullOrEmpty(Password)) return "Error: Password";
            if (string.IsNullOrEmpty(Server)) return "Error: Server Name";
            if (string.IsNullOrEmpty(Database)) return "Error: Database Name";

            string user = string.Concat("User ID = ", UserId, ");");
            string password = string.Concat("Password =", Password, ");");

            return string.Concat(_persistInfo, _trust, _security, user, password, Server, ";", Database);
        }
    }
}
