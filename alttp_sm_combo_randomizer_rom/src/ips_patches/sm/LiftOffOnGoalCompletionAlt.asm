lorom

org $a2aaa5
    JSL check_alt_goal

org $92ef00
check_alt_goal:
    PHX
    PHY
    PHP
    REP #$30

    LDA.l $a176f0 ; SRAM_GOAL_MET
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