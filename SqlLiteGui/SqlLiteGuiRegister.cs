/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGuiRegister.cs
 * PURPOSE:     Some basic Objects we need for Data-binding and message stuff, here lie all Data-Binding Objects and Shared Objects
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global, they are used but as described as observable Object so no need to complain
// ReSharper disable MemberCanBeInternal, Problem here is these need to be public to be visible as observable Object

using SqliteHelper;

namespace SQLiteGui
{
    /// <summary>
    ///     All Items we use for Data-binding
    /// </summary>
    public sealed class TableDetails
    {
        /// <summary>
        ///     Name of the Table
        ///     Public although Class is internal, don't ask
        /// </summary>
        public string TableAlias { get; internal set; }
    }

    /// <inheritdoc />
    /// <summary>
    ///     Here we collect a custom where Clause, for example right now it processes all Binary Input
    /// </summary>
    internal sealed class Binary : ObservableObject
    {
        /// <summary>
        ///     The value.
        /// </summary>
        private string _value;

        /// <summary>
        ///     The where.
        /// </summary>
        private string _where;

        /// <summary>
        ///     Gets or sets the where.
        /// </summary>
        public string Where
        {
            get => _where;
            set
            {
                _where = value;
                RaisePropertyChangedEvent(SqLiteGuiResource.ObsColumn);
            }
        }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                RaisePropertyChangedEvent(nameof(Value));
            }
        }
    }

    /// <inheritdoc />
    /// <summary>
    ///     Generated Update Item that will be loaded into the Database
    /// </summary>
    internal sealed class UpdateItem : ObservableObject
    {
        /// <summary>
        ///     The value.
        /// </summary>
        private string _value;

        /// <summary>
        ///     Gets or sets the header name.
        /// </summary>
        public string HeaderName { get; internal init; }

        /// <summary>
        ///     Gets or sets the data type.
        /// </summary>
        public SqLiteDataTypes DataType { get; internal init; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                RaisePropertyChangedEvent(nameof(DataType));
            }
        }
    }

    /// <inheritdoc />
    /// <summary>
    ///     Used for adding new Tables
    ///     Collects Data from Data Grid
    /// </summary>
    internal sealed class TableColumnsExtended : ObservableObject
    {
        /// <summary>
        ///     The data type.
        /// </summary>
        private SqLiteDataTypes _dataType;

        /// <summary>
        ///     The header.
        /// </summary>
        private string _header;

        /// <summary>
        ///     Is it not null.
        /// </summary>
        private bool _notNull;

        /// <summary>
        ///     The primary key.
        /// </summary>
        private bool _primaryKey;

        /// <summary>
        ///     The unique identifier.
        /// </summary>
        private bool _unique;

        /// <summary>
        ///     Must be Unique, will be checked at Runtime
        /// </summary>
        public string Header
        {
            get => _header;
            set
            {
                _header = value;
                RaisePropertyChangedEvent(nameof(DataType));
            }
        }

        /// <summary>
        ///     DataType
        /// </summary>
        public SqLiteDataTypes DataType
        {
            get => _dataType;
            set
            {
                _dataType = value;
                RaisePropertyChangedEvent(nameof(DataType));
            }
        }

        /// <summary>
        ///     Optional
        /// </summary>
        public bool Unique
        {
            get => _unique;
            set
            {
                _unique = value;
                RaisePropertyChangedEvent(nameof(DataType));
            }
        }

        /// <summary>
        ///     Optional
        /// </summary>
        public bool PrimaryKey
        {
            get => _primaryKey;
            set
            {
                _primaryKey = value;
                RaisePropertyChangedEvent(nameof(DataType));
            }
        }

        /// <summary>
        ///     Optional
        ///     false is standard
        /// </summary>
        public bool NotNull
        {
            get => _notNull;
            set
            {
                _notNull = value;
                RaisePropertyChangedEvent(nameof(DataType));
            }
        }
    }
}
