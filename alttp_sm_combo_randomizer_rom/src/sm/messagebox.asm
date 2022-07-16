; SM Message box patches

; Hooks
; $85:81EA 22 0C 8F 80 JSL $808F0C[$80:8F0C]  ; Handle music queue
; Hook to DMA lowercase font data into VRAM
org $c581ea
    jsl msg_copy_charset
org $c5861f
    jsl msg_restore_vram : nop

table box.tbl,rtl

org $c5877f
dw "______    Energy Tank    _______"

org $c587bf
dw "___          Missile         ___"

org $c588bf
dw "___      Super Missile       ___"

org $c589bf
dw "___        Power Bomb        ___"

org $c58abf
dw "___      Grappling Beam      ___"

org $c58bbf
dw "___        X-Ray Scope       ___"

org $c58cbf
dw "______    Varia Suit     _______"
dw "______    Spring Ball    _______"
dw "______   Morphing Ball   _______"
dw "______    Screw Attack   _______"
dw "______   Hi-Jump Boots   _______"
dw "______     Space Jump    _______"

org $c58e3f
dw "___       Speed Booster      ___"

org $c58f3f
dw "______    Charge Beam    _______"
dw "______     Ice Beam      _______"
dw "______     Wave Beam     _______"
dw "______  ~ S p A z E r ~  _______"
dw "______    Plasma Beam    _______"

org $c5907f
dw "___           Bomb           ___"

org $c5917f
dw "______  Map data access  _______"
dw "______                   _______"
dw "______     Completed     _______"

dw "______  Energy recharge  _______"
dw "______                   _______"
dw "______     Completed     _______"

dw "______  Missile reload   _______"
dw "______                   _______"
dw "______     Completed     _______"

dw "______  Would you like   _______"
dw "______  to save?         _______"
dw "______                   _______"

org $c594bf
dw "______  Save completed   _______"
dw "______   Reserve Tank    _______"
dw "______   Gravity Suit    _______"

cleartable

; Routines
org $c8f000
base $88f000
msg_copy_charset:
    rep #$30

    ; Get the correct pointer into BG3 tilemap
    lda $005d
    asl #4
    and #$f000
    clc
    adc #$0400

    sta $2116
    lda #$1801
    sta $4310
    lda.w #lowercase_charset
    sta $4312
    lda.w #lowercase_charset>>16
    sta $4314
    lda #$0300
    sta $4315
    stz $4317
    stz $4319
    sep #$20
    lda #$80
    sta $2115
    lda #$02
    sta $420b

    jsl $808F0C
    rtl
    
msg_restore_vram:
    rep #$20

    ; Get the correct pointer into BG3 tilemap
    lda $005d
    asl #4
    and #$f000
    clc
    adc #$0400
    
    sta $2116
    lda #$1801
    sta $4310
    lda.w #$ba00
    sta $4312
    lda.w #$009a
    sta $4314
    lda #$0300
    sta $4315
    stz $4317
    stz $4319
    sep #$20
    lda #$80
    sta $2115
    lda #$02
    sta $420b
    rep #$20
    lda #$5880
    rtl    

lowercase_charset:
    incbin "data/lowercase.bin"
