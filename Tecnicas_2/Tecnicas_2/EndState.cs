using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tecnicas_2
{
    public class EndState
    {
        private readonly SpriteFont _font;
        private readonly Texture2D _whitePixel;
        private readonly Texture2D _background;
        private readonly Button _retryButton;
        private readonly Button _leaveButton;
        private MouseState _prevMouse;

        public bool RetryClicked => _retryButton.IsClicked;
        public bool LeaveClicked => _leaveButton.IsClicked;

        public EndState(SpriteFont font, Texture2D whitePixel, Texture2D background, int screenWidth, int screenHeight)
        {
            _font = font;
            _whitePixel = whitePixel;
            _background = background;
            int btnWidth = 200, btnHeight = 60;
            _retryButton = new Button(new Rectangle(screenWidth / 2 - 220, screenHeight / 2 + 40, btnWidth, btnHeight), "Retry", font);
            _leaveButton = new Button(new Rectangle(screenWidth / 2 + 20, screenHeight / 2 + 40, btnWidth, btnHeight), "Leave", font);
        }

        public void Update()
        {
            var mouse = Mouse.GetState();
            _retryButton.Update(mouse, _prevMouse);
            _leaveButton.Update(mouse, _prevMouse);
            _prevMouse = mouse;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            string scoreText = $"Score: {ScoreManager.Score}";
            string highScoreText = $"High Score: {ScoreManager.HighScore}";

            var scoreSize = _font.MeasureString(scoreText);
            var highScoreSize = _font.MeasureString(highScoreText);

            int centerX = 683;
            int centerY = 200;

            spriteBatch.Draw(_background, new Rectangle(0, 0, 1366, 768), Color.White);
            spriteBatch.DrawString(_font, scoreText, new Vector2(centerX - scoreSize.X / 2, centerY), Color.White);
            spriteBatch.DrawString(_font, highScoreText, new Vector2(centerX - highScoreSize.X / 2, centerY + 50), Color.Yellow);

            _retryButton.Draw(spriteBatch, _whitePixel);
            _leaveButton.Draw(spriteBatch, _whitePixel);
        }
    }
}