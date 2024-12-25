$parentFolder = Split-Path -parent $PSScriptRoot

# Get publish folder
$folder = "$parentFolder\src\TrackerCouncil.Smz3.UI\bin\Release\net8.0\linux-x64\publish"
$winFolder = "$parentFolder\src\TrackerCouncil.Smz3.UI\bin\Release\net8.0\win-x86\publish"
if (-not (Test-Path $folder))
{
    $folder = "$parentFolder\src\TrackerCouncil.Smz3.UI\bin\Release\net8.0\publish\linux-x64"
    $winFolder = "$parentFolder\src\TrackerCouncil.Smz3.UI\bin\Release\net8.0\publish\win-x86"
}

# Get version number from TrackerCouncil.Smz3.UI
$version = "0.0.0"
if (Test-Path "$winFolder\TrackerCouncil.Smz3.UI.exe") {
    $version = (Get-Item "$winFolder\SMZ3CasRandomizer.exe").VersionInfo.ProductVersion
}
else {
    $version = (Get-Item "$folder\SMZ3CasRandomizer.dll").VersionInfo.ProductVersion
}
$version = $version -replace "\+.*", ""

# Copy sprites to be bundled together
if (Test-Path -LiteralPath "$folder\Sprites") {
    Remove-Item -LiteralPath "$folder\Sprites" -Recurse
}
Copy-Item "$parentFolder\sprites\Sprites\" -Destination "$folder\Sprites" -Recurse

# Copy tracker sprites to be bundled together
if (Test-Path -LiteralPath "$folder\TrackerSprites") {
    Remove-Item -LiteralPath "$folder\TrackerSprites" -Recurse
}
Copy-Item "$parentFolder\trackersprites\" -Destination "$folder\TrackerSprites" -Recurse

# Copy configs to be bundled together
if (Test-Path -LiteralPath "$folder\Configs") {
    Remove-Item -LiteralPath "$folder\Configs" -Recurse
}
Copy-Item "$parentFolder\configs\Profiles\" -Destination "$folder\Configs" -Recurse

# Create package
$fullVersion = "SMZ3CasRandomizerLinux_$version"
$outputFile = "$PSScriptRoot\Output\$fullVersion.tar.gz"
if (Test-Path $outputFile) {
    Remove-Item $outputFile -Force
}
if (-not (Test-Path $outputFile)) {
    Set-Location $folder
    tar -cvzf $outputFile *
}
Set-Location $PSScriptRoot