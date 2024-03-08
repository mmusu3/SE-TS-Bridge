# SE-TS-Bridge
Plugins for Space Engineers and TeamSpeak to enable positional audio features.  
This project has not had extensive development and so it is lacking some functionality. Please report any crashing or freezing issues.  

## Installation
First download and unzip the `SE-TS_Bridge.zip` file from the latest release here https://github.com/mmusu3/SE-TS-Bridge/releases  
Inside there are two plugins. One for Space Engineers (SE) and one for TeamSpeak (TS).  
- The one for Space Engineers is called `TSPluginForSE.dll`.
- The one for TeamSpeak is called `SEPluginForTS.dll`.

Each one is different and will only work for the specified program so make sure not to mix them up.  

#### Space Engineers Plugin
* Since SE version `1.203.022` using plugins now requires the PluginLoader launcher. Visit https://github.com/sepluginloader/SpaceEngineersLauncher for downloads and installation instructions.  
* Once you have the launcher installed you can add the plugin file for SE-TS Bridge.
* First you may need to unblock the `TSPluginForSE.dll` file in order for it to work. To do so, right click the file and click Properties. At the bottom of the menu if you see a Security section with a checkbox that says Unblock, check it and press the OK button.  
* Next you need to locate your Space Engineers install directory. The easiest way to do so is to first open the game properties menu for Space Engineers in Steam. Then in the `Local Files` tab click the `Browse` button. This will take you to straight to the install folder.  
* Once there open the `Bin64` folder. Here, if you don't already have one, make a folder named `Plugins` and another folder inside that named `local`.  
* Now copy the `TSPluginForSE.dll` file from the unziped release into the `Plugins\local` folder.  
* Launch SE using the PluginLoader launcher (`SpaceEngineersLauncher.exe`) and click the Plugins button on the main menu.  
* Click the `Add Plugin` button near the bottom center of the menu (looks like a plus sign) and then serch for `TSPluginForSE`.  
* Mark the checkbox on `TSPluginForSE.dll` and then close the `Plugin List` menu.  
* Click the `Apply` button and confirm the restart when prompted.  
* Once the game has restarted the plugin for SE should now be ready for use.  

#### TeamSpeak Plugin
* This plugin will only work with the 64bit version of TeamSpeak so be sure you have that instead of the 32bit version.  
* To install the TS plugin, first copy the `SEPluginForTS.dll` file into your TeamSpeak plugins folder which can usually be found at `%appdata%/TS3Client/plugins`.  
* To enable the plugin go to TeamSpeak Options->Addons->Plugins and ensure the `SE-TS Bridge` plugin is enabled there (the button should say Enabled).  
* To enable positional audio support go to TeamSpeak Options->Playback and ensure the `Always set clients 3D positions when available` checkbox is ticked.  

When updating either plugin, first close the respective application before re-copying the plugin files.  

##### Commands
The Teamspeak plugin has a couple of chat based commands to control the audio settings.

* `/setsbridge distancescale (x)`
* `/setsbridge distancefalloff (x)`
* `/setsbridge maxdistance (x)`

Where (x) is a number.

Default values:  
* `distancescale 0.05`
* `distancefalloff 2`
* `maxdistance 150`

These commands may be restricted or removed in future releases.

#### Space Engineers Server Mod
These plugins can also work togther with a server mod for SE. The mod makes Teamspeak whispers only audible by players of the same faction from players with an active antenna connection.
The server mod is linked here: https://steamcommunity.com/sharedfiles/filedetails/?id=2935694294
