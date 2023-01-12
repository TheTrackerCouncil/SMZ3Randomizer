@echo off

:: To use this, you will need the following in the resources folder
    :: asar (https://github.com/RPGHacker/asar/releases/)

:: To build optional IPS patches, you will also need the following in the resources folder
    :: Lunar IPS (https://www.romhacking.net/utilities/240/)
    :: smz3.sfc (A relatively clean copy of SMZ3 with as few patches as possible)

echo Building Super Metroid + Zelda 3 Randomizer

for /r %%f in (build*.py) do python %%f

cd resources
python create_dummies.py 00.sfc ff.sfc
asar --no-title-check --symbols=wla --symbols-path=..\build\zsm.sym ..\src\main.asm 00.sfc
asar --no-title-check --symbols=wla --symbols-path=..\build\zsm.sym ..\src\main.asm ff.sfc
python create_ips.py 00.sfc ff.sfc ..\build\zsm.ips
del 00.sfc ff.sfc

cd ..

create_ips_patches.bat

echo Done
