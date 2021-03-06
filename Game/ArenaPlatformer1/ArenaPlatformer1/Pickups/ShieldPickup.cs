﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ArenaPlatformer1
{
    public class ShieldPickup : Item
    {
        public static new Texture2D Texture;

        public ShieldPickup(Vector2 position)
        {
            ItemType = ItemType.Shield;
            Position = position;
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            CollisionRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
        }

        public override void Update(GameTime gameTime)
        {
            float yOffset = (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 2.6f) * -8;
            DestinationRectangle = new Rectangle((int)Position.X, (int)(Position.Y + yOffset), Texture.Width, Texture.Height);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, Color.White);
        }
    }
}
