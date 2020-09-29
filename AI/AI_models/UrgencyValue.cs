using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.AI.AI_models
{
    public enum GameTask { StartBuild, Build, Repair, ChangeTemperature, Wait }

    class UrgencyValue
    {
        public int Value { get; set; }
        public GameTask GameTask { get; set; }
        public UrgencyValue(int value, GameTask gameTask)
        {
            Value = value;
            GameTask = gameTask;
        }
    }
}
