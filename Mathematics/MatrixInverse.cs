﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/MatrixInverse.cs
 * PURPOSE:     Helper class that does some basic Matrix operations
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://bratched.com/en/?s=matrix
 *              https://jamesmccaffrey.wordpress.com/2015/03/06/inverting-a-matrix-using-c/
 */

using System;
using ExtendedSystemObjects;

namespace Mathematics
{
    internal static class MatrixInverse
    {
        /// <summary>
        ///     Calculate the Inverse Matrix
        ///     Source:
        ///     https://jamesmccaffrey.wordpress.com/2015/03/06/inverting-a-matrix-using-c/
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <returns>Inverse Matrix</returns>
        /// <exception cref="ArithmeticException">Unable to compute inverse</exception>
        internal static double[,] Inverse(double[,] matrix)
        {
            var n = matrix.GetLength(0);

            var result = matrix.Duplicate();

            var lum = MatrixDecompose(matrix, out var perm, out _);

            if (lum == null)
            {
                throw new ArithmeticException(MathResources.MatrixErrorInverse);
            }

            //var b = new double[n];
            //constant vector
            var b = new double[n];

            for (var i = 0; i < n; ++i)
            {
                for (var j = 0; j < n; ++j)
                {
                    if (i == perm[j])
                    {
                        b[j] = 1.0;
                    }
                    else
                    {
                        b[j] = 0.0;
                    }
                }

                var x = HelperSolve(lum, b); // 

                for (var j = 0; j < n; ++j)
                {
                    result[j, i] = x[j];
                }
            }

            return result;
        }

        /// <summary>
        ///     Calculate the determinant of the matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <returns>Determinant of the matrix</returns>
        /// <exception cref="ArithmeticException">Unable to compute MatrixDeterminant</exception>
        internal static double MatrixDeterminant(double[,] matrix)
        {
            var lum = MatrixDecompose(matrix, out _, out var toggle);

            if (lum == null)
            {
                throw new ArithmeticException(MathResources.MatrixErrorDeterminant);
            }

            double result = toggle;

            for (var i = 0; i < lum.GetLength(1); ++i)
            {
                result *= lum[i, i];
            }

            return result;
        }

        /// <summary>
        ///     Decompose the Matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="perm">The perm.</param>
        /// <param name="toggle">The toggle.</param>
        /// <returns>Decomposed Matrix</returns>
        /// <exception cref="ArithmeticException">
        ///     Attempt to decompose a non-square m
        ///     or
        ///     Cannot use Doolittle's method
        /// </exception>
        private static double[,] MatrixDecompose(double[,] matrix, out int[] perm, out int toggle)
        {
            // Doolittle LUP decomposition with partial pivoting.
            // returns: result is L (with 1s on diagonal) and U;
            // perm holds row permutations; toggle is +1 or -1 (even or odd)
            var rows = matrix.GetLength(0);
            var cols = matrix.GetLength(1); // assume square, Column

            if (rows != cols)
            {
                throw new ArithmeticException(MathResources.MatrixErrorDecompose);
            }

            var n = rows; // convenience

            var result = matrix.Duplicate(); // MatrixDuplicate(matrix);

            perm = new int[n]; // set up row permutation result
            for (var i = 0; i < n; ++i)
            {
                perm[i] = i;
            }

            toggle = 1; // toggle tracks row swaps.
            // +1 -greater-than even, -1 -greater-than odd. used by MatrixDeterminant

            for (var j = 0; j < n - 1; ++j) // each column
            {
                var colMax = Math.Abs(result[j, j]); // find largest val in col
                var pRow = j;

                // reader Matt V needed this:
                for (var i = j + 1; i < n; ++i)
                {
                    if (!(Math.Abs(result[i, j]) > colMax))
                    {
                        continue;
                    }

                    colMax = Math.Abs(result[i, j]);
                    pRow = i;
                }
                // Not sure if this approach is needed always, or not.

                if (pRow != j) // if largest value not on pivot, swap rows
                {
                    result.SwapRow(pRow, j);

                    (perm[pRow], perm[j]) = (perm[j], perm[pRow]);

                    toggle = -toggle; // adjust the row-swap toggle
                }

                // --------------------------------------------------
                // This part added later (not in original)
                // and replaces the 'return null' below.
                // if there is a 0 on the diagonal, find a good row
                // from i = j+1 down that doesn't have
                // a 0 in column j, and swap that good row with row j
                // --------------------------------------------------

                if (result[j, j] == 0.0)
                {
                    // find a good row to swap
                    var goodRow = -1;

                    for (var row = j + 1; row < n; ++row)
                    {
                        if (result[row, j] != 0.0)
                        {
                            goodRow = row;
                        }
                    }

                    if (goodRow == -1)
                    {
                        throw new ArithmeticException(MathResources.MatrixErrorDoolittle);
                    }

                    // swap rows so 0.0 no longer on diagonal
                    result.SwapRow(goodRow, j);

                    (perm[goodRow], perm[j]) = (perm[j], perm[goodRow]);

                    toggle = -toggle; // adjust the row-swap toggle
                }
                // --------------------------------------------------
                // if diagonal after swap is zero . .
                //if (Math.Abs(result[j,j]) < 1.0E-20)
                //  return null; // consider a throw

                for (var i = j + 1; i < n; ++i)
                {
                    result[i, j] /= result[j, j];

                    for (var k = j + 1; k < n; ++k)
                    {
                        result[i, k] -= result[i, j] * result[j, k];
                    }
                }
            } // main j column loop

            return result;
        }

        /// <summary>
        ///     Helper solve.
        /// </summary>
        /// <param name="luMatrix">The lu matrix.</param>
        /// <param name="b">The b.</param>
        /// <returns>Changed matrix</returns>
        private static double[] HelperSolve(double[,] luMatrix, double[] b)
        {
            // before calling this helper, permute b using the perm array
            // from MatrixDecompose that generated luMatrix
            //      Columns
            var n = luMatrix.GetLength(0);
            var x = new double[n];
            b.CopyTo(x, 0);

            for (var i = 1; i < n; ++i)
            {
                var sum = x[i];
                for (var j = 0; j < i; ++j)
                {
                    sum -= luMatrix[i, j] * x[j];
                }

                x[i] = sum;
            }

            x[n - 1] /= luMatrix[n - 1, n - 1];
            for (var i = n - 2; i >= 0; --i)
            {
                var sum = x[i];

                for (var j = i + 1; j < n; ++j)
                {
                    sum -= luMatrix[i, j] * x[j];
                    x[i] = sum / luMatrix[i, i];
                }
            }

            return x;
        }
    }
}
