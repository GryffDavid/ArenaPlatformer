using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    public class Map
    {
        public List<Tile> TileList = new List<Tile>();

        public Map()
        {
            //Bottom
            for (int i = 2; i < 30; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(i * 64, 1016)
                };

                TileList.Add(tile);
            }

            //Top
            for (int i = 0; i < 30; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(i * 64, 0)
                };

                TileList.Add(tile);
            }

            //Left
            for (int i = 0; i < 17; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(0, 64 * i)
                };

                TileList.Add(tile);
            }

            //Right
            for (int i = 0; i < 17; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(1856, 64 * i)
                };

                TileList.Add(tile);
            }


            //Platform
            for (int i = 8; i < 14; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(64 * i, 800)
                };

                TileList.Add(tile);
            }

            Tile tile3 = new Tile()
            {
                Position = new Vector2(7 * 64, 800),
                TileType = TileType.BouncePad,
                Color = Color.Red
            };

            TileList.Add(tile3);

            Tile tile2 = new Tile()
            {
                Position = new Vector2(64 * 14, 800),
                TileType = TileType.BouncePad,
                Color = Color.Red
            };

            TileList.Add(tile2);

            //Platform
            for (int i = 3; i < 10; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(64 * i, 900)
                };

                TileList.Add(tile);
            }

            TileList.Add(new Tile()
            {
                Position = new Vector2(64, 1016),
                TileType = TileType.BouncePad,
                Color = Color.Red
            });
        }

        public void Initialize()
        {

        }

        public void LoadContent(ContentManager content)
        {
            foreach (Tile tile in TileList)
            {
                tile.LoadContent(content);
            }
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Tile tile in TileList)
            {
                tile.Draw(spriteBatch);
            }
        }
    }
}
