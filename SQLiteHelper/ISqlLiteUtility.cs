/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteHelper
 * FILE:        SQLiteHelper/ISqlLiteUtility.cs
 * PURPOSE:     Interface Tools for SqlLite
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;

// ReSharper disable UnusedMemberInSuper.Global

namespace SQLiteHelper
{
    /// <summary>
    ///     The ISqlLiteUtility interface.
    /// </summary>
    internal interface ISqlLiteUtility
    {
        /// <summary>
        ///     Convert the object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The <see cref="DictionaryTableColumns" />.</returns>
        DictionaryTableColumns ConvertObject(object obj);

        /// <summary>
        ///     Convert the to attribute.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///     The <see cref="T:List{string}" />, can return null.
        /// </returns>
        List<string> ConvertToAttribute(object obj);

        /// <summary>
        ///     Converts to table set.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///     The TableSet, can return null.
        /// </returns>
        TableSet ConvertToTableSet(object obj);

        /// <summary>
        ///     Fill the object.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="object" />.</returns>
        object FillObject(List<string> row, object obj);
    }
}
