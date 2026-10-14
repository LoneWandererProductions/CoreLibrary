/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        DocumentDto.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects.Documents
{
    internal sealed class DocumentDto
    {
        public string? Format { get; set; }

        public int Version { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public List<LayerDto>? Layers { get; set; }
    }
}