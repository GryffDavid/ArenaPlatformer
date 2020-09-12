using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    class Mine : Trap
    {
        public Mine()
        {
            TrapType = TrapType.Mine;
            ResetTime = new Vector2(0, 1500);
            DetonationLimit = 5;            
        }

        public void LoadContent(ContentManager content)
        {

        }

        public override void Update(GameTime gameTime)
        {
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, 32, 16);
            CollisionRectangle = new Rectangle((int)Position.X, (int)Position.Y, 32, 16);

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Position = Position;
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, Color.Red);
        }
    }
}
