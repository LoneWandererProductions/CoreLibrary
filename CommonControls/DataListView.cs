/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/DataListView.cs
 * PURPOSE:     Basic View for ListView
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using ExtendedSystemObjects;
using ViewModel;

namespace CommonControls
{
    /// <inheritdoc />
    /// <summary>
    ///     View for ListView
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged" />
    public sealed class DataListView : INotifyPropertyChanged
    {
        /// <summary>
        ///     The data list
        /// </summary>
        private readonly DataList _dataList;

        /// <summary>
        ///     The identifier
        /// </summary>
        private readonly List<int> _id;

        /// <summary>
        ///     The items
        /// </summary>
        private readonly BindingList<DataItem> _items;

        /// <summary>
        ///     The list box
        /// </summary>
        private readonly ListBox _listBox;

        /// <summary>
        ///     The selection
        /// </summary>
        private readonly object _selection;

        /// <summary>
        ///     The unique tag
        /// </summary>
        private readonly bool _unique;

        private readonly List<string> _uniqueName;

        /// <summary>
        ///     The add command
        /// </summary>
        private ICommand _addCommand;

        /// <summary>
        ///     The delete command
        /// </summary>
        private ICommand _deleteCommand;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataListView" /> class.
        /// </summary>
        /// <param name="dataItems">The data items.</param>
        /// <param name="dataList">The data list.</param>
        /// <param name="unique">if set to <c>true</c> [unique].</param>
        /// <param name="listBox">The listbox</param>
        public DataListView(IList<DataItem> dataItems, DataList dataList, bool unique, ListBox listBox)
        {
            _dataList = dataList;
            _id = new List<int>();
            _listBox = listBox;
            _uniqueName = new List<string>();

            ChangeLog = new TransactionLogs();

            if (dataItems == null || dataItems.Count == 0)
            {
                Items = new BindingList<DataItem> { new() { Id = 0, Name = ComCtlResources.DatalistEntry } };

                foreach (var item in Items)
                {
                    ChangeLog.Add(item.Id, item, true);
                }
            }
            else
            {
                Items = new BindingList<DataItem>(dataItems);

                foreach (var item in Items)
                {
                    var check = _uniqueName.AddIsDistinct(item.Name);

                    if (!check && _unique)
                    {
                        _dataList.NotUniqueStart(item.Name);
                        return;
                    }

                    ChangeLog.Add(item.Id, item, true);
                }
            }

            _unique = unique;

            foreach (var item in Items)
            {
                _id.Add(item.Id);
            }

            Items.ListChanged += ItemsListChanged;
            listBox.SelectionChanged += ItemSelected;
        }

        /// <summary>
        ///     The change log
        /// </summary>
        /// <value>
        ///     The change log.
        /// </value>
        internal TransactionLogs ChangeLog { get; }

        /// <summary>
        ///     Checks if something was changed
        ///     Should be used more Careful
        /// </summary>
        internal bool Changed => ChangeLog.Changed;

        /// <summary>
        ///     Gets the delete command.
        /// </summary>
        /// <value>
        ///     The delete command.
        /// </value>
        public ICommand DeleteCommand =>
            _deleteCommand ??= new DelegateCommand<object>(DeleteAction, CanExecute);

        /// <summary>
        ///     Gets the add command.
        /// </summary>
        /// <value>
        ///     The add command.
        /// </value>
        public ICommand AddCommand =>
            _addCommand ??= new DelegateCommand<object>(AddAction, CanExecute);

        /// <summary>
        ///     Gets or sets the items.
        /// </summary>
        /// <value>
        ///     The items.
        /// </value>
        public BindingList<DataItem> Items
        {
            get => _items;
            init
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public object Selection
        {
            get => _selection;
            init
            {
                _selection = value;
                OnPropertyChanged(nameof(Selection));
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Updates the selected item. Only used for ChangeLog a bit of a hack but it works
        /// </summary>
        public void UpdateSelectedItem()
        {
            if (_dataList.Selection == null)
            {
                return;
            }

            var item = _dataList.Selection;

            //first add Remove command
            ChangeLog.Remove(item.Id);
            //than add the exact item again
            ChangeLog.Add(item.Id, item, false);
        }

        /// <summary>
        ///     Deletes the action.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void DeleteAction(object obj)
        {
            var item = obj as DataItem;
            RemoveItem(item);
        }

        /// <summary>
        ///     Adds the action.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void AddAction(object obj)
        {
            var name = CheckName(ComCtlResources.NewItem);

            AddItem(name);
        }

        /// <summary>
        ///     Determines whether this instance can execute the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///     <c>true</c> if this instance can execute the specified object; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExecute(object obj)
        {
            // check if executing is allowed, not used right now
            return true;
        }

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Checks the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>New name</returns>
        private string CheckName(string name)
        {
            while (true)
            {
                if (!CheckItemName(name))
                {
                    return name;
                }

                var count = 0;

                while (CheckItemName(name))
                {
                    var cache = $"{name}({count++})";
                    if (CheckItemName(cache))
                    {
                        continue;
                    }

                    name = cache;
                }
            }
        }

        /// <summary>
        ///     Checks the name of the item.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>string equal or not</returns>
        private bool CheckItemName(string name)
        {
            return _items.Any(item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     Adds a new Entry to the Save List
        /// </summary>
        /// <param name="name">Name of the Entry, if something goes wrong well tough luck, nothing changes</param>
        private void AddItem(string name)
        {
            //basic sanity checks
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            var idList = Items.Select(element => element.Id).ToList();

            int? firstAvailable = Enumerable.Range(0, int.MaxValue)
                .Except(idList)
                .FirstOrDefault();

            var item = new DataItem { Name = name, Id = (int)firstAvailable };

            Items.Add(item);
            _dataList.Added(item);
        }

        /// <summary>
        ///     Removes the item.
        /// </summary>
        /// <param name="item">The item.</param>
        private void RemoveItem(DataItem item)
        {
            var check = Items.Remove(item);
            if (!check)
            {
                return;
            }

            _dataList.Removed(item);
        }

        /// <summary>
        ///     Items changed list.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ListChangedEventArgs" /> instance containing the event data.</param>
        private void ItemsListChanged(object sender, ListChangedEventArgs e)
        {
            bool check;

            switch (e.ListChangedType)
            {
                case ListChangedType.ItemChanged:
                {
                    //NewIndex = OldIndex
                    var item = Items[e.NewIndex];

                    check = _uniqueName.AddIsDistinct(item.Name);

                    if (!check && _unique)
                    {
                        _dataList.NotUnique(item.Name);
                        return;
                    }

                    ChangeLog.Change(item.Id, item);

                    break;
                }
                case ListChangedType.ItemAdded:
                {
                    //must be NewIndex
                    var item = Items[e.NewIndex];

                    check = _uniqueName.AddIsDistinct(item.Name);

                    if (!check && _unique)
                    {
                        _dataList.NotUnique(item.Name);
                        return;
                    }

                    ChangeLog.Add(item.Id, item, false);

                    _id.Add(item.Id);

                    break;
                }
                case ListChangedType.ItemDeleted:
                {
                    //must be OldIndex
                    var id = _id[e.NewIndex];

                    _id.Remove(id);

                    //must be NewIndex
                    var item = Items[id];

                    ChangeLog.Remove(item.Id);

                    _uniqueName.Remove(item.Name);

                    break;
                }
            }
        }

        /// <summary>
        ///     Items the selected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void ItemSelected(object sender, SelectionChangedEventArgs e)
        {
            var lst = _listBox.SelectedItems;

            if (lst.Count == 0)
            {
                return;
            }

            var items = new List<DataItem>(lst.Count);

            items.AddRange(lst.Cast<DataItem>());

            if (items.Count == 0)
            {
                return;
            }

            _dataList.Selection = items[0];
            _dataList.Selections = items;
        }
    }
}
