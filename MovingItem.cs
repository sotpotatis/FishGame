using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace FishGame
{
    class MovingItem
    {
        // Representerar ett objekt som genenerellt rör sig på skärmen
        public Vector2 Position { get; set; } //Position på skärmen
        public float Speed { get; set; } // Hastighet
        public bool HasBeenCollidedWith { get; set; } // Om föremålet har blivit krockad med eller inte
        public bool IsFish { get; set; } // Om föremålet är en fisk eller inte

        private float _lastDepth { get; set; } = 0; // Det senaste djupet som föremålet befann sig på. Detta används för att hämta nästa position.
        public Texture2D AssociatedAsset { get; set; } // Texturen som associeras med objektet som rör sig
        public virtual Vector2 getNextPos(int screenHeight, int screenWidth, float depth, float fishingRodCatchingSpeed)
        {
            // Funktion för att hantera ny position, bl.a. vid en kollision. Returnerar den nästa positionen som objektet ska ha.
            // (om en vektor med negativa koordinater returneras så ska objektet tas bort)
            Debug.WriteLine(
                $"Kontrollerar position för objekt som har positionen {Position.X},{Position.Y} (kolliderad med: {HasBeenCollidedWith}, hastighet {Speed})."
            );
            if (!HasBeenCollidedWith)
            { // Inte kolliderad med - fortsätt röra sig tills vi når kanten
                if (Position.X - AssociatedAsset.Width <= 0)
                { // Om vi har nått kanten/gränsen på skärmen
                    Debug.WriteLine("X-gräns har nåtts");
                    Position = new Vector2(-1, -1); // (koden kommer hantera den negativa positionen och radera fisken)
                }
                else
                { // Om vi inte har nått kanten på skärmen
                    Debug.WriteLine("X-gräns har inte nåtts");
                    Position = new Vector2(Position.X - Speed, Position.Y-(depth-_lastDepth));
                }
            }
            else
            { // Koliderad med - fortsätt röra sig uppåt tills vi når toppen av skärmen
                if (Position.Y - AssociatedAsset.Width <= 0)
                { // Om vi har nått kanten/gränsen på skärmen
                    Debug.WriteLine("Y-gräns har nåtts");
                    Position = new Vector2(-1, -1);
                }
                else
                { // Om vi har nått kanten/gränsen på skärmen
                    Debug.WriteLine("Y-gräns har inte nåtts");
                    Position = new Vector2(Position.X, Position.Y - fishingRodCatchingSpeed);
                }
            }
            _lastDepth = depth;
            return Position;
        }
        /// <summary>
        /// Hämtar en rektangel för bildobjektet kopplat till föremålet. Kan med fördel användas till kollisionshantering etc.
        /// </summary>
        /// <returns></returns>
        public Rectangle getAssociatedRectangle()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, AssociatedAsset.Width, AssociatedAsset.Height); // Returnera en rektangel
        }
    }
}
