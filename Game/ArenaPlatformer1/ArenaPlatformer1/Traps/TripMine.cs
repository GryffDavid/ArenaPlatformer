using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    class TripMine : Trap
    {
        //public BulletTrail Laser;
        public LaserBeam Laser;

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                spriteBatch.Draw(Texture, DestinationRectangle, null, Color.White, Rotation,
                    new Vector2(DestinationRectangle.Width / 2, DestinationRectangle.Height), SpriteEffects.None, 0);
            }
        }
    }
}
