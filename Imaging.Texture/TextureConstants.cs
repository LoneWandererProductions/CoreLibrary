/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Texture
 * FILE:        TextureConstants.cs
 * PURPOSE:     String and Number Resource class.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

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

        /// <summary>
        /// Gets the brushed steel configuration.
        /// </summary>
        /// <returns>The steel configuration.</returns>
        public static TextureConfig GetSteelConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 80.0, // Used for Y-grain scale
                CenterRgb =  [180,
                185,
                190], // Base steel tone
                EdgeRgb =  [230,
                235,
                240] // Highlight grain tone
            };
        }

        /// <summary>
        /// Gets the glossy latex configuration.
        /// </summary>
        /// <returns>The latex configuration.</returns>
        public static TextureConfig GetLatexConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 64.0,
                Persistence = 8.0, // Reused for specular exponent
                CenterRgb =  [20,
                20,
                25], // Deep base material
                EdgeRgb =  [240,
                245,
                255] // Sharp specular sheen
            };
        }

        /// <summary>
        /// Gets the organic leather configuration.
        /// </summary>
        /// <returns>The leather configuration.</returns>
        public static TextureConfig GetLeatherConfig()
        {
            return new TextureConfig
            {
                CellSize = 12,
                WarpStrength = 6.0,
                CenterRgb =  [110,
                65,
                35], // Raised leather skin tone
                EdgeRgb =  [40,
                25,
                15] // Deep pore/crease shadow
            };
        }

        /// <summary>
        /// Gets the glossy latex configuration.
        /// </summary>
        /// <returns>The latex configuration.</returns>
        public static TextureConfig GetCustomLatexConfig(byte r, byte g, byte b)
        {
            // Lighten base color for sheen specular tinting
            var sheenR = (byte)Math.Min(255, r + 100);
            var sheenG = (byte)Math.Min(255, g + 100);
            var sheenB = (byte)Math.Min(255, b + 100);

            return new TextureConfig
            {
                TurbulenceSize = 64.0,
                Persistence = 8.0,
                CenterRgb =  [r,
                g,
                b],
                EdgeRgb =  [sheenR,
                sheenG,
                sheenB]
            };
        }

        /// <summary>
        /// Gets the organic leather configuration.
        /// </summary>
        /// <returns>The leather configuration.</returns>
        public static TextureConfig GetCustomLeatherConfig(byte r, byte g, byte b, double shadowFactor = 0.35)
        {
            // Darken base color to create realistic crease depth
            var poreR = (byte)(r * shadowFactor);
            var poreG = (byte)(g * shadowFactor);
            var poreB = (byte)(b * shadowFactor);

            return new TextureConfig
            {
                CellSize = 12,
                WarpStrength = 6.0,
                CenterRgb =  [r,
                g,
                b],
                EdgeRgb =  [poreR,
                poreG,
                poreB]
            };
        }

        /// <summary>
        /// Gets the high polished chrome steel configuration.
        /// </summary>
        /// <returns>The polished steel configuration.</returns>
        public static TextureConfig GetPolishedSteelConfig()
        {
            return new TextureConfig
            {
                WarpScale = 64.0, WarpStrength = 16.0, Persistence = 3.0 // Reused for reflection band count
            };
        }
    }
}
