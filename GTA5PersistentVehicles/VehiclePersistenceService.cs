using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using GTA;
using GTA.Math;
using GTA.Native;

namespace GTA5PersistentVehicles
{
    /// <summary>Owns saved vehicle data, persistence, restoration, and map blips.</summary>
    internal sealed class VehiclePersistenceService : IDisposable
    {
        private readonly string _filePath;
        private readonly int _maximum;
        private readonly List<SavedVehicle> _vehicles = new List<SavedVehicle>();

        public VehiclePersistenceService(string filePath, int maximum)
        {
            _filePath = filePath;
            _maximum = Math.Max(1, maximum);
            Load();
        }

        public void SaveVehicle(Vehicle vehicle)
        {
            if (vehicle == null || !vehicle.Exists() || vehicle.IsDead)
                return;

            SavedVehicle snapshot = SavedVehicle.Capture(vehicle);
            SavedVehicle existing = _vehicles.FirstOrDefault(v => v.NumberPlate == snapshot.NumberPlate && Vector3.Distance(v.Position, snapshot.Position) < 8f);
            if (existing != null)
            {
                RemoveBlip(existing);
                _vehicles.Remove(existing);
            }

            while (_vehicles.Count >= _maximum)
            {
                RemoveBlip(_vehicles[0]);
                _vehicles.RemoveAt(0);
            }

            _vehicles.Add(snapshot);
            Save();
            CreateBlip(snapshot);
            UI.Notify("~g~Vehicle saved.~s~ " + _vehicles.Count + "/" + _maximum);
        }

        public void RestoreVehicles()
        {
            foreach (SavedVehicle saved in _vehicles.ToList())
            {
                try
                {
                    Model model = new Model(saved.ModelHash);
                    if (!model.IsInCdImage || !model.IsVehicle)
                        continue;
                    model.Request(1000);
                    if (!model.IsLoaded)
                        continue;

                    Vehicle vehicle = World.CreateVehicle(model, saved.Position, saved.Heading);
                    if (vehicle == null || !vehicle.Exists())
                        continue;
                    vehicle.IsPersistent = true;
                    vehicle.NumberPlate = saved.NumberPlate ?? string.Empty;
                    vehicle.Health = saved.Health;
                    vehicle.EngineHealth = saved.EngineHealth;
                    vehicle.DirtLevel = saved.DirtLevel;
                    saved.SpawnedVehicle = vehicle;
                    model.MarkAsNoLongerNeeded();
                    CreateBlip(saved);
                }
                catch (Exception ex)
                {
                    Logger.Write("Restore failed: " + ex.Message);
                }
            }
        }

        public void Clear()
        {
            foreach (SavedVehicle saved in _vehicles)
            {
                RemoveBlip(saved);
                if (saved.SpawnedVehicle != null && saved.SpawnedVehicle.Exists())
                    saved.SpawnedVehicle.Delete();
            }
            _vehicles.Clear();
            Save();
        }

        private void CreateBlip(SavedVehicle saved)
        {
            RemoveBlip(saved);
            saved.Blip = World.CreateBlip(saved.Position);
            if (saved.Blip != null && saved.Blip.Exists())
            {
                saved.Blip.Name = "Saved Vehicle";
                saved.Blip.IsShortRange = true;
                saved.Blip.Color = BlipColor.Blue;
                saved.Blip.Scale = 0.8f;
            }
        }

        private static void RemoveBlip(SavedVehicle saved)
        {
            if (saved.Blip != null && saved.Blip.Exists())
                saved.Blip.Remove();
            saved.Blip = null;
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return;
                using (FileStream stream = File.OpenRead(_filePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<SavedVehicle>));
                    List<SavedVehicle> loaded = serializer.Deserialize(stream) as List<SavedVehicle>;
                    if (loaded != null)
                        _vehicles.AddRange(loaded.Take(_maximum));
                }
            }
            catch (Exception ex) { Logger.Write("Load failed: " + ex.Message); }
        }

        private void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                string temporary = _filePath + ".tmp";
                using (FileStream stream = File.Create(temporary))
                {
                    new XmlSerializer(typeof(List<SavedVehicle>)).Serialize(stream, _vehicles);
                }
                if (File.Exists(_filePath)) File.Delete(_filePath);
                File.Move(temporary, _filePath);
            }
            catch (Exception ex) { Logger.Write("Save failed: " + ex.Message); }
        }

        public void Dispose()
        {
            foreach (SavedVehicle saved in _vehicles) RemoveBlip(saved);
        }
    }

    internal static class Logger
    {
        private static readonly string PathName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GTA5PersistentVehicles.log");
        public static void Write(string message) { try { File.AppendAllText(PathName, DateTime.Now + " " + message + Environment.NewLine); } catch { } }
    }
}
