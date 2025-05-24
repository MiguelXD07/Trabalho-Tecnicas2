using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tecnicas_2
{
    public class Objeto
    {
        public Texture2D Texture;
        public Vector2 Position;
        public int CustomHitboxWidth = -1;  // -1 means use Texture.Width
        public int CustomHitboxHeight = -1; // -1 means use Texture.Height

        public Rectangle Hitbox
        {
            get
            {
                int width;
                if (CustomHitboxWidth > 0)
                    width = CustomHitboxWidth;
                else
                    width = Texture.Width;

                int height;
                if (CustomHitboxHeight > 0)
                    height = CustomHitboxHeight;
                else
                    height = Texture.Height;

                int offsetX = (Texture.Width - width) / 2;
                int offsetY = Texture.Height - height; // Align hitbox to the top of the piso
                return new Rectangle((int)Position.X + offsetX, (int)Position.Y + offsetY, width, height);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.White);
        }
    }
}