# GTA5PersistentVehicles

ScriptHookVDotNet 3 / .NET Framework 4.8 script for GTA V single-player.

## Build

1. Install Visual Studio 2022 with the **.NET desktop development** workload.
2. Copy `ScriptHookVDotNet3.dll` from your GTA V installation into this project's `lib` folder.
3. Open `GTA5PersistentVehicles.csproj` and build in Release mode.
4. Copy these files into the GTA V root folder:
   - `bin/Release/GTA5PersistentVehicles.dll`
   - `Settings.ini`

The script creates `Data/vehicles.xml` and `GTA5PersistentVehicles.log` beside the script DLL.

## Behavior

While the player is driving, the script tracks the player's vehicle. When the player exits, the vehicle's model, location, heading, colors, plate, health, engine health, and dirt level are saved. A blue short-range blip is created at its location. Saved vehicles are restored when the script loads. Press the configured key to remove all saved vehicle records, spawned restored vehicles, and blips.

## Compatibility

This targets ScriptHookVDotNet 3 and GTA V single-player. Enhanced builds may require the current compatible ScriptHookVDotNet 3 binary for that game build. Do not distribute Rockstar game files or ScriptHook binaries with this project.

## Notes

The script saves a vehicle when its driver exits. If the game or script is terminated before the exit event is observed, that vehicle may not be saved. Avoid editing `Data/vehicles.xml` while the game is running.
