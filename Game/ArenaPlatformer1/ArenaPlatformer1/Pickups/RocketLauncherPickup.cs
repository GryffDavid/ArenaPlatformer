using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArenaPlatformer1
{
    public class RocketLauncherPickup : Item
    {
        public static new Texture2D Texture;

        public RocketLauncherPickup(Vector2 position)
        {
            //string myType = this.GetType().Name.ToString();
            //myType = myType.Remove(myType.Length - "Pickup".Length, "Pickup".Length);
            //ItemType thingy = (ItemType)ItemType.Parse(typeof(ItemType), myType);
            ItemType = ItemType.RocketLauncher;
            Position = position;
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            CollisionRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, Color.Red);
        }
    }
}
