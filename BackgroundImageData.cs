using System;
using System.Collections.Generic;
using System.Text;

namespace FishGame
{
    class BackgroundImageData
    {
        public BackgroundImageData(
            string name,
            string associatedAssetName,
            int rarity,
            float defaultSpeed,
            int minDepth = 0,
            int? maxDepth = null
        )
        {
            Name = name;
            AssociatedAssetName = associatedAssetName;
            Rarity = rarity;
            MinDepth = minDepth;
            MaxDepth = maxDepth;
            DefaultSpeed = defaultSpeed;
        }

        // Innehåller data relaterat till en bakgrundsbild. Den rör sig i bakgrunden så det är inte så mycket man behöver ha koll på.
        public string Name { get; } // Föremålets läsbara namn
        public string AssociatedAssetName { get; } // Namnet på föremålets associerade bild
        public int MinDepth { get; } // Det minsta djupet som bilden spawnas på.
        public int? MaxDepth { get; } // Det maximala djupet som bilden spawnas på.
        public float DefaultSpeed { get; } // Standardhastigheten som föremålet har
        public double Rarity { get; private set; } // Hur sällan denna bild ritas ut.

        public bool IsAvailableAt(double depth)
        {
            // Vissa bilder finns bara tillgängliga vid ett visst djup. Denna funktion returnerar om bakgrunden finns tillgänglig vid djupet eller inte
            if (depth >= MinDepth && (MaxDepth == null || depth <= MaxDepth))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
