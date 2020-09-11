using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArenaPlatformer1
{
    class ParticleData
    {
        //What is needed to perform updates
        public Vector2 Position, Angle, Velocity, Friction;
        public Color StartColor, EndColor;
        public float
            Rotation, RotationIncrement,
            CurrentTime, MaxTime,
            FadeDelay, CurrentFadeDelay,
            CurrentScale, MaxScale,
            Gravity, StartingTransparency,
            BounceY;

        public float SourceID;
        public bool Active, Shrink, Grow, Fade, RotateVelocity, CanBounce, StopBounce, HardBounce, HasBounced, Emissive;

        public void Update(GameTime gameTime)
        {
            if (FadeDelay > 0)
            {
                CurrentFadeDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (CurrentFadeDelay >= FadeDelay)
                {
                    if (CurrentTime < MaxTime)
                        CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
            }
            else
            {
                if (CurrentTime < MaxTime)
                    CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            Velocity.Y += Gravity;

            #region Handle Friction
            if (Friction != new Vector2(0, 0))
            {
                Velocity.Y = MathHelper.Lerp(Velocity.Y, 0, Friction.Y * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
                Velocity.X = MathHelper.Lerp(Velocity.X, 0, Friction.X * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
            }
            #endregion

            #region Handle bouncing
            if (CanBounce == true)
                if (Position.Y >= BounceY && HasBounced == false)
                {
                    if (HardBounce == true)
                        Position.Y -= Velocity.Y;

                    RotateVelocity = false;
                    Velocity.Y = (-Velocity.Y / 3);
                    Velocity.X = (Velocity.X / 3);
                    RotationIncrement = (RotationIncrement * 3);
                    HasBounced = true;
                }

            if (StopBounce == true &&
                HasBounced == true &&
                Position.Y > BounceY)
            {
                Velocity.Y = (-Velocity.Y / 2);

                Velocity.X *= 0.9f;

                RotationIncrement = MathHelper.Lerp(RotationIncrement, 0, 0.2f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));

                if (Velocity.Y < 0.2f && Velocity.Y > 0)
                {
                    Velocity.Y = 0;
                }

                if (Velocity.Y > -0.2f && Velocity.Y < 0)
                {
                    Velocity.Y = 0;
                }

                if (Velocity.X < 0.2f && Velocity.X > 0)
                {
                    Velocity.X = 0;
                }

                if (Velocity.X > -0.2f && Velocity.X < 0)
                {
                    Velocity.X = 0;
                }
            }
            #endregion

            if (CurrentTime >= MaxTime)
            {
                Active = false;
            }
        }
    }
}
