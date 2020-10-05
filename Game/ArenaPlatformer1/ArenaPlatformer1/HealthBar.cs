using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    public class HealthBar
    {
        public static Texture2D Texture;
        public Vector2 HealthValue, Position, Size;
        public Player Player;
        public float HealthPerc;

        public HealthBar()
        {

        }

        public void LoadContent()
        {

        }

        public void Update(GameTime gameTime)
        {
            HealthValue = Player.Health;
            HealthPerc = ((100f / HealthValue.Y) * HealthValue.X)/100f;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y), Color.White);
            spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, (int)(Size.X * HealthPerc), (int)Size.Y), Color.Red);
        }
    }
}
