; SM Multiworld support
;
;

; Implement auto-saving on death as a temporary measure to prevent lost items
; Game state 15h (Samus ran out of health, black out surroundings) ;;;
; JSL $9BB3A7[$9B:B3A7]
; org $C2DD7E ; $82DD7E
;    jsl sm_mw_autosave

org $C28BB3
    jsl sm_mw_hook_main_game

org $C09490
    jsl sm_mw_check_softreset : nop

org $f83000

sm_mw_nmi_read_messages:
    rep #$30
    lda.l config_multiworld
    beq +
    lda.l !MESSAGEBASE+$18E
    bne +
    jsl read_messages
+
    inc $05b8
    rtl

; Display message that we picked up someone elses item
; X = item id, Y = player id
sm_mw_display_item_sent:
    stx $c1
    sty $c3
    ;lda #$0168       ; With fanfare skip, no need to queue room track
    ;jsl $82e118      ; Queue room track after item fanfare
    lda #$005c
    jsl $858080
    rtl

sm_mw_receive_item:
    pha : phx
    cmp #$00b0                  ; If below B0 it's an alttp item
    bcc .alttpItem
    cmp #$00c5
    bcs .smCustomItem           ; Above C5 is custom SM items
    sec
    sbc #$00b0
    asl #4 : tax
    lda.l sm_item_table+$2, x     ; Read item flag
    sta $cc
    lda #$005d
    sta $ce
    lda.l #sm_item_table
    sta $ca
    txa : clc : adc $ca : tax
    ldy #$00cc
    jsl sm_mw_call_receive           ; Call original item receive code (reading the item to get from $cc-ce)
    plx : pla
    rts

.alttpItem
    lda #$0001
    sta.l !SM_MULTIWORLD_PICKUP
    jsl alttp_item_pickup
    plx : pla
    rts

.smCustomItem
    pha
    lda #$0001
    sta.l !SM_MULTIWORLD_PICKUP
    pla
    sec
    sbc #$00b0
    jsl receive_sm_item_long
    plx : pla
    rts

sm_mw_handle_queue:
    pha : phx

.loop
;     lda.l !SRAM_MW_RPTR
;     cmp.l !SRAM_MW_WPTR
;     beq .end
    
;     asl #2 : tax
;     lda.l !SRAM_MW_RECVQ, x : sta $c3
;     lda.l !SRAM_MW_RECVQ+$2, x : sta $c1
;     jsr sm_mw_receive_item

;     lda.l !SRAM_MW_RPTR
;     inc a
;     cmp #$0010
;     bne +
;     lda.l #$0000
; +
;     sta.l !SRAM_MW_RPTR

    lda.l !SRAM_MW_ITEMS_RECV_RPTR
    cmp.l !SRAM_MW_ITEMS_RECV_WPTR
    beq .end

    asl #2 : tax
    lda.l !SRAM_MW_ITEMS_RECV, x : sta $c3
    lda.l !SRAM_MW_ITEMS_RECV+$2, x : sta $c1
    jsr sm_mw_receive_item

    lda.l !SRAM_MW_ITEMS_RECV_RPTR
    inc a
    sta.l !SRAM_MW_ITEMS_RECV_RPTR

    bra .loop    

.end
    plx : pla
    rts

sm_mw_hook_main_game:
    jsl $A09169     ; Last routine of game mode 8 (main gameplay)
    
    lda.l config_multiworld
    beq +

    lda $0998
    cmp #$0008
    bne +
    jsr sm_mw_handle_queue     ; Handle MW RECVQ only in gamemode 8 
+
    rtl

sm_mw_autosave:
    lda.l $a16168 : pha             ; Backup save station variables
    lda.l $a16166 : pha             ; so the autosave saves to the last savestation

    lda $a16030                     ; Load old saved health and resave it
    bne +
    lda #$0063                      ; Load 99 if old saved health is 0 to prevent death loops
+
    sta $7e09c2
    lda #$0000
    jsl $818000                     ; Save SRAM
    pla : sta.l $a16166
    pla : sta.l $a16168             ; Set these values to 0 to force load from the ship if samus dies
    jsl sm_fix_checksum             ; Fix SRAM checksum (otherwise SM deletes the file on load)
    jsl $9BB3A7
    rtl

sm_mw_check_softreset:
    lda $8b
    cmp #$3030          ; Check if Start+Select+L+R are pressed
    bne +
    lda.l !SRAM_SAVING  ; Don't reset while saving to SRAM
    bne +
    lda $0617           ; Don't reset if uploading to the APU
    bne +
    lda $0998           ; Don't reset during SM boot or title screen
    cmp #$0002
    bcc +
    stz $4200           ; Disable NMI and joypad autoread
    jml $808462         ; Jump to SM soft-reset
+
    rtl
