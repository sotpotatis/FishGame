using System;
using System.Collections.Generic;
using System.Text;

namespace FishGame
{
    class PowerUpData
    {
        public PowerUpData(
            string name,
            string associatedAssetName,
            int rarity,
            int activeFor,
            PowerUpTypes type,
            double multiplier,
            int minDepth = 0
        )
        {
            Name = name;
            AssociatedAssetName = associatedAssetName;
            Rarity = rarity;
            ActiveFor = activeFor;
            MinDepth = minDepth;
            Multiplier = multiplier;
            Type = type;
        }

        // Innehåller data relaterat till en powerup.
        public string Name { get; }
        public string AssociatedAssetName { get; }
        public int Rarity { get; } // Hur sällan denna powerup spawnas
        public int ActiveFor { get; } // Hur länge denna powerup är aktiv
        public int MinDepth { get; } // Det minsta djupet som powerupen spawnas på.
        public double Multiplier { get; private set; } // Vad värden ska multipliceras med när powerupen är aktiv

        // KORT OM POWERUP-TYPER:
        // -Sömnpiller: får fiskarna att åka långsammare
        // -Healer: gör så att ditt fiskespö
        // -Dykare: ger dig möjlighet att komma ner på ett lägre djup under en viss tid
        // -Stimmultiplikator: gör så att fler fiskar "spawnas" på skärmen
        // -Pengamultiplikator: gör så att du får fler poäng per sekund
        public enum PowerUpTypes
        {
            SleepPill,
            Healer,
            Diver,
            FishMultiplier,
            ScoreMultiplier
        }

        public PowerUpTypes Type { get; set; }
    }
}
