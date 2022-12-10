$parentFolder = Split-Path -parent $PSScriptRoot
$folder = "$parentFolder\src\Randomizer.Multiplayer.Server\bin\Release\net6.0\publish"
$version = (Get-Item "$folder\Randomizer.Multiplayer.Server.exe").VersionInfo.ProductVersion
$fullVersion = "Randomizer.Multiplayer.Server-$version"
$outputZip = "$PSScriptRoot\output\$fullVersion.zip"
if (Test-Path $outputZip) {
    Remove-Item $outputZip -Force
}
if (-not (Test-Path $outputZip)) {
    Compress-Archive -Path "$folder\*" -DestinationPath $outputZip
}