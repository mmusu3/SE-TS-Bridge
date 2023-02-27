# SE-TS-Bridge
Plugins for Space Engineers and TeamSpeak to enable positional audio features.  
This project is early in development. It is lacking some functionality and may still have occasional crashing or freezing issues.  

## Installation
First download and unzip the `SE-TS_Bridge.zip` file from the latest release here https://github.com/mmusu3/SE-TS-Bridge/releases  
Inside there are two plugins. One for Space Engineers (SE) and one for TeamSpeak (TS). Their names start with the program abbreviation that they apply to.  
- The one for SE is called `SE-TS_Plugin.dll`.
- The one for TS is called `TS-SE_Plugin.dll`.

The naming can be confusing so make sure not to mix them up.  

#### Space Engineers Plugin
* First you may need to unblock the `SE-TS_Plugin.dll` file in order for it to work. To do so, right click the file and click Properties. At the bottom of the menu if you see a Security section with a checkbox that says Unblock, check it and press the OK button.  
* Next you need to locate your Space Engineers install directory. The easiest way to do so is to first open the game properties menu for Space Engineers in Steam. Then in the `Local Files` tab click the `Browse` button. This will take you to straight to the install folder.  
* Once there open the `Bin64` folder. Here, if you don't already have one, you may want to add a `Plugins` folder for better organization.  
* Now copy the `SE-TS_Plugin.dll` file from the unziped release into the `Plugins` folder.  
* To use the plugin you will need to change the Space Engineers launch options which are located back in the same Steam game properties menu in the `General` tab.  
* If you are not using any other plugins simply add `-plugin ./Plugins/SE-TS_Plugin.dll` to the launch options box.  
* If you are also using other plugins, instead add `./Plugins/SE-TS_Plugin.dll` after the previous plugins paths. Eg. `-plugin ./Plugins/otherplugin.dll ./Plugins/SE-TS_Plugin.dll`  
* If SE is already running you will need to restart it for the plugin to load.  

#### TeamSpeak Plugin
* This plugin will only work with the 64bit version of TeamSpeak so be sure you have that instead of the 32bit version.  
* To install the TS plugin, first copy the `TS-SE_Plugin.dll` file into your TeamSpeak plugins folder which can usually be found at `%appdata%/TS3Client/plugins`.  
* To enable the plugin go to TeamSpeak Options->Addons->Plugins and ensure TS-SE Plugin is enabled there (the button should say Enabled).  
* To enable positional audio support go to TeamSpeak Options->Playback and ensure the `Always set clients 3D positions when available` checkbox is ticked.  

When updating either plugin, first close the respective application before re-copying the plugin files.  

##### Commands
The Teamspeak plugin has a couple of chat based commands to control the audio settings.

* `setsbridge distancescale (x)`
* `setsbridge distancefalloff (x)`

Where (x) is a number.

Default values:  
* `distancescale 0.3`
* `distancefalloff 0.9`

These commands may be restricted in future releases.

#### Space Engineers Server Mod
These plugins can also work togther with a server mod for SE. The mod makes Teamspeak whispers only audible by players of the same faction from players with an active antenna connection.
The server mod is linked here: https://steamcommunity.com/sharedfiles/filedetails/?id=2935694294
