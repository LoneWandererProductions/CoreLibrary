/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilderTests
 * FILE:        UnusedConstantAnalyzerTests.cs
 * PURPOSE:     Unit tests for UnusedConstantAnalyzer.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CoreBuilder;
using CoreBuilder.Rules;

namespace CoreBuilderTests
{
    /// <summary>
    /// Unit tests for UnusedConstantAnalyzer.
    /// </summary>
    [TestClass]
    public class UnusedConstantAnalyzerTests
    {
        /// <summary>
        /// Analyzes the project finds unused constant.
        /// </summary>
        [TestMethod]
        public void AnalyzeProjectFindsUnusedConstant()
        {
            // Arrange
            var analyzer = new UnusedConstantAnalyzer();
            var files = new Dictionary<string, string>
            {
                ["FileA.cs"] = @"
                    namespace Demo
                    {
                        public static class Foo
                        {
                            public const int UNUSED_CONST = 42;
                            public const int USED_CONST = 7;
                        }
                    }
                ",
                ["FileB.cs"] = @"
                    namespace Demo
                    {
                        public class Bar
                        {
                            public void Test()
                            {
                                var x = Foo.USED_CONST;
                            }
                        }
                    }
                "
            };

            // Act
            var diagnostics = analyzer.AnalyzeProject(files);

            // Assert
            var diagsList = new List<Diagnostic>(diagnostics);

            // Expect one diagnostic for UNUSED_CONST
            Assert.AreEqual(1, diagsList.Count, "Exactly one unused constant should be detected.");
            Assert.AreEqual("UNUSED_CONST", diagsList[0].Name);
            StringAssert.Contains(diagsList[0].Message, "never used");
        }

        /// <summary>
        /// Analyzes the project no unused when all used.
        /// </summary>
        [TestMethod]
        public void AnalyzeProjectNoUnusedWhenAllUsed()
        {
            // Arrange
            var analyzer = new UnusedConstantAnalyzer();
            var files = new Dictionary<string, string>
            {
                ["FileA.cs"] = @"
                    public static class Foo
                    {
                        public const string CONST_A = ""Hello"";
                        public static readonly int CONST_B = 10;
                    }
                ",
                ["FileB.cs"] = @"
                    public class Bar
                    {
                        void Test()
                        {
                            var msg = Foo.CONST_A + Foo.CONST_B;
                        }
                    }
                "
            };

            // Act
            var diagnostics = analyzer.AnalyzeProject(files);

            // Assert
            Assert.AreEqual(0, new List<Diagnostic>(diagnostics).Count, "No unused constants expected.");
        }

        /// <summary>
        /// Analyzes the project handles empty project.
        /// </summary>
        [TestMethod]
        public void AnalyzeProjectHandlesEmptyProject()
        {
            // Arrange
            var analyzer = new UnusedConstantAnalyzer();
            var files = new Dictionary<string, string>();

            // Act
            var diagnostics = analyzer.AnalyzeProject(files);

            // Assert
            Assert.AreEqual(0, new List<Diagnostic>(diagnostics).Count, "Empty project should yield no diagnostics.");
        }
    }
}
