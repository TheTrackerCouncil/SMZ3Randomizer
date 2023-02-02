; Hold Fire
; Holding the item cancel button will toggle super missiles or power bombs
; Releasing will deselect the item

lorom

!LAST_INPUT = $7EFA02
!LAST_STATE = $7EFA04
!LAST_MORPH = $7EFA06
!MISSILE_TYPE = $7EFA08
!MISSILE = #$0000
!SUPER_MISSILE = #$0001
!SLOT_MISSILE = #$0000
!SLOT_SUPER_MISSILE = #$0001
!SLOT_POWER_BOMB = #$0002
!SUPERS_ONLY = #$0000
!HOLD_TYPE = $90FF50

macro branch_no_missiles(branch)
	LDA $09C6 : CMP #$0000 : BEQ <branch>
endmacro

macro branch_no_supers(branch)
	LDA $09CA : CMP #$0000 : BEQ <branch>
endmacro

macro branch_no_power_bombs(branch)
	LDA $09CE : CMP #$0000 : BEQ <branch>
endmacro

macro select_missiles()
	LDA !SLOT_MISSILE : BRA .select_item
endmacro

macro select_supers()
	LDA !SLOT_SUPER_MISSILE : BRA .select_item
endmacro

macro select_power_bombs()
	LDA !SLOT_POWER_BOMB : BRA .select_item
endmacro

org $90c4bd
JML quick_toggle

org $90c4e0
JML item_select

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
        JSR check_morph : CMP #$0001 : BNE +
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
        %branch_no_power_bombs(+)
            %select_power_bombs()    
        +
        BRA .clear_item

    ; Checks if the player has super missile ammo. If they don't,
    ; check if they have missile ammo. If neither, select nothing
    .check_missiles
        ; If Fusion-style is set, don't check the missile type and simply try supers
        LDA !HOLD_TYPE : CMP !SUPERS_ONLY : BEQ +

        ; If the user was previous on missiles, try to select them first
        LDA !MISSILE_TYPE : CMP !MISSILE : BNE +
            %branch_no_missiles(++)
                %select_missiles()
            ++
            %branch_no_supers(++)
                %select_supers()
            ++
            BRA .clear_item
        +
        ; Otherwise, try to select supers first
        %branch_no_supers(+)
            %select_supers()
        +
        %branch_no_missiles(+)
            %select_missiles()
        +
        BRA .clear_item

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
        LDA $0A1F : AND #$00FF
        CMP #$0004 : BEQ .morph ; Morph ball on ground
        CMP #$0008 : BEQ .morph ; Morph ball falling
        CMP #$0011 : BEQ .morph ; Spring ball on ground
        CMP #$0012 : BEQ .morph ; Spring ball in air
        CMP #$0013 : BEQ .morph ; Spring ball falling
        LDA #$0000 : RTS
    .morph
        LDA #$0001 : RTS
        
item_select:
    .init
        ; If the player is not pressing the item cancel button, do the normal behavior
        LDA $8b : AND $09B8 : CMP $09B8 : BEQ +
            LDA $09D2 ; Load the current item selected
            JML $90C4F9 ; Jump to where it increments the current item
        +

        ; If supers only is set, select the previously selected item
        LDA !HOLD_TYPE : CMP !SUPERS_ONLY : BNE +
            LDA $09D2 : DEC
            BRA .select_item
        +

        ; If morphed, keep the current item
        JSR check_morph : CMP #$0001 : BNE +
            %branch_no_power_bombs(++)
                %select_power_bombs()    
            ++
            BRA .clear_item
        +

        ; Load the current selected item
        LDA $09D2
        
        ; If not morphed and missiles are selected
        CMP #$0001 : BNE +
            ; If the player has super missiles, switch to them
            %branch_no_supers(++)
                LDA !SUPER_MISSILE : STA !MISSILE_TYPE
                %select_supers()
            ++
            
            ; If the player has missiles, stay on them
            %branch_no_missiles(++)
                %select_missiles()
            ++

            ; Otherwise clear the item
            BRA .clear_item
        +
        ; If not morphed and super missiles are selected
        CMP #$0002 : BNE +
            ; If the player has missiles, switch to them
            %branch_no_missiles(++)
                LDA !MISSILE : STA !MISSILE_TYPE
                %select_missiles()
            ++

            ; If the player has super missiles, stay on them
            %branch_no_supers(++)
                %select_supers()
            ++

            ; Otherwise clear the item
            BRA .clear_item
        +
        
        BRA .clear_item

    ; Selects an item by setting the currently selected item to the left
    ; of the item we want, then calling the default change selection code
    .select_item
        STA $09D2
        STZ $16
        JML $90c4f6

    ; Selects nothing
    .clear_item
        STZ $0a04
        JML $90C4E9

    


