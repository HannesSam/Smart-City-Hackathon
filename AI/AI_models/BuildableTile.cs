﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.models
{
    class BuildableTile
    {
        public int XSpot { get; set; }
        public int YSpot { get; set; }
        public int Value { get; set; }

        public BuildableTile(int xSpot, int ySpot, int value)
        {
            XSpot = xSpot;
            YSpot = ySpot;
            Value = value;
        }
    }

}
