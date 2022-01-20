using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdr2CinematicCamera
{
    public static class Global
    {
        public static Config Config { get; set; }
        public static CinematicBars CinematicBars { get; set; }
        public static bool IsActive { get; set; } = false;
        public static bool IsCruising { get; set; } = false;
        public static bool IsDriving { get; set; } = false;
        public static bool ForceCinCam { get; set; } = false;
        public static bool ForceCinCam2 { get; set; } = false;
        public static bool SameHold { get; set; } = false;
        public static bool AlreadyCleared { get; set; } = false;
    }
}

/*
Global.IsActive       = false;
Global.IsCruising     = false;
Global.IsDriving      = false;
Global.ForceCinCam    = false;
Global.ForceCinCam2   = false;
Global.SameHold       = false;
Global.AlreadyCleared = false;
 */