/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        ProceduralWallFactory.cs
 * PURPOSE:     Factory to generate custom Walls.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

using Imaging.Objects;

namespace Solaris
{
    /// <summary>
    /// Provides walls for Aurora.
    /// </summary>
    public static class ProceduralWallFactory
    {
        /// <summary>
        /// The identifier intact
        /// </summary>
        public const int IdIntact = 9001;

        /// <summary>
        /// The identifier cutaway
        /// </summary>
        public const int IdCutaway = 9002;

        /// <summary>
        /// The identifier ruined
        /// </summary>
        public const int IdRuined = 9003;

        /// <summary>
        /// The identifier rubble
        /// </summary>
        public const int IdRubble = 9004;

        /// <summary>
        /// Generates dummy wall buffers into the unmanaged texture cache.
        /// Later, you can swap these IDs with real disk textures via TextureManager.PreloadGlobalAsset().
        /// </summary>
        public static void InitializeWallDummies(int textureSize)
        {
            RegisterWall(IdIntact, CreateIntactDummy(textureSize));
            RegisterWall(IdCutaway, CreateCutawayDummy(textureSize));
            RegisterWall(IdRuined, CreateRuinedDummy(textureSize));
            RegisterWall(IdRubble, CreateRubbleDummy(textureSize));
        }

        /// <summary>
        /// Registers the wall.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="buffer">The buffer.</param>
        private static void RegisterWall(int id, UnmanagedImageBuffer? buffer)
        {
            var tex = new Texture
            {
                Id = id,
                Path = $"[ProceduralWall:{id}]",
                Layer = 1, // Walls sit on Layer 1 above the floor (Layer 0)
                IsStandingSprite = true
            };

            TextureManager.RegisterGeneratedTexture(id, buffer, layer: 1);
        }

        /// <summary>
        /// Creates the intact dummy.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns>The UnmanagedImageBuffer with Dummy.</returns>
        private static UnmanagedImageBuffer? CreateIntactDummy(int size)
        {
            var buf = new UnmanagedImageBuffer(size, size);
            buf.Clear(0, 0, 0, 0);

            int halfW = size / 2;
            int halfH = size / 4;
            int topY = halfH;
            int botY = size - halfH;

            DrawPrismFacets(buf, size, topY, botY, isRuined: false);
            return buf;
        }

        /// <summary>
        /// Creates the cutaway dummy.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns>The UnmanagedImageBuffer with cutaway Dummy.</returns>
        private static UnmanagedImageBuffer? CreateCutawayDummy(int size)
        {
            var buf = new UnmanagedImageBuffer(size, size);
            buf.Clear(0, 0, 0, 0);

            int halfH = size / 4;
            // Cutaway starts much lower (top 65% is transparent air)
            int topY = size - (int)(halfH * 2.2);
            int botY = size - halfH;

            DrawPrismFacets(buf, size, topY, botY, isRuined: false);
            return buf;
        }

        /// <summary>
        /// Creates the ruined dummy.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns>The UnmanagedImageBuffer with ruined Dummy.</returns>
        private static UnmanagedImageBuffer? CreateRuinedDummy(int size)
        {
            var buf = new UnmanagedImageBuffer(size, size);
            buf.Clear(0, 0, 0, 0);

            int halfH = size / 4;
            int topY = (int)(halfH * 1.5);
            int botY = size - halfH;

            DrawPrismFacets(buf, size, topY, botY, isRuined: true);
            return buf;
        }

        /// <summary>
        /// Creates the rubble dummy.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns>The UnmanagedImageBuffer with rubble Dummy.</returns>
        private static UnmanagedImageBuffer? CreateRubbleDummy(int size)
        {
            // Rubble is flat against the floor diamond
            var buf = new UnmanagedImageBuffer(size, size / 2);
            buf.Clear(0, 0, 0, 0);

            int cx = size / 2;
            int cy = size / 4;

            // Draw 3 scattered stone chunks
            DrawStone(buf, cx - 10, cy - 2, 8, 5, 110, 110, 115);
            DrawStone(buf, cx + 4, cy, 7, 4, 90, 90, 95);
            DrawStone(buf, cx - 2, cy + 4, 9, 5, 130, 130, 135);

            return buf;
        }

        /// <summary>
        /// Draws the prism facets.
        /// </summary>
        /// <param name="buf">The buf.</param>
        /// <param name="size">The size.</param>
        /// <param name="topY">The top y.</param>
        /// <param name="botY">The bot y.</param>
        /// <param name="isRuined">if set to <c>true</c> [is ruined].</param>
        private static void DrawPrismFacets(UnmanagedImageBuffer? buf, int size, int topY, int botY, bool isRuined)
        {
            int halfW = size / 2;
            int halfH = size / 4;
            int wallDrop = botY - topY;

            // Left Face (South-West: Shadow 60%)
            for (int col = 0; col < halfW; col++)
            {
                int sx = col;
                int faceTop = topY + (int)(col * (float)halfH / halfW);

                // Ruined profile: notch in center and sloped damage
                if (isRuined && col > halfW * 0.45) faceTop += 6;

                for (int y = faceTop; y < botY + (int)(col * (float)halfH / halfW); y++)
                {
                    if (y >= 0 && y < size)
                    {
                        byte shade = (byte)(y == faceTop ? 180 : 100);
                        buf.SetPixelUnsafe(sx, y, shade, shade, (byte)(shade + 10), 255);
                    }
                }
            }

            // Right Face (South-East: Light 80%)
            for (int col = 0; col < halfW; col++)
            {
                int sx = halfW + col;
                int faceTop = (topY + halfH) - (int)(col * (float)halfH / halfW);

                if (isRuined) faceTop += (int)(col * 0.35); // Sloping crumbling edge

                for (int y = faceTop; y < (botY + halfH) - (int)(col * (float)halfH / halfW); y++)
                {
                    if (y >= 0 && y < size)
                    {
                        byte shade = (byte)(y == faceTop ? 220 : 140);
                        buf.SetPixelUnsafe(sx, y, shade, shade, (byte)(shade + 10), 255);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the stone.
        /// </summary>
        /// <param name="buf">The buf.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="w">The w.</param>
        /// <param name="h">The h.</param>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        private static void DrawStone(UnmanagedImageBuffer? buf, int x, int y, int w, int h, byte r, byte g, byte b)
        {
            for (int row = y; row < y + h; row++)
            {
                for (int col = x; col < x + w; col++)
                {
                    if (col >= 0 && col < buf.Width && row >= 0 && row < buf.Height)
                    {
                        buf.SetPixelUnsafe(col, row, r, g, b, 255);
                    }
                }
            }
        }
    }
}
