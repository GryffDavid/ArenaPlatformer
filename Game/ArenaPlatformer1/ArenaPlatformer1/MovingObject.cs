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
        public Rectangle CollisionRectangle, DestinationRectangle;
        public Vector2 Position, Velocity, Size;
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
    }
}
