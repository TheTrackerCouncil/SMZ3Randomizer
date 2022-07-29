pushpc
org $0ef51b
    jml RenderCharExtended
org $0ef520
    RenderCharExtended_returnOriginal:
org $0ef567
    RenderCharExtended_returnUncompressed:

org $0ef356
    jsl RenderCharLookupWidth
org $0ef3ba
    jsl RenderCharLookupWidth
org $0ef48e
    jml RenderCharLookupWidthDraw
org $0ef499
    RenderCharLookupWidthDraw_return:

org $0ef6aa
    jml RenderCharToMapExtended
org $0ef6c2
    RenderCharToMapExtended_return:

org $0efa50
    jsl RenderCharSetColorExtended
org $0eee5d
    jsl RenderCharSetColorExtended_init
org $0ef285
    jsl RenderCharSetColorExtended_close : nop

; Change this to free space somewhere
org $531000
    NewFont:
        incbin data/newfont.bin
    NewFontInverted:
        incbin data/newfont_inverted.bin
pullpc

!INVERTED_TEMP = $35

RenderCharSetColorExtended_init:
    stz !INVERTED_TEMP
    jsl $00d84e
    rtl

RenderCharSetColorExtended_close:
    stz !INVERTED_TEMP
    lda $010c
    sta $10
    rtl

RenderCharSetColorExtended:
    pha
    and #$10
    cmp #$10
    beq .inverted
    lda #$00
    bra .end
.inverted
    lda #$01
.end
    sta !INVERTED_TEMP
    pla
    and #$07 : asl : asl
    rtl

RenderCharToMapExtended:
    phx : tya : asl #2 : tax
    lda.l FontProperties, x
    and #$0001
    bne .uncompressed
.compressed
    plx
    lda #$0000
    sta $00
    lda #$007f
    sta $02
    lda #$0000
    clc : adc #$0020
    sta $03
    lda #$007f
    sta $05
    jml RenderCharToMapExtended_return

.uncompressed    
    lda.l FontProperties+$2, x
    plx
    clc : adc #(NewFont&$ffff)
    sta $00
    clc : adc #$0100
    pha
    lda #(NewFont>>16)
    sta $02
    pla : sta $03
    lda #(NewFont>>16)
    sta $05
    jml RenderCharToMapExtended_return

RenderCharLookupWidthDraw:
    rep #$30
    phx : lda $09 : and #$fffe : tax
    lda.l FontProperties, x
    bmi .thin
.wide
    plx : sep #$30
    lda $09 : and #$03 : tay
    lda $fd7c, y : tay
    jml RenderCharLookupWidthDraw_return
.thin
    xba : and #$004f : bne .vwf
    plx : sep #$30
    lda $09 : and #$03 : phx : tax
    lda.l RenderCharThinTable, x : tay : plx
    jml RenderCharLookupWidthDraw_return
.vwf
    and #$000f : tay
    plx : sep #$30
    lda $09 : and #$03 : phx : tax
    cpx #$00 : bne +
    tya : bra ++
+   lda.l RenderCharThinTable, x
++  tay : plx : jml RenderCharLookupWidthDraw_return


RenderCharLookupWidth:
    phx : lda $09 : and #$fffe : tax
    lda.l FontProperties, x
    bmi .thin
.wide
    plx : lda $fd7c, x : clc
    rtl
.thin
    xba : and #$004f : bne .vwf
    plx : lda.l RenderCharThinTable, x : clc
    rtl
.vwf
    and #$000f     
    plx : cpx #$0000 : beq + : lda.l RenderCharThinTable, x
+   clc : rtl

RenderCharThinTable:
    db $08, $00, $ff

RenderCharExtended:
    pha
    asl : asl : tax
    lda.l FontProperties, x
    and #$00ff
    bne .renderUncompressed

.renderOriginal
    pla : asl : tax : asl : adc $0e
    jml RenderCharExtended_returnOriginal

.renderUncompressed
    pla : phb : pea $5353 : plb : plb
    lda.l FontProperties+$2, x
    tay

    lda !INVERTED_TEMP
    bne .inverted

    ldx #$00000
-
    lda.w NewFont, y
    sta.l $7EBFC0, x
    lda.w NewFont+$100, y
    sta.l $7EBFC0+$16, x
    inx #2
    iny #2
    cpx #$0010
    bne -
    bra .end

.inverted
    ldx #$00000
-
    lda.w NewFontInverted, y
    sta.l $7EBFC0, x
    lda.w NewFontInverted+$100, y
    sta.l $7EBFC0+$16, x
    inx #2
    iny #2
    cpx #$0010
    bne -

.end    
    plb
    jml RenderCharExtended_returnUncompressed

; Table of font properties and tilemap offset
; Properties are these for now:
;  tv--wwww -------u
;  t = thin spacing (0 px instead of 3 px)
;  u = uncompressed character loaded from offset
;  v = use variable width rendering for this character (thin spacing must be set as well)
;  w = character width for VWF rendering

FontProperties:
;      props, offset
    dw $0000, $0000     ; 00
    dw $0000, $0000     ; 01
    dw $0000, $0000     ; 02
    dw $0000, $0000     ; 03
    dw $0000, $0000     ; 04
    dw $0000, $0000     ; 05
    dw $0000, $0000     ; 06
    dw $0000, $0000     ; 07
    dw $0000, $0000     ; 08
    dw $0000, $0000     ; 09
    dw $0000, $0000     ; 0A
    dw $0000, $0000     ; 0B
    dw $0000, $0000     ; 0C
    dw $0000, $0000     ; 0D
    dw $0000, $0000     ; 0E
    dw $0000, $0000     ; 0F

    dw $0000, $0000     ; 10
    dw $0000, $0000     ; 11
    dw $0000, $0000     ; 12
    dw $0000, $0000     ; 13
    dw $0000, $0000     ; 14
    dw $0000, $0000     ; 15
    dw $0000, $0000     ; 16
    dw $0000, $0000     ; 17
    dw $0000, $0000     ; 18
    dw $0000, $0000     ; 19
    dw $0000, $0000     ; 1A
    dw $0000, $0000     ; 1B
    dw $0000, $0000     ; 1C
    dw $0000, $0000     ; 1D
    dw $0000, $0000     ; 1E
    dw $0000, $0000     ; 1F

    dw $0000, $0000     ; 20
    dw $0000, $0000     ; 21
    dw $0000, $0000     ; 22
    dw $0000, $0000     ; 23
    dw $0000, $0000     ; 24
    dw $0000, $0000     ; 25
    dw $0000, $0000     ; 26
    dw $0000, $0000     ; 27
    dw $0000, $0000     ; 28
    dw $0000, $0000     ; 29
    dw $0000, $0000     ; 2A
    dw $0000, $0000     ; 2B
    dw $0000, $0000     ; 2C
    dw $0000, $0000     ; 2D
    dw $0000, $0000     ; 2E
    dw $0000, $0000     ; 2F

    dw $8001, $0400     ; 30 ; a
    dw $8001, $0410     ; 31
    dw $8001, $0420     ; 32
    dw $8001, $0430     ; 33
    dw $8001, $0440     ; 34
    dw $8001, $0450     ; 35
    dw $8001, $0460     ; 36
    dw $8001, $0470     ; 37
    dw $8001, $0480     ; 38
    dw $8001, $0490     ; 39
    dw $8001, $04A0     ; 3A
    dw $8001, $04B0     ; 3B
    dw $8001, $04C0     ; 3C
    dw $8001, $04D0     ; 3D
    dw $8001, $04E0     ; 3E
    dw $8001, $04F0     ; 3F

    dw $8001, $0600     ; 40
    dw $8001, $0610     ; 41
    dw $8001, $0620     ; 42
    dw $8001, $0630     ; 43
    dw $8001, $0640     ; 44
    dw $8001, $0650     ; 45
    dw $8001, $0660     ; 46
    dw $8001, $0670     ; 47
    dw $8001, $0680     ; 48
    dw $8001, $0690     ; 49 ; z
    dw $8001, $06F0     ; 4A ; :
    dw $8001, $0A90     ; 4B ; @ (thin)
    dw $8001, $0AA0     ; 4C ; # (thin)
    dw $8001, $0AB0     ; 4D ; morphball left
    dw $8001, $0AC0     ; 4E ; morphball right
    dw $8001, $0EF0     ; 4F ; <sp>

    dw $0000, $0000     ; 50
    dw $0000, $0000     ; 51
    dw $0000, $0000     ; 52
    dw $0000, $0000     ; 53
    dw $0000, $0000     ; 54
    dw $0000, $0000     ; 55
    dw $0000, $0000     ; 56
    dw $0000, $0000     ; 57
    dw $0000, $0000     ; 58
    dw $0000, $0000     ; 59
    dw $0000, $0000     ; 5A
    dw $0000, $0000     ; 5B
    dw $0000, $0000     ; 5C
    dw $0000, $0000     ; 5D
    dw $0000, $0000     ; 5E
    dw $0000, $0000     ; 5F

    dw $0000, $0000     ; 60
    dw $0000, $0000     ; 61
    dw $0000, $0000     ; 62
    dw $0000, $0000     ; 63
    dw $0000, $0000     ; 64
    dw $0000, $0000     ; 65
    dw $0000, $0000     ; 66
    dw $0000, $0000     ; 67
    dw $0000, $0000     ; 68
    dw $0000, $0000     ; 69
    dw $0000, $0000     ; 6A
    dw $0000, $0000     ; 6B
    dw $0000, $0000     ; 6C
    dw $0000, $0000     ; 6D
    dw $0000, $0000     ; 6E
    dw $0000, $0000     ; 6F

    dw $0000, $0000     ; 70
    dw $0000, $0000     ; 71
    dw $0000, $0000     ; 72
    dw $0000, $0000     ; 73
    dw $0000, $0000     ; 74
    dw $0000, $0000     ; 75
    dw $0000, $0000     ; 76
    dw $0000, $0000     ; 77
    dw $0000, $0000     ; 78
    dw $0000, $0000     ; 79
    dw $0000, $0000     ; 7A
    dw $0000, $0000     ; 7B
    dw $0000, $0000     ; 7C
    dw $0000, $0000     ; 7D
    dw $0000, $0000     ; 7E
    dw $0000, $0000     ; 7F

    dw $0000, $0000     ; 80
    dw $0000, $0000     ; 81
    dw $0000, $0000     ; 82
    dw $0000, $0000     ; 83
    dw $0000, $0000     ; 84
    dw $0000, $0000     ; 85
    dw $0000, $0000     ; 86
    dw $0000, $0000     ; 87
    dw $0000, $0000     ; 88
    dw $0000, $0000     ; 89
    dw $0000, $0000     ; 8A
    dw $0000, $0000     ; 8B
    dw $0000, $0000     ; 8C
    dw $0000, $0000     ; 8D
    dw $0000, $0000     ; 8E
    dw $0000, $0000     ; 8F

    dw $0000, $0000     ; 90
    dw $0000, $0000     ; 91
    dw $0000, $0000     ; 92
    dw $0000, $0000     ; 93
    dw $0000, $0000     ; 94
    dw $0000, $0000     ; 95
    dw $0000, $0000     ; 96
    dw $0000, $0000     ; 97
    dw $0000, $0000     ; 98
    dw $0000, $0000     ; 99
    dw $0000, $0000     ; 9A
    dw $0000, $0000     ; 9B
    dw $0000, $0000     ; 9C
    dw $0000, $0000     ; 9D
    dw $0000, $0000     ; 9E
    dw $0000, $0000     ; 9F

    dw $8001, $0800     ; A0 ; 0
    dw $8001, $0810     ; A1
    dw $8001, $0820     ; A2
    dw $8001, $0830     ; A3
    dw $8001, $0840     ; A4
    dw $8001, $0850     ; A5
    dw $8001, $0860     ; A6
    dw $8001, $0870     ; A7
    dw $8001, $0880     ; A8
    dw $8001, $0890     ; A9 ; 9
    dw $8001, $0000     ; AA ; A
    dw $8001, $0010     ; AB
    dw $8001, $0020     ; AC
    dw $8001, $0030     ; AD
    dw $8001, $0040     ; AE
    dw $8001, $0050     ; AF

    dw $8001, $0060     ; B0
    dw $8001, $0070     ; B1
    dw $8001, $0080     ; B2
    dw $8001, $0090     ; B3
    dw $8001, $00A0     ; B4
    dw $8001, $00B0     ; B5
    dw $8001, $00C0     ; B6
    dw $8001, $00D0     ; B7
    dw $8001, $00E0     ; B8
    dw $8001, $00F0     ; B9
    dw $8001, $0200     ; BA
    dw $8001, $0210     ; BB
    dw $8001, $0220     ; BC
    dw $8001, $0230     ; BD
    dw $8001, $0240     ; BE
    dw $8001, $0250     ; BF

    dw $8001, $0260     ; C0
    dw $8001, $0270     ; C1
    dw $8001, $0280     ; C2
    dw $8001, $0290     ; C3 ; Z
    dw $8000, $0000     ; C4
    dw $8000, $0000     ; C5
    dw $8001, $06D0     ; C6 ; ?
    dw $8001, $06C0     ; C7 ; !
    dw $8001, $02D0     ; C8 ; ,
    dw $8001, $02B0     ; C9 ; -
    dw $8000, $0000     ; CA
    dw $8000, $0000     ; CB
    dw $8000, $02E0     ; CC ; ...
    dw $8001, $02C0     ; CD ; .
    dw $8001, $02F0     ; CE ; ~
    dw $8000, $0000     ; CF

    dw $0000, $0000     ; D0
    dw $0000, $0000     ; D1
    dw $8001, $06a0     ; D2 ; Link face left
    dw $8001, $06b0     ; D3 ; Link face right
    dw $0000, $0000     ; D4
    dw $0000, $0000     ; D5
    dw $0000, $0000     ; D6
    dw $0000, $0000     ; D7
    dw $8001, $06E0     ; D8 ; '
    dw $0000, $0000     ; D9
    dw $0000, $0000     ; DA
    dw $0000, $0000     ; DB
    dw $0000, $0000     ; DC
    dw $0000, $0000     ; DD
    dw $0000, $0000     ; DE
    dw $0000, $0000     ; DF

    dw $0000, $0000     ; E0
    dw $0000, $0000     ; E1
    dw $0000, $0000     ; E2
    dw $0000, $0000     ; E3
    dw $8001, $02A0     ; E4 ; Cursor |>
    dw $0000, $0000     ; E5
    dw $0000, $0000     ; E6
    dw $0000, $0000     ; E7
    dw $0000, $0000     ; E8
    dw $0000, $0000     ; E9
    dw $0000, $0000     ; EA
    dw $0000, $0000     ; EB
    dw $0000, $0000     ; EC
    dw $0000, $0000     ; ED
    dw $0000, $0000     ; EE
    dw $0000, $0000     ; EF

    dw $0000, $0000     ; F0
    dw $0000, $0000     ; F1
    dw $0000, $0000     ; F2
    dw $0000, $0000     ; F3
    dw $0000, $0000     ; F4
    dw $0000, $0000     ; F5
    dw $0000, $0000     ; F6
    dw $0000, $0000     ; F7
    dw $0000, $0000     ; F8
    dw $0000, $0000     ; F9
    dw $0000, $0000     ; FA
    dw $0000, $0000     ; FB
    dw $0000, $0000     ; FC
    dw $0000, $0000     ; FD
    dw $0000, $0000     ; FE
    dw $8001, $0EF0     ; FF ; <sp>
        
                                ; VWF Data - disabled for now until more fixes can be done
    ;      props, offset
    ; dw $C800, $0000     ; 00
    ; dw $C800, $0000     ; 01    
    ; dw $C800, $0000     ; 02    
    ; dw $C800, $0000     ; 03    
    ; dw $C800, $0000     ; 04    
    ; dw $C800, $0000     ; 05    
    ; dw $C800, $0000     ; 06    
    ; dw $C800, $0000     ; 07    
    ; dw $C800, $0000     ; 08
    ; dw $C800, $0000     ; 09    
    ; dw $C800, $0000     ; 0A    
    ; dw $C800, $0000     ; 0B    
    ; dw $C800, $0000     ; 0C    
    ; dw $C800, $0000     ; 0D    
    ; dw $C800, $0000     ; 0E    
    ; dw $C800, $0000     ; 0F    

    ; dw $C800, $0000     ; 10
    ; dw $C800, $0000     ; 11    
    ; dw $C800, $0000     ; 12    
    ; dw $C800, $0000     ; 13    
    ; dw $C800, $0000     ; 14    
    ; dw $C800, $0000     ; 15    
    ; dw $C800, $0000     ; 16    
    ; dw $C800, $0000     ; 17    
    ; dw $C800, $0000     ; 18
    ; dw $C800, $0000     ; 19    
    ; dw $C800, $0000     ; 1A    
    ; dw $C800, $0000     ; 1B    
    ; dw $C800, $0000     ; 1C    
    ; dw $C800, $0000     ; 1D    
    ; dw $C800, $0000     ; 1E    
    ; dw $C800, $0000     ; 1F    

    ; dw $C800, $0000     ; 20
    ; dw $C800, $0000     ; 21    
    ; dw $C800, $0000     ; 22    
    ; dw $C800, $0000     ; 23    
    ; dw $C800, $0000     ; 24    
    ; dw $C800, $0000     ; 25    
    ; dw $C800, $0000     ; 26    
    ; dw $C800, $0000     ; 27    
    ; dw $C800, $0000     ; 28
    ; dw $C800, $0000     ; 29    
    ; dw $C800, $0000     ; 2A    
    ; dw $C800, $0000     ; 2B    
    ; dw $C800, $0000     ; 2C    
    ; dw $C800, $0000     ; 2D    
    ; dw $C800, $0000     ; 2E    
    ; dw $C800, $0000     ; 2F    

    ; dw $C601, $0400     ; 30 ; a
    ; dw $C601, $0410     ; 31   b
    ; dw $C601, $0420     ; 32   c
    ; dw $C601, $0430     ; 33   d
    ; dw $C601, $0440     ; 34   e
    ; dw $C601, $0450     ; 35   f
    ; dw $C601, $0460     ; 36   g
    ; dw $C601, $0470     ; 37   h
    ; dw $C301, $0480     ; 38   i
    ; dw $C501, $0490     ; 39   j
    ; dw $C601, $04A0     ; 3A   k
    ; dw $C301, $04B0     ; 3B   l
    ; dw $C701, $04C0     ; 3C   m
    ; dw $C601, $04D0     ; 3D   n
    ; dw $C601, $04E0     ; 3E   o
    ; dw $C601, $04F0     ; 3F   p

    ; dw $C601, $0600     ; 40   q
    ; dw $C501, $0610     ; 41   r
    ; dw $C601, $0620     ; 42   s
    ; dw $C601, $0630     ; 43   t
    ; dw $C601, $0640     ; 44   u
    ; dw $C701, $0650     ; 45   v
    ; dw $C701, $0660     ; 46   w
    ; dw $C701, $0670     ; 47   x
    ; dw $C701, $0680     ; 48   y
    ; dw $C601, $0690     ; 49 ; z   
    ; dw $C401, $06F0     ; 4A ; :   
    ; dw $C701, $0A10     ; 4B ; @ (thin)
    ; dw $C701, $0A30     ; 4C ; # (thin)
    ; dw $C801, $0AC0     ; 4D ; morphball left
    ; dw $C801, $0AD0     ; 4E ; morphball right
    ; dw $C401, $0EF0     ; 4F ; 4 width space

    ; dw $C800, $0000     ; 50
    ; dw $C800, $0000     ; 51    
    ; dw $C800, $0000     ; 52    
    ; dw $C800, $0000     ; 53    
    ; dw $C800, $0000     ; 54    
    ; dw $C800, $0000     ; 55    
    ; dw $C800, $0000     ; 56    
    ; dw $C800, $0000     ; 57    
    ; dw $C800, $0000     ; 58
    ; dw $C800, $0000     ; 59    
    ; dw $C800, $0000     ; 5A    
    ; dw $C800, $0000     ; 5B    
    ; dw $C800, $0000     ; 5C    
    ; dw $C800, $0000     ; 5D    
    ; dw $C800, $0000     ; 5E    
    ; dw $C800, $0000     ; 5F    

    ; dw $C800, $0000     ; 60
    ; dw $C800, $0000     ; 61    
    ; dw $C800, $0000     ; 62    
    ; dw $C800, $0000     ; 63    
    ; dw $C800, $0000     ; 64    
    ; dw $C800, $0000     ; 65    
    ; dw $C800, $0000     ; 66    
    ; dw $C800, $0000     ; 67    
    ; dw $C800, $0000     ; 68
    ; dw $C800, $0000     ; 69    
    ; dw $C800, $0000     ; 6A    
    ; dw $C800, $0000     ; 6B    
    ; dw $C800, $0000     ; 6C    
    ; dw $C800, $0000     ; 6D    
    ; dw $C800, $0000     ; 6E    
    ; dw $C800, $0000     ; 6F    

    ; dw $C800, $0000     ; 70
    ; dw $C800, $0000     ; 71    
    ; dw $C800, $0000     ; 72    
    ; dw $C800, $0000     ; 73    
    ; dw $C800, $0000     ; 74    
    ; dw $C800, $0000     ; 75    
    ; dw $C800, $0000     ; 76    
    ; dw $C800, $0000     ; 77    
    ; dw $C800, $0000     ; 78
    ; dw $C800, $0000     ; 79    
    ; dw $C800, $0000     ; 7A    
    ; dw $C800, $0000     ; 7B    
    ; dw $C800, $0000     ; 7C    
    ; dw $C800, $0000     ; 7D    
    ; dw $C800, $0000     ; 7E    
    ; dw $C800, $0000     ; 7F    

    ; dw $C800, $0000     ; 80
    ; dw $C800, $0000     ; 81    
    ; dw $C800, $0000     ; 82    
    ; dw $C800, $0000     ; 83    
    ; dw $C800, $0000     ; 84    
    ; dw $C800, $0000     ; 85    
    ; dw $C800, $0000     ; 86    
    ; dw $C800, $0000     ; 87    
    ; dw $C800, $0000     ; 88
    ; dw $C800, $0000     ; 89    
    ; dw $C800, $0000     ; 8A    
    ; dw $C800, $0000     ; 8B    
    ; dw $C800, $0000     ; 8C    
    ; dw $C800, $0000     ; 8D    
    ; dw $C800, $0000     ; 8E    
    ; dw $C800, $0000     ; 8F    

    ; dw $C800, $0000     ; 90
    ; dw $C800, $0000     ; 91    
    ; dw $C800, $0000     ; 92    
    ; dw $C800, $0000     ; 93    
    ; dw $C800, $0000     ; 94    
    ; dw $C800, $0000     ; 95    
    ; dw $C800, $0000     ; 96    
    ; dw $C800, $0000     ; 97    
    ; dw $C800, $0000     ; 98
    ; dw $C800, $0000     ; 99    
    ; dw $C800, $0000     ; 9A    
    ; dw $C800, $0000     ; 9B    
    ; dw $C800, $0000     ; 9C    
    ; dw $C800, $0000     ; 9D    
    ; dw $C800, $0000     ; 9E    
    ; dw $C800, $0000     ; 9F    

    ; dw $C801, $0800     ; A0
    ; dw $C801, $0810     ; A1    
    ; dw $C801, $0820     ; A2    
    ; dw $C801, $0830     ; A3    
    ; dw $C801, $0840     ; A4    
    ; dw $C801, $0850     ; A5    
    ; dw $C801, $0860     ; A6    
    ; dw $C801, $0870     ; A7    
    ; dw $C801, $0880     ; A8
    ; dw $C801, $0890     ; A9    
    ; dw $C601, $0000     ; AA  A
    ; dw $C601, $0010     ; AB  B  
    ; dw $C601, $0020     ; AC  C  
    ; dw $C601, $0030     ; AD  D 
    ; dw $C601, $0040     ; AE  E 
    ; dw $C601, $0050     ; AF  F 

    ; dw $C601, $0060     ; B0  G
    ; dw $C601, $0070     ; B1  H 
    ; dw $C301, $0080     ; B2  I 
    ; dw $C601, $0090     ; B3  J 
    ; dw $C601, $00A0     ; B4  K 
    ; dw $C601, $00B0     ; B5  L 
    ; dw $C701, $00C0     ; B6  M 
    ; dw $C601, $00D0     ; B7  N 
    ; dw $C601, $00E0     ; B8  O
    ; dw $C601, $00F0     ; B9  P 
    ; dw $C601, $0200     ; BA  Q 
    ; dw $C601, $0210     ; BB  R 
    ; dw $C601, $0220     ; BC  S 
    ; dw $C701, $0230     ; BD  T 
    ; dw $C601, $0240     ; BE  U 
    ; dw $C701, $0250     ; BF  V 

    ; dw $C701, $0260     ; C0  W
    ; dw $C701, $0270     ; C1  X 
    ; dw $C701, $0280     ; C2  Y 
    ; dw $C601, $0290     ; C3  Z  
    ; dw $C800, $0000     ; C4    
    ; dw $C800, $0000     ; C5   
    ; dw $C701, $06D0     ; C6  ?
    ; dw $C301, $06C0     ; C7  !  
    ; dw $C401, $02D0     ; C8  ,
    ; dw $C601, $02B0     ; C9  -  
    ; dw $C800, $0000     ; CA    
    ; dw $C800, $0000     ; CB    
    ; dw $C600, $02E0     ; CC  ...  
    ; dw $C401, $02C0     ; CD  .  
    ; dw $C701, $02F0     ; CE  ~  
    ; dw $C800, $0000     ; CF    

    ; dw $C800, $0000     ; D0
    ; dw $C800, $0000     ; D1    
    ; dw $C801, $06a0     ; D2  Link face left
    ; dw $C801, $06b0     ; D3  Link face right
    ; dw $C800, $0000     ; D4    
    ; dw $C800, $0000     ; D5    
    ; dw $C800, $0000     ; D6    
    ; dw $C800, $0000     ; D7    
    ; dw $C401, $06E0     ; D8  '
    ; dw $C800, $0000     ; D9    
    ; dw $C800, $0000     ; DA    
    ; dw $C800, $0000     ; DB    
    ; dw $C800, $0000     ; DC    
    ; dw $C800, $0000     ; DD    
    ; dw $C800, $0000     ; DE    
    ; dw $C800, $0000     ; DF    

    ; dw $C800, $0000     ; E0
    ; dw $C800, $0000     ; E1    
    ; dw $C800, $0000     ; E2    
    ; dw $C800, $0000     ; E3    
    ; dw $C801, $02A0     ; E4  Arrow >
    ; dw $C800, $0000     ; E5    
    ; dw $C800, $0000     ; E6    
    ; dw $C800, $0000     ; E7    
    ; dw $C800, $0000     ; E8
    ; dw $C800, $0000     ; E9    
    ; dw $C800, $0000     ; EA    
    ; dw $C800, $0000     ; EB    
    ; dw $C800, $0000     ; EC    
    ; dw $C800, $0000     ; ED    
    ; dw $C800, $0000     ; EE    
    ; dw $C800, $0000     ; EF    

    ; dw $C800, $0000     ; F0
    ; dw $C800, $0000     ; F1    
    ; dw $C800, $0000     ; F2    
    ; dw $C800, $0000     ; F3    
    ; dw $C800, $0000     ; F4    
    ; dw $C800, $0000     ; F5    
    ; dw $C800, $0000     ; F6    
    ; dw $C800, $0000     ; F7    
    ; dw $C800, $0000     ; F8
    ; dw $C800, $0000     ; F9    
    ; dw $C800, $0000     ; FA    
    ; dw $C800, $0000     ; FB    
    ; dw $C800, $0000     ; FC    
    ; dw $C800, $0000     ; FD    
    ; dw $C800, $0000     ; FE    
    ; dw $C801, $0EF0     ; FF ; Full width space 
