/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteHelper
 * FILE:        SQLiteHelper/SQLiteHelperResources.cs
 * PURPOSE:     Basic String Resources and magic Numbers
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace SQLiteHelper
{
    /// <summary>
    ///     The sql lite helper resources class.
    /// </summary>
    internal static class SqLiteHelperResources
    {
        /// <summary>
        ///     0 cid not needed by us, id of column
        ///     1 name
        ///     2 type
        ///     3 not null is 1 is true
        ///     4 default Value
        ///     5 Primary Key 1 is primary Key
        /// </summary>
        internal const string TableContentsName = "1";

        /// <summary>
        ///     The data source (const). Value: "Data Source=".
        /// </summary>
        internal const string DataSource = "Data Source=";

        /// <summary>
        ///     The data version (const). Value: ";Version=".
        /// </summary>
        internal const string DataVersion = ";Version=";

        /// <summary>
        ///     The standard name (const). Value: "SqlLiteDB.db".
        /// </summary>
        internal const string StandardName = "SqlLiteDB.db";

        /// <summary>
        ///     The bracket open (const). Value: " (".
        /// </summary>
        internal const string BracketOpen = " ( ";

        /// <summary>
        ///     The bracket close (const). Value: " ) ".
        /// </summary>
        internal const string BracketClose = " ) ";

        /// <summary>
        ///     The end (const). Value: ";".
        /// </summary>
        internal const string End = ";";

        /// <summary>
        ///     The spacing (const). Value: " ".
        /// </summary>
        internal const string Spacing = " ";

        /// <summary>
        ///     The escape (const). Value: "'".
        /// </summary>
        internal const string Escape = "'";

        /// <summary>
        ///     The comma (const). Value: " , ".
        /// </summary>
        internal const string Comma = " , ";

        /// <summary>
        ///     The star (const). Value: " * ".
        /// </summary>
        internal const string Star = " * ";

        /// <summary>
        ///     The param (const). Value: "@param".
        /// </summary>
        internal const string Param = "@param";

        /// <summary>
        ///     The sql where (const). Value: "Where".
        /// </summary>
        internal const string SqlWhere = "Where";

        /// <summary>
        ///     The sql order by (const). Value: "order by".
        /// </summary>
        internal const string SqlOrderBy = "order by";

        /// <summary>
        ///     The sql not null (const). Value: " NOT NULL".
        /// </summary>
        internal const string SqlNotNull = " NOT NULL";

        /// <summary>
        ///     The sql select (const). Value: "Select ".
        /// </summary>
        internal const string SqlSelect = "Select ";

        /// <summary>
        ///     The sql db ext (const). Value: ".db".
        /// </summary>
        internal const string SqlDbExt = ".db";

        /// <summary>
        ///     The sql in (const). Value: "IN".
        /// </summary>
        internal const string SqlIn = "IN";

        /// <summary>
        ///     The comp none (const). Value: "none".
        /// </summary>
        internal const string CompNone = "none";

        /// <summary>
        ///     The comp equal (const). Value: "=".
        /// </summary>
        internal const string CompEqual = "=";

        /// <summary>
        ///     The comp not equal (const). Value: "!=".
        /// </summary>
        internal const string CompNotEqual = "!=";

        /// <summary>
        ///     The comp like (const). Value: "like".
        /// </summary>
        internal const string CompLike = "like";

        /// <summary>
        ///     The comp not like (const). Value: "not like".
        /// </summary>
        internal const string CompNotLike = "not like";

        /// <summary>
        ///     The sql lite data type integer (const). Value: "integer".
        /// </summary>
        internal const string SqlLiteDataTypeInteger = "integer";

        /// <summary>
        ///     The sql lite data type decimal (const). Value: "decimal".
        /// </summary>
        internal const string SqlLiteDataTypeDecimal = "decimal";

        /// <summary>
        ///     The sql lite data type date time (const). Value: "datetime".
        /// </summary>
        internal const string SqlLiteDataTypeDateTime = "datetime";

        /// <summary>
        ///     The sql lite data type real (const). Value: "real".
        /// </summary>
        internal const string SqlLiteDataTypeReal = "real";

        /// <summary>
        ///     The sql lite data type text (const). Value: "Text".
        /// </summary>
        internal const string SqlLiteDataTypeText = "text";

        /// <summary>
        ///     The convert to (const). Value: " to: ".
        /// </summary>
        internal const string ConvertTo = " to: ";

        /// <summary>
        ///     The error check (const). Value: "Error".
        /// </summary>
        internal const string ErrorCheck = "Error";

        /// <summary>
        ///     The message error (const). Value: "Error: ".
        /// </summary>
        internal const string MessageError = "Error: ";

        /// <summary>
        ///     The message warning (const). Value: "Warning: ".
        /// </summary>
        internal const string MessageWarning = "Warning: ";

        /// <summary>
        ///     The message info (const). Value: "Information: ".
        /// </summary>
        internal const string MessageInfo = "Information: ";

        /// <summary>
        ///     The message initiate (const). Value: "Initiate: ".
        /// </summary>
        internal const string MessageInitiate = "Initiate: ";

        /// <summary>
        ///     The context switch log (const). Value: "DatabaseContext was switched".
        /// </summary>
        internal const string ContextSwitchLog = "DatabaseContext was switched";

        /// <summary>
        ///     The success executed log (const). Value: "was executed ".
        /// </summary>
        internal const string SuccessExecutedLog = "was executed ";

        /// <summary>
        ///     The success created log (const). Value: "was created ".
        /// </summary>
        internal const string SuccessCreatedLog = "was created ";

        /// <summary>
        ///     The success deleted log (const). Value: "was deleted ".
        /// </summary>
        internal const string SuccessDeletedLog = "was deleted ";

        /// <summary>
        ///     The error database already exists (const). Value: "Database already exists".
        /// </summary>
        internal const string ErrorDatabaseAlreadyExists = "Database already exists";

        /// <summary>
        ///     The error table does not exist (const). Value: "Table does not exist".
        /// </summary>
        internal const string ErrorTableDoesNotExist = "Table does not exist";

        /// <summary>
        ///     The error table does already exist (const). Value: "Table does already exist".
        /// </summary>
        internal const string ErrorTableDoesAlreadyExist = "Table does already exist";

        /// <summary>
        ///     The error table key constraint (const). Value: "Table Unique Key Constraint was violated".
        /// </summary>
        internal const string ErrorTableKeyConstraint = "Table Unique Key Constraint was violated";

        /// <summary>
        ///     The error table info not found (const). Value: "Table Info was not found".
        /// </summary>
        internal const string ErrorTableInfoNotFound = "Table Info was not found";

        /// <summary>
        ///     The error convert table infos (const). Value: " in ConvertTableHeaders ".
        /// </summary>
        internal const string ErrorConvertTableInfos = " in ConvertTableHeaders ";

        /// <summary>
        ///     The error check unique table headers (const). Value: " in CheckUniqueTableHeaders ".
        /// </summary>
        internal const string ErrorCheckUniqueTableHeaders = " in CheckUniqueTableHeaders ";

        /// <summary>
        ///     The error get table header (const). Value: " in GetTableHeader".
        /// </summary>
        internal const string ErrorGetTableHeader = " in GetTableHeader";

        /// <summary>
        ///     The error pragma index list (const). Value: " in Pragma_index_list".
        /// </summary>
        internal const string ErrorPragmaIndexList = " in Pragma_index_list";

        /// <summary>
        ///     The error add unique status (const). Value: " in AddUniqueStatus".
        /// </summary>
        internal const string ErrorAddUniqueStatus = " in AddUniqueStatus";

        /// <summary>
        ///     The error deleted (const). Value: "was not deleted ".
        /// </summary>
        internal const string ErrorDeleted = "was not deleted ";

        /// <summary>
        ///     The error insert could not get table info error (const). Value: "While inserting could not get info by
        ///     Pragma_index_list was switched".
        /// </summary>
        internal const string ErrorInsertCouldNotGetTableInfoError =
            "While inserting could not get info by Pragma_index_list was switched";

        /// <summary>
        ///     The error insert single row (const). Value: " In Insert".
        /// </summary>
        internal const string ErrorInsertSingleRow = " In Insert";

        /// <summary>
        ///     The error more elements to add than rows (const). Value: "In InsertSingleRow, more Elements to add than rows".
        /// </summary>
        internal const string ErrorMoreElementsToAddThanRows =
            "In InsertSingleRow, more Elements to add than rows";

        /// <summary>
        ///     The error wrong type (const). Value: "Can't Convert Value to expected Type: ".
        /// </summary>
        internal const string ErrorWrongType = "Can't Convert Value to expected Type: ";

        /// <summary>
        ///     The error not null able (const). Value: "Value is Null but Table is not Nullable".
        /// </summary>
        internal const string ErrorNotNullAble = "Value is Null but Table is not Nullable";

        /// <summary>
        ///     The error not unique (const). Value: "Values added are not Unique".
        /// </summary>
        internal const string ErrorNotUnique = "Values added are not Unique";

        /// <summary>
        ///     The error simple select parameters (const). Value: "Error in SimpleSelect Parameters provided contained an error".
        /// </summary>
        internal const string ErrorSimpleSelectParameters =
            "Error in SimpleSelect Parameters provided contained an error";

        /// <summary>
        ///     The error simple select execution (const). Value: "Error in SimpleSelect Query Execution, Empty Result set".
        /// </summary>
        internal const string ErrorSimpleSelectExecution =
            "Error in SimpleSelect Query Execution, Empty Result set";

        /// <summary>
        ///     The error db info create (const). Value: "Tried to create Database, overwrite was set to: ".
        /// </summary>
        internal const string ErrorDbInfoCreate = "Tried to create Database, overwrite was set to: ";

        /// <summary>
        ///     The error db not found (const). Value: "Error Database not Found: ".
        /// </summary>
        internal const string ErrorDbNotFound = "Error Database not Found: ";

        /// <summary>
        ///     The error db info delete (const). Value: "Tried to Delete Database, path: ".
        /// </summary>
        internal const string ErrorDbInfoDelete = "Tried to Delete Database, path: ";

        /// <summary>
        ///     The error update table (const). Value: " In Update".
        /// </summary>
        internal const string ErrorUpdateTable = " In Update";

        /// <summary>
        ///     The error delete rows (const). Value: " In DeleteRows".
        /// </summary>
        internal const string ErrorDeleteRows = " In DeleteRows";

        /// <summary>
        ///     The error in select statement (const). Value: " Select Statement did not return correct values: ".
        /// </summary>
        internal const string ErrorInSelectStatement = " Select Statement did not return correct values: ";

        /// <summary>
        ///     The error empty input (const). Value: " Input Value was empty ".
        /// </summary>
        internal const string ErrorEmptyInput = " Input Value was empty ";

        /// <summary>
        ///     The Information Property is protected (const). Value: "Property is protected.".
        /// </summary>
        internal const string InformationPropertyProtected = "Property is protected.";

        /// <summary>
        ///     The Information Property needs a Parameter (const). Value: "Property needs a Parameter.".
        /// </summary>
        internal const string InformationPropertyNeedsParameter = "Property needs a Parameter.";

        /// <summary>
        ///     The Information Property was null (const). Value: "Property was null.".
        /// </summary>
        internal const string InformationPropertyWasNull = "Property was null.";

        /// <summary>
        ///     The suppress error (const). Value: true.
        /// </summary>
        internal const bool SuppressError = true;

        /// <summary>
        ///     The do not suppress error (const). Value: false.
        /// </summary>
        internal const bool DoNotSuppressError = false;
    }
}
