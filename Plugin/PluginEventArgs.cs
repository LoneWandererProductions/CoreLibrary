/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Plugin
 * FILE:        Plugin/PluginEventArgs.cs
 * PURPOSE:     Basic Event Provider for our new Event Management System in the Plugin Manager
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
 */


using System;

namespace Plugin
{
    public class PluginEventArgs : EventArgs
    {
        public string Message { get; }

        public PluginEventArgs(string message)
        {
            Message = message;
        }
    }
}
