using System.Collections.Generic;
using CommonControls.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    [TestClass]
    public class LogicEvaluate
    {
        [TestMethod]
        public void Evaluate()
        {
            // Example list of conditions
            var conditions = new List<(string Operator, string Text, string LogicalOperator)>
            {
                ("like", "hello", "and"),
                ("not like", "world", "or"),
                // Add more conditions as needed
            };

            // Input string to test against the conditions
            string inputString = "hello";

            // Evaluate the conditions against the input string
            bool result = LogicEvaluations.Evaluate(inputString, conditions);


            Assert.IsTrue(result, $"Input string '{inputString}' fulfills the conditions: {result}");
        }
    }
}
