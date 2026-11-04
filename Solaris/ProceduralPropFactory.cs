/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        ProceduralPropFactory.cs
 * PURPOSE:     Simple factory for generating procedural props like trees and crates, allowing for dynamic creation of game assets at runtime.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects;
using System;

namespace Solaris
{
    /// <summary>
    /// A simple factory for generating procedural props like trees and crates. This class provides methods to initialize and register these props with the texture manager, allowing for dynamic creation of game assets at runtime.
    /// </summary>
    public static class ProceduralPropFactory
    {
        /// <summary>
        /// The identifier tree
        /// </summary>
        public const int IdTree = 9101;

        /// <summary>
        /// The identifier crate
        /// </summary>
        public const int IdCrate = 9102;

        /// <summary>
        /// Initializes the props.
        /// </summary>
        /// <param name="textureSize">Size of the texture.</param>
        public static void InitializeProps(int textureSize)
        {
            // -----------------------------------------------------------------
            // 1. TALL HIGH-CANOPY TREE (Grounded trunk at bottom)
            // -----------------------------------------------------------------
            var treeH = (int)(textureSize * 1.5); // 96px tall if textureSize=64
            var treeBuffer = new UnmanagedImageBuffer(textureSize, treeH);
            treeBuffer.Clear(0, 0, 0, 0);

            var cx = textureSize / 2;

            // Trunk: Starts at bottom of buffer (y = treeH - 1) up to canopy base
            for (var y = treeH / 3; y < treeH; y++)
            {
                for (var x = cx - 3; x <= cx + 3; x++)
                {
                    var isTrunkBorder = x == cx - 3 || x == cx + 3;
                    var shade = (byte)(75 + (x - cx) * 8);
                    var r = isTrunkBorder ? (byte)45 : (byte)(shade * 1.1);
                    var g = isTrunkBorder ? (byte)30 : (byte)(shade * 0.7);
                    var b = isTrunkBorder ? (byte)15 : (byte)(shade * 0.4);
                    treeBuffer.SetPixelUnsafe(x, y, r, g, b, 255);
                }
            }

            // Small, dark contact mark/root collar at the base of the trunk
            for (var x = cx - 5; x <= cx + 5; x++)
            {
                treeBuffer.SetPixelUnsafe(x, treeH - 1, 30, 20, 10, 200);
            }

            // Canopy: High foliage canopy (radius 22px)
            var canopyCenterY = treeH / 4;
            var radiusX = (int)(textureSize * 0.38);
            var radiusY = (int)(radiusX * 0.75);

            for (var y = canopyCenterY - radiusY; y <= canopyCenterY + radiusY; y++)
            {
                for (var x = cx - radiusX; x <= cx + radiusX; x++)
                {
                    var dx = (x - cx) / (float)radiusX;
                    var dy = (y - canopyCenterY) / (float)radiusY;

                    if ((dx * dx) + (dy * dy) <= 1.0f)
                    {
                        var isEdge = (dx * dx) + (dy * dy) > 0.85f;
                        var gVal = isEdge ? (byte)60 : (byte)Math.Clamp(140 - (int)(dy * 30), 70, 175);
                        var rVal = isEdge ? (byte)20 : (byte)(gVal * 0.35);
                        var bVal = isEdge ? (byte)15 : (byte)(gVal * 0.25);
                        treeBuffer.SetPixelUnsafe(x, y, rVal, gVal, bVal, 255);
                    }
                }
            }

            TextureManager.RegisterGeneratedTexture(IdTree, treeBuffer, layer: 1, isStandingSprite: true);

            // -----------------------------------------------------------------
            // 2. ISOMETRIC WOODEN CRATE
            // -----------------------------------------------------------------
            var crateSize = (int)(textureSize * 0.55); // ~35px
            var crateBuffer = new UnmanagedImageBuffer(crateSize, crateSize);
            crateBuffer.Clear(0, 0, 0, 0);

            var halfC = crateSize / 2;
            var qtrC = crateSize / 4;
            var wallDrop = (int)(crateSize * 0.45);

            // Left Face (Shadow 60%)
            for (var col = 0; col < halfC; col++)
            {
                var sx = col;
                var topY = qtrC + (int)(col * 0.5f);
                for (var y = topY; y < topY + wallDrop; y++)
                {
                    if (y < crateSize)
                    {
                        var border = col == 0 || y == topY || y == topY + wallDrop - 1;
                        var r = border ? (byte)65 : (byte)110;
                        var g = border ? (byte)45 : (byte)75;
                        var b = border ? (byte)20 : (byte)35;
                        crateBuffer.SetPixelUnsafe(sx, y, r, g, b, 255);
                    }
                }
            }

            // Right Face (Lit 85%)
            for (var col = 0; col < halfC; col++)
            {
                var sx = halfC + col;
                var topY = (qtrC + (halfC / 2)) - (int)(col * 0.5f);
                for (var y = topY; y < topY + wallDrop; y++)
                {
                    if (y < crateSize)
                    {
                        var border = col == halfC - 1 || y == topY || y == topY + wallDrop - 1;
                        var r = border ? (byte)85 : (byte)150;
                        var g = border ? (byte)55 : (byte)105;
                        var b = border ? (byte)25 : (byte)50;
                        crateBuffer.SetPixelUnsafe(sx, y, r, g, b, 255);
                    }
                }
            }

            // Top Face (Light surface)
            for (var y = 0; y < halfC; y++)
            {
                for (var x = 0; x < crateSize; x++)
                {
                    var dx = Math.Abs(x - halfC) / (float)halfC;
                    var dy = Math.Abs(y - qtrC) / (float)qtrC;
                    if (dx + dy <= 1.0f)
                    {
                        var border = dx + dy > 0.88f;
                        var r = border ? (byte)95 : (byte)175;
                        var g = border ? (byte)65 : (byte)125;
                        var b = border ? (byte)30 : (byte)65;
                        crateBuffer.SetPixelUnsafe(x, y, r, g, b, 255);
                    }
                }
            }

            // Anker liegt exakt im Zentrum der Standraute: (halfC, qtrC + wallDrop) = (17, 23)
            TextureManager.RegisterGeneratedTexture(
                IdCrate,
                crateBuffer,
                layer: 1,
                isStandingSprite: true,
                anchorX: halfC,
                anchorY: qtrC + wallDrop);
        }
    }
}
