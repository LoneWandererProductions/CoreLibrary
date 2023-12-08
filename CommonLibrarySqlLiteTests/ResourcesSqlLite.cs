/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrarySqlLiteTests
 * FILE:        CommonLibrarySqlLiteTests/ResourcesSqlLite.cs
 * PURPOSE:     String Resources
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.IO;

namespace CommonLibrarySqlLiteTests
{
    /// <summary>
    ///     The resources for Sqlite Tests.
    /// </summary>
    internal static class ResourcesSqlLite
    {
        /// <summary>
        ///     The db database select (const). Value: "DbDatabaseSelect.db".
        /// </summary>
        internal const string DbDatabaseSelect = "DbDatabaseSelect.db";

        /// <summary>
        ///     The db advanced select (const). Value: "DbAdvancedSelect.db".
        /// </summary>
        internal const string DbAdvancedSelect = "DbAdvancedSelect.db";

        /// <summary>
        ///     The db select in (const). Value: "DbSelectIn.db".
        /// </summary>
        internal const string DbSelectIn = "DbSelectIn.db";

        /// <summary>
        ///     The db database create (const). Value: "DbDatabaseCreate.db".
        /// </summary>
        internal const string DbDatabaseCreate = "DbDatabaseCreate.db";

        /// <summary>
        ///     The db valid column names (const). Value: "DbValidColumnNames.db".
        /// </summary>
        internal const string DbValidColumnNames = "DbValidColumnNames.db";

        /// <summary>
        ///     The db crash (const). Value: "DbCrash.db".
        /// </summary>
        internal const string DbCrash = "DbCrash.db";

        /// <summary>
        ///     The db pragma index list (const). Value: "DbPragmaIndexList.db".
        /// </summary>
        internal const string DbPragmaIndexList = "DbPragmaIndexList.db";

        /// <summary>
        ///     The db table status (const). Value: "DbTableStatus.db".
        /// </summary>
        internal const string DbTableStatus = "DbTableStatus.db";

        /// <summary>
        ///     The db unique status (const). Value: "DbUniqueStatus.db".
        /// </summary>
        internal const string DbUniqueStatus = "DbUniqueStatus.db";

        /// <summary>
        ///     The db delete row (const). Value: "DbDeleteRow .db".
        /// </summary>
        internal const string DbDeleteRow = "DbDeleteRow .db";

        /// <summary>
        ///     The db update (const). Value: "DbUpdate.db".
        /// </summary>
        internal const string DbUpdate = "DbUpdate.db";

        /// <summary>
        ///     The db detach (const). Value: "DbDetach.db".
        /// </summary>
        internal const string DbDetach = "DbDetach.db";

        /// <summary>
        ///     The db complex (const). Value: "DbComplex.db".
        /// </summary>
        internal const string DbComplex = "DbComplex.db";

        /// <summary>
        ///     The db Import Export (const). Value: "DbExport.db".
        /// </summary>
        internal const string DbImportExport = "DbExport.db";

        /// <summary>
        ///     The db copy table (const). Value: "DbCopyTable.db".
        /// </summary>
        internal const string DbCopyTable = "DbCopyTable.db";

        /// <summary>
        ///     The db copy table advanced (const). Value: "DbCopyTableAdvanced.db".
        /// </summary>
        internal const string DbCopyTableAdvanced = "DbCopyTableAdvanced.db";

        /// <summary>
        ///     The root (readonly). Value: Environment.CurrentDirectory.
        /// </summary>
        internal static readonly string Root = Environment.CurrentDirectory;

        internal static string PathDbCrash => Path.Combine(Root, DbCrash);

        internal static string PathDbSelect => Path.Combine(Root, DbDatabaseSelect);

        internal static string PathCopyTable => Path.Combine(Root, DbCopyTable);

        internal static string PathCopyTableAdvanced => Path.Combine(Root, DbCopyTableAdvanced);

        internal static string PathDbCreate => Path.Combine(Root, DbDatabaseCreate);

        internal static string PathDbRowDelete => Path.Combine(Root, DbDeleteRow);

        internal static string PathDbCreateComplex => Path.Combine(Root, DbComplex);

        internal static string PathDbDbImportExport => Path.Combine(Root, DbImportExport);

        internal static string PathDbDbSelectIn => Path.Combine(Root, DbSelectIn);

        internal static string PathPragmaTableInfo => Path.Combine(Root, DbPragmaIndexList);

        internal static string PathDatabaseTableStatus => Path.Combine(Root, DbTableStatus);

        internal static string PathDbTableUniqueStatus => Path.Combine(Root, DbUniqueStatus);

        internal static string PathDbUpdate => Path.Combine(Root, DbUpdate);

        public static string PathDbAdvancedSelect => Path.Combine(Root, DbAdvancedSelect);

        public static string PathDbDetach => Path.Combine(Root, DbDetach);
    }
}
