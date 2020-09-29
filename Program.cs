﻿using System;
using System.Linq;
using DotNet.AI;
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

            var hej = GameLayer.GetGames();
            //GameLayer.EndGame("687b0d01-da3d-40b6-a780-7534ca9f9729");
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

            while (GameLayer.GetState().Turn < GameLayer.GetState().MaxTurns)
            {
                AI.Take_turn(gameId);
            }
            Log.Information($"Done with game: {GameLayer.GetState().GameId}");
            Log.Information(GameLayer.GetScore(gameId).FinalScore.ToString());
            

            Log.CloseAndFlush();
            Console.ReadKey();
        }

    }
}