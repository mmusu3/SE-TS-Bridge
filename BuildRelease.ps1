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

$publishDirectory = Join-Path $InstallerOutput ([guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $publishDirectory | Out-Null

try {
	dotnet publish (Join-Path $PSScriptRoot 'Installer\Installer.csproj') -c Release -o $publishDirectory
	if ($LASTEXITCODE -ne 0) {
		throw "Installer publish failed with exit code $LASTEXITCODE."
	}

	$installerFiles = @(Get-ChildItem -LiteralPath $publishDirectory | Where-Object { $_.Extension -ne '.pdb' } | ForEach-Object { $_.FullName })
	Compress-Archive -LiteralPath ($installerFiles + @($sePlugin, $tsPlugin, (Join-Path $PSScriptRoot 'README.md'))) -DestinationPath $DestinationPath
}
finally {
	Remove-Item -LiteralPath $publishDirectory -Recurse -Force
}
