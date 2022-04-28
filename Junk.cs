using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishGame
{
    class Junk : MovingItem
    {
        // Representerar skräp. Till skillnad från JunkData så innehåller även denna klass de variabler som krävs för att personligt identifiera skräp.
        public JunkData Data { get; set; } // Data om skräpet

        public Junk(
            Vector2 position,
            JunkData data,
            int speed,
            ContentManager contentLoader,
            bool hasBeenCollidedWith = false
        )
        {
            // Funktion för att initiera ett skräpföremål som flyttar på sig.
            Data = data;
            Position = position;
            Speed = speed;
            IsFish = false;
            AssociatedAsset = contentLoader.Load<Texture2D>(data.AssociatedAssetName);
            HasBeenCollidedWith = hasBeenCollidedWith;
        }
    }
}
