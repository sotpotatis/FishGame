using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        public bool HasBeenCollidedWith { get; protected set; } // Om föremålet har blivit krockat med eller inte
        public bool IsFish { get; protected set; } // Om föremålet är en fisk eller inte

        private float _lastDepth { get; set; } = 0; // Det senaste djupet som föremålet befann sig på. Detta används för att hämta nästa position.
        public Texture2D AssociatedAsset { get; set; } // Texturen som associeras med objektet som rör sig

        public virtual Vector2 getNextPos(
            int screenHeight,
            int screenWidth,
            float depth,
            float fishingRodCatchingSpeed
        )
        {
            // Funktion för att hantera ny position, bl.a. vid en kollision. Returnerar den nästa positionen som objektet ska ha.
            // (om en vektor med negativa koordinater returneras så ska objektet tas bort)
            Debug.WriteLine(
                $"Kontrollerar position för objekt som har positionen {Position.X},{Position.Y} (kolliderad med: {HasBeenCollidedWith}, hastighet {Speed})."
            );
            if (!HasBeenCollidedWith)
            { // Inte kolliderad med - fortsätt röra sig tills vi når kanten
                if (Position.X + AssociatedAsset.Width <= 0)
                { // Om vi har nått kanten/gränsen på skärmen
                    Debug.WriteLine("X-gräns har nåtts");
                    Position = new Vector2(-1, -1); // (koden kommer hantera den negativa positionen och radera föremålet)
                }
                else
                { // Om vi inte har nått kanten på skärmen
                    Debug.WriteLine("X-gräns har inte nåtts");
                    Position = new Vector2(Position.X - Speed, Position.Y - (depth - _lastDepth));
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
            return new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                AssociatedAsset.Width,
                AssociatedAsset.Height
            ); // Returnera en rektangel runt föremålet
        }

        /// <summary>
        /// Markerar det som att man har föremålet har kolliderats med.
        /// </summary>
        /// <returns></returns>
        public void MarkAsCollidedWith()
        {
            if (!HasBeenCollidedWith)
            {
                HasBeenCollidedWith = true;
            }
        }

        /// <summary>
        /// Markerar det som att man har föremålet har kolliderats med.
        /// </summary>
        /// <returns></returns>
        public void MarkAsNotCollidedWith()
        {
            if (HasBeenCollidedWith)
            {
                HasBeenCollidedWith = false;
            }
        }

        public void CheckAndHandleCollisionWithFishingRod(
            FishingRod fishingRod,
            float depth,
            GraphicsDeviceManager _graphics,
            KeyboardState keyboardState
        )
        {
            Debug.WriteLine(
                $"Kriterier: {!fishingRod.HasBeenCollidedWith}, {keyboardState.IsKeyDown(Keys.Space)}, {getAssociatedRectangle().Intersects(fishingRod.getAssociatedRectangle())}"
            );
            bool collidedThisTimeAround = false;
            if (
                !fishingRod.HasBeenCollidedWith
                && keyboardState.IsKeyDown(Keys.Space)
                && getAssociatedRectangle().Intersects(fishingRod.getAssociatedRectangle())
            ) // Tillåt endast kollisioner när fiskespöet är redo.
            {
                Debug.WriteLine("Vi har en kollison mellan en sak och ett metspö.");
                HasBeenCollidedWith = true; // Markera kollision för föremålet
                fishingRod.MarkAsCollidedWith(); // Markera kollision med fiskespöet
                fishingRod.collideWith(this); // Ställ in kolliderat föremål
                collidedThisTimeAround = true;
            }
            ;
            // Uppdatera plats för saken som kolliderat med fiskespöet
            Position = getNextPos(
                _graphics.PreferredBackBufferHeight,
                _graphics.PreferredBackBufferWidth,
                depth,
                fishingRod.Speed
            );
            if (Position.X == -1 && !collidedThisTimeAround) // Ny position har negativa koordindater - föremålet ska gömmas från skärmen
            {
                HasBeenCollidedWith = false;
            }
            // Uppdatera plats för fiskespöet
            fishingRod.Position = fishingRod.getNextPos(
                _graphics.PreferredBackBufferHeight,
                _graphics.PreferredBackBufferWidth,
                depth,
                fishingRod.Speed
            );
        }
    }
}
