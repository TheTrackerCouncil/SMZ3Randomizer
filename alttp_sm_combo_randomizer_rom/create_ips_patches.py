import os
import pathlib
import glob
import shutil
import subprocess

print('Building Super Metroid + Zelda 3 IPS Patches')

path = pathlib.Path(__file__).parent.resolve()
os.chdir(path)
os.chdir('resources')

if not pathlib.Path("sm.sfc").is_file():
    print("sm.sfc missing")
    exit()

if not pathlib.Path("flips-linux").is_file():
    print("flips-linux missing (https://www.romhacking.net/utilities/1040/)")
    exit()

if not pathlib.Path("asar").is_file():
    print("asar missing (https://github.com/RPGHacker/asar)")
    exit()

# Create SM patches
for file in glob.glob(r'../src/ips_patches/sm/*.asm', recursive=True):
    basename = os.path.basename(file).replace('.asm', '.ips')
    print('Creating', basename)
    shutil.copyfile('sm.sfc', 'working.sfc')
    subprocess.run(["./asar", file, "working.sfc"])
    subprocess.run(["./flips-linux", "-c", "--ips", 'sm.sfc', 'working.sfc', '../build/' + basename])

