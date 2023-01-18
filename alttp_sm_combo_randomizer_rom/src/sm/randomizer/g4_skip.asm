; Set door asm pointer (Door going into the corridor before G4)
org $c38c5c
    db $00, $ea

; Door ASM to set the G4 open event bit if all major bosses are killed
org $cfea00
base $8fea00
    PHX
    ldx #$0000

    lda $7ed828
    bit.w #$0100
    beq + : inx

+   lda $7ed82c
    bit.w #$0001
    beq + : inx

+   lda $7ed82a
    and.w #$0101
    bit.w #$0001
    beq + : inx

+   bit.w #$0100
    beq + : inx

+   
    txa
    cmp.l config_sm_bosses
    bcc +

    lda $7ed820
    ora.w #$07C0
    sta $7ed820

+   PLX
    RTS