using System;
using System.Collections.Generic;
using System.Text;

namespace FishGame
{
    struct FishingRodData
    {
        // Representerar data knutet till ett fiskespö
        public string RodName { get; set; }
        private double RodSpeed { get; set; } // Fiskespöets hastighet
        private double RodMaxDepth { get; set; } // Fiskespöets maxdjup
        private int RodMaxFishLevel { get; set; } // Fiskespöets maximala fisknivå

        public FishingRodData(
            string rodName,
            double rodSpeed,
            double rodMaxDepth,
            int rodMaxFishLevel
        )
        {
            RodName = rodName;
            RodSpeed = rodSpeed;
            RodMaxDepth = rodMaxDepth;
            RodMaxFishLevel = rodMaxFishLevel;
        }

        public void LevelUpFishingRod(
            double speedMultiplier = 1.5,
            double depthMultiplier = 1.75,
            int fishLevelIncrease = 2
        )
        {
            // Funktion för att levla upp ett fiskespö så att den får nya egenskaper
            RodSpeed = RodSpeed * speedMultiplier;
            RodMaxDepth = RodMaxDepth * depthMultiplier; // Öka bredden
            RodMaxFishLevel = RodMaxFishLevel + fishLevelIncrease;
        }
    }
}
