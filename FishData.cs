using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace FishGame
{
    struct FishData
    {
        public FishData(
            string name,
            string associatedAssetName,
            int rarity,
            int value,
            float defaultSpeed,
            int minSpawnDepth = 0,
            int? maxSpawnDepth = null
        )
        {
            Rarity = rarity;
            Value = value;
            DefaultSpeed = defaultSpeed;
            Name = name;
            AssociatedAssetName = associatedAssetName;
            MinSpawnDepth = minSpawnDepth;
            MaxSpawnDepth = maxSpawnDepth;
        }

        // Representerar data om en fisk i spelet.
        private int Rarity { get; set; } // Hur sällsynt fisken är
        public int Value { get; set; } // Hur mycket fisken är värd i spelvaluta
        public string Name { get; } // Vad fisken heter (läsbart för spelaren)
        public string AssociatedAssetName { get; } // Namnet på bilden som hör till fisken

        public float DefaultSpeed { get; set; } // Fiskens standardhastighet
        public float MinSpawnDepth { get; set; } // Det minsta djupet där fisken "spawnas" på.
        public float? MaxSpawnDepth { get; set; } // Maxdjupet där fisken "spawnas" på.

        public bool IsAvailableAt(double depth)
        {
            // Vissa fiskar finns bara tillgängliga vid ett visst djup. Denna funktion returnerar om fisken finns tillgänglig vid djupet eller inte
            if (depth >= MinSpawnDepth && (MaxSpawnDepth == null || depth <= MaxSpawnDepth))
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
