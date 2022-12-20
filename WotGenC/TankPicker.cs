using System;
using System.Collections.Generic;

namespace WotGenC
{
    public static class TankPicker
    {
        public static Tank PickARandomTank(List<Tank> tanks)
        {
            if (tanks.Count == 0)
            {
                return null;
            }

            Random rng = new Random();
            return tanks[rng.Next(0, tanks.Count)];
        }

    }
}