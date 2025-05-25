# Trabalho-Tecnicas2
## Trabalho realizado por:
Guilherme Carneiro - A31463

Miguel Ferreira - A31472

Nuno Santos - A33191

## Créditos dos sprites e audio
https://pixabay.com/pt/

https://opengameart.org

https://mixkit.co

## Introdução
Este projeto foi desenvolvido no âmbito da unidade curricular de Técnicas de Desenvolvimento de Jogos Digitais, tendo como objetivo o desenvolvimento de um jogo em MonoGame, com um mundo aberto e linear, sem o uso de tiles.

Para este trabalho, optámos por desenvolver um Sidescroller Arena Survival onde o jogador tem como objetivo sobreviver o tempo máximo dentro de uma arena horizontal, que se estende para ambos os lados, derrotando o maior número de inimigos possível e desviando-se dos seus ataques constantes. A cada inimigo derrotado o jogador acumula pontos, os quais representam o tempo que o mesmo se manteve vivo. O jogo termina quando o jogador é derrotado.

Não foi possível uma organização mais eficiente dos ficheiros, porém criamos uma pasta "Models" que contém as classes para o Player e o Enemy, criamos também uma pasta "Content" e dentro dela mais 3 folders essencias: Sprites, Audio e Fonts; compostos por imagens, sons e as fontes de texto respetivemente. As classes restantes: Buttons, ScoreManager, Objeto, Mapa, EndState e Game1; ficaram na raiz do projeto.

![image](https://github.com/user-attachments/assets/da020c7f-ae78-4749-9e72-e570ec3d0f46)
------------------------------------------------------------------------------------------------------------------------------------
## Player

A class Player é responsável por criar e gerir o personagem principal. Implementamos nesta classe as mecânicas de movimento, incluindo salto e velocidade, a direção para qual o player está virado e um sistema de ataque.
Neste excerto de código temos o método "GetAttackBox()", onde é calculada a área de ataque do jogador assim como o método "Draw()", que senha o jogador no ecrã consoante o seu estado: iddle, a andar 
```
 public Rectangle? GetAttackBox()
 {
     if (!isAttacking) return null;
     int x = facingRight ? (int)(Position.X + spriteWidth) : (int)(Position.X - AttackWidth);
     int y = (int)(Position.Y + spriteHeight / 2 - AttackHeight / 2);
     return new Rectangle(x, y, AttackWidth, AttackHeight);
 }

 public void Draw(SpriteBatch spriteBatch)
 {
     Texture2D texture;
     int frameCount;
     if (state == PlayerState.Attack)
     {
         texture = AttackTexture;
         frameCount = attackFrames;
     }
     else if (state == PlayerState.Idle)
     {
         texture = IdleTexture;
         frameCount = idleFrames;
     }
     else if (state == PlayerState.Walk)
     {
         texture = WalkTexture;
         frameCount = walkFrames;
     }
     else
     {
         texture = JumpTexture;
         frameCount = jumpFrames;
     }

     Rectangle sourceRect = new(currentFrame * spriteWidth, 0, spriteWidth, spriteHeight);
     var effects = facingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
     spriteBatch.Draw(texture, Position, sourceRect, Color.White, 0f, Vector2.Zero, Scale, effects, 0f);
 }

public void Die() => IsDead = true;
```
## Enemy

Esta classe representa os inimigo do jogo. Criamos aqui a hitbox, lógica de movimento automático, da animação e fazemos a gestão de spawn dos mesmos.
Implementamos também um sistema de dificuldade: cada vez que o jogador derrota 5 inimigos o tempo entre spawns diminui 0.5 segundos, indo dos iniciais 3 segundos até 1.5 onde fica até o player perder.
```
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

        public Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, FrameWidth, FrameHeight);

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

            spriteBatch.Draw(texture, new Vector2((int)Position.X, (int)Position.Y), sourceRect, Color.White, 0f, Vector2.Zero, 1f, effects, 0f);
        }

        public bool CollidesWith(Rectangle playerRect)
        {
            return IsAlive && Hitbox.Intersects(playerRect);
        }
```
```
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
```
------------------------------------------------------------------------------------------------------------------------------------
## Classe Button:

Esta classe é usada para a criação de botões no ecrã. Contem toda a estrutura e lógica necessária para gerir o estado de um botão, inculindo detetar quando o cursor passa por cima e/ou clica no mesmo. Por fim, contém o método "Draw()" que é responsável por desenhar o botão no ecrã e trocar as suas cores quando está hovered.
Aqui definimos os limites do botão, assim como o texto e a fonte.

```
public Button(Rectangle bounds, string text, SpriteFont font)
        {
            Bounds = bounds;
            Text = text;
            _font = font;
        }
```
No método "Update()" verificamos se o botão tem o cursor por cima e se foi clicado com o boão esquerdo do rato
```
        public void Update(MouseState mouse, MouseState prevMouse)
        {
            IsHovered = Bounds.Contains(mouse.Position);
            IsClicked = IsHovered && mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released;
        }
```

------------------------------------------------------------------------------------------------------------------------------------
## Classe Camera:

Classe que gere a posição da camera que segue jogador. O movimento vertical é limitado entre o topo do mundo e o chão, enquanto o movimento horizontal é restringido pelas dimensôes da arena.

```
public Camera(int viewportWidth, int viewportHeight)
        {
            this.viewportWidth = viewportWidth;
            this.viewportHeight = viewportHeight;
        }

        // Set world bounds (call this after loading the map)
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
```
------------------------------------------------------------------------------------------------------------------------------------
## Classe EndState

Classe que define o que aparece quando o jogador perde. Exibe o score e highscore e também dois butões, um para recomeçar e outro para sair do jogo.

```
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
```
------------------------------------------------------------------------------------------------------------------------------------
## Classe Game1

Classe que gere todo o funcionamento do jogo incluindo a mudança de states.A classe ativa também todos os sound effects.

```
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
```

```
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
```

```
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
```
------------------------------------------------------------------------------------------------------------------------------------
## Classe Mapa

Classe onde são criados e colocados todos os objetos que compõem o mapa do jogo.

```
for (int x = startX; x < endX; x += pisoWidth)
            {
                _objetos.Add(new Objeto{Texture = pisoTexture, Position = new Vector2(x, pisoY), CustomHitboxHeight = 10});
            }
```

```
LeftTree = new Objeto { Texture = treeTexture, Position = new Vector2(startX, treeY), CustomHitboxWidth = 24 };
            _objetos.Add(LeftTree);

            RightTree = new Objeto { Texture = treeTexture, Position = new Vector2(endX - treeWidth, treeY), CustomHitboxWidth = 24 };
            _objetos.Add(RightTree);

            Random rand = new Random();
            int minX = startX + treeWidth;
            int maxX = endX - 2 * treeWidth;
            int minDistance = treeWidth * 2; // Minimum distance between trees

            int randomX1 = rand.Next(minX, maxX);

            int randomX2;
            int attempts = 0;
            do
            {
                randomX2 = rand.Next(minX, maxX);
                attempts++;
            } while (Math.Abs(randomX2 - randomX1) < minDistance && attempts < 100);

            _objetos.Add(new Objeto { Texture = treeTexture, Position = new Vector2(randomX1, treeY) });
            _objetos.Add(new Objeto { Texture = treeTexture, Position = new Vector2(randomX2, treeY) });
```
------------------------------------------------------------------------------------------------------------------------------------
## Classe Objeto 

Classe utilizada para definir a hitbox de objetos utilizados no jogo.

```
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
```
------------------------------------------------------------------------------------------------------------------------------------
## Classe ScoreManager

Classe utilizada para guardar o score e calcular e armazenar o highscore do jogador.

```
static ScoreManager()
        {
            LoadHighScore();
        }

        public static int Score => _score;
        public static int HighScore => _highScore;

        public static void AddScore(int amount)
        {
            _score += amount;
            if (_score > _highScore)
            {
                _highScore = _score;
                SaveHighScore();
            }
        }

        public static void ResetScore()
        {
            _score = 0;
        }

        private static void LoadHighScore()
        {
            if (File.Exists(ScoreFile))
            {
                int.TryParse(File.ReadAllText(ScoreFile), out _highScore);
            }
        }

        private static void SaveHighScore()
        {
            File.WriteAllText(ScoreFile, _highScore.ToString());
        }
```
------------------------------------------------------------------------------------------------------------------------------------
## Observações/Melhorias
No momento o jogo está bastante funcional, mas podiamos adicionar mais algumas coisas e melhorias. Como por exemplo, um menú, com botões de play, ajuda para informações e sair, colocar inimigos especiais, melhorar a organização dos ficheiros e outrs ideias que temos em mente.
Contudo, estamos orgulhosos com o que fizemos e talvez adicionaremos as melhorias e ideias.
