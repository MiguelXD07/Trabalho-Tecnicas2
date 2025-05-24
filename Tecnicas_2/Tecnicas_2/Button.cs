using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tecnicas_2
{
    public class Button
    {
        public Rectangle Bounds;
        public string Text;
        public bool IsHovered { get; private set; }
        public bool IsClicked { get; private set; }

        private SpriteFont _font;

        public Button(Rectangle bounds, string text, SpriteFont font)
        {
            Bounds = bounds;
            Text = text;
            _font = font;
        }

        public void Update(MouseState mouse, MouseState prevMouse)
        {
            IsHovered = Bounds.Contains(mouse.Position);
            IsClicked = IsHovered && mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D whitePixel)
        {
            Color color = IsHovered ? Color.LightGray : Color.Gray;
            spriteBatch.Draw(whitePixel, Bounds, color);
            var textSize = _font.MeasureString(Text);
            var textPos = new Vector2(Bounds.Center.X - textSize.X / 2, Bounds.Center.Y - textSize.Y / 2);
            spriteBatch.DrawString(_font, Text, textPos, Color.Black);
        }
    }
}
