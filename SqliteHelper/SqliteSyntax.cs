/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/SqLiteDataTypes.cs
 * PURPOSE:     Enums of Types Sqlite Supports and Syntax Checking
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace SqliteHelper
{
    /// <summary>
    ///     Handles syntax checking and data validation for SQLite operations
    /// </summary>
    internal sealed class SqliteSyntax
    {
        /// <summary>
        ///     Send our Message to the Subscribers
        /// </summary>
        private readonly EventHandler<MessageItem> _setMessage;

        /// <summary>
        ///     Logging of System Messages
        /// </summary>
        private MessageItem _message;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteSyntax" /> class.
        /// </summary>
        /// <param name="setMessage">The set message.</param>
        public SqliteSyntax(EventHandler<MessageItem> setMessage = null)
        {
            _setMessage = setMessage;
        }

        /// <summary>
        ///     Basic sanity checks.
        ///     Refactored to check the Entire Row against the Entire Schema.
        /// </summary>
        /// <param name="tableInfo">Headers, Data Type and Constraints of the table</param>
        /// <param name="row">The specific Row we are checking</param>
        /// <param name="batch">The full batch of rows (needed for uniqueness checks within the batch)</param>
        /// <returns>true if no errors were found</returns>
        internal bool AdvancedSyntaxCheck(
            Dictionary<string, TableColumns> tableInfo,
            TableSet row,
            IEnumerable<TableSet> batch)
        {
            // Sanity Check: Do the column counts match?
            if (tableInfo.Count != row.Row.Count)
            {
                LogError(SqliteHelperResources.ErrorMoreElementsToAddThanRows);
                return false;
            }

            // Convert dictionary values to a list to access by index
            // (Assuming order matches Insert logic)
            var schemaColumns = tableInfo.Values.ToList();

            // Loop through each column in the row
            for (int i = 0; i < row.Row.Count; i++)
            {
                var value = row.Row[i];
                var definition = schemaColumns[i];

                // Check Nullability
                if (!CheckNullability(value, definition))
                {
                    return false;
                }

                // Check Data Type
                if (!CheckTypeCompatibility(value, definition))
                {
                    return false;
                }

                // Check Uniqueness (Primary Key / Unique Constraint)
                if (!CheckBatchUniqueness(value, i, definition, batch))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Checks the nullability.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="convert">The column definition.</param>
        /// <returns>Condition fulfilled</returns>
        private bool CheckNullability(string value, TableColumns convert)
        {
            // Logic Fix: Valid if value exists OR if the column allows nulls (NOT NotNull).
            if (!string.IsNullOrEmpty(value) || !convert.NotNull)
            {
                return true;
            }

            LogError($"{SqliteHelperResources.ErrorNotNullAble} (Column: {convert.RowId})");
            return false;
        }

        /// <summary>
        ///     Checks the type compatibility.
        /// </summary>
        /// <param name="value">The value string.</param>
        /// <param name="convert">The column definition.</param>
        /// <returns>Condition fulfilled</returns>
        private bool CheckTypeCompatibility(string value, TableColumns convert)
        {
            // If empty and nullable, skip type check
            if (string.IsNullOrEmpty(value)) return true;

            var check = SqliteProcessing.CheckConvert(convert.DataType, value);
            if (check)
            {
                return true;
            }

            LogError($"{SqliteHelperResources.ErrorWrongType} - Value: {value}, Expected Type: {convert.DataType}");
            return false;
        }

        /// <summary>
        ///     Checks the uniqueness within the current batch.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="columnIndex">Index of the column.</param>
        /// <param name="convert">The column definition.</param>
        /// <param name="batch">The full batch of rows.</param>
        /// <returns>True if unique in batch</returns>
        private bool CheckBatchUniqueness(string value, int columnIndex, TableColumns convert,
            IEnumerable<TableSet> batch)
        {
            // Only check if it is a Primary Key or explicitly Unique
            if (!convert.PrimaryKey && !convert.Unique)
            {
                return true;
            }

            // Check if this value appears more than once in this specific column index across the batch
            // Note: This does not check the Database itself, only the data we are about to insert.
            int count = batch.Count(r => r.Row.Count > columnIndex && r.Row[columnIndex] == value);

            if (count <= 1)
            {
                return true;
            }

            LogError($"{SqliteHelperResources.ErrorNotUnique} (Duplicate found in batch: {value})");
            return false;
        }

        /// <summary>
        ///     Logs the error.
        /// </summary>
        /// <param name="message">The message.</param>
        private void LogError(string message)
        {
            _message = new MessageItem { Message = message, Level = 0 };
            OnError(_message);
        }

        /// <summary>
        ///     Inform Subscribers about the News
        /// </summary>
        /// <param name="dbMessage">Message</param>
        private void OnError(MessageItem dbMessage)
        {
            _setMessage?.Invoke(this, dbMessage);
        }
    }
}
