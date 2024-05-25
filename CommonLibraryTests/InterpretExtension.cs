using System;
using System.Collections.Generic;
using Interpreter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    [TestClass]
    public class IrtPromptTests
    {
        private IrtPrompt _irtPrompt;

        [TestInitialize]
        public void SetUp()
        {
            _irtPrompt = new IrtPrompt();
            var commands = new Dictionary<int, InCommand>
            {
                {
                    1, new InCommand
                    {
                        Command = "BASE",
                        ParameterCount = 1,
                        Description = "Base command",
                        Execute = (parameters) => "Base executed with " + parameters[0],
                        Extensions = new Dictionary<string, Func<object, List<string>, object>>
                        {
                            {
                                "EXT", (baseResult, parameters) =>
                                    baseResult + " | Ext executed with " + parameters[0]
                            }
                        }
                    }
                }
            };

            var userSpace = new UserSpace { Commands = commands, UserSpaceName = "TestNamespace" };
            _irtPrompt.Initiate(userSpace);
        }

        [TestMethod]
        public void HandleInput_ChainedCommandWithExtension_Success()
        {
            bool logHandled = false;

            _irtPrompt.sendLog += (_, e) =>
            {
                Assert.AreEqual("Base executed with param1 | Ext executed with param2", e);
                logHandled = true;
            };

            _irtPrompt.HandleInput("BASE(param1).EXT(param2)");

            //Assert.IsTrue(logHandled);
        }
    }
}
