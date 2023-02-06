; Transition into Super Metroid

org $c18003
base $818003
    jml sm_save_hook

org $c1807F
base $81807F
    jml sm_save_done_hook

org $c18087
base $818087
    jml sm_load_hook


; Place all the transition code in the top of the upper bank AA/EA (free space in SM)
org $eafd00
base $aafd00

; room id in !SRAM_SM_EXIT
; When we're entering this routine, ALTTP is running but all the relevant SRAM data
; and extra state data has already been saved, so all we do here is setup SM in a 
; proper state and then trigger the door transition.
transition_to_sm:
    sei                         ; Disable IRQ's
    
    %i16()
    %a8()

    phk
    plb                         ; Set data bank program bank

    lda #$01
    sta $420d                   ; Toggle FastROM on

    lda #$00
    sta $004200                 ; Disable NMI and Joypad autoread
    sta $00420c                 ; Disable H-DMA

    lda #$8f
    sta $002100                 ; Enable PPU force blank

    jsl sm_spc_reset            ; Kill the ALTTP music engine and put the SPC in IPL upload mode
                                ; Gotta do this before switching RAM contents

-
    bit $4212                   ; Wait for a fresh NMI
    bmi -

-
    bit $4212
    bpl -

    %a16()
    lda !SRAM_SM_EXIT
    tax
    lda $830003,x	            ; Load door direction bit
	and #$0003
    beq +

    %a8()
    ldx.w #sm_vram>>16          ; Put SM VRAM bank in X
    jsl copy_to_vram            ; Call the DMA routine to copy SM template VRAM from ROM

    ldx.w #sm_wram>>16          ; Put SM WRAM bank in X
    jsl copy_to_wram            ; Call the DMA routine to copy SM template WRAM from ROM
    jmp ++

+
    %a8()
    ldx.w #sm_vram_right>>16    ; Put SM VRAM bank in X
    jsl copy_to_vram            ; Call the DMA routine to copy SM template VRAM from ROM

    ldx.w #sm_wram_right>>16    ; Put SM WRAM bank in X
    jsl copy_to_wram            ; Call the DMA routine to copy SM template WRAM from ROM

++
    %ai16()

    ldx #$1ff0
    txs                         ; Adjust stack pointer

    lda #$ffff                  ; Set the "game flag" to SM so IRQ's/NMI runs using the 
    sta !SRAM_CURRENT_GAME      ; correct game

    jsl sm_fix_checksum         ; Fix SRAM checksum (otherwise SM deletes the file on load)
    
    lda #$0000
    jsl $818085                 ; Load SRAM contents back into RAM

    ; jsl $80858c                 ; Update map
    ; removing this call because the region is not set correctly at this point
    ; we will be moving it to another location

    jsl $809a79                 ; Redraw HUD

    jsr sm_spc_load             ; Load SM's music engine

    lda !SRAM_SM_EXIT
    sta $078d                   ; Store the selected door index

    jsl sm_setup_door

    %ai16()

    lda #$000b
    sta $0998                   ; Set game mode to loading door

    lda #$e29e
    sta $099c

    lda #$0001
    sta.l $7fff10               ; Set this transition to not count for stats

    lda #$001b                  ; Add transition to SM
    jsl inc_stat

    %a8()

    lda $84
    sta $4200                   ; Turn NMI/IRQ/Autojoypad read back on


    %ai16()

    cli                         ; Enable interrupts and push processor status to the stack
    php

    lda $4210                   ; Acknowledge any pending IRQ's
    pea $8282
    plb
    plb
    jml $82897a                 ; Put game directly into "Wait for IRQ" in the main game loop

sm_spc_reset:
    pha
    php
    %a8()
    lda #$ff                    ; Send N-SPC into "upload mode"
    sta $2140

    lda #$00 : STA $2004
    lda #$00 : STA $2005

    lda.b #sm_spc_data            ; Store the location of our "exploit data"
    sta $00                     ; so that the ALTTP music upload routine
    lda.b #sm_spc_data>>8         ; uses it.
    sta $01
    lda.b #sm_spc_data>>16
    sta $02

    jsl alttp_load_music        ; Call the alttp SPC upload routine
    plp
    pla
    rtl

sm_spc_load:
    jsl $80800a                 ; Call the SM SPC upload routine with the parameter set to
    dl $cf8000                  ; the whole full music engine and samples.
    rts

sm_fix_checksum:
    pha
    phx
    phy
    php

    %ai16()
    
    lda $14
    pha
    stz $14
    ldx #$0010
 -
    lda.l $a16000,x
    clc
    adc $14
    sta $14
    inx
    inx
    cpx #$065c
    bne -

    ldx #$0000
    lda $14
    sta.l $a16000,x
    sta.l $a17ff0,x
    eor #$ffff
    sta.l $a16008,x
    sta.l $a17ff8,x
    pla
    sta $14

    plp
    ply
    plx
    pla
    rtl

sm_save_hook:
    phb : phx : phy : pha
    pea $7e00
    plb
    plb
    lda #$0001
    sta.l !SRAM_SAVING
    jsl sm_save_alttp_items
    jsl stats_save_sram
    jsl mw_save_sram
    pla
    jml $81800b

sm_save_done_hook:
    lda #$0000
    sta.l !SRAM_SAVING
    ply : plx : clc : plb : plp
    rtl

sm_load_hook:
    phb : phx : phy
    pea $7e00
    plb
    plb
    jsl sm_copy_alttp_items
    jsl stats_load_sram
    jsl mw_load_sram
    jml $81808f

sm_copy_alttp_items: ; Copies ALTTP items into a temporary SRAM buffer used when SM writes data to ALTTP (so that when Samus dies, alttp progress doesn't stay)
    pha
    phx
    php
    %a16()
    ldx #$0000
-
    lda.l !SRAM_ALTTP_START+$300,x    ; copy 300-3FF from ALTTP SRAM
    sta.l !SRAM_ALTTP_ITEM_BUF,X      ; save to temporary buffer
    inx : inx
    cpx #$0100
    bne -
    ldx #$0000
-
    lda.l !SRAM_ALTTP_START+$420,x    ; copy 420-467 from ALTTP SRAM (reference z3/randomizer/stats.asm)
    sta.l !SRAM_ALTTP_STATS_BUF,x     ; save to temporary buffer
    inx : inx
    cpx #$0048
    bne -
    ldx #$0000
-
    lda.l !SRAM_ALTTP_START+$4E0,x    ; copy 4E0-4EF from ALTTP SRAM for dungeon small key count
    sta.l !SRAM_ALTTP_SMALLKEY_BUF,x  ; save to temporary buffer
    inx : inx
    cpx #$0010
    bne -

    plp
    plx
    pla
    rtl

sm_save_alttp_items: ; Restores ALTTP items to the real SRAM
    pha
    phx
    php
    %a16()
    ldx #$0000
-
    lda.l !SRAM_ALTTP_ITEM_BUF,X      ; save to temporary buffer
    sta.l !SRAM_ALTTP_START+$300,x    ; copy 300-3FF from ALTTP SRAM
    inx : inx
    cpx #$0100
    bne -
    ldx #$0000
-
    lda.l !SRAM_ALTTP_STATS_BUF,x     ; copy from temporary buffer
    sta.l !SRAM_ALTTP_START+$420,x    ; save to 420-467 in ALTTP SRAM
    inx : inx
    cpx #$0048
    bne -
    ldx #$0000
-
    lda.l !SRAM_ALTTP_SMALLKEY_BUF,x  ; copy from temporary buffer
    sta.l !SRAM_ALTTP_START+$4E0,x    ; save to 4E0-4EF from ALTTP SRAM for dungeon small key count
    inx : inx
    cpx #$0010
    bne -

    plp
    plx
    pla
    rtl




sm_spc_data:        ; Upload this data to the SM music engine to kill it and put it back into IPL mode
    dw $002a, $0b00
    db $8f, $6c, $f2 
    db $8f, $e0, $f3 ; Disable echo buffer writes and mute amplifier
    db $8f, $7c, $f2 
    db $8f, $ff, $f3 ; ENDX
    db $8f, $7d, $f2 
    db $8f, $00, $f3 ; Disable echo delay
    db $8f, $4d, $f2 
    db $8f, $00, $f3 ; EON
    db $8f, $5c, $f2 
    db $8f, $ff, $f3 ; KOFF
    db $8f, $5c, $f2 
    db $8f, $00, $f3 ; KOFF
    db $8f, $80, $f1 ; Enable IPL ROM
    db $5f, $c0, $ff ; jmp $ffc0
    dw $0000, $0a00

org $c2f710
base $82f710
sm_setup_door:
    php                         ; This runs some important routines to update the RAM with
    phb                         ; needed values for the door transition to work at all
    rep #$30            
    pea $8f00
    plb
    plb
    jsr $dfc7
    jsr $ddf1
    jsr $de12
    jsr $de6f  ; ADDING this call here. it will set the region number. this is called every door transition
    jsr $def2
    jsr $d961
    jsl $80858c  ; and then we're adding in the map restoration here

    plb
    plp
    rtl

; Game state template data (banks e0-e7 = ALTTP, e8-ef = SM)
org $e60000
sm_wram:
    incbin "../data/sm-wram-lo-1.bin"
org $e70000
    incbin "../data/sm-wram-lo-2.bin"
org $e80000
    incbin "../data/sm-wram-hi-1.bin"
org $e90000
    incbin "../data/sm-wram-hi-2.bin"

org $ea0000
sm_wram_right:
    incbin "../data/sm-wram-right-lo-1.bin"
org $eb0000
    incbin "../data/sm-wram-right-lo-2.bin"
org $ec0000
    incbin "../data/sm-wram-right-hi-1.bin"
org $ed0000
    incbin "../data/sm-wram-right-hi-2.bin"

org $ee0000
sm_vram:
    incbin "../data/sm-vram-1.bin"
org $ef0000
    incbin "../data/sm-vram-2.bin"

org $f00000
sm_vram_right:
    incbin "../data/sm-vram-right-1.bin"
org $f10000
    incbin "../data/sm-vram-right-2.bin"
