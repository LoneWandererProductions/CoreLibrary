/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Texture
 * FILE:        TextureConstants.cs
 * PURPOSE:     String and Number Resource class.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Texture
{
    /// <summary>
    /// Class that holds all needed constants.
    /// </summary>
    public static class TextureConstants
    {
        /// <summary>
        /// The seed for procedural determinism.
        /// </summary>
        public const int DefaultSeed = 42;

        // --- RECIPES (CONFIGURATIONS) ---

        /// <summary>
        /// Gets the raw stone configuration.
        /// </summary>
        /// <returns>The stone configuration.</returns>
        public static TextureConfig GetStoneConfig()
        {
            return new TextureConfig
            {
                // Reusing this property for the Voronoi grid size (e.g., 4 cells across)
                VoronoiGridSize = 4,
                // Format: Highlight [0-2], Base Stone [3-5], Shadow [6-8], Mortar [9-11]
                RgbRamp =  [
                220,
                220,
                220, // Bright edge highlight
                130,
                130,
                130, // Base mid-tone
                40,
                40,
                40, // Deep directional shadow
                15,
                15,
                15 // Dark mortar/grout
                    ]
            };
        }

        /// <summary>
        /// Gets the lava pool configuration.
        /// </summary>
        /// <returns>The lava pool configuration.</returns>
        public static TextureConfig GetLavaPoolConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 32.0,
                // Stored purely as flat [R, G, B] bytes for the math engine
                RgbRamp =  [40,
                10,
                10,
                180,
                20,
                0,
                255,
                120,
                0,
                255,
                220,
                50]
            };
        }

        /// <summary>
        /// Gets the cobblestone configuration.
        /// </summary>
        /// <returns>The cobblestone configuration.</returns>
        public static TextureConfig GetCobblestoneConfig()
        {
            return new TextureConfig
            {
                CellSize = 48,
                CenterRgb =  [140,
                140,
                145],
                EdgeRgb =  [30,
                30,
                30]
            };
        }

        /// <summary>
        /// Gets the magical ether configuration.
        /// </summary>
        /// <returns>The magical ether configuration.</returns>
        public static TextureConfig GetMagicalEtherConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 128.0,
                // Flat [R, G, B] bytes for: Dark Navy -> Deep Blue -> Bright Purple -> White
                RgbRamp =  [5,
                5,
                20,
                0,
                150,
                200,
                180,
                50,
                255,
                255,
                255,
                255]
            };
        }

        /// <summary>
        /// Gets the cracked ice configuration.
        /// </summary>
        /// <returns>The cracked ice configuration.</returns>
        public static TextureConfig GetCrackedIceConfig()
        {
            return new TextureConfig
            {
                CellSize = 64,
                CenterRgb =  [230,
                245,
                255], // Bright icy white for the sharp ridges
                EdgeRgb =  [10,
                40,
                80] // Deep water blue for the flat cells
            };
        }

        /// <summary>
        /// Gets the magic portal configuration.
        /// </summary>
        /// <returns>The magic portal configuration.</returns>
        public static TextureConfig GetMagicPortalConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 64.0,
                WarpScale = 128.0,
                WarpStrength = 4.0, // High strength creates deep liquid swirls
                RgbRamp =  [0,
                0,
                10,
                40,
                10,
                120,
                150,
                40,
                255,
                255,
                200,
                255]
            };
        }

        /// <summary>
        /// Gets the plasma arc configuration.
        /// </summary>
        /// <returns>The plasma arc configuration.</returns>
        public static TextureConfig GetPlasmaArcConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 128.0,
                Octaves = 5,
                Persistence = 0.5,
                // Black background -> dark purple -> bright cyan -> white hot core
                RgbRamp =  [0,
                0,
                0,
                40,
                0,
                80,
                0,
                200,
                255,
                255,
                255,
                255]
            };
        }

        /// <summary>
        /// Gets the furrowed tree bark configuration.
        /// </summary>
        /// <returns>The tree bark configuration.</returns>
        public static TextureConfig GetTreeBarkConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 16.0, // Maps to horizontal frequency scale
                WarpStrength = 12.0, // Grain twisting displacement power
                CenterRgb =  [140,
                95,
                55], // Bright ridge wood highlight color
                EdgeRgb =  [75,
                45,
                25] // Dark deep furrow crease color
            };
        }

        /// <summary>
        /// Gets the leaf foliage configuration.
        /// </summary>
        /// <returns>The leaf foliage configuration.</returns>
        public static TextureConfig GetFoliageConfig()
        {
            return new TextureConfig
            {
                CellSize = 40,
                CenterRgb =  [34,
                110,
                24], // Primary outer leaf green color
                EdgeRgb =  [12,
                35,
                10] // Deep background ambient shadow drop color
            };
        }

        /// <summary>
        /// Gets the wooden plank board configuration.
        /// </summary>
        /// <returns>The wooden plank board configuration.</returns>
        public static TextureConfig GetWoodPlankConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 32.0,
                WarpStrength = 0.15, // Reusing WarpStrength for internal Engine TurbulencePower
                CenterRgb =  [130,
                85,
                45], // Base board brown
                EdgeRgb =  [70,
                40,
                20] // Dark grain accent lines
            };
        }
    }
}
