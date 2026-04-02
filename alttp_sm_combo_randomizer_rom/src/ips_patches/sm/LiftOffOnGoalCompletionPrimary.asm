lorom

org $a2aaa5
    JSL check_goal

org $92ef00
check_goal:
    PHX
    PHY
    PHP
    REP #$30

    ; Count number of set SM boss flags
    ; using the new event bits for the SM boss credits

    lda $7ed832
    stz $12
    ldx #$0000
-
    lsr : bcc +
    inc $12 ; If C is set, count this as a killed boss
+
    inx
    cpx #$0004
    bne -

    ; Check if the number of Metroid bosses has been met
    lda $12
    cmp.l $F47008 ; config_sm_bosses
    bcc .refuel

    ; Count number of crystals using new bits
    lda $a17b7A
    stz $12
    ldx #$0000

    -
    lsr : bcc +
    inc $12 ; If C is set, count this as a killed boss
+
    inx
    cpx #$0007
    bne -

    ; Check if the number of Metroid bosses has been met
    lda $12
    cmp.l $F47012 ; config_ganon_crystals
    bcc .refuel
    bra .liftoff

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