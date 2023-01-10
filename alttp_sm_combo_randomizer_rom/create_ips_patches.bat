@echo off

:: To use this, you will need the following in the resources folder
    :: asar (https://github.com/RPGHacker/asar/releases/)
    :: Lunar IPS (https://www.romhacking.net/utilities/240/)
    :: smz3.sfc (A relatively clean copy of SMZ3 with as few patches as possible)

echo Building Super Metroid + Zelda 3 IPS Patches

cd resources

IF NOT EXIST smz3.sfc EXIT /b
IF NOT EXIST "Lunar IPS.exe" EXIT /b

:: Create patches
for /f %%a IN ('dir /b /s "..\src\ips_patches\*.asm"') do (
    echo Creating %%~na.ips
    copy smz3.sfc working.sfc > NUL
    asar %%a working.sfc
    "Lunar IPS.exe" -CreateIPS %%~na.ips smz3.sfc working.sfc
    copy %%~na.ips  ..\build\%%~na.ips  > NUL
)

del working.sfc
cd ..
echo Done
PAUSE
