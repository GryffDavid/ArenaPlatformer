using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace ArenaPlatformer1
{
    public class Emitter
    {
        static Random Random = new Random();
        public static Map Map;
        public static UpdateManager UpdateManager;
        public static RenderManager RenderManager;

        RenderData renderData;
        ParticleData gameData;

        Texture2D Texture;
        Rectangle CollisionRectangle;
        public Color StartColor, EndColor;
        public Vector2 PreviousPosition, AngleRange, Position, ScaleRange, TimeRange, 
                       RotationIncrementRange, SpeedRange, StartingRotationRange, EmitterDirection, 
                       Velocity, YRange, Friction;
        public float Transparency, Gravity, ActiveSeconds, Interval, EmitterSpeed,
                     EmitterAngle, EmitterGravity, FadeDelay, StartingInterval, BounceY, DrawDepth;        
        public bool Fade, CanBounce, AddMore, Shrink, StopBounce, HardBounce, BouncedOnGround,
                    RotateVelocity, FlipHor, FlipVer, ReduceDensity, SortParticles, Grow, Active, Emissive, Lit;
        public double IntervalTime, CurrentTime, MaxTime;
        public object Tether;
        public int Burst;
        public int Orientation = 0;
        
        private ChangeEmitter _changeEmitter;
        public ChangeEmitter CurrentChange
        {
            get { return _changeEmitter; }
            set
            {
                _changeEmitter.ChangeTime = value.ChangeTime;

                #region Boolean Changes
                ////Active
                //if (value.Active == null)
                //    _changeEmitter.Active = false;
                //else
                _changeEmitter.Active = value.Active;

                //Shrink
                if (value.shrink == null)
                    _changeEmitter.shrink = Shrink;
                else
                    _changeEmitter.shrink = value.shrink.Value;

                //Grow
                if (value.grow == null)
                    _changeEmitter.grow = Grow;
                else
                    _changeEmitter.grow = value.grow.Value;

                //Fade
                if (value.fade == null)
                    _changeEmitter.fade = Fade;
                else
                    _changeEmitter.fade = value.fade.Value;

                //Reduce Density
                if (value.reduceDensity == null)
                    _changeEmitter.reduceDensity = ReduceDensity;
                else
                    _changeEmitter.reduceDensity = value.reduceDensity.Value;

                //Rotate Velocity
                if (value.rotateVelocity == null)
                    _changeEmitter.rotateVelocity = RotateVelocity;
                else
                    _changeEmitter.rotateVelocity = value.rotateVelocity.Value;

                //Can Bounce
                if (value.canBounce == null)
                    _changeEmitter.canBounce = CanBounce;
                else
                    _changeEmitter.canBounce = value.canBounce.Value;

                //Stop Bounce
                if (value.stopBounce == null)
                    _changeEmitter.stopBounce = StopBounce;
                else
                    _changeEmitter.stopBounce = value.stopBounce.Value;

                //Hard Bounce
                if (value.hardBounce == null)
                    _changeEmitter.hardBounce = HardBounce;
                else
                    _changeEmitter.hardBounce = value.hardBounce.Value;

                //Flip Vertically
                if (value.flipVer == null)
                    _changeEmitter.flipVer = FlipVer;
                else
                    _changeEmitter.flipVer = value.flipVer.Value;

                //Flip Horizontally
                if (value.flipHor == null)
                    _changeEmitter.flipHor = FlipHor;
                else
                    _changeEmitter.flipHor = value.flipHor.Value;

                //Emissive
                if (value.emissive == null)
                    _changeEmitter.emissive = Emissive;
                else
                    _changeEmitter.emissive = value.emissive.Value;

                //Lit
                if (value.lit == null)
                    _changeEmitter.lit = Lit;
                else
                    _changeEmitter.lit = value.lit.Value;
                #endregion

                #region Vector2 Changes
                //Angle Range
                if (value.angleRange == null)
                    _changeEmitter.angleRange = AngleRange;
                else
                    _changeEmitter.angleRange = value.speedRange.Value;

                //Time Range
                if (value.timeRange == null)
                    _changeEmitter.timeRange = TimeRange;
                else
                    _changeEmitter.timeRange = value.timeRange.Value;

                //Speed Range
                if (value.speedRange == null)
                    _changeEmitter.speedRange = SpeedRange;
                else
                    _changeEmitter.speedRange = value.speedRange.Value;

                //Friction
                if (value.friction == null)
                    _changeEmitter.friction = Friction;
                else
                    _changeEmitter.friction = value.friction.Value;

                //Scale Range
                if (value.scaleRange == null)
                    _changeEmitter.scaleRange = ScaleRange;
                else
                    _changeEmitter.scaleRange = value.scaleRange.Value;

                //Rotation Increment
                if (value.rotationIncrement == null)
                    _changeEmitter.rotationIncrement = RotationIncrementRange;
                else
                    _changeEmitter.rotationIncrement = value.rotationIncrement.Value;

                //Starting Rotation
                if (value.startingRotation == null)
                    _changeEmitter.startingRotation = StartingRotationRange;
                else
                    _changeEmitter.startingRotation = value.startingRotation.Value;

                //YRange
                if (value.yRange == null)
                    _changeEmitter.yRange = YRange;
                else
                    _changeEmitter.yRange = value.yRange.Value;
                #endregion

                #region Float Changes
                //Gravity
                if (value.gravity == null)
                    _changeEmitter.gravity = Gravity;
                else
                    _changeEmitter.gravity = value.gravity.Value;

                //Starting Transparency
                if (value.startingTransparency == null)
                    _changeEmitter.startingTransparency = Transparency;
                else
                    _changeEmitter.startingTransparency = value.startingTransparency.Value;

                //Fade Delay
                if (value.fadeDelay == null)
                    _changeEmitter.fadeDelay = FadeDelay;
                else
                    _changeEmitter.fadeDelay = value.fadeDelay.Value;

                //Active Seconds
                if (value.activeSeconds == null)
                    _changeEmitter.activeSeconds = ActiveSeconds;
                else
                    _changeEmitter.activeSeconds = value.activeSeconds.Value;

                //Interval
                if (value.interval == null)
                    _changeEmitter.interval = Interval;
                else
                    _changeEmitter.interval = value.interval.Value;
                #endregion

                //StartColor
                if (value.startColor == null)
                    _changeEmitter.startColor = StartColor;
                else
                    _changeEmitter.startColor = value.startColor.Value;

                //EndColor
                if (value.endColor == null)
                    _changeEmitter.endColor = EndColor;
                else
                    _changeEmitter.endColor = value.endColor.Value;

                //Burst
                if (value.burst == null)
                    _changeEmitter.burst = Burst;
                else
                    _changeEmitter.burst = value.burst.Value;

                //DrawDepth
                if (value.drawDepth == null)
                    _changeEmitter.drawDepth = DrawDepth;
                else
                    _changeEmitter.drawDepth = value.drawDepth.Value;
            }
        }

        #region ChangeEmitter Struct
        public struct ChangeEmitter
        {
            /// <summary>
            /// How long the change is effective for
            /// X = CurrentTime, Y = MaxTime
            /// </summary>
            public Vector2 ChangeTime;
            public bool Active;
            public bool? shrink, grow, fade, reduceDensity,
                        rotateVelocity, canBounce, stopBounce,
                        hardBounce, flipVer, flipHor, emissive, lit;
            public Vector2? angleRange, timeRange, speedRange, friction,
                           scaleRange, rotationIncrement, startingRotation, yRange;
            public float? gravity, startingTransparency,
                         fadeDelay, activeSeconds, interval, drawDepth;
            public Color? startColor, endColor;
            public int? burst;

            public void Update(GameTime gameTime)
            {
                ChangeTime.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (ChangeTime.X > ChangeTime.Y)
                {
                    Active = false;
                    ChangeTime.X = 0;
                }
            }

            public void Activate()
            {
                Active = true;
            }

            public void Deactivate()
            {
                Active = false;
            }
        } 
        #endregion
        
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
            PreviousPosition = position;
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
                Velocity = EmitterDirection * EmitterSpeed;
                AngleRange = new Vector2(
                                -(MathHelper.ToDegrees((float)Math.Atan2(-Velocity.Y, -Velocity.X))) - 20,
                                -(MathHelper.ToDegrees((float)Math.Atan2(-Velocity.Y, -Velocity.X))) + 20);
            }

            if (rotateVelocity != null)
                RotateVelocity = rotateVelocity.Value;
            else
                RotateVelocity = false;

            //if (RotateVelocity == true)
            //{
            //    AngleRange = new Vector2(
            //                    -(MathHelper.ToDegrees((float)Math.Atan2(-Velocity.Y, -Velocity.X))) - 20,
            //                    -(MathHelper.ToDegrees((float)Math.Atan2(-Velocity.Y, -Velocity.X))) + 20);
            //}
            
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
                Velocity.Y += EmitterGravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                Position += Velocity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                #region Emitter bouncing
                if (CanBounce == true)
                    if (Position.Y >= BounceY && BouncedOnGround == false)
                    {
                        if (HardBounce == true)
                            Position.Y -= Velocity.Y * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                        Velocity.Y = (-Velocity.Y / 3) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                        Velocity.X = (Velocity.X / 3) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                        BouncedOnGround = true;
                    }

                
                if (StopBounce == true &&
                    BouncedOnGround == true &&
                    Position.Y > BounceY)
                {
                    Velocity.Y = (-Velocity.Y / 2) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                    Velocity.X *= 0.9f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

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

                //#region Emitter collisions
                if (Velocity != Vector2.Zero)
                {
                    if (Velocity.X > 0)
                    {
                        if (CheckRight(out float rPos) == true)
                        {
                            Velocity.X = -Velocity.X * 0.5f;
                            Position.X = rPos - CollisionRectangle.Width / 2 - 1;
                        }

                    }

                    if (Velocity.X < 0)
                    {
                        if (CheckLeft(out float lPos) == true)
                        {
                            Velocity.X = -Velocity.X * 0.65f;
                            Position.X = lPos + 64 + CollisionRectangle.Width / 2;
                        }
                    }

                    if (Velocity.Y > 0)
                    {
                        bool thing = OnGround(out float groundY);

                        if (thing == true)
                        {
                            Velocity.Y = -Velocity.Y * 0.65f;
                            Velocity.X *= 0.5f;
                            Position.Y = groundY - CollisionRectangle.Height / 2;
                        }
                    }

                    if (Velocity.Y < 0)
                    {
                        if (OnCeiling(out float cPos) == true)
                        {
                            Position.Y = cPos + 64;
                            Velocity.X *= 0.5f;
                            Velocity.Y = -Velocity.Y * 0.65f;
                            //Position.Y = cPos + CollisionRectangle.Height / 2;
                        }
                    }
                }
                //#endregion
            }
            #endregion

            #region Add particle
            if (CurrentChange.Active == false)
            {
                IntervalTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (IntervalTime > Interval && AddMore == true)
                {
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

                    Vector2 offset = (PreviousPosition - Position) / Burst;

                    //Vector2 offset = Vector2.Zero;

                    for (int i = 0; i < Burst; i++)
                    {
                        UpdateManager.AddParticle(
                                Texture, Position + offset * i, AngleRange, SpeedRange, ScaleRange, StartColor, EndColor,
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
                _changeEmitter.Update(gameTime);

                IntervalTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (IntervalTime > CurrentChange.interval && AddMore == true)
                {
                    #region Particle orientation
                    if (CurrentChange.flipHor == true && CurrentChange.flipVer == false)
                    {
                        Orientation = RandomOrientation(SpriteEffects.None, SpriteEffects.FlipHorizontally);
                    }

                    if (CurrentChange.flipHor == false && CurrentChange.flipVer == true)
                    {
                        Orientation = RandomOrientation(SpriteEffects.None, SpriteEffects.FlipVertically);
                    }

                    if (CurrentChange.flipHor == true && CurrentChange.flipVer == true)
                    {
                        Orientation = RandomOrientation(SpriteEffects.None, SpriteEffects.FlipVertically, SpriteEffects.FlipHorizontally);
                    }
                    #endregion

                    for (int i = 0; i < CurrentChange.burst; i++)
                    {
                        UpdateManager.AddParticle(
                                Texture, Position, 
                                CurrentChange.angleRange.Value, 
                                CurrentChange.speedRange.Value, 
                                CurrentChange.scaleRange.Value, 
                                CurrentChange.startColor.Value, 
                                CurrentChange.endColor.Value,
                                CurrentChange.gravity.Value, 
                                CurrentChange.shrink.Value, 
                                CurrentChange.fade.Value,
                                CurrentChange.startingRotation.Value, 
                                CurrentChange.rotationIncrement.Value,
                                CurrentChange.startingTransparency.Value, 
                                CurrentChange.timeRange.Value, 
                                CurrentChange.grow.Value,
                                CurrentChange.rotateVelocity.Value, 
                                CurrentChange.friction.Value, 
                                Orientation, 
                                CurrentChange.fadeDelay.Value,
                                CurrentChange.yRange.Value, 
                                CurrentChange.canBounce.Value, 
                                CurrentChange.stopBounce.Value, 
                                CurrentChange.hardBounce.Value, 
                                CurrentChange.drawDepth.Value,
                                CurrentChange.emissive.Value, 
                                CurrentChange.lit.Value,
                                out gameData, out renderData);

                        RenderManager.RenderDataObjects.Add(renderData);
                        UpdateManager.ParticleDataObjects.Add(gameData);
                    }

                    IntervalTime = 0;
                }
            }
            #endregion

            PreviousPosition = Position;
            CollisionRectangle = new Rectangle((int)Position.X - 1, (int)Position.Y - 1, 2, 2);
        }

        private int RandomOrientation(params SpriteEffects[] Orientations)
        {
            return (int)Orientations[Random.Next(0, Orientations.Length)];
        }

        public double DoubleRange(double one, double two)
        {
            return one + Random.NextDouble() * (two - one);
        }

        public void ActivateChanges()
        {
            _changeEmitter.Activate();
        }

        public void DeactivateChanges()
        {
            _changeEmitter.Deactivate();
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
                    return true;

                if (checkedTile.X >= topRight.X)
                    break;
            }

            return false;
        }
    }
}
