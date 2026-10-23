/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrary.Tests
 * FILE:        VectorTests.cs
 * PURPOSE:     2D and 3d Vector Tests
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Mathematics;

namespace CommonLibrary.Tests
{
    /// <summary>
    ///     Generic Vector Tests
    /// </summary>
    [TestClass]
    public class VectorTests
    {
        /// <summary>
        ///     Vector3D Test.
        /// </summary>
        [TestMethod]
        public void Vector3DTest()
        {
            //dot product
            var one = new Vector3D(8, 4, 5);
            var two = new Vector3D(1, 2, 4);
            var scalar = one * two; //36
            Assert.AreEqual(36, scalar, "Dot Product");

            //scalar product
            var vector = one * 3;
            Assert.AreEqual(24, vector.X, "X scalar Product");
            Assert.AreEqual(12, vector.Y, "Y scalar Product");
            Assert.AreEqual(15, vector.Z, "Z scalar Product");

            vector = 3 * one;
            Assert.AreEqual(24, vector.X, "X scalar Product");
            Assert.AreEqual(12, vector.Y, "Y scalar Product");
            Assert.AreEqual(15, vector.Z, "Z scalar Product");

            //scalar division
            vector = two / 2;
            Assert.AreEqual(0.5, vector.X, "X scalar Division");
            Assert.AreEqual(1, vector.Y, "Y scalar Division");
            Assert.AreEqual(2, vector.Z, "Z scalar Division");

            //magnitude (vector length)
            scalar = one.VectorLength(); //10.247
            Assert.AreEqual(10.247, Math.Round(scalar, 3), "Vector length");

            //Vector Addition
            vector = one + two;
            Assert.AreEqual(9, vector.X, "X Addition");
            Assert.AreEqual(6, vector.Y, "Y Addition");
            Assert.AreEqual(9, vector.Z, "Z Addition");

            //Vector subtraction
            vector = one - two;
            Assert.AreEqual(7, vector.X, "X Subtraction");
            Assert.AreEqual(2, vector.Y, "Y Subtraction");
            Assert.AreEqual(1, vector.Z, "Z Subtraction");

            //Vector Cross Product
            vector = one.CrossProduct(two);
            Assert.AreEqual(6, vector.X, "X Cross Product");
            Assert.AreEqual(-27, vector.Y, "Y Cross Product");
            Assert.AreEqual(12, vector.Z, "Z Cross Product");

            //negation
            vector = -one;
            Assert.AreEqual(-8, vector.X, "X Negation");
            Assert.AreEqual(-4, vector.Y, "Y Negation");
            Assert.AreEqual(-5, vector.Z, "Z Negation");

            //Angle between Vector
            scalar = one.Angle(two); //39.946
            scalar = scalar * 180 / Math.PI;
            Assert.AreEqual(39.946, Math.Round(scalar, 3), "Vector Angle");

            //normalize Vector, Unit Vector
            vector = one.Normalize();
            Assert.AreEqual(0.78072, Math.Round(vector.X, 5), "X Unit Vector");
            Assert.AreEqual(0.39036, Math.Round(vector.Y, 5), "Y Unit Vector");
            Assert.AreEqual(0.48795, Math.Round(vector.Z, 5), "Z Unit Vector");

            //inequality
            Assert.IsTrue(one != two, "Inequality");
            //inequality
            // ReSharper disable once EqualExpressionComparison
            Assert.IsTrue(one == one, "Equality, first");
            Assert.IsTrue(one.Equals(one), "Equality, second");

            var nullVector = Vector3D.ZeroVector;
            Assert.AreEqual(0, nullVector.X, "X Addition");
            Assert.AreEqual(0, nullVector.Y, "Y Addition");
            Assert.AreEqual(0, nullVector.Z, "Z Addition");

            var unitVector = Vector3D.UnitVector;
            Assert.AreEqual(1, unitVector.X, "X Addition");
            Assert.AreEqual(1, unitVector.Y, "Y Addition");
            Assert.AreEqual(1, unitVector.Z, "Z Addition");
        }

        /// <summary>
        ///     Vector2D Test.
        /// </summary>
        [TestMethod]
        public void Vector2DTest()
        {
            //dot product
            var one = new Vector2D(8, 4);
            var two = new Vector2D(1, 2);
            var scalar = one * two;
            Assert.AreEqual(16, scalar, "Dot Product");

            //scalar multiplication
            var vector = two * 2;
            Assert.AreEqual(2, vector.X, "X scalar Multiplication.");
            Assert.AreEqual(4, vector.Y, "Y scalar Multiplication.");

            //scalar multiplication
            vector = 2 * two;
            Assert.AreEqual(2, vector.X, "X scalar Multiplication.");
            Assert.AreEqual(4, vector.Y, "Y scalar Multiplication.");

            //scalar division
            vector = one / 2;
            Assert.AreEqual(4, vector.X, "X scalar Division.");
            Assert.AreEqual(2, vector.Y, "Y scalar Division.");

            //negation
            vector = -one;
            Assert.AreEqual(-8, vector.X, "X Negation");
            Assert.AreEqual(-4, vector.Y, "Y Negation");

            //Vector Addition
            vector = one + two;
            Assert.AreEqual(9, vector.X, "X Addition");
            Assert.AreEqual(6, vector.Y, "Y Addition");

            //normalize Vector, Unit Vector
            vector = one.Normalize();
            Assert.AreEqual(0, Math.Round(vector.X, 5), 89443, "X Unit Vector");
            Assert.AreEqual(0, Math.Round(vector.Y, 5), 44721, "Y Unit Vector");

            //Angle between Vector
            scalar = one.Angle(two);
            scalar = scalar * 180 / Math.PI;
            Assert.AreEqual(36.87, Math.Round(scalar, 3), "Vector Angle");

            //inequality
            Assert.IsTrue(one != two, "Inequality");
            //inequality
            // ReSharper disable once EqualExpressionComparison
#pragma warning disable CS1718 // Vergleich erfolgte mit derselben Variable
            Assert.IsTrue(one == one, "Equality, first");
#pragma warning restore CS1718 // Vergleich erfolgte mit derselben Variable
            Assert.IsTrue(one.Equals(one), "Equality, second");
        }

        /// <summary>
        ///     Vector transformations.
        /// </summary>
        [TestMethod]
        public void VectorTransformations()
        {
            var vector = new Vector3D(1, 1, 1);

            //scale Test
            var result = Projection3D.Scale(vector, 2);
            var v1 = (Vector3D)result;

            Assert.IsTrue(v1.X.Equals(2), "X");
            Assert.IsTrue(v1.Y.Equals(2), "Y");
            Assert.IsTrue(v1.Z.Equals(2), "Z");

            result = Projection3D.Scale(vector, 2, 2, 2);
            v1 = (Vector3D)result;

            Assert.IsTrue(v1.X.Equals(2), "X");
            Assert.IsTrue(v1.Y.Equals(2), "Y");
            Assert.IsTrue(v1.Z.Equals(2), "Z");

            vector = new Vector3D(2, 4, 5);

            result = Projection3D.Scale(vector, 4, 2.5, 6);
            v1 = (Vector3D)result;

            Assert.IsTrue(v1.X.Equals(8), "X");
            Assert.IsTrue(v1.Y.Equals(10), "Y");
            Assert.IsTrue(v1.Z.Equals(30), "Z");

            //translate
            vector = new Vector3D(1, 1, 1);
            var target = new Vector3D(2, 2, 2);

            result = Projection3D.Translate(vector, target);
            v1 = (Vector3D)result;

            Assert.IsTrue(v1.X.Equals(3), "X");
            Assert.IsTrue(v1.Y.Equals(3), "Y");
            Assert.IsTrue(v1.Z.Equals(3), "Z");

            vector = new Vector3D(1, 0, 0);

            //https://en.wikipedia.org/wiki/Rotation_matrix
            result = Projection3D.RotateZ(vector, 90);
            v1 = (Vector3D)result;

            var inputValue = Math.Round(v1.X, 2);
            Assert.AreEqual(0, inputValue, "X");
            Assert.IsTrue(v1.Y.Equals(1), "Y");
            Assert.IsTrue(v1.Z.Equals(0), "Z");

            vector = new Vector3D(1, 1, 1);
            result = Projection3D.RotateX(vector, 90);
            v1 = (Vector3D)result;

            inputValue = Math.Round(v1.Y, 2);

            Assert.IsTrue(v1.X.Equals(1), "X");
            Assert.AreEqual(-1, inputValue, "Y");
            Assert.IsTrue(v1.Z.Equals(1), "Z");

            vector = new Vector3D(1, 1, 1);
            result = Projection3D.RotateY(vector, 90);
            v1 = (Vector3D)result;

            inputValue = Math.Round(v1.Z, 2);

            Assert.IsTrue(v1.X.Equals(1), "X");
            Assert.IsTrue(v1.Y.Equals(1), "Y");
            Assert.AreEqual(-1, inputValue, "Z");

            vector = new Vector3D(1, 1, 1);
            result = Projection3D.RotateZ(vector, 90);
            v1 = (Vector3D)result;

            inputValue = Math.Round(v1.X, 2);

            Assert.AreEqual(-1, inputValue, "X");
            Assert.IsTrue(v1.Y.Equals(1), "Y");
            Assert.IsTrue(v1.Z.Equals(1), "Z");
        }
    }
}
