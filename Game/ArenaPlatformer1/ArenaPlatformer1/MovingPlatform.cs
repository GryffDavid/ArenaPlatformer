using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    public class MovingPlatform
    {
        public Texture2D Texture;
        public Vector2 Position, Size, Speed;
        public Rectangle DestinationRectangle, CollisionRectangle;

        public static Map Map;

        public MovingPlatform()
        {

        }

        public void LoadContent(ContentManager content)
        {

        }
        
        public void Update(GameTime gameTime)
        {
            Position += Speed;

            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            CollisionRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

            //foreach (Tile tile in Map.TileList)
            //{
            //    if (tile.CollisionRectangle.Intersects(CollisionRectangle))
            //    {
            //        Speed = -Speed;
            //    }
            //}
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, Color.White);
        }
    }
}
