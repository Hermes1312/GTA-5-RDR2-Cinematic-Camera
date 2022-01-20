using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;

namespace Rdr2CinematicCamera
{
    public static class Airport
    {
        public static bool IsLanding = false;
        public static Vector3 LSA1       = new Vector3(-1465.082f, -2425.991f, 0f);
        public static Vector3 LSA0       = new Vector3(-773.5446f, -1459.155f, 0f);
        public static Vector3 TrevorA0   = new Vector3(2219.578f, 3426.11f, 50f);
        public static Vector3 TrevorA1   = new Vector3(11619.344f,3228.664f,0f);
        public static Vector3 McKenzieA0 = new Vector3(1618.074f, 4623.038f, 0f);
        public static Vector3 McKenzieA1 = new Vector3(1967.584f, 4735.401f, 0f);

        public static void Land(int airportIndex)
        {
            if (!IsLanding)
            {
                switch (airportIndex)
                {
                    case 0:
                        Game.Player.Character.Task.LandPlane(LSA0.IncreaseZ(100), LSA1);
                        break;

                    case 1:
                        Game.Player.Character.Task.LandPlane(TrevorA0, TrevorA1);
                        break;

                    case 2:
                        Game.Player.Character.Task.LandPlane(McKenzieA0, McKenzieA1);
                        break;
                }

                IsLanding = true;
            }
            else
            {
                Game.Player.Character.Task.ClearAll();
                IsLanding = false;
            }
        }

        private static Vector3 IncreaseZ(this Vector3 vector, int i)
            => new Vector3(vector.X, vector.Y, vector.Z+i);
    }
}

/*

2384.971f,3472.285f,55.66455f
1492.958f,3198.145f,0f
1619.878f,4625.831f,52.71381f
2028.533f,4760.589f,0f */