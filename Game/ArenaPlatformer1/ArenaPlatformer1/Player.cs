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

    class Player
    {        
        bool Active = true;
        public Texture2D Texture;
        public Vector2 Position, Velocity, MoveStick, AimStick, RumbleValues, AimDirection;
        Vector2 MaxSpeed;
        Vector2 CurrentFriction = new Vector2(1f, 1f);
        public GamePadState CurrentGamePadState, PreviousGamePadState;
        public KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        public MouseState CurrentMouseState, PreviousMouseState;
        public Rectangle DestinationRectangle, CollisionRectangle;
        float Gravity;
        public PlayerIndex PlayerIndex;

        GamePadThumbSticks Sticks;

        Facing CurrentFacing = Facing.Right;
        Facing PreviousFacing = Facing.Right;

        Pose CurrentPose = Pose.Standing;
        Pose PreviousPos = Pose.Standing;

        List<Tile> TileList = new List<Tile>();

        public Player(PlayerIndex playerIndex, List<Tile> tileList)
        {
            PlayerIndex = playerIndex;
            Position = new Vector2(500, 500);
            MaxSpeed = new Vector2(2.5f, 6);
            Gravity = 0.6f;

            TileList = tileList;
        }

        public void Initialize()
        {

        }

        public void LoadContent(ContentManager content)
        {
            Texture = content.Load<Texture2D>("Blank");
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
                }
                #endregion

                #region Move stick right
                if (MoveStick.X > 0f)
                {
                    AimDirection.X = 1f;
                    CurrentFacing = Facing.Right;
                }
                #endregion

                Velocity.X += MoveStick.X * 3f;

                #region Stop Moving
                if (MoveStick.X == 0)
                {
                    Velocity.X = 0;
                }
                #endregion

                #region Move right
                if (CurrentGamePadState.ThumbSticks.Left.X > 0 && 
                    CheckRightCollisions() == false)
                {
                    Position.X += (Velocity.X * CurrentFriction.X) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
                }
                #endregion

                #region Moving left
                if (CurrentGamePadState.ThumbSticks.Left.X < 0 && 
                    CheckLeftCollisions() == false)
                {
                    Position.X += (Velocity.X * CurrentFriction.X) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
                }
                #endregion

                #region Press A - Jump
                if (CurrentGamePadState.Buttons.A == ButtonState.Pressed &&
                    PreviousGamePadState.Buttons.A == ButtonState.Released)
                {
                    Velocity.Y -= 12;
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
            spriteBatch.Draw(Texture, new Rectangle((int)Position.X-2, (int)Position.Y-2, 4, 4), Color.Red);
        }

        /// <summary>
        /// Draw the collision box and other useful debug info
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="basicEffect"></param>
        public void DrawInfo(GraphicsDevice graphics, BasicEffect basicEffect)
        {

        }

        //public void MakeRumble(float time, Vector2 value)
        //{
        //    GamePad.SetVibration(PlayerIndex, value.X, value.Y);
        //    RumbleTime = 0;
        //    MaxRumbleTime = time;
        //}

        public bool CheckDownCollisions()
        {
            foreach (Tile tile in TileList)
            {
                for (int i = 0; i < CollisionRectangle.Width; i++)
                {
                    if (tile.CollisionRectangle.Contains(
                        new Point(
                        (int)(CollisionRectangle.Left + i), 
                        (int)(CollisionRectangle.Bottom + Velocity.Y + 1))))
                    {
                        Position.Y += (tile.CollisionRectangle.Top - CollisionRectangle.Bottom);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool CheckRightCollisions()
        {
            foreach (Tile tile in TileList)
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
            foreach (Tile tile in TileList)
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
            foreach (Tile tile in TileList)
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
