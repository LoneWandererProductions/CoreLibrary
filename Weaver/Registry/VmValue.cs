/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Registry
 * FILE:        VmValue.cs
 * PURPOSE:     Struct that contains all possible VM value types for VariableRegistry.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Globalization;
using Weaver.Messages;

namespace Weaver.Registry
{
    /// <summary>
    /// Smallest unit that contains all possible VM value types for VariableRegistry.
    /// </summary>
    public readonly struct VmValue
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        public EnumTypes Type { get; }

        /// <summary>
        /// Gets the int64.
        /// </summary>
        public long Int64 { get; }

        /// <summary>
        /// Gets the double.
        /// </summary>
        public double Double { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="VmValue"/> is bool.
        /// </summary>
        public bool Bool { get; }

        /// <summary>
        /// Gets the string.
        /// </summary>
        public string? String { get; }

        /// <summary>
        /// Optional single attribute
        /// Gets the attribute.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        public string? Attribute { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VmValue" /> struct.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="i">The i.</param>
        /// <param name="d">The d.</param>
        /// <param name="b">if set to <c>true</c> [b].</param>
        /// <param name="s">The s.</param>
        /// <param name="attribute">The attribute.</param>
        public VmValue(EnumTypes type, long i, double d, bool b, string? s, string? attribute = null)
        {
            Type = type;
            Int64 = i;
            Double = d;
            Bool = b;
            String = s;
            Attribute = attribute;
        }

        /// <summary>
        /// Froms the int.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>New VmValue</returns>
        public static VmValue FromInt(long value) =>
            new(EnumTypes.Wint, value, default, default, null);

        /// <summary>
        /// Froms the double.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>New VmValue</returns>
        public static VmValue FromDouble(double value) =>
            new(EnumTypes.Wdouble, default, value, default, null);

        /// <summary>
        /// Froms the bool.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns>New VmValue</returns>
        public static VmValue FromBool(bool value) =>
            new(EnumTypes.Wbool, default, default, value, null);

        /// <summary>
        /// From string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>New VmValue</returns>
        public static VmValue FromString(string? value) =>
            new(EnumTypes.Wstring, default, default, default, value);

        /// <summary>
        /// Froms the int.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns>New VmValue</returns>
        public static VmValue FromInt(long value, string? attribute = null) =>
            new VmValue(EnumTypes.Wint, i: value, d: 0, b: false, s: null, attribute: attribute);

        /// <summary>
        /// Froms the double.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns>New VmValue</returns>
        public static VmValue FromDouble(double value, string? attribute = null) =>
            new VmValue(EnumTypes.Wdouble, i: 0, d: value, b: false, s: null, attribute: attribute);

        /// <summary>
        /// Froms the bool.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns>New VmValue</returns>
        public static VmValue FromBool(bool value, string? attribute = null) =>
            new VmValue(EnumTypes.Wbool, i: 0, d: 0, b: value, s: null, attribute: attribute);

        /// <summary>
        /// Froms the string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns>New VmValue</returns>
        public static VmValue FromString(string? value, string? attribute = null) =>
            new VmValue(EnumTypes.Wstring, i: 0, d: 0, b: false, s: value, attribute: attribute);

        /// <summary>
        /// Froms the pointer.
        /// </summary>
        /// <param name="pointerKey">The pointer key.</param>
        /// <returns>New VmValue</returns>
        public static VmValue FromPointer(string pointerKey) =>
            new VmValue(EnumTypes.Wpointer, i: 0, d: 0, b: false, s: pointerKey);

        /// <summary>
        /// Froms the object.
        /// </summary>
        /// <returns>New VmValue</returns>
        public static VmValue FromObject() =>
            new VmValue(EnumTypes.Wobject, i: 0, d: 0, b: false, s: null);

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var valueStr = Type switch
            {
                EnumTypes.Wint => Int64.ToString(),
                EnumTypes.Wdouble => Double.ToString("G17", CultureInfo.InvariantCulture),
                EnumTypes.Wbool => Bool.ToString(),
                EnumTypes.Wstring => String ?? "<null>",
                EnumTypes.Wpointer => String ?? "<null>",
                EnumTypes.Wobject => "<object>",
                _ => "<unknown>"
            };

            // Append attribute if present
            if (!string.IsNullOrEmpty(Attribute))
                valueStr += $" [Attribute: {Attribute}]";

            return $"({Type}): {valueStr}";
        }

        /// <summary>
        /// Deconstructs the VMValue into a raw object value and the EnumType.
        /// </summary>
        /// <param name="value">Outputs the boxed value based on Type.</param>
        /// <param name="type">Outputs the EnumTypes discriminator.</param>
        public void Deconstruct(out object? value, out EnumTypes type)
        {
            type = Type;

            value = Type switch
            {
                EnumTypes.Wint => Int64,
                EnumTypes.Wdouble => Double,
                EnumTypes.Wbool => Bool,
                EnumTypes.Wstring => String,
                _ => null
            };
        }
    }
}