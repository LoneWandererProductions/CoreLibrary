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

        /// <summary>
        /// Gets the leaf cloud foliage canopy configuration.
        /// </summary>
        /// <returns>The leaf cloud configuration.</returns>
        public static TextureConfig GetLeafCloudConfig()
        {
            return new TextureConfig
            {
                VoronoiGridSize = 12, // High cell density creates tight leaf clusters
                                      // Format: Highlight [0-2], Base Leaf [3-5], Deep Shadow [6-8], Mortar/Gap [9-11]
                RgbRamp = [
                    140, 210, 60,  // Bright sunlit leaf highlight
            50,  140, 35,  // Mid-tone foliage green
            20,  60,  20,  // Deep shadow crease
            10,  30,  10   // Inner dark gap / ambient shadow
                ]
            };
        }

        /// <summary>
        /// Gets the smooth volumetric leaf cloud configuration.
        /// </summary>
        /// <returns>The volumetric leaf cloud configuration.</returns>
        public static TextureConfig GetVolumetricLeafCloudConfig()
        {
            return new TextureConfig
            {
                VoronoiGridSize = 8, // Grid resolution for rounded leaf puffs
                RgbRamp = [
                    140, 210, 60,  // Sunlit leaf highlight
            50,  140, 35,  // Mid-tone foliage green
            20,  60,  20,  // Deep shadow crease
            10,  30,  10   // Inner dark gap / ambient shadow
                ]
            };
        }

        /// <summary>
        /// Gets the soft, low-noise terrain grass configuration.
        /// </summary>
        /// <returns>The terrain grass configuration.</returns>
        public static TextureConfig GetTerrainGrassConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 48.0, // Large macro scale for broad field color transitions
                                       // Format: Dark Moss [0-2], Mid Meadow [3-5], Warm Highlight [6-8]
                RgbRamp = [
                    35, 75, 30,    // Deep forest green
            70, 115, 45,   // Mid-tone meadow green
            95, 135, 55    // Soft sunlit grass accent
                ]
            };
        }

        /// <summary>
        /// Gets the soft earthy terrain dirt configuration.
        /// </summary>
        /// <returns>The terrain dirt configuration.</returns>
        public static TextureConfig GetTerrainDirtConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 32.0, // Macro scale for soil patches
                                       // Format: Damp Rich Soil [0-2], Mid Loam [3-5], Soft Dry Silt [6-8]
                RgbRamp = [
                    55, 40, 28,    // Deep rich damp soil
            85, 62, 42,    // Mid-tone brown loam
            115, 88, 60    // Soft dry silt highlight
                ]
            };
        }

        /// <summary>
        /// Gets the mountain cliff rock configuration.
        /// </summary>
        /// <returns>The mountain rock configuration.</returns>
        public static TextureConfig GetMountainRockConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 64.0, // Macro elevation contours
                                       // Format: Deep Shadow Slate [0-2], Cliff Grey [3-5], Ridge Highlight [6-8]
                RgbRamp = [
                    45, 50, 55,    // Deep slate shadow
            85, 90, 95,    // Mid-tone cliff rock
            135, 140, 145  // Sunlit mountain ridge highlight
                ]
            };
        }

        /// <summary>
        /// Gets the layered dungeon sandstone configuration.
        /// </summary>
        /// <returns>The dungeon sandstone configuration.</returns>
        public static TextureConfig GetDungeonSandstoneConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 16.0, // Strata layer spacing
                WarpScale = 40.0,      // Bedding plane wave scale
                WarpStrength = 12.0,   // Bedding distortion strength
                                       // Format: Seam Line [0-2], Mid Tan Sandstone [3-5], Ochre Layer [6-8]
                RgbRamp = [
                    110, 85, 55,   // Dark sediment seam
            160, 130, 90,  // Mid-tone tan sandstone
            205, 175, 130  // Warm ochre highlight band
                ]
            };
        }

        /// <summary>
        /// Gets the flat, low-noise dungeon sandstone terrain configuration.
        /// </summary>
        /// <returns>The flat dungeon sandstone configuration.</returns>
        public static TextureConfig GetDungeonSandstoneFlatConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 40.0, // Macro scale for broad, smooth sand patch transitions
                                       // Format: Deep Warm Sand [0-2], Mid Tan [3-5], Ochre Highlight [6-8]
                RgbRamp = [
                    120, 95, 60,   // Deep warm sand shadow
            165, 135, 95,  // Mid-tone tan sandstone
            210, 180, 135  // Soft sunlit ochre highlight
                ]
            };
        }

        /// <summary>
        /// Gets the raw cast iron configuration.
        /// </summary>
        /// <returns>The raw iron configuration.</returns>
        public static TextureConfig GetRawIronConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 32.0,
                CenterRgb = [55, 58, 62],      // Dark matte cast iron base
                EdgeRgb = [110, 115, 122]       // Micro-pitting highlight tone
            };
        }

        /// <summary>
        /// Gets the wrought/hammered iron configuration.
        /// </summary>
        /// <returns>The wrought iron configuration.</returns>
        public static TextureConfig GetWroughtIronConfig()
        {
            return new TextureConfig
            {
                CellSize = 28,
                WarpStrength = 4.0,
                CenterRgb = [85, 90, 98],       // Hammer face highlight
                EdgeRgb = [25, 27, 30]          // Deep forged crease/shadow
            };
        }

        /// <summary>
        /// Gets the rusted iron configuration.
        /// </summary>
        /// <returns>The rusted iron configuration.</returns>
        public static TextureConfig GetRustedIronConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 48.0,
                WarpScale = 32.0,
                WarpStrength = 8.0,
                // Dark Oxidized Iron Base -> Deep Corrosion -> Rich Oxide Rust -> Ochre Highlight
                RgbRamp = [
                    35, 35, 40,    // Oxidized dark iron base
                    90, 35, 15,    // Deep corrosion shadow
                    175, 70, 20,   // Rich iron oxide rust
                    215, 120, 35   // Flaky ochre rust highlight
                ]
            };
        }

        /// <summary>
        /// Gets the desert sand configuration with gentle ripple contours.
        /// </summary>
        /// <returns>The desert sand configuration.</returns>
        public static TextureConfig GetDesertSandConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 24.0, // Ripple wavelength
                WarpScale = 48.0,      // Smooth wave distortion
                WarpStrength = 6.0,    // Soft ripple drift
                // Silt/Shadow [0-2], Mid Tan / Dune Gold [3-5], Warm Pale Sunlit Sand [6-8]
                RgbRamp = [
                    165, 130, 80,  // Soft silt shadow / wet sand
                    215, 180, 120, // Mid-tone golden desert sand
                    240, 210, 155  // Warm pale highlight
                ]
            };
        }

        /// <summary>
        /// Gets the flat, muted desert sand configuration for toned-down areas and pond bottoms.
        /// </summary>
        /// <returns>The flat desert sand configuration.</returns>
        public static TextureConfig GetDesertSandFlatConfig()
        {
            return new TextureConfig
            {
                TurbulenceSize = 48.0, // Macro scale for broad, smooth sand patch transitions
                // Deep Silt [0-2], Muted Tan Sand [3-5], Soft Warm Sand [6-8]
                RgbRamp = [
                    155, 125, 75,  // Muted damp silt shadow
                    205, 170, 110, // Smooth mid-tone sand
                    230, 200, 145  // Soft low-contrast highlight
                ]
            };
        }
    }
}
