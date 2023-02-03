lorom

!AIM_UP_BUTTON = $09BE
!AIM_DOWN_BUTTON = $09BC
!UP_BUTTON = $09AA
!DOWN_BUTTON = $09AC

!AIM_DIRECTION = $7EFA0A
!AIMING_UP = #$0000
!AIMING_DOWN = #$0001
!QUICK_MORPH_TYPE = $7EFA0C
!MORPHING = #$0000
!UNMORPHING = #$0001

ORG $9181B0
JML UnifiedAim

org $83B000
UnifiedAim:
    .init

        ; Skip if hitting the buttons required for a crystal flash
        LDA $8B : CMP #$1430 : BNE +
            JMP UnifiedAimExit
        +

        ; Load previous aim direction
        LDA !AIM_DIRECTION : TAX

        ; Set aim up and down values based on new inputs
        LDA $8F : BIT !AIM_UP_BUTTON : BEQ +
            LDA !AIMING_UP : STA !AIM_DIRECTION
            LDA $8b : BIT !DOWN_BUTTON : BEQ +
            LDA !AIMING_DOWN : STA !AIM_DIRECTION
        + 
        LDA $8F : BIT !UP_BUTTON : BEQ +
            LDA !AIMING_UP : STA !AIM_DIRECTION
        +
        LDA $8F : BIT !DOWN_BUTTON : BEQ +
            LDA $0A1F : AND #$00FF : CMP #$0016 : BEQ + ; skip if grappling
            LDA !AIMING_DOWN : STA !AIM_DIRECTION
        +

        ; If the user is not pressing aim up button
        LDA $8B : BIT !AIM_UP_BUTTON : BNE +
            ; Record them as not pressing either aim up or aim down
            LDA $12 : ORA #$0030 : STA $12
            LDA $14 : ORA #$0030 : STA $14

            ; If the player is pressing aim down, execute the quick morph code
            LDA $8B : BIT !AIM_DOWN_BUTTON : BNE .quick_morph

            BRA .exit
        +

        ; Check if the user needs to crouch/uncround/morph/unmorph
        ; And set the correct L/R values based on if aiming up or down
        LDA $8F : BIT !UP_BUTTON : BNE .pressed_up
        LDA $8F : BIT !DOWN_BUTTON : BNE .pressed_down
        BRA .set_aim_buttons

    .quick_morph
        JML QuickMorph
        
    .pressed_up
        ; If the player is already aiming up, treat like vanilla
        ; so that they uncrouch/unmorph
        CPX !AIMING_UP : BEQ .exit

        ; If morphed and press up, treat like normal
        JSR check_morph : CMP #$0001 : BEQ .exit

        BRA .set_aim_buttons

    .pressed_down
        ; If the player is already aiming down, treat like vanilla
        ; so that they crouch/morph
        CPX !AIMING_DOWN : BNE +
            LDA $12 : AND #$FFDF : ORA #$0010 : STA $12
            LDA $14 : AND #$FFDF : ORA #$0010 : STA $14
            BRA .exit
        +

        BRA .set_aim_buttons

    .set_aim_buttons
        ; If in aim down mode, tell the game that the player is pressing the aim
        ; down button but not the aim up button even though they aren't
        ; Also, remove the up/down inputs
        LDA !AIM_DIRECTION : CMP !AIMING_DOWN : BNE +
            LDA $12 : AND #$FFDF : ORA #$0C10 : STA $12
            LDA $14 : AND #$FFDF : ORA #$0C10 : STA $14
            BRA .exit
        +
        ; Otherwise tell the game that the player is pressing the aim up button
        ; but not the aim down button
        ; Also, remove the up/down inputs
        LDA $12 : AND #$FFEF : ORA #$0C20 : STA $12
        LDA $14 : AND #$FFEF : ORA #$0C20 : STA $14
        BRA .exit

    .exit
        JMP UnifiedAimExit

QuickMorph:
    ; If the player just pressed aim down, check if they
    ; need to morph or unmorph
    LDA $8F : BIT !AIM_DOWN_BUTTON : BEQ +
        JSR check_morph : STA !QUICK_MORPH_TYPE
        LDA !QUICK_MORPH_TYPE : CMP !MORPHING : BNE +
        LDA $12 : ORA #$0380 : STA $12
        LDA $14 : ORA #$0380 : STA $14
    +
    ; Set appropriate input/output based on if morphing
    ; or unmorphing
    LDA !QUICK_MORPH_TYPE : CMP !MORPHING : BEQ +
        LDA $12 : AND #$F7FF : STA $12
        LDA $14 : AND #$F7FF : STA $14
        JMP UnifiedAimExit
    +
    LDA $12 : AND #$FBFF : STA $12
    LDA $14 : AND #$FBFF : STA $14
    JMP UnifiedAimExit

; Jumps back into the vanilla state change code
UnifiedAimExit:
    LDA $0A1C
    ASL A
    TAX
    JML $9181B5

; Loads A with 1 if morphed or 0 otherwise
check_morph:
    .init
        LDA $0A1F : AND #$00FF ; Load the player state
        CMP #$0004 : BEQ .morph ; Morph ball on ground
        CMP #$0008 : BEQ .morph ; Morph ball falling
        CMP #$0011 : BEQ .morph ; Spring ball on ground
        CMP #$0012 : BEQ .morph ; Spring ball in air
        CMP #$0013 : BEQ .morph ; Spring ball falling
        LDA #$0000 : RTS
    .morph
        LDA #$0001 : RTS