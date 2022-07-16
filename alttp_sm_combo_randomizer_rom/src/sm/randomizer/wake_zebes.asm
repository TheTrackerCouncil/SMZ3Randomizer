; Door ASM pointer (Door into small corridor before construction zone)
org $c38eb4
    db $00, $eb

; Door ASM to set Zebes awake
org $cfeb00
base $8feb00
    lda $7ed872
    bit #$0400
    beq exit
    lda $7ed820
    ora.w #$0001
    sta $7ed820
    exit:
    rts