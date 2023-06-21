using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class PlayerData
    {
        private const string LevelCompletedKey = "LevelCompleted";
        private const string GameCurrencyKey = "GameCurrency";

        public float BaseSpeed = 0.3f;
        public float BaseForce = 1f;
        public float BaseMilitaryReproduction = 4f;
        public float BaseArmyReproduction = 2f;
        public float BaseDebuffProtection;
        public float BaseProtection;

        public readonly List<BuildingType> AvailableBuildings = new List<BuildingType>();
        
        public readonly List<Talent> BoughtTalents = new List<Talent>();

        public static int LevelCompleted
        {
            get => PlayerPrefs.GetInt(LevelCompletedKey, 0);
            set => PlayerPrefs.SetInt(LevelCompletedKey, value);
        }

        public static int GameCurrency
        {
            get => PlayerPrefs.GetInt(GameCurrencyKey, 0);
            set => PlayerPrefs.SetInt(GameCurrencyKey, value);
        }
    }
}