/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        TranslatedLine.cs
 * PURPOSE:     Hold the Graphic Objects
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/de-de/dotnet/api/system.drawing.graphics.drawcurve?view=netframework-4.8
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global

using System.Collections.Generic;
using System.Xml.Serialization;
using LightVector.Enums;

namespace LightVector
{
    /// <summary>
    ///     The graphic object class.
    ///     Contains all the basic Options
    /// </summary>
    [XmlInclude(typeof(LineObject))]
    [XmlInclude(typeof(BezierCurve))]
    [System.Serializable]
    public abstract class GraphicObject
    {
        /// <summary>
        /// The fill
        /// Default null/transparent
        /// </summary>
        public string Fill { get; set; }

        /// <summary>
        /// The stroke
        /// </summary>
        public string Stroke { get; set; } = "#FF000000";

        /// <summary>
        ///     Optional
        /// </summary>
        public int Thickness { get; init; } = 1;

        /// <summary>
        /// Gets or sets the optional attributes.
        /// </summary>
        /// <value>
        /// The optional attributes.
        /// </value>
        [XmlIgnore] public Dictionary<string, object> OptionalAttributes { get; set; } = new();

        /// <summary>
        /// Gets or sets the serializable attributes.
        ///  For XML serialization
        /// </summary>
        /// <value>
        /// The serializable attributes.
        /// </value>
        [XmlArray("OptionalAttributes")]
        [XmlArrayItem("Attribute")]
        public List<SerializableAttribute> SerializableAttributes
        {
            get
            {
                var list = new List<SerializableAttribute>();
                foreach (var kvp in OptionalAttributes)
                {
                    list.Add(new SerializableAttribute { Key = kvp.Key, Value = kvp.Value?.ToString() });
                }

                return list;
            }
            set
            {
                OptionalAttributes.Clear();
                if (value != null)
                {
                    foreach (var attr in value)
                    {
                        OptionalAttributes[attr.Key] = attr.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Optional
        /// </summary>
        /// <value>
        /// The stroke line join.
        /// </value>
        public VectorLineJoin StrokeLineJoin { get; init; } = VectorLineJoin.Bevel;

        /// <summary>
        /// Checks if this object supports the given transformation.
        /// </summary>
        /// <param name="transformation">The transformation.</param>
        /// <returns>Checks if transformation is supported.</returns>
        public abstract bool SupportsTransformation(Transform transformation);

        /// <summary>
        ///     Apply transformation method (scaling, rotation, etc.)
        ///     Each subclass will override this method to implement specific transformation logic
        /// </summary>
        /// <param name="transformation">The transformation.</param>
        public virtual void ApplyTransformation(Transform transformation)
        {
            // Each subclass will override this method to implement specific transformation logic
        }

        /// <summary>
        ///     Helper for Serialization of Dictionaries
        /// </summary>
        public sealed class SerializableAttribute
        {
            /// <summary>
            ///     Gets or sets the key.
            /// </summary>
            /// <value>
            ///     The key.
            /// </value>
            [XmlAttribute(nameof(Key))]
            public string Key { get; set; }

            /// <summary>
            ///     Gets or sets the value.
            /// </summary>
            /// <value>
            ///     The value.
            /// </value>
            [XmlAttribute(nameof(Value))]
            public string Value { get; set; }
        }
    }
}
