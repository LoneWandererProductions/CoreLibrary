/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Cifs
 * FILE:        CifResources.cs
 * PURPOSE:     String Resources
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Imaging.Cifs
{
    /// <summary>
    ///     The com Control resources class.
    /// </summary>
    public static class CifResources
    {
        // General Messages

        /// <summary>
        ///     The error message displayed when a file is not found. Value: "File not found."
        /// </summary>
        internal const string FileNotFoundMessage = "File not found.";

        /// <summary>
        ///     The message displayed when skipping padding or a sub-block terminator. Value: "Skipping padding or sub-block
        ///     terminator (0x00)"
        /// </summary>
        internal const string SkipPaddingMessage = "Skipping padding or sub-block terminator (0x00)";

        /// <summary>
        ///     The description for image frames. Value: "Image Frame"
        /// </summary>
        internal const string ImageFrameDescription = "Image Frame";

        // Formatting

        /// <summary>
        ///     The message format for processing a block. Example: "Processing block: 0x{0:X2}"
        /// </summary>
        internal const string ProcessingBlockMessage = "Processing block: 0x{0:X2}";

        /// <summary>
        ///     The message format for an unknown block being encountered. Example: "Unknown block encountered: 0x{0:X2}.
        ///     Skipping."
        /// </summary>
        internal const string UnknownBlockMessage = "Unknown block encountered: 0x{0:X2}. Skipping.";

        /// <summary>
        ///     The message format for skipping an unknown block. Example: "Skipping unknown block: 0x{0:X2}"
        /// </summary>
        internal const string SkipUnknownBlockMessage = "Skipping unknown block: 0x{0:X2}";

        /// <summary>
        ///     The message format for skipping an extension block. Example: "Skipping extension block of size: {0}"
        /// </summary>
        internal const string SkipExtensionBlockMessage = "Skipping extension block of size: {0}";

        /// <summary>
        ///     The error missing file (const). Value: "File not Found: ".
        /// </summary>
        internal const string ErrorFileNotFound = "File not Found: ";

        /// <summary>
        ///     Error, wrong parameters (const). Value: "Wrong Arguments provided".
        /// </summary>
        internal const string ErrorWrongParameters = "Wrong Arguments provided: ";

        /// <summary>
        ///     The Spacing (const). Value:  " : ".
        /// </summary>
        internal const string Spacing = " : ";

        /// <summary>
        ///     The Separator (const). Value:  ','.
        /// </summary>
        internal const char Separator = ',';

        /// <summary>
        ///     The Interval Splitter (const). Value: "-".
        /// </summary>
        internal const string IntervalSplitter = "-";

        /// <summary>
        ///     Separator (const). Value: " , ".
        /// </summary>
        internal const string Indexer = " , ";

        /// <summary>
        ///     Color string (const). Value: "Color: ".
        /// </summary>
        internal const string Color = "Color: ";

        /// <summary>
        ///     The flag that indicates that image is not compressed (const). Value:  "0".
        /// </summary>
        internal const string CifUnCompressed = "0";

        /// <summary>
        ///     The flag that indicates if image is compressed (const). Value:  "1".
        /// </summary>
        internal const string CifCompressed = "1";

        /// <summary>
        ///     The cif Separator used for compression (const). Value:  "-".
        /// </summary>
        internal const string CifSeparator = "-";

        /// <summary>
        ///     The error, interface is null (const). Value: "Error: Interface is Null."
        /// </summary>
        internal const string ErrorInterface = "Error: Interface is Null.";

        /// <summary>
        ///     The error, image is null (const). Value: "Error: Image is Null."
        /// </summary>
        internal const string ErrorImage = "Error: Image is Null.";

        /// <summary>
        ///     The error, Radius is smaller null (const). Value: "Error: Radius cannot be negative."
        /// </summary>
        internal const string ErrorRadius = "Error: Radius cannot be negative.";

        /// <summary>
        ///     The error out of bounds (const). Value: "Error: Point is outside the bounds of the image."
        /// </summary>
        internal const string ErrorOutOfBounds = "Error: Point is outside the bounds of the image.";

        /// <summary>
        ///     The error Invalid Operation (const). Value: "Error: Bits array is not properly initialized."
        /// </summary>
        internal const string ErrorInvalidOperation = "Error: Bits array is not properly initialized.";

        /// <summary>
        ///     The error for Pixel Operation (const). Value: "Error setting pixels: "
        /// </summary>
        internal const string ErrorPixel = "Error setting pixels: ";

        /// <summary>
        ///     The error, Path is null (const). Value: "Error: Path is Null."
        /// </summary>
        internal const string ErrorPath = "Error: Path is Null.";

        /// <summary>
        ///     The error, could not load Setting (const). Value: "Error loading Configuration:"
        /// </summary>
        internal const string ErrorLoadSettings = "Error loading Configuration:";

        /// <summary>
        ///     The error with shape polygon (const). Value: "Error loading Configuration:"
        /// </summary>
        internal const string ErrorWithShapePolygon = "Error loading Configuration:";

        /// <summary>
        ///     The invalid dimensions (const). Value: "Width and height must be positive integers."
        /// </summary>
        public const string InvalidDimensions = "Width and height must be positive integers.";

        /// <summary>
        ///     The image settings null (const). Value:  "Image settings cannot be null."
        /// </summary>
        public const string ImageSettingsNull = "Image settings cannot be null.";

        /// <summary>
        ///     The invalid texture settings (const). Value:  "Invalid texture settings."
        /// </summary>
        public const string InvalidTextureSettings = "Invalid texture settings.";

        /// <summary>
        ///     The unsupported texture (const). Value: "Unsupported texture type."
        /// </summary>
        public const string UnsupportedTexture = "Unsupported texture type.";

        /// <summary>
        ///     The invalid polygon parameters (const). Value:"Invalid shape parameters for polygon mask."
        /// </summary>
        public const string InvalidPolygonParams = "Invalid shape parameters for polygon mask.";

        /// <summary>
        ///     The unsupported shape (const). Value:  "Unsupported shape type."
        /// </summary>
        public const string UnsupportedShape = "Unsupported shape type.";

        /// <summary>
        ///     The error, min/ max value exceeded 0 or 255 (const). Value: "Error: minValue and maxValue must be between 0 and
        ///     255, and minValue must not be greater than maxValue."
        /// </summary>
        internal const string ErrorColorRange =
            "Error: minValue and maxValue must be between 0 and 255, and minValue must not be greater than maxValue.";

        /// <summary>
        ///     The exception null (const). Value: "The exception object cannot be null."
        /// </summary>
        public const string ExceptionNull = "The exception object cannot be null.";

        /// <summary>
        ///     The exception type (const). Value: "Exception Type: {0}"
        /// </summary>
        public const string ExceptionType = "Exception Type: {0}";

        /// <summary>
        ///     The exception message (const). Value: "Message: {0}"
        /// </summary>
        public const string ExceptionMessage = "Message: {0}";

        /// <summary>
        ///     The exception stack trace (const). Value: "Stack Trace: {0}"
        /// </summary>
        public const string ExceptionStackTrace = "Stack Trace: {0}";

        /// <summary>
        ///     The general processing error (const). Value: "An error occurred while processing the image."
        /// </summary>
        public const string GeneralProcessingError = "An error occurred while processing the image.";
    }
}
