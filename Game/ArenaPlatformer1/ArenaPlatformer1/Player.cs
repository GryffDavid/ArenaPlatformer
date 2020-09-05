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
    class Player
    {
        int ID;
        public Texture2D Texture;
        public Vector2 Position;
        public Vector2 Velocity;
        public GamePadState CurrentGamePadState, PreviousGamePadState;
        public KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        public MouseState CurrentMouseState, PreviousMouseState;
        public Rectangle DestinationRectangle, CollisionRectangle;

        public PlayerIndex PlayerIndex;

        GamePadThumbSticks Sticks;
        Vector2 MoveStick, AimStick, RumbleValues;
        Vector2 CurrentFriction = new Vector2(1f, 1f);

        public Player(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
            Position = new Vector2(500, 500);
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

            Sticks = CurrentGamePadState.ThumbSticks;
            MoveStick = Sticks.Left;
            AimStick = Sticks.Right;


            Velocity.X += MoveStick.X * 3f;

            Position.X += (Velocity.X * CurrentFriction.X) * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);

            if (MoveStick.X == 0)
            {
                Velocity.X = 0;
            }

            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, 32, 32);

            PreviousGamePadState = CurrentGamePadState;
            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, Color.White);
        }
    }
}
