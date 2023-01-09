@echo off
:: Remember to commit an updated build.sh as well if making changes

echo Building Super Metroid + Zelda 3 Randomizer

for /r %%f in (build*.py) do python %%f

cd resources
python create_dummies.py 00.sfc ff.sfc
asar --no-title-check --symbols=wla --symbols-path=..\build\zsm.sym ..\src\ips_patches\nerfed_charge.asm 00.sfc
asar --no-title-check --symbols=wla --symbols-path=..\build\zsm.sym ..\src\ips_patches\nerfed_charge.asm ff.sfc
python create_ips.py 00.sfc ff.sfc nerfed_charge.ips
del 00.sfc ff.sfc

copy nerfed_charge.ips ..\build\nerfed_charge.ips > NUL

cd ..
echo Done

PAUSE