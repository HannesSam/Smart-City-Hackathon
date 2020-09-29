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
        private List<BuildableTile> BuildingsUnderConstr;
        private List<BuildableTile> ConstructedBuildings;

        public AI_nr2(GameLayer GL)
        {
            GameLayer = GL;
            ListOfBuildPositions = new List<BuildableTile>();
            ConstructedBuildings = new List<BuildableTile>();
        }

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
                        if (CheckTile(state.Map, i - 1, j -1))
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
                        if (CheckTile(state.Map, i +1 , j))
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
        }

        public void Take_turn(string gameId, ConfigValues config)
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
            var state = GameLayer.GetState();

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
            UrgencyValue RepairTask = new UrgencyValue(repairValue, GameTask.Repair);
            urgencyValues.Add(RepairTask);
            UrgencyValue TemperatureTask = new UrgencyValue(temperatureValue, GameTask.ChangeTemperature);
            urgencyValues.Add(TemperatureTask);
            UrgencyValue WaitTask = new UrgencyValue(waitValue, GameTask.Wait);
            urgencyValues.Add(WaitTask);

            //Evaluate neccissity of every task

            //start build
            if (state.ResidenceBuildings.Count < config.NumberOfResidenceBuildings)
            {
                StartBuildTask.Value = 70;
            }
            else if(state.Funds > config.FundsLevelBuildHouse && state.HousingQueue > config.HousingQueue)
            {
                StartBuildTask.Value = 40;
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
                    var building = ListOfBuildPositions[0];
                    GameLayer.StartBuild(new Position(building.XSpot, building.YSpot), state.AvailableResidenceBuildings[config.TypeOfHouse].BuildingName,
        gameId);
                    ListOfBuildPositions.RemoveAt(0);
                    //BuildableTile build = new BuildableTile(building.XSpot, building.YSpot);
                    //ConstructedBuildings.Add(build);
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
                case GameTask.ChangeTemperature:
                    for (int i = 0; i < state.ResidenceBuildings.Count; i++)
                    {
                        var changeTemperatureSpot = state.ResidenceBuildings[i];
                        if (changeTemperatureSpot.Temperature < config.BuildingMinTemp)
                        {
                            var bluePrint = GameLayer.GetResidenceBlueprint(changeTemperatureSpot.BuildingName);
                            var energy = bluePrint.BaseEnergyNeed + (changeTemperatureSpot.Temperature - state.CurrentTemp)
                                * bluePrint.Emissivity / 1 + config.TempAdjustValue - changeTemperatureSpot.CurrentPop * 0.04;
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

