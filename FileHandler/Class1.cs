//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;

//namespace FileHandler
//{
//    public static class FileHandleCopy
//    {
//        public static bool CopyFiles(string source, string target, bool overwrite)
//        {
//            ValidatePaths(source, target);

//            if (!Directory.Exists(source))
//            {
//                return false;
//            }

//            var sourceDir = new DirectoryInfo(source);
//            var targetDir = new DirectoryInfo(target);
//            if (!targetDir.Exists)
//            {
//                targetDir.Create();
//            }

//            var files = sourceDir.GetFiles();
//            foreach (var file in files)
//            {
//                try
//                {
//                    file.CopyTo(Path.Combine(target, file.Name), overwrite);
//                    FileHandlerRegister.SendStatus?.Invoke(nameof(CopyFiles), file.Name);
//                }
//                catch (Exception ex) when (ex is UnauthorizedAccessException or ArgumentException or IOException or NotSupportedException)
//                {
//                    FileHandlerRegister.AddError(nameof(CopyFiles), file.Name, ex);
//                    Trace.WriteLine(ex);
//                    return false;
//                }
//            }

//            foreach (var subDir in sourceDir.GetDirectories())
//            {
//                CopyFiles(subDir.FullName, Path.Combine(target, subDir.Name), overwrite);
//            }

//            return true;
//        }

//        public static bool CopyFiles(List<string> source, string target, bool overwrite)
//        {
//            if (source == null || source.Count == 0 || string.IsNullOrEmpty(target))
//            {
//                throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);
//            }

//            if (!Directory.Exists(target))
//            {
//                Directory.CreateDirectory(target);
//            }

//            foreach (var file in source)
//            {
//                try
//                {
//                    var fileInfo = new FileInfo(file);
//                    var relativePath = GetRelativePath(fileInfo.DirectoryName, source.First());
//                    var destinationPath = Path.Combine(target, relativePath, fileInfo.Name);

//                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? string.Empty);
//                    fileInfo.CopyTo(destinationPath, overwrite);

//                    FileHandlerRegister.SendStatus?.Invoke(nameof(CopyFiles), fileInfo.Name);
//                }
//                catch (Exception ex) when (ex is UnauthorizedAccessException or ArgumentException or IOException or NotSupportedException)
//                {
//                    FileHandlerRegister.AddError(nameof(CopyFiles), file, ex);
//                    Trace.WriteLine(ex);
//                    return false;
//                }
//            }

//            return true;
//        }

//        public static IList<string> CopyFiles(string source, string target)
//        {
//            ValidatePaths(source, target);

//            if (!Directory.Exists(source))
//            {
//                return null;
//            }

//            var sourceFiles = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
//            var targetFiles = Directory.GetFiles(target, "*", SearchOption.AllDirectories);
//            var filesToCopy = sourceFiles.Except(targetFiles).ToList();

//            if (filesToCopy.Count == 0)
//            {
//                return null;
//            }

//            if (CopyFiles(filesToCopy, target, false))
//            {
//                return filesToCopy;
//            }

//            return null;
//        }

//        public static bool CopyFilesReplaceIfNewer(string source, string target)
//        {
//            ValidatePaths(source, target);

//            if (!Directory.Exists(source))
//            {
//                return false;
//            }

//            var sourceDir = new DirectoryInfo(source);
//            var targetDir = new DirectoryInfo(target);
//            if (!targetDir.Exists)
//            {
//                targetDir.Create();
//            }

//            var files = sourceDir.GetFiles();
//            foreach (var file in files)
//            {
//                var targetPath = Path.Combine(target, file.Name);
//                if (!File.Exists(targetPath) || file.LastWriteTime > File.GetLastWriteTime(targetPath))
//                {
//                    try
//                    {
//                        file.CopyTo(targetPath, true);
//                        FileHandlerRegister.SendStatus?.Invoke(nameof(CopyFilesReplaceIfNewer), file.Name);
//                    }
//                    catch (Exception ex) when (ex is UnauthorizedAccessException or ArgumentException or IOException or NotSupportedException)
//                    {
//                        FileHandlerRegister.AddError(nameof(CopyFilesReplaceIfNewer), file.Name, ex);
//                        Trace.WriteLine(ex);
//                        return false;
//                    }
//                }
//            }

//            foreach (var subDir in sourceDir.GetDirectories())
//            {
//                CopyFilesReplaceIfNewer(subDir.FullName, Path.Combine(target, subDir.Name));
//            }

//            return true;
//        }

//        private static void ValidatePaths(string source, string target)
//        {
//            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
//            {
//                throw new FileHandlerException(FileHandlerResources.ErrorEmptyString);
//            }

//            if (source.Equals(target, StringComparison.OrdinalIgnoreCase))
//            {
//                throw new FileHandlerException(FileHandlerResources.ErrorEqualPath);
//            }
//        }

//        private static string GetRelativePath(string fromPath, string toPath)
//        {
//            var fromUri = new Uri(fromPath);
//            var toUri = new Uri(toPath);

//            var relativeUri = fromUri.MakeRelativeUri(toUri);
//            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

//            return relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
//        }
//    }
//}
