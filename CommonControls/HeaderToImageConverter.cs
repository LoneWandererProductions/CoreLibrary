/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/HeaderToImageConverter.cs
 * PURPOSE:     Needed for the FolderView Control, and FolderBrowser, converts Image into the tree Control
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal, same as usual we can not make it internal because we bind it to the window

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Imaging;

namespace CommonControls
{
    /// <inheritdoc />
    /// <summary>
    ///     here we build in our Image Converter
    /// </summary>
    [ValueConversion(typeof(string), typeof(bool))]
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
        /// <param name="value">Value of Object, directly from the Wpf Form</param>
        /// <param name="targetType">Type of Object</param>
        /// <param name="parameter">Parameter</param>
        /// <param name="culture">CultureInfo</param>
        /// <returns>Image</returns>
        [return: MaybeNull]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;

            var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (root == null)
            {
                return null;
            }

            var drive = Path.Combine(root, ComCtlResources.DriveImage);
            var folder = Path.Combine(root, ComCtlResources.FolderImage);

            var source = new BitmapImage();

            if (string.IsNullOrEmpty(str))
            {
                return source;
            }

            if (str.Contains(ComCtlResources.PathElement))
            {
                if (File.Exists(drive))
                {
                    return ImageStream.GetBitmapImageFileStream(drive);
                }
            }
            else
            {
                if (File.Exists(folder))
                {
                    return ImageStream.GetBitmapImageFileStream(folder);
                }
            }

            return source;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Try to Convert
        /// </summary>
        /// <param name="value">Value of Object</param>
        /// <param name="targetType">Type of Object</param>
        /// <param name="parameter">Parameter</param>
        /// <param name="culture">CultureInfo</param>
        /// <returns>Catch error</returns>
        /// <exception cref="NotSupportedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(ComCtlResources.ErrorConversion);
        }
    }
}
