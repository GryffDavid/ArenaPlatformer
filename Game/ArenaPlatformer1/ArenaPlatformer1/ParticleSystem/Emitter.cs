using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        public Color StartColor, EndColor, ThirdColor;
        public bool Fade, CanBounce, AddMore, Shrink, StopBounce, HardBounce, BouncedOnGround,
                    RotateVelocity, FlipHor, FlipVer, ReduceDensity, SortParticles, Emissive;
        public bool Grow, Active;
        public string TextureName;
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

        public Emitter(Texture2D texture, Vector2 position, Vector2 angleRange, Vector2 speedRange, Vector2 timeRange,
                       float startingTransparency, bool fade, Vector2 startingRotationRange, Vector2 rotationIncrement, Vector2 scaleRange,
                       Color startColor, Color endColor, float gravity, float activeSeconds, float interval, int burst, bool canBounce,
                       Vector2 yrange, bool? shrink = null, float? drawDepth = null, bool? stopBounce = null, bool? hardBounce = null,
                       Vector2? emitterSpeed = null, Vector2? emitterAngle = null, float? emitterGravity = null, bool? rotateVelocity = null,
                       Vector2? friction = null, bool? flipHor = null, bool? flipVer = null, float? fadeDelay = null, bool? reduceDensity = null,
                       bool? sortParticles = null, bool? grow = false, Vector4? thirdColor = null, bool? emissive = false)
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

            Emissive = emissive.Value;

            if (grow != null)
                Grow = grow.Value;
            else
                Grow = false;

            if (thirdColor != null)
                ThirdColor = new Color(thirdColor.Value.X, thirdColor.Value.Y, thirdColor.Value.Z, thirdColor.Value.W);
            else
                ThirdColor = Color.Transparent;

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
            IntervalTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (ActiveSeconds > 0)
            {
                CurrentTime += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (CurrentTime > ActiveSeconds * 1000)
                {
                    AddMore = false;
                }
            }


            if (EmitterSpeed != 0)
            {
                EmitterVelocity.Y += EmitterGravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                Position += EmitterVelocity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

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
            }


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

            if (IntervalTime > Interval && AddMore == true)
            {
                for (int i = 0; i < Burst; i++)
                {
                    UpdateManager.AddParticle(
                            Texture, Position, AngleRange, SpeedRange, ScaleRange, StartColor, EndColor,
                            Gravity, Shrink, Fade, StartingRotationRange, RotationIncrementRange,
                            Transparency, TimeRange, Grow, RotateVelocity, Friction, Orientation, FadeDelay,
                            YRange, CanBounce, StopBounce, HardBounce, Emissive,
                            out gameData, out renderData);

                    RenderManager.RenderDataObjects.Add(renderData);
                    UpdateManager.ParticleDataObjects.Add(gameData);
                }

                IntervalTime = 0;
            }
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
