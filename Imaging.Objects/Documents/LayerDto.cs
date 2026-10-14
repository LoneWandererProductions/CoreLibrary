/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        LayerDto.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Shapes;

namespace Imaging.Objects.Documents
{
    public sealed class LayerDto
    {
        public string? Type { get; set; }

        public Guid Id { get; set; }

        public string? Name { get; set; }

        public bool Visible { get; set; } = true;

        public double Opacity { get; set; } = 1.0;

        public string? Blend { get; set; }

        public string? File { get; set; }

        public List<Shape>? Shapes { get; set; }
    }
}