﻿using System;
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
        public void CreatePlayerShoot()
        {
            OnPlayerShootHappened();
        }
        protected virtual void OnPlayerShootHappened()
        {
            if (PlayerShootHappened != null)
                PlayerShootHappened(this, new PlayerShootEventArgs() { Player = this });
        }

        bool Active = true;
        public Texture2D Texture;
        public Vector2 Position, Velocity, MoveStick, AimStick, RumbleValues, AimDirection;
        Vector2 MaxSpeed;
        Vector2 CurrentFriction = new Vector2(1f, 1f);
        public GamePadState CurrentGamePadState, PreviousGamePadState;
        public KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        public MouseState CurrentMouseState, PreviousMouseState;
        public Rectangle DestinationRectangle, CollisionRectangle;
        public float Gravity;
        public PlayerIndex PlayerIndex;

        GamePadThumbSticks Sticks;
        Buttons JumpButton, ShootButton, GrenadeButton;

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
            MaxSpeed = new Vector2(2.5f, 6);
            Gravity = 0.6f;
        }

        public void Initialize()
        {

        }

        public void LoadContent(ContentManager content)
        {
            Texture = content.Load<Texture2D>("Blank");

            JumpButton = Buttons.A;
            ShootButton = Buttons.B;
            GrenadeButton = Buttons.X;
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

                Velocity.X += MoveStick.X * 3f;

                #region Move stick left
                if (MoveStick.X < 0f)
                {
                    AimDirection.X = -1f;
                    CurrentFacing = Facing.Left;

                    if (CheckLeftCollisions() == false)
                        Position.X += (Velocity.X * CurrentFriction.X) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
                }
                #endregion

                #region Move stick right
                if (MoveStick.X > 0f)
                {
                    AimDirection.X = 1f;
                    CurrentFacing = Facing.Right;

                    if (CheckRightCollisions() == false)
                        Position.X += (Velocity.X * CurrentFriction.X) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
                }
                #endregion

                #region Stop Moving
                if (MoveStick.X == 0)
                {
                    Velocity.X = 0;
                }
                #endregion
                
                #region Jump
                if (CurrentGamePadState.IsButtonDown(JumpButton) &&
                    PreviousGamePadState.IsButtonUp(JumpButton) &&
                    Velocity.Y > -20 &&
                    CheckUpCollisions() == false)
                {
                    Velocity.Y -= 12;
                }
                #endregion

                if (CurrentGamePadState.IsButtonDown(ShootButton) &&
                    PreviousGamePadState.IsButtonUp(ShootButton))
                {
                    CreatePlayerShoot();
                }

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
                    Velocity.Y += Gravity;
                    Position.Y += (Velocity.Y * CurrentFriction.Y) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
                    //InAir = true;
                }
                else
                {
                    Velocity.Y = 0f;
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
            }

            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            CollisionRectangle = new Rectangle((int)(Position.X - Texture.Width/2), (int)(Position.Y - Texture.Height), Texture.Width, Texture.Height);

            PreviousGamePadState = CurrentGamePadState;
            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, null, Color.White, 0, new Vector2(Texture.Width / 2, Texture.Height), SpriteEffects.None, 0);
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
            foreach (Tile tile in Map.TileList.Where(Tile => Vector2.Distance(Tile.Position, Position) < 80))
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
            foreach(Tile tile in Map.TileList.Where(Tile => Vector2.Distance(Tile.Position, Position) < 80))
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
            foreach (Tile tile in Map.TileList.Where(Tile => Vector2.Distance(Tile.Position, Position) < 80))
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
            foreach (Tile tile in Map.TileList.Where(Tile => Vector2.Distance(Tile.Position, Position) < 80))
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
