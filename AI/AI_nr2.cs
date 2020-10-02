﻿using DotNet.AI.AI_models;
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
            UtilityPositions = BestUtilityPositions((ListOfBuildPositions.Count / config.PartOfUtilityBuildings), UtilityPositions);
            Console.WriteLine("Antal Utilities = " + UtilityPositions.Count);
            //Tar bort positionerna som finns i UtilityPositions i ListOfBuildPositions
            ReserveUtilityPositions();
            ResidencePositions = ListOfBuildPositions;

            //Detta gör så att vi bygger en jämn blanding av alla utilitybuildings
            // OBS detta är bara Optimalt för tillfället
            int counter = 0;
            foreach (var item in UtilityPositions)
            {
                if (counter == 0)
                {
                    item.UtilityType = Utility.WindTurbine;
                }
                else if (counter == 1)
                {
                    item.UtilityType = Utility.WindTurbine;
                }
                else if (counter == 2)
                {
                    item.UtilityType = Utility.WindTurbine;
                }

                counter++;
                if (counter == 3)
                {
                    counter = 1;
                }
            }

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

        //Alla lediga tiles får ett värde beroende på hur många lediga rutor som finns i en radie av 2. Läggs in i ListOfUtilityPositions. 
        public List<BuildableTile> BestUtilityPositions(int antalUtilities, List<BuildableTile> supportBuildings)
        {
            var state = GameLayer.GetState();
            for (var i = 0; i < 10; i++)
            {
                for (var j = 0; j < 10; j++)
                {
                    if (state.Map[i][j] == 0)
                    {
                        //Checks to see if the squares in a radius of 2 is buildable
                        //Om den här platsen är reserverad i listan supportBuildings så breakar vi. 
                        if (reservedTile(i, j, supportBuildings))
                        {
                            break;
                        }
                        int value;
                        if (supportBuildings.Count < 1)
                        {
                            value = 1;
                        }
                        else
                        {
                            value = AdjecentSupport(i, j, supportBuildings);
                        };
                        if (CheckTile(state.Map, i + 1, j))
                        {
                            value++;
                        }
                        if (CheckTile(state.Map, i + 2, j))
                        {
                            value++;
                        }
                        if (CheckTile(state.Map, i - 1, j))
                        {
                            value++;
                        }
                        if (CheckTile(state.Map, i - 2, j))
                        {
                            value++;
                        }
                        if (CheckTile(state.Map, i, j - 2))
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
                        if (CheckTile(state.Map, i, j + 2))
                        {
                            value++;
                        }
                        if (CheckTile(state.Map, i - 1, j - 1))
                        {
                            value++;
                        }
                        if (CheckTile(state.Map, i + 1, j + 1))
                        {
                            value++;
                        }
                        if (CheckTile(state.Map, i - 1, j + 1))
                        {
                            value++;
                        }
                        if (CheckTile(state.Map, i + 1, j - 1))
                        {
                            value++;
                        }

                        BuildableTile tile = new BuildableTile(i, j, value);

                        ListOfUtilityPositions.Add(tile);
                    }
                }
            }
            int maxposition = 0;

            for (int i = 0; i < ListOfUtilityPositions.Count - 1; i++)
            {
                if (ListOfUtilityPositions[i].Value > ListOfUtilityPositions[maxposition].Value)
                {
                    maxposition = i;
                }
            }
            supportBuildings.Add(ListOfUtilityPositions[maxposition]);
            if (antalUtilities - 1 == 0)
            {

                return supportBuildings;
            }
            ListOfUtilityPositions.Clear();
            return BestUtilityPositions(antalUtilities - 1, supportBuildings);
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
            if (ResidencePositions.Count == 0 && UtilityPositions.Count == 0)
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
            else if (UtilityPositions.Count > 0 && state.Funds > 20000 && BuiltResidences.Count>BuiltUtilitys.Count*3)
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
                var residence = BuiltResidences[i];
                foreach (var item in config.UpgradesToBuild)
                {
                    if (!residence.BuiltUpgrades.Contains(item) && state.Funds > 8000)
                    {
                        UpgradetTask.Value = 20;
                    }
                }
            }
            //same for utility buildings
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
            if (ResidencePositions.Count == 0)
            {
                taskToPerform = GameTask.Wait;
            }
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
                        Upgrades upgradeToBuild = Upgrades.None;
                        foreach (var item in config.UpgradesToBuild)
                        {
                            
                            if (!residence.BuiltUpgrades.Contains(item))
                            {
                                upgradeToBuild = item;
                            }
                        }
                        if(upgradeToBuild != Upgrades.None)
                        {
                            GameLayer.BuyUpgrade(new Position(residence.XSpot, residence.YSpot), state.AvailableUpgrades[(int) upgradeToBuild].Name, gameId);
                            BuiltResidences[i].BuiltUpgrades.Add(upgradeToBuild);
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

        private void ReserveUtilityPositions()
        {
            for (int i = 0; i < UtilityPositions.Count; i++)
            {
                var item1 = UtilityPositions[i];
                for (int j = 0; j < ListOfBuildPositions.Count; j++)
                {
                    var item2 = ListOfBuildPositions[j];
                    if (item1.XSpot == item2.XSpot && item1.YSpot == item2.YSpot)
                    {
                        ListOfBuildPositions.RemoveAt(j);
                    }
                }
            }
        }

        private int AdjecentSupport(int x, int y, List<BuildableTile> lista)
        {
            int value = 1;
            if (lista.Count == 0)
            {
                return value;
            }

            if (reservedTile(x, y + 1, lista))
            {
                value -= 2;
            }
            if (reservedTile(x, y + 2, lista))
            {
                value -= 2;
            }
            if (reservedTile(x, y - 1, lista))
            {
                value -= 2;
            }
            if (reservedTile(x, y - 2, lista))
            {
                value -= 2;
            }
            if (reservedTile(x + 1, y - 1, lista))
            {
                value -= 2;
            }
            if (reservedTile(x + 1, y + 1, lista))
            {
                value -= 2;
            }
            if (reservedTile(x + 1, y, lista))
            {
                value -= 2;
            }
            if (reservedTile(x - 1, y, lista))
            {
                value -= 2;
            }
            if (reservedTile(x - 1, y + 1, lista))
            {
                value -= 2;
            }
            if (reservedTile(x - 1, y - 1, lista))
            {
                value -= 2;
            }
            if (reservedTile(x - 2, y, lista))
            {
                value -= 2;
            }
            if (reservedTile(x + 2, y, lista))
            {
                value -= 2;
            }
            return value;
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

        private bool reservedTile(int x, int y, List<BuildableTile> upptagna)
        {
            foreach (var item in upptagna)
            {
                if (item.XSpot == x && item.YSpot == y)
                {
                    return true;
                }
            }
            return false;
        }

        //Kan användas om man ska bygga fler än 3 utilities.
        private bool CheckAOE(int x, int y, string aoetype)
        {
            switch (aoetype)
            {
                case "mall1":
                    foreach (var item in BuiltResidences)
                    {
                        if (item.XSpot == x && item.YSpot == y && item.mall1Effect == true)
                        {
                            return true;
                        }
                    }
                    return false;
                case "mall2":
                    foreach (var item in BuiltResidences)
                    {
                        if (item.XSpot == x && item.YSpot == y && item.mall2Effect == true)
                        {
                            return true;
                        }
                    }
                    return false;
                case "windturbine":
                    foreach (var item in BuiltResidences)
                    {
                        if (item.XSpot == x && item.YSpot == y && item.windTurbineEffect == true)
                        {
                            return true;
                        }
                    }
                    return false;
                case "park":
                    foreach (var item in BuiltResidences)
                    {
                        if (item.XSpot == x && item.YSpot == y && item.parkEffect == true)
                        {
                            return true;
                        }
                    }
                    return false;
            }
            return false;
        }
    }
}