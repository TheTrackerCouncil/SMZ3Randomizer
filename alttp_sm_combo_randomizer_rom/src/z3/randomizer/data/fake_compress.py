#!/usr/bin/env python
import sys
import os

# This will "compress" ALTTP graphics data by just using the copy command

if sys.argv[1] == "":
	print("fake_compress.py <input bin> <output gfx>")
	sys.exit()
else:
	bin_name = sys.argv[1]
	gfx_name = sys.argv[2]


f_bin = open(bin_name, "rb")
f_gfx = open(gfx_name, "wb")

d_bin = f_bin.read()

for i in range(0, len(d_bin), 0x400):
    data = d_bin[i : i + 0x400]
    cmd = 0xE000 | ((len(data) & 0xFFF)-1)
    
    f_gfx.write(bytes([(cmd >> 8) & 0xFF, (cmd & 0xFF)]))
    f_gfx.write(data)

f_gfx.write(bytes([0xFF]))
f_gfx.close()
f_bin.close()
        