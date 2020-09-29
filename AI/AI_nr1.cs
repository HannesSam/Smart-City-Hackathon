using DotNet.models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.AI
{
    //Detta är den "AI" som kom med som exempel från utvecklarna
    public class AI_nr1
    {

        private readonly GameLayer GameLayer;

        public AI_nr1(GameLayer GL)
        {
            GameLayer = GL;
        }

        public void take_turn(string gameId)
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
            var x = 0;
            var y = 0;
            var state = GameLayer.GetState();
            if (state.ResidenceBuildings.Count < 1)
            {
                for (var i = 0; i < 10; i++)
                {
                    for (var j = 0; j < 10; j++)
                    {
                        if (state.Map[i][j] == 0)
                        {
                            x = i;
                            y = j;
                            break;
                        }
                    }
                }

                GameLayer.StartBuild(new Position(x, y), state.AvailableResidenceBuildings[0].BuildingName,
                    gameId);
            }

            else
            {
                var building = state.ResidenceBuildings[0];
                if (building.BuildProgress < 100)
                {
                    GameLayer.Build(building.Position, gameId);
                }
                else if (!building.Effects.Contains(state.AvailableUpgrades[0].Name))
                    GameLayer.BuyUpgrade(building.Position, state.AvailableUpgrades[0].Name, gameId);
                else if (building.Health < 50)
                {
                    GameLayer.Maintenance(building.Position, gameId);
                }

                else if (building.Temperature < 18)
                {
                    var bluePrint = GameLayer.GetResidenceBlueprint(building.BuildingName);
                    var energy = bluePrint.BaseEnergyNeed + (building.Temperature - state.CurrentTemp)
                        * bluePrint.Emissivity / 1 + 0.5 - building.CurrentPop * 0.04;
                    GameLayer.AdjustEnergy(building.Position, energy, gameId);
                }
                else if (building.Temperature > 24)
                {
                    var bluePrint = GameLayer.GetResidenceBlueprint(building.BuildingName);
                    var energy = bluePrint.BaseEnergyNeed + (building.Temperature - state.CurrentTemp)
                        * bluePrint.Emissivity / 1 - 0.5 - building.CurrentPop * 0.04;
                    GameLayer.AdjustEnergy(building.Position, energy, gameId);
                }
                else GameLayer.Wait(gameId);

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
}
