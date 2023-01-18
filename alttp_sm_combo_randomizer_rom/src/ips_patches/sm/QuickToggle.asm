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

    ; Annoyingly there's no good way of easily seeing if the player
    ; is morphed or not. We have to check a bunch of different states.
    LDA $0A1F : AND #$00FF
    CMP #$0004 : BEQ .check_power_bomb ; Morph ball on ground
    CMP #$0008 : BEQ .check_power_bomb ; Morph ball falling
    CMP #$0011 : BEQ .check_power_bomb ; Spring ball on ground
    CMP #$0012 : BEQ .check_power_bomb ; Spring ball in air
    CMP #$0013 : BEQ .check_power_bomb ; Spring ball falling

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
