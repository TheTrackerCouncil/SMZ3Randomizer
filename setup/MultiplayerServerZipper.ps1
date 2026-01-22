$parentFolder = Split-Path -parent $PSScriptRoot
$folder = "$parentFolder\src\TrackerCouncil.Smz3.Multiplayer.Server\bin\Release\net10.0\publish"
$version = (Get-Item "$folder\TrackerCouncil.Smz3.Multiplayer.Server.exe").VersionInfo.ProductVersion
$version = $version -replace "\+.*", ""
$fullVersion = "TrackerCouncil.Smz3.Multiplayer.Server-$version"
$outputZip = "$PSScriptRoot\output\$fullVersion.zip"
if (Test-Path $outputZip) {
    Remove-Item $outputZip -Force
}
if (-not (Test-Path $outputZip)) {
    Compress-Archive -Path "$folder\*" -DestinationPath $outputZip
}