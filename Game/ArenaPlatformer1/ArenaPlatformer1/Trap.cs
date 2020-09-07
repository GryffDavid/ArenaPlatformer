using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    abstract class Trap
    {
        public Texture2D Texture;
        public Vector2 Position;
        public bool Active;
        public static TrapType TrapType;
        public Rectangle CollisionRectangle, DestinationRectangle;
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);
    }
}
