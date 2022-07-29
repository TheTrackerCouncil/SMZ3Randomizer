#!/usr/bin/env bash
# Remember to commit an updated build.bat as well if making changes

echo Building Super Metroid + Zelda 3 Randomizer

find . -name build.py -exec python3 {} \;

cd resources
python3 create_dummies.py 00.sfc ff.sfc
./asar --no-title-check --symbols=wla --symbols-path=../build/zsm.sym ../src/main.asm 00.sfc
./asar --no-title-check --symbols=wla --symbols-path=../build/zsm.sym ../src/main.asm ff.sfc
python3 create_ips.py 00.sfc ff.sfc zsm.ips
rm 00.sfc ff.sfc

cp zsm.ips ../build/zsm.ips > /dev/null

cd ..
echo Done
