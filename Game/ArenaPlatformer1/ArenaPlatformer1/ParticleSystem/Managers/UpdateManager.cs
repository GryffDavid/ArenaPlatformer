using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Threading;
using System.Diagnostics;

namespace ArenaPlatformer1
{
    public class UpdateManager
    {
        protected ChangeBuffer MessageBuffer;
        private GameTime GameTime;
        public List<ParticleData> ParticleDataObjects { get; set; }
        public DoubleBuffer DoubleBuffer;
        public Thread RunningThread;

        static Random Random = new Random();

        public UpdateManager(DoubleBuffer doubleBuffer)
        {
            DoubleBuffer = doubleBuffer;
            ParticleDataObjects = new List<ParticleData>();
        }

        public void Update(GameTime gameTime)
        {
            MessageBuffer.Clear();

            for (int i = 0; i < ParticleDataObjects.Count; i++)
            {
                ParticleData gameData = ParticleDataObjects[i];
                float Rot;



                #region Handle particle rotation
                if (gameData.RotateVelocity == true)
                {
                    Rot = (float)Math.Atan2(gameData.Velocity.Y, gameData.Velocity.X);
                }
                else
                {
                    Rot = gameData.Rotation + MathHelper.ToRadians(gameData.RotationIncrement) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                }
                #endregion


                gameData.Update(gameTime);


                #region Update position

                Vector2 newPos = gameData.Position + gameData.Velocity;
                #endregion




                float percTime = gameData.CurrentTime / gameData.MaxTime;

                #region Fade transparency
                float transparency = gameData.StartingTransparency;
                if (gameData.Fade == true)
                {
                    transparency = MathHelper.Lerp(gameData.StartingTransparency, 0, percTime);
                    //transparency = gameData.StartingTransparency * (1.0f - ((1 / (gameData.MaxTime + gameData.FadeDelay)) * (gameData.CurrentTime + gameData.CurrentFadeDelay)));
                }
                #endregion

                #region Scale particles
                float newScale = gameData.CurrentScale;

                if (gameData.Shrink == true && gameData.Grow == false)
                    newScale = gameData.MaxScale * (1.0f - ((1 / gameData.MaxTime) * (gameData.CurrentTime)));

                if (gameData.Grow == true && gameData.Shrink == false)
                    newScale = gameData.MaxScale * ((1 / gameData.MaxTime) * (gameData.CurrentTime));

                if (gameData.Shrink == true && gameData.Grow == true)
                    newScale = gameData.MaxScale * (float)Math.Sin(Math.PI * percTime);
                #endregion

                #region Fade Colours
                Color newCol = Color.Lerp(gameData.StartColor, gameData.EndColor, percTime);
                #endregion








                //transparency = MathHelper.Lerp(gameData.StartingTransparency, 0, percTime);

                ChangeMessage msg = new ChangeMessage()
                {
                    ID = i
                };

                if (gameData.Active == false)
                {
                    msg.MessageType = ChangeMessageType.DeleteRenderData;
                    ParticleDataObjects.Remove(gameData);
                    MessageBuffer.Add(msg);
                    i--;
                }

                if (gameData.CurrentTime <= gameData.MaxTime)
                {
                    msg.MessageType = ChangeMessageType.UpdateParticle;
                    msg.Position = newPos;
                    msg.Rotation = Rot;
                    msg.Color = newCol;
                    msg.Scale = newScale;
                    msg.Transparency = transparency;
                    MessageBuffer.Add(msg);

                    gameData.Position = newPos;
                    gameData.Rotation = Rot;
                }
            }
        }

        public void AddParticle(Texture2D texture, Vector2 pos, Vector2 angleRange, Vector2 speedRange, Vector2 scaleRange,
            Color startColor, Color endColor, float gravity, bool shrink, bool fade, Vector2 startingRotation,
            Vector2 rotationIncrement, float startingTransparency, Vector2 timeRange, bool grow, bool rotateVelocity,
            Vector2 friction, int orientation, float fadeDelay, Vector2 yRange, bool canBounce, bool stopBounce, bool hardBounce,
            float drawDepth, bool emissive, bool lit,
            out ParticleData gameData, out RenderData renderData)
        {
            float myAngle, mySpeed, myScale, myRotation, myIncrement, myTime, myBounceY;
            Vector2 Direction, myVelocity;

            mySpeed = (float)DoubleRange(speedRange.X, speedRange.Y);
            myAngle = -MathHelper.ToRadians((float)DoubleRange(angleRange.X, angleRange.Y));
            myScale = (float)DoubleRange(scaleRange.X, scaleRange.Y);
            myRotation = MathHelper.ToRadians((float)DoubleRange(startingRotation.X, startingRotation.Y));
            myIncrement = MathHelper.ToRadians((float)DoubleRange(rotationIncrement.X, rotationIncrement.Y));
            myTime = (float)DoubleRange(timeRange.X, timeRange.Y);
            myBounceY = (float)DoubleRange(yRange.X, yRange.Y);

            Direction.X = (float)Math.Cos(myAngle);
            Direction.Y = (float)Math.Sin(myAngle);

            myVelocity = Direction * mySpeed;

            gameData = new ParticleData()
            {
                Active = true,
                Position = pos,
                Angle = angleRange,
                CurrentTime = 0,
                MaxTime = myTime,
                Velocity = myVelocity,
                StartColor = startColor,
                EndColor = endColor,
                Gravity = gravity,
                CurrentScale = myScale,
                MaxScale = myScale,
                Shrink = shrink,
                Grow = grow,
                Fade = fade,
                Friction = friction,
                StartingTransparency = startingTransparency,
                Rotation = myRotation,
                RotationIncrement = myIncrement,
                RotateVelocity = rotateVelocity,
                FadeDelay = fadeDelay,
                CurrentFadeDelay = 0,

                BounceY = myBounceY,
                CanBounce = canBounce,
                HardBounce = hardBounce,
                StopBounce = stopBounce,
                HasBounced = false,
            };

            if (grow == true)
                myScale = 0;

            renderData = new RenderData()
            {
                Texture = texture,
                Position = gameData.Position,
                Color = startColor,
                Rotation = myRotation,
                Scale = myScale,
                Transparency = startingTransparency,
                Orientation = orientation,
                DrawDepth = drawDepth,

                Emissive = emissive,
                Lit = lit
            };
        }


        public void StartOnNewThread()
        {
            ThreadStart threadStart = new ThreadStart(Run);
            RunningThread = new Thread(threadStart);
            RunningThread.Start();
        }

        private void Run()
        {
            while (true)
            {
                DoFrame();
            }
        }

        public void DoFrame()
        {
            DoubleBuffer.StartUpdateProcessing(out MessageBuffer, out GameTime);
            Update(GameTime);
            DoubleBuffer.SubmitUpdate();
        }

        public double DoubleRange(double one, double two)
        {
            return one + Random.NextDouble() * (two - one);
        }
    }
}
