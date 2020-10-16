using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    public abstract class Trap
    {
        public static Map Map;
        public Texture2D Texture;
        public Vector2 Position, Velocity;
        public float Gravity = 0.6f;

        public int DetonationLimit;

        public List<Emitter> EmitterList = new List<Emitter>();

        /// <summary>
        /// X = CurrentTime, Y = MaxTime
        /// </summary>
        public Vector2 Time;

        /// <summary>
        /// X = CurrentTime, Y = MaxTime
        /// </summary>
        public Vector2 ResetTime;

        /// <summary>
        /// Whether or not the trap exists in the world any more
        /// </summary>
        public bool Exists = true;


        /// <summary>
        /// Whether or not the trap can be interacted with or not
        /// </summary>
        public bool Active = true;

        public static TrapType TrapType;
        public Rectangle CollisionRectangle, DestinationRectangle;

        public virtual void Update(GameTime gameTime)
        {
            if (Time.Y > 0)
            {
                Time.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            if (Active == false && ResetTime.Y > 0)
            {
                ResetTime.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (ResetTime.X >= ResetTime.Y)
                {
                    ResetTime.X = 0;
                    Active = true;
                }
            }
            


            bool thing = OnGround(Velocity, Position, out float groundY);

            if (Velocity.Y >= 0)
            {
                if (thing == true)
                {
                    Velocity.Y = 0;
                    Position.Y = groundY - CollisionRectangle.Height;
                }
            }

            if (thing == false)
                Velocity.Y += Gravity;

            Position += Velocity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);

            foreach (Emitter emitter in EmitterList)
            {                
                emitter.Update(gameTime);
            }
        }

        public abstract void Draw(SpriteBatch spriteBatch);

        /// <summary>
        /// Draw the collision box and other useful debug info
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="basicEffect"></param>
        public void DrawInfo(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect basicEffect)
        {
            #region Draw the collision box for debugging
            VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[4];
            int[] Indices = new int[8];

            Vertices[0] = new VertexPositionColorTexture()
            {
                Color = Color.Cyan,
                Position = new Vector3(CollisionRectangle.Left, CollisionRectangle.Top, 0),
                TextureCoordinate = new Vector2(0, 0)
            };

            Vertices[1] = new VertexPositionColorTexture()
            {
                Color = Color.Cyan,
                Position = new Vector3(CollisionRectangle.Right - 1, CollisionRectangle.Top, 0),
                TextureCoordinate = new Vector2(1, 0)
            };

            Vertices[2] = new VertexPositionColorTexture()
            {
                Color = Color.Cyan,
                Position = new Vector3(CollisionRectangle.Right - 1, CollisionRectangle.Bottom - 1, 0),
                TextureCoordinate = new Vector2(1, 1)
            };

            Vertices[3] = new VertexPositionColorTexture()
            {
                Color = Color.Cyan,
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

        /// <summary>
        /// Reset the trap so that it has to cool down
        /// </summary>
        public virtual void Reset()
        {
            DetonationLimit--;
            Active = false;
            ResetTime.X = 0;

            if (DetonationLimit <= 0)
            {
                Active = false;
                Exists = false;
            }
        }

        public bool CheckLeft(out Vector2 tPos)
        {
            Vector2 bottomLeft = new Vector2(
                Position.X - (CollisionRectangle.Width / 2) + Velocity.X - 1,
                Position.Y);

            Vector2 topLeft = new Vector2(
                Position.X - (CollisionRectangle.Width / 2) + Velocity.X - 1,
                Position.Y - CollisionRectangle.Height);

            int tileIndexX, tileIndexY;
            tPos = Vector2.Zero;

            for (var checkedTile = topLeft; ; checkedTile.Y += Map.TileSize.Y)
            {
                checkedTile.Y = Math.Min(checkedTile.Y, bottomLeft.Y);

                tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                if (Map.IsObstacle(tileIndexX, tileIndexY))
                {
                    //Map.DrawTiles[tileIndexX, tileIndexY].Color = Color.Red;
                    tPos = Map.DrawTiles[tileIndexX, tileIndexY].Position;
                    return true;
                }

                if (checkedTile.Y >= bottomLeft.Y)
                    break;
            }

            return false;
        }

        public bool CheckRight(out Vector2 tPos)
        {
            Vector2 bottomRight = new Vector2(
                Position.X + (CollisionRectangle.Width / 2) + Velocity.X + 1,
                Position.Y);

            Vector2 topRight = new Vector2(
                Position.X + (CollisionRectangle.Width / 2) + Velocity.X + 1,
                Position.Y - CollisionRectangle.Height);

            int tileIndexX, tileIndexY;
            tPos = Vector2.Zero;

            for (var checkedTile = topRight; ; checkedTile.Y += Map.TileSize.Y)
            {
                checkedTile.Y = Math.Min(checkedTile.Y, bottomRight.Y);

                tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                if (Map.IsObstacle(tileIndexX, tileIndexY))
                {
                    //Map.DrawTiles[tileIndexX, tileIndexY].Color = Color.Yellow;
                    tPos = Map.DrawTiles[tileIndexX, tileIndexY].Position;
                    return true;
                }

                if (checkedTile.Y >= bottomRight.Y)
                    break;
            }

            return false;
        }

        public bool OnGround(Vector2 velocity, Vector2 position, out float groundY)
        {
            Vector2 bottomLeft = new Vector2(
                Position.X - (CollisionRectangle.Width / 2),
                Position.Y + Velocity.Y + (CollisionRectangle.Height) + 1);

            Vector2 bottomRight = new Vector2(
                Position.X + (CollisionRectangle.Width / 2),
                Position.Y + Velocity.Y + (CollisionRectangle.Height) + 1);

            int tileIndexX, tileIndexY;

            for (var checkedTile = bottomLeft; ; checkedTile.X += Map.TileSize.X)
            {
                checkedTile.X = Math.Min(checkedTile.X, bottomRight.X);

                tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                groundY = (float)tileIndexY * Map.TileSize.Y;

                //if (Map.IsBounce(tileIndexX, tileIndexY))
                //{
                //    Velocity.Y -= 25f;
                //    return false;
                //}

                if (Map.IsObstacle(tileIndexX, tileIndexY))
                    return true;

                if (checkedTile.X >= bottomRight.X)
                    break;
            }

            return false;
        }

        public bool OnCeiling(Vector2 velocity, Vector2 position, out float tPos)
        {
            Vector2 topLeft = new Vector2(
                Position.X - (CollisionRectangle.Width / 2),
                Position.Y - CollisionRectangle.Height + Velocity.Y - 1);

            Vector2 topRight = new Vector2(
                Position.X + (CollisionRectangle.Width / 2),
                Position.Y - CollisionRectangle.Height + Velocity.Y - 1);

            int tileIndexX, tileIndexY;

            for (var checkedTile = topLeft; ; checkedTile.X += Map.TileSize.X)
            {
                checkedTile.X = Math.Min(checkedTile.X, topRight.X);

                tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                tPos = (float)tileIndexY * Map.TileSize.Y;

                if (Map.IsObstacle(tileIndexX, tileIndexY))
                    return true;

                if (checkedTile.X >= topRight.X)
                    break;
            }

            return false;
        }
    }
}
