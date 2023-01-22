; Hold Fire
; Holding the item cancel button will toggle super missiles or power bombs
; Releasing will deselect the item

lorom

!LAST_INPUT = $7EFA02
!LAST_STATE = $7EFA04
!LAST_MORPH = $7EFA06

org $90c4bd
JML quick_toggle

ORG $90FA10
quick_toggle:
    .init
        ; Check if the player has pressed or released the item
        ; clear button 
        LDA $8b : AND $09B8 : CMP !LAST_INPUT : BEQ +
            STA !LAST_INPUT
            CMP $09B8 : BNE ++
                BRA .press_item_clear
            ++
            BRA .release_item_clear
        +

        ; If holding down the item clear button, check if the
        ; player has changed states
        CMP $09B8 : BNE +
            LDA $0A1F : AND #$00FF : CMP !LAST_STATE : BEQ +
                STA !LAST_STATE
                JSR check_morph : CMP !LAST_MORPH : BEQ +++
                    STA !LAST_MORPH
                    CMP #$0001 : BEQ ++++
                        BRA .unmorphed
                    ++++
                    BRA .morphed
                +++
            ++
        +

        JML $90C4C9

    .release_item_clear
        BRA .clear_item

    .press_item_clear
        LDA $0A1F : AND #$00FF : JSR check_morph : CMP #$0001 : BNE +
            BRA .check_power_bomb
        +
        BRA .check_missiles

    .morphed
        BRA .check_power_bomb

    .unmorphed
        BRA .check_missiles

    ; Checks if the player has power bombs ammo. If they don't, 
    ; don't select anything
    .check_power_bomb
        LDA $09CE : CMP #$0000 : BNE ++
            BRA .clear_item
        ++
        LDA #$0002
        BRA .select_item

    ; Checks if the player has super missile ammo. If they don't,
    ; check if they have missile ammo. If neither, select nothing
    .check_missiles
        LDA $09CA : CMP #$0000 : BNE +
            LDA $09C6 : CMP #$0000 : BNE ++
                BRA .clear_item
            ++
            LDA #$0000
            BRA .select_item
        +
        LDA #$0001
        BRA .select_item

    ; Selects nothing (default behavior)
    .clear_item
        STZ $0a04
        JML $90C4E9

    ; Selects an item by setting the currently selected item to the left
    ; of the item we want, then calling the default change selection code
    .select_item
        STA $09D2
        STZ $16
    JML $90c4f6

; Loads A with 1 if morphed or 0 otherwise
check_morph:
    .init
        CMP #$0004 : BEQ .morph ; Morph ball on ground
        CMP #$0008 : BEQ .morph ; Morph ball falling
        CMP #$0011 : BEQ .morph ; Spring ball on ground
        CMP #$0012 : BEQ .morph ; Spring ball in air
        CMP #$0013 : BEQ .morph ; Spring ball falling
        LDA #$0000 : RTS
    .morph
        LDA #$0001 : RTS
        
