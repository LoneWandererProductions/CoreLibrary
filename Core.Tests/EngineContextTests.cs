/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Tests
 * FILE:        EngineContextTests.cs
 * PURPOSE:     Test for our Core State Executive Engine, specifically the EngineContext class which manages state transitions based on conditions and resources.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.StateExecutive;
using Core.StateExecutive.Builder;

namespace Core.Tests
{
    /// <summary>
    /// Fluent Builder tests for the State Executive Engine. This test class demonstrates how to use the EngineContext and StateBuilder to create states and transitions,
    /// and then evaluates those transitions based on a simulated drone blackboard context.
    /// </summary>
    [TestClass]
    public class EngineContextTests
    {
        /// <summary>
        /// The engine
        /// </summary>
        private EngineContext _engine;

        /// <summary>
        /// The blackboard
        /// </summary>
        private DroneBlackboard _blackboard;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _engine = new EngineContext();
            _blackboard = new DroneBlackboard();

            // 1. Build the States using your Fluent Builder
            var idleState = StateBuilder.Create("Idle")
                .TransitionTo("Flying")
                // GUARD: Must be true to proceed
                .When(ctx => ((DroneBlackboard)ctx).IsEngineOn)
                // RESOURCE: Costs 5 battery to take off
                .Claim("Battery", 5)
                // ACTION: Do this during the switch
                .OnTransition(ctx => ctx.Log("Liftoff successful!"))
                .EndTransition()
                .Build();

            var flyingState = StateBuilder.Create("Flying").Build();

            // 2. Register them with the engine
            _engine.RegisterState(idleState);
            _engine.RegisterState(flyingState);
            _engine.SetInitialState("Idle");
        }

        /// <summary>
        /// Evaluates the when conditions met transitions and consumes resources.
        /// </summary>
        [TestMethod]
        public void Evaluate_WhenConditionsMet_TransitionsAndConsumesResources()
        {
            // Arrange
            _blackboard.IsEngineOn = true; // Set the guard to TRUE
            _blackboard.BatteryLevel = 100;

            // Act: "Hey Engine, look at the blackboard. Should we change states?"
            var didTransition = _engine.Evaluate(_blackboard);

            // Assert
            Assert.IsTrue(didTransition, "The engine should have triggered a transition.");
            Assert.AreEqual("Flying", _engine.CurrentState.Id, "The engine should now be in the Flying state.");

            // Prove the atomic transaction worked
            Assert.AreEqual(95, _blackboard.BatteryLevel, "The transition should have consumed 5 Battery.");
            Assert.IsTrue(_blackboard.EventLog.Contains("Liftoff successful!"),
                "The transition effect should have fired.");
        }

        /// <summary>
        /// Evaluates the when guard fails ignores transition and saves resources.
        /// </summary>
        [TestMethod]
        public void Evaluate_WhenGuardFails_IgnoresTransitionAndSavesResources()
        {
            // Arrange
            _blackboard.IsEngineOn = false; // Set the guard to FALSE
            _blackboard.BatteryLevel = 100;

            // Act
            var didTransition = _engine.Evaluate(_blackboard);

            // Assert
            Assert.IsFalse(didTransition, "The engine should NOT have transitioned.");
            Assert.AreEqual("Idle", _engine.CurrentState.Id, "The engine must remain in the Idle state.");

            // Prove the atomic transaction aborted cleanly
            Assert.AreEqual(100, _blackboard.BatteryLevel, "No battery should have been consumed.");
            Assert.IsFalse(_blackboard.EventLog.Contains("Liftoff successful!"), "No effects should have fired.");
        }

        /// <summary>
        /// Evaluates the when resource missing ignores transition.
        /// </summary>
        [TestMethod]
        public void Evaluate_WhenResourceMissing_IgnoresTransition()
        {
            // Arrange
            _blackboard.IsEngineOn = true;
            _blackboard.BatteryLevel = 2; // Guard is true, but NOT enough battery!

            // Act
            var didTransition = _engine.Evaluate(_blackboard);

            // Assert
            Assert.IsFalse(didTransition, "Transition should fail due to lack of resources.");
            Assert.AreEqual("Idle", _engine.CurrentState.Id);
            Assert.AreEqual(2, _blackboard.BatteryLevel, "Battery should remain untouched.");
        }
    }
}
