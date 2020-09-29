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
                        BuildableTile tile = new BuildableTile(i, j);
                        ListOfBuildPositions.Add(tile);
                    }
                }
            }
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
            var state = GameLayer.GetState();

            //Urgency values
            int startBuildValue = 0;
            int buildValue = 0;
            int repairValue = 0;
            int temperatureValue = 0;
            int waitValue = 10;

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
            if (state.ResidenceBuildings.Count < 8)
            {
                StartBuildTask.Value = 70;
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
                    RepairTask.Value = 50;
                }
                if (building.Temperature < 17 || building.Temperature > 24)
                {
                    TemperatureTask.Value = 90;
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
                    GameLayer.StartBuild(new Position(building.XSpot, building.YSpot), state.AvailableResidenceBuildings[0].BuildingName,
        gameId);
                    ListOfBuildPositions.RemoveAt(0);
                    BuildableTile build = new BuildableTile(building.XSpot, building.YSpot);
                    ConstructedBuildings.Add(build);
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
                        if (changeTemperatureSpot.Temperature < 17)
                        {
                            var bluePrint = GameLayer.GetResidenceBlueprint(changeTemperatureSpot.BuildingName);
                            var energy = bluePrint.BaseEnergyNeed + (changeTemperatureSpot.Temperature - state.CurrentTemp)
                                * bluePrint.Emissivity / 1 + 1.5 - changeTemperatureSpot.CurrentPop * 0.04;
                            GameLayer.AdjustEnergy(changeTemperatureSpot.Position, energy, gameId);
                            break;
                        }
                        else if (changeTemperatureSpot.Temperature > 24)
                        {
                            var bluePrint = GameLayer.GetResidenceBlueprint(changeTemperatureSpot.BuildingName);
                            var energy = bluePrint.BaseEnergyNeed + (changeTemperatureSpot.Temperature - state.CurrentTemp)
                                * bluePrint.Emissivity / 1 - 1.5 - changeTemperatureSpot.CurrentPop * 0.04;
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
    }
}

