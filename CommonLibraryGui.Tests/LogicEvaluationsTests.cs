/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryGui.Tests
 * FILE:        LogicEvaluationsTests.cs
 * PURPOSE:     Tests for LogicEvaluationsTests in CommonFilter.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CommonFilter;
using NUnit.Framework;

namespace CommonLibraryGui.Tests
{
    /// <summary>
    /// Logic Evaluation Tests.
    /// </summary>
    [TestFixture]
    public class LogicEvaluationsTests
    {
        /// <summary>
        /// Evaluates the like operator returns true when match.
        /// </summary>
        [Test]
        public void Evaluate_LikeOperator_ReturnsTrueWhenMatch()
        {
            // Arrange
            var evaluator = new LogicEvaluations();
            var conditions = new List<FilterOption>
            {
                new FilterOption
                {
                    SelectedLogicalOperator = LogicOperator.And,
                    SelectedCompareOperator = CompareOperator.Like,
                    EntryText = "Wayfarer"
                }
            };

            // Act
            var result = evaluator.Evaluate("Hello Wayfarer!", conditions);

            // Assert (NUnit 4 constraint syntax)
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// Evaluates the and not operator excludes matching string.
        /// </summary>
        [Test]
        public void Evaluate_AndNotOperator_ExcludesMatchingString()
        {
            // Arrange
            var evaluator = new LogicEvaluations();
            var conditions = new List<FilterOption>
            {
                new FilterOption { SelectedCompareOperator = CompareOperator.Like, EntryText = "Common" },
                new FilterOption
                {
                    SelectedLogicalOperator = LogicOperator.AndNot,
                    SelectedCompareOperator = CompareOperator.Like,
                    EntryText = "Draft"
                }
            };

            // Act & Assert
            Assert.That(evaluator.Evaluate("CommonFilter", conditions), Is.True);
            Assert.That(evaluator.Evaluate("CommonDraft", conditions), Is.False);
        }
    }
}
