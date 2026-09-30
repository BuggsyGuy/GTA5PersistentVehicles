using System;
using System.IO;
using System.Reflection;
using GTA;

[assembly: AssemblyTitle("GTA5PersistentVehicles")]
[assembly: AssemblyDescription("Persistent vehicle storage and map markers for GTA V single-player.")]
[assembly: AssemblyVersion("1.0.0.0")]

namespace GTA5PersistentVehicles
{
    /// <summary>Entry point loaded by ScriptHookVDotNet.</summary>
    public sealed class PersistentVehiclesScript : Script
    {
        private readonly Configuration _configuration;
        private readonly VehiclePersistenceService _persistence;
        private Vehicle _vehicleOccupiedOnLastTick;
        private bool _wasInVehicle;

        public PersistentVehiclesScript()
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppDomain.CurrentDomain.BaseDirectory;
            _configuration = Configuration.Load(Path.Combine(directory, "Settings.ini"));
            _persistence = new VehiclePersistenceService(Path.Combine(directory, "Data", "vehicles.xml"), _configuration.MaxSavedVehicles);

            Tick += OnTick;
            KeyDown += OnKeyDown;
            Aborted += OnAborted;

            if (_configuration.Enabled)
            {
                _persistence.RestoreVehicles();
                UI.Notify("~b~Persistent Vehicles~s~ enabled.");
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!_configuration.Enabled)
                return;

            Ped player = Game.Player.Character;
            bool isInVehicle = player != null && player.Exists() && player.IsInVehicle();

            if (isInVehicle)
            {
                Vehicle vehicle = player.CurrentVehicle;
                if (vehicle != null && vehicle.Exists() && vehicle.Driver == player)
                    _vehicleOccupiedOnLastTick = vehicle;
            }
            else if (_wasInVehicle && _vehicleOccupiedOnLastTick != null && _vehicleOccupiedOnLastTick.Exists())
            {
                _persistence.SaveVehicle(_vehicleOccupiedOnLastTick);
                _vehicleOccupiedOnLastTick = null;
            }

            _wasInVehicle = isInVehicle;
        }

        private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (_configuration.Enabled && e.KeyCode == _configuration.ClearVehiclesKey)
            {
                _persistence.Clear();
                UI.Notify("~y~Persistent vehicle list cleared.");
            }
        }

        private void OnAborted(object sender, EventArgs e)
        {
            _persistence.Dispose();
        }
    }
}
