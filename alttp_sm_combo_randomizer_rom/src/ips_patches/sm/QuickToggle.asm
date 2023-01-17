; Quick Toggle
; Pressing the item cancel button when no weapon is selected will auto select super missiles or power bombs
; based on if you are morphed or not. If no super missiles, it will select missiles.

lorom

org $90c4c4
JML quick_toggle

ORG $90FA80
quick_toggle:
    ; If something is already selected, clear it
    LDA $09D2 : CMP #$0000 : BEQ +
        BRA .clear_item
    +

    ; If the player is in morph ball mode
    LDA $7E0A1C : BIT #$0040 : BEQ +
        
        ; If the player has no power bombs, don't select anything
        LDA $09CE : CMP #$0000 : BNE ++
            BRA .clear_item
        ++

        ; Otherwise select the power bombs
        LDA #$0002 : STA $7e00CC
        BRA .select_item
    +

    ; If the player has no super missiles, try missiles
    LDA $09CA : CMP #$0000 : BNE +

        ; If the player has no missiles, don't select anything
        LDA $09C6 : CMP #$0000 : BNE ++
            BRA .clear_item
        ++
        LDA #$0000
        BRA .select_item
    +

    ; Select super missiles
    LDA #$0001
    BRA .select_item

.clear_item
    STZ $0a04
    JML $90C4E9

.select_item
    STA $09D2
    STZ $16
    JML $90c4f6
