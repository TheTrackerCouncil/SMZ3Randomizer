BRINSTAR_MAP = $8000
BRINSTAR_VISIBLE = $9827
CRATERIA_MAP = $9000
CRATERIA_VISIBLE = $9727
NORFAIR_MAP = $A000
NORFAIR_VISIBLE = $9927
WRECKED_SHIP_MAP = $B000
WRECKED_SHIP_VISIBLE = $9A27
MARIDIA_MAP = $C000
MARIDIA_VISIBLE = $9B27
TOURIAN_MAP = $D000
TOURIAN_VISIBLE = $9C27
CERES_MAP = $E000
CERES_VISIBLE = $9D27

; Place the portal tiles into the maps where needed.
; Each of these maps is 64x32 tiles big, split into two 32x32 blocks.
; First is the left 32x32 tiles, and then following is the rightmost 32x32 tiles

; To place a maptile at X,Y calculate ((X%32)*2 + Y*64) + ((X/32) * 0x800)
macro write_map_tile(start, x, y, tile)
    org $F50000+<start>+((((<x>%32)*2)+(<y>*64))+((<x>/32)*$800))
        dw <tile>
endmacro

; Modifies the maptiles that can be revealed by a map station, 1 byte is 8 tiles, one per bit
macro write_map_visibility(start, x, y, value)
    org $C20000+<start>+(((<x>%32)/8)+(<y>*4)+((<x>/32)*$80))
        db <value>
endmacro

; Write crateria portal icon
%write_map_tile(CRATERIA_MAP, $14, $8, $0c1a)

; Blank out inaccessible crateria room tiles
%write_map_tile(CRATERIA_MAP, $15, $08, $0c1f)
%write_map_tile(CRATERIA_MAP, $16, $08, $0c1f)
%write_map_tile(CRATERIA_MAP, $17, $08, $0c1f)

; Write maridia portal icon
%write_map_tile(MARIDIA_MAP, $26, $09, $0c1a)

; Write upper norfair portal icon
%write_map_tile(NORFAIR_MAP, $09, $05, $4c1a)

; Write lower norfair portal icon
%write_map_tile(NORFAIR_MAP, $16, $10, $0c1a)
; Modify the norfair default visible map tiles to show the lower norfair portal
%write_map_visibility(NORFAIR_VISIBLE, $16, $10, $02)

; Insert the new portal map tile into the map tile bank
org $DAB3A0 
DB $00,$E0,$60,$95,$7C,$80,$7C,$80,$7C,$80,$7C,$80,$60,$95,$00,$E0

org $F68340
DB $00,$E0,$60,$95,$7C,$80,$7C,$80,$7C,$80,$7C,$80,$60,$95,$00,$E0
DB $00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00

; This fills in the bitfields responsible for triggering the
; "Map Station Reveals This Tile" behavior. Since the map
; stations are all activated by default, this should cause
; the entire map to be revealed from the start of the game.
org $C29727
fillbyte $ff : fill $FF ; Crateria
fillbyte $ff : fill $FF ; Norfair
fillbyte $ff : fill $FF ; Wrecked Ship
fillbyte $ff : fill $FF ; Maridia
fillbyte $ff : fill $FF ; Tourian