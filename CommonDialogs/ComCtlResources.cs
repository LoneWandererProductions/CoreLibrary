/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonDialogs
 * FILE:        CommonDialogs/ComCtlResources.cs
 * PURPOSE:     String Resources
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace CommonDialogs
{
    /// <summary>
    ///     The com Control resources class.
    /// </summary>
    internal static class ComCtlResources
    {
        /// <summary>
        ///     The path element (const). Value: @"\".
        /// </summary>
        internal const string PathElement = @"\";

        /// <summary>
        ///     The Image for the drive (const). @"System\drive.png".
        /// </summary>
        internal const string DriveImage = @"System\drive.png";

        /// <summary>
        ///     The Image for the folder (const). Value: = @"System\folder.png".
        /// </summary>
        internal const string FolderImage = @"System\folder.png";

        /// <summary>
        ///     The error conversion (const). Value: "Could not convert back".
        /// </summary>
        internal const string ErrorConversion = "Could not convert back";

        /// <summary>
        ///     Error, Database problem with the Server Name (const). Value: "Error: Server Name was empty.".
        /// </summary>
        internal const string DbServerError = "Error: Server Name was empty.";

        /// <summary>
        ///     Error, Database problem with the Database Name (const). Value: "Error: Database Name was empty.".
        /// </summary>
        internal const string DbNameError = "Error: Database Name was empty.";

        /// <summary>
        ///     Database string, about PersistSecurity Info (const). Value: "PersistSecurity Info".
        /// </summary>
        internal const string DbPersistSecurityInfo = "PersistSecurityInfo=";

        /// <summary>
        ///     The database integrated Security set to true (const). Value: "Integrated Security=True;".
        /// </summary>
        internal const string DbIntegratedTrue = "Integrated Security=True;";

        /// <summary>
        ///     The database Trust Server Certificate set to false (const). Value: "TrustServerCertificate=False;".
        /// </summary>
        internal const string DbTrustServerCertificateFalse = "TrustServerCertificate=False;";

        /// <summary>
        ///     The database Trust Server Certificate set to True (const). Value: "TrustServerCertificate=True;".
        /// </summary>
        internal const string DbTrustServerCertificateTrue = "TrustServerCertificate=True;";

        /// <summary>
        ///     The database Log Message, Connection object with the data was created (const). Value: "SQL Connection object was
        ///     created."
        /// </summary>
        internal const string DbLogConnectionStringBuild = "SQL Connection object was created.";

        /// <summary>
        ///     The database Log Message, Connection object with the data was not created complete (const). Value: ""SQL Connection
        ///     was not created correctly."
        /// </summary>
        internal const string DbLogConnectionStringBuildError = "Warning, SQL Connection was not created correctly.";

        /// <summary>
        ///     Database string, command end (const). Value: ";".
        /// </summary>
        internal const string DbFin = ";";

        /// <summary>
        ///     Database string, tag for Server (const). Value: "Server = ".
        /// </summary>
        internal const string DbServer = "Server=";

        /// <summary>
        ///     Database string, tag for Database (const). Value: "Database = ".
        /// </summary>
        internal const string DbDatabase = "Database=";

        /// <summary>
        ///     File Extension. Value: ".*".
        /// </summary>
        internal const string Appendix = ".*";
    }
}
