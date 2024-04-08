/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/ComCtlResources.cs
 * PURPOSE:     String Resources
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace CommonControls
{
    /// <summary>
    ///     The com Control resources class.
    /// </summary>
    internal static class ComCtlResources
    {
        /// <summary>
        ///     The path (const). Value: "\\".
        /// </summary>
        internal const string Path = "\\";

        /// <summary>
        ///     The path element (const). Value: @"\".
        /// </summary>
        internal const string PathElement = @"\";

        /// <summary>
        ///     The Explorer (const). Value: "Explorer.exe".
        /// </summary>
        internal const string Explorer = "Explorer.exe";

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
        ///     Error, wrong parameters (const). Value: "Wrong Arguments provided".
        /// </summary>
        internal const string ErrorWrongParameters = "Wrong Arguments provided: ";

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
        ///     Image add, needed for the Image Name (const). Value: "T".
        /// </summary>
        internal const string ImageAdd = "T";

        /// <summary>
        ///     File Extension. Value: ".*".
        /// </summary>
        internal const string Appendix = ".*";

        /// <summary>
        ///     The Header for the Directory (const). Value: "Directory Name?".
        /// </summary>
        internal const string HeaderDirectoryName = "Directory Name?";

        /// <summary>
        ///     The description Text (const). Value: "Name of the Folder:".
        /// </summary>
        internal const string TextNameFolder = "Name of the Folder:";

        /// <summary>
        ///     The unique Caption Text (const). Value: "Name was not unique".
        /// </summary>
        internal const string CaptionUnique = "Name was not unique";

        /// <summary>
        ///     The unique Message Text (const). Value: "Name was not unique, but the Property for unique was set.".
        /// </summary>
        internal const string UniqueMessage = "Name was not unique, but the Property for unique was set.";

        /// <summary>
        ///     The unique Message for start List (const). Value: "Start Entry was not empty but unique was set.".
        /// </summary>
        internal const string UniqueMessageStart = "Start Entry was not empty but unique was set.";

        /// <summary>
        ///     The Datalist Entry (const). Value: "Empty".
        /// </summary>
        internal const string DatalistEntry = "Empty";

        /// <summary>
        ///     The Context Menu Deselect (const). Value: "Deselect".
        /// </summary>
        internal const string ContextDeselect = "Deselect";

        /// <summary>
        ///     The Context Menu Deselect all (const). Value: "Deselect All".
        /// </summary>
        internal const string ContextDeselectAll = "Deselect All";

        /// <summary>
        ///     New Item (const). Value: "NewItem".
        /// </summary>
        internal const string NewItem = "New Item";

        /// <summary>
        ///     Information about Plugin Status (const). Value: "No Plugins found.".
        /// </summary>
        internal const string InformationPlugin = "No Plugins found.";

        /// <summary>
        ///     Separator (const). Value: " , ".
        /// </summary>
        internal const string Separator = " , ";
    }
}
