using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishGame
{
    class PowerUp : MovingItem
    {
        // Representerar en powerup. Till skillnad från PowerupData så innehåller även denna klass de variabler som krävs för att personligt identifiera powerups.
        public PowerUpData Data { get; set; } // Data om skräpet
        public bool IsActivated { get; private set; } // Om powerupen är aktiverad eller inte
        public int PowerUpActivatedAt { get; private set; } // När powerupen aktiverades

        public PowerUp(
            Vector2 position,
            PowerUpData data,
            int speed,
            ContentManager contentLoader,
            bool hasBeenCollidedWith = false
        )
        {
            // Funktion för att initiera en powerup som flyttar på sig.
            Data = data;
            Position = position;
            Speed = speed;
            IsFish = false;
            AssociatedAsset = contentLoader.Load<Texture2D>(data.AssociatedAssetName);
            HasBeenCollidedWith = hasBeenCollidedWith;
            IsActivated = false;
            PowerUpActivatedAt = 0;
        }
        
        /// <summary>
        /// Kontrollerar om powerupen fortfarande är aktiv eller inte.
        /// </summary>
        /// <param name="currentGameTimeSeconds">Den aktuella tiden som har passerat sedan spelet startades</param>
        /// <returns></returns>
        public bool CheckPowerUpStillActive(int currentGameTimeSeconds)
        {
            if (currentGameTimeSeconds - PowerUpActivatedAt >  Data.ActiveFor)
            {
                IsActivated = false;
            }
            else
            {
                IsActivated = true;
            }
            return IsActivated;
        }

        /// <summary>
        /// Aktiverar en powerup om det går att aktivera den.
        /// </summary>
        /// <param name="currentGameTimeSeconds">Den aktuella tiden som har passerat sedan spelet startades</param>
        /// <returns></returns>
        public void activatePowerUp(int currentGameTimeSeconds)
        {
            if (!IsActivated)
            {
                IsActivated = true;
                PowerUpActivatedAt = currentGameTimeSeconds;
            }
        }
    }
}
