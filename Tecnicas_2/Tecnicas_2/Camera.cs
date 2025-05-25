using Microsoft.Xna.Framework;

namespace Tecnicas_2
{
    public class Camera
    {
        public Matrix Transform;
        public Vector2 Position;
        private int viewportWidth;
        private int viewportHeight;

        // World bounds
        private int worldLeft;
        private int worldRight;
        private int worldTop = 0;
        private int worldBottom;

        public Camera(int viewportWidth, int viewportHeight)
        {
            this.viewportWidth = viewportWidth;
            this.viewportHeight = viewportHeight;
        }

        // Set world bounds
        public void SetWorldBounds(int left, int right, int bottom)
        {
            worldLeft = left;
            worldRight = right;
            worldBottom = bottom;
        }

        public void Follow(Rectangle playerRect)
        {
            // Center camera on player
            float camX = playerRect.Center.X - viewportWidth / 2;
            float camY = playerRect.Center.Y - viewportHeight / 2;

            // Clamp X so camera doesn't go beyond the world bounds
            float minX = worldLeft;
            float maxX = worldRight - viewportWidth;
            camX = MathHelper.Clamp(camX, minX, maxX);

            // Clamp Y so camera doesn't go below the floor
            float minY = worldTop;
            float maxY = worldBottom - viewportHeight;
            camY = MathHelper.Clamp(camY, minY, maxY);

            Position = new Vector2(camX, camY);
            Transform = Matrix.CreateTranslation(-Position.X, -Position.Y, 0f);
        }
    }
}
