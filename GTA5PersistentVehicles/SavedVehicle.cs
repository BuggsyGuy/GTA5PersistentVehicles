using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using GTA;
using GTA.Math;

namespace GTA5PersistentVehicles
{
    /// <summary>A serializable snapshot of one vehicle and its world location.</summary>
    [Serializable]
    public sealed class SavedVehicle
    {
        public int ModelHash { get; set; }
        public string ModelName { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Heading { get; set; }
        public int PrimaryColor { get; set; }
        public int SecondaryColor { get; set; }
        public string NumberPlate { get; set; }
        public float Health { get; set; }
        public float EngineHealth { get; set; }
        public float DirtLevel { get; set; }
        public DateTime SavedAtUtc { get; set; }
        [XmlIgnore] public Blip Blip { get; set; }
        [XmlIgnore] public Vehicle SpawnedVehicle { get; set; }

        public static SavedVehicle Capture(Vehicle vehicle)
        {
            return new SavedVehicle
            {
                ModelHash = vehicle.Model.Hash,
                ModelName = vehicle.Model.ToString(),
                X = vehicle.Position.X,
                Y = vehicle.Position.Y,
                Z = vehicle.Position.Z,
                Heading = vehicle.Heading,
                PrimaryColor = (int)vehicle.Mods.PrimaryColor,
                SecondaryColor = (int)vehicle.Mods.SecondaryColor,
                NumberPlate = vehicle.NumberPlate,
                Health = vehicle.Health,
                EngineHealth = vehicle.EngineHealth,
                DirtLevel = vehicle.DirtLevel,
                SavedAtUtc = DateTime.UtcNow
            };
        }

        public Vector3 Position { get { return new Vector3(X, Y, Z); } }
    }
}
