using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    class Bullet : Projectile
    {
        public new static Texture2D Texture;

        public Bullet()
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
            //Position += Velocity;
            base.Update(gameTime);

            if (PushesRightTile == true)
            {
                Active = false;
            }

            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            //CollisionRectangle = new Rectangle((int)(Position.X - Texture.Width / 2), (int)(Position.Y - Texture.Height / 2), Texture.Width, Texture.Height);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Color color;

            //color = new Color(Color.Violet.R, Color.Violet.G, Color.Violet.B, 120);
            color = new Color(Color.HotPink.R, Color.HotPink.G, Color.HotPink.B, 120);

            spriteBatch.Draw(Texture, DestinationRectangle, null, color, 0, 
                             new Vector2(Texture.Width / 2, Texture.Height / 2), SpriteEffects.None, 0);          
        }
    }
}
