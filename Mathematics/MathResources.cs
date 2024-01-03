/*
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
    }
}
