/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrarySqlLiteTests
 * FILE:        CommonLibrarySqlLiteTests/ResourcesSqlLite.cs
 * PURPOSE:     String Resources
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.IO;

namespace CommonLibrarySqlLiteTests;

/// <summary>
///     The resources for Sqlite Tests.
/// </summary>
internal static class ResourcesSqlLite
{
    /// <summary>
    ///     The db database select (const). Value: "DbDatabaseSelect.db".
    /// </summary>
    internal const string? DbDatabaseSelect = "DbDatabaseSelect.db";

    /// <summary>
    ///     The db advanced select (const). Value: "DbAdvancedSelect.db".
    /// </summary>
    internal const string? DbAdvancedSelect = "DbAdvancedSelect.db";

    /// <summary>
    ///     The db select in (const). Value: "DbSelectIn.db".
    /// </summary>
    internal const string? DbSelectIn = "DbSelectIn.db";

    /// <summary>
    ///     The db database create (const). Value: "DbDatabaseCreate.db".
    /// </summary>
    internal const string? DbDatabaseCreate = "DbDatabaseCreate.db";

    /// <summary>
    ///     The db valid column names (const). Value: "DbValidColumnNames.db".
    /// </summary>
    internal const string? DbValidColumnNames = "DbValidColumnNames.db";

    /// <summary>
    ///     The db crash (const). Value: "DbCrash.db".
    /// </summary>
    internal const string? DbCrash = "DbCrash.db";

    /// <summary>
    ///     The db pragma index list (const). Value: "DbPragmaIndexList.db".
    /// </summary>
    internal const string? DbPragmaIndexList = "DbPragmaIndexList.db";

    /// <summary>
    ///     The db table status (const). Value: "DbTableStatus.db".
    /// </summary>
    internal const string? DbTableStatus = "DbTableStatus.db";

    /// <summary>
    ///     The db unique status (const). Value: "DbUniqueStatus.db".
    /// </summary>
    internal const string? DbUniqueStatus = "DbUniqueStatus.db";

    /// <summary>
    ///     The db delete row (const). Value: "DbDeleteRow .db".
    /// </summary>
    internal const string? DbDeleteRow = "DbDeleteRow .db";

    /// <summary>
    ///     The db update (const). Value: "DbUpdate.db".
    /// </summary>
    internal const string? DbUpdate = "DbUpdate.db";

    /// <summary>
    ///     The db detach (const). Value: "DbDetach.db".
    /// </summary>
    internal const string? DbDetach = "DbDetach.db";

    /// <summary>
    ///     The db complex (const). Value: "DbComplex.db".
    /// </summary>
    internal const string? DbComplex = "DbComplex.db";

    /// <summary>
    ///     The db Import Export (const). Value: "DbExport.db".
    /// </summary>
    internal const string? DbImportExport = "DbExport.db";

    /// <summary>
    ///     The db copy table (const). Value: "DbCopyTable.db".
    /// </summary>
    internal const string? DbCopyTable = "DbCopyTable.db";

    /// <summary>
    ///     The db copy table advanced (const). Value: "DbCopyTableAdvanced.db".
    /// </summary>
    internal const string? DbCopyTableAdvanced = "DbCopyTableAdvanced.db";

    /// <summary>
    ///     The root (readonly). Value: Environment.CurrentDirectory.
    /// </summary>
    internal static readonly string? Root = Environment.CurrentDirectory;

    /// <summary>
    ///     Gets the path database crash.
    /// </summary>
    /// <value>
    ///     The path database crash.
    /// </value>
    internal static string? PathDbCrash => Path.Combine(Root, DbCrash);

    /// <summary>
    ///     Gets the path database select.
    /// </summary>
    /// <value>
    ///     The path database select.
    /// </value>
    internal static string? PathDbSelect => Path.Combine(Root, DbDatabaseSelect);

    /// <summary>
    ///     Gets the path copy table.
    /// </summary>
    /// <value>
    ///     The path copy table.
    /// </value>
    internal static string? PathCopyTable => Path.Combine(Root, DbCopyTable);

    /// <summary>
    ///     Gets the path copy table advanced.
    /// </summary>
    /// <value>
    ///     The path copy table advanced.
    /// </value>
    internal static string? PathCopyTableAdvanced => Path.Combine(Root, DbCopyTableAdvanced);

    /// <summary>
    ///     Gets the path database create.
    /// </summary>
    /// <value>
    ///     The path database create.
    /// </value>
    internal static string? PathDbCreate => Path.Combine(Root, DbDatabaseCreate);

    /// <summary>
    ///     Gets the path database row delete.
    /// </summary>
    /// <value>
    ///     The path database row delete.
    /// </value>
    internal static string? PathDbRowDelete => Path.Combine(Root, DbDeleteRow);

    /// <summary>
    ///     Gets the path database create complex.
    /// </summary>
    /// <value>
    ///     The path database create complex.
    /// </value>
    internal static string? PathDbCreateComplex => Path.Combine(Root, DbComplex);

    /// <summary>
    ///     Gets the path database select in.
    /// </summary>
    /// <value>
    ///     The path database select in.
    /// </value>
    internal static string PathDbDbSelectIn => Path.Combine(Root, DbSelectIn);

    /// <summary>
    ///     Gets the path pragma table information.
    /// </summary>
    /// <value>
    ///     The path pragma table information.
    /// </value>
    internal static string? PathPragmaTableInfo => Path.Combine(Root, DbPragmaIndexList);

    /// <summary>
    ///     Gets the path database table status.
    /// </summary>
    /// <value>
    ///     The path database table status.
    /// </value>
    internal static string? PathDatabaseTableStatus => Path.Combine(Root, DbTableStatus);

    /// <summary>
    ///     Gets the path database table unique status.
    /// </summary>
    /// <value>
    ///     The path database table unique status.
    /// </value>
    internal static string? PathDbTableUniqueStatus => Path.Combine(Root, DbUniqueStatus);

    /// <summary>
    ///     Gets the path database update.
    /// </summary>
    /// <value>
    ///     The path database update.
    /// </value>
    internal static string? PathDbUpdate => Path.Combine(Root, DbUpdate);

    /// <summary>
    ///     Gets the path database advanced select.
    /// </summary>
    /// <value>
    ///     The path database advanced select.
    /// </value>
    public static string? PathDbAdvancedSelect => Path.Combine(Root, DbAdvancedSelect);

    /// <summary>
    ///     Gets the path database detach.
    /// </summary>
    /// <value>
    ///     The path database detach.
    /// </value>
    public static string? PathDbDetach => Path.Combine(Root, DbDetach);
}
