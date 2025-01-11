/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        Communication/NetworkFolder.cs
 * PURPOSE:     File Watcher, that observes some File changes.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */


using System;
using System.IO;
using System.Threading.Tasks;

namespace Communication
{
    internal class FileObserver
    {
        public async Task SearchNetworkFolderAsync(string path)
        {
            var watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName,
                IncludeSubdirectories = true,
                InternalBufferSize = 64 * 1024 // Increase buffer size to handle more events
            };

            watcher.Created += OnCreated;
            watcher.Changed += OnChanged;
            watcher.Deleted += OnDeleted;
            watcher.Error += OnError;

            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Monitoring folder for changes. Press any key to stop.");
            await Task.Run(() => Console.ReadKey()); // Run in background to avoid blocking

            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File or folder '{e.FullPath}' was created.");
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File or folder '{e.FullPath}' was modified.");
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File or folder '{e.FullPath}' was deleted.");
        }

        private static void OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("Error occurred: " + e.GetException().Message);
        }
    }
}
