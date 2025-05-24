using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Tecnicas_2.Models;
using System;

namespace Tecnicas_2
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Mapa _map;
        private Player _player;
        private Camera _camera;
        private Texture2D _whitePixel;
        private Texture2D _background;

        private List<Enemy> _enemies = new();
        private Texture2D _enemyTexture;
        private Texture2D _enemyDeathTexture;
        private enum GameState { Playing, End }
        private GameState _gameState = GameState.Playing;
        private EndState _endState;
        private SpriteFont _font;

        private SoundEffect _playerDeathSfx;
        private SoundEffect _enemyDeathSfx;
        private SoundEffect _jumpSfx;
        private SoundEffect _walkSfx;
        private SoundEffectInstance _walkSfxInstance;
        private SoundEffect _attackSfx;
        private Song _bgm;

        private double _walkStepTimer;
        private const double WalkStepInterval = 0.5;
        private bool _wasWalking;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1366,
                PreferredBackBufferHeight = 768
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _map = new Mapa();
            _camera = new Camera(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _player = new Player(new Vector2(100, 150), 96, 128) { Scale = 1f };
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _map.Load(Content);

            _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });

            _camera.SetWorldBounds(_map.PisoLeft, _map.PisoRight, _map.PisoY);

            _player.IdleTexture = Content.Load<Texture2D>("Sprites/rogue_idle");
            _player.WalkTexture = Content.Load<Texture2D>("Sprites/rogue_run");
            _player.JumpTexture = Content.Load<Texture2D>("Sprites/rogue_jump");
            _player.AttackTexture = Content.Load<Texture2D>("Sprites/rogue atack");

            _enemyTexture = Content.Load<Texture2D>("Sprites/rogue_run_ghost");
            _enemyDeathTexture = Content.Load<Texture2D>("Sprites/rogue death");

            _font = Content.Load<SpriteFont>("Fonts/DefaultFont");
            _background = Content.Load<Texture2D>("Sprites/Background");
            _endState = new EndState(_font, _whitePixel, _background, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            _playerDeathSfx = Content.Load<SoundEffect>("Audio/Player Death");
            _enemyDeathSfx = Content.Load<SoundEffect>("Audio/monster hit");
            _jumpSfx = Content.Load<SoundEffect>("Audio/jump");
            _attackSfx = Content.Load<SoundEffect>("Audio/melee swing 2");
            _walkSfx = Content.Load<SoundEffect>("Audio/footsteps forest-");
            _walkSfxInstance = _walkSfx.CreateInstance();
            _bgm = Content.Load<Song>("Audio/BG_Music");

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.5f;
            MediaPlayer.Play(_bgm);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (_gameState)
            {
                case GameState.Playing:
                    UpdatePlaying(gameTime);
                    break;
                case GameState.End:
                    UpdateEnd();
                    break;
            }

            base.Update(gameTime);
        }

        private void UpdatePlaying(GameTime gameTime)
        {
            _player.Update(gameTime, _map.LeftTree.Hitbox, _map.RightTree.Hitbox);
            _camera.Follow(_player.Rect);

            Enemy.UpdateSpawner(gameTime, enemy =>
            {
                enemy.DeathTexture = _enemyDeathTexture;
                _enemies.Add(enemy);
            }, _map.PisoLeft, _map.PisoRight, _map.PisoY);

            foreach (var enemy in _enemies)
            {
                enemy.Update(gameTime);
                if (enemy.CollidesWith(_player.Rect))
                {
                    _player.Die();
                    _playerDeathSfx.Play();
                    _gameState = GameState.End;
                    _walkSfxInstance.Stop();
                    MediaPlayer.Stop();
                }
            }

            var attackBox = _player.GetAttackBox();
            if (attackBox.HasValue)
            {
                foreach (var enemy in _enemies)
                {
                    if (enemy.IsAlive && enemy.Hitbox.Intersects(attackBox.Value))
                    {
                        enemy.Kill();
                        _enemyDeathSfx.Play();
                        ScoreManager.AddScore(100);
                    }
                }
            }

            _enemies = _enemies.Where(e => !e.IsDespawned).ToList();

            if (_player.IsOnGround && Math.Abs(_player.Velocity.X) > 0.1f)
            {
                _walkStepTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                if (_walkStepTimer <= 0)
                {
                    if (!_wasWalking)
                    {
                        _walkSfxInstance.Play();
                        _wasWalking = true;
                    }
                    _walkStepTimer = WalkStepInterval;
                }
            }
            else
            {
                _walkSfxInstance.Stop();
                _wasWalking = false;
                _walkStepTimer = 0;
            }
        }

        private void UpdateEnd()
        {
            _endState.Update();
            if (_endState.RetryClicked)
            {
                ScoreManager.ResetScore();
                _player = new Player(new Vector2(100, 150), 96, 128)
                {
                    IdleTexture = Content.Load<Texture2D>("Sprites/rogue_idle"),
                    WalkTexture = Content.Load<Texture2D>("Sprites/rogue_run"),
                    JumpTexture = Content.Load<Texture2D>("Sprites/rogue_jump"),
                    AttackTexture = Content.Load<Texture2D>("Sprites/rogue atack")
                };
                _enemies.Clear();
                _gameState = GameState.Playing;
                MediaPlayer.Play(_bgm);
            }
            else if (_endState.LeaveClicked)
            {
                Exit();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (_gameState == GameState.Playing)
            {
                _spriteBatch.Begin(transformMatrix: _camera.Transform);

                // Tile the background from PisoLeft to PisoRight, shifted up by 100 pixels
                int bgWidth = _background.Width;
                int bgHeight = _graphics.PreferredBackBufferHeight;
                int yOffset = -100; // Move background up by 100 pixels

                for (int x = _map.PisoLeft; x < _map.PisoRight; x += bgWidth)
                {
                    int drawWidth = Math.Min(bgWidth, _map.PisoRight - x);
                    _spriteBatch.Draw(
                        _background,
                        new Rectangle(x, yOffset, drawWidth, bgHeight),
                        new Rectangle(0, 0, drawWidth, _background.Height),
                        Color.White
                    );
                }

                _map.Draw(_spriteBatch);
                _player.Draw(_spriteBatch);

                foreach (var enemy in _enemies)
                    enemy.Draw(_spriteBatch, _enemyTexture);

                _spriteBatch.End();
            }
            else if (_gameState == GameState.End)
            {
                _spriteBatch.Begin();
                _endState.Draw(_spriteBatch);
                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}