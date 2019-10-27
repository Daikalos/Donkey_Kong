﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace Donkey_Kong
{
    class Player
    {
        private enum PlayerState
        {
            isWalking,
            isClimbing,
            isJumping,
            isFalling,
            isDead
        }

        private Texture2D
            myTexture,
            myMarioHPTexture;
        private Animation
            myWalkingAnimation,
            myDeathAnimation;

        private Vector2
            myPosition,
            myDirection,
            myDestination;
        private Point mySize;
        private Rectangle myBoundingBox;
        private Color myPlayerColor; //For invincibility
        private PlayerState myPlayerState;
        private SpriteEffects myFlipSprite;

        private int myHealth;
        private float
            mySpeed,
            myClimbSpeed,
            myVelocity,
            myGravity,
            myJumpHeight,
            myInvincibilityFrames;
        private bool
            myIsMoving,
            myJumpAvailable,
            myIsDead;

        public Texture2D MarioHPTexture
        {
            get => myMarioHPTexture;
        }
        public Vector2 Position
        {
            get => myPosition;
        }
        public Rectangle BoundingBox
        {
            get => myBoundingBox;
            set => myBoundingBox = value;
        }
        public int Health
        {
            get => myHealth;
        }
        public bool IsDead
        {
            get => myIsDead;
        }

        public Player(Vector2 aPosition, Point aSize, int aHealth, float aSpeed, float aClimbSpeed, float aGravity, float aJumpHeight)
        {
            this.myPosition = aPosition;
            this.mySize = aSize;
            this.myHealth = aHealth;
            this.mySpeed = aSpeed;
            this.myClimbSpeed = aClimbSpeed;
            this.myGravity = aGravity;
            this.myJumpHeight = aJumpHeight;

            this.myIsMoving = false;
            this.myJumpAvailable = true;
            this.myDeathAnimation = new Animation(new Point(5, 2), 0.3f, false, true);
            this.myWalkingAnimation = new Animation(new Point(3, 1), 0.035f, true, false);
            this.myPlayerState = PlayerState.isFalling;
            this.myDestination = new Vector2(myPosition.X + mySize.X / 2, myPosition.Y);
            this.myDirection = Vector2.Zero;
            this.myPlayerColor = Color.White;
            this.myMarioHPTexture = ResourceManager.RequestTexture("Mario_Lives");
            this.myBoundingBox = new Rectangle((int)myPosition.X, (int)myPosition.Y, mySize.X, mySize.Y);
        }

        public void Update(GameWindow aWindow, GameTime aGameTime)
        {
            myBoundingBox = new Rectangle((int)myPosition.X, (int)myPosition.Y, mySize.X, mySize.Y);

            switch (myPlayerState)
            {
                case PlayerState.isWalking:
                    Movement(aWindow, aGameTime);
                    break;
                case PlayerState.isClimbing:
                    Climbing(aGameTime);
                    break;
                case PlayerState.isJumping:
                    Jumping(aGameTime);
                    break;
                case PlayerState.isFalling:
                    myVelocity += myGravity;
                    myPosition.Y += myVelocity * (float)aGameTime.ElapsedGameTime.TotalSeconds;
                    break;
                case PlayerState.isDead:

                    break;
            }

            Invincibility(aGameTime);
            CollisionChecking(aGameTime);
            CheckIsDead();
        }

        public void Draw(SpriteBatch aSpriteBatch, GameTime aGameTime)
        {
            switch (myPlayerState)
            {
                case PlayerState.isWalking:
                    if (myIsMoving)
                    {
                        myWalkingAnimation.DrawSpriteSheet(aSpriteBatch, aGameTime, myTexture, myPosition,
                            new Point(myTexture.Width / 3, myTexture.Height), mySize, myPlayerColor, myFlipSprite);
                    }
                    else
                    {
                        aSpriteBatch.Draw(myTexture, myBoundingBox,
                            new Rectangle(myTexture.Width / 3, 0, myTexture.Width / 3, myTexture.Height), myPlayerColor, 0.0f, Vector2.Zero, myFlipSprite, 0.0f);
                    }
                    break;
                case PlayerState.isClimbing:
                    aSpriteBatch.Draw(myTexture, myBoundingBox, null, myPlayerColor, 0.0f, Vector2.Zero, myFlipSprite, 0.0f);
                    break;
                case PlayerState.isJumping:
                    aSpriteBatch.Draw(myTexture, myBoundingBox, null, myPlayerColor, 0.0f, Vector2.Zero, myFlipSprite, 0.0f);
                    break;
                case PlayerState.isFalling:
                    aSpriteBatch.Draw(myTexture, myBoundingBox, null, myPlayerColor, 0.0f, Vector2.Zero, myFlipSprite, 0.0f);
                    break;
                case PlayerState.isDead:
                    myDeathAnimation.DrawSpriteSheet(aSpriteBatch, aGameTime, myTexture, myPosition,
                        new Point(myTexture.Width / 5, myTexture.Height / 2), mySize, Color.White, SpriteEffects.None);
                    break;
            }
        }

        private void Movement(GameWindow aWindow, GameTime aGameTime)
        {
            if (KeyMouseReader.KeyHold(Keys.Left) && OutsideBounds(aWindow) != -1)
            {
                myDestination = Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X - Level.TileSize.X, myBoundingBox.Center.Y)).BoundingBox.Center.ToVector2();

                myFlipSprite = SpriteEffects.FlipHorizontally;
                myIsMoving = true;
            }
            if (KeyMouseReader.KeyHold(Keys.Right) && OutsideBounds(aWindow) != 1)
            {
                myDestination = Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X + Level.TileSize.X, myBoundingBox.Center.Y)).BoundingBox.Center.ToVector2();

                myFlipSprite = SpriteEffects.None;
                myIsMoving = true;
            }

            if (Math.Abs(myBoundingBox.Center.ToVector2().X - myDestination.X) > 1.0f)
            {
                myDirection.X = myDestination.X - myBoundingBox.Center.ToVector2().X;
                myDirection.Normalize();

                myPosition.X += myDirection.X * mySpeed * (float)aGameTime.ElapsedGameTime.TotalSeconds;

                ResourceManager.StopSound("Jump");
                ResourceManager.PlaySound("Walking");
            }
            else
            {
                myPosition.X = myDestination.X - mySize.X / 2;
                myIsMoving = false;

                ResourceManager.StopSound("Walking");
            }

            if (KeyMouseReader.KeyPressed(Keys.Space) && myJumpAvailable && Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X, myBoundingBox.Center.Y + Level.TileSize.Y)).TileType != '.')
            {
                myPlayerState = PlayerState.isJumping;
                myVelocity = myJumpHeight;
                myJumpAvailable = false;

                ResourceManager.StopSound("Walking");
                ResourceManager.PlaySound("Jump");

                if (myIsMoving)
                {
                    for (int i = 4; i > 0; i--)
                    {
                        if (myFlipSprite == SpriteEffects.None)
                        {
                            Tile tempTile = Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X + (Level.TileSize.X * (i - 1)), myBoundingBox.Center.Y + Level.TileSize.Y));
                            if (tempTile.TileType == '#' || tempTile.TileType == '%')
                            {
                                myDestination = tempTile.BoundingBox.Center.ToVector2();
                                break;
                            }
                        }
                        else
                        {
                            Tile tempTile = Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X - (Level.TileSize.X * (i - 1)), myBoundingBox.Center.Y + Level.TileSize.Y));
                            if (tempTile.TileType == '#' || tempTile.TileType == '%')
                            {
                                myDestination = tempTile.BoundingBox.Center.ToVector2();
                                break;
                            }
                        }
                    }
                }

                SetTexture("Mario_Jumping");
            }
        }
        private void Climbing(GameTime aGameTime)
        {
            if (KeyMouseReader.KeyHold(Keys.Up))
            {
                myDestination = Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X, myBoundingBox.Center.Y - Level.TileSize.Y)).BoundingBox.Center.ToVector2();
            }
            if (KeyMouseReader.KeyHold(Keys.Down) && Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X, myBoundingBox.Center.Y + 40)).TileType != '#')
            {
                myDestination = Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X, myBoundingBox.Center.Y + Level.TileSize.Y)).BoundingBox.Center.ToVector2();
            }

            if (Math.Abs(myBoundingBox.Center.ToVector2().Y - myDestination.Y) > 1.0f)
            {
                myDirection.Y = myDestination.Y - myBoundingBox.Center.ToVector2().Y;
                myDirection.Normalize();

                myPosition.Y += myDirection.Y * myClimbSpeed * (float)aGameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                myPosition.Y = myDestination.Y - mySize.Y / 2;
            }
        }
        private void Jumping(GameTime aGameTime)
        {
            myVelocity += myGravity;
            myPosition.Y += myVelocity * (float)aGameTime.ElapsedGameTime.TotalSeconds;

            if (Math.Abs(myBoundingBox.Center.ToVector2().X - myDestination.X) > 1.0f)
            {
                myDirection.X = myDestination.X - myBoundingBox.Center.ToVector2().X;
                myDirection.Normalize();

                myPosition.X += myDirection.X * mySpeed * (float)aGameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                myPosition.X = myDestination.X - mySize.X / 2;
            }
        }
        private void Invincibility(GameTime aGameTime)
        {
            if (myInvincibilityFrames > 0)
            {
                myInvincibilityFrames -= (float)aGameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                myInvincibilityFrames = 0;
                myPlayerColor = Color.White;
            }
        }
        private void CheckIsDead()
        {
            if (myHealth <= 0 || GameInfo.BonusScore <= 0)
            {
                if (!myDeathAnimation.AtLastFrame)
                {
                    ResourceManager.PlaySound("Death");
                }
                if (ResourceManager.RequestSoundEffect("Death").State == SoundState.Stopped)
                {
                    myIsDead = true;
                }

                myPlayerState = PlayerState.isDead;
                SetTexture("Mario_Death");
            }
        }
        public void TakeDamage()
        {
            if (myInvincibilityFrames <= 0)
            {
                myHealth--;
                myPlayerColor = new Color(0, 60, 120, 210.0f);
                myInvincibilityFrames = 2.0f;
            }
        }
        public void LevelFinished()
        {
            SetTexture("Mario_Walking");
            myFlipSprite = SpriteEffects.FlipHorizontally;
            myPlayerColor = Color.White;
            myPlayerState = PlayerState.isWalking;
            myIsMoving = false;
        }

        private void CollisionChecking(GameTime aGameTime)
        {
            IsFalling();
            CollisionBlock(aGameTime);
            CollisionLadder();
            CollisionPin();
            CollisionItem();
            CollisionEnemy();
        }
        private void IsFalling()
        {
            Tile tempTile = Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X, myBoundingBox.Center.Y + Level.TileSize.Y));
            if (Math.Abs(myBoundingBox.Center.X - tempTile.BoundingBox.Center.X) <= 1.0f && myPlayerState != PlayerState.isJumping)
            {
                if (tempTile.TileType == '.')
                {
                    myPosition.X = tempTile.Position.X;

                    myPlayerState = PlayerState.isFalling;
                    SetTexture("Mario_Jumping");

                    ResourceManager.StopSound("Walking");
                }
            }
        }
        private void CollisionBlock(GameTime aGameTime)
        {
            Tile tempTile = Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X, myBoundingBox.Center.Y + Level.TileSize.Y));
            if (myPlayerState != PlayerState.isClimbing && myPosition.Y + (myVelocity * (float)aGameTime.ElapsedGameTime.TotalSeconds) >= tempTile.BoundingBox.Y - mySize.Y)
            {
                if (tempTile.TileType == '#' || tempTile.TileType == '%')
                {
                    myVelocity = 0.0f;

                    myPosition.Y = tempTile.BoundingBox.Y - mySize.Y;
                    myJumpAvailable = true;

                    myPlayerState = PlayerState.isWalking;
                    SetTexture("Mario_Walking");
                }
            }
        }
        private void CollisionLadder()
        {
            if (!myIsMoving)
            {
                if (myPlayerState != PlayerState.isClimbing)
                {
                    if (Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X, myBoundingBox.Center.Y)).TileType == '@')
                    {
                        if (KeyMouseReader.KeyHold(Keys.Up))
                        {
                            myPlayerState = PlayerState.isClimbing;
                            SetTexture("Mario_Jumping");
                        }
                    }
                    if (Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X, myBoundingBox.Center.Y + Level.TileSize.Y)).TileType == '%')
                    {
                        if (KeyMouseReader.KeyHold(Keys.Down))
                        {
                            myPlayerState = PlayerState.isClimbing;
                            SetTexture("Mario_Jumping");
                        }
                    }
                }
                else
                {
                    if (Math.Abs(myBoundingBox.Center.ToVector2().Y - myDestination.Y) < 1.0f)
                    {
                        Tile tempTile = Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X, myBoundingBox.Center.Y + 40));
                        if (tempTile.TileType == '%' || tempTile.TileType == '#')
                        {
                            myPosition.Y = Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X, myBoundingBox.Center.Y + 40)).BoundingBox.Y - myBoundingBox.Height;
                            myJumpAvailable = true;

                            myPlayerState = PlayerState.isWalking;
                            SetTexture("Mario_Walking");
                        }
                    }
                }
            }
        }
        private void CollisionPin()
        {
            for (int i = 1; i <= 2; i++)
            {
                Tile tempTile = Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X, myBoundingBox.Center.Y + Level.TileSize.Y * i));
                if (tempTile.TileType == '?')
                {
                    tempTile.TileType = '.';
                    tempTile.SetTexture();

                    ResourceManager.PlaySound("Item_Get");
                    GameInfo.AddScore(tempTile.BoundingBox.Center.ToVector2(), 100);
                }
            }
        }
        private void CollisionItem()
        {
            Tile tempTile = Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X, myBoundingBox.Center.Y));
            if (tempTile.TileType == '/')
            {
                tempTile.TileType = '.';
                tempTile.SetTexture();

                ResourceManager.PlaySound("Item_Get");
                GameInfo.AddScore(tempTile.BoundingBox.Center.ToVector2(), 100);
            }
        }
        private void CollisionEnemy()
        {
            for (int i = EnemyManager.Enemies.Count; i > 0; i--)
            {
                if (Vector2.Distance(EnemyManager.Enemies[i - 1].BoundingBox.Center.ToVector2(), myBoundingBox.Center.ToVector2()) < 25.0f)
                {
                    TakeDamage();
                }
            }
        }
        private int OutsideBounds(GameWindow aWindow)
        {
            if (Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X - Level.TileSize.X, myBoundingBox.Center.Y)).Position == Vector2.Zero) //Will return myTiles[0, 0] position if outside grid
            {
                myDestination = Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X, myBoundingBox.Center.Y - Level.TileSize.Y)).BoundingBox.Center.ToVector2();
                return -1;
            }
            if (Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X + Level.TileSize.X, myBoundingBox.Center.Y)).Position == Vector2.Zero)
            {
                myDestination = Level.GetTileAtPos(new Vector2(myBoundingBox.Center.X, myBoundingBox.Center.Y - Level.TileSize.Y)).BoundingBox.Center.ToVector2();
                return 1;
            }
            if (myPosition.Y > aWindow.ClientBounds.Height) //Safety measure
            {
                myPosition = new Vector2(aWindow.ClientBounds.Width / 6, aWindow.ClientBounds.Height - 60);
                myPlayerState = PlayerState.isFalling;
            }
            return 0;
        }

        public void SetTexture(string aTextureName)
        {
            myTexture = ResourceManager.RequestTexture(aTextureName);
        }
        public void SetMarioHPTexture()
        {
            myMarioHPTexture = ResourceManager.RequestTexture("Mario_Lives");
        }
    }
}