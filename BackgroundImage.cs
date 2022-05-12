using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishGame
{
    class BackgroundImage : MovingItem
    {
        // Representerar ett bakgrundsföremål.
        public BackgroundImage(
            Vector2 position,
            ContentManager contentLoader,
            BackgroundImageData data
        )
        {
            Position = position;
            Speed = data.DefaultSpeed;
            IsFish = true;
            AssociatedAsset = contentLoader.Load<Texture2D>(data.AssociatedAssetName);
            HasBeenCollidedWith = false;
        }
    }
}
