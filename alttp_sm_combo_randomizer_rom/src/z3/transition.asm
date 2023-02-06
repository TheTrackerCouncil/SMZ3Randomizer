; Transition into Zelda


; Place all the transition code in the top of the upper bank AA/EA (free space in SM)
org $eaf800     
base $aaf800


; exit id in !SRAM_ALTTP_EXIT
; darkworld flag in !SRAM_ALTTP_DARKWORLD
transition_to_zelda:
    sei                         ; Disable IRQ's
    
    %a8()
    %i16()

    phk
    plb                         ; Set data bank program bank

    lda #$00
    sta $004200                 ; Disable NMI and Joypad autoread
    sta $00420c                 ; Disable H-DMA

    lda #$8f
    sta $002100                 ; Enable PPU force blank

    jsl zelda_spc_reset         ; Kill the SM music engine and put the SPC in IPL upload mode
                                ; Gotta do this before switching RAM contents

-
    bit $4212                   ; Wait for a fresh NMI
    bmi -

-
    bit $4212
    bpl -

    ldx.w #zelda_vram>>16       ; Put Zelda VRAM bank in X
    jsl copy_to_vram            ; Call the DMA routine to copy Zelda template VRAM from ROM

    ldx.w #zelda_wram>>16       ; Put Zelda VRAM bank in X
    jsl copy_to_wram            ; Call the DMA routine to copy Zelda template VRAM from ROM
    
    %ai16()
    
    ldx #$01ec
    txs                         ; Adjust stack pointer

    lda #$0000                  ; Set the "game flag" to Zelda so IRQ's/NMI runs using the 
    sta !SRAM_CURRENT_GAME      ; correct game

    jsr zelda_copy_sram         ; Copy SRAM back to RAM
    jsl zelda_fix_checksum
    jsl zelda_copy_sm_items     ; Copy SM items to temp buffer
    jsr zelda_spc_load          ; Load Zelda's music engine
    jsr zelda_blank_cgram       ; Blank out CGRAM
    jsr zelda_restore_dmaregs   ; Restore ALTTP DMA regs
    
    jsl zelda_restore_randomizer_ram

    lda !SRAM_ALTTP_EXIT
    sta $a0                     ; Store links house as exit

    lda !SRAM_ALTTP_DARKWORLD
    sta $7ef3ca                 ; Store lightworld/darkworld flag (0x0040 = dark world)

    %a8()
    sta $7b
    sta $a063ca
    cmp.b #$40
    bne +
    lda #$01
    sta $7e0fff
    lda #$0b
    sta $7e0aa4
    lda #$01
    sta $7e0ab3
    bra ++
+
    lda #$00
    sta $7e0fff
    sta $7e0ab3
    lda #$01
    sta $7e0aa4
++

    %a8()

    lda $7ef35a
    asl #6
    sta $7ef416                 ; Set progressive shield flag

    lda !SRAM_ALTTP_EQUIPMENT_1
    sta $000202
    lda !SRAM_ALTTP_EQUIPMENT_2
    sta $000303


    php
    jsl $0dfa78                 ; Redraw HUD
    jsl $00fc41
    %ai8()
    jsl $09c499                 ; Load all overworld sprites
    plp

    jsl $1cf37a                 ; Regenerate dialog pointers

    jsl DecompSwordGfx          ; Update sword graphics
    jsl DecompShieldGfx         ; Update shield graphics

    ; Load starting equipment, pre-open stuff
    LDA !SRAM_ALTTP_FRESH_FILE : BNE +
        JSL.l OnNewFile
        LDA.b #$FF : STA !SRAM_ALTTP_FRESH_FILE
    +

    lda #$ff
    sta $4201

    ;lda $13
    ;sta $2100

    lda $1c
    sta $212c
    lda $1d
    sta $212d
    lda $1e
    sta $212e
    lda $1f
    sta $212f
    lda $94
    sta $2105
    lda $95
    sta $2106
    lda $96
    sta $2123
    lda $97
    sta $2124
    lda $98
    sta $2125

    lda #$13
    sta $2107
    lda #$03
    sta $2108
    lda #$63
    sta $2109
    lda #$22
    sta $210b
    lda #$07
    sta $210c

    lda #$02
    sta $2101

    lda #$00
    sta $2102
    sta $2103

    lda #$81
    sta $4200                   ; Turn NMI/IRQ/Autojoypad read back on

    lda #$01
    sta $420d                   ; Toggle FastROM on (used for rando banks)


    %ai16()

    cli                         ; Enable interrupts and push processor status to the stack
    ;php   

    lda $4210                   ; Acknowledge any pending IRQ's
    
    pea $0707
    plb

    lda $a0
    sta $a2

    %ai8()

    lda $0114
    jsl $02a0be
    jsl $02b81d
    lda #$08

    lda #$08
    sta $010c
    lda #$0f
    sta $10
    stz $11
    stz $b0


    %ai16()

    ldx #$0000                  ; Restore overworld area and coordinate data
-
    lda.l !SRAM_ALTTP_OVERWORLD_BUF,x
    sta.l $7ec140,x
    inx
    inx
    cpx #$0032
    bne -


    lda #$0000
    ldx #$00ff
    ldy #$0000

    %ai8()
    jml $02b6fb                 ; Jump directly to pre-overworld module

zelda_spc_reset:
    pha
    php
    %a8()
    
    lda #$ff                    ; Send N-SPC into "upload mode"
    sta $2140

    lda #$00 : STA $2004
    lda #$00 : STA $2005

    rep #$30
    lda #$0000
    sta $12
    sta $14

    jsl $80800a
    db alttp_spc_data, (alttp_spc_data>>8)+$80, alttp_spc_data>>16

    plp
    pla
    rtl

zelda_spc_load:
    pha
    php

    %a8()

    ldx #$0000
-
    lda $00,x
    sta !SRAM_ALTTP_SPC_BUF,x
    inx
    cpx #$0100
    bne -

    lda #$00                    
    sta $00                     
    lda #$80                    
    sta $01
    lda #$19
    sta $02

    jsl alttp_load_music        ; Call the alttp SPC upload routine

    ldx #$0000
-
    lda !SRAM_ALTTP_SPC_BUF,x
    sta $00,x
    inx
    cpx #$0100
    bne -


    plp
    pla
    rts

zelda_copy_sram:
    pha
    phx
    phy
    php
    phb

    rep #$20
    
    ;lda $C8
    ;asl a
    ;inc
    ;inc
    lda #$0000
    sta $a07ffe     ; Always set save slot to 1 for now
    
    sep #$20
    %ai16()
    pea $7e7e
    plb
    plb
    ldy #$0000
    ldx #$0000
-
    LDA $a06000,X
    STA $F000,Y
    LDA $a06100,X
    STA $F100,Y
    LDA $a06200,X
    STA $F200,Y
    LDA $a06300,X
    STA $F300,Y
    LDA $a06400,X
    STA $F400,Y
    inx
    inx
    iny
    iny
    cpy #$0100
    bne -

    plb
    plp
    ply
    plx
    pla
    rts

zelda_blank_cgram:
    lda #$0000
    sta $2121
    ldx #$0000
-
    sta $2122
    inx
    cpx #$00ff
    bne -
    rts

zelda_fix_checksum:
    pha
    phx
    php
    %ai16()
    lda $00
    pha

    ldx #$0000              ; Copy main SRAM to backup SRAM
-
    lda.l $a06000,x
    sta.l $a06f00,x
    inx : inx
    cpx #$04fe
    bne -

    ldx #$0000
    lda #$0000
-
    clc
    adc $a06000,x
    inx
    inx
    cpx #$04fe
    bne -

    sta $00
    lda #$5a5a
    sec
    sbc $00
    ;sta $7EF4fe
    sta $a064fe
    sta $a073fe
    pla
    plp
    plx
    pla
    rtl

zelda_restore_dmaregs:
    php
    %ai16()
    ldx #$0000                  ; Restore overworld area and coordinate data
-
    lda.l zelda_dmaregs,x
    sta.l $004300,x
    inx
    inx
    cpx #$0080
    bne -
    plp
    rts

zelda_copy_sm_items:        
    pha
    phx
    php
    %ai16()
    ldx #$0000
-
    lda.l !SRAM_SM_START,x        
    sta.l !SRAM_SM_ITEM_BUF,X      ; save to temporary buffer
    inx : inx
    cpx #$0080
    bne -

    plp
    plx
    pla
    rtl

zelda_save_sm_items:        ; Restores SM items to the real SRAM
    pha
    phx
    phy
    php

    %ai16()
    ldx #$0000
-
    lda.l !SRAM_SM_ITEM_BUF,X    
    sta.l !SRAM_SM_START,x       
    inx : inx
    cpx #$0080
    bne -

    jsl sm_fix_checksum     ; Update SM checksum so the savefile doesn't get deleted

    plp
    ply
    plx
    pla
    rtl

zelda_save_start_hook:
    ; The save routine will be disrupted by NMI if this takes too long.
    ; Avoid doing anything time consuming here.
    lda #$01
    sta.l !SRAM_SAVING
    rtl

zelda_save_done_hook:
    jsl zelda_save_sm_items
    jsl mw_save_sram
    lda #$0000
    sta.l !SRAM_SAVING
    sep #$30
    plb
    rtl

;zelda_cgram:
;    incbin "../data/zelda-cgram.bin"

zelda_dmaregs:
    db $01, $18, $32, $ad, $7e, $4f, $01, $ff, $ff, $ff, $ff, $ff, $00, $00, $00, $ff
    db $01, $18, $80, $bb, $7e, $00, $00, $ff, $ff, $ff, $ff, $ff, $00, $00, $00, $ff
    db $01, $18, $c0, $bd, $7e, $00, $00, $ff, $ff, $ff, $ff, $ff, $00, $00, $00, $ff
    db $01, $18, $40, $b3, $7e, $00, $00, $ff, $ff, $ff, $ff, $ff, $00, $00, $00, $ff
    db $01, $18, $c0, $a5, $7e, $00, $00, $ff, $ff, $ff, $ff, $ff, $00, $00, $00, $ff
    db $ff, $ff, $ff, $ff, $ff, $ff, $ff, $ff, $ff, $ff, $ff, $ff, $00, $00, $00, $ff
    db $41, $26, $f6, $f2, $00, $ff, $ff, $00, $ff, $ff, $ff, $ff, $00, $00, $00, $ff
    db $41, $26, $f6, $f2, $00, $c2, $1c, $00, $fc, $f2, $8f, $ff, $00, $00, $00, $ff    

; Game state template data (banks e0-e7 = ALTTP, e8-ef = SM)
org $e00000
zelda_wram:
    incbin "../data/zelda-wram-lo-1.bin"
org $e10000
    incbin "../data/zelda-wram-lo-2.bin"
org $e20000
    incbin "../data/zelda-wram-hi-1.bin"
org $e30000
    incbin "../data/zelda-wram-hi-2.bin"

org $e40000
zelda_vram:
    incbin "../data/zelda-vram-1.bin"
org $e50000
    incbin "../data/zelda-vram-2.bin"
