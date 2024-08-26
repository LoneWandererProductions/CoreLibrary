/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/TextureGenerator.cs
 * PURPOSE:     Implementation of Interface of ITextureGenerator, provides textures
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * Sources:     https://lodev.org/cgtutor/randomnoise.html
 */

using System.Drawing;

namespace Imaging
{
    /// <summary>
    /// Main Entry Class that will handle all things related to textures
    /// </summary>
    /// <seealso cref="Imaging.ITextureGenerator" />
    public class TextureGenerator : ITextureGenerator
    {
        ///<inheritdoc/>
        /// <summary>
        /// Generates the noise bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="useSmoothNoise">if set to <c>true</c> [use smooth noise].</param>
        /// <param name="useTurbulence">if set to <c>true</c> [use turbulence].</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <returns>Texture Bitmap</returns>
        public Bitmap GenerateNoiseBitmap(int width, int height, int minValue = 0, int maxValue = 255, int alpha = 255, bool useSmoothNoise = false, bool useTurbulence = false, double turbulenceSize = 64)
        {
            return Texture.GenerateNoiseBitmap(width, height, minValue, maxValue, alpha, useSmoothNoise, useTurbulence, turbulenceSize);
        }

        ///<inheritdoc/>
        /// <summary>
        /// Generates the clouds bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <returns>Texture Bitmap</returns>
        public Bitmap GenerateCloudsBitmap(int width, int height, int minValue = 0, int maxValue = 255, int alpha = 255, double turbulenceSize = 64)
        {
            return Texture.GenerateCloudsBitmap(width, height, minValue, maxValue, alpha, turbulenceSize);
        }

        ///<inheritdoc/>
        /// <summary>
        /// Generates the marble bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="xPeriod">The x period.</param>
        /// <param name="yPeriod">The y period.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="turbPower">The turb power.</param>
        /// <param name="turbSize">Size of the turb.</param>
        /// <param name="baseColor">Color of the base.</param>
        /// <returns>Texture Bitmap</returns>
        public Bitmap GenerateMarbleBitmap(int width, int height,  double xPeriod = 5, double yPeriod = 10, int alpha =255, double turbPower = 5, double turbSize = 32, Color baseColor = default)
        {
            return Texture.GenerateMarbleBitmap(width, height, xPeriod, yPeriod, alpha, turbPower, turbSize, baseColor);
        }

        ///<inheritdoc/>
        /// <summary>
        /// Generates the wave bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="xyPeriod">The xy period.</param>
        /// <param name="turbPower">The turb power.</param>
        /// <param name="turbSize">Size of the turb.</param>
        /// <returns>Texture Bitmap</returns>
        public Bitmap GenerateWaveBitmap(int width, int height,int alpha = 255, double xyPeriod = 12, double turbPower = 0.1, double turbSize = 32)
        {
            return Texture.GenerateWaveBitmap(width, height, alpha, xyPeriod, turbPower, turbSize);
        }

        ///<inheritdoc/>
        /// <summary>
        /// Generates the wood bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="xyPeriod">The xy period.</param>
        /// <param name="turbPower">The turb power.</param>
        /// <param name="turbSize">Size of the turb.</param>
        /// <param name="baseColor">Color of the base.</param>
        /// <returns>Texture Bitmap</returns>
        public Bitmap GenerateWoodBitmap(int width, int height, int alpha = 255, double xyPeriod = 12, double turbPower = 0.1, double turbSize = 32, Color baseColor = default)
        {
            return Texture.GenerateWoodBitmap(width, height, alpha, xyPeriod, turbPower, turbSize, baseColor);
        }

        ///<inheritdoc/>
        /// <summary>
        /// Generates the texture.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="shapeParams">The shape parameters.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <param name="baseColor">Color of the base.</param>
        /// <returns>Texture Bitmap</returns>
        public Bitmap GenerateTexture(int width, int height, TextureType filter, TextureShape shape, object shapeParams = null, int minValue = 0, int maxValue = 255, int alpha = 255, double turbulenceSize = 64, Color? baseColor = null)
        {
            return TextureAreas.GenerateTexture(width, height, filter, shape, shapeParams, minValue, maxValue, alpha, turbulenceSize, baseColor);
        }
    }
}
