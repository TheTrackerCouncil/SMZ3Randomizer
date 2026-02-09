; Set door asm pointer (Door going into the corridor before G4)
org $c38c5c
    db $00, $ea

; Door ASM to set the G4 open event bit if all major bosses are killed
org $cfea00
base $8fea00
    PHX
    ldx #$0000

    ; If another goal is set, check if the SRAM_GOAL_MET has been set
    ; Otherwise, perform normal boss check 
    lda.l config_other_goal 
    beq +
    
    lda.l !SRAM_GOAL_MET
    cmp #$0001
    bcs .done 

+   lda $7ed828
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

.done
    lda $7ed820
    ora.w #$07C0
    sta $7ed820

+   PLX
    RTS

org $e2aaa5
base $a2aaa5
    JSL $f26200

org $f26200
check_alt_goal:
    LDA.l config_other_goal 
    BEQ +

        PHX
        PHY
        PHP
        REP #$30

        LDA.l !SRAM_GOAL_MET
        CMP #$0001
        BCS .liftoff 

    .refuel
        PLP
        PLY
        PLX
        CLC
        RTL
    
    .liftoff
        PLP
        PLY
        PLX
        SEC
        RTL

+   JSL $808233
    RTL
    