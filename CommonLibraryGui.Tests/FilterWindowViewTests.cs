/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryGui.Tests
 * FILE:        FilterWindowViewTests.cs
 * PURPOSE:     Tests for FilterWindowView in CommonFilter.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CommonFilter;
using NUnit.Framework;

namespace CommonLibraryGui.Tests
{
    /// <summary>
    /// Filter Window Tests.
    /// </summary>
    [TestFixture]
    public class FilterWindowViewTests
    {
        /// <summary>
        /// The view
        /// </summary>
        private FilterWindowView _view = null!;

        /// <summary>
        /// Sets up.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _view = new FilterWindowView();
        }

        /// <summary>
        /// Adds the command adds new search parameter view model.
        /// </summary>
        [Test]
        public void AddCommand_AddsNewSearchParameterViewModel()
        {
            // Act
            _view.AddCommand.Execute(null);

            // Assert
            Assert.That(_view.Filters, Has.Count.EqualTo(1));
        }

        /// <summary>
        /// Deletes the command removes parameter from collection.
        /// </summary>
        [Test]
        public void DeleteCommand_RemovesParameterFromCollection()
        {
            // Arrange
            _view.AddCommand.Execute(null);
            var itemToDelete = _view.Filters[0];

            // Act
            itemToDelete.DeleteCommand.Execute(null);

            // Assert
            Assert.That(_view.Filters, Is.Empty);
        }

        /// <summary>
        /// Dones the command triggers request close with captured options.
        /// </summary>
        [Test]
        public void DoneCommand_TriggersRequestCloseWithCapturedOptions()
        {
            // Arrange
            _view.AddCommand.Execute(null);
            _view.Filters[0].EntryText = "TestFilter";
            _view.Filters[0].SelectedCompareOperator = CompareOperator.Equal;

            List<FilterOption>? outputConditions = null;
            _view.RequestClose += (sender, conditions) => outputConditions = conditions;

            // Act
            _view.DoneCommand.Execute(null);

            // Assert
            Assert.That(outputConditions, Is.Not.Null);
            Assert.That(outputConditions, Has.Count.EqualTo(1));
            Assert.That(outputConditions![0].EntryText, Is.EqualTo("TestFilter"));
            Assert.That(outputConditions[0].SelectedCompareOperator, Is.EqualTo(CompareOperator.Equal));
        }
    }
}
