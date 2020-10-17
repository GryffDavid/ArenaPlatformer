using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    public abstract class Trap
    {
        public Player SourcePlayer;
        public static Map Map;
        public Texture2D Texture;
        public Vector2 Position;

        public int DetonationLimit;
        public List<Emitter> EmitterList = new List<Emitter>();

        public Vector2 Time, ResetTime;

        public bool Exists = true;
        public bool Active = true;

        public static TrapType TrapType;

        public Rectangle CollisionRectangle, DestinationRectangle;
        public float Rotation = 0; //The orientation of this trap oncec placed. Is it on a wall, the floor or the ceiling?


        public virtual void Update(GameTime gameTime)
        {
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, 32, 16);
            CollisionRectangle = DestinationRectangle;
        }

        public abstract void Draw(SpriteBatch spriteBatch);

        public virtual void Reset()
        {

        }
    }
}
