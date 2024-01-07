/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/DataList.xaml.cs
 * PURPOSE:     Basic List View with some extended Features
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal, never else it won't even work as an control
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable EventNeverSubscribedTo.Global

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ExtendedSystemObjects;

namespace CommonControls
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    ///     Basic Control
    /// </summary>
    public sealed partial class DataList
    {
        /// <summary>
        ///     Delegate for Item add
        /// </summary>
        /// <param name="item">The item.</param>
        public delegate void ItemAdd(DataItem item);

        /// <summary>
        ///     delegate for Item remove
        /// </summary>
        /// <param name="item">The item.</param>
        public delegate void ItemRemove(DataItem item);

        /// <summary>
        ///     The data collection (readonly). Value: DependencyProperty.Register DataCollection
        /// </summary>
        public static DependencyProperty DataCollection =
            DependencyProperty.Register(nameof(DataCollection), typeof(List<DataItem>), typeof(DataList), null);

        /// <summary>
        ///     The list title Dependency Property
        /// </summary>
        public static readonly DependencyProperty ListTitle = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(DataList), null);

        /// <summary>
        ///     The unique elements
        /// </summary>
        public static readonly DependencyProperty UniqueElements = DependencyProperty.Register(
            nameof(Unique),
            typeof(bool),
            typeof(DataList), null);

        /// <summary>
        ///     The unique elements
        /// </summary>
        public static readonly DependencyProperty SelectedItem = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(DataItem),
            typeof(DataList), null);

        /// <summary>
        ///     The selected items
        /// </summary>
        public static DependencyProperty SelectedItems =
            DependencyProperty.Register(nameof(SelectedItems), typeof(List<DataItem>), typeof(DataList), null);

        /// <summary>
        ///     The view
        /// </summary>
        private DataListView _view;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="DataList" /> class.
        /// </summary>
        public DataList()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets the Changelog.
        /// </summary>
        /// <value>
        ///     The Changelog.
        /// </value>
        public TransactionLogs ChangeLog => _view.ChangeLog;

        /// <summary>
        ///     Gets or sets the title.
        /// </summary>
        public string Title
        {
            get => (string)GetValue(ListTitle);
            set => SetValue(ListTitle, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="DataList" /> is unique.
        /// </summary>
        /// <value>
        ///     <c>true</c> if unique; otherwise, <c>false</c>.
        /// </value>
        public bool Unique
        {
            get => (bool)GetValue(UniqueElements);
            set => SetValue(UniqueElements, value);
        }

        /// <summary>
        ///     Gets or sets the selection.
        /// </summary>
        /// <value>
        ///     The selection.
        /// </value>
        public DataItem Selection
        {
            get => (DataItem)GetValue(SelectedItem);
            set => SetValue(SelectedItem, value);
        }

        /// <summary>
        ///     Gets or sets the selections.
        /// </summary>
        /// <value>
        ///     The selections.
        /// </value>
        public List<DataItem> Selections
        {
            get => (List<DataItem>)GetValue(SelectedItems);
            set => SetValue(SelectedItems, value);
        }

        /// <summary>
        ///     Gets or sets the collection.
        /// </summary>
        public List<DataItem> Collection
        {
            private get { return (List<DataItem>)GetValue(DataCollection); }
            set { SetValue(DataCollection, value); }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="DataList" /> is changed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if changed; otherwise, <c>false</c>.
        /// </value>
        public bool Changed => _view.Changed;

        /// <summary>
        ///     Updates the item mostly in the Changelog.
        /// </summary>
        public void UpdateItem()
        {
            _view.UpdateSelectedItem();
        }

        /// <summary>
        ///     Occurs when [item added].
        /// </summary>
        public event ItemAdd ItemAdded;

        /// <summary>
        ///     Occurs when [item added].
        /// </summary>
        public event ItemRemove ItemRemoved;

        /// <summary>
        ///     Handles the Loaded event of the UserControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _view = new DataListView(Collection, this, Unique, List);
            DataContext = _view;
        }

        /// <summary>
        ///     Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal void Added(DataItem item)
        {
            ItemAdded?.Invoke(item);
        }

        /// <summary>
        ///     Removed the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal void Removed(DataItem item)
        {
            ItemRemoved?.Invoke(item);
        }

        /// <summary>
        ///     Not a unique entry.
        /// </summary>
        /// <param name="name">The name.</param>
        internal void NotUnique(string name)
        {
            MessageBox.Show(ComCtlResources.CaptionUnique, string.Concat(ComCtlResources.UniqueMessage, name),
                MessageBoxButton.OK);
        }

        /// <summary>
        ///     Not an unique start.
        /// </summary>
        /// <param name="name">The name.</param>
        internal void NotUniqueStart(string name)
        {
            MessageBox.Show(ComCtlResources.CaptionUnique, string.Concat(ComCtlResources.UniqueMessageStart, name),
                MessageBoxButton.OK);
        }
    }
}