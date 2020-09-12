using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    public class MovingPlatform : MovingObject
    {
        //Handle moving platform collisions just like tiles but accounting for an offset from the top left corner
        //and then just handle them like a regular 64,64 tile

        public Texture2D Texture;
        public Vector2 Position, Size, Speed, Velocity;
        public Rectangle DestinationRectangle, CollisionRectangle;

        public static Map Map;

        public MovingPlatform(Vector2 velocity)
        {
            Velocity = velocity;
        }

        public void LoadContent(ContentManager content)
        {

        }
        
        public void Update(GameTime gameTime)
        {
            if (Velocity.X > 0)
            {
                if (CheckRight(out Vector2 rPos) == true)
                {
                    Velocity.X = -Velocity.X;
                    //Position.X = rPos.X - DestinationRectangle.Width / 2 - 1;
                }

            }

            if (Velocity.X < 0)
            {
                if (CheckLeft(out Vector2 lPos) == true)
                {
                    Velocity.X = -Velocity.X;
                    //Position.X = lPos.X + 64 + DestinationRectangle.Width / 2;
                }
            }

            if (Velocity.Y > 0)
            {
                bool thing = OnGround(Velocity, Position, out float groundY);

                if (thing == true)
                {
                    Velocity.Y = -Velocity.Y;
                    //Position.Y = groundY - (DestinationRectangle.Height / 2);
                }
            }

            if (Velocity.Y < 0)
            {
                if (OnCeiling(Velocity, Position, out float cPos) == true)
                {
                    //Position.Y = cPos + 64 + (DestinationRectangle.Height);
                    Velocity.Y = -Velocity.Y;
                }
            }

            Position += Velocity;

            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            CollisionRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

            //foreach (Tile tile in Map.TileList)
            //{
            //    if (tile.CollisionRectangle.Intersects(CollisionRectangle))
            //    {
            //        Speed = -Speed;
            //    }
            //}
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, Color.White);
        }

        public bool CheckLeft(out Vector2 tPos)
        {
            Vector2 bottomLeft = new Vector2(
                Position.X + Velocity.X - 1,
                Position.Y + CollisionRectangle.Height);

            Vector2 topLeft = new Vector2(
                Position.X + Velocity.X - 1,
                Position.Y);

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
                Position.X + CollisionRectangle.Width + Velocity.X + 1,
                Position.Y + CollisionRectangle.Height);

            Vector2 topRight = new Vector2(
                Position.X + CollisionRectangle.Width + Velocity.X + 1,
                Position.Y);

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
                Position.X - CollisionRectangle.Width,
                Position.Y + CollisionRectangle.Height + Velocity.Y + 1);

            Vector2 bottomRight = new Vector2(
                Position.X + CollisionRectangle.Width,
                Position.Y + CollisionRectangle.Height + Velocity.Y + 1);

            int tileIndexX, tileIndexY;

            for (var checkedTile = bottomLeft; ; checkedTile.X += Map.TileSize.X)
            {
                checkedTile.X = Math.Min(checkedTile.X, bottomRight.X);

                tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                groundY = (float)tileIndexY * Map.TileSize.Y;

                if (Map.IsBounce(tileIndexX, tileIndexY))
                {
                    Velocity.Y -= 25f;
                    return false;
                }

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
                Position.X - CollisionRectangle.Width,
                Position.Y + Velocity.Y - 1);

            Vector2 topRight = new Vector2(
                Position.X + CollisionRectangle.Width,
                Position.Y + Velocity.Y - 1);

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
