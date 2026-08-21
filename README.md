**Looking for a New Maintainer**

*Hey guys,*

*Unfortunately, I've gotten pretty frustrated with the game over the last few months, to the point where I uninstalled it. Performance is so bad on my PC that I can barely make any progress with a city of more than 30k citizens.*

*If you're interested in taking it over, please contact me through the Issues page.*

---


<img src="https://raw.githubusercontent.com/thiago-rcarvalho/cs2-parking-fee-control/refs/heads/main/pdx/pfc-logo.png" width="280"/>

# Cities: Skylines II — Parking Fee Control Mod

Parking Fee Control is a mod that lets you manage parking fees for buildings and districts, helping you keep rates always up-to-date and exactly as you want!
Detailed info on the Paradox mod page: [Parking Fee Control](https://mods.paradoxplaza.com/mods/134032/Windows)

## Translating (Contributing to Locale)

Translations are stored in JSON files inside the `cs-parking-fees/Locale/` folder. To contribute:

1. Open or create a file for your language (e.g., `pt-BR.json`, `en-US.json`).
2. Add or update the translation keys/values as needed.
3. Submit a pull request with your changes.

### Supported Languages

> <img src="https://purecatamphetamine.github.io/country-flag-icons/3x2/US.svg" width="20"/> en-US | <img src="https://purecatamphetamine.github.io/country-flag-icons/3x2/BR.svg" width="20"/> pt-BR | <img src="https://purecatamphetamine.github.io/country-flag-icons/3x2/DE.svg" width="20"/> de-DE (by @AndyStgt89) | <img src="https://purecatamphetamine.github.io/country-flag-icons/3x2/CN.svg" width="20"/> zh-HANS (by @AriadusTT)

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

## Acknowledgments

This mod was developed using as reference the excellent mods from these amazing modders: **yenyang, Bruceyboy24804, franzvz, TDW, DanielVNZ, Triton Supreme**.

Thank you for your work in the CS2 community ❤️

## License

Copyright (C) 2026 thiago-rcarvalho

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the [GNU General Public License](LICENSE) for more details.

This project incorporates a small amount of third-party MIT-licensed code; see [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for details.
