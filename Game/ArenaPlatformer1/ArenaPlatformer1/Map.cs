using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    //https://gamedevelopment.tutsplus.com/tutorials/basic-2d-platformer-physics-part-2--cms-25922?_ga=2.50805920.845617979.1510510859-1473812659.1510510859


    public class Map
    {
        //Collision tiles
        private TileType[,] Tiles;

        //Draw data
        public Tile[,] DrawTiles;

        //The size of each tile
        public Vector2 TileSize = new Vector2(64, 64);

        //The size of the map in tiles
        public Vector2 MapSize = new Vector2(30, 17);

        public Map()
        {
            Tiles = new TileType[(int)MapSize.X, (int)MapSize.Y];
            DrawTiles = new Tile[(int)MapSize.X, (int)MapSize.Y];

            for (int x = 0; x < (int)MapSize.X; x++)
            {
                for (int y = 0; y < (int)MapSize.Y; y++)
                {
                    Tiles[x, y] = TileType.Empty;
                }
            }

            #region Add tiles
            #region Border
            for (int x = 0; x < MapSize.X; x++)
            {
                Tiles[x, 0] = TileType.Solid;
            }

            for (int x = 0; x < MapSize.X; x++)
            {
                Tiles[x, (int)MapSize.Y - 1] = TileType.Solid;
            }

            for (int y = 0; y < MapSize.Y; y++)
            {
                Tiles[0, y] = TileType.Solid;
            }

            for (int y = 0; y < MapSize.Y; y++)
            {
                Tiles[(int)MapSize.X - 1, y] = TileType.Solid;
            }
            #endregion

            for (int x = 10; x < 20; x++)
            {
                Tiles[x, 13] = TileType.Solid;
            }

            for (int x = 5; x < 10; x++)
            {
                Tiles[x, 14] = TileType.Solid;
            }

            for (int x = 5; x < 10; x++)
            {
                Tiles[x, 11] = TileType.Solid;
            }

            for (int x = 20; x < 25; x++)
            {
                Tiles[x, 12] = TileType.Solid;
            }
            #endregion
        }

        public void Initialize()
        {

        }

        public void LoadContent(ContentManager content)
        {
            for (int x = 0; x < (int)MapSize.X; x++)
            {
                for (int y = 0; y < (int)MapSize.Y; y++)
                {
                    if (Tiles[x, y] == TileType.Solid)
                    {
                        Tile drawTile = new Tile()
                        {
                            Size = TileSize,
                            Position = new Vector2(x * TileSize.X, y * TileSize.Y)
                        };
                        drawTile.LoadContent(content);

                        DrawTiles[x, y] = drawTile;
                    }
                }
            }
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Tile drawTile in DrawTiles)
            {
                if (drawTile != null)
                    drawTile.Draw(spriteBatch);
            }
        }

        public TileType GetTile(int x, int y)
        {
            if (x < 0 || x >= MapSize.X || y < 0 || y >= MapSize.Y)
                return TileType.Solid;

            return Tiles[x, y];
        }

        public bool IsEmpty(int x, int y)
        {
            if (x < 0 || x >= MapSize.X || y < 0 || y >= MapSize.Y)
                return false;

            return (Tiles[x, y] == TileType.Empty);
        }

        public int GetMapTileXAtPoint(float x)
        {
            return (int)(x / (float)(TileSize.X));
        }

        public int GetMapTileYAtPoint(float y)
        {
            return (int)(y / (float)(TileSize.Y));
        }

        public Vector2 GetMapTilePosition(int tileIndexX, int tileIndexY)
        {
            return new Vector2(
                    (float)(tileIndexX * TileSize.X),
                    (float)(tileIndexY * TileSize.Y)
                );
        }

    }
}
