﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Serializer
 * FILE:        Serializer/Item.cs
 * PURPOSE:     Item, helper Object needed for Dictionaries
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// Disabled ReSharper Warnings, do not make the suggested Changes!
// ReSharper disable MemberCanBeInternal
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

using System;

namespace Serializer
{
    /// <summary>
    ///     Helper Object for serializing Dictionaries
    ///     Do not remove any Constructors or change Visibility!
    /// </summary>
    public sealed class Item
    {
        /// <summary>
        ///     Don't Remove
        ///     needed for Serialization of Dictionaries
        /// </summary>
        public Item()
        {
        }

        /// <summary>
        ///     Holds Key Value Pair, Key as usual in, Value a Serialized Object saved as String
        /// </summary>
        /// <param name="key">Key of Dictionary</param>
        /// <param name="value">String of Serialized Value</param>
        public Item(string key, string value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        ///     Don't Remove
        ///     needed for Serialization of Dictionaries
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        ///     Don't Remove
        ///     needed for Serialization of Dictionaries
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     Converts to string.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Concat(Key, SerialResources.Separator, Value);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value);
        }
    }
}
