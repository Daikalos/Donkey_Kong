﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Donkey_Kong
{
    class EnemyManager
    {
        private List<Enemy> myEnemies;
        private int myMaxEnemies;
        private float
            mySpawnTimer,
            mySpawnTimerMax,
            myEnemySpeed;

        public EnemyManager(float aSpawnTimerMax, int someMaxEnemies)
        {
            this.mySpawnTimerMax = aSpawnTimerMax;
            this.myMaxEnemies = someMaxEnemies;

            mySpawnTimer = 0;
            myEnemies = new List<Enemy>();
        }

        public void Update(GameWindow aWindow, GameTime aGameTime, Random aRNG, Level aLevel, Player aPlayer)
        {
            if (myEnemies.Count < myMaxEnemies)
            {
                mySpawnTimer += (float)aGameTime.ElapsedGameTime.TotalSeconds;
            }
            if (mySpawnTimer >= mySpawnTimerMax)
            {
                mySpawnTimer = 0;

                AddEnemy(aWindow, aRNG, aPlayer);
            }

            for (int i = myEnemies.Count; i > 0; i--)
            {
                myEnemies[i - 1].Update(aGameTime, aRNG, aLevel);
                if (!myEnemies[i - 1].IsAlive)
                {
                    myEnemies.RemoveAt(i - 1);
                }
            }
        }

        public void Draw(SpriteBatch aSpriteBatch)
        {
            for (int i = myEnemies.Count; i > 0; i--)
            {
                myEnemies[i - 1].Draw(aSpriteBatch);
            }
        }

        public void AddEnemy(GameWindow aWindow, Random aRNG, Player aPlayer)
        {
            Vector2 tempSpawnPos = Vector2.Zero;
            float tempSpeed = aRNG.Next(100, 140);

            tempSpawnPos = new Vector2((aWindow.ClientBounds.Width / 2) - 40, aWindow.ClientBounds.Height - (120 * (aRNG.Next(0, 4) + 1) + 60));
            if (Vector2.Distance(tempSpawnPos, aPlayer.Position) < 200.0f)
            {
                AddEnemy(aWindow, aRNG, aPlayer);
            }

            Enemy tempEnemy = new Enemy(tempSpawnPos, new Point(40), tempSpeed, 1.5f);
            tempEnemy.SetTexture("Enemy");

            myEnemies.Add(tempEnemy);
        }
    }
}
