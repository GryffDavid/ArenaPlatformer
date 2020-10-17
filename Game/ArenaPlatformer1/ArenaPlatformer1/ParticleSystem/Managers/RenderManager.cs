using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ArenaPlatformer1
{
    public class RenderManager
    {
        public List<RenderData> RenderDataObjects { get; set; }
        private DoubleBuffer DoubleBuffer;
        private GameTime GameTime;

        protected ChangeBuffer MessageBuffer;
        
        public RenderManager(DoubleBuffer doubleBuffer)
        {
            DoubleBuffer = doubleBuffer;
            RenderDataObjects = new List<RenderData>();
        }

        public void LoadContent(ContentManager content)
        {
            //Texture = content.Load<Texture2D>("diamond");
        }

        public void DoFrame()
        {
            DoubleBuffer.StartRenderProcessing(out MessageBuffer, out GameTime);

            foreach (ChangeMessage msg in MessageBuffer.Messages)
            {
                switch (msg.MessageType)
                {
                    #region UpdateParticle

                    case ChangeMessageType.UpdateParticle:
                        {
                            //TODO: Got an out of index error here. Very, very unusual and possibly very intermittent
                            RenderDataObjects[msg.ID].Position = msg.Position;
                            RenderDataObjects[msg.ID].Rotation = msg.Rotation;
                            RenderDataObjects[msg.ID].Color = msg.Color;
                            RenderDataObjects[msg.ID].Scale = msg.Scale;
                            RenderDataObjects[msg.ID].Transparency = msg.Transparency;
                        }
                        break;
                    #endregion

                    #region DeleteRenderData
                    case ChangeMessageType.DeleteRenderData:
                        {
                            RenderDataObjects.RemoveAt(msg.ID);
                        }
                        break;
                        #endregion
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (RenderData renderData in RenderDataObjects.Where(data => data.Emissive == false && data.Lit == false))
            {
                spriteBatch.Draw(renderData.Texture,
                    new Rectangle((int)renderData.Position.X,
                                  (int)renderData.Position.Y,
                                  (int)(renderData.Texture.Width * renderData.Scale),
                                  (int)(renderData.Texture.Height * renderData.Scale)),
                    null, renderData.Color * renderData.Transparency, renderData.Rotation,
                    new Vector2(renderData.Texture.Width / 2, renderData.Texture.Height / 2), (SpriteEffects)renderData.Orientation, renderData.DrawDepth);
            }
        }

        public void DrawEmissive(SpriteBatch spriteBatch)
        {
            foreach (RenderData renderData in RenderDataObjects.Where(data => data.Emissive == true))
            {
                spriteBatch.Draw(renderData.Texture,
                    new Rectangle((int)renderData.Position.X,
                                  (int)renderData.Position.Y,
                                  (int)(renderData.Texture.Width * renderData.Scale),
                                  (int)(renderData.Texture.Height * renderData.Scale)),
                    null, renderData.Color * renderData.Transparency, renderData.Rotation,
                    new Vector2(renderData.Texture.Width / 2, renderData.Texture.Height / 2), (SpriteEffects)renderData.Orientation, renderData.DrawDepth);
            }
        }

        public void DrawLit(SpriteBatch spriteBatch)
        {
            foreach (RenderData renderData in RenderDataObjects.Where(data => data.Lit == true))
            {
                spriteBatch.Draw(renderData.Texture,
                    new Rectangle((int)renderData.Position.X,
                                  (int)renderData.Position.Y,
                                  (int)(renderData.Texture.Width * renderData.Scale),
                                  (int)(renderData.Texture.Height * renderData.Scale)),
                    null, renderData.Color * renderData.Transparency, renderData.Rotation,
                    new Vector2(renderData.Texture.Width / 2, renderData.Texture.Height / 2), (SpriteEffects)renderData.Orientation, renderData.DrawDepth);
            }
        }

        //public SpriteEffects GetOrientation(RenderData data)
        //{
        //    SpriteEffects Orientation = SpriteEffects.None;

        //    switch (data.Orientation)
        //    {
        //        default:
        //            Orientation = SpriteEffects.None;
        //            break;

        //        case 1:
        //            Orientation = SpriteEffects.FlipHorizontally;
        //            break;

        //        case 2:
        //            Orientation = SpriteEffects.FlipVertically;
        //            break;
        //    }

        //    return Orientation;
        //}
    }
}
