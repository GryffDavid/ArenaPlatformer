﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    [Serializable]
    public enum ItemType { Shield };

    [Serializable]
    public class ItemSpawn
    {
        static Random Random = new Random();

        [NonSerialized]
        public static List<Item> ItemList;

        public ItemType[] ItemSpawnType;
        public Vector2 SpawnTime = new Vector2(0, 6000);
        public Vector2 Position;

        public ItemSpawn(ItemType[] itemType, Vector2 position)
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
            ItemType thing = ItemSpawnType[Random.Next(0, ItemSpawnType.Length)];

            switch (thing)
            {
                case ItemType.Shield:
                    {
                        Item spawn = new ShieldPickup(Position);
                        spawn.SpawnSource = this;
                        ItemList.Add(spawn);
                    }
                    break;
            }
        }
    }
}
