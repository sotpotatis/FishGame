using System;
using System.Collections.Generic;
using System.Text;

namespace FishGame
{
    struct PowerUpData
    {
        public PowerUpData(string name, string associatedAssetName, int rarity, int activeFor, int minDepth=0)
        {
            Name = name;
            AssociatedAssetName = associatedAssetName;
            Rarity = rarity;
            ActiveFor = activeFor;
            MinDepth = minDepth; 
        }

        // Innehåller data relaterat till en powerup.
        public string Name { get; }
        public string AssociatedAssetName { get; }
        public int Rarity { get; } // Hur sällan denna powerup spawnas
        public int ActiveFor { get; } // Hur länge denna powerup är aktiv
        public int MinDepth { get; } // Det minsta djupet som powerupen spawnas på.
    }
}
