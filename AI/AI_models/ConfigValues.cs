using DotNet.models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.AI.AI_models
{
    class ConfigValues
    {
        public int StartBuildValue { get; set; }
        public int BuildValue { get; set; }
        public int RepairValue { get; set; }
        public int TemperatureValue { get; set; }
        public int WaitValue { get; set; }
        public int NumberOfResidenceBuildings { get; set; }
        public int HousingQueue { get; set; }
        public int FundsLevelBuildHouse {get; set;}
        public Residence TypeOfHouse { get; set; }
        public int BuildingMaxTemp { get; set; }
        public int BuildingMinTemp { get; set; }
        public double TempAdjustValue { get; set; }
        public int PartOfUtilityBuildings { get; set; }
        public List<Upgrades> UpgradesToBuild { get; set; }

        public ConfigValues()
        {
            StartBuildValue = 0;
            BuildValue = 0;
            RepairValue = 0;
            TemperatureValue = 0;
            WaitValue = 10;
            NumberOfResidenceBuildings = 2;
            HousingQueue = 10;
            FundsLevelBuildHouse = 10000;
            TypeOfHouse = Residence.ModernApartments;
            BuildingMaxTemp = 24;
            BuildingMinTemp = 18;
            TempAdjustValue = 1.95;
            PartOfUtilityBuildings = 5;
            UpgradesToBuild = new List<Upgrades>();
            UpgradesToBuild.Add(Upgrades.Playground);
            UpgradesToBuild.Add(Upgrades.SolarPanel);
            UpgradesToBuild.Add(Upgrades.Regulator);
        }
    }
}
