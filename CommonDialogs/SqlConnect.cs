/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonDialogs/SqlConnect.cs
 * PURPOSE:     Class that will build a sql Connection string in the future
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCE:      https://stackoverflow.com/questions/55590869/how-to-protect-strings-without-securestring
 * NOTE:        Based on security concerns we won't provide a login via password and id over .Net, for more information read provided Link
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using System.Diagnostics;
using System.Text;

namespace CommonDialogs
{
    /// <summary>
    ///     The Sql connection string class
    /// </summary>
    public sealed class SqlConnect
    {
        /// <summary>
        ///     The persist security information, for Security reasons always deactivated
        ///     Until I know of a case where it is necessary it stays that way.
        /// </summary>
        internal const bool PersistSecurityInfo = false;

        /// <summary>
        ///     The persist information string configuration for the connection string.
        /// </summary>
        private readonly string _persistInfo = string.Concat(ComCtlResources.DbPersistSecurityInfo, PersistSecurityInfo,
            ComCtlResources.DbFin);

        /// <summary>
        ///     The IntegratedSecurity string for the connection string
        /// </summary>
        private string _security;

        /// <summary>
        ///     The TrustServerCertificate string for the connection string
        /// </summary>
        private string _trust;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlConnect" /> class.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="server">The server.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        internal SqlConnect(string database, string server, bool isActive)
        {
            Server = server;
            Database = database;
            TrustServerCertificate = isActive;
        }

        /// <summary>
        ///     Gets or sets the server.
        ///     Set to be changed External
        /// </summary>
        /// <value>
        ///     The server.
        /// </value>
        public string Server { get; set; }

        /// <summary>
        ///     Gets or sets the database.
        ///     Set to be changed External
        /// </summary>
        /// <value>
        ///     The database.
        /// </value>
        public string Database { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [trust server certificate].
        ///     Set to be changed External, sometimes this can cause Problems, if it is not supported by the server.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [trust server certificate]; otherwise, <c>false</c>.
        /// </value>
        public bool TrustServerCertificate { get; set; }

        /// <summary>
        ///     Gets the connection string.
        /// </summary>
        /// <param name="includeDatabaseName">if set to <c>true</c> [include database name].</param>
        /// <returns>Connection String</returns>
        public string GetConnectionString(bool includeDatabaseName)
        {
            _security = ComCtlResources.DbIntegratedTrue;
            _trust = TrustServerCertificate
                ? ComCtlResources.DbTrustServerCertificateTrue
                : ComCtlResources.DbTrustServerCertificateFalse;

            if (string.IsNullOrEmpty(Server))
            {
                Trace.WriteLine(ComCtlResources.DbServerError);
                return ComCtlResources.DbServerError;
            }

            if (includeDatabaseName && string.IsNullOrEmpty(Database))
            {
                Trace.WriteLine(ComCtlResources.DbNameError);
                return ComCtlResources.DbServerError;
            }

            var connectionStringBuilder = new StringBuilder();
            connectionStringBuilder.Append(_persistInfo);
            connectionStringBuilder.Append(_trust);
            connectionStringBuilder.Append(_security);
            connectionStringBuilder.Append(ComCtlResources.DbServer);
            connectionStringBuilder.Append(Server);
            connectionStringBuilder.Append(ComCtlResources.DbFin);

            if (includeDatabaseName)
            {
                connectionStringBuilder.Append(ComCtlResources.DbDatabase);
                connectionStringBuilder.Append(Database);
                connectionStringBuilder.Append(ComCtlResources.DbFin);
            }

            return connectionStringBuilder.ToString();
        }

        ///// <summary>
        ///// Gets or sets a value indicating whether [integrated security].
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if [integrated security]; otherwise, <c>false</c>.
        ///// </value>
        //public bool IntegratedSecurity { get; set; } = true;

        ///// <summary>
        ///// Gets or sets the user identifier.
        ///// </summary>
        ///// <value>
        ///// The user identifier.
        ///// </value>
        //public string UserId { get; set; }

        ///// <summary>
        ///// Gets or sets the password.
        ///// </summary>
        ///// <value>
        ///// The password.
        ///// </value>
        //public string Password { get; set; }

        ///// <summary>
        ///// Authentication with SqlClient and Password.
        ///// </summary>
        ///// <returns>Connection string</returns>
        //private string SqlAuthentication()
        //{
        //    if (string.IsNullOrEmpty(UserId)) return "Error: UserId";
        //    if (string.IsNullOrEmpty(Password)) return "Error: Password";
        //    if (string.IsNullOrEmpty(Server)) return "Error: Server Name";
        //    if (string.IsNullOrEmpty(Database)) return "Error: Database Name";

        //    string user = string.Concat("User ID = ", UserId, ");");
        //    string password = string.Concat("Password =", Password, ");");

        //    return string.Concat(_persistInfo, _trust, _security, user, password, Server, ";", Database);
        //}
    }
}
