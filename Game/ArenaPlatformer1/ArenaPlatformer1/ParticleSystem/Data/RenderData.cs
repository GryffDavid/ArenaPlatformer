using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArenaPlatformer1
{
    class RenderData
    {
        //What is needed to draw the objects
        public Texture2D Texture;
        public Vector2 Position;
        public float Rotation, Scale, Transparency, DrawDepth;
        public Color Color;

        //0 = None
        //1 = Flip Horizontally
        //2 = Flip Vertically
        public int Orientation;

        public bool Emissive;
        public float SourceID;
    }
}
