using System;
using System.Collections.Generic;
using System.Linq;
using DotNet.AI;
using DotNet.AI.AI_models;
using DotNet.models;
using Serilog;

namespace DotNet
{
    public static class Program
    {
        //test kommentar för att se att git funkar som det ska
        private const string ApiKey = "6ffe9713-2be9-487d-afd3-f345b3df59b8";           // TODO: Enter your API key
        // The different map names can be found on considition.com/rules
        private const string Map = "training1";     // TODO: Enter your desired map
        private static readonly GameLayer GameLayer = new GameLayer(ApiKey);

        public static void Main(string[] args)
        {
            //Dessa här under kan användas för att rensa i aktiva spel ifall vi inte får starta ett nytt för det är så många på gång.
            //var hej = GameLayer.GetGames();
            //GameLayer.EndGame("5a512a1d-96fb-4ae2-ba2e-e2968f14f9e8");
            //GameLayer.EndGame("dc3b2bba-8b70-4724-bdfb-349b40da4d5c");
            //GameLayer.EndGame("1a300ac5-c834-4582-8e4b-224af2a9d521");
            //GameLayer.EndGame("caea16a3-5d0c-41e3-9828-95d87cc95189");


            //log för att logga info om score över olika körningar och de olika parametrar som då fanns.
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs\\myapp.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            //Obligatorisk hello world log
            Log.Information("Hello, world!");

            ConfigValues config = new ConfigValues();
            AI_nr2 AI = new AI_nr2(GameLayer, config);


            //Ändra denna för att köra programmet i olika konfigurationer
            int runMode = 1;
            //1 = kör en gång som vanligt.
            //2 = kör 6 rundor
            //3 = kalibrera temp värden

            switch (runMode)
            {
                case 1:
                    string endgame = RunOneGame(AI);
                    Log.Information(endgame);
                    break;
                case 2:
                    RunFourGames(AI);
                    break;
                case 3:

                    break;
                default:
                    break;
            }


            Log.CloseAndFlush();
        }

        static void RunFourGames(AI_nr2 AI)
        {
            List<string> results = new List<string>();
            string temp;
            for (int i = 0; i < 4; i++)
            {
                temp = RunOneGame(AI);
                results.Add(temp);
            }
            int counter = 1;
            foreach (var item in results)
            {
                Log.Information($"\nGame number {counter}");
                Log.Information(item);
                counter++;
            }
        }

        static string RunOneGame(AI_nr2 AI)
        {
            var gameId = GameLayer.NewGame(Map);
            Log.Information($"Starting game: {GameLayer.GetState().GameId}");
            GameLayer.StartGame(gameId);
            AI.ConfigureMap();

            while (GameLayer.GetState().Turn < GameLayer.GetState().MaxTurns)
            {
                AI.Take_turn(gameId);

            }
            string endgame = string.Format("Money: {0} \nPop: {1} \nHappiness: {2} \nCo2: {3} \n\nFinal Pop Score: {4}, \nFinal Happiness Score: {5} \nFinal Co2 Score: {6} \nFinal Score: {7} \nDone with game {8}",
               GameLayer.GetState().Funds, GameLayer.GetScore(gameId).FinalPopulation, GameLayer.GetScore(gameId).TotalHappiness, GameLayer.GetScore(gameId).TotalCo2,
               GameLayer.GetScore(gameId).FinalPopulation * 15, GameLayer.GetScore(gameId).TotalHappiness / 10, GameLayer.GetScore(gameId).TotalCo2, GameLayer.GetScore(gameId).FinalScore.ToString(),
               GameLayer.GetState().GameId);
            return endgame;

        }

    }
}