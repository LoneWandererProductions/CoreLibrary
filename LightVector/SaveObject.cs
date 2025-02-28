using System;
using System.Windows;
using System.Xml.Serialization;

namespace LightVector
{
    /// <summary>
    /// The save object class.
    /// Save in a Dictionary, Id will be the Key and forwarder for the ParentId
    /// </summary>
    [Serializable]
    [XmlInclude(typeof(LineObject))]
    [XmlInclude(typeof(CurveObject))]
    [XmlInclude(typeof(Polygons))]
    public sealed class SaveObject
    {
        /// <summary>
        /// Unique identifier for the object.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Layer number, for sorting in rendering.
        /// </summary>
        public int Layer { get; set; }

        /// <summary>
        /// Reference starting point for object positioning.
        /// </summary>
        public Point StartCoordinates { get; set; }

        /// <summary>
        /// Graphic object (Line, Curve, Polygon).
        /// </summary>
        public GraphicObject Graphic { get; set; }

        /// <summary>
        /// Type of graphic object (Line, Curve, etc.).
        /// </summary>
        public GraphicTypes Type { get; set; }
    }
}