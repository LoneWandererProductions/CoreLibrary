/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/VectorTests.cs
 * PURPOSE:     2D and 3d Vector Tests
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests;

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
        Assert.AreEqual(scalar, 36, "Dot Product");

        //scalar product
        var vector = one * 3;
        Assert.AreEqual(vector.X, 24, "X scalar Product");
        Assert.AreEqual(vector.Y, 12, "Y scalar Product");
        Assert.AreEqual(vector.Z, 15, "Z scalar Product");

        vector = 3 * one;
        Assert.AreEqual(vector.X, 24, "X scalar Product");
        Assert.AreEqual(vector.Y, 12, "Y scalar Product");
        Assert.AreEqual(vector.Z, 15, "Z scalar Product");

        //scalar division
        vector = two / 2;
        Assert.AreEqual(vector.X, 0.5, "X scalar Division");
        Assert.AreEqual(vector.Y, 1, "Y scalar Division");
        Assert.AreEqual(vector.Z, 2, "Z scalar Division");

        //magnitude (vector length)
        scalar = one.VectorLength(); //10.247
        Assert.AreEqual(Math.Round(scalar, 3), 10.247, "Vector length");

        //Vector Addition
        vector = one + two;
        Assert.AreEqual(vector.X, 9, "X Addition");
        Assert.AreEqual(vector.Y, 6, "Y Addition");
        Assert.AreEqual(vector.Z, 9, "Z Addition");

        //Vector subtraction
        vector = one - two;
        Assert.AreEqual(vector.X, 7, "X Subtraction");
        Assert.AreEqual(vector.Y, 2, "Y Subtraction");
        Assert.AreEqual(vector.Z, 1, "Z Subtraction");

        //Vector Cross Product
        vector = one.CrossProduct(two);
        Assert.AreEqual(vector.X, 6, "X Cross Product");
        Assert.AreEqual(vector.Y, -27, "Y Cross Product");
        Assert.AreEqual(vector.Z, 12, "Z Cross Product");

        //negation
        vector = -one;
        Assert.AreEqual(vector.X, -8, "X Negation");
        Assert.AreEqual(vector.Y, -4, "Y Negation");
        Assert.AreEqual(vector.Z, -5, "Z Negation");

        //Angle between Vector
        scalar = one.Angle(two); //39.946
        scalar = scalar * 180 / Math.PI;
        Assert.AreEqual(Math.Round(scalar, 3), 39.946, "Vector Angle");

        //normalize Vector, Unit Vector
        vector = one.Normalize();
        Assert.AreEqual(Math.Round(vector.X, 5), 0.78072, "X Unit Vector");
        Assert.AreEqual(Math.Round(vector.Y, 5), 0.39036, "Y Unit Vector");
        Assert.AreEqual(Math.Round(vector.Z, 5), 0.48795, "Z Unit Vector");

        //inequality
        Assert.IsTrue(one != two, "Inequality");
        //inequality
        // ReSharper disable once EqualExpressionComparison
        Assert.IsTrue(one == one, "Equality, first");
        Assert.IsTrue(one.Equals(one), "Equality, second");

        var nullVector = Vector3D.ZeroVector;
        Assert.AreEqual(nullVector.X, 0, "X Addition");
        Assert.AreEqual(nullVector.Y, 0, "Y Addition");
        Assert.AreEqual(nullVector.Z, 0, "Z Addition");

        var unitVector = Vector3D.UnitVector;
        Assert.AreEqual(unitVector.X, 1, "X Addition");
        Assert.AreEqual(unitVector.Y, 1, "Y Addition");
        Assert.AreEqual(unitVector.Z, 1, "Z Addition");
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
        Assert.AreEqual(scalar, 16, "Dot Product");

        //scalar multiplication
        var vector = two * 2;
        Assert.AreEqual(vector.X, 2, "X scalar Multiplication.");
        Assert.AreEqual(vector.Y, 4, "Y scalar Multiplication.");

        //scalar multiplication
        vector = 2 * two;
        Assert.AreEqual(vector.X, 2, "X scalar Multiplication.");
        Assert.AreEqual(vector.Y, 4, "Y scalar Multiplication.");

        //scalar division
        vector = one / 2;
        Assert.AreEqual(vector.X, 4, "X scalar Division.");
        Assert.AreEqual(vector.Y, 2, "Y scalar Division.");

        //negation
        vector = -one;
        Assert.AreEqual(vector.X, -8, "X Negation");
        Assert.AreEqual(vector.Y, -4, "Y Negation");

        //Vector Addition
        vector = one + two;
        Assert.AreEqual(vector.X, 9, "X Addition");
        Assert.AreEqual(vector.Y, 6, "Y Addition");

        //normalize Vector, Unit Vector
        vector = one.Normalize();
        Assert.AreEqual(Math.Round(vector.X, 5), 0, 89443, "X Unit Vector");
        Assert.AreEqual(Math.Round(vector.Y, 5), 0, 44721, "Y Unit Vector");

        //Angle between Vector
        scalar = one.Angle(two);
        scalar = scalar * 180 / Math.PI;
        Assert.AreEqual(Math.Round(scalar, 3), 36.87, "Vector Angle");

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
        Assert.AreEqual(inputValue, 0, "X");
        Assert.IsTrue(v1.Y.Equals(1), "Y");
        Assert.IsTrue(v1.Z.Equals(0), "Z");

        vector = new Vector3D(1, 1, 1);
        result = Projection3D.RotateX(vector, 90);
        v1 = (Vector3D)result;

        inputValue = Math.Round(v1.Y, 2);

        Assert.IsTrue(v1.X.Equals(1), "X");
        Assert.AreEqual(inputValue, -1, "Y");
        Assert.IsTrue(v1.Z.Equals(1), "Z");

        vector = new Vector3D(1, 1, 1);
        result = Projection3D.RotateY(vector, 90);
        v1 = (Vector3D)result;

        inputValue = Math.Round(v1.Z, 2);

        Assert.IsTrue(v1.X.Equals(1), "X");
        Assert.IsTrue(v1.Y.Equals(1), "Y");
        Assert.AreEqual(inputValue, -1, "Z");

        vector = new Vector3D(1, 1, 1);
        result = Projection3D.RotateZ(vector, 90);
        v1 = (Vector3D)result;

        inputValue = Math.Round(v1.X, 2);

        Assert.AreEqual(inputValue, -1, "X");
        Assert.IsTrue(v1.Y.Equals(1), "Y");
        Assert.IsTrue(v1.Z.Equals(1), "Z");
    }
}
