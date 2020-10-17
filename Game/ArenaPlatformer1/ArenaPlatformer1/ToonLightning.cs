using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    class ToonLightning
    {
        bool Active;

        Vector2 EndPosition, StartPosition;

        public static Random Random = new Random();

        public class Node
        {
            public Vector2 NodePosition, NodeEnd, Direction, TangentDirection;
            public float Angle, Width;
        }

        public new Color Color = Color.White * 0.9f;

        public Vector2 LengthRange;

        int TotalLength = 100;

        float fadeTime = 0;

        new VertexPositionColor[] vertices;// = new VertexPositionColor[200];
        VertexPositionColor[] vertices2;// = new VertexPositionColor[200];

        public List<Node> NodeList = new List<Node>();

        public ToonLightning(int segments, int width, Vector2 startPoint, Vector2 endPoint, Vector2 lenRang, Color color)
        {
            Active = true;
            LengthRange = lenRang;

            TotalLength = segments;

            vertices = new VertexPositionColor[TotalLength];
            vertices2 = new VertexPositionColor[TotalLength];

            EndPosition = endPoint;
            StartPosition = startPoint;

            Color = color;

            for (int i = 0; i < TotalLength / 2; i++)
            {
                Node newNode;

                #region First Node
                if (i == 0)
                {
                    newNode = new Node()
                    {
                        NodePosition = StartPosition,
                        Angle = Random.Next(-90, 90),
                        Width = Random.Next(5, 10)
                    };

                    newNode.Direction = new Vector2((float)Math.Cos(MathHelper.ToRadians(newNode.Angle)),
                                                    (float)Math.Sin(MathHelper.ToRadians(newNode.Angle)));
                    newNode.Direction.Normalize();
                    newNode.NodeEnd = newNode.NodePosition + (newNode.Direction * 25);

                    NodeList.Add(newNode);


                    vertices[i] = new VertexPositionColor(new Vector3(NodeList[0].NodePosition, 0), Color);
                    vertices[i + 1] = new VertexPositionColor(new Vector3(NodeList[0].NodeEnd, 0), Color);

                    vertices2[0] = new VertexPositionColor(new Vector3(StartPosition, 0), Color);
                    vertices2[1] = new VertexPositionColor(new Vector3(StartPosition.X, StartPosition.Y + 10, 0), Color);
                }
                else
                #endregion
                {
                    #region Adjust angle and length
                    float ang, lenp;

                    lenp = Random.Next(20, 40);

                    if (Random.NextDouble() >= 0.5)
                    {
                        ang = (MathHelper.Lerp(NodeList[NodeList.Count - 1].Angle, Random.Next(-90, 90), 0.1f));
                    }
                    else
                    {
                        ang = (MathHelper.Lerp(NodeList[NodeList.Count - 1].Angle, Random.Next(-90, 90), 0.2f)) / i;
                    }

                    if (Random.NextDouble() > 0.8)
                    {
                        lenp = Random.Next((int)LengthRange.X, (int)LengthRange.Y);

                        int m;

                        if (Random.NextDouble() >= 0.5)
                        {
                            m = -1;
                        }
                        else
                        {
                            m = 1;
                        }

                        ang = (MathHelper.Lerp(NodeList[NodeList.Count - 1].Angle, Random.Next(90, 120) * m, 0.3f));
                    }

                    #endregion

                    newNode = new Node()
                    {
                        NodePosition = NodeList[i - 1].NodeEnd,
                        Angle = ang,
                        Width = MathHelper.Lerp(NodeList[i - 1].Width, Random.Next(0, width), 0.4f)
                    };

                    if (i > (TotalLength / 2) - 5)
                    {
                        newNode = new Node()
                        {
                            NodePosition = NodeList[i - 1].NodeEnd,
                            Angle = ang,
                            Width = (TotalLength / 2) - i
                        };
                    }

                    newNode.Direction = new Vector2((float)Math.Cos(MathHelper.ToRadians(newNode.Angle)),
                                                    (float)Math.Sin(MathHelper.ToRadians(newNode.Angle)));
                    newNode.NodeEnd = newNode.NodePosition + (newNode.Direction * lenp);

                    NodeList.Add(newNode);
                }
            }

            #region Adjust Deltas
            float DeltaY = NodeList[NodeList.Count - 1].NodeEnd.Y - EndPosition.Y;
            float DeltaX = NodeList[NodeList.Count - 1].NodeEnd.X - EndPosition.X;

            List<Node> NewNodes = new List<Node>
            {
                NodeList[0],
                NodeList[1]
            };

            for (int i = 2; i < TotalLength / 2; i++)
            {
                double dif = (i / (double)((TotalLength - 2) / 2));

                NodeList[i].NodeEnd.Y -= (float)(DeltaY * dif);
                //node.NodePosition.Y -= (float)(DeltaY * dif);

                NodeList[i].NodeEnd.X -= (float)(DeltaX * dif);
                //node.NodePosition.X -= (float)(DeltaX * dif);

            }
            #endregion


            Vector2 diffy;
            float angl;


            for (int p = 1; p < TotalLength / 2 - 1; p++)
            {
                diffy = (NodeList[p + 1].NodeEnd - NodeList[p - 1].NodeEnd);
                angl = (float)Math.Atan2(diffy.Y, diffy.X);

                angl -= (float)MathHelper.ToRadians(90 + Random.Next(-5, 5));

                NodeList[p].TangentDirection = new Vector2((float)Math.Cos(angl), (float)Math.Sin(angl));
                NodeList[p].TangentDirection.Normalize();
            }

            for (int i = 2; i < TotalLength; i += 2)
            {
                vertices2[i] = new VertexPositionColor(new Vector3(NodeList[i / 2].NodeEnd - (NodeList[i / 2].TangentDirection * NodeList[i / 2].Width), 0), Color);
                vertices2[i + 1] = new VertexPositionColor(new Vector3(NodeList[i / 2].NodeEnd + (NodeList[i / 2].TangentDirection * NodeList[i / 2].Width), 0), Color);
            }
        }

        public void Update(GameTime gameTime)
        {
            fadeTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (fadeTime > 250)
            {
                for (int i = 0; i < vertices2.Length - 1; i++)
                {
                    vertices2[i].Color = Color.Lerp(vertices2[i].Color, Color.Transparent, 0.2f);
                }

                if (vertices2[vertices2.Count() - 2].Color == Color.Transparent)
                {
                    Active = false;
                }
            }

            //if (curInd < vertices2.Length-8)
            //{
            //    vertices2[curInd].Color = Color.White;
            //    vertices2[curInd+1].Color = Color.White;
            //    vertices2[curInd + 2].Color = Color.White;
            //    vertices2[curInd + 3].Color = Color.White;
            //    vertices2[curInd + 4].Color = Color.White;
            //    vertices2[curInd + 5].Color = Color.White;
            //    vertices2[curInd + 6].Color = Color.White;
            //    vertices2[curInd + 7].Color = Color.White;
            //    vertices2[curInd + 8].Color = Color.White;

            //    //timeR = 0;
            //    curInd += 8;
            //}

            //if (vertices2[vertices2.Length-1].Color == Color.White)
            //{
            //    if (curInd2 < vertices2.Length -5)
            //    {
            //        vertices2[curInd2].Color = Color.Transparent;
            //        vertices2[curInd2 + 1].Color = Color.Transparent;
            //        vertices2[curInd2 + 2].Color = Color.Transparent;
            //        vertices2[curInd2 + 3].Color = Color.Transparent;
            //        vertices2[curInd2 + 4].Color = Color.Transparent;
            //        vertices2[curInd2 + 5].Color = Color.Transparent;

            //        curInd2 += 5;
            //    }
            //}
        }

        public void Draw(GraphicsDevice graphics, BasicEffect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices2, 0, TotalLength - 2);
            }

            //base.Draw(graphics, effect);
        }

        private int GetEven(int num)
        {
            if (num % 2 == 0)
            {
                return num;
            }
            else
            {
                return num + 1;
            }
        }

    }
}
