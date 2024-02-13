/*
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

        internal static double[,] Inverse(double[,] m)
        {
            // inverse using QR decomp (Householder algorithm)
            double[,] q;
            double[,] r;
            MatDecomposeQr(m, out q, out r);

            // TODO: check if determinant is zero (no inverse)
            // double absDet = 1.0;
            // for (int i = 0; i < M.Length; ++i)
            //   absDet *= R[i,i];

            var rinv = MatInverseUpperTri(r); // std algo
            var qtrans = MatTranspose(q); // is inv(Q)
            return MatProduct(rinv, qtrans);

            // ----------------------------------------------------
            // helpers: MatDecomposeQR, MatMake, MatTranspose,
            // MatInverseUpperTri, MatIdentity, MatCopy, VecNorm,
            // VecDot, VecToMat, MatProduct
            // ----------------------------------------------------

            static void MatDecomposeQr(double[,] mat,
                out double[,] unitaryQ, out double[,] triangularR)
            {
                // QR decomposition, Householder algorithm.
                // https://rosettacode.org/wiki/QR_decomposition
                var row = mat.GetLength(0); // assumes mat is nxn
                var column = mat.GetLength(1);// check m == n

                var q = MatIdentity(row);
                var r = mat.Duplicate();
                var end = column - 1;
                // if (m == n) end = n - 1; else end = n;

                for (var i = 0; i < end; ++i)
                {
                    var h = MatIdentity(row);
                    var a = new double[column - i];
                    var k = 0;
                    for (var ii = i; ii < column; ++ii)
                    {
                        a[k++] = r[ii, i];
                    }

                    var normA = VecNorm(a);
                    if (a[0] < 0.0) { normA = -normA; }

                    var v = new double[a.Length];
                    for (var j = 0; j < v.Length; ++j)
                    {
                        v[j] = a[j] / (a[0] + normA);
                    }

                    v[0] = 1.0;

                    var identityH = MatIdentity(a.Length);
                    var vvDot = VecDot(v, v);
                    var alpha = VecToMat(v, v.Length, 1);
                    var beta = VecToMat(v, 1, v.Length);
                    var aMultB = MatProduct(alpha, beta);

                    for (var ii = 0; ii < identityH.GetLength(0); ++ii)
                        for (var jj = 0; jj < identityH.GetLength(1); ++jj)
                        {
                            identityH[ii, jj] -= (2.0 / vvDot) * aMultB[ii, jj];
                        }

                    // copy h into lower right of H
                    var d = column - identityH.GetLength(0);
                    for (var ii = 0; ii < identityH.GetLength(0); ++ii)
                        for (var jj = 0; jj < identityH.GetLength(1); ++jj)
                        {
                            h[ii + d, jj + d] = identityH[ii, jj];
                        }

                    q = MatProduct(q, h);
                    r = MatProduct(h, r);
                } // i

                unitaryQ = q;
                triangularR = r;
            } // QR decomposition 

            static double[,] MatInverseUpperTri(double[,] u)
            {
                var n = u.GetLength(0); // must be square matrix

                var result = MatIdentity(n);

                for (var k = 0; k < n; ++k)
                {
                    for (var j = 0; j < n; ++j)
                    {
                        for (var i = 0; i < k; ++i)
                        {
                            result[j, k] -= result[j, i] * u[i, k];
                        }

                        result[j, k] /= u[k, k];
                    }
                }

                return result;
            }

            static double[,] MatTranspose(double[,] m)
            {
                var nr = m.GetLength(0);
                var nc = m.GetLength(1);
                var result = MatMake(nc, nr); // note
                for (var i = 0; i < nr; ++i)
                    for (var j = 0; j < nc; ++j)
                    {
                        result[j, i] = m[i, j];
                    }

                return result;
            }

            static double[,] MatMake(int nRows, int nCols)
            {
                return new double[nRows, nCols];
            }

            static double[,] MatIdentity(int n)
            {
                var result = MatMake(n, n);
                for (var i = 0; i < n; ++i)
                {
                    result[i, i] = 1.0;
                }

                return result;
            }


            static double[,] MatProduct(double[,] matA,
                double[,] matB)
            {
                var aRows = matA.GetLength(0);
                var aCols = matA.GetLength(1);
                var bRows = matB.GetLength(0);
                var bCols = matB.GetLength(1);
                if (aCols != bRows)
                {
                    throw new Exception("Non-conformable matrices");
                }

                var result = MatMake(aRows, bCols);

                for (var i = 0; i < aRows; ++i) // each row of A
                    for (var j = 0; j < bCols; ++j) // each col of B
                        for (var k = 0; k < aCols; ++k)
                        {
                            result[i, j] += matA[i, k] * matB[k, j];
                        }

                return result;
            }

            static double VecDot(double[] v1, double[] v2)
            {
                var result = 0.0;
                var n = v1.Length;
                for (var i = 0; i < n; ++i)
                {
                    result += v1[i] * v2[i];
                }

                return result;
            }

            static double VecNorm(double[] vec)
            {
                var n = vec.Length;
                var sum = 0.0;
                for (var i = 0; i < n; ++i)
                {
                    sum += vec[i] * vec[i];
                }

                return Math.Sqrt(sum);
            }

            static double[,] VecToMat(double[] vec,
                int nRows, int nCols)
            {
                var result = MatMake(nRows, nCols);
                var k = 0;
                for (var i = 0; i < nRows; ++i)
                    for (var j = 0; j < nCols; ++j)
                    {
                        result[i, j] = vec[k++];
                    }

                return result;
            }
        } // MatInverseQR

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
    }
}
