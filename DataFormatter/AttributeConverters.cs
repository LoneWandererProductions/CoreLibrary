/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     DataFormatter
 * FILE:        DataFormatter/AttributeConverter.cs
 * PURPOSE:     Helper for csv, Interface and implementation of AttributeConverter.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace DataFormatter
{
    /// <summary>
    ///     Interface for AttributeConverter Converter
    /// </summary>
    internal interface IAttributeConverter
    {
        /// <summary>
        ///     Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Generic return Type.</returns>
        object Convert(string value);
    }

    /// <inheritdoc />
    /// <summary>
    ///     Implementation of AttributeConverter for int
    /// </summary>
    /// <seealso cref="T:DataFormatter.IAttributeConverter" />
    public sealed class IntAttributeConverter : IAttributeConverter
    {
        /// <inheritdoc />
        /// <summary>
        ///     Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Converted to int</returns>
        public object Convert(string value)
        {
            return int.Parse(value);
        }
    }

    /// <inheritdoc />
    /// <summary>
    ///     Implementation of AttributeConverter for string
    /// </summary>
    /// <seealso cref="T:DataFormatter.IAttributeConverter" />
    public sealed class StringAttributeConverter : IAttributeConverter
    {
        /// <inheritdoc />
        /// <summary>
        ///     Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Converted to string</returns>
        public object Convert(string value)
        {
            return value;
        }
    }
}
