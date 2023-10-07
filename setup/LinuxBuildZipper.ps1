$parentFolder = Split-Path -parent $PSScriptRoot

# Get publish folder
$folder = "$parentFolder\src\Randomizer.CrossPlatform\bin\Release\net7.0\linux-x64\publish"
$winFolder = "$parentFolder\src\Randomizer.App\bin\Release\net7.0-windows\win-x86\publish"
if (-not (Test-Path $folder))
{
    $folder = "$parentFolder\src\Randomizer.CrossPlatform\bin\Release\net7.0\publish\linux-x64"
    $winFolder = "$parentFolder\src\Randomizer.App\bin\Release\net7.0-windows\publish\win-x86"
}

# Get version number from Randomizer.App
$version = "0.0.0"
if (Test-Path "$winFolder\Randomizer.App.exe") {
    $version = (Get-Item "$winFolder\Randomizer.App.exe").VersionInfo.ProductVersion
}
else {
    $version = (Get-Item "$folder\Randomizer.CrossPlatform.dll").VersionInfo.ProductVersion
}

# Copy sprites to be bundled together
if (Test-Path -LiteralPath "$folder\Sprites") {
    Remove-Item -LiteralPath "$folder\Sprites" -Recurse
}
Copy-Item "$parentFolder\src\Randomizer.Sprites\" -Destination "$folder\Sprites" -Recurse
Remove-Item "$folder\Sprites\bin" -Recurse
Remove-Item "$folder\Sprites\obj" -Recurse
Get-ChildItem -Exclude *.png,*.rdc,*.ips,*.gif -Recurse -Path "$folder\Sprites" | Where-Object { !$_.PSisContainer } | ForEach-Object {
    Remove-Item -Path $_.FullName -Recurse
}

# Create dupes of Randomizer.CrossPlatform as SMZ3CasRandomizer
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