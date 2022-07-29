incsrc "tile_object_macros.asm"

; Tile object section structure:
; 2 bytes header
; A number of layers, new ones started with the sequence $FF $FF
;   A number of 3 byte tile objects
;   Doors list within a layer, started with the sequence $F0 $FF
;     A number of dw entries
; $FF $FF


; The Waterfall of Wishing cave, on super tile index $114 (276), has its two
; side walls lowered, and the two torches replaced by chests.

org $3F714+2+(3*3)                   ; layer 1, tile object at index 3

%tile_object_type1( $61, 4,  44, 5)  ; extend left wall down
%tile_object_type2($101, 4,  54)     ; move inner wall SW down
%tile_object_type1( $02, 8,  54, 2)  ; move bottom wall down
%tile_object_type1( $61, 11, 56, 1)  ; list left wall before outer wall for z order
%tile_object_type2($105, 11, 54)     ; move outer wall SW down

org $3F714+2+(3*9)                   ; layer 1, tile object at index 9

%tile_object_type1( $62, 24, 44, 5)  ; extend right wall down
%tile_object_type2($103, 24, 54)     ; move inner Wall SE down
%tile_object_type1( $02, 21, 54, 2)  ; move bottom wall down
%tile_object_type1( $62, 17, 56, 1)  ; list right wall before outer wall for z order
%tile_object_type2($107, 17, 54)     ; move Outer wall SE down

org $3F714+2+(3*19)                  ; layer 1, tile object at index 19

%tile_object_type3($F99, 12, 49)     ; replace torch with chest
%tile_object_type3($F99, 18, 49)     ; replace torch with chest

org $3F714+2+(3*24)                  ; layer 1, tile object at index 24

%tile_object_type1( $C0, 3,  58, 4)  ; large ceiling at the entrance, shrink and move down 
%tile_object_type1( $C0, 21, 58, 4)  ; large ceiling at the entrance, shrink and move down 

; Repurpose unused dungeon chest data entries at index 22, 33 for Waterfall of Wishing chests

org $1E96C+(3*22) : dw $0114
org $1E96C+(3*33) : dw $0114


; The Pyramid Fairy cave, on super tile index $116 (278), has its two torches
; replaced by chests.

org $3FBF3+2+(3*11)                  ; layer 1, tile object at index 11

%tile_object_type3($F99, 44, 49)     ; replace torch with chest
%tile_object_type3($F99, 50, 49)     ; replace torch with chest

; Repurpose unused dungeon chest data entries at index 6, 7 for Pyramid Fairy chests

org $1E96C+(3*6)
    dw $0116 : db $08  ; default giving ice rod
    dw $0116 : db $07  ; default giving fire rod
