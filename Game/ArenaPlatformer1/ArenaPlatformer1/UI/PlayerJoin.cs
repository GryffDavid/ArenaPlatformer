using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    class PlayerJoin
    {
        Texture2D Texture;
        Vector2 Position, Size;
        Color CurrentColor;
        public bool Occupied = false;

        public PlayerJoin(Texture2D texture, Vector2 position, Vector2 size)
        {
            Texture = texture;
            Position = position;
            Size = size;
            CurrentColor = Color.Gray;
        }

        public void LoadContent(ContentManager content)
        {

        }

        public void Update(GameTime gameTime)
        {
            if (Occupied == true)
            {
                CurrentColor = Color.Red;
            }
            else
            {
                CurrentColor = Color.Gray;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y), CurrentColor);
        }
    }
}
