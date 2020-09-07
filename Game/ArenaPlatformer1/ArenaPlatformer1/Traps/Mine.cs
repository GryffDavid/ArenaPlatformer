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
        }

        public void LoadContent(ContentManager content)
        {

        }

        public override void Update(GameTime gameTime)
        {
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, 32, 16);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, Color.Red);
        }
    }
}
