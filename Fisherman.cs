using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishGame
{
    class Fisherman
    {
        // Representerar fiskaren i spelet.
        public Texture2D Asset { get; set; } // Aktuell bild för fiskaren
        public Vector2 Position { get; set; } // Aktuell position för fiskaren
    }
}
