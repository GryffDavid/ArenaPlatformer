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
        Vector2 myVel;


        public Rocket()
        {
            Gravity = 0.05f;
            Size = new Vector2(8, 8);
            Active = true;
        }

        public void LoadContent(ContentManager content)
        {

        }

        public override void Update(GameTime gameTime)
        {
            myVel = Vector2.Lerp(myVel, Velocity, 0.1f);

            Velocity.Y += Gravity;

            base.Update(gameTime);

            if (PushesRightTile == true || PushesLeftTile == true || 
                PushesBottomTile == true || PushesTopTile == true)
            {
                Active = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Color color = new Color(Color.Lime.R, Color.Lime.G, Color.Lime.B, 120);

            spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, Texture.Width / 2, Texture.Height / 2),
                color);
        }
    }
}
