﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace ArenaPlatformer1
{
    class Emitter
    {
        static Random Random = new Random();
        public static Map Map;

        Texture2D Texture;
        public Vector2 PreviousPosition, AngleRange;
        public Vector2 Position, ScaleRange, TimeRange, RotationIncrementRange, SpeedRange, 
                       StartingRotationRange, EmitterDirection, EmitterVelocity, YRange, Friction;
        public float Transparency, Gravity, ActiveSeconds, Interval, EmitterSpeed,
                     EmitterAngle, EmitterGravity, FadeDelay, StartingInterval;
        public Color StartColor, EndColor;
        public bool Fade, CanBounce, AddMore, Shrink, StopBounce, HardBounce, BouncedOnGround,
                    RotateVelocity, FlipHor, FlipVer, ReduceDensity, SortParticles;
        public bool Grow, Active, Emissive, Lit;
        public int Burst;
        public double IntervalTime, CurrentTime, MaxTime;
        //public SpriteEffects Orientation = SpriteEffects.None;
        public int Orientation = 0;
        public object Tether;
        public float BounceY;
        public float DrawDepth;


        RenderData renderData;
        ParticleData gameData;

        public static UpdateManager UpdateManager;
        public static RenderManager RenderManager;

        Rectangle CollisionRectangle;

        public struct ChangeEmitter
        {
            /// <summary>
            /// How long the change is effective for
            /// X = CurrentTime, Y = MaxTime
            /// </summary>
            public Vector2 ChangeTime;

            public bool Active, shrink, grow, fade, reduceDensity, 
                        rotateVelocity, canBounce, stopBounce, 
                        hardBounce, flipVer, flipHor, emissive, lit;
            public Vector2 angleRange, timeRange, speedRange, friction, 
                           scaleRange, rotationIncrement, startingRotation;
            public float gravity, startingTransparency, 
                         fadeDelay, activeSeconds, interval;
            public Color startColor, endColor;
            public int burst;

            public void Update(GameTime gameTime)
            {
                ChangeTime.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (ChangeTime.X > ChangeTime.Y)
                {
                    Active = false;
                    ChangeTime.X = 0;
                }
            }
        }

        public ChangeEmitter CurrentChange;

        public Emitter(Texture2D texture, Vector2 position, Vector2 angleRange, Vector2 speedRange, Vector2 timeRange,
                       float startingTransparency, bool fade, Vector2 startingRotationRange, Vector2 rotationIncrement, Vector2 scaleRange,
                       Color startColor, Color endColor, float gravity, float activeSeconds, float interval, int burst, bool canBounce,
                       Vector2 yrange, bool? shrink = null, float? drawDepth = null, bool? stopBounce = null, bool? hardBounce = null,
                       Vector2? emitterSpeed = null, Vector2? emitterAngle = null, float? emitterGravity = null, bool? rotateVelocity = null,
                       Vector2? friction = null, bool? flipHor = null, bool? flipVer = null, float? fadeDelay = null, bool? reduceDensity = null,
                       bool? sortParticles = null, bool? grow = false, bool? emissive = false, bool? lit = false)
        {
            Active = true;
            Texture = texture;
            SpeedRange = speedRange;
            TimeRange = timeRange;
            Transparency = startingTransparency;
            Fade = fade;
            StartingRotationRange = startingRotationRange;
            RotationIncrementRange = rotationIncrement;
            ScaleRange = scaleRange;
            StartColor = startColor;
            EndColor = endColor;
            Position = position;
            AngleRange = angleRange;
            Gravity = gravity;
            ActiveSeconds = activeSeconds;
            Interval = interval;
            StartingInterval = interval;
            IntervalTime = Interval;
            Burst = burst;
            CanBounce = canBounce;
            

            if (lit != null)
                Lit = lit.Value;
            else
                Lit = false;


            if (emissive != null)
                Emissive = emissive.Value;
            else
                Emissive = false;




            if (grow != null)
                Grow = grow.Value;
            else
                Grow = false;

            if (shrink == null)
                Shrink = false;
            else
                Shrink = shrink.Value;

            if (drawDepth == null)
                DrawDepth = 0;
            else
                DrawDepth = drawDepth.Value;

            if (stopBounce == null)
                StopBounce = false;
            else
                StopBounce = stopBounce.Value;

            if (hardBounce == null)
                HardBounce = false;
            else
                HardBounce = hardBounce.Value;

            if (friction != null)
                Friction = friction.Value;
            else
                Friction = new Vector2(0, 0);

            if (fadeDelay != null)
                FadeDelay = fadeDelay.Value;
            else
                FadeDelay = 0;

            if (emitterSpeed != null)
                EmitterSpeed = (float)DoubleRange(emitterSpeed.Value.X, emitterSpeed.Value.Y);
            else
                EmitterSpeed = 0;

            if (emitterAngle != null)
            {
                EmitterAngle = -MathHelper.ToRadians((float)DoubleRange(emitterAngle.Value.X, emitterAngle.Value.Y));
            }
            else
            {
                EmitterAngle = 0;
            }

            if (emitterGravity != null)
                EmitterGravity = emitterGravity.Value;
            else
                EmitterGravity = 0;

            if (EmitterSpeed != 0)
            {
                EmitterDirection.X = (float)Math.Cos(EmitterAngle);
                EmitterDirection.Y = (float)Math.Sin(EmitterAngle);
                EmitterVelocity = EmitterDirection * EmitterSpeed;
                AngleRange = new Vector2(
                                -(MathHelper.ToDegrees((float)Math.Atan2(-EmitterVelocity.Y, -EmitterVelocity.X))) - 20,
                                -(MathHelper.ToDegrees((float)Math.Atan2(-EmitterVelocity.Y, -EmitterVelocity.X))) + 20);
            }

            if (rotateVelocity != null)
                RotateVelocity = rotateVelocity.Value;
            else
                RotateVelocity = false;

            if (reduceDensity != null)
                ReduceDensity = reduceDensity.Value;
            else
                ReduceDensity = false;

            if (flipHor == null)
                FlipHor = false;
            else
                FlipHor = flipHor.Value;

            if (flipVer == null)
                FlipVer = false;
            else
                FlipVer = flipVer.Value;

            if (sortParticles == null)
                SortParticles = false;
            else
                SortParticles = sortParticles.Value;

            YRange = yrange;
            BounceY = Random.Next((int)yrange.X, (int)yrange.Y);
            AddMore = true;
        }

        public void Update(GameTime gameTime)
        {
            #region Control emitter life time
            if (ActiveSeconds > 0)
            {
                CurrentTime += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (CurrentTime > ActiveSeconds * 1000)
                {
                    AddMore = false;
                }
            }
            #endregion

            #region Reduce density
            if (ReduceDensity == true)
            {
                //After halftime, begin reducing the density from 100% down to 0% as the time continues to expire                    
                //Interval = MathHelper.Lerp((float)Interval, (float)(Interval * 5), 0.0001f);
                float PercentageThrough = ((float)CurrentTime / (ActiveSeconds * 1000)) * 100;

                if (PercentageThrough >= 50)
                    Interval = StartingInterval + (Interval / 100 * PercentageThrough);
            }
            #endregion

            #region Emitter motion
            if (EmitterSpeed != 0)
            {
                EmitterVelocity.Y += EmitterGravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                Position += EmitterVelocity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                #region Emitter bouncing
                if (CanBounce == true)
                    if (Position.Y >= BounceY && BouncedOnGround == false)
                    {
                        if (HardBounce == true)
                            Position.Y -= EmitterVelocity.Y * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                        EmitterVelocity.Y = (-EmitterVelocity.Y / 3) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                        EmitterVelocity.X = (EmitterVelocity.X / 3) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                        BouncedOnGround = true;
                    }

                
                if (StopBounce == true &&
                            BouncedOnGround == true &&
                            Position.Y > BounceY)
                {
                    EmitterVelocity.Y = (-EmitterVelocity.Y / 2) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                    EmitterVelocity.X *= 0.9f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                    if (EmitterVelocity.Y < 0.2f && EmitterVelocity.Y > 0)
                    {
                        EmitterVelocity.Y = 0;
                    }

                    if (EmitterVelocity.Y > -0.2f && EmitterVelocity.Y < 0)
                    {
                        EmitterVelocity.Y = 0;
                    }

                    if (EmitterVelocity.X < 0.2f && EmitterVelocity.X > 0)
                    {
                        EmitterVelocity.X = 0;
                    }

                    if (EmitterVelocity.X > -0.2f && EmitterVelocity.X < 0)
                    {
                        EmitterVelocity.X = 0;
                    }
                } 
                #endregion

                #region Emitter collisions
                if (EmitterVelocity != Vector2.Zero)
                    CollisionRectangle = new Rectangle((int)Position.X - 1, (int)Position.Y - 1, 2, 2);

                if (EmitterVelocity.X > 0)
                {
                    CheckRightCollisions();
                }

                if (EmitterVelocity.X < 0)
                {
                    CheckLeftCollisions();
                }

                if (EmitterVelocity.Y > 0)
                {
                    CheckDownCollisions();

                    if (CheckDownCollisions() == true)
                    {
                        EmitterVelocity.X *= 0.5f;
                    }
                }

                if (EmitterVelocity.Y < 0)
                {
                    CheckUpCollisions();
                }
                #endregion
            }
            #endregion

            #region Particle orientation
            if (FlipHor == true && FlipVer == false)
            {
                Orientation = RandomOrientation(SpriteEffects.None, SpriteEffects.FlipHorizontally);
                //Get back None or FlipHor
                //0
            }

            if (FlipHor == false && FlipVer == true)
            {
                Orientation = RandomOrientation(SpriteEffects.None, SpriteEffects.FlipVertically);
                //Get back None or FlipVer
                //1
            }

            if (FlipHor == true && FlipVer == true)
            {
                Orientation = RandomOrientation(SpriteEffects.None, SpriteEffects.FlipVertically, SpriteEffects.FlipHorizontally);
                //Get back None, FlipHor, FlipVer
                //2
            }
            #endregion

            #region Add particle
            if (CurrentChange.Active == false)
            {
                IntervalTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (IntervalTime > Interval && AddMore == true)
                {
                    for (int i = 0; i < Burst; i++)
                    {
                        UpdateManager.AddParticle(
                                Texture, Position, AngleRange, SpeedRange, ScaleRange, StartColor, EndColor,
                                Gravity, Shrink, Fade, StartingRotationRange, RotationIncrementRange,
                                Transparency, TimeRange, Grow, RotateVelocity, Friction, Orientation, FadeDelay,
                                YRange, CanBounce, StopBounce, HardBounce, DrawDepth, Emissive, Lit,
                                out gameData, out renderData);

                        RenderManager.RenderDataObjects.Add(renderData);
                        UpdateManager.ParticleDataObjects.Add(gameData);
                    }

                    IntervalTime = 0;
                }
            }
            else
            {
                CurrentChange.Update(gameTime);

                IntervalTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (IntervalTime > CurrentChange.interval && AddMore == true)
                {
                    for (int i = 0; i < CurrentChange.burst; i++)
                    {
                        UpdateManager.AddParticle(
                                Texture, Position, CurrentChange.angleRange, CurrentChange.speedRange, ScaleRange, 
                                CurrentChange.startColor, CurrentChange.endColor,
                                CurrentChange.gravity, CurrentChange.shrink, CurrentChange.fade,
                                CurrentChange.startingRotation, CurrentChange.rotationIncrement,
                                CurrentChange.startingTransparency, CurrentChange.timeRange, CurrentChange.grow,
                                CurrentChange.rotateVelocity, CurrentChange.friction, Orientation, CurrentChange.fadeDelay,
                                YRange, 
                                CurrentChange.canBounce, CurrentChange.stopBounce, CurrentChange.hardBounce, 
                                DrawDepth,
                                CurrentChange.emissive, CurrentChange.lit,
                                out gameData, out renderData);

                        RenderManager.RenderDataObjects.Add(renderData);
                        UpdateManager.ParticleDataObjects.Add(gameData);
                    }

                    IntervalTime = 0;
                }
            }
            #endregion
        }

        private int RandomOrientation(params SpriteEffects[] Orientations)
        {
            return (int)Orientations[Random.Next(0, Orientations.Length)];
        }

        public double DoubleRange(double one, double two)
        {
            return one + Random.NextDouble() * (two - one);
        }

        public bool CheckDownCollisions()
        {
            foreach (Tile tile in Map.TileList)
            {
                for (int i = 0; i < CollisionRectangle.Width; i++)
                {
                    if (tile.CollisionRectangle.Contains(
                        new Point(
                        (int)(CollisionRectangle.Left + i),
                        (int)(CollisionRectangle.Bottom + EmitterVelocity.Y + 1))))
                    {
                        Position.Y += (tile.CollisionRectangle.Top - CollisionRectangle.Bottom);
                        EmitterVelocity.Y = -EmitterVelocity.Y * 0.85f;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool CheckRightCollisions()
        {
            foreach (Tile tile in Map.TileList)
            {
                for (int i = 0; i < CollisionRectangle.Height; i++)
                {
                    if (tile.CollisionRectangle.Contains(
                        new Point(
                            (int)(CollisionRectangle.Right + EmitterVelocity.X),
                            (int)(CollisionRectangle.Top + i))))
                    {
                        Position.X -= (CollisionRectangle.Right - tile.CollisionRectangle.Left);
                        EmitterVelocity.X = -EmitterVelocity.X * 0.85f;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool CheckLeftCollisions()
        {
            foreach (Tile tile in Map.TileList)
            {
                for (int i = 0; i < CollisionRectangle.Height; i++)
                {
                    if (tile.CollisionRectangle.Contains(
                        new Point(
                            (int)(CollisionRectangle.Left + EmitterVelocity.X - 1),
                            (int)(CollisionRectangle.Top + i))))
                    {
                        Position.X += (tile.CollisionRectangle.Right - CollisionRectangle.Left);
                        EmitterVelocity.X = -EmitterVelocity.X * 0.85f;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool CheckUpCollisions()
        {
            foreach (Tile tile in Map.TileList)
            {
                for (int i = 0; i < CollisionRectangle.Width; i++)
                {
                    if (EmitterVelocity.Y < 0)
                        if (tile.CollisionRectangle.Contains(
                            new Point(
                            (int)(CollisionRectangle.Left + i),
                            (int)(CollisionRectangle.Top + EmitterVelocity.Y - 1))))
                        {
                            Position.Y += (tile.CollisionRectangle.Bottom - CollisionRectangle.Top);
                            EmitterVelocity.Y = -EmitterVelocity.Y * 0.85f;
                            return true;
                        }
                }
            }
            return false;
        }
    }
}
