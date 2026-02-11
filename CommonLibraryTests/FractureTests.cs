/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        FractureTests.cs
 * PURPOSE:     Tests for the Fracture Class
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    ///     Tests for my fraction class.
    /// </summary>
    [TestClass]
    public class FractureTests
    {
        /// <summary>
        ///     Test basic fraction creation and simplification
        /// </summary>
        [TestMethod]
        public void Fractures()
        {
            var one = new Fraction(14, 2);
            Assert.AreEqual(7, one.Numerator, "Expected numerator to be 7 after simplification.");
            Assert.AreEqual(1, one.Denominator, "Expected denominator to be 1 after simplification.");
            Assert.AreEqual(0, one.Exponent, "Expected exponent to be 0.");
            Assert.AreEqual(7, one.ExponentNumerator, "Expected exponent numerator to be 7.");
            Assert.AreEqual(7, one.Decimal, "Expected decimal value to be 7.");

            one = new Fraction(14, 8);
            Assert.AreEqual(3, one.Numerator, "Expected numerator to be 3 after simplification.");
            Assert.AreEqual(4, one.Denominator, "Expected denominator to be 4.");
            Assert.AreEqual(1, one.Exponent, "Expected exponent to be 1.");
            Assert.AreEqual(7, one.ExponentNumerator, "Expected exponent numerator to be 7.");
            Assert.AreEqual(1.75m, one.Decimal, "Expected decimal value to be 1.75.");

            one = new Fraction(0, 1, 2);
            Assert.AreEqual(0, one.Numerator, "Expected numerator to be 0.");
            Assert.AreEqual(1, one.Denominator, "Expected denominator to be 1.");
            Assert.AreEqual(0, one.Exponent, "Expected exponent to be 0.");
            Assert.AreEqual(0, one.ExponentNumerator, "Expected exponent numerator to be 0.");
            Assert.AreEqual(0, one.Decimal, "Expected decimal value to be 0.");
        }

        /// <summary>
        ///     Test various fraction operations
        /// </summary>
        [TestMethod]
        public void FracturesOperations()
        {
            var one = new Fraction(1, 2, 2);
            Assert.AreEqual(5, one.ExponentNumerator, "Expected exponent numerator to be 5.");
            var two = new Fraction(1, 2, 2);
            Assert.AreEqual(5, two.ExponentNumerator, "Expected exponent numerator to be 5.");

            var result = one + two;
            Assert.AreEqual(5, result.Numerator, "Expected numerator to be 5.");
            Assert.AreEqual(1, result.Denominator, "Expected denominator to be 1.");
            Assert.AreEqual(0, result.Exponent, "Expected exponent to be 0.");
            Assert.AreEqual(5, result.ExponentNumerator, "Expected exponent numerator to be 5.");
            Assert.AreEqual(5, result.Decimal, "Expected decimal value to be 5.");

            result = one - two;
            Assert.AreEqual(0, result.Numerator, "Expected numerator to be 0.");
            Assert.AreEqual(4, result.Denominator, "Expected denominator to be 4.");
            Assert.AreEqual(0, result.Exponent, "Expected exponent to be 0.");
            Assert.AreEqual(0, result.ExponentNumerator, "Expected exponent numerator to be 0.");
            Assert.AreEqual(0, result.Decimal, "Expected decimal value to be 0.");

            one = new Fraction(4, 2);
            Assert.AreEqual(2, one.ExponentNumerator, "Expected exponent numerator to be 2.");
            Assert.AreEqual(2, one.Numerator, "Expected numerator to be 2.");
            Assert.AreEqual(1, one.Denominator, "Expected denominator to be 1.");

            two = new Fraction(1, 1, 4);
            Assert.AreEqual(4, two.ExponentNumerator, "Expected exponent numerator to be 4.");

            result = one * two;
            Assert.AreEqual(8, result.Numerator, "Expected numerator to be 8.");
            Assert.AreEqual(1, result.Denominator, "Expected denominator to be 1.");
            Assert.AreEqual(0, result.Exponent, "Expected exponent to be 0.");
            Assert.AreEqual(8, result.ExponentNumerator, "Expected exponent numerator to be 8.");
            Assert.AreEqual(8, result.Decimal, "Expected decimal value to be 8.");

            result = one / two;
            Assert.AreEqual(1, result.Numerator, "Expected numerator to be 1.");
            Assert.AreEqual(2, result.Denominator, "Expected denominator to be 2.");
            Assert.AreEqual(0, result.Exponent, "Expected exponent to be 0.");
            Assert.AreEqual(1, result.ExponentNumerator, "Expected exponent numerator to be 1.");
            Assert.AreEqual(0.5m, result.Decimal, "Expected decimal value to be 0.5.");

            // Edge case tests
            one = new Fraction(2, 2);
            two = new Fraction(1, 2, 1);

            result = one + two;
            Assert.AreEqual(1, result.Numerator, "Expected numerator to be 1.");
            Assert.AreEqual(2, result.Denominator, "Expected denominator to be 2.");
            Assert.AreEqual(2, result.Exponent, "Expected exponent to be 2.");
            Assert.AreEqual(5, result.ExponentNumerator, "Expected exponent numerator to be 5.");
            Assert.AreEqual(2.5m, result.Decimal, "Expected decimal value to be 2.5.");

            // Negative handling
            one = new Fraction(-1, 2);
            two = new Fraction(1, 2, -1);
            var three = new Fraction(1, 4, -1);

            result = one * two;
            Assert.AreEqual(3, result.Numerator, "Expected numerator to be 3.");
            Assert.AreEqual(4, result.Denominator, "Expected denominator to be 4.");
            Assert.AreEqual(0, result.Exponent, "Expected exponent to be 0.");
            Assert.AreEqual(3, result.ExponentNumerator, "Expected exponent numerator to be 3.");
            Assert.AreEqual(0.75m, result.Decimal, "Expected decimal value to be 0.75.");

            result = one * three;
            Assert.AreEqual(5, result.Numerator, "Expected numerator to be 5.");
            Assert.AreEqual(8, result.Denominator, "Expected denominator to be 8.");
            Assert.AreEqual(0, result.Exponent, "Expected exponent to be 0.");
            Assert.AreEqual(5, result.ExponentNumerator, "Expected exponent numerator to be 5.");
            Assert.AreEqual(0.625m, result.Decimal, "Expected decimal value to be 0.625.");
        }

        /// <summary>
        ///     Test fraction simplification
        /// </summary>
        [TestMethod]
        public void FractionSimplification()
        {
            var frac = new Fraction(10, 20);
            Assert.AreEqual(1, frac.Numerator, "Expected simplified numerator to be 1.");
            Assert.AreEqual(2, frac.Denominator, "Expected simplified denominator to be 2.");
        }

        /// <summary>
        ///     Test handling of negative fractions
        /// </summary>
        [TestMethod]
        public void FractionNegativeHandling()
        {
            var frac = new Fraction(-3, 9);
            Assert.AreEqual(-1, frac.Numerator, "Expected numerator to be -1.");
            Assert.AreEqual(3, frac.Denominator, "Expected denominator to be 3.");

            var frac2 = new Fraction(3, -9);
            Assert.AreEqual(-1, frac2.Numerator, "Expected numerator to be -1.");
            Assert.AreEqual(3, frac2.Denominator, "Expected denominator to be 3.");
        }

        /// <summary>
        ///     Test multiplication of fractions
        /// </summary>
        [TestMethod]
        public void FractionMultiplication()
        {
            var frac1 = new Fraction(1, 2);
            var frac2 = new Fraction(2, 3);
            var result = frac1 * frac2;

            Assert.AreEqual(1, result.Numerator, "Expected numerator to be 1.");
            Assert.AreEqual(3, result.Denominator, "Expected denominator to be 3.");
        }

        /// <summary>
        ///     Test handling of zero denominator (should throw exception)
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(DivideByZeroException))]
        public void FractionDivideByZero()
        {
            var frac = new Fraction(1, 0);
        }

        /// <summary>
        ///     Test for large numbers
        /// </summary>
        [TestMethod]
        public void FractionLargeNumbers()
        {
            var frac = new Fraction(1000000, 2000000);
            Assert.AreEqual(1, frac.Numerator, "Expected numerator to be 1.");
            Assert.AreEqual(2, frac.Denominator, "Expected denominator to be 2.");
        }
    }
}
