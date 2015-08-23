using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace Pulse.DirectX
{
    public sealed class DxRectangle
    {
        public float X, Y, Width, Height;

        public DxRectangle()
        {

        }

        public DxRectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public void DrawSolid(PrimitiveBatch<VertexPositionColor> primitiveBatch, Color color)
        {
            VertexPositionColor p1 = new VertexPositionColor(new Vector3(X, Y + Height, 1.0f), color);
            VertexPositionColor p2 = new VertexPositionColor(new Vector3(X, Y, 1.0f), color);
            VertexPositionColor p3 = new VertexPositionColor(new Vector3(X + Width, Y, 1.0f), color);
            VertexPositionColor p4 = new VertexPositionColor(new Vector3(X + Width, Y + Height, 1.0f), color);
            primitiveBatch.DrawLine(p1, p4);
            primitiveBatch.DrawQuad(p1, p2, p3, p4);
        }

        public void DrawBorder(PrimitiveBatch<VertexPositionColor> primitiveBatch, Color color)
        {
            VertexPositionColor p1 = new VertexPositionColor(new Vector3(X, Y + Height, 1.0f), color);
            VertexPositionColor p2 = new VertexPositionColor(new Vector3(X, Y, 1.0f), color);
            VertexPositionColor p3 = new VertexPositionColor(new Vector3(X + Width, Y, 1.0f), color);
            VertexPositionColor p4 = new VertexPositionColor(new Vector3(X + Width, Y + Height, 1.0f), color);
            primitiveBatch.DrawQuad(p1, p2, p3, p4);
        }
    }
}