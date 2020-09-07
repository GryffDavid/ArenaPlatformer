using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    class Rocket : Projectile
    {
        public static Texture2D Texture;

        public Rocket()
        {
            Gravity = 0.05f;
            //Velocity = new Vector2(17, 0);
            Active = true;
        }

        public void LoadContent(ContentManager content)
        {

        }

        public override void Update(GameTime gameTime)
        {
            Velocity.Y += Gravity;
            Position += Velocity;

            CollisionRectangle = new Rectangle((int)(Position.X - Texture.Width / 2), (int)(Position.Y - Texture.Height / 2), Texture.Width, Texture.Height);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Color color;

            //color = new Color(Color.Violet.R, Color.Violet.G, Color.Violet.B, 120);
            color = new Color(Color.Lime.R, Color.Lime.G, Color.Lime.B, 120);


            spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, Texture.Width / 2, Texture.Height / 2),
                color);
        }
    }
}
