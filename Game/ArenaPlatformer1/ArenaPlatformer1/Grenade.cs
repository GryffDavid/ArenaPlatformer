using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    public class Grenade
    {
        static Random Random = new Random();
        public static Map Map;
        public static Texture2D GrenadeTexture;
        public Vector2 Position, Velocity;

        /// <summary>
        /// The time until the grenade detonates. X = CurrentTime, Y = MaxTime
        /// </summary>
        public Vector2 Time;

        public object Source;
        public float Speed, Rotation, RotationIncrement;
        public bool Active = true;        
        public int BlastRadius = 200;

        public Rectangle DestinationRectangle, CollisionRectangle;

        //TODO: Maybe have a mechanic that allows players to either throw or drop a grenade. 
        //Allowing them to drop it on a bounce pad OR throw it at another player
        
        public Grenade(Vector2 position, Vector2 direction, float speed, object source)
        {
            Position = position;
            Speed = speed;
            Velocity = direction * Speed;
            Source = source;

            Rotation = MathHelper.ToRadians(Random.Next(0, 360));
            RotationIncrement = MathHelper.ToRadians(3);

            Time.Y = 1000f;
        }

        public void Update(GameTime gameTime)
        {
            Time.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (Time.X >= Time.Y)
            {
                Active = false;
            }

            //Position += Velocity * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60f);
            Velocity.Y += 0.6f;

            bool leftCol, rightCol;
            bool upCol, downCol;

            leftCol = CheckLeft(out float lPos);
            rightCol = CheckRight(out float rPos);
            downCol = OnGround(out float gPos);
            upCol = OnCeiling(out float cPos);

            #region Left Collisions
            if (Velocity.X < 0)
            {
                if (leftCol == true)
                {
                    Velocity.X = -Velocity.X * 0.65f;
                    Position.X = lPos + 64 + DestinationRectangle.Width / 2;
                }
            }
            #endregion

            #region Right Collisions
            if (Velocity.X > 0)
            {
                if (rightCol == true)
                {
                    Velocity.X = -Velocity.X * 0.65f;
                    Position.X = rPos - DestinationRectangle.Width / 2 - 1;
                }
            }
            #endregion

            #region Down Collisions
            if (Velocity.Y > 0)
            {
                if (downCol == true)
                {
                    Velocity.Y = -Velocity.Y * 0.65f;
                    Velocity.X *= 0.65f;
                    Position.Y = gPos - (DestinationRectangle.Height / 2);
                }
            }
            #endregion

            #region Up Collisions
            if (Velocity.Y < 0)
            {
                if (upCol == true)
                {
                    Position.Y = cPos + 64 + (DestinationRectangle.Height);
                    Velocity.Y = -Velocity.Y * 0.65f;
                }
            }
            #endregion

            if (upCol == false && downCol == false)
            {
                Position.Y += Velocity.Y * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
            }

            if ((leftCol == false && Velocity.X < 0) ||
                (rightCol == false && Velocity.X > 0))
            {
                Position.X += Velocity.X * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
            }

            #region Rotation
            if (Math.Abs(Velocity.X) >= 1 || Math.Abs(Velocity.Y) >= 1)
            {
                float dir = (float)Math.Atan2(-Velocity.Y, -Velocity.X);
                RotationIncrement = MathHelper.ToRadians(Random.Next(0, 4)) * (Velocity.X + Velocity.Y);
                Rotation += RotationIncrement;
            }
            #endregion

            DestinationRectangle = new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                GrenadeTexture.Width, GrenadeTexture.Height);

            CollisionRectangle = new Rectangle(
                (int)(Position.X - (GrenadeTexture.Width / 2)), 
                (int)(Position.Y - GrenadeTexture.Height / 2), 
                GrenadeTexture.Width, GrenadeTexture.Height);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(GrenadeTexture, DestinationRectangle, null, Color.White, Rotation, new Vector2(GrenadeTexture.Width/2, GrenadeTexture.Height/2), SpriteEffects.None, 0);
        }

        public void DrawInfo(GraphicsDevice graphics, BasicEffect basicEffect)
        {
            #region Draw the collision box for debugging
            VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[4];
            int[] Indices = new int[8];

            Vertices[0] = new VertexPositionColorTexture()
            {
                Color = Color.Red,
                Position = new Vector3(CollisionRectangle.Left, CollisionRectangle.Top, 0),
                TextureCoordinate = new Vector2(0, 0)
            };

            Vertices[1] = new VertexPositionColorTexture()
            {
                Color = Color.Red,
                Position = new Vector3(CollisionRectangle.Right - 1, CollisionRectangle.Top, 0),
                TextureCoordinate = new Vector2(1, 0)
            };

            Vertices[2] = new VertexPositionColorTexture()
            {
                Color = Color.Red,
                Position = new Vector3(CollisionRectangle.Right - 1, CollisionRectangle.Bottom - 1, 0),
                TextureCoordinate = new Vector2(1, 1)
            };

            Vertices[3] = new VertexPositionColorTexture()
            {
                Color = Color.Red,
                Position = new Vector3(CollisionRectangle.Left, CollisionRectangle.Bottom - 1, 0),
                TextureCoordinate = new Vector2(0, 1)
            };

            Indices[0] = 0;
            Indices[1] = 1;

            Indices[2] = 2;
            Indices[3] = 3;

            Indices[4] = 0;

            Indices[5] = 2;
            Indices[6] = 0;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserIndexedPrimitives(PrimitiveType.LineStrip, Vertices, 0, 4, Indices, 0, 6, VertexPositionColorTexture.VertexDeclaration);
            }
            #endregion
        }


        public bool CheckLeft(out float tPos)
        {
            Vector2 bottomLeft = new Vector2(
                Position.X - (CollisionRectangle.Width / 2) + Velocity.X - 1,
                Position.Y + (CollisionRectangle.Height / 2) - 1);

            Vector2 topLeft = new Vector2(
                Position.X - (CollisionRectangle.Width / 2) + Velocity.X - 1,
                Position.Y - (CollisionRectangle.Height / 2) + 1);

            int tileIndexX, tileIndexY;
            tPos = 0;

            for (var checkedTile = topLeft; ; checkedTile.Y += Map.TileSize.Y)
            {
                checkedTile.Y = Math.Min(checkedTile.Y, bottomLeft.Y);

                tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                if (Map.IsObstacle(tileIndexX, tileIndexY))
                {
                    tPos = Map.DrawTiles[tileIndexX, tileIndexY].Position.X;
                    return true;
                }

                if (checkedTile.Y >= bottomLeft.Y)
                    break;
            }

            return false;
        }

        public bool CheckRight(out float tPos)
        {
            Vector2 bottomRight = new Vector2(
                Position.X + (CollisionRectangle.Width / 2) + Velocity.X + 1,
                Position.Y + (CollisionRectangle.Height / 2) - 1);

            Vector2 topRight = new Vector2(
                Position.X + (CollisionRectangle.Width / 2) + Velocity.X + 1,
                Position.Y - (CollisionRectangle.Height / 2) + 1);

            int tileIndexX, tileIndexY;
            tPos = 0f;

            for (var checkedTile = topRight; ; checkedTile.Y += Map.TileSize.Y)
            {
                checkedTile.Y = Math.Min(checkedTile.Y, bottomRight.Y);

                tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                if (Map.IsObstacle(tileIndexX, tileIndexY))
                {
                    tPos = Map.DrawTiles[tileIndexX, tileIndexY].Position.X;
                    return true;
                }

                if (checkedTile.Y >= bottomRight.Y)
                    break;
            }

            return false;
        }

        public bool OnGround(out float groundY)
        {
            Vector2 bottomLeft = new Vector2(
                Position.X,
                Position.Y + (CollisionRectangle.Height / 2) + Velocity.Y + 1);

            Vector2 bottomRight = new Vector2(
                Position.X + (CollisionRectangle.Width / 2),
                Position.Y + (CollisionRectangle.Height / 2) + Velocity.Y + 1);

            int tileIndexX, tileIndexY;

            for (var checkedTile = bottomLeft; ; checkedTile.X += Map.TileSize.X)
            {
                checkedTile.X = Math.Min(checkedTile.X, bottomRight.X);

                tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                groundY = (float)tileIndexY * Map.TileSize.Y;

                if (Map.IsGround(tileIndexX, tileIndexY))
                {
                    return true;
                }

                if (Map.IsBounce(tileIndexX, tileIndexY) == true)
                {
                    Velocity.Y = -25f;
                    return false;
                }

                if (checkedTile.X >= bottomRight.X)
                    break;
            }


            bottomLeft = new Vector2(
                Position.X,
                Position.Y + +(CollisionRectangle.Height / 2) + Velocity.Y + 1);

            bottomRight = new Vector2(
                Position.X - (CollisionRectangle.Width / 2),
                Position.Y + +(CollisionRectangle.Height / 2) + Velocity.Y + 1);

            for (var checkedTile = bottomLeft; ; checkedTile.X -= Map.TileSize.X)
            {
                checkedTile.X = Math.Min(checkedTile.X, bottomRight.X);

                tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                groundY = (float)tileIndexY * Map.TileSize.Y;

                if (Map.IsGround(tileIndexX, tileIndexY))
                {
                    return true;
                }

                if (Map.IsBounce(tileIndexX, tileIndexY) == true)
                {
                    Velocity.Y = -25f;
                    return false;
                }

                if (checkedTile.X <= bottomRight.X)
                    break;
            }

            return false;
        }

        public bool OnCeiling(out float ceilingY)
        {
            Vector2 topLeft = new Vector2(
                Position.X - (CollisionRectangle.Width / 2),
                Position.Y - (CollisionRectangle.Height / 2) + Velocity.Y - 2);

            Vector2 topRight = new Vector2(
                Position.X + (CollisionRectangle.Width / 2),
                Position.Y - (CollisionRectangle.Height / 2) + Velocity.Y - 2);

            int tileIndexX, tileIndexY;

            for (var checkedTile = topLeft; ; checkedTile.X += Map.TileSize.X)
            {
                checkedTile.X = Math.Min(checkedTile.X, topRight.X);

                tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                ceilingY = (float)tileIndexY * Map.TileSize.Y;

                if (Map.IsObstacle(tileIndexX, tileIndexY))
                    return true;

                if (checkedTile.X >= topRight.X)
                    break;
            }

            return false;
        }
    }
}
