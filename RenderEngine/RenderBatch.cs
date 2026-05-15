/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        RenderBatch.cs
 * PURPOSE:     Batches 2D and 3D draw calls to minimize state changes and draw calls.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Numerics;
using ExtendedSystemObjects;

namespace RenderEngine
{
    /// <summary>
    /// Batches 2D and 3D draw calls to minimize state changes and draw calls.
    /// </summary>
    public sealed class RenderBatch
    {
        // --- 2D Storage ---

        /// <summary>
        /// The solid line vertices
        /// </summary>
        public readonly UnmanagedList<float> SolidLineVertices = new(8192);

        /// <summary>
        /// The solid triangle vertices
        /// </summary>
        public readonly UnmanagedList<float> SolidTriangleVertices = new(8192);

        /// <summary>
        /// The textured batches
        /// </summary>
        public readonly Dictionary<int, List<float>> TexturedBatches = new();

        // --- 3D Storage (Ported Tech) ---
        /// <summary>
        /// The solid 3D vertices
        ///  Layout: X, Y, Z, R, G, B, A (7 floats)
        /// </summary>
        public readonly UnmanagedList<float> Solid3DVertices = new(16384);

        // 
        /// <summary>
        /// The textured 3D batches
        /// Layout: X, Y, Z, U, V, R, G, B, A (9 floats) - Grouped by TextureId
        /// </summary>
        public readonly Dictionary<int, List<float>> Textured3DBatches = new();

        /// <summary>
        /// The host actions
        /// </summary>
        public readonly List<Action> HostActions = new();

        /// <summary>
        /// Adds the colored line.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        /// <param name="a">a.</param>
        public void AddColoredLine(float x, float y, int r, int g, int b, int a)
        {
            AddColoredVertex(x, y, r, g, b, a, SolidLineVertices);
        }

        /// <summary>
        /// Adds the solid triangle vertex.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        /// <param name="a">a.</param>
        public void AddSolidTriangleVertex(float x, float y, int r, int g, int b, int a)
        {
            AddColoredVertex(x, y, r, g, b, a, SolidTriangleVertices);
        }

        /// <summary>
        /// Adds the colored triangle.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="v3">The v3.</param>
        public void AddColoredTriangle(
            (float x, float y, int r, int g, int b, int a) v1,
            (float x, float y, int r, int g, int b, int a) v2,
            (float x, float y, int r, int g, int b, int a) v3)
        {
            AddColoredVertex(v1.x, v1.y, v1.r, v1.g, v1.b, v1.a, SolidTriangleVertices);
            AddColoredVertex(v2.x, v2.y, v2.r, v2.g, v2.b, v2.a, SolidTriangleVertices);
            AddColoredVertex(v3.x, v3.y, v3.r, v3.g, v3.b, v3.a, SolidTriangleVertices);
        }

        /// <summary>
        /// Adds the solid quad.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        /// <param name="p4">The p4.</param>
        /// <param name="color">The color.</param>
        public void AddSolidQuad(
            (int x, int y) p1, (int x, int y) p2,
            (int x, int y) p3, (int x, int y) p4,
            (int r, int g, int b, int a) color)
        {
            var v1 = (p1.x, p1.y, color.r, color.g, color.b, color.a);
            var v2 = (p2.x, p2.y, color.r, color.g, color.b, color.a);
            var v3 = (p3.x, p3.y, color.r, color.g, color.b, color.a);
            var v4 = (p4.x, p4.y, color.r, color.g, color.b, color.a);

            AddColoredTriangle(v1, v2, v3);
            AddColoredTriangle(v1, v3, v4);
        }

        /// <summary>
        /// Adds the textured quad.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        /// <param name="p4">The p4.</param>
        /// <param name="textureId">The texture identifier.</param>
        public void AddTexturedQuad((int x, int y) p1, (int x, int y) p2, (int x, int y) p3, (int x, int y) p4, int textureId)
        {
            AddTexturedTriangle(p1, p2, p3, textureId);
            AddTexturedTriangle(p1, p3, p4, textureId);
        }

        /// <summary>
        /// Adds the textured triangle.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        /// <param name="textureId">The texture identifier.</param>
        public void AddTexturedTriangle((int x, int y) p1, (int x, int y) p2, (int x, int y) p3, int textureId)
        {
            if (!TexturedBatches.TryGetValue(textureId, out var list))
            {
                list = new List<float>(4096);
                TexturedBatches[textureId] = list;
            }

            AddTexturedVertex(p1.x, p1.y, 0f, 0f, list);
            AddTexturedVertex(p2.x, p2.y, 1f, 0f, list);
            AddTexturedVertex(p3.x, p3.y, 0f, 1f, list);
        }

        /// <summary>
        /// Adds a solid colored triangle to the 3D batch.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="v3">The v3.</param>
        /// <param name="color">The color.</param>
        public void AddSolid3DTriangle(Vector3 v1, Vector3 v2, Vector3 v3, (int r, int g, int b, int a) color)
        {
            AddColoredVertex3D(v1, color, Solid3DVertices);
            AddColoredVertex3D(v2, color, Solid3DVertices);
            AddColoredVertex3D(v3, color, Solid3DVertices);
        }

        /// <summary>
        /// Adds a textured triangle to the 3D batch, grouped by texture ID.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="uv1">The uv1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="uv2">The uv2.</param>
        /// <param name="v3">The v3.</param>
        /// <param name="uv3">The uv3.</param>
        /// <param name="textureId">The texture identifier.</param>
        public void AddTextured3DTriangle(Vector3 v1, Vector2 uv1, Vector3 v2, Vector2 uv2, Vector3 v3, Vector2 uv3, int textureId)
        {
            if (!Textured3DBatches.TryGetValue(textureId, out var list))
            {
                list = new List<float>(4096);
                Textured3DBatches[textureId] = list;
            }

            AddTexturedVertex3D(v1, uv1, list);
            AddTexturedVertex3D(v2, uv2, list);
            AddTexturedVertex3D(v3, uv3, list);
        }

        /// <summary>
        /// Helper for 3D textured vertex: X, Y, Z, U, V, R, G, B, A (9 floats).
        /// Defaulting color to White (1,1,1,1).
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="uv">The uv.</param>
        /// <param name="targetList">The target list.</param>
        private void AddTexturedVertex3D(Vector3 pos, Vector2 uv, List<float> targetList)
        {
            targetList.Add(pos.X);
            targetList.Add(pos.Y);
            targetList.Add(pos.Z);
            targetList.Add(uv.X);
            targetList.Add(uv.Y);
            targetList.Add(1f); // R
            targetList.Add(1f); // G
            targetList.Add(1f); // B
            targetList.Add(1f); // A
        }

        /// <summary>
        /// Helper for 3D colored vertex: X, Y, Z, R, G, B, A (7 floats).
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="c">The c.</param>
        /// <param name="targetList">The target list.</param>
        private void AddColoredVertex3D(Vector3 pos, (int r, int g, int b, int a) c, UnmanagedList<float> targetList)
        {
            targetList.Add(pos.X);
            targetList.Add(pos.Y);
            targetList.Add(pos.Z);
            targetList.Add(c.r / 255f);
            targetList.Add(c.g / 255f);
            targetList.Add(c.b / 255f);
            targetList.Add(c.a / 255f);
        }

        /// <summary>
        /// Adds the textured vertex.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="u">The u.</param>
        /// <param name="v">The v.</param>
        /// <param name="targetList">The target list.</param>
        private void AddTexturedVertex(float x, float y, float u, float v, List<float> targetList)
        {
            targetList.Add(x);
            targetList.Add(y);
            targetList.Add(u);
            targetList.Add(v);
        }

        /// <summary>
        /// Adds the colored vertex.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        /// <param name="a">a.</param>
        /// <param name="targetList">The target list.</param>
        private void AddColoredVertex(float x, float y, int r, int g, int b, int a, UnmanagedList<float> targetList)
        {
            targetList.Add(x);
            targetList.Add(y);
            targetList.Add(r / 255f);
            targetList.Add(g / 255f);
            targetList.Add(b / 255f);
            targetList.Add(a / 255f);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            SolidLineVertices.Clear();
            SolidTriangleVertices.Clear();
            Solid3DVertices.Clear();

            foreach (var list in TexturedBatches.Values)
            {
                list.Clear();
            }

            foreach (var list in Textured3DBatches.Values)
            {
                list.Clear();
            }

            HostActions.Clear();
        }

        /// <summary>
        /// Adds the host action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <exception cref="System.ArgumentNullException">action</exception>
        public void AddHostAction(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            HostActions.Add(action);
        }
    }
}
