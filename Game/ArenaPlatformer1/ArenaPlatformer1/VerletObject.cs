using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    class VerletObject
    {
        #region Stick
        public class Stick
        {
            public Node Point1, Point2;
            public float Length;
            public float Rotation;
            public Rectangle DestinationRectangle;
        } 
        #endregion

        #region Node
        public class Node
        {
            public Vector2 CurrentPosition, PreviousPosition, Velocity;
            public float Friction = 0.999f;
            public bool Pinned;
            public static Map Map;
            public Rectangle CollisionRectangle;

            public Node()
            {
                CollisionRectangle = new Rectangle((int)CurrentPosition.X - 3, (int)CurrentPosition.Y - 3, 5, 5);
            }

            public bool CheckLeft(out float tPos)
            {
                Vector2 bottomLeft = new Vector2(
                    CurrentPosition.X - (CollisionRectangle.Width / 2) + Velocity.X - 1,
                    CurrentPosition.Y + (CollisionRectangle.Height / 2) - 1);

                Vector2 topLeft = new Vector2(
                    CurrentPosition.X - (CollisionRectangle.Width / 2) + Velocity.X - 1,
                    CurrentPosition.Y - (CollisionRectangle.Height / 2) + 1);

                int tileIndexX, tileIndexY;
                tPos = 0;

                for (var checkedTile = topLeft; ; checkedTile.Y += Map.TileSize.Y)
                {
                    checkedTile.Y = Math.Min(checkedTile.Y, bottomLeft.Y);

                    tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                    tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                    if (Map.IsObstacle(tileIndexX, tileIndexY))
                    {
                        tPos = Map.DrawTiles[tileIndexX, tileIndexY].Position.X + 64 + 3;
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
                    CurrentPosition.X + (CollisionRectangle.Width / 2) + Velocity.X + 1,
                    CurrentPosition.Y + (CollisionRectangle.Height / 2) - 1);

                Vector2 topRight = new Vector2(
                    CurrentPosition.X + (CollisionRectangle.Width / 2) + Velocity.X + 1,
                    CurrentPosition.Y - (CollisionRectangle.Height / 2) + 1);

                int tileIndexX, tileIndexY;
                tPos = 0f;

                for (var checkedTile = topRight; ; checkedTile.Y += Map.TileSize.Y)
                {
                    checkedTile.Y = Math.Min(checkedTile.Y, bottomRight.Y);

                    tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                    tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                    if (Map.IsObstacle(tileIndexX, tileIndexY))
                    {
                        tPos = Map.DrawTiles[tileIndexX, tileIndexY].Position.X - 3;
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
                   CurrentPosition.X,
                   CurrentPosition.Y + (CollisionRectangle.Height / 2) + Velocity.Y + 1);

                Vector2 bottomRight = new Vector2(
                    CurrentPosition.X + (CollisionRectangle.Width / 2),
                    CurrentPosition.Y + (CollisionRectangle.Height / 2) + Velocity.Y + 1);

                int tileIndexX, tileIndexY;

                for (var checkedTile = bottomLeft; ; checkedTile.X += Map.TileSize.X)
                {
                    checkedTile.X = Math.Min(checkedTile.X, bottomRight.X);

                    tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                    tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                    groundY = (float)tileIndexY * Map.TileSize.Y - 3;

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

                return false;
            }

            public bool OnCeiling(out float ceilingY)
            {
                Vector2 topLeft = new Vector2(
                    CurrentPosition.X - (CollisionRectangle.Width / 2),
                    CurrentPosition.Y - (CollisionRectangle.Height / 2) + Velocity.Y - 2);

                Vector2 topRight = new Vector2(
                    CurrentPosition.X + (CollisionRectangle.Width / 2),
                    CurrentPosition.Y - (CollisionRectangle.Height / 2) + Velocity.Y - 2);

                int tileIndexX, tileIndexY;

                for (var checkedTile = topLeft; ; checkedTile.X += Map.TileSize.X)
                {
                    checkedTile.X = Math.Min(checkedTile.X, topRight.X);

                    tileIndexX = Map.GetMapTileXAtPoint((int)checkedTile.X);
                    tileIndexY = Map.GetMapTileYAtPoint((int)checkedTile.Y);

                    ceilingY = (float)tileIndexY * Map.TileSize.Y + 64 + 3;

                    if (Map.IsObstacle(tileIndexX, tileIndexY))
                        return true;

                    if (checkedTile.X >= topRight.X)
                        break;
                }

                return false;
            }
        }
        #endregion

        public static Random Random = new Random();
        float Bounce = 0.6f;
        float Gravity = 0.06f;
        float Friction = 0.999f;

        public Vector2 YRange;
        public float BounceY, MaxY;

        public List<Node> Nodes = new List<Node>();
        public List<Stick> Sticks = new List<Stick>();

        double Time;

        public VerletObject(Vector2 position, Vector2 velocity)
        {
            Nodes.Add(new Node()
            {
                CurrentPosition = position,
                PreviousPosition = position + velocity,
                Pinned = false
            });

            Nodes.Add(new Node()
            {
                CurrentPosition = position,
                PreviousPosition = position,
                Pinned = false
            });

            Sticks.Add(new Stick()
            {
                Length = 15, 
                Point1 = Nodes[0],
                Point2 = Nodes[1]
            });
        }

        public void Initialize()
        {

        }

        public virtual void Update(GameTime gameTime)
        {
            Time += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (Time > 16)
            {
                UpdateNodes(gameTime);
                

                for (int i = 0; i < 10; i++)
                {
                    UpdateSticks(gameTime);
                    ConstrainNodes(gameTime);
                }

                Time = 0;
            }

            foreach (Stick stick in Sticks)
            {
                Vector2 dir = stick.Point2.CurrentPosition - stick.Point1.CurrentPosition;
                float rot = (float)Math.Atan2(dir.Y, dir.X);

                stick.Rotation = rot;

                stick.DestinationRectangle = new Rectangle(
                        (int)stick.Point1.CurrentPosition.X,
                        (int)stick.Point1.CurrentPosition.Y,
                        (int)(14), (int)(5));
            }
        }

        public void UpdateSticks(GameTime gameTime)
        {
            foreach (Stick stick in Sticks)
            {
                Vector2 directioon = stick.Point1.CurrentPosition - stick.Point2.CurrentPosition;

                float currentLength = directioon.Length();

                if (currentLength != stick.Length)
                {
                    directioon.Normalize();

                    if (stick.Point2.Pinned == false)
                        stick.Point2.CurrentPosition += (directioon * (currentLength - stick.Length) / 2);

                    if (stick.Point1.Pinned == false)
                        stick.Point1.CurrentPosition -= (directioon * (currentLength - stick.Length) / 2);
                }
            }
        }

        public void ConstrainNodes(GameTime gameTime)
        {
            foreach (Node node in Nodes)
            {
                if (node.Velocity != Vector2.Zero)
                {
                    if (node.Velocity.Y > 0)
                    {
                        bool thing = node.OnGround(out float groundY);

                        if (thing == true)
                        {
                            node.CurrentPosition.Y = groundY;
                            node.PreviousPosition.Y = node.CurrentPosition.Y + node.Velocity.Y * Bounce;
                            node.Velocity.X *= 0.5f;
                            node.Friction = 0.92f;
                        }
                    }

                    if (node.Velocity.Y < 0)
                    {
                        bool thing = node.OnCeiling(out float ceilingY);

                        if (thing == true)
                        {
                            node.CurrentPosition.Y = ceilingY;
                            node.PreviousPosition.Y = node.CurrentPosition.Y + node.Velocity.Y * Bounce;
                            //node.Velocity.X *= 0.5f;
                        }
                    }


                    if (node.Velocity.X < 0)
                    {
                        if (node.CheckLeft(out float lPos) == true)
                        {
                            node.CurrentPosition.X = lPos;
                            node.PreviousPosition.X = node.CurrentPosition.X + node.Velocity.X * Bounce;
                        }
                    }

                    if (node.Velocity.X > 0)
                    {
                        if (node.CheckRight(out float rPos) == true)
                        {
                            node.CurrentPosition.X = rPos;
                            node.PreviousPosition.X = node.CurrentPosition.X + node.Velocity.X * Bounce;
                        }
                    }
                }
            }
        }

        public void UpdateNodes(GameTime gameTime)
        {
            foreach (Node node in Nodes)
            {
                if (node.Pinned == false)
                {
                    node.Velocity = (node.CurrentPosition - node.PreviousPosition) * node.Friction;
                    node.PreviousPosition = node.CurrentPosition;
                    node.Velocity.Y += Gravity * (float)Time;
                    node.CurrentPosition += node.Velocity;

                    //if (node.CurrentPosition.Y >= BounceY)
                    //{
                    //    Friction = 0.92f;
                    //}
                    //else
                    //{
                    //    Friction = 0.999f;
                    //}
                }
            }
        }

        public void Draw(Texture2D texture, SpriteBatch spriteBatch)
        {
            foreach (Stick stick in Sticks)
            {
                spriteBatch.Draw(texture, stick.DestinationRectangle, null,
                    Color.White, stick.Rotation, new Vector2(0, texture.Height / 2), SpriteEffects.None, 0);
            }
        }
    }
}
