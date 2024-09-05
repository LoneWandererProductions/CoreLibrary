/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/Lif.cs
 * PURPOSE:     Custom Image Format object, an extension of Cif, I'd like to create an layered Cif
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

using System.Collections.Generic;
using System.Drawing;

namespace Imaging
{
    public class Lif
    {
        public List<Bitmap> Layers { get; set; } = new List<Bitmap>();

        public Lif() { }

        public Lif(List<Bitmap> layers)
        {
            Layers = layers;
        }

        // Convert Lif to a Bitmap
        public List<Dictionary<Color, SortedSet<int>>> ToBitmap(string lifPath)
        {
            // Generate final bitmap from LIF
            return LifProcessing.LoadLif(lifPath);
        }

        // Save Lif to a file
        public void Save(string path, List<Bitmap> Layers)
        {
        }
    }
}
