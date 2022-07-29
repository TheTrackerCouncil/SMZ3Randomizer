@echo off
:: Remember to commit an updated build.sh as well if making changes

echo Building Super Metroid + Zelda 3 Randomizer

for /r %%f in (build*.py) do python %%f

cd resources
python create_dummies.py 00.sfc ff.sfc
asar --no-title-check --symbols=wla --symbols-path=..\build\zsm.sym ..\src\main.asm 00.sfc
asar --no-title-check --symbols=wla --symbols-path=..\build\zsm.sym ..\src\main.asm ff.sfc
python create_ips.py 00.sfc ff.sfc zsm.ips
del 00.sfc ff.sfc

copy zsm.ips ..\build\zsm.ips > NUL

cd ..
echo Done
