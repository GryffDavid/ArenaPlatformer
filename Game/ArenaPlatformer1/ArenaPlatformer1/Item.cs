using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    /// <summary>
    /// The base object for all objects that the player can pick up
    /// </summary>
    public abstract class Item
    {
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);

        public bool Exists = true;
        public Vector2 Position;
        public Rectangle DestinationRectangle, CollisionRectangle;
        public Texture2D Texture;
        public ItemType ItemType;
    }
}
