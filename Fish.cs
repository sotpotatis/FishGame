using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishGame
{
    class Fish : MovingItem
    {
        // Representerar en fisk. Till skillnad från FishData så innehåller även denna klass de variabler som krävs för att personligt identifiera en fisk.
        public FishData Data { get; set; } // Data om fisken

        public Fish(
            Vector2 position,
            FishData data,
            Texture2D associatedAsset,
            bool hasBeenCollidedWith = false
        )
        {
            // Funktion för att initiera en fisk föremål som flyttar på sig.
            Data = data;
            Position = position;
            Speed = data.DefaultSpeed;
            IsFish = true;
            AssociatedAsset = associatedAsset;
            HasBeenCollidedWith = hasBeenCollidedWith;
        }
    }
}
