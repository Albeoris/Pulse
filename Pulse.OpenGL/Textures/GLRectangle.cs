using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Pulse.OpenGL
{
    public sealed class GLRectangle
    {
        public float X, Y, Width, Height;

        public GLRectangle()
        {

        }

        public GLRectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public void DrawSolid(Color color)
        {
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(color);
            GL.Vertex2(X, Y + Height);
            GL.Vertex2(X, Y);
            GL.Vertex2(X + Width, Y);
            GL.Vertex2(X + Width, Y + Height);
            GL.End();
        }

        public void DrawBorder(Color color)
        {
            GL.Begin(PrimitiveType.LineLoop);
            GL.Color4(color);
            GL.Vertex2(X, Y + Height);
            GL.Vertex2(X, Y);
            GL.Vertex2(X + Width, Y);
            GL.Vertex2(X + Width, Y + Height);
            GL.End();
        }
    }
}