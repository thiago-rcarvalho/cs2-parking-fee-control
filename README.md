# Cities: Skylines II — Parking Fee Control Mod

This repository contains the Parking Fee Control mod for Cities: Skylines II.

## About the mod

Parking Fee Control is a mod that lets you manage parking fees for buildings, helping you keep rates always up-to-date and exactly as you want!
Detailed info on the Paradox mod page: [Parking Fee Control]()

## Translating (Contributing to Locale)

Translations are stored in JSON files inside the `cs-parking-fees/Locale/` folder. To contribute:

1. Open or create a file for your language (e.g., `pt-BR.json`, `en-US.json`).
2. Add or update the translation keys/values as needed.
3. Submit a pull request with your changes.

### Supported Languages

> <img src="https://purecatamphetamine.github.io/country-flag-icons/3x2/US.svg" width="20"/> en-US | <img src="https://purecatamphetamine.github.io/country-flag-icons/3x2/BR.svg" width="20"/> pt-BR | <img src="https://purecatamphetamine.github.io/country-flag-icons/3x2/DE.svg" width="20"/> de-DE (by @AndyStgt89)

## Adding Compatibility for Other Mods

You can add compatibility with other mods by editing `cs-parking-fees/parking-data.json`:

1. Find the desired mod ID on the Paradox mods page (website/in-game) or in Skyve.
2. Note the prefab name for each parking asset in the mod (use the Scene Explorer mod or Asset Editor to get these names).
3. Open `cs-parking-fees/parking-data.json` and add the prefab names along with the mod ID.
4. Submit a pull request with your changes.



## Before Build this mod

**Configure your environment:**
- Create a file named `local.envs` in the project root (same folder as `compile.sh`). Example:
	```env
	export GAME_MODS_DIR="/path/to/your/Mods/ParkingFeeControl"
	export CSII_USERDATAPATH="/path/to/your/AppData/LocalLow/Collosal Order/Cities Skylines 2"
	export BUILD_DIR="/bin/Debug/net472/"
	```
- This file is ignored by git and allows each user to set their own mod output path.
- For C# dependencies, if you need to override library paths, create or edit `cs-parking-fees/Directory.Build.local.props`:
	```xml
	<Project>
		<PropertyGroup>
			<MANAGED_DLLS_PATH>/path/to/your/libs</MANAGED_DLLS_PATH>
		</PropertyGroup>
	</Project>
	```
**Build**

If you are using Linux, use the `compile.sh` script. But if you are using Windows, simply run your IDE build command.

**Publish**

Publish can only be made on Windows through the IDE publish menu.