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
    /// An object that can spawn an Item for the player to pick up
    /// </summary>
    public class ItemSpawn
    {
        static Random Random = new Random();
        public static List<Item> ItemList;

        /// <summary>
        /// The type of object that this spawn will create
        /// </summary>
        public ItemType ItemSpawnType;

        /// <summary>
        /// X = CurrentSpawnTime, Y = MaxSpawnTime
        /// </summary>
        public Vector2 SpawnTime = new Vector2(0, 6000);

        public Vector2 Position;        

        public ItemSpawn(ItemType itemType, Vector2 position)
        {
            ItemSpawnType = itemType;
            Position = position;
        }

        public void Update(GameTime gameTime)
        {
            SpawnTime.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (SpawnTime.X > SpawnTime.Y)
            {
                if (ItemList.Count(Item => Item.Position == Position) == 0)
                {
                    SpawnItem();
                }

                SpawnTime.X = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            
        }

        public void SpawnItem()
        {
            switch (ItemSpawnType)
            {
                case ItemType.Shield:
                    {
                        ItemList.Add(new ShieldPickup(Position));
                    }
                    break;
            }            
        }
    }
}
