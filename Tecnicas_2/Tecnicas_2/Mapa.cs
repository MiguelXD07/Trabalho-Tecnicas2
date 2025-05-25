using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Tecnicas_2
{
    public class Mapa
    {
        private List<Objeto> _objetos;

        public int PisoLeft;
        public int PisoRight;
        public int PisoY;
        public Objeto LeftTree;
        public Objeto RightTree;

        public Mapa()
        {
            _objetos = new List<Objeto>();
        }

        public void Load(ContentManager content)
        {
            // Load the piso texture
            Texture2D pisoTexture = content.Load<Texture2D>("Sprites/Piso");

            // Load the tree texture
            Texture2D treeTexture = content.Load<Texture2D>("Sprites/trees");

            // Screen and piso info
            int screenWidth = 1366;
            int screenHeight = 768;
            int pisoWidth = pisoTexture.Width;
            int pisoHeight = pisoTexture.Height;
            int tilesNeeded = (int)Math.Ceiling(screenWidth / (float)pisoWidth);
            int extraTiles = 2; // 1 extra to the left, 1 to the right
            int startX = -extraTiles * pisoWidth;
            int endX = (tilesNeeded + extraTiles) * pisoWidth;
            float pisoY = 650;
            PisoY = (int)pisoY;

            PisoLeft = startX;
            PisoRight = endX;

            // Add piso tiles
            for (int x = startX; x < endX; x += pisoWidth)
            {
                _objetos.Add(new Objeto{Texture = pisoTexture, Position = new Vector2(x, pisoY), CustomHitboxHeight = 10});
            }

            // Tree info
            int treeWidth = treeTexture.Width;
            int treeHeight = treeTexture.Height;
            float treeY = 125;

            // Leftmost tree (at the left end of the piso)
            LeftTree = new Objeto { Texture = treeTexture, Position = new Vector2(startX, treeY), CustomHitboxWidth = 24 };
            _objetos.Add(LeftTree);

            // Rightmost tree (at the right end of the piso)
            RightTree = new Objeto { Texture = treeTexture, Position = new Vector2(endX - treeWidth, treeY), CustomHitboxWidth = 24 };
            _objetos.Add(RightTree);

            // Random trees in the middle (not at the very ends)
            Random rand = new Random();
            int minX = startX + treeWidth;
            int maxX = endX - 2 * treeWidth;
            int minDistance = treeWidth * 2; // Minimum distance between trees

            // Generate the first random tree
            int randomX1 = rand.Next(minX, maxX);

            // Try to generate the second random tree far enough from the first
            int randomX2;
            int attempts = 0;
            do
            {
                randomX2 = rand.Next(minX, maxX);
                attempts++;
            } while (Math.Abs(randomX2 - randomX1) < minDistance && attempts < 100);

            _objetos.Add(new Objeto { Texture = treeTexture, Position = new Vector2(randomX1, treeY) });
            _objetos.Add(new Objeto { Texture = treeTexture, Position = new Vector2(randomX2, treeY) });
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var objeto in _objetos)
            {
                objeto.Draw(spriteBatch);
            }
        }

    }
}
