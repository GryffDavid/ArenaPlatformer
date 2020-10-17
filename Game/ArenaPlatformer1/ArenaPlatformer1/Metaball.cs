using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    class Metaball : MovingObject
    {
        public static Texture2D MetaballTexture, MetaballSquashed;
        public float Orientation;
        public float Scale, Gravity;
        static Random Random = new Random();
        public Liquid Source;
        public bool Active;
        public float CurTime, MaxTime;

        public Metaball(Liquid source, Vector2 position, Vector2 velocity, float orientation, float scale)
        {
            Active = true;
            Source = source;
            Position = position;
            Velocity = velocity;
            Orientation = orientation;
            Texture = MetaballTexture;
            Scale = scale;
            Gravity = 0.45f;
            MaxTime = 6000f;
        }

        public override void Update(GameTime gameTime)
        {
            CurTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurTime >= MaxTime)
            {
                Scale -= 0.0015f;
            }

            Velocity.Y += Gravity;

            base.Update(gameTime);
            
            if (PushesBottomTile == true ||
                PushesTopTile == true)
            {
                if (Math.Abs(Velocity.Y) > 8)
                {
                    if (Scale >= 1.25f)
                    {
                        Source.AddMetaballs(Position, 5, new Vector2(Scale * 0.5f), new Vector2(-5, 5), new Vector2(-5, -1));
                        Active = false;
                    }
                }

                Velocity.Y = -Velocity.Y * (0.15f + (float)DoubleRange(0, 0.1));
            }

            if (PushesBottomTile == true)
            {
                Velocity.X *= 0.90f + (float)DoubleRange(0, 0.1);
                Texture = MetaballSquashed;
            }
            else
            {
                Texture = MetaballTexture;
            }

            if (PushesLeftTile == true ||
                PushesRightTile == true)
            {
                Velocity.X = -Velocity.X * (0.15f + (float)DoubleRange(0, 0.1));
            }

            if (Velocity.Y > 0.05f)
                Orientation = (float)Math.Atan2(Velocity.Y, Velocity.X);
            else
                Orientation = (float)-Math.PI / 2;
        }
        
        public double DoubleRange(double one, double two)
        {
            return one + Random.NextDouble() * (two - one);
        }
    }
}
