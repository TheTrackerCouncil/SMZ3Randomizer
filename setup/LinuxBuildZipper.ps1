$parentFolder = Split-Path -parent $PSScriptRoot

# Get publish folder
$folder = "$parentFolder\src\Randomizer.CrossPlatform\bin\Release\net8.0\linux-x64\publish"
$winFolder = "$parentFolder\src\Randomizer.CrossPlatform\bin\Release\net8.0\win-x86\publish"
if (-not (Test-Path $folder))
{
    $folder = "$parentFolder\src\Randomizer.CrossPlatform\bin\Release\net8.0\publish\linux-x64"
    $winFolder = "$parentFolder\src\Randomizer.CrossPlatform\bin\Release\net8.0\publish\win-x86"
}

# Get version number from Randomizer.App
$version = "0.0.0"
if (Test-Path "$winFolder\Randomizer.CrossPlatform.exe") {
    $version = (Get-Item "$winFolder\Randomizer.CrossPlatform.exe").VersionInfo.ProductVersion
}
else {
    $version = (Get-Item "$folder\Randomizer.CrossPlatform.dll").VersionInfo.ProductVersion
}
$version = $version -replace "\+.*", ""

# Copy the README.md
Copy-Item "$parentFolder\src\Randomizer.CrossPlatform\README.md" -Destination "$folder"

# Copy sprites to be bundled together
if (Test-Path -LiteralPath "$folder\Sprites") {
    Remove-Item -LiteralPath "$folder\Sprites" -Recurse
}
Copy-Item "$parentFolder\sprites\Sprites\" -Destination "$folder\Sprites" -Recurse

# Copy configs to be bundled together
if (Test-Path -LiteralPath "$folder\Configs") {
    Remove-Item -LiteralPath "$folder\Configs" -Recurse
}
Copy-Item "$parentFolder\configs\Profiles\" -Destination "$folder\Configs" -Recurse

# Create dupes of Randomizer.App as SMZ3CasRandomizer
Get-ChildItem -Filter "Randomizer.CrossPlatform*" -Path "$folder" | ForEach-Object {
    $newFileName = $_.Name -replace "Randomizer.CrossPlatform", "SMZ3CasRandomizer"
    Copy-Item -Path $_.FullName -Destination "$folder\$newFileName"
}

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