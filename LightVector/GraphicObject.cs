/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/TranslatedLine.cs
 * PURPOSE:     Hold the Graphic Objects
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.microsoft.com/de-de/dotnet/api/system.drawing.graphics.drawcurve?view=netframework-4.8
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global

using System.Collections.Generic;
using System.Windows.Media;
using System.Xml.Serialization;

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
        ///     Optional
        /// </summary>
        public int Thickness { get; init; } = 1;

        /// <summary>
        ///     Gets or sets the stroke.
        ///     Optional
        /// </summary>
        [XmlIgnore]
        public SolidColorBrush Stroke { get; set; } = Brushes.Black;

        /// <summary>
        ///     Workaround for XML serialization of Stroke
        /// </summary>
        [XmlElement("Stroke")]
        public string StrokeColor
        {
            get => Stroke.Color.ToString();
            set => Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
        }

        /// <summary>
        ///     Optional
        ///     If filled we will get filled curves
        /// </summary>
        [XmlIgnore]
        public SolidColorBrush Fill { get; set; }

        /// <summary>
        ///     Workaround for XML serialization of Fill
        /// </summary>
        [XmlElement("Fill")]
        public string FillColor
        {
            get => Fill?.Color.ToString();
            set => Fill = string.IsNullOrEmpty(value)
                ? null
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
        }

        [XmlIgnore] public Dictionary<string, object> OptionalAttributes { get; set; } = new();

        // For XML serialization
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
        ///     Optional
        /// </summary>
        public PenLineJoin StrokeLineJoin { get; init; } = PenLineJoin.Bevel;

        /// <summary>
        ///     Checks if this object supports the given transformation.
        /// </summary>
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
