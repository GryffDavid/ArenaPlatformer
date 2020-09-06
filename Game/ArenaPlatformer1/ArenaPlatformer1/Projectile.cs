using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArenaPlatformer1
{
    enum ProjectileType { Rocket, Bullet };

    abstract class Projectile
    {
        public static Texture2D Texture;
        public PlayerIndex PlayerIndex;
        public Vector2 Velocity, Position, Direction;
        public float Rotation, Angle, Gravity;
        public static Map Map;

        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void Update(GameTime gameTime);
    }
}
