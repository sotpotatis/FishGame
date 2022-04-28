using System;
using System.Collections.Generic;
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
            int defaultSpeed
        )
        {
            Rarity = rarity;
            Value = value;
            DefaultSpeed = defaultSpeed;
            Name = name;
            AssociatedAssetName = associatedAssetName;
        }

        // Representerar data om en fisk i spelet.
        private int Rarity { get; set; } // Hur sällsynt fisken är
        public int Value { get; set; } // Hur mycket fisken är värd i spelvaluta
        public string Name { get; } // Vad fisken heter
        public string AssociatedAssetName { get; } // Namnet på bilden som hör till fisken

        public int DefaultSpeed { get; set; } // Fiskens standardhastighet
    }
}
