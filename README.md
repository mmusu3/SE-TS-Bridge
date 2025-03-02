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
* Plugins in SE from version `1.203.022` onwards require the use of a third-party launcher exe. You must have one in order to use plugins.  
The SE community has developed a launcher which can be found here: https://github.com/sepluginloader/SpaceEngineersLauncher  
Downloads and installation instructions for the launcher can be found through that link.  
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

#### Usage
To enable the positional audio features the plugin requires that a TeamSpeak channel be designated as an in-game channel.
This is done by setting the channel topic text to `sets-ingame` The text must match exactly, no extra spaces. This alone is the most basic setup, the plugin also offers more control over sub-channels.  
There are 3 other types of channel that are designated in the same way by setting the channel topic. They are:
* `sets-autocomms`
* `sets-alwaysconnected`
* `sets-crosscomms`

These options are used for sub-channels of the designated `sets-ingame` channel.

#### sets-autocomms
This option will disable voice spatialization only when players have an antenna connection between them (requires the companion mod), and will fall-back to being spatialized when the connection is lost. Clients in other sub-channels under the main ingame channel will still be spatialized.

#### sets-alwaysconnected
This option is like the previous one except it always disable voice spatialization for the clients in that one channel regardless of antenna connection. 

#### sets-crosscomms
This option will disable voice spatialization within it and between sub-channels of this channel but only when players have an antenna connection between them. It will fall-back to spatialized when the connection is lost. Clients in other sub-channels under the main ingame channel will still be spatialized.

Any channel not designated as one of the above options will function as normal like without the plugin.

To enable speaking across channels the plugin uses TeamSpeaks whisper functionality. By default TeamSpeak plays a sound when you receive a whisper from someone. When using the sub-channel features of this plugin it is recommended to disable the whisper notification sound and popup by unckecing the two checkboxes in `Tools->Options->Whisper->Settings for Received Whispers`.

##### Commands
The TeamSpeak plugin has a couple of chat based commands to control the audio settings.

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
There is an optional server-side mod for SE that works togther with the SE plugin to properly enable the functionality of the autocomms and crosscomms channel types. It restrict non-spatialized communication to only be available between players within antenna range including relayed connections.  
The mod can be found here: https://steamcommunity.com/sharedfiles/filedetails/?id=2935694294
