using System;
using System.Collections.Generic;
using System.Text;

namespace FishGame
{
    struct JunkData
    {
        public JunkData(string name, string associatedAssetName, int rarity, int value)
        {
            Name = name;
            AssociatedAssetName = associatedAssetName;
            Rarity = rarity;
            Value = value;
        }

        // Innehåller data relaterat till ett skräp.
        public string Name { get; }
        public string AssociatedAssetName { get; }
        public int Rarity { get; set; }
        public int Value { get; set; } // Hur mycket värdet minskar varje gång man fiskar upp saken
    }
}
