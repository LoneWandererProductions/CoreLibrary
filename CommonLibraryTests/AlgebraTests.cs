/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/AlgebraTests.cs
 * PURPOSE:     Some simple Algebra Tes
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests;

/// <summary>
///     Test for Permutations mostly now
/// </summary>
[TestClass]
public class AlgebraTests
{
    /// <summary>
    ///     Permutations this instance.
    /// </summary>
    [TestMethod]
    public void Permutation()
    {
        var lst = new List<string> { "A", "B", "C" };
        var result = lst.GetCombination();

        foreach (var items in result)
        {
            foreach (var item in items)
            {
                Trace.Write(item);
            }

            Trace.WriteLine(string.Empty);
        }

        Assert.AreEqual(7, result.Count(), "Right amount");
    }

    /// <summary>
    ///     Permutations of k in n Elements.
    /// </summary>
    [TestMethod]
    public void PermutationNk()
    {
        Trace.WriteLine("First Test");

        var a = new List<string> { "A", "B", "C" };

        var lst = a.CombinationsWithRepetition(3);

        foreach (var item in lst)
        {
            Trace.WriteLine(item);
        }

        Assert.AreEqual(27, lst.Count(), "Right amount");
    }
}
