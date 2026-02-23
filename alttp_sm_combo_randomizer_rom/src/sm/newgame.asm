; This skips the intro
org $c2eeda
    db $1f

; Hijack init routine to autosave and set door flags
org $c28067
    jsl introskip_doorflags

org $c0fd00
base $80fd00
introskip_doorflags:
    ; Do some checks to see that we're actually starting a new game
    
    ; Make sure game mode is 1f
    lda $7e0998
    cmp.w #$001f
    beq +
    jmp .ret
+

    ; Check if samus saved energy is 00, if it is, run startup code
    lda $7ed7e2
    beq +
    jmp .ret

+
    ; Set construction zone and red tower elevator doors to blue
    lda $7ed8b6
    ora.w #$0004
    sta $7ed8b6    
    lda $7ed8b2
    ora.w #$0001
    sta $7ed8b2

    ; Unlock crateria map station door
    lda $7ed8b0
    ora.w #$0020
    sta $7ed8b0

    ; Unlock norfair map station door
    lda $7ed8b8
    ora.w #$1000
    sta $7ed8b8

    ; Set up open mode event bit flags
    lda.l config_events
    sta $7ed820
    
    lda #$0000
    sta.l !SRAM_SM_COMPLETED
    sta.l !SRAM_ALTTP_EQUIPMENT_1
    sta.l !SRAM_ALTTP_EQUIPMENT_2
    sta.l !SRAM_ALTTP_COMPLETED
    sta.l !SRAM_ALTTP_RANDOMIZER_SAVED
    sta.l !SRAM_ALTTP_FRESH_FILE
    sta.l !door_timer_tmp
    sta.l !door_adjust_tmp
    sta.l !add_time_tmp
    sta.l !region_timer_tmp
    sta.l !region_tmp
    sta.l !transition_tmp
    
    jsl stats_clear_values  ; Clear SM stats
    jsl alttp_new_game      ; Setup new game for ALTTP
    jsl sm_copy_alttp_items ; Copy alttp items into temporary SRAM buffer
    jsl zelda_fix_checksum  ; Fix alttp checksum    

    ; Clear multiworld seed data and reinitialize on new game.
    lda config_multiworld
    beq +
    lda #$0000
    ldx #$0000
-
    sta.l !SRAM_MW_SEED_DATA, x
    inx : inx
    cpx #$0050
    bne -
    jsl mw_init
+

    ; begin Leno edits here!
    LDA #$FFFF  ; decrement the accumulator by 1, making it #$FFFF
    sta.l $7ED908  ; activate Crateria and Brinstar maps
    ; sta.l $7ED909
    sta.l $7ED90A  ; activate Norfair and Wrecked Ship maps
    ; sta.l $7ED90B
    sta.l $7ED90C  ; activate Maridia and Tourian maps
    ; sta.l $7ED90D
    ; sta.l $7ED90E  Ceres and debug maps
    ; sta.l $7ED90F
    
    %a8()
    lda.b #$01
    sta $0789  ; this is used for the minimap, so blue tiles can show up on it. this also lets the main map scroll
    %a16()

    jsr init_sm_equipment

    ; Call the save code to create a new file
    lda $7e0952
    jsl $818000

.ret:   
    lda #$0000
    rtl

org $c0f800
base $80f800
init_sm_equipment:
    lda.l starting_equipment : sta $09a2 : sta $09a4 ; Equipment
    lda.l starting_equipment+$2 : sta $09a6 : sta $09a8 ; Beams
    lda.l starting_equipment+$4 : sta $09c2 : sta $09c4 ; Energy
    lda.l starting_equipment+$6 : sta $09c6 : sta $09c8 ; Missiles
    lda.l starting_equipment+$8 : sta $09ca : sta $09cc ; Supers
    lda.l starting_equipment+$a : sta $09ce : sta $09d0 ; Power Bombs
    rts

