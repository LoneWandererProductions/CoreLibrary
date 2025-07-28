/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/Statistics.cs
 * PURPOSE:     Tests for the Math Tools
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests;

/// <summary>
///     Test some image related stuff
/// </summary>
[TestClass]
public class Statistics
{
    /// <summary>
    ///     Test the custom DirectBitmap and how it works
    /// </summary>
    [TestMethod]
    public void Dispersion()
    {
        var lst = new List<double>
        {
            1,
            3,
            5,
            9,
            12
        };
        var arithmetic = new Dispersion(lst);

        Assert.AreEqual(6, arithmetic.ArithmeticMean, $"Test Failed: {arithmetic.ArithmeticMean}");

        Assert.AreEqual(16, arithmetic.Variance, $"Test Failed: {arithmetic.Variance}");

        Assert.AreEqual(4, arithmetic.StandardDeviation,
            $"Test Failed: {arithmetic.StandardDeviation}");

        Assert.AreEqual(0.667, Math.Round(arithmetic.CoefficientOfVariation, 3),
            $"Test Failed: {arithmetic.CoefficientOfVariation}");

        Assert.AreEqual(11, arithmetic.Span, $"Test Failed: {arithmetic.Span}");

        Assert.AreEqual(3.6, arithmetic.MeanAbsoluteDeviation,
            $"Test Failed: {arithmetic.MeanAbsoluteDeviation}");

        lst = new List<double>
        {
            11,
            3,
            5,
            9,
            1
        };
        arithmetic = new Dispersion(lst);

        Assert.AreEqual(10, arithmetic.Span, $"Test Failed: {arithmetic.Span}");
    }
}
