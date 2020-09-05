using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    class Tile
    {
        Texture2D Texture;
        public Vector2 Position, Size;
        Rectangle DestinationRectangle, CollisionRectangle;

        public Tile()
        {
            
        }

        public void LoadContent(ContentManager content)
        {
            Texture = content.Load<Texture2D>("Blank");
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, 32, 32);
        }

        public void Update(GameTime gameTime)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, Color.Black);
        }
    }
}
