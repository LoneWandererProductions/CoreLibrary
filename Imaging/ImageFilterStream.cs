/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/ImageFilterStream.cs
 * PURPOSE:     Separate out all Filter Operations
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

// TODO add:
// Prewitt
// Roberts Cross
// Laplacian
// Laplacian of Gaussain
// Anisotropic Kuwahara


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Imaging
{
    /// <summary>
    /// Here we handle the grunt work for all image filters
    /// </summary>
    internal static class ImageFilterStream
    {
        /// <summary>
        ///     Pixelate the specified input image.
        /// </summary>
        /// <param name="image">The input image.</param>
        /// <param name="stepWidth">Width of the step.</param>
        /// <returns>Pixelated Image</returns>
        internal static Bitmap Pixelate(Image image, int stepWidth)
        {
            if (image == null)
            {
                var innerException = new ArgumentNullException(string.Concat(nameof(Pixelate),
                    ImagingResources.Spacing, nameof(image)));
                throw new ArgumentNullException(ImagingResources.ErrorWrongParameters, innerException);
            }

            // Create a new bitmap to store the processed image
            var dbm = new DirectBitmap(image);
            // Create a new bitmap to store the processed image
            var processedImage = new Bitmap(dbm.Width, dbm.Height);


            // Iterate over the image with the specified step width
            for (var y = 0; y < dbm.Height; y += stepWidth)
            for (var x = 0; x < dbm.Width; x += stepWidth)
            {
                // Get the color of the current rectangle
                var averageColor = ImageStream.GetAverageColor(image, x, y, stepWidth, stepWidth);

                using var g = Graphics.FromImage(processedImage);
                using var brush = new SolidBrush(averageColor);
                g.FillRectangle(brush, x, y, stepWidth, stepWidth);
            }

            return processedImage;
        }

        /// <summary>
        ///     Applies the filter.
        /// </summary>
        /// <param name="sourceBitmap">The source bitmap.</param>
        /// <param name="filterMatrix">
        ///     The filter matrix.
        ///     Matrix Definition: The convolution matrix is typically a 2D array of numbers (weights) that defines how each pixel
        ///     in the image should be altered based on its neighboring pixels. Common sizes are 3x3, 5x5, or 7x7.
        ///     Placement: Place the center of the convolution matrix on the target pixel in the image.
        ///     Neighborhood Calculation: Multiply the value of each pixel in the neighborhood by the corresponding value in the
        ///     convolution matrix.
        ///     Summation: Sum all these products.
        ///     Normalization: Often, the result is normalized (e.g., dividing by the sum of the matrix values) to ensure that
        ///     pixel values remain within a valid range.
        ///     Pixel Update: The resulting value is assigned to the target pixel in the output image.
        ///     Matrix Size: The size of the matrix affects the area of the image that influences each output pixel. For example:
        ///     3x3 Matrix: Considers the pixel itself and its immediate 8 neighbors.
        ///     5x5 Matrix: Considers a larger area, including 24 neighbors and the pixel itself.
        /// </param>
        /// <param name="factor">The factor.</param>
        /// <param name="bias">The bias.</param>
        /// <returns>Image with applied filter</returns>
        internal static Bitmap ApplyFilter(Image sourceBitmap, double[,] filterMatrix, double factor = 1.0,
            double bias = 0.0)
        {
            // Initialize DirectBitmap instances
            var source = new DirectBitmap(sourceBitmap);
            var result = new DirectBitmap(source.Width, source.Height);

            var filterWidth = filterMatrix.GetLength(1);
            var filterHeight = filterMatrix.GetLength(0);
            var filterOffset = filterWidth / 2;

            // Prepare a list to store the pixels to set in bulk using SIMD
            var pixelsToSet = new List<(int x, int y, Color color)>();

            for (var y = filterOffset; y < source.Height - filterOffset; y++)
            {
                for (var x = filterOffset; x < source.Width - filterOffset; x++)
                {
                    double blue = 0.0, green = 0.0, red = 0.0;

                    for (var filterY = 0; filterY < filterHeight; filterY++)
                    {
                        for (var filterX = 0; filterX < filterWidth; filterX++)
                        {
                            var imageX = x + (filterX - filterOffset);
                            var imageY = y + (filterY - filterOffset);

                            // Check bounds to prevent out-of-bounds access
                            if (imageX < 0 || imageX >= source.Width || imageY < 0 || imageY >= source.Height)
                            {
                                continue;
                            }

                            var pixelColor = source.GetPixel(imageX, imageY);

                            blue += pixelColor.B * filterMatrix[filterY, filterX];
                            green += pixelColor.G * filterMatrix[filterY, filterX];
                            red += pixelColor.R * filterMatrix[filterY, filterX];
                        }
                    }

                    var newBlue = Math.Min(Math.Max((int)((factor * blue) + bias), 0), 255);
                    var newGreen = Math.Min(Math.Max((int)((factor * green) + bias), 0), 255);
                    var newRed = Math.Min(Math.Max((int)((factor * red) + bias), 0), 255);

                    // Instead of setting the pixel immediately, add it to the list
                    pixelsToSet.Add((x, y, Color.FromArgb(newRed, newGreen, newBlue)));
                }
            }

            // Use SIMD to set all the pixels in bulk
            try
            {
                result.SetPixelsSimd(pixelsToSet);

                return result.Bitmap;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error setting pixels: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        ///     Applies the Sobel.
        /// </summary>
        /// <param name="originalImage">The original image.</param>
        /// <returns>Contour of an Image</returns>
        internal static Bitmap ApplySobel(Bitmap originalImage)
        {
            // Convert the original image to greyscale
            var greyscaleImage = ImageStream.FilterImage(originalImage, ImageFilters.GrayScale);

            // Create a new bitmap to store the result of Sobel operator
            var resultImage = new Bitmap(greyscaleImage.Width, greyscaleImage.Height);

            // Sobel masks for gradient calculation
            int[,] sobelX = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] sobelY = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            // Prepare a list to store the pixels to set in bulk using SIMD
            var pixelsToSet = new List<(int x, int y, Color color)>();

            var dbmBase = new DirectBitmap(greyscaleImage);
            var dbmResult = new DirectBitmap(resultImage);

            // Apply Sobel operator to each pixel in the image
            for (var x = 1; x < greyscaleImage.Width - 1; x++)
            for (var y = 1; y < greyscaleImage.Height - 1; y++)
            {
                var gx = 0;
                var gy = 0;

                // Convolve the image with the Sobel masks
                for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                {
                    var pixel = dbmBase.GetPixel(x + i, y + j);
                    int grayValue = pixel.R; // Since it's a greyscale image, R=G=B
                    gx += sobelX[i + 1, j + 1] * grayValue;
                    gy += sobelY[i + 1, j + 1] * grayValue;
                }

                // Calculate gradient magnitude
                var magnitude = (int)Math.Sqrt((gx * gx) + (gy * gy));

                // Normalize the magnitude to fit within the range of 0-255
                magnitude = (int)(magnitude / Math.Sqrt(2)); // Divide by sqrt(2) for normalization
                magnitude = Math.Min(255, Math.Max(0, magnitude));

                // Instead of setting the pixel immediately, add it to the list
                pixelsToSet.Add((x, y, Color.FromArgb(magnitude, magnitude, magnitude)));
            }

            // Use SIMD to set all the pixels in bulk
            try
            {
                dbmResult.SetPixelsSimd(pixelsToSet);
                dbmBase.Dispose();

                return dbmResult.Bitmap;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error setting pixels: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Applies the difference of gaussians.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>Filtered Image</returns>
        internal static Bitmap ApplyDifferenceOfGaussians(Image image)
        {
            // Gaussian blur with small sigma
            double[,] gaussianBlurSmall = GenerateGaussianKernel(1.0, 5);

            // Gaussian blur with larger sigma
            double[,] gaussianBlurLarge = GenerateGaussianKernel(2.0, 5);

            // Apply both Gaussian blurs to the image
            Bitmap blurredSmall = ApplyFilter(image, gaussianBlurSmall, 1.0 / 16.0);
            Bitmap blurredLarge = ApplyFilter(image, gaussianBlurLarge, 1.0 / 16.0);

            // Subtract the two blurred images to get the DoG result
            return SubtractImages(blurredSmall, blurredLarge);
        }

        /// <summary>
        /// Applies the crosshatch.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>Filtered Image</returns>
        internal static Bitmap ApplyCrosshatch(Image image)
        {
            // Define directional edge detection kernels for crosshatching
            double[,] kernel45Degrees = {
                { -1, -1, 2 },
                { -1, 2, -1 },
                { 2, -1, -1 }
            };

            double[,] kernel135Degrees = {
                { 2, -1, -1 },
                { -1, 2, -1 },
                { -1, -1, 2 }
            };

            // Apply the 45-degree and 135-degree filters
            Bitmap hatch45 = ApplyFilter(image, kernel45Degrees, 1.0);
            Bitmap hatch135 = ApplyFilter(image, kernel135Degrees, 1.0);

            // Combine the two hatching directions
            return CombineImages(hatch45, hatch135);
        }

        private static Bitmap ApplyAnisotropicKuwahara(Bitmap image, int baseWindowSize)
        {
            var dbmBase = new DirectBitmap(image);
            var result = new DirectBitmap(image.Width, image.Height);
            int halfBaseWindow = baseWindowSize / 2;

            for (int y = halfBaseWindow; y < dbmBase.Height - halfBaseWindow; y++)
            {
                for (int x = halfBaseWindow; x < dbmBase.Width - halfBaseWindow; x++)
                {
                    // Determine region size and shape based on local image characteristics
                    int regionWidth, regionHeight;
                    DetermineRegionSizeAndShape(dbmBase, x, y, halfBaseWindow, out regionWidth, out regionHeight);

                    var bestColor = ComputeBestRegionColor(dbmBase, x, y, regionWidth, regionHeight);

                    // Set the pixel value to the mean of the best region
                    result.SetPixel(x, y, bestColor);
                }
            }

            return result.Bitmap;
        }

        private static Bitmap ApplyFloydSteinbergDithering(Bitmap image)
        {
            var dbmBase = new DirectBitmap(image);
            var result = new DirectBitmap(image.Width, image.Height);

            // Convert to grayscale
            var grayBitmap = ImageStream.FilterImage(image, ImageFilters.GrayScale);

            // Define the color palette for dithering
            var palette = new List<Color>
            {
                Color.Black,
                Color.White
            };

            // Floyd-Steinberg dithering matrix
            int[,] ditherMatrix =
            {
                { 0, 0, 7 },
                { 3, 5, 1 }
            };

            // Apply dithering
            for (int y = 0; y < grayBitmap.Height; y++)
            {
                for (int x = 0; x < grayBitmap.Width; x++)
                {
                    // Get the original grayscale pixel value
                    var oldColor = grayBitmap.GetPixel(x, y);
                    var oldIntensity = oldColor.R; // Since it's grayscale, R=G=B

                    // Find the closest color in the palette
                    var newColor = GetNearestColor(oldIntensity, palette);
                    result.SetPixel(x, y, newColor);

                    // Calculate the quantization error
                    var error = oldIntensity - newColor.R;

                    // Distribute the error to neighboring pixels
                    DistributeError(dbmBase, x, y, error, ditherMatrix);
                }
            }

            return result.Bitmap;
        }

        private static Bitmap ApplySupersamplingAntialiasing(Bitmap image, int scale)
        {
            // Create a higher-resolution bitmap for supersampling
            var scaledBitmap = new Bitmap(image.Width * scale, image.Height * scale);
            using (var g = Graphics.FromImage(scaledBitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, scaledBitmap.Width, scaledBitmap.Height);
            }

            // Downsample the high-resolution bitmap to the original size
            var resultBitmap = new DirectBitmap(image.Width, image.Height);
            var scaledDBM = new DirectBitmap(scaledBitmap);

            for (int y = 0; y < resultBitmap.Height; y++)
            {
                for (int x = 0; x < resultBitmap.Width; x++)
                {
                    int startX = x * scale;
                    int startY = y * scale;

                    // Average the color values of the sample region
                    int sumR = 0, sumG = 0, sumB = 0;
                    for (int dy = 0; dy < scale; dy++)
                    {
                        for (int dx = 0; dx < scale; dx++)
                        {
                            var pixelColor = scaledDBM.GetPixel(startX + dx, startY + dy);
                            sumR += pixelColor.R;
                            sumG += pixelColor.G;
                            sumB += pixelColor.B;
                        }
                    }

                    int avgR = sumR / (scale * scale);
                    int avgG = sumG / (scale * scale);
                    int avgB = sumB / (scale * scale);

                    resultBitmap.SetPixel(x, y, Color.FromArgb(avgR, avgG, avgB));
                }
            }

            return resultBitmap.Bitmap;
        }

        private static Bitmap ApplyPostProcessingAntialiasing(Bitmap image, double sigma = 1.0)
        {
            // Convert the image to DirectBitmap
            var dbmBase = new DirectBitmap(image);

            // Generate a Gaussian kernel
            var gaussianKernel = GenerateGaussianKernel(sigma, 5);

            // Apply the Gaussian blur filter
            return ImageFilterStream.ApplyFilter(dbmBase.Bitmap, gaussianKernel, 1.0);
        }

        private static void DetermineRegionSizeAndShape(DirectBitmap dbmBase, int x, int y, int baseHalfWindow, out int regionWidth, out int regionHeight)
        {
            // Placeholder logic to determine region size and shape
            // This is an example, you may need to adjust this based on your specific needs
            // For simplicity, let's assume a fixed size for regions, but in practice, this should be adaptive
            regionWidth = baseHalfWindow * 2;
            regionHeight = baseHalfWindow * 2;
        }

        private static Color ComputeBestRegionColor(DirectBitmap dbmBase, int x, int y, int regionWidth, int regionHeight)
        {
            var bestColor = Color.Black;
            double bestVariance = double.MaxValue;

            // Define regions within the current window
            var regions = DefineRegions(x, y, regionWidth, regionHeight);

            foreach (var region in regions)
            {
                var (pixels, meanColor) = GetRegionPixelsAndMeanColor(dbmBase, region);

                // Calculate variance for the current region
                double variance = CalculateVariance(pixels, meanColor);

                if (variance < bestVariance)
                {
                    bestVariance = variance;
                    bestColor = meanColor;
                }
            }

            return bestColor;
        }

        private static IEnumerable<Rectangle> DefineRegions(int centerX, int centerY, int width, int height)
        {
            // Example logic to generate multiple regions
            // This is a placeholder and should be replaced with logic to adapt the shape and size based on local image characteristics
            var regions = new List<Rectangle>
    {
        new Rectangle(centerX - width / 2, centerY - height / 2, width, height),
        // Add more regions if needed, e.g., smaller regions, different orientations
    };

            return regions;
        }

        private static (List<Color> Pixels, Color Mean) GetRegionPixelsAndMeanColor(DirectBitmap dbmBase, Rectangle region)
        {
            var pixels = new List<Color>();
            int rSum = 0, gSum = 0, bSum = 0;
            int count = 0;

            for (int y = region.Top; y < region.Bottom; y++)
            {
                for (int x = region.Left; x < region.Right; x++)
                {
                    var pixel = dbmBase.GetPixel(x, y);
                    pixels.Add(pixel);
                    rSum += pixel.R;
                    gSum += pixel.G;
                    bSum += pixel.B;
                    count++;
                }
            }

            var meanColor = Color.FromArgb(rSum / count, gSum / count, bSum / count);
            return (pixels, meanColor);
        }

        private static double CalculateVariance(List<Color> pixels, Color meanColor)
        {
            double variance = 0.0;

            foreach (var pixel in pixels)
            {
                variance += Math.Pow(pixel.R - meanColor.R, 2) + Math.Pow(pixel.G - meanColor.G, 2) + Math.Pow(pixel.B - meanColor.B, 2);
            }

            return variance / pixels.Count;
        }

        private static Color GetNearestColor(int intensity, List<Color> palette)
        {
            Color nearestColor = palette[0];
            int minDifference = Math.Abs(intensity - nearestColor.R);

            foreach (var color in palette)
            {
                var difference = Math.Abs(intensity - color.R);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    nearestColor = color;
                }
            }

            return nearestColor;
        }

        private static void DistributeError(DirectBitmap dbmBase, int x, int y, int error, int[,] ditherMatrix)
        {
            int matrixHeight = ditherMatrix.GetLength(0);
            int matrixWidth = ditherMatrix.GetLength(1);

            for (int dy = 0; dy < matrixHeight; dy++)
            {
                for (int dx = 0; dx < matrixWidth; dx++)
                {
                    int nx = x + dx - 1;
                    int ny = y + dy;

                    if (nx >= 0 && nx < dbmBase.Width && ny >= 0 && ny < dbmBase.Height)
                    {
                        var pixel = dbmBase.GetPixel(nx, ny);
                        var oldIntensity = pixel.R; // Since it's grayscale, R=G=B
                        var newIntensity = oldIntensity + (error * ditherMatrix[dy, dx]) / 16;
                        newIntensity = Math.Max(0, Math.Min(255, newIntensity));
                        var newColor = Color.FromArgb(newIntensity, newIntensity, newIntensity);
                        dbmBase.SetPixel(nx, ny, newColor);
                    }
                }
            }
        }


        /// <summary>
        /// Subtracts the images.
        /// </summary>
        /// <param name="imgOne">The img1.</param>
        /// <param name="imgTwo">The img2.</param>
        /// <returns>Filtered Image</returns>
        private static Bitmap SubtractImages(Image imgOne, Image imgTwo)
        {
            var result = new DirectBitmap(imgOne.Width, imgOne.Height);
            // Prepare a list to store the pixels to set in bulk using SIMD
            var pixelsToSet = new List<(int x, int y, Color color)>();

            var dbmOne = new DirectBitmap(imgOne);
            var dbmTwo = new DirectBitmap(imgTwo);

            for (var y = 0; y < dbmOne.Height; y++)
            {
                for (var x = 0; x < dbmOne.Width; x++)
                {
                    var color1 = dbmOne.GetPixel(x, y);
                    var color2 = dbmTwo.GetPixel(x, y);

                    var r = Math.Max(0, color1.R - color2.R);
                    var g = Math.Max(0, color1.G - color2.G);
                    var b = Math.Max(0, color1.B - color2.B);

                    // Instead of setting the pixel immediately, add it to the list
                    pixelsToSet.Add((x, y, Color.FromArgb(r, g, b)));
                }
            }

            // Use SIMD to set all the pixels in bulk
            try
            {
                result.SetPixelsSimd(pixelsToSet);

                return result.Bitmap;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error setting pixels: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Generates the gaussian kernel.
        /// </summary>
        /// <param name="sigma">The sigma.</param>
        /// <param name="size">The size.</param>
        /// <returns>Filtered Image</returns>
        private static double[,] GenerateGaussianKernel(double sigma, int size)
        {
            double[,] kernel = new double[size, size];
            double mean = size / 2.0;
            double sum = 0.0;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    kernel[y, x] = Math.Exp(-0.5 * (Math.Pow((x - mean) / sigma, 2.0) + Math.Pow((y - mean) / sigma, 2.0)))
                                   / (2 * Math.PI * sigma * sigma);
                    sum += kernel[y, x];
                }
            }

            // Normalize the kernel
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    kernel[y, x] /= sum;
                }
            }

            return kernel;
        }

        /// <summary>
        /// Combines the images.
        /// </summary>
        /// <param name="imgOne">The first image.</param>
        /// <param name="imgTwo">The second image.</param>
        /// <returns>Filtered Image</returns>
        private static Bitmap CombineImages(Image imgOne, Image imgTwo)
        {
            var result = new DirectBitmap(imgOne.Width, imgOne.Height);

            // Prepare a list to store the pixels to set in bulk using SIMD
            var pixelsToSet = new List<(int x, int y, Color color)>();

            var dbmOne = new DirectBitmap(imgOne);
            var dbmTwo = new DirectBitmap(imgTwo);

            for (int y = 0; y < dbmOne.Height; y++)
            {
                for (int x = 0; x < dbmOne.Width; x++)
                {
                    Color color1 = dbmOne.GetPixel(x, y);
                    Color color2 = dbmTwo.GetPixel(x, y);

                    int r = Math.Min(255, color1.R + color2.R);
                    int g = Math.Min(255, color1.G + color2.G);
                    int b = Math.Min(255, color1.B + color2.B);

                    // Instead of setting the pixel immediately, add it to the list
                    pixelsToSet.Add((x, y, Color.FromArgb(r, g, b)));
                }
            }

            // Use SIMD to set all the pixels in bulk
            try
            {
                result.SetPixelsSimd(pixelsToSet);

                return result.Bitmap;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error setting pixels: {ex.Message}");
            }

            return null;
        }
    }
}
