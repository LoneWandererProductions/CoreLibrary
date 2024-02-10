﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/MathResources.cs
 * PURPOSE:     Some basic string Resources
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */


namespace Mathematics
{
    internal static class MathResources
    {
        /// <summary>
        ///     The Tolerance (const). Value: 0.000000001.
        /// </summary>
        internal const double Tolerance = 0.000000001;

        /// <summary>
        ///     The matrix error inverse (const). Value: "Unable to compute inverse.".
        /// </summary>
        internal const string MatrixErrorInverse = "Unable to compute inverse.";

        /// <summary>
        ///     The matrix error Determinant (const). Value: "Unable to compute Matrix Determinant.".
        /// </summary>
        internal const string MatrixErrorDeterminant = "Unable to compute Matrix Determinant.";

        /// <summary>
        ///     The matrix error Decompose (const). Value: "Attempt to decompose a non-square m.".
        /// </summary>
        internal const string MatrixErrorDecompose = "Attempt to decompose a non-square m.";

        /// <summary>
        ///     The matrix error Doolittle (const). Value: "Cannot use Doolittle's method.".
        /// </summary>
        internal const string MatrixErrorDoolittle = "Cannot use Doolittle's method.";

        /// <summary>
        ///     The matrix error Doolittle (const). Value: "Number of Columns of first are not equal to the number of rows in the
        ///     second Matrix.".
        /// </summary>
        internal const string MatrixErrorColumns =
            "Number of Columns of first are not equal to the number of rows in the second Matrix.";

        /// <summary>
        ///     The string X (const). Value: "X: ".
        /// </summary>
        internal const string StrX = "X: ";

        /// <summary>
        ///     The string Y (const). Value: " Y: ".
        /// </summary>
        internal const string StrY = " Y: ";

        /// <summary>
        ///     The string Z (const). Value: " Z: ".
        /// </summary>
        internal const string StrZ = " Z: ";

        /// <summary>
        ///     The string 1 (const). Value: "1: ".
        /// </summary>
        internal const string StrOne = "1: ";

        /// <summary>
        ///     The string 2 (const). Value: " 2: ".
        /// </summary>
        internal const string StrTwo = " 2: ";

        /// <summary>
        ///     The string 3 (const). Value: " 3: ".
        /// </summary>
        internal const string StrThree = " 3: ";

        /// <summary>
        ///     The string Id (const). Value: " Id: ".
        /// </summary>
        internal const string StrId = " Id: ";
    }
}
