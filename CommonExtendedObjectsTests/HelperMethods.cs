/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        CommonExtendedObjectsTests/HelperMethods.cs
 * PURPOSE:     Helper Methods
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using ExtendedSystemObjects;
using Mathematics;

namespace CommonExtendedObjectsTests;

/// <summary>
///     The helper methods class.
/// </summary>
internal static class HelperMethods
{
    /// <summary>
    ///     Tests one. Matrix Multiplication
    /// </summary>
    /// <param name="mOne">The m one.</param>
    /// <param name="mTwo">The m two.</param>
    /// <returns>Matrix Multiplications</returns>
    internal static BaseMatrix MatrixTestOne(BaseMatrix mOne, BaseMatrix mTwo)
    {
        var h = mOne.Height;
        var w = mTwo.Width;
        var l = mOne.Width;
        var result = new BaseMatrix(h, w);
        var spanOne = mOne.Matrix.ToSpan();
        var spanTwo = mTwo.Matrix.ToSpan();
        var spanResult = result.Matrix.ToSpan();

        for (var i = 0; i < h; i++)
        {
            var iOne = i * l;

            for (var j = 0; j < w; j++)
            {
                var iTwo = j;

                double res = 0;

                for (var k = 0; k < l; k++, iTwo += w)
                {
                    res += spanOne[iOne + k] * spanTwo[iTwo];
                }

                spanResult[(i * w) + j] = res;
            }
        }

        return result;
    }
}
