/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/TextureConfig.cs
 * PURPOSE:     Basic stuff for generating textures, this class is used for finetuning
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * Sources:     https://lodev.org/cgtutor/randomnoise.html
 */

using System.Drawing;

namespace Imaging
{
    /// <summary>
    ///     Attributes for our texture generators
    /// </summary>
    public class TextureConfig
    {
        public int MinValue { get; init; } = 0;
        public int MaxValue { get; init; } = 255;
        public int Alpha { get; init; } = 255;
        public double TurbulenceSize { get; init; } = 64;
        public Color BaseColor { get; init; } = Color.White;
        public bool IsMonochrome { get; set; } = true;
        public bool IsTiled { get; set; } = true;
    }
}
