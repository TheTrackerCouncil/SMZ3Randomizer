;
; SM Keycard system (used for Keysanity)
;

; Hook initial drawing of pause screen to draw keycards status on BG3
; and make sure it draws only on the map screen and not equipment screen
org $C28D47
base $828D47
    jsl keycard_update_layer3

org $C291AD
base $8291AD
    jsl keycard_hide_maptext

org $C291D9
base $8291D9
    jsl keycard_show_maptext

org $C2933A
base $82933A
    jsl keycard_restore_vars

; Redirect the grey door preinstruction list so we can customize it
org $c4be43
base $84be43
    jsr keycard_greydoor_preinstruction_hook

; Pick out some unused space into the PLM bank for any custom things we might need
org $c4d410
base $84d410
keycard_plm:
    dw keycard_plm_setup, keycard_plm_instructions
keycard_door_plms:
    dw keycard_greydoor_setup, $BE70, $BE59
    dw keycard_greydoor_setup, $BED9, $BEC2
    dw keycard_greydoor_setup, $BF42, $BF2B
    dw keycard_greydoor_setup, $BFAB, $BF94

keycard_greydoor_preinstruction_hook:
    cpy #$0020      ; If grey door type is > E0, it's a keycard door
    bcs .keydoor
    lda.w $BE4B, y
    rts
.keydoor
    lda #keycard_greydoor_preinstruction
    rts

keycard_greydoor_preinstruction:
    lda $1e17, x        ; Load special argument
    clc : adc #$0060    ; Turn it into an index start at $D830 (event 128 and up)
    jsl $808233         ; Check if event has happened and go to linked instruction if it has
    bcc .nope
    jmp $bdb2
.nope
    jmp $bdc4

keycard_plm_load:
    phy : phx 
    lda $1dc7, x            ; Load room argument (keycard item id)
    sec : sbc #$00b0

    %a8()
    sta $4202
    lda #$0A
    sta $4203
    nop : nop : %ai16()
    lda $4216               ; Multiply it by 0x0A
    clc
    adc #item_graphics
    tay                     ; Add it to the graphics table and transfer into Y
    lda $0000, y
    jsr $8764               ; Jump to original PLM graphics loading routine
    plx
    ply
    rts

keycard_plm_instructions:
    dw keycard_plm_load     ; Load graphics
    dw $e04f                ; Draw
    dw $86bc                ; Delete

keycard_plm_setup:
    ldx $1C87,y             ;\
    lda #$0800              ;} Make PLM block a solid block with no BTS value
    jsr $82B4               ;/
    lda $7ED91A             ;\
    inc a                   ;} Increment global number of items loaded counter
    sta $7ED91A             ;/
    rts
warnpc $84d490

org $c486d1
base $8486d1

keycard_greydoor_setup:
    lda $1DC8, y
    and #$00FF
    clc : adc #$0020       ; Add $20 to mark this as a Keycard grey door
    sta $1E17, y

    lda $1DC7, y   
    and #$00FF
    clc : adc #$0100       ; Add $100 so we start at door 256 and up (unused)
    sta $1DC7, y   
    
    ldx $1C87, y   ;\
    lda #$C044     ;} Make PLM shotblock with BTS 44h (generic shot trigger)
    jsr $82B4      ;/
    rts    

warnpc $84870b

; Use free space in bank 8e for any code that doesn't have to be in the PLM bank
org $cef000
base $8ef000
keycard_update_layer3:
    ldx #$0EFE
    lda #$184E
-
    sta $7E4000,x
    dex #2
    bpl -

    lda config_keysanity
    beq keycard_maptext_skip
    jsr keycard_draw_maptext
keycard_maptext_skip:

    ldx $0330  
    lda #$0F00
    sta $D0,x  
    lda #$4000
    sta $D2,x  
    lda #$007E
    sta $D4,x  
    lda #$5880
    sta $D5,x  
    txa
    clc
    adc #$0007
    sta $0330

    lda $5b
    sta $87     ; Backup $5b
    lda #$005A
    sta $5b

    rtl

keycard_restore_vars:
    lda $87
    sta $5b
    jsl $80834B
    rtl

keycard_hide_maptext:
    lda #$005C
    sta $5b
    jsl $82bb30
    rtl

keycard_show_maptext:
    lda #$005A
    sta $5b
    jsl $82bb30
    rtl

keycard_draw_maptext:
    phx : phy : phb
    phk : plb

    ldx #$4372
    ldy #$0000
    lda #$0080
    sta $14

.loop_three
    ; Draw a region
    lda .regions, y
    sta $7E0000, x
    inx #4
    
    ; Draw the three keys for the region
    phy
    ldy #$0000
-
    lda $14
    jsl $808233
    bcc +
    lda .cards, y
    sta $7E0000, x
+
    inx #2
    iny #2
    inc $14
    cpy #$0006
    bne -
    ply

    txa : clc : adc #$0036 : tax
    iny #2
    cpy #$0008
    bne .loop_three

.loop_two
    ; Draw a region
    lda .regions, y
    sta $7E0000, x
    inx #4
    
    ; Draw the two keys for the region
    phy
    ldy #$0000
-
    lda $14
    jsl $808233
    bcc +
    lda .cards, y
    sta $7E0000, x
+
    inx #4
    iny #4
    inc $14
    cpy #$0008
    bne -
    ply

    txa : clc : adc #$0034 : tax
    iny #2
    cpy #$000c
    bne .loop_two

    plb : ply : plx
    rts


.regions
table box.tbl,rtl
    dw "CBNMWL"
.cards
    dw $2C00
    dw $2801
table box_yellow.tbl,rtl
    dw "B"
cleartable

warnpc $8fffff
