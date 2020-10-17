using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    public class MovingObject
    {
        public static Map Map;

        public Texture2D Texture;
        public Rectangle CollisionRectangle;
        public Vector2 Position, PreviousPosition, Velocity, Size, HalfSize, Center;

        public Color Color = Color.White;

        #region Push Directions
        public bool PushesRight = false;
        public bool PushesLeft = false;
        public bool PushesBottom = false;
        public bool PushesTop = false;

        public bool PushedTop = false;
        public bool PushedBottom = false;
        public bool PushedRight = false;
        public bool PushedLeft = false;

        public bool PushesLeftObject = false;
        public bool PushesRightObject = false;
        public bool PushesBottomObject = false;
        public bool PushesTopObject = false;

        public bool PushedLeftObject = false;
        public bool PushedRightObject = false;
        public bool PushedBottomObject = false;
        public bool PushedTopObject = false;

        public bool PushesRightTile = false;
        public bool PushesLeftTile = false;
        public bool PushesBottomTile = false;
        public bool PushesTopTile = false;

        public bool PushedTopTile = false;
        public bool PushedBottomTile = false;
        public bool PushedRightTile = false;
        public bool PushedLeftTile = false; 
        #endregion
        

        public void Initialize()
        {
            //HalfSize = Size / 2;
        }

        public virtual void Update(GameTime gameTime)
        {
            HalfSize = Size / 2;
            PreviousPosition = Position;
            
            CheckTiles(gameTime);
            
            CollisionRectangle = new Rectangle(
                (int)(Position.X - (Size.X / 2)),
                (int)(Position.Y - (Size.Y / 2)),
                (int)Size.X, (int)Size.Y);

            Center = new Vector2(CollisionRectangle.Center.X, CollisionRectangle.Center.Y);

            PushesRight = PushesRightObject || PushesRightTile;
            PushesLeft = PushesLeftObject || PushesLeftTile;
            PushesBottom = PushesBottomObject || PushesBottomTile;
            PushesTop = PushesTopObject || PushesTopTile;
        }
        
        private void CheckTiles(GameTime gameTime)
        {
            PushesLeftTile = false;
            PushesRightTile = false;
            PushesBottomTile = false;
            PushesTopTile = false;

            bool leftCol, rightCol;
            bool upCol, downCol;

            leftCol = CheckLeft(out float lPos);
            #region Left Collisions
            if (Velocity.X < 0)
            {
                if (leftCol == true)
                {
                    //Velocity.X = -Velocity.X * 0.65f;
                    PushesLeftTile = true;
                    Position.X = lPos + 64 + (CollisionRectangle.Width / 2);
                }
            }
            #endregion

            rightCol = CheckRight(out float rPos);
            #region Right Collisions
            if (Velocity.X > 0)
            {
                if (rightCol == true)
                {
                    //Velocity.X = -Velocity.X * 0.65f;
                    PushesRightTile = true;
                    Position.X = rPos - (CollisionRectangle.Width / 2) - 1;
                }
            }
            #endregion

            downCol = OnGround(out float gPos);
            #region Down Collisions
            if (Velocity.Y > 0)
            {
                if (downCol == true)
                {
                    //Velocity.Y = -Velocity.Y * 0.65f;
                    //Velocity.X *= 0.65f;
                    PushesBottomTile = true;
                    Position.Y = gPos - (CollisionRectangle.Height / 2);
                }
            }
            #endregion

            upCol = OnCeiling(out float cPos);
            #region Up Collisions
            if (Velocity.Y < 0)
            {
                if (upCol == true)
                {
                    PushesTopTile = true;
                    Position.Y = cPos + 64 + (CollisionRectangle.Height / 2);
                    //Velocity.Y = -Velocity.Y * 0.65f;
                }
            }
            #endregion
            
            if (upCol == false && 
                downCol == false)
            {
                Position.Y += Velocity.Y * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
            }

            if ((leftCol == false && Velocity.X < 0) ||
                (rightCol == false && Velocity.X > 0))
            {
                Position.X += Velocity.X * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
            }
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
                {
                    return true;
                }

                if (checkedTile.X >= topRight.X)
                    break;
            }

            return false;
        }
    }
}
