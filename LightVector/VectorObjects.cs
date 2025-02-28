﻿/*
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

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace LightVector
{
    /// <summary>
    ///     The graphic object class.
    ///     Contains all the basic Options
    /// </summary>
    [XmlInclude(typeof(LineObject))]
    [XmlInclude(typeof(CurveObject))]
    [Serializable]
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
            set => Fill = string.IsNullOrEmpty(value) ? null : new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
        }

        /// <summary>
        ///     Optional
        /// </summary>
        public PenLineJoin StrokeLineJoin { get; init; } = PenLineJoin.Bevel;

        /// <summary>
        /// Gets or sets the index of the z.
        /// </summary>
        /// <value>
        /// The index of the z.
        /// </value>
        public int ZIndex { get; set; } = 0;


        /// <summary>
        /// Apply transformation method (scaling, rotation, etc.)
        ///  Each subclass will override this method to implement specific transformation logic
        /// </summary>
        /// <param name="transformation">The transformation.</param>
        public virtual void ApplyTransformation(Transform transformation)
        {
            // Each subclass will override this method to implement specific transformation logic
        }
    }

        /// <summary>
        ///     The polygon object class.
        /// </summary>
        [Serializable]
        public sealed class Polygons
        {
            /// <summary>
            ///     Gets or sets the points.
            /// </summary>
            public List<Point> Points { get; set; } = new();
        }

        /// <summary>
        ///     Save Container
        /// </summary>
        [XmlRoot(ElementName = "Element")]
        public sealed class SaveContainer
        {
            /// <summary>
            ///     Gets or sets the objects.
            /// </summary>
            /// <value>
            ///     The objects.
            /// </value>
            public List<SaveObject> Objects { get; init; } = new();

            /// <summary>
            ///     Gets or sets the width.
            /// </summary>
            /// <value>
            ///     The width.
            /// </value>
            public int Width { get; init; }

            /// <summary>
            /// Gets the height.
            /// </summary>
            /// <value>
            /// The height.
            /// </value>
            public int Height { get; init; }

            //TODO add more stuff
        }

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
            public VectorObjects Type { get; set; }
        }
    }
