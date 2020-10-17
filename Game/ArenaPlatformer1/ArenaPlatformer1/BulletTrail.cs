using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    public class BulletTrail
    {
        public static Texture2D Texture;
        public Vector2 Source, Destination, Direction;
        float FadeOutRate, Angle, Length, StartingLength;
        public Vector2 MiddleScale, MiddleOrigin, CapOrigin;

        public VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];
        public int[] indices = new int[6];
        public Color Color;

        public BulletTrail(Vector2 source, Vector2 destination, Color color)
        {
            Source = source;
            Destination = destination;
            FadeOutRate = 0.35f;
            Direction = Destination - Source;
            Angle = (float)Math.Atan2(Direction.Y, Direction.X);
            //Color = Color.RoyalBlue * 0.75f;
            Color = color;

            Length = Vector2.Distance(source, destination);
            StartingLength = Length;
            SetVerts();

            vertices[0].Color = Color;
            vertices[1].Color = Color*0.15f;
            vertices[2].Color = Color*0.15f;
            vertices[3].Color = Color;

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 3;
            indices[5] = 0;
        }

        public void Update(GameTime gameTime)
        {
            //Color = Color.Lerp(Color, Color.Transparent, FadeOutRate * 0.85f * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60));

            //vertices[0].Color = Color;
            //vertices[1].Color = Color;
            //vertices[2].Color = Color;
            //vertices[3].Color = Color;

            Length = MathHelper.Lerp(Length, 0, FadeOutRate * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60));
            //if (Length > 0) Length -= (StartingLength * 0.07f);

            SetVerts();
        }

        public void Draw(GraphicsDevice graphics, BasicEffect effect)
        {
            effect.VertexColorEnabled = true;
            effect.World = Matrix.CreateRotationZ(Angle + (float)(Math.PI)) *
                           Matrix.CreateTranslation(new Vector3(Destination.X, Destination.Y, 0));

            effect.Texture = Texture;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2);
            }
        }

        public void SetVerts()
        {
            vertices[0] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(0, 0, 0),
                TextureCoordinate = new Vector2(0, 0)
            };

            vertices[1] = new VertexPositionColorTexture()
            {
                Color = Color.Transparent,
                Position = new Vector3(Length, 0, 0),
                TextureCoordinate = new Vector2(1, 0)
            };

            vertices[2] = new VertexPositionColorTexture()
            {
                Color = Color.Transparent,
                Position = new Vector3(Length, 1, 0),
                TextureCoordinate = new Vector2(1, 1)
            };

            vertices[3] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(0, 1, 0),
                TextureCoordinate = new Vector2(0, 1)
            };
        }
    }
}
