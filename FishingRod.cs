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
    }
}
