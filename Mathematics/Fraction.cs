/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Fraction.cs
 * PURPOSE:     Helper class that helps with some basic Fraction Operations
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://dotnet-snippets.de/snippet/klasse-bruchrechnung-class-fraction/12049
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable NonReadonlyMemberInGetHashCode

using System;

namespace Mathematics
{
    /// <summary>
    ///     A lightweight, immutable Fraction (Rational Number) struct.
    ///     Internally stores values strictly as a reduced improper fraction (e.g., 3/2 instead of 1 1/2).
    ///     This prevents mathematical errors during multiplication and division.
    /// </summary>
    public readonly struct Fraction : IEquatable<Fraction>, IComparable<Fraction>
    {
        /// <summary>
        /// The top part of the fraction.
        /// </summary>
        /// <value>
        /// The numerator.
        /// </value>
        public int Numerator { get; }

        /// <summary>
        /// The bottom part of the fraction. Guaranteed to always be positive.
        /// </summary>
        /// <value>
        /// The denominator.
        /// </value>
        public int Denominator { get; }

        /// <summary>
        /// Calculates the Whole Number part of the fraction (e.g., the '2' in 2 1/3).
        /// Integer division naturally truncates the decimal, giving us exactly what we need.
        /// </summary>
        /// <value>
        /// The whole part.
        /// </value>
        public int WholePart => Numerator / Denominator;

        /// <summary>
        /// Calculates the remaining Numerator for a mixed number display (e.g., the '1' in 2 1/3).
        /// We use Math.Abs so the remainder doesn't display as a negative number if the whole fraction is negative.
        /// </summary>
        public int RemainderNumerator => Math.Abs(Numerator % Denominator);

        /// <summary>
        /// Initializes a standard fraction (e.g., 3 / 4) and automatically reduces it.
        /// </summary>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        /// <exception cref="System.DivideByZeroException">Denominator cannot be zero.</exception>
        public Fraction(int numerator, int denominator)
        {
            if (denominator == 0)
            {
                throw new DivideByZeroException("Denominator cannot be zero.");
            }

            // SIGN NORMALIZATION:
            // We always want the negative sign to stay on the Numerator.
            // If someone passes (1, -2) or (-1, -2), we flip the signs so the Denominator is always positive.
            if (denominator < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }

            // REDUCTION:
            // Find the Greatest Common Divisor (GCD). We use Math.Abs on the numerator 
            // so the negative sign doesn't break the Euclidean algorithm.
            int gcd = GetGcf(Math.Abs(numerator), denominator);

            // Apply the reduction and lock the values in.
            Numerator = numerator / gcd;
            Denominator = denominator / gcd;
        }

        /// <summary>
        /// Initializes a mixed number fraction (e.g., wholeNumber 2, num 1, den 3 = 2 1/3).
        /// This immediately converts it into an improper fraction (7/3) and passes it to the main constructor.
        /// </summary>
        /// <param name="wholeNumber">The whole number.</param>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        public Fraction(int wholeNumber, int numerator, int denominator)
            // Math trick: If the whole number is negative (-2 1/3), we must subtract the numerator, not add it.
            : this((wholeNumber * denominator) + (wholeNumber < 0 ? -numerator : numerator), denominator)
        {
        }

        /// <summary>
        /// Converts the fraction to a highly precise decimal.
        /// </summary>
        /// <returns>Dezimal Value.</returns>
        public decimal ToDecimal() => (decimal)Numerator / Denominator;

        /// <summary>
        /// Converts the fraction to a standard double.
        /// </summary>
        /// <returns>Double Value</returns>
        public double ToDouble() => (double)Numerator / Denominator;

        /// <summary>
        /// Implements the operator *.
        /// Multiplication: (a/b) * (c/d) = (a*c) / (b*d)
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Fraction operator *(Fraction a, Fraction b) =>
            new(a.Numerator * b.Numerator, a.Denominator * b.Denominator);

        /// <summary>
        /// Division: Multiply by the reciprocal. (a/b) / (c/d) = (a*d) / (b*c)
        /// Implements the operator /.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Fraction operator /(Fraction a, Fraction b) =>
            new(a.Numerator * b.Denominator, a.Denominator * b.Numerator);

        /// <summary>
        /// Addition: Find a common denominator by multiplying them, then cross-multiply the numerators.
        /// Implements the operator +.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Fraction operator +(Fraction a, Fraction b) =>
            new((a.Numerator * b.Denominator) + (b.Numerator * a.Denominator), a.Denominator * b.Denominator);

        /// <summary>
        /// Subtraction: Same as addition, just subtract the cross-multiplied numerators.
        /// Implements the operator -.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Fraction operator -(Fraction a, Fraction b) =>
            new((a.Numerator * b.Denominator) - (b.Numerator * a.Denominator), a.Denominator * b.Denominator);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// Because our constructor automatically reduces fractions (e.g., 2/4 becomes 1/2), 
        /// two equal fractions will ALWAYS have the exact same Numerator and Denominator.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(Fraction other) => Numerator == other.Numerator && Denominator == other.Denominator;

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is Fraction other && Equals(other);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Fraction a, Fraction b) => a.Equals(b);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Fraction a, Fraction b) => !a.Equals(b);

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings:
        /// <list type="table"><listheader><term> Value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="other" /> in the sort order.</description></item><item><term> Zero</term><description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="other" /> in the sort order.</description></item></list>
        /// </returns>
        public int CompareTo(Fraction other) => ToDecimal().CompareTo(other.ToDecimal());

        /// <summary>
        /// Implements the operator &lt;.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator <(Fraction a, Fraction b) => a.CompareTo(b) < 0;

        /// <summary>
        /// Implements the operator &gt;.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator >(Fraction a, Fraction b) => a.CompareTo(b) > 0;

        /// <summary>
        /// Implements the operator &lt;=.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator <=(Fraction a, Fraction b) => a.CompareTo(b) <= 0;

        /// <summary>
        /// Implements the operator &gt;=.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator >=(Fraction a, Fraction b) => a.CompareTo(b) >= 0;

        /// <summary>
        /// Performs an implicit conversion from <see cref="Fraction" /> to <see cref="System.Decimal" />.
        /// </summary>
        /// <param name="f">The f.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator decimal(Fraction f) => f.ToDecimal();

        /// <summary>
        /// Performs an implicit conversion from <see cref="Fraction"/> to <see cref="System.Double"/>.
        /// </summary>
        /// <param name="f">The f.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator double(Fraction f) => f.ToDouble();

        /// <summary>
        /// Formats the fraction nicely for UI display.
        /// </summary>
        public override string ToString()
        {
            // If it's a whole number (e.g., 5/1), just return "5"
            if (Denominator == 1) return Numerator.ToString();

            // If it has a whole part (e.g., 7/3), format as a mixed number: "2 1/3"
            int whole = WholePart;
            if (whole != 0)
            {
                return $"{whole} {RemainderNumerator}/{Denominator}";
            }

            // Otherwise, just return the standard fraction (e.g., "1/2")
            return $"{Numerator}/{Denominator}";
        }

        /// <summary>
        /// Gets the Greatest Common Factor (GCF) using the Euclidean algorithm.
        /// It repeatedly takes the remainder of division until it hits 0.
        /// </summary>
        private static int GetGcf(int a, int b)
        {

            while (b != 0)
            {
                int temp = b;
                b = a % b; // Get the remainder
                a = temp;  // Shift the values
            }
            return a;
        }
    }
}
