/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        RenderBatch.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;

namespace RenderEngine
{
    public sealed class RenderBatch
    {
        public readonly List<float> SolidLineVertices = new();
        public readonly List<float> SolidTriangleVertices = new();
        public readonly List<float> TexVertices = new();
        public readonly List<Action> HostActions = new();

        public void AddColoredLine(
            float x0, float y0,
            float x1, float y1,
            int r, int g, int b, int a)
        {
            AddColoredVertex(x0, y0, r, g, b, a, SolidLineVertices);
            AddColoredVertex(x1, y1, r, g, b, a, SolidLineVertices);
        }

        public void AddColoredTriangle(
            (float x, float y, int r, int g, int b, int a) v1,
            (float x, float y, int r, int g, int b, int a) v2,
            (float x, float y, int r, int g, int b, int a) v3)
        {
            AddColoredVertex(v1.x, v1.y, v1.r, v1.g, v1.b, v1.a, SolidTriangleVertices);
            AddColoredVertex(v2.x, v2.y, v2.r, v2.g, v2.b, v2.a, SolidTriangleVertices);
            AddColoredVertex(v3.x, v3.y, v3.r, v3.g, v3.b, v3.a, SolidTriangleVertices);
        }

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

        public void AddTexturedQuad(
            (int x, int y) p1, (int x, int y) p2,
            (int x, int y) p3, (int x, int y) p4,
            int textureId) // Note: Batching assumes the same textureId for the whole batch for now
        {
            AddTexturedTriangle(p1, p2, p3);
            AddTexturedTriangle(p1, p3, p4);
        }
        public void AddTexturedTriangle(
            (int x, int y) p1, (int x, int y) p2, (int x, int y) p3)
        {
            AddTexturedVertex(p1.x, p1.y, 0f, 0f);
            AddTexturedVertex(p2.x, p2.y, 1f, 0f);
            AddTexturedVertex(p3.x, p3.y, 0f, 1f);
        }

        private void AddTexturedVertex(float x, float y, float u, float v)
        {
            TexVertices.Add(x);
            TexVertices.Add(y);
            TexVertices.Add(u);
            TexVertices.Add(v);
        }

        private void AddColoredVertex(
            float x, float y, int r, int g, int b, int a, List<float> targetList)
        {
            targetList.Add(x);
            targetList.Add(y);
            targetList.Add(r / 255f);
            targetList.Add(g / 255f);
            targetList.Add(b / 255f);
            targetList.Add(a / 255f);
        }
        public void Clear()
        {
            TexVertices.Clear();
            SolidLineVertices.Clear();
            SolidTriangleVertices.Clear();
            HostActions.Clear();
        }

        public void AddHostAction(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            HostActions.Add(action);
        }
    }
}
