using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using Control = GTA.Control;

namespace Rdr2CinematicCamera
{
    public class Main : Script
    {
        // Variables
        private bool _firstTime = true;

        private const string ModName = "RDR2 Cinematic Camera";
        private const string Developer = "Hermes";
        private const string Version = "1.2";

        private readonly Menu _menu;
        private readonly CinematicBars _cinematicBars;
        private readonly Stopwatch _holdStopwatch = new Stopwatch();
        private int _pressedCounter;
        private bool _debug = false;
        private Vector3 _currentDestination;

        public Main()
        {
            _cinematicBars = new CinematicBars();
            Global.CinematicBars = _cinematicBars;

            Global.Config = new Config();
            _menu = new Menu(Global.Config);

            Tick += OnTick;
            KeyDown += OnKeyDown;
            Interval = 1;
        }


        private void OnTick(object sender, EventArgs e)
        {
            if (_debug)
            {
                if (Game.IsWaypointActive)
                {
                    var wpos = World.WaypointPosition;
                    new TextElement($"X: {wpos.X}\nY: {wpos.Y}\nZ: {wpos.Z}\n", new PointF(0, 300f), 0.35f, Color.Beige).Draw();
                }
            }

            //new TextElement(_pressedCounter.ToString(), Point.Empty, 1.0f).Draw();
            if (!Global.Config.Enabled) return;
            
            if (Global.ForceCinCam)
                Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, true);

            if (Global.IsActive && Game.IsControlPressed(Control.NextCamera))
                Global.ForceCinCam = false;

            if (Game.IsControlJustPressed(Control.VehicleCinCam))
                _pressedCounter++;

            if (Game.IsControlPressed(Control.VehicleCinCam))
            {
                if (!Global.IsActive)
                    Global.ForceCinCam2 = false;

                if (!_holdStopwatch.IsRunning)
                    _holdStopwatch.Start();

                if (_holdStopwatch.ElapsedMilliseconds > 1000 && _pressedCounter == 1)
                {
                    Global.ForceCinCam2 = true;

                    if (Game.Player.Character.CurrentVehicle.Type != VehicleType.Helicopter &&
                        Game.Player.Character.CurrentVehicle.Type != VehicleType.Plane)
                        if (Game.IsWaypointActive)
                            CinematicDriveToWaypoint();
                        else
                            CinematicCruising();
                    else
                        Global.IsActive = !Global.IsActive;

                    _holdStopwatch.Stop();
                    _holdStopwatch.Reset();
                    _pressedCounter = 0;
                }

                if (_holdStopwatch.ElapsedMilliseconds < 1000 && Global.SameHold && _pressedCounter == 1)
                    if (Global.IsActive) _cinematicBars.DecreaseY(2);
                    else _cinematicBars.IncreaseY(2);

                Global.SameHold = true;
            }

            if (Game.IsControlJustReleased(Control.VehicleCinCam))
            {
                if (_holdStopwatch.ElapsedMilliseconds < 1000)
                    if (Global.IsActive)
                        _cinematicBars.Setup(1);
                    else
                        _cinematicBars.DecreaseY(2);

                _holdStopwatch.Stop();
                _holdStopwatch.Reset();

                Global.ForceCinCam = Global.IsActive;
                Global.SameHold = false;
                _pressedCounter = 0;
            }


            if (Game.IsControlJustReleased(Control.VehicleHandbrake) &&
                Game.IsControlJustReleased(Control.VehicleDuck))
                _menu.Toggle();


            if (_firstTime)
            {
                Notification.Show(ModName + " " + Version + " by " + Developer + " Loaded");
                _firstTime = false;
            }

            if (Global.IsDriving && Game.Player.Character.Position.DistanceTo(_currentDestination) < 30)
            {
                Global.IsDriving = false;
                Global.IsActive = false;
            }

            if (Global.IsActive)
            {
                Function.Call(Hash.DISPLAY_RADAR, false); 
                Global.AlreadyCleared = false;
            }

            else
            {
                if (!Global.AlreadyCleared)
                {
                    Game.Player.Character.Task.ClearAll();
                    Global.AlreadyCleared = true;
                }

                if (!Global.SameHold)
                    _cinematicBars.DecreaseY(2);
                    
                if (Global.ForceCinCam2)
                    Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, false);

                Function.Call(Hash.DISPLAY_RADAR, true);
            }

            _menu.ProcessMenus();

            if (Global.Config.CinematicBars && Game.Player.Character.CurrentVehicle != null)
                _cinematicBars.Draw();
        }

        private static void CinematicCruising()
        {
            if (Game.Player.Character.CurrentVehicle == null) return;

            if (!Global.IsActive && !Game.IsWaypointActive)
                Game.Player.Character.Task.CruiseWithVehicle(Game.Player.Character.CurrentVehicle, Global.Config.Speed, Global.Config.DrivingStyle);
            
            else
                Game.Player.Character.Task.ClearAll();

            Global.IsActive = !Global.IsActive;
            Global.IsCruising = !Global.IsCruising;
        }

        public void CinematicDriveToWaypoint()
        {
            if (Game.Player.Character.CurrentVehicle == null) return;

            if (!Global.IsActive && Game.IsWaypointActive)
            {
                _currentDestination = World.WaypointPosition;

                Game.Player.Character.Task.DriveTo
                (
                    Game.Player.Character.CurrentVehicle,
                    _currentDestination,
                    25.0f,
                    Global.Config.Speed,
                    Global.Config.DrivingStyle
                );
            }

            else
                Game.Player.Character.Task.ClearAll();
            
            Global.IsActive = !Global.IsActive;
            Global.IsDriving = !Global.IsDriving;
        }
        
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.KeyCode == Keys.F10)
                _menu.Toggle();
        }
    }
}