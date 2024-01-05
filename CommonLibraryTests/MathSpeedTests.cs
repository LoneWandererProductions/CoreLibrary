using ExtendedSystemObjects;
using Mathematics;

namespace CommonLibraryTests
{
    internal static class MathSpeedTests
    {
        /// <summary>
        /// Tests one. Matrix Multiplication
        /// </summary>
        /// <param name="mOne">The m one.</param>
        /// <param name="mTwo">The m two.</param>
        /// <returns></returns>
        internal static BaseMatrix TestOne(BaseMatrix mOne, BaseMatrix mTwo)
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
}
