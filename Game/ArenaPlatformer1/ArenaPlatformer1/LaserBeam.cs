using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    public class LaserBeam
    {
        public static Map Map;
        public static Texture2D Texture;
        public Vector2 Source, Destination, Direction;
        float Length, Rotation;

        public VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];
        public int[] indices = new int[6];
        public Color Color;

        public Ray Ray;

        public LaserBeam(Vector2 source, float rotation, Color color)
        {
            float? hitDist;
            Source = source;
            Rotation = rotation - (float)Math.PI / 2;
            Direction = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));
            Color = color;

            Ray = new Ray(new Vector3(source, 0), new Vector3(Direction, 0));

            //USE RAY TO GET END POINT

            List<Tile> TileIntersections = new List<Tile>();

            for (int x = 0; x < Map.DrawTiles.GetLength(0); x++)
            {
                for (int y = 0; y < Map.DrawTiles.GetLength(1); y++)
                {
                    if (Map.DrawTiles[x, y] != null &&
                        Map.DrawTiles[x, y].BoundingBox.Intersects(Ray) < float.PositiveInfinity)
                    {
                        TileIntersections.Add(Map.DrawTiles[x, y]);
                    }
                }
            }

            CollisionSolid colObject = new CollisionSolid();

            colObject = TileIntersections.OrderBy(Collision => Collision.BoundingBox.Intersects(Ray)).FirstOrDefault();
            hitDist = colObject.BoundingBox.Intersects(Ray);

            Destination = new Vector2(source.X + (Ray.Direction.X * (float)hitDist),
                                      source.Y + (Ray.Direction.Y * (float)hitDist));


            //Destination = Source + (Direction * 32);
            Length = Vector2.Distance(Source, Destination);


            //SET LENGTH
            SetVerts();

            vertices[0].Color = Color * 0.15f;
            vertices[1].Color = Color;
            vertices[2].Color = Color;
            vertices[3].Color = Color * 0.15f;

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 3;
            indices[5] = 0;
        }

        public void Draw(GraphicsDevice graphics, BasicEffect effect)
        {
            effect.VertexColorEnabled = true;
            effect.World = Matrix.CreateRotationZ(Rotation + (float)(Math.PI)) *
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
                Color = Color.Transparent,
                Position = new Vector3(0, 0, 0),
                TextureCoordinate = new Vector2(0, 0)
            };

            vertices[1] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(Length, 0, 0),
                TextureCoordinate = new Vector2(1, 0)
            };

            vertices[2] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(Length, 1, 0),
                TextureCoordinate = new Vector2(1, 1)
            };

            vertices[3] = new VertexPositionColorTexture()
            {
                Color = Color.Transparent,
                Position = new Vector3(0, 1, 0),
                TextureCoordinate = new Vector2(0, 1)
            };
        }
    }
}
