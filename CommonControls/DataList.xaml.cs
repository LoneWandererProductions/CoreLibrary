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
        /// The data collection (readonly).
        /// </summary>
        public static readonly DependencyProperty DataCollectionProperty =
            DependencyProperty.Register(nameof(DataCollection), typeof(List<DataItem>), typeof(DataList), null);

        /// <summary>
        /// The list title Dependency Property
        /// </summary>
        public static readonly DependencyProperty ListTitleProperty = DependencyProperty.Register(
            nameof(ListTitle),
            typeof(string),
            typeof(DataList), null);

        /// <summary>
        ///     The unique elements
        /// </summary>
        public static readonly DependencyProperty UniqueElementsProperty = DependencyProperty.Register(
            nameof(UniqueElements),
            typeof(bool),
            typeof(DataList), null);

        /// <summary>
        ///     The unique elements
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(DataItem),
            typeof(DataList), null);

        /// <summary>
        ///     The selected items
        /// </summary>
        public static readonly DependencyProperty SelectedItemsProperty =
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
        public string ListTitle
        {
            get => (string)GetValue(ListTitleProperty);
            set => SetValue(ListTitleProperty, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="DataList" /> is unique.
        /// </summary>
        /// <value>
        ///     <c>true</c> if unique; otherwise, <c>false</c>.
        /// </value>
        public bool UniqueElements
        {
            get => (bool)GetValue(UniqueElementsProperty);
            set => SetValue(UniqueElementsProperty, value);
        }

        /// <summary>
        ///     Gets or sets the selection.
        /// </summary>
        /// <value>
        ///     The selection.
        /// </value>
        public DataItem SelectedItem
        {
            get => (DataItem)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        /// <summary>
        ///     Gets or sets the selections.
        /// </summary>
        /// <value>
        ///     The selections.
        /// </value>
        public List<DataItem> SelectedItems
        {
            get => (List<DataItem>)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        /// <summary>
        ///     Gets or sets the collection.
        /// </summary>
        public List<DataItem> DataCollection
        {
            private get { return (List<DataItem>)GetValue(DataCollectionProperty); }
            set { SetValue(DataCollectionProperty, value); }
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
            _view = new DataListView(DataCollection, this, UniqueElements, List);
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
