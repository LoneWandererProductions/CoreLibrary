/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/MessageItem.cs
 * PURPOSE:     Internal Message Item
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace SqliteHelper
{
    /// <summary>
    ///     Simple Container that holds System Messages
    /// </summary>
    internal sealed class MessageItem
    {
        /// <summary>
        ///     Gets or sets the Error level.
        /// </summary>
        internal int Level { init; get; }

        /// <summary>
        ///     Gets or sets the message.
        /// </summary>
        internal string Message { init; get; }
    }
}
