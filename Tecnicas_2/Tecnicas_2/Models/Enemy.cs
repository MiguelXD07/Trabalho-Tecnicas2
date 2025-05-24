using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tecnicas_2.Models
{
    public class Enemy
    {
        // Animation constants
        private const int FrameWidth = 96;
        private const int FrameHeight = 128;
        private const int FrameCount = 8;
        private const float FrameDuration = 0.12f;

        // Death animation
        private const int DeathFrameCount = 8;
        private const float DeathFrameDuration = 0.12f;
        private bool _isDying = false;
        private int _deathFrame = 0;
        private float _deathFrameTimer = 0f;
        public bool IsDespawned { get; private set; } = false;
        public Texture2D DeathTexture { get; set; }

        // Movement
        private const float Speed = 80f;

        // Animation state
        private int _currentFrame = 0;
        private float _frameTimer = 0f;

        // Position and direction
        public Vector2 Position;
        private float _direction; // -1 for left, 1 for right

        // Boundaries
        private readonly float _leftEdge;
        private readonly float _rightEdge;

        // State
        public bool IsAlive { get; private set; } = true;

        // Static spawn logic
        private static float _spawnTimer = 0f;
        private static float _spawnInterval = 3f;
        private static int _enemiesKilled = 0;

        public Enemy(Vector2 position, float leftEdge, float rightEdge, float direction)
        {
            Position = position;
            _leftEdge = leftEdge;
            _rightEdge = rightEdge;
            _direction = direction;
        }

        public Rectangle Hitbox =>
            new Rectangle((int)Position.X, (int)Position.Y, FrameWidth, FrameHeight);

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_isDying)
            {
                _deathFrameTimer += dt;
                if (_deathFrameTimer >= DeathFrameDuration)
                {
                    _deathFrameTimer = 0f;
                    _deathFrame++;
                    if (_deathFrame >= DeathFrameCount)
                    {
                        IsDespawned = true;
                    }
                }
                return;
            }

            if (!IsAlive) return;

            // Move
            Position.X += _direction * Speed * dt;

            // Turn at edges
            if (Position.X <= _leftEdge)
            {
                Position.X = _leftEdge;
                _direction = 1;
            }
            else if (Position.X + FrameWidth >= _rightEdge)
            {
                Position.X = _rightEdge - FrameWidth;
                _direction = -1;
            }

            // Animate
            _frameTimer += dt;
            while (_frameTimer >= FrameDuration)
            {
                _frameTimer -= FrameDuration;
                _currentFrame = (_currentFrame + 1) % FrameCount;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D walkTexture)
        {
            if (IsDespawned) return;

            Texture2D texture = _isDying && DeathTexture != null ? DeathTexture : walkTexture;
            int frame = _isDying ? _deathFrame : _currentFrame;
            var sourceRect = new Rectangle(frame * FrameWidth, 0, FrameWidth, FrameHeight);
            var effects = _direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            spriteBatch.Draw(
                texture,
                new Vector2((int)Position.X, (int)Position.Y),
                sourceRect,
                Color.White,
                0f,
                Vector2.Zero,
                1f,
                effects,
                0f
            );
        }

        public bool CollidesWith(Rectangle playerRect)
        {
            return IsAlive && Hitbox.Intersects(playerRect);
        }

        public void Kill()
        {
            if (_isDying || IsDespawned) return;
            _isDying = true;
            IsAlive = false;
            _deathFrame = 0;
            _deathFrameTimer = 0f;
            _enemiesKilled++;
            if (_enemiesKilled % 5 == 0 && _spawnInterval > 1.5f)
                _spawnInterval = MathF.Max(1.5f, _spawnInterval - 0.5f);
        }

        // Static: Spawner logic
        public static void UpdateSpawner(
            GameTime gameTime,
            Action<Enemy> spawnEnemy,
            float leftEdge,
            float rightEdge,
            float groundY)
        {
            _spawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_spawnTimer >= _spawnInterval)
            {
                _spawnTimer = 0f;
                bool spawnLeft = Random.Shared.Next(2) == 0;
                float x = spawnLeft ? leftEdge : rightEdge - FrameWidth;
                float dir = spawnLeft ? 1 : -1;
                spawnEnemy(new Enemy(new Vector2(x, groundY - FrameHeight), leftEdge, rightEdge, dir));
            }
        }
    }
}
