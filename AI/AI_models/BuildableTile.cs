﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.models
{
    public enum Utility { Park = 0, Mall = 1, WindTurbine = 2}
    public enum Residence { Apartments = 0, ModernApartments = 1, Cabin = 2, EnviromentalHouse = 3, HighRise = 4, LuxuryResidence = 5 }
    public enum Upgrades { Caretaker = 0, SolarPanel = 1, Insulation = 2, Playground = 3, Charger = 4, Regulator = 5, None = 6}
    class BuildableTile
    {
        public int XSpot { get; set; }
        public int YSpot { get; set; }
        public Position Pos { get; set; }
        public int Value { get; set; }
        public Utility UtilityType {get; set;}
        public Residence ResidenceType { get; set; }
        public Upgrades UpgradeType { get; set; }
        public bool mall1Effect { get; set; }

        public bool mall2Effect { get; set; }

        public bool windTurbineEffect { get; set; }
        public bool parkEffect { get; set; }
        public bool hasBuilding { get; set; }



        public BuildableTile(int xSpot, int ySpot, int value)
        {
            XSpot = xSpot;
            YSpot = ySpot;
            Pos = new Position(ySpot, xSpot);
            Value = value;
            UpgradeType = Upgrades.None;
        }
    }

}
