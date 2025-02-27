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

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
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
    }

    /// <summary>
    ///     The line object class.
    /// </summary>
    [Serializable]
    public sealed class LineObject
    {
        /// <summary>
        ///     Gets or sets the start point.
        /// </summary>
        public Point StartPoint { get; init; }

        /// <summary>
        ///     Gets or sets the end point.
        /// </summary>
        public Point EndPoint { get; init; }
    }

    /// <summary>
    ///     The curve object class.
    /// </summary>
    [Serializable]
    public sealed class CurveObject
    {
        /// <summary>
        ///     Gets or sets the points.
        /// </summary>
        public List<Point> Points { get; set; } = new();

        /// <summary>
        ///     Gets or sets the tension.
        /// </summary>
        public double Tension { get; init; } = 1.0;

        /// <summary>
        ///     Get the path.
        /// </summary>
        /// <returns>The <see cref="Path" /> Bezier Path.</returns>
        internal System.Windows.Shapes.Path GetPath()
        {
            var path = CustomObjects.ReturnBezierCurve(Points, Tension);
            //path.Fill = Fill;
            //path.Stroke = Stroke;
            //path.StrokeThickness = Thickness;
            //path.StrokeLineJoin = StrokeLineJoin;
            return path;
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
    }

    /// <summary>
    ///     The save object class.
    ///     Save in a Dictionary, Id will be the Key and forwarder for the ParentId
    /// </summary>
    [Serializable]
    [XmlInclude(typeof(LineObject))]
    [XmlInclude(typeof(CurveObject))]
    [XmlInclude(typeof(Polygons))]
    public sealed class SaveObject
    {
        /// <summary>
        ///     Gets or sets the point id.
        /// </summary>
        public object Graphic { get; set; }

        /// <summary>
        ///     Gets or sets the Type.
        ///     0 is Point
        ///     1 is Line
        ///     2 is Curve
        /// </summary>
        public VectorObjects Type { get; set; }
    }

    /// <summary>
    ///     Simple test class for serialization
    /// </summary>
    public static class SerializerTest
    {
        public static void RunTest()
        {
            var container = new SaveContainer
            {
                Width = 500,
                Objects = new List<SaveObject>
                {
                    new SaveObject
                    {
                        Graphic = new LineObject
                        {
                            StartPoint = new Point(10, 10),
                            EndPoint = new Point(50, 50)
                        },
                        Type = VectorObjects.Line
                    },
                    new SaveObject
                    {
                        Graphic = new CurveObject
                        {
                            Points = new List<Point> { new(10, 10), new(20, 40), new(50, 50) },
                            Tension = 0.5
                        },
                        Type = VectorObjects.Curve
                    }
                }
            };

            var serializer = new XmlSerializer(typeof(SaveContainer));

            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, container);
                Console.WriteLine(writer.ToString());  // Print XML output
            }
        }
    }
}
