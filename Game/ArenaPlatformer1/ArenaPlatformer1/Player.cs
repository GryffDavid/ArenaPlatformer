using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace ArenaPlatformer1
{
    enum Facing { Left, Right };
    enum Pose { Standing, Crouching };

    public class Player
    {
        public event PlayerShootHappenedEventHandler PlayerShootHappened;
        public void CreatePlayerShoot(Vector2 velocity)
        {
            OnPlayerShootHappened(velocity);
        }
        protected virtual void OnPlayerShootHappened(Vector2 velocity)
        {
            PlayerShootHappened?.Invoke(this, new PlayerShootEventArgs() { Player = this, Velocity = velocity });
        }
        
        public Animation RunRightAnimation, RunRightUpAnimation, RunRightDownAnimation,
                         RunLeftAnimation, RunLeftUpAnimation, RunLeftDownAnimation,
                         StandRightAnimation, StandRightUpAnimation, StandRightDownAnimation,
                         StandLeftAnimation, StandLeftUpAnimation, StandLeftDownAnimation,
                         JumpRightAnimation, JumpRightUpAnimation, JumpRightDownAnimation,
                         JumpLeftAnimation, JumpLeftUpAnimation, JumpLeftDownAnimation,
                         CrouchRightAnimation, CrouchLeftAnimation;

        public Animation CurrentAnimation;

        public Texture2D RunRightTexture, RunRightUpTexture, RunRightDownTexture,
                         RunLeftTexture, RunLeftUpTexture, RunLeftDownTexture,
                         StandRightTexture, StandRightUpTexture, StandRightDownTexture,
                         StandLeftTexture, StandLeftUpTexture, StandLeftDownTexture,
                         JumpRightTexture, JumpRightUpTexture, JumpRightDownTexture,
                         JumpLeftTexture, JumpLeftUpTexture, JumpLeftDownTexture,
                         CrouchRightTexture, CrouchLeftTexture,
                         HeadTexture;

        bool Active = true;
        bool InAir;
        bool DoubleJumped = false;
        public Texture2D Texture;
        public Vector2 Position, Velocity, MoveStick, AimStick, RumbleValues, AimDirection;
        Vector2 MaxSpeed;
        Vector2 CurrentFriction = new Vector2(0.9999f, 1f);
        public GamePadState CurrentGamePadState, PreviousGamePadState;
        public KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        public MouseState CurrentMouseState, PreviousMouseState;
        public Rectangle DestinationRectangle, CollisionRectangle;
        public float Gravity;
        public PlayerIndex PlayerIndex;

        //Current health, Max health
        Vector2 Health = new Vector2(100, 100);

        GamePadThumbSticks Sticks;
        Buttons JumpButton, ShootButton, GrenadeButton, TrapButton;

        Facing CurrentFacing = Facing.Right;
        Facing PreviousFacing = Facing.Right;

        Pose CurrentPose = Pose.Standing;
        Pose PreviousPos = Pose.Standing;

        float RumbleTime, MaxRumbleTime;
        
        public static Map Map;

        public Player(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
            Position = new Vector2(500, 500);
            MaxSpeed = new Vector2(5f, 6);
            Gravity = 0.6f;
        }

        public void Initialize()
        {

        }

        public void LoadContent(ContentManager content)
        {
            Texture = content.Load<Texture2D>("Blank");

            JumpButton = Buttons.A;
            ShootButton = Buttons.X;
            GrenadeButton = Buttons.B;
            TrapButton = Buttons.Y;
            
            RunRightTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunRight");
            RunRightUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunRightUp");
            RunRightDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunRightDown");

            RunLeftTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunLeft");
            RunLeftUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunLeftUp");
            RunLeftDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunLeftDown");

            StandRightTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandRight");
            StandRightUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandRightUp");
            StandRightDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandRightDown");

            StandLeftTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandLeft");
            StandLeftUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandLeftUp");
            StandLeftDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandLeftDown");

            JumpRightTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpRight");
            JumpRightUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpRightUp");
            JumpRightDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpRightDown");

            JumpLeftTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpLeft");
            JumpLeftUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpLeftUp");
            JumpLeftDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpLeftDown");

            CrouchRightTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/CrouchRight");
            CrouchLeftTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/CrouchLeft");

            RunRightAnimation = new Animation(RunRightTexture, 8, 50);
            RunRightUpAnimation = new Animation(RunRightUpTexture, 8, 50);
            RunRightDownAnimation = new Animation(RunRightDownTexture, 8, 50);

            RunLeftAnimation = new Animation(RunLeftTexture, 8, 50);
            RunLeftUpAnimation = new Animation(RunLeftUpTexture, 8, 50);
            RunLeftDownAnimation = new Animation(RunLeftDownTexture, 8, 50);

            StandLeftAnimation = new Animation(StandLeftTexture, 1, 50);
            StandLeftUpAnimation = new Animation(StandLeftUpTexture, 1, 50);
            StandLeftDownAnimation = new Animation(StandLeftDownTexture, 1, 50);

            StandRightAnimation = new Animation(StandRightTexture, 1, 50);
            StandRightUpAnimation = new Animation(StandRightUpTexture, 1, 50);
            StandRightDownAnimation = new Animation(StandRightDownTexture, 1, 50);

            JumpLeftAnimation = new Animation(JumpLeftTexture, 1, 50);
            JumpLeftUpAnimation = new Animation(JumpLeftUpTexture, 1, 50);
            JumpLeftDownAnimation = new Animation(JumpLeftDownTexture, 1, 50);

            JumpRightAnimation = new Animation(JumpRightTexture, 1, 50);
            JumpRightUpAnimation = new Animation(JumpRightUpTexture, 1, 50);
            JumpRightDownAnimation = new Animation(JumpRightDownTexture, 1, 50);

            CrouchRightAnimation = new Animation(CrouchRightTexture, 1, 50);
            CrouchLeftAnimation = new Animation(CrouchLeftTexture, 1, 50);

            CurrentAnimation = RunRightAnimation;
        }

        public void Update(GameTime gameTime)
        {
            CurrentGamePadState = GamePad.GetState(PlayerIndex);
            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();

            if (Active == true)
            {
                Sticks = CurrentGamePadState.ThumbSticks;
                MoveStick = Sticks.Left;
                AimStick = Sticks.Right;



                #region Move stick left
                if (MoveStick.X < 0f)
                {
                    AimDirection.X = -1f;
                    CurrentFacing = Facing.Left;

                    Velocity.X += (MoveStick.X * 3f);
                }
                #endregion

                #region Move stick right
                if (MoveStick.X > 0f)
                {
                    AimDirection.X = 1f;
                    CurrentFacing = Facing.Right;

                    Velocity.X += (MoveStick.X * 3f);
                }
                #endregion

                if (Velocity.X < 0)
                    CheckLeftCollisions();

                if (Velocity.X > 0)
                    CheckRightCollisions();

                if (Velocity.X <= 0.5f &&
                    Velocity.X >= -0.5f)
                {
                    Velocity.X = 0f;
                }

                Position.X += Velocity.X * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);

                #region Stop Moving
                if (MoveStick.X == 0)
                {
                    if (InAir == false)
                        Velocity.X *= 0.85f;
                    else
                        Velocity.X *= 0.95f;
                }
                #endregion

                #region Jump
                if (CurrentGamePadState.IsButtonDown(JumpButton) &&
                    PreviousGamePadState.IsButtonUp(JumpButton) &&
                    CheckUpCollisions() == false &&
                    DoubleJumped == false &&
                    Velocity.Y >= 0)
                {
                    if (InAir == true)
                    {
                        Velocity.Y = -15f;
                        DoubleJumped = true;
                    }
                    else
                    {
                        Velocity.Y -= 15f;
                    }
                }
                #endregion

                #region Shoot
                if (CurrentGamePadState.IsButtonDown(ShootButton) &&
                           PreviousGamePadState.IsButtonUp(ShootButton))
                {
                    switch (CurrentFacing)
                    {
                        case Facing.Left:
                            CreatePlayerShoot(new Vector2(-30, 0));
                            break;

                        case Facing.Right:
                            CreatePlayerShoot(new Vector2(30, 0));
                            break;
                    }
                }
                #endregion

                #region Grenade
                if (CurrentGamePadState.IsButtonDown(GrenadeButton) &&
                            PreviousGamePadState.IsButtonUp(GrenadeButton))
                {
                    //Create grenades!
                }
                #endregion

                #region Trap
                if (CurrentGamePadState.IsButtonDown(TrapButton) &&
                    PreviousGamePadState.IsButtonUp(TrapButton))
                {
                    //Create traps!
                }
                #endregion

                #region Handle Rumble
                if (RumbleTime <= MaxRumbleTime)
                    RumbleTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (RumbleTime >= MaxRumbleTime)
                {
                    GamePad.SetVibration(PlayerIndex, 0, 0);
                }
                #endregion

                if (CheckUpCollisions() == true)
                {
                    Velocity.Y = 0;
                }

                if (CheckDownCollisions() == false)
                {
                    Velocity.Y += Gravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
                    Position.Y += Velocity.Y * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
                    InAir = true;
                }
                else
                {
                    Velocity.Y = 0f;
                    InAir = false;
                    DoubleJumped = false;
                }

                #region Limit Speed
                if (Velocity.X > MaxSpeed.X)
                {
                    Velocity.X = MaxSpeed.X;
                }

                if (Velocity.X < -MaxSpeed.X)
                {
                    Velocity.X = -MaxSpeed.X;
                }

                if (Velocity.Y < -25)
                {
                    Velocity.Y = -25;
                }

                if (Velocity.Y > 25)
                {
                    Velocity.Y = 25;
                }
                #endregion

                if (CurrentAnimation != null)
                {
                    CurrentAnimation.Position = Position;
                    CurrentAnimation.Update(gameTime);
                }
            }

            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);

            //Collision Rectangle same size as animation rectangle
            //CollisionRectangle = new Rectangle(
            //    (int)(CurrentAnimation.Position.X - CurrentAnimation.FrameSize.X/2), (int)(CurrentAnimation.Position.Y - CurrentAnimation.FrameSize.Y), 
            //    (int)CurrentAnimation.FrameSize.X, (int)CurrentAnimation.FrameSize.Y);

            //Collision rectangle standard size
            CollisionRectangle = new Rectangle(
                (int)(Position.X - 30), (int)(Position.Y - 80),
                (int)60, 80);

            PreviousFacing = CurrentFacing;
            PreviousGamePadState = CurrentGamePadState;
            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(Texture, DestinationRectangle, null, Color.White, 0, new Vector2(Texture.Width / 2, Texture.Height), SpriteEffects.None, 0);

            if (CurrentAnimation != null)
                CurrentAnimation.Draw(spriteBatch, Position);
        }

        /// <summary>
        /// Draw the collision box and other useful debug info
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="basicEffect"></param>
        public void DrawInfo(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect basicEffect)
        {
            //Mark the Position of the player
            spriteBatch.Draw(Texture, new Rectangle((int)Position.X - 1, (int)Position.Y - 4, 2, 8), Color.Red);

            #region Draw the collision box for debugging
            VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[4];
            int[] Indices = new int[8];

            Vertices[0] = new VertexPositionColorTexture()
            {
                Color = Color.Red,
                Position = new Vector3(CollisionRectangle.Left, CollisionRectangle.Top, 0),
                TextureCoordinate = new Vector2(0, 0)
            };

            Vertices[1] = new VertexPositionColorTexture()
            {
                Color = Color.Red,
                Position = new Vector3(CollisionRectangle.Right - 1, CollisionRectangle.Top, 0),
                TextureCoordinate = new Vector2(1, 0)
            };

            Vertices[2] = new VertexPositionColorTexture()
            {
                Color = Color.Red,
                Position = new Vector3(CollisionRectangle.Right - 1, CollisionRectangle.Bottom - 1, 0),
                TextureCoordinate = new Vector2(1, 1)
            };

            Vertices[3] = new VertexPositionColorTexture()
            {
                Color = Color.Red,
                Position = new Vector3(CollisionRectangle.Left, CollisionRectangle.Bottom - 1, 0),
                TextureCoordinate = new Vector2(0, 1)
            };

            Indices[0] = 0;
            Indices[1] = 1;

            Indices[2] = 2;
            Indices[3] = 3;

            Indices[4] = 0;

            Indices[5] = 2;
            Indices[6] = 0;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserIndexedPrimitives(PrimitiveType.LineStrip, Vertices, 0, 4, Indices, 0, 6, VertexPositionColorTexture.VertexDeclaration);
            } 
            #endregion
        }

        public void MakeRumble(float time, Vector2 value)
        {
            GamePad.SetVibration(PlayerIndex, value.X, value.Y);            
            RumbleTime = 0;
            MaxRumbleTime = time;
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
                        (int)(CollisionRectangle.Bottom + Velocity.Y + 1))))
                    {
                        if (tile.TileType == TileType.BouncePad)
                        {
                            Position.Y += (tile.CollisionRectangle.Top - CollisionRectangle.Bottom);
                            Velocity.Y = -25f;
                            return false;
                        }
                        else
                        {
                            Position.Y += (tile.CollisionRectangle.Top - CollisionRectangle.Bottom);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool CheckRightCollisions()
        {
            foreach(Tile tile in Map.TileList)
            {
                for (int i = 0; i < CollisionRectangle.Height; i++)
                {
                    if (tile.CollisionRectangle.Contains(
                        new Point(
                            (int)(CollisionRectangle.Right + Velocity.X),
                            (int)(CollisionRectangle.Top + i))))
                    {
                        Position.X -= (CollisionRectangle.Right - tile.CollisionRectangle.Left);
                        Velocity.X = 0;
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
                            (int)(CollisionRectangle.Left + Velocity.X - 1),
                            (int)(CollisionRectangle.Top + i))))
                    {
                        Position.X += (tile.CollisionRectangle.Right - CollisionRectangle.Left);
                        Velocity.X = 0;
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
                    if (Velocity.Y < 0)
                    if (tile.CollisionRectangle.Contains(
                        new Point(
                        (int)(CollisionRectangle.Left + i),
                        (int)(CollisionRectangle.Top + Velocity.Y - 1))))
                    {
                            Position.Y += (tile.CollisionRectangle.Bottom - CollisionRectangle.Top);
                            Velocity.Y = 0;
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
