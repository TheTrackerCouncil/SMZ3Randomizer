lorom

org $a2aaa5
    JSL check_goal

org $92ef00
check_goal:
    PHX
    PHY
    PHP
    REP #$30

    LDA $A17B74 : AND #$0007 : CMP #$0007 : BNE .refuel ; require all pendants
    LDA $A17B7A : AND #$007F : CMP #$007F : BNE .refuel ; require all crystals
    LDA $7ED832 : AND #$000F : CMP #$000F : BNE .refuel ; required all boss tokens
    LDA $A17BC5 : CMP #$0003 : BCC .refuel ; require post-aga world state
    LDA $A17ADB : AND #$0020 : CMP #$0020 : BNE .refuel ; require aga2 defeated (pyramid hole open)

    BRA .liftoff

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