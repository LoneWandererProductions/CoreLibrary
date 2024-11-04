/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonDialogs
 * FILE:        CommonDialogs/HeaderToImageConverter.cs
 * PURPOSE:     Needed for the FolderView Control, and FolderBrowser, converts Image into the tree Control
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal, same as usual we can not make it internal because we bind it to the window

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Imaging;

namespace CommonDialogs
{
    /// <inheritdoc />
    /// <summary>
    ///     Here we build in our Image Converter
    /// </summary>
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public sealed class HeaderToImageConverter : IValueConverter
    {
        /// <summary>
        ///     The instance (readonly). Value: new HeaderToImageConverter().
        /// </summary>
        public static readonly HeaderToImageConverter Instance = new();

        /// <inheritdoc />
        /// <summary>
        ///     Return Image view
        /// </summary>
        /// <param name="value">Value of Object, directly from the WPF Form</param>
        /// <param name="targetType">Type of Object</param>
        /// <param name="parameter">Parameter</param>
        /// <param name="culture">CultureInfo</param>
        /// <returns>Image</returns>
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string str || string.IsNullOrEmpty(str)) return null;

            // Get the directory where the executing assembly is located
            var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (root == null) return null;

            // Construct the full paths to the images
            var driveImagePath = Path.Combine(root, ComCtlResources.DriveImage);
            var folderImagePath = Path.Combine(root, ComCtlResources.FolderImage);

            // Select the appropriate image based on the path string
            var imagePath = str.Contains(ComCtlResources.PathElement) && File.Exists(driveImagePath)
                ? driveImagePath
                : File.Exists(folderImagePath)
                    ? folderImagePath
                    : null;

            // Return the image if it exists
            return imagePath != null ? ImageStreamMedia.GetBitmapImageFileStream(imagePath) : null;
        }

        /// <inheritdoc />
        /// <summary>
        ///     ConvertBack is not supported in this converter.
        /// </summary>
        /// <param name="value">Value of Object</param>
        /// <param name="targetType">Type of Object</param>
        /// <param name="parameter">Parameter</param>
        /// <param name="culture">CultureInfo</param>
        /// <returns>Throws NotSupportedException</returns>
        /// <exception cref="NotSupportedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(ComCtlResources.ErrorConversion);
        }
    }
}