org $c2ecbb
base $82ecbb
	jsr seed_display

org $c2f800
base $82f800
seed_display:
	pha
	phx
    php
    rep #$30

    lda.l $420000
    and #$003f
    asl
    asl
    asl
    tay
    ldx #$0000
-
    lda WordTable, y
    and #$00ff
    asl
    phx
    tax
    lda CharTable, x
    plx
    sta $7fc052, x
    inx
    inx
    iny
    cpx #$000E
    bne -

    lda.l $420001
    and #$003f
    asl
    asl
    asl
    tay
    ldx #$0000
-
    lda WordTable, y
    and #$00ff
    asl
    phx
    tax
    lda CharTable, x
    plx
    sta $7fc060, x
    inx
    inx
    iny
    cpx #$000E
    bne -

    lda.l $420002
    and #$003f
    asl
    asl
    asl
    tay
    ldx #$0000
-
    lda WordTable, y
    and #$00ff
    asl
    phx
    tax
    lda CharTable, x
    plx
    sta $7fc092, x
    inx
    inx
    iny
    cpx #$000E
    bne -

    lda.l $420003
    and #$003f
    asl
    asl
    asl
    tay
    ldx #$0000
-
    lda WordTable, y
    and #$00ff
    asl
    phx
    tax
    lda CharTable, x
    plx
    sta $7fc0a0, x
    inx
    inx
    iny
    cpx #$000E
    bne -
    
    plp
	plx
	pla
    ldx #$07fe
	rts

CharTable:
	; 0x00																				  	     0x0F
	dw $000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f
	; 0x10																				  	     0x0F
	dw $000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f,$000f
	; 0x20																				  	     0x0F
	dw $000f,$0084,$002d,$000f,$000f,$000f,$000f,$0022,$008a,$008b,$000f,$0086,$0089,$0087,$0088,$000f
NumTable:
	; 0x30																				  	     0x0F
	dw $0060,$0061,$0062,$0063,$0064,$0065,$0066,$0067,$0068,$0069,$000f,$000f,$000f,$000f,$000f,$0085
	; 0x40																				  	     0x0F
	dw $000f,$006a,$006b,$006c,$006d,$006e,$006f,$0070,$0071,$0072,$0073,$0074,$0075,$0076,$0077,$0078
	; 0x50																				  	     0x0F
	dw $0079,$007a,$007b,$007c,$007d,$007e,$007f,$0080,$0081,$0082,$0083,$000f,$000f,$000f,$000f,$000f








WordTable:
    db "GEEMER  "
    db "RIPPER  "
    db "ATOMIC  "
    db "POWAMP  "
    db "SCISER  "
    db "NAMIHE  "
    db "PUROMI  "
    db "ALCOON  "
    db "BEETOM  "
    db "OWTCH   "
    db "ZEBBO   "
    db "ZEELA   "
    db "HOLTZ   "
    db "VIOLA   "
    db "WAVER   "
    db "RINKA   "
    db "BOYON   "
    db "CHOOT   "
    db "KAGO    "
    db "SKREE   "
    db "COVERN  "
    db "EVIR    "
    db "TATORI  "
    db "OUM     "
    db "PUYO    "
    db "YARD    "
    db "ZOA     "
    db "FUNE    "
    db "GAMET   "
    db "GERUTA  "
    db "SOVA    "
    db "BULL    "
    db "ARRGI   "
    db "BABUSU  "
    db "BORU    "
    db "HACHI   "
    db "BABURU  "
    db "TAINON  "
    db "GERUDO  "
    db "GIBO    "
    db "KOPPI   "
    db "PON     "
    db "HOBA    "
    db "HYU     "
    db "KISU    "
    db "KYUNE   "
    db "RIBA    "
    db "MEDUSA  "
    db "TERU    "
    db "FANGIN  "
    db "PIKKU   "
    db "POPO    "
    db "NOMOSU  "
    db "GUZU    "
    db "AIGORU  "
    db "ROPA    "
    db "GAPURA  "
    db "HEISHI  "
    db "SUTARU  "
    db "TOZOKU  "
    db "TOPPO   "
    db "WAINDA  "
    db "KURIPI  "
    db "ZORA    "
