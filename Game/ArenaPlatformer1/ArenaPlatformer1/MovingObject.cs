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
        #region CollisionData Struct
        public struct CollisionData
        {
            public CollisionData(
                MovingObject other,
                Vector2 overlap = default(Vector2),

                Vector2 speed1 = default(Vector2),
                Vector2 speed2 = default(Vector2),

                Vector2 oldPos1 = default(Vector2),
                Vector2 oldPos2 = default(Vector2),

                Vector2 pos1 = default(Vector2),
                Vector2 pos2 = default(Vector2))
            {
                Other = other;
                Overlap = overlap;

                Speed1 = speed1;
                Speed2 = speed2;

                PreviousPosition1 = oldPos1;
                PreviousPosition2 = oldPos2;

                Position1 = pos1;
                Position2 = pos2;
            }

            public MovingObject Other;
            public Vector2 Overlap;
            public Vector2 Speed1, Speed2,
                           Position1, Position2,
                           PreviousPosition1, PreviousPosition2;
        }
        #endregion

        public List<CollisionData> CollisionDataList = new List<CollisionData>();

        public Rectangle CollisionRectangle;
        public Vector2 Position, PreviousPosition, Velocity, Size, HalfSize, Center;
        public bool IsKinematic = false;

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

        public List<Vector2> Areas = new List<Vector2>();
        public List<int> IDsInAreas = new List<int>();

        public void Initialize()
        {
            CollisionRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            HalfSize = Size / 2;
        }

        public void Update(GameTime gameTime)
        {
            PreviousPosition = Position;

            CheckPhysics();

            Position += Velocity;
            CollisionRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);            
            Center = new Vector2(CollisionRectangle.Center.X, CollisionRectangle.Center.Y);
        }

        private void CheckPhysics()
        {
            if (IsKinematic == true)
                return;

            PushedBottomObject = PushesBottomObject;
            PushedRightObject = PushesRightObject;
            PushedLeftObject = PushesLeftObject;
            PushedTopObject = PushesTopObject;

            PushesBottomObject = false;
            PushesRightObject = false;
            PushesLeftObject = false;
            PushesTopObject = false;

            Vector2 offsetSum = Vector2.Zero;

            for (int i = 0; i < CollisionDataList.Count; i++)
            {
                var other = CollisionDataList[i].Other;
                var data = CollisionDataList[i];
                Vector2 Overlap = data.Overlap - offsetSum;

                if (Overlap.X == 0f)
                {
                    if (other.Center.X > Center.X)
                    {
                        PushesRightObject = true;
                        Velocity.X = Math.Min(Velocity.X, 0f);
                    }
                    else
                    {
                        PushesLeftObject = true;
                        Velocity.X = Math.Max(Velocity.X, 0.0f);
                    }
                    continue;
                }
                else if (Overlap.Y == 0f)
                {
                    if (other.Center.Y > Center.Y)
                    {
                        PushesTopObject = true;
                        Velocity.Y = Math.Min(Velocity.Y, 0f);
                    }
                    else
                    {
                        PushesBottomObject = true;
                        Velocity.Y = Math.Max(Velocity.Y, 0f);
                    }
                    continue;
                }

                Vector2 absSpeed1 = new Vector2(Math.Abs(data.Position1.X - data.PreviousPosition1.X),
                                                Math.Abs(data.Position1.Y - data.PreviousPosition1.Y));

                Vector2 absSpeed2 = new Vector2(Math.Abs(data.Position2.X - data.PreviousPosition2.X),
                                                Math.Abs(data.Position2.Y - data.PreviousPosition2.Y));

                Vector2 speedSum = absSpeed1 + absSpeed2;


                Vector2 speedRatio;

                if (other.IsKinematic)
                {
                    speedRatio.X = speedRatio.Y = 1.0f;
                }
                else
                {
                    if (speedSum.X == 0 && speedSum.Y == 0)
                    {
                        speedRatio.X = speedRatio.Y = 0.5f;
                    }
                    else if (speedSum.X == 0)
                    {
                        speedRatio.X = 0.5f;
                        speedRatio.Y = absSpeed1.Y / speedSum.Y;
                    }
                    else if (speedSum.Y == 0)
                    {
                        speedRatio.Y = 0.5f;
                        speedRatio.X = absSpeed1.X / speedSum.X;
                    }
                    else
                    {
                        speedRatio.X = absSpeed1.X / speedSum.X;
                        speedRatio.Y = absSpeed1.Y / speedSum.Y;
                    }
                }

                Vector2 Offset = Overlap * speedRatio;
                
                bool overlappedLastFrameX = Math.Abs(data.PreviousPosition1.X - data.PreviousPosition2.X) < (other.HalfSize.X + HalfSize.X);
                bool overlappedLastFrameY = Math.Abs(data.PreviousPosition1.Y - data.PreviousPosition2.Y) < (other.HalfSize.Y + HalfSize.Y);

                offsetSum = Vector2.Zero;

                if ((!overlappedLastFrameX && overlappedLastFrameY) || 
                    (!overlappedLastFrameX && !overlappedLastFrameY && Math.Abs(Overlap.X) <= Math.Abs(Overlap.Y)))
                {
                    Position.X += Offset.X;
                    offsetSum.X += Offset.X;

                    if (Overlap.X < 0.0f)
                    {
                        PushesRightObject = true;
                        Velocity.X = Math.Min(Velocity.X, 0.0f);
                    }
                    else
                    {
                        PushesLeftObject = true;
                        Velocity.X = Math.Max(Velocity.X, 0.0f);
                    }
                }
                else
                {
                    Position.Y += Offset.Y;
                    offsetSum.Y += Offset.Y;

                    if (Overlap.Y < 0.0f)
                    {
                        PushesTopObject = true;
                        Velocity.Y = Math.Min(Velocity.Y, 0.0f);
                    }
                    else
                    {
                        PushesBottomObject = true;
                        Velocity.Y = Math.Max(Velocity.Y, 0.0f);
                    }
                }
            }
        }

        private void CheckTiles()
        {

        }

        public bool OverlapsSigned(MovingObject other, out Vector2 overlap)
        {
            overlap = Vector2.Zero;

            if (HalfSize.X == 0.0f || 
                HalfSize.Y == 0.0f || 
                other.HalfSize.X == 0.0f || 
                other.HalfSize.Y == 0.0f
                || Math.Abs(Center.X - other.Center.X) > HalfSize.X + other.HalfSize.X
                || Math.Abs(Center.Y - other.Center.Y) > HalfSize.Y + other.HalfSize.Y)
                return false;

            overlap = new Vector2(
                Math.Sign(Center.X - other.Center.X) * ((other.HalfSize.X + HalfSize.X) - Math.Abs(Center.X - other.Center.X)),
                Math.Sign(Center.Y - other.Center.Y) * ((other.HalfSize.Y + HalfSize.Y) - Math.Abs(Center.Y - other.Center.Y)));

            return true;
        }

        public bool HasCollisionDataFor(MovingObject other)
        {
            for (int i = 0; i < CollisionDataList.Count; i++)
            {
                if (CollisionDataList[i].Other == other)
                    return true;
            }

            return false;
        }
    }
}
