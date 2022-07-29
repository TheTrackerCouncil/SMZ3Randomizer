; Check for specific caves and teleport to SM when entering one of those
;

org $02d70f
    jml zelda_check_teleport

org $f20000
zelda_check_teleport:
    phx
    pha

    ldx #$0000
-
    lda.l zelda_teleport_table,x
    beq .no_teleport
    cmp $a0
    beq .teleport
    inx
    inx
    inx
    inx
    inx
    inx
    bne -
    jmp .no_teleport
.teleport
    lda $7ec140
    cmp.l zelda_teleport_table+4,x
    bne .no_teleport
    jmp zelda_do_teleport
.no_teleport
    pla
    plx
    lda $cbb3,x
    sta $e8
    jml $02d714

zelda_do_teleport:
    inx
    inx
    lda.l zelda_teleport_table,x
    sta !SRAM_SM_EXIT                     ; Store SM door ID to SM for transition
    %a8()
    lda $000202
    sta !SRAM_ALTTP_EQUIPMENT_1
    lda $000303
    sta !SRAM_ALTTP_EQUIPMENT_2
    jsl $00894a                           ; Autosave ALTTP state

    jsr zelda_save_randomizer_ram
    jml transition_to_sm


zelda_save_randomizer_ram:
    php
    %ai16()
    ldx #$0000
-
    lda.l $7F5000,x
    sta.l !SRAM_ALTTP_RANDOMIZER_BUF,x
    inx
    inx
    cpx #$00d0
    bne -
    lda #$0000
    sta.l !SRAM_ALTTP_RANDOMIZER_SAVED
    plp
    rts

zelda_restore_randomizer_ram:
    pha
    phx
    php
    %ai16()
    lda.l !SRAM_ALTTP_RANDOMIZER_SAVED
    beq .end

    ldx #$0000
-
    lda.l !SRAM_ALTTP_RANDOMIZER_BUF,x
    sta.l $7F5000,x
    inx
    inx
    cpx #$00d0
    bne -

.end
    plp
    plx
    pla
    rtl


zelda_teleport_table:
    ; cave_id, door_id, 
    dw $0122, $8bce, $0035  ; Links house -> Parlor (from Crateria Map station)
    dw $00e5, $97c2, $0003  ; Death mountain cave -> Norfair map station
    dw $010e, $a894, $0077  ; Dark world ice rod cave -> Maridia missile refill
    dw $0115, NewLNRefillDoorData_entry, $0070  ; Misery mire right side -> LN GT Refill
    dw $0000
