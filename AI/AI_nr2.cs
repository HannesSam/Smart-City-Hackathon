using DotNet.AI.AI_models;
using DotNet.models;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace DotNet.AI
{
    class AI_nr2
    {
        private readonly GameLayer GameLayer;

        private List<BuildableTile> ListOfBuildPositions;
        private List<BuildableTile> UtilityPositions;
        private List<BuildableTile> ResidencePositions;
        private List<BuildableTile> ListOfUtilityPositions;
        private List<BuildableTile> BuiltResidences;
        private List<BuildableTile> BuiltUtilitys;

        readonly ConfigValues config;
        GameState prevState;

        public AI_nr2(GameLayer GL, ConfigValues config)
        {
            GameLayer = GL;
            this.config = config;
            ListOfBuildPositions = new List<BuildableTile>();
            UtilityPositions = new List<BuildableTile>();
            ResidencePositions = new List<BuildableTile>();
            ListOfUtilityPositions = new List<BuildableTile>();
            BuiltResidences = new List<BuildableTile>();
            BuiltUtilitys = new List<BuildableTile>();

        }

        //Alla lediga tiles får ett värde beroende på hur många lediga rutor som finns i en radie av 1. Läggs in i ListOfBuildPositions.
        // TODO När man bygger en Utility borde denna updateras. 
        public void ConfigureMap()
        {
            var state = GameLayer.GetState();
            for (var i = 0; i < 10; i++)
            {
                for (var j = 0; j < 10; j++)
                {
                    if (state.Map[i][j] == 0)
                    {
                        int value = 1;

                        //Checks to see if the six squares around it are buildable land or not. Then raises that tiles score by one. 
                        if (CheckTile(state.Map, i - 1, j - 1))
                        {
                            value++;
                        }
                        if (CheckTile(state.Map, i - 1, j))
                        {
                            value++;
                        }
                        if (CheckTile(state.Map, i - 1, j + 1))
                        {
                            value++;
                        }
                        if (CheckTile(state.Map, i, j - 1))
                        {
                            value++;
                        }
                        if (CheckTile(state.Map, i, j + 1))
                        {
                            value++;
                        }
                        if (CheckTile(state.Map, i + 1, j - 1))
                        {
                            value++;
                        }
                        if (CheckTile(state.Map, i + 1, j))
                        {
                            value++;
                        }
                        if (CheckTile(state.Map, i + 1, j + 1))
                        {
                            value++;
                        }


                        BuildableTile tile = new BuildableTile(i, j, value);


                        ListOfBuildPositions.Add(tile);
                    }
                }
            }
            //Sortera listan efter mest värdefulla positionerna. 
            ListOfBuildPositions = ListOfBuildPositions.OrderByDescending(x => x.Value).ToList();

            //Räkna ut hur många platser som ska tas up av utility buildings och lägg till så många platser till listan med UtilityPositions.
            //Utility buildings får de mest värdefulla platserna då de har area of effect vilket vi ska se till täcker så många hus som möjligt.
            UtilityPositions = ListOfBuildPositions.GetRange(0, ListOfBuildPositions.Count / config.PartOfUtilityBuildings);

            //Detta gör så att vi bygger en jämn blanding av alla utilitybuildings
            int counter = 0;
            foreach (var item in UtilityPositions)
            {
                if (counter == 0)
                {
                    item.UtilityType = Utility.Park;
                }
                else if (counter == 1)
                {
                    item.UtilityType = Utility.Mall;
                }
                else if (counter == 2 )
                {
                    item.UtilityType = Utility.WindTurbine;
                }

                counter++;
                if (counter == 3) 
                {
                    counter = 0;
                }
            }

            ListOfBuildPositions.RemoveRange(0, ListOfBuildPositions.Count / config.PartOfUtilityBuildings);

            //De byggplatser som finns kvar sparas för att bygga hus på.
            ResidencePositions = ListOfBuildPositions;
            //sätt alla hus till de hus vi har definerat i config klassen
            foreach (var item in ResidencePositions)
            {
                item.ResidenceType = config.TypeOfHouse;
            }

            //detta är bara ett test för den senaste metan när man får bonus för att det finns flera typer av hus på varje bana.
            //ResidencePositions[3].ResidenceType = Residence.Apartments;
            //ResidencePositions[4].ResidenceType = Residence.Cabin;
            //ResidencePositions[5].ResidenceType = Residence.HighRise;
            //ResidencePositions[6].ResidenceType = Residence.LuxuryResidence;
            //ResidencePositions[7].ResidenceType = Residence.EnviromentalHouse;
            //Lägg till resten till listan med residence positions

        }

        //Alla lediga tiles får ett värde beroende på hur många lediga rutor som finns i en radie av 1. Läggs in i ListOfUtilityPositions. 
        // TODO Updatera värdet varje gång någonting byggs. Värdet ökar ju fler byggnader och potentiella byggnader som byggs och 
                //ökar inte om en närliggande byggnad är en annan utilitybyggnad. 
        
        //Egentligen borde man utgå från ListOfBuildPositions så slipper man söka igenom hela brädet igen.
        public void BestUtilityPositions(int numberOfUtilities)
        {
            var state = GameLayer.GetState();
            var effectedByMall = new List<Tuple<int, int>>();
            var effectedByWindTurbine = new List<Tuple<int, int>>();
            var effectedByPark = new List<Tuple<int, int>>();
            for (int k = 0; k < numberOfUtilities; k++)
            {
                for (var i = 0; i < 10; i++)
                {
                    for (var j = 0; j < 10; j++)
                    {
                        if (state.Map[i][j] == 0)
                        {
                            int value = 1;

                            //Checks to see if the squares in a radius of 2 is buildable
                            if (CheckTile(state.Map, i - 2, j - 2))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i - 1, j - 2))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i, j - 2))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i + 1, j - 2))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i + 2, j - 2))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i - 2, j - 1))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i - 1, j - 1))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i, j - 1))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i + 1, j - 1))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i + 2, j - 1))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i - 2, j))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i - 1, j))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i, j))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i + 1, j))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i + 2, j))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i - 2, j + 1))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i - 1, j + 1))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i, j + 1))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i + 1, j + 1))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i + 2, j + 1))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i - 2, j + 2))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i - 1, j + 2))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i, j + 2))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i + 1, j + 2))
                            {
                                value++;
                            }
                            if (CheckTile(state.Map, i + 2, j + 2))
                            {
                                value++;
                            }


                            BuildableTile tile = new BuildableTile(i, j, value);


                            ListOfUtilityPositions.Add(tile);
                        }
                    }
                }
            }
        }

        public void ReserveUtilityPositions()
        {

        }

        public void Take_turn(string gameId)
        {

            //Vi skapar "Urgency values" som alltid ska refleketera hur viktigt något är att göra denna tur.
            //Dessa använder vi sen för att väljav vilken handling vi tar. 

            //Måste även implementera en "value map" som beräknar vilka platser på kartan som är mest
            //Värdefulla att bygga hushåll alternativt utilities på.
            //Tex så är ju platser utan många angrändsande platser attraktiva för vindkraftverk 
            //och platser med många rutor nära varandra attraktiva för ställen med områdeseffekter. 

            // TODO Implement your artificial intelligence here.
            // TODO Taking one action per turn until the game ends.
            // TODO The following is a short example of how to use the StarterKit
            GameState state = GameLayer.GetState();

            //Urgency values
            int startBuildValue = config.StartBuildValue;
            int buildValue = config.BuildValue;
            int repairValue = config.RepairValue;
            int temperatureValue = config.TemperatureValue;
            int waitValue = config.WaitValue;

            List<UrgencyValue> urgencyValues = new List<UrgencyValue>();

            UrgencyValue StartBuildTask = new UrgencyValue(startBuildValue, GameTask.StartBuild);
            urgencyValues.Add(StartBuildTask);
            UrgencyValue BuildTask = new UrgencyValue(buildValue, GameTask.Build);
            urgencyValues.Add(BuildTask);
            //implementera start värde
            UrgencyValue UtilityTask = new UrgencyValue(0, GameTask.BuildUtility);
            urgencyValues.Add(UtilityTask);
            UrgencyValue UpgradetTask = new UrgencyValue(0, GameTask.Upgrade);
            urgencyValues.Add(UpgradetTask);
            UrgencyValue RepairTask = new UrgencyValue(repairValue, GameTask.Repair);
            urgencyValues.Add(RepairTask);
            UrgencyValue TemperatureTask = new UrgencyValue(temperatureValue, GameTask.ChangeTemperature);
            urgencyValues.Add(TemperatureTask);
            UrgencyValue WaitTask = new UrgencyValue(waitValue, GameTask.Wait);
            urgencyValues.Add(WaitTask);

            //Evaluate neccissity of every task

            //start build
            if (ResidencePositions.Count < 1)
            {
                //Gör ingenting
            }
            else if (state.ResidenceBuildings.Count < config.NumberOfResidenceBuildings)
            {
                StartBuildTask.Value = 70;
            }
            else if (state.Funds > config.FundsLevelBuildHouse && state.HousingQueue > config.HousingQueue)
            {
                StartBuildTask.Value = 40;
            }
            //Utility buildings
            else if (UtilityPositions.Count > 0 && state.Funds > 20000)
            {
                UtilityTask.Value = 30;
            }


            //Build building
            //Repair buildning
            //Change temperature
            for (int i = 0; i < state.ResidenceBuildings.Count; i++)
            {
                var building = state.ResidenceBuildings[i];
                if (building.BuildProgress < 100)
                {
                    BuildTask.Value = 80;
                }
                if (building.Health < 50)
                {
                    RepairTask.Value = 70;
                }
                if (building.Temperature < config.BuildingMinTemp || building.Temperature > config.BuildingMaxTemp)
                {
                    TemperatureTask.Value = 60;
                }

            }

            //Samma för upgrades. En annan loop här då vi går igenom våran egen lista med byggda byggnader. 
            for (int i = 0; i < BuiltResidences.Count; i++)
            {
                var building = BuiltResidences[i];
                if (state.Funds > 8000 && building.UpgradesType == Upgrades.None)
                {
                    UpgradetTask.Value = 20;
                }
            }
                //same for utility buildoings
                for (int i = 0; i < state.UtilityBuildings.Count; i++)
            {
                var building = state.UtilityBuildings[i];
                if (building.BuildProgress < 100)
                {
                    BuildTask.Value = 80;
                }
            }

            //Get the biggest urgencyValue. That being the most urgent task 
            int maxValue = 0;
            GameTask taskToPerform = GameTask.Build;
            foreach (UrgencyValue item in urgencyValues)
            {
                if (item.Value > maxValue)
                {
                    maxValue = item.Value;
                    taskToPerform = item.GameTask;
                }
            }

            switch (taskToPerform)
            {
                case GameTask.StartBuild:
                    var building = ResidencePositions[0];
                    GameLayer.StartBuild(new Position(building.XSpot, building.YSpot), state.AvailableResidenceBuildings[(int)ResidencePositions[0].ResidenceType].BuildingName,
        gameId);
                    BuiltResidences.Add(ResidencePositions[0]);
                    ResidencePositions.RemoveAt(0);
                    break;
                case GameTask.Build:
                    for (int i = 0; i < state.ResidenceBuildings.Count; i++)
                    {
                        var buildSpot = state.ResidenceBuildings[i];
                        if (buildSpot.BuildProgress < 100)
                        {
                            GameLayer.Build(buildSpot.Position, gameId);
                            break;
                        }
                    }
                    for (int i = 0; i < state.UtilityBuildings.Count; i++)
                    {
                        var buildSpot = state.UtilityBuildings[i];
                        if (buildSpot.BuildProgress < 100)
                        {
                            GameLayer.Build(buildSpot.Position, gameId);
                            break;
                        }
                    }
                    break;
                case GameTask.BuildUtility:
                    var utilityBuilding = UtilityPositions[0];
                    GameLayer.StartBuild(new Position(utilityBuilding.XSpot, utilityBuilding.YSpot), state.AvailableUtilityBuildings[(int)UtilityPositions[0].UtilityType].BuildingName,
        gameId);
                    BuiltUtilitys.Add(UtilityPositions[0]);
                    UtilityPositions.RemoveAt(0);
                    break;
                case GameTask.Repair:
                    for (int i = 0; i < state.ResidenceBuildings.Count; i++)
                    {
                        var repairSpot = state.ResidenceBuildings[i];
                        if (repairSpot.Health < 50)
                        {
                            GameLayer.Maintenance(repairSpot.Position, gameId);
                            break;
                        }
                    }
                    break;
                case GameTask.Upgrade:
                    for (int i = 0; i < BuiltResidences.Count; i++)
                    {
                        var residence = BuiltResidences[i];
                        if (residence.UpgradesType == Upgrades.None)
                        {
                            GameLayer.BuyUpgrade(new Position(residence.XSpot, residence.YSpot), state.AvailableUpgrades[3].Name, gameId);
                            BuiltResidences[i].UpgradesType = Upgrades.SolarPanel;
                            break;
                        }
                    }
                    break;
                case GameTask.ChangeTemperature:
                    for (int i = 0; i < state.ResidenceBuildings.Count; i++)
                    {
                        var changeTemperatureSpot = state.ResidenceBuildings[i];
                        if (changeTemperatureSpot.Temperature < config.BuildingMinTemp)
                        {
                            var bluePrint = GameLayer.GetResidenceBlueprint(changeTemperatureSpot.BuildingName);
                            var energy = bluePrint.BaseEnergyNeed + (changeTemperatureSpot.Temperature - state.CurrentTemp)
                                * bluePrint.Emissivity / 1 + config.TempAdjustValue - changeTemperatureSpot.CurrentPop * 0.04;

                            //test av alternativ algoritm 
                            //var degPerPop = 0.04;
                            //var degPerExcessMwh = config.TempAdjustValue;
                            //var energy = (21 - changeTemperatureSpot.Temperature - degPerPop * changeTemperatureSpot.CurrentPop +
                            //    (changeTemperatureSpot.Temperature - state.CurrentTemp) * bluePrint.Emissivity) / degPerExcessMwh + bluePrint.BaseEnergyNeed;
                            GameLayer.AdjustEnergy(changeTemperatureSpot.Position, energy, gameId);
                            break;
                        }
                        else if (changeTemperatureSpot.Temperature > config.BuildingMaxTemp)
                        {
                            var bluePrint = GameLayer.GetResidenceBlueprint(changeTemperatureSpot.BuildingName);
                            var energy = bluePrint.BaseEnergyNeed + (changeTemperatureSpot.Temperature - state.CurrentTemp)
                                * bluePrint.Emissivity / 1 - config.TempAdjustValue - changeTemperatureSpot.CurrentPop * 0.04;
                            GameLayer.AdjustEnergy(changeTemperatureSpot.Position, energy, gameId);
                            break;
                        }
                    }
                    break;
                case GameTask.Wait:
                    GameLayer.Wait(gameId);
                    break;
                default:
                    break;
            }

            foreach (var message in GameLayer.GetState().Messages)
            {
                Log.Information(message);
            }

            foreach (var error in GameLayer.GetState().Errors)
            {
                Log.Information("Error: " + error);
            }

            prevState = state;

        }

        private bool CheckTile(int[][] map, int x, int y)
        {
            try
            {
                if (map[x][y] == 0)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }
    }
}

