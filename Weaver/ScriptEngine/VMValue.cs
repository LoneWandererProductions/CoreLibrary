/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        VMValue.cs
 * PURPOSE:     Struct that contains all possible VM value types for VariableRegistry.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Messages;

namespace Weaver.ScriptEngine
{
    /// <summary>
    /// Smallest unit that contains all possible VM value types for VariableRegistry.
    /// </summary>
    public readonly struct VMValue
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
        /// Gets a value indicating whether this <see cref="VMValue"/> is bool.
        /// </summary>
        public bool Bool { get; }

        /// <summary>
        /// Gets the string.
        /// </summary>
        public string? String { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VMValue"/> struct.
        /// </summary>
        private VMValue(EnumTypes type, long i, double d, bool b, string? s)
        {
            Type = type;
            Int64 = i;
            Double = d;
            Bool = b;
            String = s;
        }

        /// <summary>
        /// Froms the int.
        /// </summary>
        public static VMValue FromInt(long value) =>
            new(EnumTypes.Wint, value, default, default, null);

        /// <summary>
        /// Froms the double.
        /// </summary>
        public static VMValue FromDouble(double value) =>
            new(EnumTypes.Wdouble, default, value, default, null);

        /// <summary>
        /// Froms the bool.
        /// </summary>
        public static VMValue FromBool(bool value) =>
            new(EnumTypes.Wbool, default, default, value, null);

        /// <summary>
        /// Froms the string.
        /// </summary>
        public static VMValue FromString(string? value) =>
            new(EnumTypes.Wstring, default, default, default, value);

        /// <summary>
        /// Converts to string.
        /// </summary>
        public override string ToString()
        {
            return Type switch
            {
                EnumTypes.Wint => Int64.ToString(),
                EnumTypes.Wdouble => Double.ToString(),
                EnumTypes.Wbool => Bool.ToString(),
                EnumTypes.Wstring => String ?? "<null>",
                _ => "<unknown>"
            };
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
