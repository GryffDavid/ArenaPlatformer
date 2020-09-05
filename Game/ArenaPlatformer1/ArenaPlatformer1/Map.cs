using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    class Map
    {
        public List<Tile> TileList = new List<Tile>();

        public Map()
        {
            //Bottom
            for (int i = 2; i < 60; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(i * 32, 1048)
                };

                TileList.Add(tile);
            }

            //Top
            for (int i = 0; i < 60; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(i * 32, 0)
                };

                TileList.Add(tile);
            }

            //Left
            for (int i = 0; i < 34; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(0, 32 * i)
                };

                TileList.Add(tile);
            }

            //Right
            for (int i = 0; i < 34; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(1888, 32 * i)
                };

                TileList.Add(tile);
            }


            //Platform
            for (int i = 16; i < 28; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(32 * i, 800)
                };

                TileList.Add(tile);
            }

            //Platform
            for (int i = 6; i < 21; i++)
            {
                Tile tile = new Tile()
                {
                    Position = new Vector2(32 * i, 900)
                };

                TileList.Add(tile);
            }

            TileList.Add(new Tile()
            {
                Position = new Vector2(32, 1048),
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
