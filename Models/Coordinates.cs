﻿using Sovereign.Models;

namespace Sovereign
{
    public class Coordinates
    {
        public Heading PointOfAim;

        public Coordinates(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int Y;
        public int X;

        public bool Equals(Coordinates ToValidate)
        {
            return this.X == ToValidate.X && this.Y == ToValidate.Y;
        }
    }
}