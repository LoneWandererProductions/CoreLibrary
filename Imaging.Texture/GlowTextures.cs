/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Texture
 * FILE:        GlowTextures.cs
 * PURPOSE:     Round soft glow sprites with an alpha falloff (magic bolt, fireball): billboards need a round picture, a square
 *              opaque texture would show as a square.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Texture
{
    /// <summary>
    /// Generates radial glow textures: a bright core that fades through a coloured halo to full transparency at the edge.
    /// </summary>
    public static class GlowTextures
    {
        /// <summary>
        /// The share of the radius (from the middle) that is white-hot core.
        /// </summary>
        private const float CoreShare = 0.22f;

        /// <summary>
        /// Generates a round glow. Pixels are BGRA, not premultiplied (the renderer blends with source alpha).
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="halo">The colour of the halo (red, green, blue); the core is blended towards white.</param>
        /// <param name="falloff">The steepness of the fade: <c>1</c> linear, <c>2</c> soft, <c>3</c> a tight point.</param>
        /// <returns>The glow, fully transparent from the edge of the inscribed circle outwards.</returns>
        public static RawTextureBuffer Generate(int width, int height, (byte R, byte G, byte B) halo,
            float falloff = 2f)
        {
            var buffer = new RawTextureBuffer(width, height);
            var pixels = buffer.PixelData;
            var exponent = Math.Max(0.5f, falloff);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    // Distance from the middle of the picture: 0 in the centre, 1 at the edge of the inscribed circle.
                    var dx = ((x + 0.5f) / width * 2f) - 1f;
                    var dy = ((y + 0.5f) / height * 2f) - 1f;
                    var d = MathF.Sqrt((dx * dx) + (dy * dy));

                    var alpha = d >= 1f ? 0f : MathF.Pow(1f - d, exponent);
                    var white = d <= CoreShare ? 1f : MathF.Max(0f, 1f - ((d - CoreShare) / 0.35f));

                    var i = ((y * width) + x) * 4;
                    pixels[i] = Mix(halo.B, 255, white);
                    pixels[i + 1] = Mix(halo.G, 255, white);
                    pixels[i + 2] = Mix(halo.R, 255, white);
                    pixels[i + 3] = (byte)Math.Clamp((int)MathF.Round(alpha * 255f), 0, 255);
                }
            }

            return buffer;
        }

        /// <summary>
        /// Generates the cold blue-white glow of a magic bolt.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>The glow.</returns>
        public static RawTextureBuffer GenerateBolt(int width, int height) =>
            Generate(width, height, (90, 170, 255), 1.6f);

        /// <summary>
        /// Generates the orange-yellow glow of a fireball.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>The glow.</returns>
        public static RawTextureBuffer GenerateFireball(int width, int height) =>
            Generate(width, height, (255, 110, 20), 1.4f);

        /// <summary>
        /// Generates the sea-green glow of a place marker (the exit zones of Journey): a colour no spell has, so it is not mistaken
        /// for a bolt or a fire.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>The glow.</returns>
        public static RawTextureBuffer GenerateBeacon(int width, int height) =>
            Generate(width, height, (70, 255, 190), 1.8f);

        /// <summary>
        /// Blends one colour channel towards another.
        /// </summary>
        private static byte Mix(byte from, byte to, float amount) =>
            (byte)Math.Clamp((int)MathF.Round(from + ((to - from) * amount)), 0, 255);
    }
}
