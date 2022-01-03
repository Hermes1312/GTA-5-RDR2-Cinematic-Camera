﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using NativeUI;
using System.Timers;
using GTA.Native;
using Timer = System.Windows.Forms.Timer;
using Control = GTA.Control;

namespace Rdr2CinematicCamera
{
    public class Main : Script
    {
        // Menu
        private MenuPool _menuPool;
        private UIMenu _uiMenu;

        // Variables
        private bool _firstTime = true;
        private bool _isActive = false;

        private const string ModName = "RDR2 Cinematic Camera";
        private const string Developer = "Hermes";
        private const string Version = "1.0";

        private readonly UIRectangle[] _cinematicBars = new UIRectangle[2]; 
        private readonly List<DrivingStyle> _drivingStyles;
        private DrivingStyle _drivingStyle = DrivingStyle.Normal;

        private int _speed = 50;
        private Vector3 _currentDestination;
        private readonly Stopwatch _holdStopwatch = new Stopwatch();
        private bool alreadyClear = false, forceCinCam = false, forceCinCam2 = false;

        public Main()
        {
            _drivingStyles = new List<DrivingStyle>()
            {
                DrivingStyle.AvoidTraffic,
                DrivingStyle.AvoidTrafficExtremely,
                DrivingStyle.IgnoreLights,
                DrivingStyle.Normal,
                DrivingStyle.Rushed,
                DrivingStyle.SometimesOvertakeTraffic
            };

            SetupCinematicBars();
            ReadConfig();
            SetupMenu();

            Tick += OnTick;
            KeyDown += OnKeyDown;
            Interval = 1;
        }

        private void ReadConfig()
        {
            if (File.Exists("scripts\\Rdr2CinematicCamera.cfg"))
            {
                var cfgLines = File.ReadAllLines("scripts\\Rdr2CinematicCamera.cfg");
                _drivingStyle = _drivingStyles[Convert.ToInt16(cfgLines[0])];
                _speed = Convert.ToInt16(cfgLines[1]);
            }
        }

        private void SetupCinematicBars()
        {
            var y = Game.ScreenResolution.Height - 360;

            _cinematicBars[0] =
                new UIRectangle(new Point(0, -100), new Size(Game.ScreenResolution.Width, 100), Color.Black);

            _cinematicBars[1] =
                new UIRectangle(new Point(0, y), new Size(Game.ScreenResolution.Width, 100), Color.Black);
        }

        private int GetIndexFromEnum(DrivingStyle drivingStyle)
        {
            for (var i = 0; i < _drivingStyles.Count; i++)
                if (_drivingStyles[i] == drivingStyle)
                    return i;

            return 3;
        }

        private void SetupMenu()
        {
            _menuPool = new MenuPool();
            _uiMenu = new UIMenu("Cinematic Camera", "Like in Red Dead Redemption 2");
            _menuPool.Add(_uiMenu);


            var listOfDrivingStyles = new List<object>
            {
                "Avoid Traffic",
                "Avoid Traffic Extremely",
                "Ignore Lights",
                "Normal",
                "Rushed",
                "Sometimes Overtake Traffic"
            };

            var menuDrivingStyles = new UIMenuListItem("Driving style: ", listOfDrivingStyles, 0);
            _uiMenu.AddItem(menuDrivingStyles);
            menuDrivingStyles.Index = GetIndexFromEnum(_drivingStyle);

            var menuSpeed = new UIMenuSliderItem("Speed: ")
            {
                Maximum = 250
            };

            menuSpeed.Value = _speed;

            _uiMenu.AddItem(menuSpeed);

            var saveButton = new UIMenuItem("Save changes");
            _uiMenu.AddItem(saveButton);

            _uiMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == saveButton)
                {
                    File.WriteAllText("scripts\\Rdr2CinematicCamera.cfg",
                        $"{menuDrivingStyles.Index}{Environment.NewLine}{menuSpeed.Value}");
                    _drivingStyle = _drivingStyles[menuDrivingStyles.Index];
                    _speed = menuSpeed.Value;

                    if (_currentDestination != Vector3.Zero)
                        Game.Player.Character.Task.DriveTo(Game.Player.Character.CurrentVehicle, _currentDestination,
                            50.0f, _speed, (int) _drivingStyle);
                }
            };
        }


        private void OnTick(object sender, EventArgs e)
        {
            if(forceCinCam)
                Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, true);

            if (_isActive && Game.IsControlPressed(2, Control.NextCamera))
                forceCinCam = false;

            if (Game.IsControlPressed(2, Control.VehicleCinCam))
            {
                if (!_isActive)
                    forceCinCam2 = false;

                if (!_holdStopwatch.IsRunning)
                    _holdStopwatch.Start();

                if (_holdStopwatch.ElapsedMilliseconds > 1000)
                {
                    forceCinCam2 = true;
                    CinematicDriveToWaypoint();

                    _holdStopwatch.Stop();
                    _holdStopwatch.Reset();
                }
            }

            if (Game.IsControlJustReleased(2, Control.VehicleCinCam))
            {
                _holdStopwatch.Stop();
                _holdStopwatch.Reset();

                if (_isActive)
                    forceCinCam = true;
            }

            if (Game.IsControlPressed(2, Control.VehicleHandbrake) && Game.IsControlPressed(2, Control.VehicleDuck))
            {
                ToggleMenu();
                Thread.Sleep(10);
            }

            if (_firstTime)
            {
                UI.Notify(ModName + " " + Version + " by " + Developer + " Loaded");
                _firstTime = false;
            }

            if (_isActive)
            {
                alreadyClear = false;
                _isActive = Game.IsWaypointActive;
                
                if(_isActive)
                    _currentDestination = World.GetWaypointPosition();

                if (_cinematicBars[0].Position.Y < 0)
                {
                    var animSpeed = 2;

                    _cinematicBars[0].Position =
                        new Point(_cinematicBars[0].Position.X, _cinematicBars[0].Position.Y + animSpeed);

                    _cinematicBars[1].Position =
                        new Point(_cinematicBars[1].Position.X, _cinematicBars[1].Position.Y - animSpeed);
                }
                _cinematicBars[0].Draw();
                _cinematicBars[1].Draw();

                Function.Call(Hash.DISPLAY_RADAR, false);
            }

            else
            {
                if (_cinematicBars[0].Position.Y > -100)
                {
                    const int animSpeed = 2;

                    _cinematicBars[0].Position =
                        new Point(_cinematicBars[0].Position.X, _cinematicBars[0].Position.Y - animSpeed);
                    _cinematicBars[1].Position =
                        new Point(_cinematicBars[1].Position.X, _cinematicBars[1].Position.Y + animSpeed);

                    _cinematicBars[0].Draw();
                    _cinematicBars[1].Draw();
                }

                if (!alreadyClear)
                {
                    Game.Player.Character.Task.ClearAll();
                    alreadyClear = true;
                }

                if(forceCinCam2)
                    Function.Call(Hash.SET_CINEMATIC_MODE_ACTIVE, false);

                Function.Call(Hash.DISPLAY_RADAR, true);
            }

            _menuPool?.ProcessMenus();
        }

        private void ToggleMenu()
            => _uiMenu.Visible = !_uiMenu.Visible;

        private void CinematicDriveToWaypoint()
        {
            if (Game.Player.Character.CurrentVehicle != null)
            {
                if (!_isActive)
                {
                    if (Game.IsWaypointActive)
                    {
                        UI.Notify("Driving started!");
                        _currentDestination = World.GetWaypointPosition();

                            Game.Player.Character.Task.DriveTo(Game.Player.Character.CurrentVehicle,
                            (Vector3) _currentDestination, 25.0f, _speed, (int) _drivingStyle);
                    }
                }

                else
                {
                    UI.Notify("Driving stopped!");
                    Game.Player.Character.Task.Wait(1);
                }

                _isActive = !_isActive;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F10)
                ToggleMenu();
        }
    }
}