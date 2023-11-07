/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/Vectors.cs
 * PURPOSE:     Entry for our Vector Program
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Shapes;
using System.Xml.Serialization;
using Debugger;
using Path = System.Windows.Shapes.Path;

namespace LightVector
{
    /// <inheritdoc />
    /// <summary>
    ///     The Vectors class.
    /// </summary>
    public sealed class Vectors : IVectors
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Vectors" /> class.
        /// </summary>
        /// <param name="width">The width.</param>
        public Vectors(int width)
        {
            Width = width;
        }

        public Vectors()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Gets the lines.
        /// </summary>
        public List<LineObject> Lines { get; private set; }

        /// <summary>
        ///     Gets the curves.
        /// </summary>
        public List<CurveObject> Curves { get; private set; }

        /// <summary>
        ///     length of the Picture
        /// </summary>
        private int Width { get; set; } = 300;

        /// <summary>
        ///     Save the container.
        /// </summary>
        /// <param name="path">Target Path of svg Object</param>
        public void SaveContainer(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                DebugLog.CreateLogFile(WvgResources.ErrorPath, 0);
                return;
            }

            var lines = VgProcessing.GenerateLines(Lines, Width);
            var curves = VgProcessing.GenerateCurves(Curves, Width);
            var save = new SaveContainer { SavedCurves = curves, SavedLines = lines, Width = Width };

            XmlSerializerObject(save, path);
        }

        /// <summary>
        ///     Get the vector image.
        /// </summary>
        /// <param name="path">Target Path of svg Object</param>
        /// <returns>The <see cref="ImageContainer" />.</returns>
        public ImageContainer GetVectorImage(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                DebugLog.CreateLogFile(WvgResources.ErrorPath, 0);
                return null;
            }

            var save = XmlDeSerializerSaveContainer(path);
            Width = save.Width;

            return new ImageContainer
            {
                Lines = VgProcessing.GetVectorLines(save), Curves = VgProcessing.GetVectorCurves(save)
            };
        }

        /// <summary>
        ///     Get the point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>The <see cref="Rectangle" />.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public Rectangle GetPoint(Point point)
        {
            return GraphicHelper.GetPoint(point);
        }

        /// <inheritdoc />
        /// <summary>
        ///     The line add.
        /// </summary>
        /// <param name="line">The line.</param>
        public void LineAdd(LineObject line)
        {
            Lines ??= new List<LineObject>();

            Lines.Add(line);
        }

        /// <inheritdoc />
        /// <summary>
        ///     The curve add.
        /// </summary>
        /// <param name="curve">The curve.</param>
        public Path CurveAdd(List<Point> curve)
        {
            Curves ??= new List<CurveObject>();

            var crv = new CurveObject { Points = curve };

            //what????
            var path = crv.GetPath();

            //TODO wtf?
            Curves.Add(crv);

            return path;
        }

        /// <summary>
        ///     The line remove.
        /// </summary>
        /// <param name="line">The line.</param>
        public void LineRemove(LineObject line)
        {
            Lines?.Remove(line);
        }

        /// <summary>
        ///     The curve remove.
        /// </summary>
        /// <param name="curve">The curve.</param>
        public void CurveRemove(CurveObject curve)
        {
            Curves?.Remove(curve);
        }

        /// <summary>
        ///     The line rotate.
        /// </summary>
        /// <param name="line">Line Object</param>
        /// <param name="degree">Degree we want to Rotate</param>
        /// <returns>The <see cref="LineObject" />.</returns>
        public LineObject LineRotate(LineObject line, int degree)
        {
            return VgProcessing.GetLineObject(line, degree, Width);
        }

        /// <summary>
        ///     The curve rotate.
        /// </summary>
        /// <param name="curve">Curve Object</param>
        /// <param name="degree">Degree we want to Rotate</param>
        /// <returns>The <see cref="CurveObject" />.</returns>
        public CurveObject CurveRotate(CurveObject curve, int degree)
        {
            return VgProcessing.CurveRotate(curve, degree);
        }

        /// <summary>
        ///     The line scale.
        /// </summary>
        /// <param name="line">Line Object</param>
        /// <param name="factor">Resize Factor</param>
        /// <returns>The <see cref="LineObject" />.</returns>
        public LineObject LineScale(LineObject line, int factor)
        {
            return VgProcessing.LineScale(line, factor, Width);
        }

        /// <summary>
        ///     The curve scale.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="factor">The factor.</param>
        /// <returns>The <see cref="CurveObject" />.</returns>
        public CurveObject CurveScale(CurveObject curve, int factor)
        {
            return VgProcessing.CurveScale(curve, factor);
        }

        /// <summary>
        ///     The rotate.
        /// </summary>
        /// <param name="degree">Degree we want to Rotate</param>
        /// <returns>The <see cref="ImageContainer" />.</returns>
        public ImageContainer Rotate(int degree)
        {
            Lines = VgProcessing.LinesRotate(Lines, degree, Width);
            Curves = VgProcessing.CurvesRotate(Curves, degree);

            return new ImageContainer { Lines = Lines, Curves = Curves };
        }

        /// <summary>
        ///     Scale whole Image.
        /// </summary>
        /// <param name="factor">Scale Factor</param>
        /// <returns>The Image Container<see cref="ImageContainer" />.</returns>
        public ImageContainer Scale(int factor)
        {
            Lines = VgProcessing.LinesScale(Lines, factor, Width);
            Curves = VgProcessing.CurvesScale(Curves, factor);

            return new ImageContainer { Lines = Lines, Curves = Curves };
        }

        /// <summary>
        ///     Generic Serializer Of Objects
        /// </summary>
        /// <param name="serializeObject">Target Object</param>
        /// <param name="path">Target Path with extension</param>
        private static void XmlSerializerObject<T>(T serializeObject, string path)
        {
            var directory = System.IO.Path.GetDirectoryName(path);
            if (!Directory.Exists(directory) && directory != null)
            {
                Directory.CreateDirectory(directory);
            }

            //check if file is empty, if empty return
            if (serializeObject.Equals(null))
            {
                DebugLog.CreateLogFile(WvgResources.ErrorSerializerEmpty + path, 0);
                File.Delete(path);
                return;
            }

            try
            {
                var serializer = new XmlSerializer(serializeObject.GetType());
                using var tr = new StreamWriter(path);
                serializer.Serialize(tr, serializeObject);
            }
            catch (Exception error)
            {
                DebugLog.CreateLogFile(WvgResources.ErrorSerializer + error, 0);
            }
        }

        /// <summary>
        ///     DeSerializes Object Type of: MapObject
        /// </summary>
        /// <param name="path">Target Path</param>
        private static SaveContainer XmlDeSerializerSaveContainer(string path)
        {
            if (!File.Exists(path))
            {
                DebugLog.CreateLogFile(WvgResources.ErrorPath, 0);
                return new SaveContainer();
            }

            //check if file is empty, if empty return a new empty one
            if (new FileInfo(path).Length == 0)
            {
                DebugLog.CreateLogFile(WvgResources.ErrorFileEmpty + path, 0);
                return new SaveContainer();
            }

            try
            {
                var serializer = new XmlSerializer(typeof(SaveContainer));
                using Stream tr = File.OpenRead(path);
                return (SaveContainer)serializer.Deserialize(tr);
            }
            catch (Exception error)
            {
                DebugLog.CreateLogFile(WvgResources.ErrorDeSerializer + error, 0);
                return new SaveContainer();
            }
        }
    }

    /// <summary>
    ///     The image container class.
    /// </summary>
    public sealed class ImageContainer
    {
        /// <summary>
        ///     Gets or sets the lines.
        /// </summary>
        public List<LineObject> Lines { get; internal init; }

        /// <summary>
        ///     Gets or sets the curves.
        /// </summary>
        public List<CurveObject> Curves { get; internal init; }
    }
}
