@echo off

:: To use this, you will need the following in the resources folder
    :: asar (https://github.com/RPGHacker/asar/releases/)
    :: Lunar IPS (https://www.romhacking.net/utilities/240/)
    :: sm.sfc
    :: z3.sfc

echo Building Super Metroid + Zelda 3 IPS Patches

cd resources

IF NOT EXIST sm.sfc EXIT /b
IF NOT EXIST "Lunar IPS.exe" EXIT /b

:: Create SM IPS patches
for /f %%a IN ('dir /b /s "..\src\ips_patches\sm\*.asm"') do (
    echo Creating %%~na.ips
    copy sm.sfc working.sfc > NUL
    asar %%a working.sfc
    "Lunar IPS.exe" -CreateIPS ..\build\%%~na.ips sm.sfc working.sfc
)

del working.sfc
cd ..
echo Done
PAUSE
