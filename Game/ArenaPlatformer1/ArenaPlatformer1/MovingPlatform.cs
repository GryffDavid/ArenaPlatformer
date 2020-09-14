using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    class MovingPlatform : MovingObject
    {
        public MovingPlatform()
        {
            Size = new Vector2(64, 16);
            IsKinematic = true;
            Velocity = new Vector2(2, 0);
        }

        public new void Update(GameTime gameTime)
        {
            

            base.Update(gameTime);
        }
    }
}
