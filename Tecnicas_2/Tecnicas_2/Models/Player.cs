using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tecnicas_2.Models
{
    public class Player : Objeto
    {
        public Texture2D IdleTexture { get; set; }
        public Texture2D WalkTexture { get; set; }
        public Texture2D JumpTexture { get; set; }
        public Texture2D AttackTexture { get; set; }

        private int idleFrames = 1, walkFrames = 6, jumpFrames = 1, attackFrames = 8;
        private int currentFrame = 0;
        private float frameTimer = 0f, frameSpeed = 0.12f;

        private enum PlayerState { Idle, Walk, Jump, Attack }
        private PlayerState state = PlayerState.Idle, lastState = PlayerState.Idle;

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Speed { get; set; } = 400f;
        public float JumpVelocity { get; set; } = -500f;
        public bool IsOnGround { get; set; }
        public float Scale { get; set; } = 1f;
        private bool facingRight = true;

        private const float Gravity = 1200f;
        private const int GroundY = 668;
        private int spriteWidth, spriteHeight;

        private bool isAttacking = false;
        private float attackTimer = 0f;
        private const float AttackDuration = 0.96f;
        private const int AttackWidth = 60, AttackHeight = 40;

        public bool IsDead { get; private set; } = false;

        public Player(Vector2 startPosition, int spriteWidth, int spriteHeight)
        {
            Position = startPosition;
            Velocity = Vector2.Zero;
            IsOnGround = false;
            this.spriteWidth = spriteWidth;
            this.spriteHeight = spriteHeight;
        }

        public void Update(GameTime gameTime, Rectangle leftTreeHitbox, Rectangle rightTreeHitbox)
        {
            if (IsDead) return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var keyboard = Keyboard.GetState();

            if (!isAttacking && keyboard.IsKeyDown(Keys.J))
            {
                isAttacking = true;
                attackTimer = AttackDuration;
                state = PlayerState.Attack;
                currentFrame = 0;
                frameTimer = 0f;
            }

            if (isAttacking)
            {
                attackTimer -= dt;
                frameTimer += dt;
                if (frameTimer >= frameSpeed)
                {
                    frameTimer = 0f;
                    currentFrame = (currentFrame + 1) % attackFrames;
                }
                if (attackTimer <= 0f)
                {
                    isAttacking = false;
                    state = IsOnGround ? (Velocity.X != 0 ? PlayerState.Walk : PlayerState.Idle) : PlayerState.Jump;
                }
            }
            else
            {
                float move = 0;
                if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)) move -= 1;
                if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) move += 1;

                Velocity = new Vector2(move * Speed, Velocity.Y);

                if (move > 0) facingRight = true;
                else if (move < 0) facingRight = false;

                if (IsOnGround && (keyboard.IsKeyDown(Keys.Space) || keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up)))
                {
                    Velocity = new Vector2(Velocity.X, JumpVelocity);
                    IsOnGround = false;
                }

                if (!IsOnGround)
                    Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * dt);

                Rectangle newRect = new((int)(Position.X + Velocity.X * dt), (int)(Position.Y + Velocity.Y * dt), spriteWidth, spriteHeight);

                if (Velocity.X < 0 && newRect.Intersects(leftTreeHitbox))
                {
                    newRect.X = leftTreeHitbox.Right;
                    Velocity = new Vector2(0, Velocity.Y);
                }
                else if (Velocity.X > 0 && newRect.Intersects(rightTreeHitbox))
                {
                    newRect.X = rightTreeHitbox.Left - spriteWidth;
                    Velocity = new Vector2(0, Velocity.Y);
                }

                Position = new Vector2(newRect.X, newRect.Y);

                if (newRect.Bottom >= GroundY)
                {
                    Position = new Vector2(newRect.X, GroundY - spriteHeight);
                    Velocity = new Vector2(Velocity.X, 0);
                    IsOnGround = true;
                }
                else
                {
                    IsOnGround = false;
                }

                state = !IsOnGround ? PlayerState.Jump : (move != 0 ? PlayerState.Walk : PlayerState.Idle);
            }

            if (state != lastState)
            {
                currentFrame = 0;
                frameTimer = 0f;
                lastState = state;
            }

            if (!isAttacking)
            {
                frameTimer += dt;
                int frameCount = state switch
                {
                    PlayerState.Idle => idleFrames,
                    PlayerState.Walk => walkFrames,
                    PlayerState.Jump => jumpFrames,
                    _ => attackFrames
                };
                if (frameTimer >= frameSpeed)
                {
                    frameTimer = 0f;
                    currentFrame = (currentFrame + 1) % frameCount;
                }
                if (state == PlayerState.Jump)
                    currentFrame = 0;
            }
        }

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

        public Rectangle Rect => new((int)Position.X, (int)Position.Y, (int)(spriteWidth * Scale), (int)(spriteHeight * Scale));

        public void Die() => IsDead = true;
    }
}