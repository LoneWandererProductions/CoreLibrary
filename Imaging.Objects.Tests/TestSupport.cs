/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Tests
 * FILE:        TestSupport.cs
 * PURPOSE:     Small helpers shared by the document tests.
 */

using Imaging.Objects.Documents;
using Imaging.Objects.Interfaces;
using Imaging.Objects.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imaging.Objects.Tests
{
    internal static class TestSupport
    {
        /// <summary>
        /// Asserts that <paramref name="action" /> throws <typeparamref name="T" /> (or a subclass).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action.</param>
        internal static void Throws<T>(Action action)
            where T : Exception
        {
            try
            {
                action();
            }
            catch (T)
            {
                return;
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected {typeof(T).Name} but got {ex.GetType().Name}: {ex.Message}");
                return;
            }

            Assert.Fail($"Expected {typeof(T).Name} but nothing was thrown.");
        }

        /// <summary>
        /// Fills the specified layer.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <param name="area">The area.</param>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        /// <param name="a">a.</param>
        internal static void Fill(RasterLayer layer, PixelRect area, byte r, byte g, byte b, byte a = 255)
        {
            for (var y = area.Y; y < area.Bottom; y++)
            {
                for (var x = area.X; x < area.Right; x++)
                {
                    layer.Pixels.SetPixel(x, y, r, g, b, a);
                }
            }
        }

        /// <summary>
        /// Pixels the specified layer.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>Pixel Color.</returns>
        internal static (byte r, byte g, byte b, byte a) Pixel(RasterLayer layer, int x, int y)
        {
            return layer.Pixels.GetPixel(x, y);
        }

        /// <summary>
        /// Pixels the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>Pixel Color.</returns>
        internal static (byte r, byte g, byte b, byte a) Pixel(UnmanagedImageBuffer buffer, int x, int y)
        {
            return buffer.GetPixel(x, y);
        }

        /// <summary>
        /// Snapshots the specified layer.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <returns>Layer as bytes.</returns>
        internal static byte[] Snapshot(RasterLayer layer)
        {
            return layer.Pixels.BufferSpan.ToArray();
        }

        /// <summary>
        /// Sames the pixels.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <param name="expected">The expected.</param>
        /// <returns></returns>
        internal static bool SamePixels(RasterLayer layer, byte[] expected)
        {
            return layer.Pixels.BufferSpan.SequenceEqual(expected);
        }

        /// <summary>
        /// Asserts that two color differ by at most <paramref name="tolerance" /> per channel.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="expected">The expected.</param>
        /// <param name="tolerance">The tolerance.</param>
        internal static void AssertColor((byte r, byte g, byte b, byte a) actual, (int r, int g, int b, int a) expected,
            int tolerance = 1)
        {
            var ok = Math.Abs(actual.r - expected.r) <= tolerance && Math.Abs(actual.g - expected.g) <= tolerance &&
                     Math.Abs(actual.b - expected.b) <= tolerance && Math.Abs(actual.a - expected.a) <= tolerance;

            Assert.IsTrue(ok, $"Expected ~{expected} but was {actual}.");
        }

        /// <summary>
        /// A rasterizer that fills the rectangle of every solid-filled RectShape. Good enough to test compositing.
        /// </summary>
        /// <seealso cref="Imaging.Objects.Interfaces.IShapeRasterizer" />
        internal sealed class FakeRasterizer : IShapeRasterizer
        {
            /// <summary>
            /// Gets the calls.
            /// </summary>
            /// <value>
            /// The calls.
            /// </value>
            internal int Calls { get; private set; }

            /// <inheritdoc />
            public void Rasterize(IReadOnlyList<Shape> shapes, UnmanagedImageBuffer target)
            {
                Calls++;

                foreach (var shape in shapes)
                {
                    if (shape is not RectShape rect || rect.Fill is not SolidFill solid) continue;

                    var a = (byte)(solid.Argb >> 24);
                    var r = (byte)(solid.Argb >> 16);
                    var g = (byte)(solid.Argb >> 8);
                    var b = (byte)solid.Argb;

                    for (var y = (int)rect.Y; y < (int)(rect.Y + rect.Height); y++)
                    {
                        for (var x = (int)rect.X; x < (int)(rect.X + rect.Width); x++)
                        {
                            target.SetPixel(x, y, r, g, b, a);
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        /// <summary>A command that always fails, to test that history stays consistent.</summary>
        internal sealed class FailingCommand : IDocumentCommand
        {
            /// <inheritdoc />
            public string Description => "Fail";

            /// <inheritdoc />
            public void Apply(Document document)
            {
                throw new InvalidOperationException("boom");
            }

            /// <inheritdoc />
            public void Revert(Document document)
            {
            }
        }
    }
}
