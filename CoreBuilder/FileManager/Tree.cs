/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.FileManager
 * FILE:        Tree.cs
 * PURPOSE:     Command to display directory structure in tree-like format.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System;
using System.IO;
using System.Text;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder.FileManager
{
    /// <inheritdoc />
    /// <summary>
    /// Displays a directory structure in a tree-like visual representation.
    /// </summary>
    public sealed class Tree : ICommand
    {
        /// <inheritdoc />
        public string Name => "Tree";

        /// <inheritdoc />
        public string Description => "Displays folder structure visually from a given path.";

        /// <inheritdoc />
        public string Namespace => "FileSystem";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            if (args.Length < 1)
                return CommandResult.Fail("Usage: Tree [path]");

            var path = args[0];

            if (!Directory.Exists(path))
                return CommandResult.Fail($"Path does not exist: {path}");

            var sb = new StringBuilder();
            sb.AppendLine(path);

            try
            {
                BuildTree(path, sb, "", true);
                return CommandResult.Ok(sb.ToString(), EnumTypes.Wstring);
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"Tree failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively builds the directory structure.
        /// </summary>
        private void BuildTree(string path, StringBuilder sb, string indent, bool last)
        {
            var prefix = last ? "└── " : "├── ";
            sb.AppendLine($"{indent}{prefix}{Path.GetFileName(path)}");

            indent += last ? "    " : "│   ";

            var dirs = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);

            for (var i = 0; i < files.Length; i++)
            {
                var isLast = i == files.Length - 1 && dirs.Length == 0;
                sb.AppendLine($"{indent}{(isLast ? "└── " : "├── ")}{Path.GetFileName(files[i])}");
            }

            for (var d = 0; d < dirs.Length; d++)
            {
                var isLastDir = d == dirs.Length - 1;
                BuildTree(dirs[d], sb, indent, isLastDir);
            }
        }

        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, params string[] args)
            => CommandResult.Fail($"'{Name}' has no extensions.");
    }
}
