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
        public PowerUpData Data { get; set; } // Data om powerupen
        public bool IsActivated { get; private set; } // Om powerupen är aktiverad eller inte
        public double PowerUpActivatedAt { get; private set; } // När powerupen aktiverades

        public double PreviousValue { get; private set; } // Det tidigare värdet som det som powerupen är kopplad till hade innan powerupen aktiverades.

        public PowerUp(
            Vector2 position,
            PowerUpData data,
            Texture2D associatedAsset,
            bool hasBeenCollidedWith = false
        )
        {
            // Funktion för att initiera en powerup som flyttar på sig.
            Data = data;
            Position = position;
            Speed = data.Speed;
            IsFish = false;
            AssociatedAsset = associatedAsset;
            HasBeenCollidedWith = hasBeenCollidedWith;
            IsActivated = false;
            PowerUpActivatedAt = 0;
        }

        /// <summary>
        /// Kontrollerar om powerupen fortfarande är aktiv eller inte.
        /// </summary>
        /// <param name="currentGameTimeSeconds">Den aktuella tiden som har passerat sedan spelet startades</param>
        /// <returns></returns>
        public bool CheckPowerUpStillActive(double currentGameTimeSeconds)
        {
            if (currentGameTimeSeconds - PowerUpActivatedAt > Data.ActiveFor)
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
        /// Hämtar en text som berättar hur lång tid det är kvar tills en powerup avaktiveras.
        /// </summary>
        /// <param name="currentGameTimeSeconds">Den aktuella speltiden, dvs. hur många sekunder av spelet som har passerat.</param>
        /// <returns></returns>
        public string GetActiveUntilText(double currentGameTimeSeconds)
        {
            int activeMinutesRemaining = 0;
            int secondsDifference = (int)Math.Floor(
                PowerUpActivatedAt + Data.ActiveFor - currentGameTimeSeconds
            ); // Hämta sekunder kvar tills powerupen avaktiveras.
            int activeSecondsRemaining = secondsDifference;
            if (secondsDifference > 60) // Om det är mer än 60 sekunder tills powerupen inaktiveras --> ändra texten
            {
                activeMinutesRemaining = (int)Math.Floor(currentGameTimeSeconds / 60);
                activeSecondsRemaining = activeSecondsRemaining - activeMinutesRemaining * 60;
            }
            return $"{activeMinutesRemaining.ToString().PadLeft(2, '0')}:{activeSecondsRemaining.ToString().PadLeft(2, '0')}";
        }

        /// <summary>
        /// Aktiverar en powerup om det går att aktivera den.
        /// </summary>
        /// <param name="currentGameTimeSeconds">Den aktuella tiden som har passerat sedan spelet startades</param>
        /// <returns></returns>
        public void activatePowerUp(double currentGameTimeSeconds)
        {
            if (!IsActivated)
            {
                IsActivated = true;
                PowerUpActivatedAt = currentGameTimeSeconds;
            }
        }

        /// <summary>
        /// Sätter variabeln PreviousValue.
        /// </summary>
        /// <param name="previousValue">Värdet.</param>
        public void savePreviousValue(double previousValue)
        {
            PreviousValue = previousValue;
        }
    }
}
