param(
	[string]$DestinationPath = (Join-Path $PSScriptRoot 'SE-TS_Bridge.zip'),
	[string]$InstallerOutput = (Join-Path $PSScriptRoot 'Bin\Installer\publish')
)

$ErrorActionPreference = 'Stop'

$sePlugin = Join-Path $PSScriptRoot 'Bin\TSPluginForSE\TSPluginForSE.dll'
$tsPlugin = Join-Path $PSScriptRoot 'Bin\SEPluginForTS\win-x64\publish\SEPluginForTS.dll'

foreach ($plugin in @($sePlugin, $tsPlugin)) {
	if (-not (Test-Path -LiteralPath $plugin -PathType Leaf)) {
		throw "Plugin file not found: $plugin"
	}
}

dotnet publish (Join-Path $PSScriptRoot 'Installer\Installer.csproj') -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $InstallerOutput
if ($LASTEXITCODE -ne 0) {
	throw "Installer publish failed with exit code $LASTEXITCODE."
}

Compress-Archive -Path (Join-Path $InstallerOutput 'Installer.exe'), $sePlugin, $tsPlugin, (Join-Path $PSScriptRoot 'README.md') -DestinationPath $DestinationPath
