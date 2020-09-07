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
    /// The class representing the physical object in the game which the player can pick up to get access to the weapon
    /// </summary>
    abstract class Gun
    {
        public Vector2 Position;
        public static GunType GunType;
        public Texture2D Texture;
        public Rectangle DestinationRectangle, CollisionRectangle;
        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void Update(GameTime gameTime);
    }
}
