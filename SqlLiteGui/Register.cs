/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/Register.cs
 * PURPOSE:     A basic Selection Object to clean up the Controls
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Data;

namespace SQLiteGui
{
    /// <summary>
    ///     Slightly adapted Factory Pattern
    /// </summary>
    internal static class Register
    {
        /// <summary>
        ///     Current DB we are using
        /// </summary>
        internal static string ActiveDb { get; set; }

        /// <summary>
        ///     Gets a value indicating whether Element is Selected in Detail view
        /// </summary>
        internal static bool IsDetailactive { get; private set; }

        /// <summary>
        ///     Name of the Last selected Table
        /// </summary>
        internal static string Tablealias { get; private set; }

        /// <summary>
        ///     Table Details
        /// </summary>
        private static dynamic TblItem { get; set; }

        /// <summary>
        ///     Provide a Unique Index if we have one
        /// </summary>
        internal static string PrimaryKey { get; private set; }

        /// <summary>
        ///     Get Item by Column Header
        /// </summary>
        /// <param name="item">Column Header Name</param>
        /// <returns>Selected Value</returns>
        public static string PrimaryKeyItem(string item)
        {
            if (TblItem == null)
            {
                return string.Empty;
            }

            var row = (DataRowView)TblItem;
            return !row.Row.Table.Columns.Contains(item) ? string.Empty : row[PrimaryKey].ToString();
        }

        /// <summary>
        ///     Reset Register
        /// </summary>
        internal static void StartNew()
        {
            Tablealias = string.Empty;
            IsDetailactive = false;
            TblItem = null;
            PrimaryKey = string.Empty;
        }

        /// <summary>
        ///     Table Selected
        /// </summary>
        /// <param name="tablealias">Name of the Table</param>
        /// <param name="uniqueIndex">Unique Index if it exists, else empty String</param>
        internal static void SelectedTable(string tablealias, string uniqueIndex)
        {
            PrimaryKey = uniqueIndex;
            Tablealias = tablealias;
        }

        /// <summary>
        ///     Get selected Row
        /// </summary>
        /// <param name="isDetailactive">Is an Row selected</param>
        /// <param name="tbi">Selected Row</param>
        internal static void SelectionChanged(bool isDetailactive, dynamic tbi)
        {
            IsDetailactive = isDetailactive;
            TblItem = tbi;
        }
    }
}
