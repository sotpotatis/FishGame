using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishGame
{
    class FishingRod : MovingItem
    {
        // Definierar ett fiskespö i spelet
        public FishingRod(
            Vector2 position,
            int speed,
            int speedX,
            int cooldown,
            string associatedAssetName,
            ContentManager contentLoader
        )
        {
            Position = position;
            Speed = speed;
            SpeedX = speedX; // Hastighet i X-led.
            Cooldown = cooldown;
            AssociatedAsset = contentLoader.Load<Texture2D>(associatedAssetName);
            IsFish = false;
            HasBeenCollidedWith = false;
        }

        public int SpeedX { get; }
        public MovingItem CollidedItem { get; private set; }

        /// <summary>
        /// Ställ in vad fiskespöet har kolliderats med
        /// </summary>
        /// <param name="collidedItem">Det som fiskespöet koliderat med.</param>
        public void collideWith(MovingItem collidedItem)
        {
            CollidedItem = collidedItem;
        }

        public int CollidedItemPoints { get; private set; } // Hur mycket "saken" som fisken kolliderar med är värd i poäng.
        public int Cooldown { get; set; }

        /// <summary>
        /// Hämtar nästa position som fiskespöet ska befinna sig på. Fiskespöet ska fungera lite annorlunda än exempelvis andra MovingItem
        /// </summary>
        /// <param name="depth">Djupet som fiskespöet just nu befinner sig på.</param>
        /// <returns>Fiskespöets nästa position</returns>
        public override Vector2 getNextPos(
            int screenWidth,
            int screenHeight,
            float depth,
            float fishingRodCatchingSpeed
        )
        {
            if (HasBeenCollidedWith)
            { // Vi vill att fiskespöet ska röra sig uppåt tillsammans med fisken när det har fångat saker
                return new Vector2(
                    (CollidedItem.Position.X + CollidedItem.AssociatedAsset.Width) / 2,
                    0 //(CollidedItem.Position.Y + CollidedItem.AssociatedAsset.Height) / 2
                );
            }
            else
            {
                return Position; // ...annars förlitar vi oss på andra delar i koden som hanterar positionen :)
            }
        }
    }
}
