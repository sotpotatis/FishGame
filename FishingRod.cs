﻿using Microsoft.Xna.Framework;
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
            string associatedAssetName,
            ContentManager contentLoader
        )
        {
            Position = position;
            Speed = speed;
            SpeedX = speedX; // Hastighet i X-led.
            AssociatedAsset = contentLoader.Load<Texture2D>(associatedAssetName);
            IsFish = false;
            HasBeenCollidedWith = false;
        }

        public int SpeedX { get; }
        public Fish CollidedFish { get; set; }

        /// <summary>
        /// Hämtar nästa position som fiskespöet ska befinna sig på. Fiskespöet ska fungera lite annorlunda
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public override Vector2 getNextPos(int screenWidth, int screenHeight, float depth, float fishingRodCatchingSpeed)
        {
            if (HasBeenCollidedWith)
            { // Vi vill att fiskespöet ska röra sig uppåt tillsammans med fisken när det har fångat saker
                return new Vector2(CollidedFish.Position.X, CollidedFish.Position.Y);
            }
            else
            {
                return Position; // ...annars förlitar vi oss på andra delar i koden som hanterar positionen :)
            }
        }
    }
}
