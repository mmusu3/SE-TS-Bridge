# SE-TS-Bridge
Plugins for Space Engineers and TeamSpeak to enable positional audio features.

## Installation
First download the latest release from https://github.com/mmusu3/SE-TS-Bridge/releases
There are two plugins. One for Space Engineers (SE) and one for TeamSpeak (TS). Their names start with the program abbreviation that they apply to.
- The one for SE is called `SE-TS_Plugin.dll`.
- The one for TS is called `TS-SE_Plugin.dll`.

Be sure not to mix them up.

#### Space Engineers Plugin
To install the SE plugin first locaate your Space Engineers install folder.
The install folder is usually of the form `C:/Program Files (x86)/Steam/steamapps/common/SpaceEngineers/Bin64`. It may be different on your computer.
Inside that folder you may want to add a `Plugins` folder for better organization. Now copy the `SE-TS_Plugin.dll` file into the `Plugins` folder.
To use the plugin you will need to change the launch options in the Space Engineers game properties page in Steam.
If you are not using any other plugins sinply add `-plugin ./Plugins/SE-TS Plugin.dll` to the launch options box.
If you are using other plugins add `./Plugins/SE-TS_Plugin.dll` after the previous plugins paths. Eg. `-plugin ./Plugins/otherplugin.dll ./Plugins/SE-TS_Plugin.dll`
If SE is already running you will need to restart it.

#### TeamSpeak Plugin
To install the TS plugin, first copy the `TS-SE_Plugin.dll` file into your TeamSpeak plugins folder which can usually be found at `%appdata%/TS3Client/plugins`.
To enable the plugin go to TeamSpeak Options->Addons->Plugins and ensure TS-SE Plugin is enabled there.
To enable positional audio support go to TeamSpeak Options->Playback and ensure the `Always set clients 3D positions when available` checkbox is ticked.

When updating either plugin the respective application will need to be closed and restarted.

## Usage
As the plugin is still rudimentary, currently Space Engineers and TeamSpeak users are matched by their name. As such, for proper operation clients must have the same name in TS and Steam.
