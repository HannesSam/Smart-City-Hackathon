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
        public int TypeOfHouse { get; set; }
        public int BuildingMaxTemp { get; set; }
        public int BuildingMinTemp { get; set; }
        public double TempAdjustValue { get; set; }

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
            TypeOfHouse = 1;
            BuildingMaxTemp = 24;
            BuildingMinTemp = 18;
            TempAdjustValue = 0.5;
        }
    }
}
