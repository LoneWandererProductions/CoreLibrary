using System.Collections.Generic;
using CommonFilter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    [TestClass]
    public class LogicEvaluate
    {
        /// <summary>
        /// Test the logic Evaluation.
        /// </summary>
        [TestMethod]
        public void Evaluate()
        {
            // Example list of conditions
            var conditions = new List<(OptionsOperator Operator, string Text, LogicOperator LogicalOperator)>
            {
                (OptionsOperator.like, "hello",LogicOperator.and),
                (OptionsOperator.Notlike, "world",LogicOperator.or),
                // Add more conditions as needed
            };

            // Input string to test against the conditions
            const string inputString = "hello";

            // Evaluate the conditions against the input string
            bool result = LogicEvaluations.Evaluate(inputString, conditions);

            Assert.IsTrue(result, $"Input string '{inputString}' fulfills the conditions: {result}");
        }
    }
}
