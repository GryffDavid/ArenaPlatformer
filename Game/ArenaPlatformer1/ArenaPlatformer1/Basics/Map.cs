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

    [Serializable]
    public class Map
    {
        public static Random Random = new Random();

        //Collision tiles
        private TileType[,] Tiles;
        public ItemSpawn[] ItemSpawnList;

        //Draw data
        public Tile[,] DrawTiles;

        //The size of each tile
        public Vector2 TileSize = new Vector2(64, 64);

        //The size of the map in tiles
        public Vector2 MapSize = new Vector2(30, 17);

        public List<Vector2> SpawnTiles;
        
        public int TreeGridWidth = 64;        
        public int TreeGridHeight = 64;
        
        public List<MovingObject>[,] ObjectsInArea;

        public List<Light> LightList;
        
        int HorizontalAreasCount;
        int VerticalAreasCount;
        
        public Map()
        {
            Tiles = new TileType[(int)MapSize.X, (int)MapSize.Y];
            DrawTiles = new Tile[(int)MapSize.X, (int)MapSize.Y];            
        }

        public void Initialize()
        {
            TreeGridWidth = 64;
            TreeGridHeight = 64;

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
            DrawTiles = new Tile[(int)MapSize.X, (int)MapSize.Y];
            SpawnTiles = new List<Vector2>();
            LightList = new List<Light>();

            for (int x = 0; x < (int)MapSize.X; x++)
            {
                for (int y = 0; y < (int)MapSize.Y; y++)
                {
                    switch (Tiles[x, y])
                    {
                        case TileType.Spawn:
                            {
                                SpawnTiles.Add(new Vector2(x, y));
                            }
                            break;

                        case TileType.Solid:
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
                            break;

                        case TileType.BouncePad:
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
                            break;
                    }
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (ItemSpawn spawn in ItemSpawnList.Where(Spawn => Spawn != null))
            {
                spawn.Update(gameTime);
            }
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

        public void UpdateAreas(MovingObject movingObject)
        {
            //if (new Rectangle(0, 0, 1920, 1080).Contains(new Point(movingObject.CollisionRectangle.Right, (int)movingObject.Center.Y)))
            {
                List<Vector2> OverlappingAreas = new List<Vector2>();

                Vector2 topLeft = GetMapTileAtPoint(
                    new Vector2(
                    movingObject.CollisionRectangle.Left,
                    movingObject.CollisionRectangle.Top)
                    );

                Vector2 topRight = GetMapTileAtPoint(
                    new Vector2(
                    movingObject.CollisionRectangle.Right,
                    movingObject.CollisionRectangle.Top)
                    );

                Vector2 bottomLeft = GetMapTileAtPoint(
                    new Vector2(
                    movingObject.CollisionRectangle.Left,
                    movingObject.CollisionRectangle.Bottom)
                    );

                Vector2 bottomRight = GetMapTileAtPoint(new Vector2(
                    movingObject.CollisionRectangle.Right,
                    movingObject.CollisionRectangle.Bottom
                    ));

                topLeft.X /= TreeGridWidth;
                topLeft.Y /= TreeGridHeight;

                topRight.X /= TreeGridWidth;
                topRight.Y /= TreeGridHeight;

                bottomLeft.X /= TreeGridWidth;
                bottomLeft.Y /= TreeGridHeight;

                bottomRight.X /= TreeGridWidth;
                bottomRight.Y /= TreeGridHeight;

                if (topLeft.X == topRight.X && topLeft.Y == bottomLeft.Y)
                {
                    OverlappingAreas.Add(topLeft * 64);
                }
                else if (topLeft.X == topRight.X)
                {
                    OverlappingAreas.Add(topLeft * 64);
                    OverlappingAreas.Add(bottomLeft * 64);
                }
                else if (topLeft.Y == bottomLeft.Y)
                {
                    OverlappingAreas.Add(topLeft * 64);
                    OverlappingAreas.Add(topRight * 64);
                }
                else
                {
                    OverlappingAreas.Add(topLeft * 64);
                    OverlappingAreas.Add(bottomLeft * 64);
                    OverlappingAreas.Add(topRight * 64);
                    OverlappingAreas.Add(bottomRight * 64);
                }

                var areas = movingObject.Areas;
                var IDs = movingObject.IDsInAreas;

                for (int i = 0; i < areas.Count; i++)
                {
                    if (!OverlappingAreas.Contains(areas[i]))
                    {
                        RemoveObjectFromArea(areas[i], IDs[i], movingObject);
                        areas.RemoveAt(i);
                        IDs.RemoveAt(i);
                        i--;
                    }
                }

                for (var i = 0; i < OverlappingAreas.Count; i++)
                {
                    var area = OverlappingAreas[i];
                    if (!areas.Contains(area))
                        AddObjectToArea(area, movingObject);
                }

                OverlappingAreas.Clear();
            }
        }

        public void AddObjectToArea(Vector2 areaIndex, MovingObject movingObject)
        {
            var area = ObjectsInArea[(int)areaIndex.X, (int)areaIndex.Y];

            movingObject.Areas.Add(areaIndex);
            movingObject.IDsInAreas.Add(area.Count);

            area.Add(movingObject);
        }

        public void RemoveObjectFromArea(Vector2 areaIndex, int objectIndex, MovingObject movingObject)
        {
            var area = ObjectsInArea[(int)areaIndex.X, (int)areaIndex.Y];

            //Swap the last item with the one we are removing
            var temp = area[area.Count - 1];
            area[area.Count - 1] = movingObject;
            area[objectIndex] = temp;

            var tempIDs = temp.IDsInAreas;
            var tempAreas = temp.Areas;

            for (int i = 0; i < tempAreas.Count; i++)
            {
                if (tempAreas[i] == areaIndex)
                {
                    tempIDs[i] = objectIndex;
                    break;
                }
            }

            //Remove last item
            area.RemoveAt(area.Count - 1);
        }

        /// <summary>
        /// Check collisions between all MovingObjects 
        /// </summary>
        public void CheckCollisions()
        {
            for (int y = 0; y < VerticalAreasCount; y++)
            {
                for (int x = 0; x < HorizontalAreasCount; x++)
                {
                    var objectsInArea = ObjectsInArea[x, y];
                    
                    for (int i = 0; i < objectsInArea.Count - 1; i++)
                    {
                        var object1 = objectsInArea[i];

                        for (int j = i + 1; j < objectsInArea.Count; j++)
                        {
                            var object2 = objectsInArea[j];

                            Vector2 overlap;

                            if (object1.OverlapsSigned(object2, out overlap) && !object1.HasCollisionDataFor(object2))
                            {
                                object1.CollisionDataList.Add(
                                    new MovingObject.CollisionData(
                                        object2, overlap,
                                        object1.Velocity, object2.Velocity,
                                        object1.PreviousPosition, object2.PreviousPosition,
                                        object1.Position, object2.Position));

                                object2.CollisionDataList.Add(
                                    new MovingObject.CollisionData(
                                        object1, -overlap,
                                        object2.Velocity, object1.Velocity,
                                        object2.PreviousPosition, object1.PreviousPosition,
                                        object2.Position, object1.Position));
                            }
                        }
                    }
                }
            }
        }

        public Vector2 FindSpawn()
        {
            if (SpawnTiles.Count == 0)
            {
                return new Vector2(5, 5);
            }
            else
                return SpawnTiles[Random.Next(0, SpawnTiles.Count)];
        }        
    }
}
