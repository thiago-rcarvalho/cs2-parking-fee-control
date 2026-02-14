# About
⚠️ Currently in BETA (please read the Technical Notes) ⚠️ 

Parking Fee Control is a mod that lets you manage parking fees for buildings and districts, helping you keep rates always up-to-date and exactly as you want!

After installing it, you'll see a Parking icon on the top-left side of the screen. Click it to access the fees screen.

Features:
* Customize fees for multiple **parking lots** and **districts** (values range from 0 to 50). Fees can be applied instantly via the configuration panel and also periodically, as configured.
* Periodically updates the fees for parking lots added to the map during gameplay, keeping everything consistent.
* You can ignore fee application for parking lots that have the following tags in their name: **[nofee], [npf], or |npf|** (configurable). Just add one of these tags to the building's name.
* Since this mod does not alter your save, it can be added or removed at any time without issues.
* Compatible with Vanilla parking lots and many other mods (see list below).
* Supported languages: en-us, pt-br, de-DE (contribute with translations on the project's GitHub)


Parking Fee Control is currently in BETA. Bugs may occur; fee application performance is still being validated, but so far no bottlenecks have been observed.

## Technical Notes
1. District fees are persisted based on the district name due to the way the game works; If you rename it, the fee will reset to the category default, as I couldn't find a fixed internal ID for it yet;
2. Since it was difficult to find an automatic way to detect parking lot mods, I chose a simpler approach: mods are mapped directly in the code. If your favorite parking mod is not yet supported, just open a pull request or an issue on the project's GitHub. The README contains all necessary information.
3. Periodic updates may be removed in the future if I find a way to intercept the placement of parking lots and change the default fee before the player places them on the map. Still working on it;

----

# Compatible MODS
- **Parking Lots By Dome** by De Magistris
- **Overground Parking By Dome** by De Magistris
- **Underground Parkings 01 / 02** by Gruny
- **Small Underground Parking Building** by Mimonsi
- **Small Underground Parking Lots** by zaynedarmoset
- **Small Underground Parkinglot** by XXThomas
- **DVNZ - Parking Lots with Parks** by DanielVNZ
- **Multi-storey Parking System** by MagicMenma
- **Winter Garage** by gog2o
- **Narrow parking lot pack_AK** by confusedlildevil
- **Jack's Decorated Parking** by marksmango
- **8x3 Parking Lot** by GrumpyZade
- **[G87] Filler Parkings** by elGendo87
- **Small parking lot pack** by confusedlildevil
- **Big BicycleParking** by 2nPlace

----
# Thank you

This mod was developed using as reference the excellent mods from these amazing modders:
**yenyang, Bruceyboy24804, franzvz, TDW, DanielVNZ, Triton Supreme**

Thank you for your work in the CS2 community ❤️

## Translations Credits

de-DE: **AndyStgt89**

----
# Bugs and suggestions
Please file an issue on:

Project GitHub: https://github.com/thiago-rcarvalho/cs2-parking-fee-control

## Known bugs
* Not displaying the name and thumbnails of a small percentage of buildings correctly on the fees screen