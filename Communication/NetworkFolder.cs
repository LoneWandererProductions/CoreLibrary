using System;
using System.IO;

namespace Communication
{
    internal class NetworkFolder
    {
        public void SearchNetworkFolder(string path)
        {
            // Create a new FileSystemWatcher instance
            FileSystemWatcher watcher = new FileSystemWatcher(path)
            {
                // Set the FileSystemWatcher properties
                NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName,
                IncludeSubdirectories = true
            };

            // Subscribe to the Deleted event
            watcher.Deleted += OnDeleted;

            // Start monitoring
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Monitoring folder for deletions. Press any key to exit.");
            Console.ReadKey();

            // Stop monitoring
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            // Handle the deletion event
            Console.WriteLine($"File or folder '{e.FullPath}' was deleted.");
        }

    }
}
