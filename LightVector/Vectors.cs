/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/Vectors.cs
 * PURPOSE:     Entry for our Vector Program
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Shapes;
using DataFormatter;
using Mathematics;

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
        /// <param name="height"></param>
        /// <param name="width">The width.</param>
        public Vectors(int height, int width)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Vectors" /> class.
        /// </summary>
        public Vectors()
        {
        }

        /// <summary>
        ///     Gets the lines.
        /// </summary>
        public List<LineObject> Lines { get; private set; }

        /// <summary>
        ///     Gets the curves.
        /// </summary>
        private List<CurveObject> Curves { get; set; }

        /// <summary>
        ///     Gets or sets the width of the Picture.
        /// </summary>
        /// <value>
        ///     The width.
        /// </value>
        private int Width { get; set; }

        /// <summary>
        ///     Gets or sets the height of the Picture.
        /// </summary>
        /// <value>
        ///     The height.
        /// </value>
        private int Height { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Save the container.
        /// </summary>
        /// <param name="path">Target Path of svg Object</param>
        public void SaveContainer(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Trace.WriteLine(WvgResources.ErrorPath);
                return;
            }

            var saveList = new List<SaveObject>();
            SaveObject save;

            if (Lines.Count != 0)
            {
                foreach (var line in Lines)
                {
                    save = new SaveObject { Type = GraphicTypes.Line, Graphic = line };
                    saveList.Add(save);
                }
            }

            if (Curves.Count != 0)
            {
                foreach (var curve in Curves)
                {
                    save = new SaveObject { Type = GraphicTypes.Curve, Graphic = curve };
                    saveList.Add(save);
                }
            }

            var saveObject = new SaveContainer { Objects = saveList, Width = Width };

            SaveHelper.XmlSerializerObject(saveObject, path);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Get the vector image.
        /// </summary>
        /// <param name="path">Target Path of svg Object</param>
        /// <returns>The <see cref="ImageContainer" />.</returns>
        public ImageContainer GetVectorImage(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Trace.WriteLine(WvgResources.ErrorPath);
                return null;
            }

            var save = SaveHelper.XmlDeSerializerSaveContainer(path);
            Width = save.Width;

            if (save.Objects == null)
            {
                return null;
            }

            return new ImageContainer
            {
                Lines = VgProcessing.GetVectorLines(save), Curves = VgProcessing.GetVectorCurves(save)
            };
        }

        /// <inheritdoc />
        /// <summary>
        ///     Generate Rectangle at point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>The <see cref="Rectangle" />.</returns>
        public Rectangle GetRectangle(Point point)
        {
            //TODO refine
            return null;
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
        public Path CurveAdd(List<Vector2> curve)
        {
            Curves ??= new List<CurveObject>();

            var crv = new CurveObject { Vectors = curve };

            //TODO implement cuvier path
            Curves.Add(crv);

            //TODO return a drawable path

            return null;
        }

        public Polygons LoadObjectFile(ObjFile obj, Vector3D translation, int angleX, int angleY,
            int angleZ, int scale)
        {
            return VgProcessing.CreatePolygon(obj, translation, angleX, angleY, angleZ, scale, Width, Height);
        }

        /// <inheritdoc />
        /// <summary>
        ///     The line remove.
        /// </summary>
        /// <param name="line">The line.</param>
        public void LineRemove(LineObject line)
        {
            Lines?.Remove(line);
        }

        /// <inheritdoc />
        /// <summary>
        ///     The curve remove.
        /// </summary>
        /// <param name="curve">The curve.</param>
        public void CurveRemove(CurveObject curve)
        {
            Curves?.Remove(curve);
        }

        /// <inheritdoc />
        /// <summary>
        ///     The line rotate.
        /// </summary>
        /// <param name="line">Line Object</param>
        /// <param name="degree">Degree we want to Rotate</param>
        /// <returns>The <see cref="LineObject" />.</returns>
        public LineObject LineRotate(LineObject line, int degree)
        {
            return VgProcessing.LineRotate(line, degree, Width);
        }

        /// <inheritdoc />
        /// <summary>
        ///     The curve rotate.
        /// </summary>
        /// <param name="curve">Curve Object</param>
        /// <param name="degree">Degree we want to Rotate</param>
        /// <returns>The <see cref="T:LightVector.CurveObject" />.</returns>
        public CurveObject CurveRotate(CurveObject curve, int degree)
        {
            return VgProcessing.CurveRotate(curve, degree);
        }

        /// <inheritdoc />
        /// <summary>
        ///     The line scale.
        /// </summary>
        /// <param name="line">Line Object</param>
        /// <param name="factor">Resize Factor</param>
        /// <returns>The <see cref="T:LightVector.LineObject" />.</returns>
        public LineObject LineScale(LineObject line, int factor)
        {
            return VgProcessing.LineScale(line, factor, Width);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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
