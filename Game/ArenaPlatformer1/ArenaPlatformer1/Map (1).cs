﻿using System;
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

        public int TreeGridWidth = 64;
        public int TreeGridHeight = 64;

        public List<MovingObject>[,] ObjectsInArea;

        int HorizontalAreasCount;
        int VerticalAreasCount;

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

            Tiles[3, 11] = TileType.Solid;
            Tiles[4, 11] = TileType.BouncePad;

            for (int x = 10; x < 21; x++)
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

            for (int y = 5; y < 10; y++)
            {
                Tiles[8, y] = TileType.Solid;
            }

            Tiles[6, (int)MapSize.Y - 1] = TileType.BouncePad;
            Tiles[5, (int)MapSize.Y - 1] = TileType.BouncePad;


            #endregion
        }

        public void Initialize()
        {
            HorizontalAreasCount = (int)Math.Ceiling((float)1920 / (float)TreeGridWidth);
            VerticalAreasCount = (int)Math.Ceiling((float)1080 / (float)TreeGridHeight);

            ObjectsInArea = new List<MovingObject>[HorizontalAreasCount, VerticalAreasCount];

            for (int y = 0; y < VerticalAreasCount; y++)
            {
                for (var x = 0; x < HorizontalAreasCount; x++)
                {
                    ObjectsInArea[x, y] = new List<MovingObject>();
                }
            }
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
                        drawTile.Index = new Vector2(x, y);
                        drawTile.LoadContent(content);
                        
                        DrawTiles[x, y] = drawTile;
                    }

                    if (Tiles[x, y] == TileType.BouncePad)
                    {
                        Tile drawTile = new Tile()
                        {
                            Size = TileSize,
                            Position = new Vector2(x * TileSize.X, y * TileSize.Y),
                            Color = Color.Red
                        };
                        drawTile.Index = new Vector2(x, y);
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

        public bool IsObstacle(int x, int y)
        {
            if (x < 0 || x >= MapSize.X || y < 0 || y >= MapSize.Y)
                return true;

            return (Tiles[x, y] == TileType.Solid || Tiles[x,y] == TileType.BouncePad);
        }

        //The player can stand on these types of blocks.
        //Need to consider that standing on a one-way platform is still "ground"
        public bool IsGround(int x, int y)
        {
            if (x < 0 || x >= MapSize.X || y < 0 || y >= MapSize.Y)
                return true;

            return (Tiles[x, y] == TileType.Solid);
        }

        public bool IsEmpty(int x, int y)
        {
            if (x < 0 || x >= MapSize.X || y < 0 || y >= MapSize.Y)
                return false;

            return (Tiles[x, y] == TileType.Empty);
        }

        public bool IsBounce(int x, int y)
        {
            return (Tiles[x, y] == TileType.BouncePad);
        }
        
        /// <summary>
        /// The the top left corner position of the tile at index x,y
        /// </summary>
        /// <param name="tileIndexX"></param>
        /// <param name="tileIndexY"></param>
        /// <returns></returns>
        public Vector2 GetTilePosition(int tileIndexX, int tileIndexY)
        {
            return new Vector2(
                    (float)(tileIndexX * TileSize.X),
                    (float)(tileIndexY * TileSize.Y)
                );
        }
        
        /// <summary>
        /// Get the x index of a tile at xPos
        /// </summary>
        /// <param name="xPos"></param>
        /// <returns></returns>
        public int GetMapTileXAtPoint(int xPos)
        {
            return (int)(xPos / TileSize.X);
        }
        
        /// <summary>
        /// Get the y index of a tile at yPos
        /// </summary>
        /// <param name="yPos"></param>
        /// <returns></returns>
        public int GetMapTileYAtPoint(int yPos)
        {
            return (int)(yPos / TileSize.Y);
        }

        public Vector2 GetMapTileAtPoint(Vector2 position)
        {
            return new Vector2((int)(position.X / TileSize.X), (int)(position.Y / TileSize.Y));
        }        
    }
}
