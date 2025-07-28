/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     SQLiteGui
* FILE:        SQLiteGui/SQLiteGuiResource.cs
* PURPOSE:     String Resources of SQLiteGui
* PROGRAMER:   Wayfarer
*/

namespace SQLiteGui;

/// <summary>
///     The sql lite Gui resource class.
/// </summary>
internal static class SqLiteGuiResource
{
    // Messages

    /// <summary>
    ///     The error no tables (const). Value: "No Tables Found".
    /// </summary>
    internal const string ErrorNoTables = "No Tables Found";

    /// <summary>
    ///     The error no valid path (const). Value: "No Valid Path".
    /// </summary>
    internal const string ErrorNoValidPath = "No Valid Path";

    /// <summary>
    ///     The error create db (const). Value: "Error: Creating Database".
    /// </summary>
    internal const string ErrorCreateDb = "Error: Creating Database";

    /// <summary>
    ///     The error no valid db name (const). Value: "Error: No valid Database Name".
    /// </summary>
    internal const string ErrorNoValidDbName = "Error: No valid Database Name";

    /// <summary>
    ///     The error empty select (const). Value: "Error: Select was Empty".
    /// </summary>
    internal const string ErrorEmptySelect = "Error: Select was Empty";

    /// <summary>
    ///     The error no valid table name (const). Value: "Error: Invalid Table Alias".
    /// </summary>
    internal const string ErrorNoValidTableName = "Error: Invalid Table Alias";

    /// <summary>
    ///     The error no valid database name (const). Value: "No Database was provided".
    /// </summary>
    internal const string ErrorNoValidDatabaseName = "No Database was provided";

    /// <summary>
    ///     The error duplicate table name (const). Value: "Table Name is already in use".
    /// </summary>
    internal const string ErrorDuplicateTableName = "Table Name is already in use";

    /// <summary>
    ///     The warning no primary key (const). Value: "Warning, no PrimaryKey available".
    /// </summary>
    internal const string WarningNoPrimaryKey = "Warning, no PrimaryKey available";

    /// <summary>
    ///     The info rows (const). Value: "Rows affected: ".
    /// </summary>
    internal const string InfoRows = "Rows affected: ";

    /// <summary>
    ///     The info create db (const). Value: "Database created".
    /// </summary>
    internal const string InfoCreateDb = "Database created";

    /// <summary>
    ///     The info primary key (const). Value: "Table PrimaryKey: ".
    /// </summary>
    internal const string InfoPrimaryKey = "Table PrimaryKey: ";

    //Data-binding

    /// <summary>
    ///     The obs column (const). Value: "Column".
    /// </summary>
    internal const string ObsColumn = "Column";

    //Internal Values

    /// <summary>
    ///     The start column (const). Value: 0.
    /// </summary>
    internal const int StartColumn = 0;

    /// <summary>
    ///     The Parameter dummy (const). Value: "Dummy".
    /// </summary>
    internal const string ParamDummy = "Dummy";

    /// <summary>
    ///     The separator (const). Value: " , ".
    /// </summary>
    internal const string Separator = " , ";

    /// <summary>
    ///     The db filter (const). Value: "Database File(*.db)|*.db|All files (*.*)|*.*".
    /// </summary>
    internal const string DbFilter = "Database File(*.db)|*.db|All files (*.*)|*.*";

    /// <summary>
    ///     The header (const). Value: "Table Name, DataType, Unique, PrimaryKey".
    /// </summary>
    internal const string Header = "Table Name, DataType, Unique, PrimaryKey";

    /// <summary>
    ///     The Type (const). Value: "Database Viewer".
    /// </summary>
    internal const string Type = "Database Viewer";

    /// <summary>
    ///     The Description (const). Value: "A simple Database Viewer with some administrative tools.".
    /// </summary>
    internal const string Description = "A simple Database Viewer with some administrative tools.";
}
