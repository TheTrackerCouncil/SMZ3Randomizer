
; Removes Gravity Suit heat protection
org $cde37d
    db $01
org $d0e9dd
    db $01

; Suit acquisition animation skip
org $c48717
    db $ea, $ea, $ea, $ea

; Fix Morph & Missiles room state
; org $cfe652
; morph_missiles:
;     lda.l $7ed873
;     beq .no_items
;     bra .items
; org $cfe65f
; .items
; org $cfe666
; .no_items

; Set morph and missiles room state to always active
org $cfe658
    db $ea, $ea

org $cfe65d
    db $ea, $ea

; Fix heat damage speed echoes bug
org $d1b629
    db $01

; Disable GT Code
org $eac91c
    db $80

; Disable Space/time
org $c2b175
    db $01

; Fix Morph Ball Hidden/Chozo PLM's
org $c4e8ce
    db $04
org $c4ee02
    db $04

; Fix Screw Attack selection in menu
org $c2b4c5
    db $0c


; Mother Brain Cutscene Edits
org $e98824
    db $01, $00
org $e98848
    db $01, $00
org $e98867
    db $01, $00
org $e9887f
    db $01, $00
org $e98bdb
    db $04, $00
org $e9897d
    db $10, $00
org $e989af
    db $10, $00
org $e989e1
    db $10, $00
org $e98a09
    db $10, $00
org $e98a31
    db $10, $00
org $e98a63
    db $10, $00
org $e98a95
    db $10, $00
org $e98b33
    db $10, $00
org $e98dc6
    db $b0
org $e98b8d
    db $12, $00
org $e98d74
    db $00, $00
org $e98d86
    db $00, $00
org $e98daf
    db $00, $01
org $e98e51
    db $01, $00
org $e9b93a
    db $00, $01
org $e98eef
    db $0a, $00
org $e98f0f
    db $60, $00
org $e9af4e
    db $0a, $00
org $e9af0d
    db $0a, $00
org $e9b00d
    db $00, $00
org $e9b132
    db $40, $00
org $e9b16d
    db $00, $00
org $e9b19f
    db $20, $00
org $e9b1b2
    db $30, $00
org $e9b20c
    db $03, $00

; Fix door event bits
org $c38bd0
    db $40

org $c397c4
    db $40

org $c398a8
    db $40

org $c3a896
    db $40

org $cf86ec
    db $23, $EF, $45, $29, $1A, $00

org $c55ef0
    incbin "data/morphroom.bin"

org $c562e4
    db $ff, $ff, $ff, $ff

