using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;

namespace Rdr2CinematicCamera
{
    public class Config
    {
        private const string ConfigPath = "scripts\\Rdr2CinematicCamera.ini";
        public readonly ScriptSettings ScriptSettings;

        public readonly List<DrivingStyle> DrivingStyles;
        public DrivingStyle DrivingStyle { get; set; } = DrivingStyle.Normal;
        public int Speed { get; set; } = 50;
        public bool CinematicBars { get; set; } = true;
        public bool Enabled { get; set; } = true;

        public Config()
        {
            DrivingStyles = new List<DrivingStyle>()
            {
                DrivingStyle.AvoidTraffic,
                DrivingStyle.AvoidTrafficExtremely,
                DrivingStyle.IgnoreLights,
                DrivingStyle.Normal,
                DrivingStyle.Rushed,
                DrivingStyle.SometimesOvertakeTraffic
            };


            if (File.Exists(ConfigPath))
            {
                ScriptSettings = ScriptSettings.Load(ConfigPath);

                Speed = ScriptSettings.GetValue("GLOBAL", "SPEED", 50);
                DrivingStyle = DrivingStyles[Convert.ToInt16(ScriptSettings.GetValue("GLOBAL", "DRIVING_STYLE", 3))];
                CinematicBars = ScriptSettings.GetValue("GLOBAL", "CINEMATIC_BARS", true);
                Enabled = ScriptSettings.GetValue("GLOBAL", "ENABLED", true);
            }

            else
            {
                File.CreateText(ConfigPath);

                ScriptSettings = ScriptSettings.Load(ConfigPath);

                ScriptSettings.SetValue("GLOBAL", "SPEED", 50);
                ScriptSettings.SetValue("GLOBAL", "DRIVING_STYLE", 3);
                ScriptSettings.SetValue("GLOBAL", "CINEMATIC_BARS", true);
                ScriptSettings.SetValue("GLOBAL", "ENABLED", true);

                ScriptSettings.Save();
            }
        }   
        private int GetIndexFromEnum(DrivingStyle drivingStyle)
        {
            for (var i = 0; i < DrivingStyles.Count; i++)
                if (DrivingStyles[i] == drivingStyle)
                    return i;
            return 3;
        }

        public void Save() 
        {
            ScriptSettings.SetValue("GLOBAL", "SPEED", Speed);
            ScriptSettings.SetValue("GLOBAL", "DRIVING_STYLE", GetIndexFromEnum(DrivingStyle));
            ScriptSettings.SetValue("GLOBAL", "CINEMATIC_BARS",  CinematicBars);
            ScriptSettings.SetValue("GLOBAL", "ENABLED", Enabled);

            ScriptSettings.Save();
        }
    }
}
