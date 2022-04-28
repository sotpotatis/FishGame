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
        public const int FishingRodSpeed = 1; // Hastigheten på fiskespöet
        public Vector2 Position { get; set; } //Position på skärmen
        public float Speed { get; set; } // Hastighet
        public bool HasBeenCollidedWith { get; set; } // Om föremålet har blivit krockad med eller inte
        public bool IsFish { get; set; } // Om föremålet är en fisk eller inte

        public Texture2D AssociatedAsset { get; set; } // Texturen som associeras med objektet som rör sig

        public Vector2 getNextPos(int screenHeight, int screenWidth)
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
                    Position = new Vector2(-1, -1);
                    return Position;
                }
                else
                { // Om vi inte har nått kanten på skärmen
                    Debug.WriteLine("X-gräns har inte nåtts");
                    Position = new Vector2(Position.X - Speed, Position.Y);
                    return Position;
                }
            }
            else
            { // Koliderad med - fortsätt röra sig uppåt tills vi når toppen av skärmen
                if (Position.Y - AssociatedAsset.Width <= 0)
                { // Om vi har nått kanten/gränsen på skärmen
                    Debug.WriteLine("Y-gräns har nåtts");
                    Position = new Vector2(-1, -1);
                    return Position;
                }
                else
                { // Om vi har nått kanten/gränsen på skärmen
                    Debug.WriteLine("Y-gräns har inte nåtts");
                    Position = new Vector2(Position.X, Position.Y + FishingRodSpeed);
                    return Position;
                }
            }
        }
    }
}
