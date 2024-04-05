﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/SqlConnect.cs
 * PURPOSE:     View for Sql Connection dialog
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ViewModel;

namespace CommonControls
{
    /// <inheritdoc />
    /// <summary>
    ///     View for Sql Connect Window
    /// </summary>
    /// <seealso cref="INotifyPropertyChanged" />
    public sealed class SqlView : INotifyPropertyChanged
    {
        /// <summary>
        ///     The close command
        /// </summary>
        private ICommand _closeCommand;

        /// <summary>
        ///     The connect command
        /// </summary>
        private ICommand _connectCommand;

        /// <summary>
        ///     The data base
        /// </summary>
        private string _dataBase;

        /// <summary>
        ///     Is the Menu active
        /// </summary>
        private bool _isActive;

        /// <summary>
        ///     The server
        /// </summary>
        private string _server;

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value)
                {
                    return;
                }

                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }

        /// <summary>
        ///     Gets or sets the database.
        /// </summary>
        /// <value>
        ///     The database.
        /// </value>
        public string Database
        {
            get => _dataBase;
            set
            {
                if (_dataBase == value)
                {
                    return;
                }

                _dataBase = value;
                OnPropertyChanged(nameof(Database));
            }
        }

        /// <summary>
        ///     Gets or sets the server.
        /// </summary>
        /// <value>
        ///     The server.
        /// </value>
        public string Server
        {
            get => _server;
            set
            {
                if (_server == value)
                {
                    return;
                }

                _server = value;
                OnPropertyChanged(nameof(Server));
            }
        }

        /// <summary>
        ///     Gets or sets the log.
        /// </summary>
        /// <value>
        ///     The log.
        /// </value>
        public string Log
        {
            get => AddLog;
            set
            {
                if (AddLog == value)
                {
                    return;
                }

                AddLog = value;
                OnPropertyChanged(nameof(Log));
            }
        }

        /// <summary>
        ///     Gets the connect command.
        /// </summary>
        /// <value>
        ///     The connect command.
        /// </value>
        public ICommand ConnectCommand =>
            _connectCommand ??= new DelegateCommand<object>(ConnectAction, CanExecute);

        /// <summary>
        ///     Gets the close command.
        /// </summary>
        /// <value>
        ///     The close command.
        /// </value>
        public ICommand CloseCommand =>
            _closeCommand ??= new DelegateCommand<object>(CloseAction, CanExecute);

        /// <summary>
        /// Gets the connection string Class.
        /// </summary>
        /// <value>
        /// Builds the Connection String.
        /// </value>
        public SqlConnect Connection { get; private set; }

        /// <summary>
        ///     Gets the connection string for a Database.
        ///     Transmitted to the external viewer
        /// </summary>
        /// <value>
        ///     The connection string.
        /// </value>
        public string ConnectionStringDB { get; private set; }

        /// <summary>
        /// Gets the connection string for server only.
        /// </summary>
        /// <value>
        /// The connection string server.
        /// </value>
        public string ConnectionStringServer { get; private set; }

        /// <summary>
        /// Gets or sets the add log.
        /// Here you can add some infos to the log in the Login Window
        /// </summary>
        /// <value>
        /// The add log.
        /// </value>
        public string AddLog { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Triggers if an Attribute gets changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Gets a value indicating whether this instance can execute.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///     <c>true</c> if this instance can execute the specified object; otherwise, <c>false</c>.
        /// </returns>
        /// <value>
        ///     <c>true</c> if this instance can execute; otherwise, <c>false</c>.
        /// </value>
        private bool CanExecute(object obj)
        {
            // check if executing is allowed, not used right now
            return true;
        }

        /// <summary>
        ///     Connects the action.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void ConnectAction(object obj)
        {
            var connect = new SqlConnect(Database, Server, IsActive);
            Connection = connect;
            ConnectionStringServer = connect.GetConnectionString(false);
            bool check = true;

            if (string.IsNullOrEmpty(Server))
            {
                check = false;
                Log = string.Concat(Log, ConnectionStringServer, Environment.NewLine);
            }

            ConnectionStringDB = connect.GetConnectionString(true);

            if (string.IsNullOrEmpty(Database))
            {
                check = false;
                Log = string.Concat(Log, ConnectionStringDB, Environment.NewLine);
            }

            if (check)

            {
                Log = string.Concat(Log, ComCtlResources.DBLogConnectionStringBuild, Environment.NewLine);
            }
            else
            {
                Log = string.Concat(Log, ComCtlResources.DBLogConnectionStringBuildError, Environment.NewLine);
            }
        }

        /// <summary>
        ///     Closes the app
        /// </summary>
        /// <param name="obj">The object.</param>
        private void CloseAction(object obj)
        {
            Application.Current.Shutdown();
        }
    }
}
