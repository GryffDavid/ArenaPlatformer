﻿using System;
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
        public Rectangle CollisionRectangle, DestinationRectangle;
        public Vector2 Position, PreviousPosition, Velocity, Size;
        public float Gravity;

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

        public List<CollisionData> CollisionDataList = new List<CollisionData>();

        Vector2 HalfSize, Center;

        public Color Color = Color.White;
        
        /// <summary>
        /// If true, the object will not budge when collided with. The other object will resolve the collision
        /// If false, then this object will also account for collisions with other objects
        /// </summary>
        public bool IsKinematic = false;

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

        public List<Vector2> Areas = new List<Vector2>();
        public List<int> IDsInAreas = new List<int>();


        public void Initialize()
        {
            CollisionRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        }

        public void Update(GameTime gameTime)
        {
            Position += Velocity;
            CollisionRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            HalfSize = new Vector2(CollisionRectangle.Width / 2, CollisionRectangle.Height / 2);
            Center = new Vector2(CollisionRectangle.Center.X, CollisionRectangle.Center.Y);

            if (CollisionDataList.Count > 0)
            {
                Color = Color.White * 0.5f;
            }
            else
            {
                Color = Color.White;
            }

            PreviousPosition = Position;
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
            
        }

        public bool OverlapsSigned(MovingObject other, out Vector2 overlap)
        {
            overlap = Vector2.Zero;

            if (HalfSize.X == 0.0f || HalfSize.Y == 0.0f || other.HalfSize.X == 0.0f || other.HalfSize.Y == 0.0f
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