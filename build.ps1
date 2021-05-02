[CmdletBinding()]
param(
    [Parameter()]
    [string]
    $PatchDir = "alttp_sm_combo_randomizer_rom"
)
process {
    $ErrorActionPreference = "Stop"

    function Compress-Item([string]$Path, [string]$OutFile) {
        $buffer = New-Object System.IO.MemoryStream
        $gzip = New-Object System.IO.Compression.GZipStream ($buffer, [IO.Compression.CompressionMode]::Compress)

        $source = [IO.File]::OpenRead($Path)
        $source.CopyTo($gzip)
        $source.Close()

        [IO.File]::WriteAllBytes($OutFile, $buffer.ToArray())
    }

    $cd = (Get-Location -PSProvider FileSystem).ProviderPath

    # Download ASAR
    if (-not(Test-Path -Path "$cd/asar")) {
        New-Item -Path "$cd/asar" -ItemType Directory
    }
    
    Invoke-WebRequest `
        -Uri "https://github.com/RPGHacker/asar/releases/download/v1.81/asar181.zip" `
        -OutFile "$cd/asar/asar181.zip"

    # Extract ASAR to the patch folder
    [Environment]::CurrentDirectory = "$cd/asar"
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    Try {
        $zip = [System.IO.Compression.ZipFile]::OpenRead("asar181.zip")
        [System.IO.Compression.ZipFileExtensions]::ExtractToFile($zip.GetEntry("asar.exe"), "asar.exe", $true);
    }
    Finally {
        $zip.Dispose()
    }
    Copy-Item -Path "$cd/asar/asar.exe" -Destination "$PatchDir/resources/asar.exe"

    # Build the patch
    Set-Location $PatchDir
    cmd.exe /c "build.bat"

    Compress-Item -Path "$cd/$PatchDir/build/zsm.ips" -OutFile "$cd/WebRandomizer/ClientApp/src/resources/zsm.ips.gz"

    Set-Location $cd
}