﻿using System;
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
            IsKinematic = true;
            Size = new Vector2(80, 32);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (PushesRightTile == true)
            {
                Velocity.X = -Velocity.X;
            }

            if (PushesLeftTile == true)
            {
                Velocity.X = -Velocity.X;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, new Rectangle((int)(Position.X - 40), (int)(Position.Y - 16), (int)Size.X, (int)Size.Y), Color.White);
        }
    }
}
