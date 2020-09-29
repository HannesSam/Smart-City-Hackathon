using System;
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

            //var hej = GameLayer.GetGames();
            //GameLayer.EndGame("a372d4bf-6665-4069-8040-a59d81e9817a");
            //log för att logga info om score över olika körningar och de olika parametrar som då fanns.
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs\\myapp.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Hello, world!");

            //för att testa en ai ändra värdet här och i loopen
            AI_nr2 AI = new AI_nr2(GameLayer);

            var gameId = GameLayer.NewGame(Map);
            Log.Information($"Starting game: {GameLayer.GetState().GameId}");
            GameLayer.StartGame(gameId);
            AI.ConfigureMap();
            ConfigValues config = new ConfigValues();

            while (GameLayer.GetState().Turn < GameLayer.GetState().MaxTurns)
            {
                AI.Take_turn(gameId, config);

            }
            Log.Information($"Final money: {GameLayer.GetState().Funds}");
            Log.Information($"Final pop: {GameLayer.GetScore(gameId).FinalPopulation}");
            Log.Information($"Final happines: {GameLayer.GetScore(gameId).TotalHappiness}");
            Log.Information($"Final Co2: {GameLayer.GetScore(gameId).TotalCo2}");

            Log.Information($"Final pop score: {GameLayer.GetScore(gameId).FinalPopulation * 15}");
            Log.Information($"Final happines score: {GameLayer.GetScore(gameId).TotalHappiness / 10}");
            Log.Information($"Final Co2 score: {GameLayer.GetScore(gameId).TotalCo2}");

            Log.Information($"Done with game: {GameLayer.GetState().GameId}");
            Log.Information(GameLayer.GetScore(gameId).FinalScore.ToString());

            Log.CloseAndFlush();
            Console.ReadKey();
        }

    }
}