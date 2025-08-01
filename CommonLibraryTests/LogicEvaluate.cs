﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        LogicEvaluate.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using CommonFilter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests;

/// <summary>
///     test class for our logic engine
/// </summary>
[TestClass]
public class LogicEvaluate
{
    /// <summary>
    ///     Test the logic Evaluation.
    /// </summary>
    [TestMethod]
    public void EvaluateTest()
    {
        var evaluate = new LogicEvaluations();

        // Example list of conditions
        var conditions = new List<FilterOption>();

        var con = new FilterOption
        {
            SelectedCompareOperator = CompareOperator.Like,
            SelectedLogicalOperator = LogicOperator.And,
            EntryText = "hello"
        };

        conditions.Add(con);

        con = new FilterOption
        {
            SelectedCompareOperator = CompareOperator.NotLike,
            SelectedLogicalOperator = LogicOperator.Or,
            EntryText = "world"
        };

        conditions.Add(con);
        // Add more conditions as needed

        // Input string to test against the conditions
        const string inputString = "hello";

        // Evaluate the conditions against the input string
        var result = evaluate.Evaluate(inputString, conditions);

        Assert.IsTrue(result, $"Input string '{inputString}' fulfills the conditions: {result}");
    }
}
