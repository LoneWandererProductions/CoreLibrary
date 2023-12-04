/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/PluginItem.cs
 * PURPOSE:     Container for the collected Plugins
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using Plugin;
using ViewModel;

namespace CommonControls
{
    /// <inheritdoc />
    /// <summary>
    ///     Basic Container for the Plugin System
    /// </summary>
    /// <seealso cref="ObservableObject" />
    public sealed class PluginItem : ObservableObject
    {
        /// <summary>
        ///     The command
        /// </summary>
        private readonly IPlugin _command;

        /// <summary>
        ///     The description
        /// </summary>
        private string _description;

        /// <summary>
        ///     The name
        /// </summary>
        private string _name;

        /// <summary>
        ///     The type
        /// </summary>
        private string _type;

        /// <summary>
        ///     The version
        /// </summary>
        private Version _version;

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChangedEvent(nameof(Name));
            }
        }

        /// <summary>
        ///     Gets or sets the type.
        /// </summary>
        /// <value>
        ///     The type.
        /// </value>
        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                RaisePropertyChangedEvent(nameof(Type));
            }
        }

        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                RaisePropertyChangedEvent(nameof(Description));
            }
        }

        /// <summary>
        ///     Gets or sets the version.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        public Version Version
        {
            get => _version;
            set
            {
                _version = value;
                RaisePropertyChangedEvent(nameof(Version));
            }
        }

        /// <summary>
        ///     Gets the command.
        /// </summary>
        /// <value>
        ///     The command.
        /// </value>
        public IPlugin Command
        {
            get => _command;
            init
            {
                _command = value;
                RaisePropertyChangedEvent(nameof(Command));
            }
        }
    }
}
