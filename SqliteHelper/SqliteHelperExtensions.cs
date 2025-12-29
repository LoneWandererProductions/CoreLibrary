/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/SqliteHelperExtensions.cs
 * PURPOSE:     Extensions of SqliteHelper
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace SqliteHelper
{
    /// <summary>
    ///     Useful Extensions, to be extended
    /// </summary>
    internal static class SqliteHelperExtensions
    {
        /// <summary>
        ///     Withes the type of the data.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <returns>Conversion of DataTypes</returns>
        internal static TableColumns WithDataType(this TableColumns column, SqLiteDataTypes dataType)
        {
            column.DataType = dataType;
            return column;
        }
    }
}
