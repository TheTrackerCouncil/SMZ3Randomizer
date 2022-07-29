; Common routines used by both games

; SNES ROM HEADER

org $00ffc0
    ;   0              f01234
    db "ALTTP+SM RANDOMIZER  "
    db $35, $02, $0D, $05, $00, $01, $00, $B1, $BC, $4E, $43

org $c0ffc0
    ;   0              f01234
    db "ALTTP+SM RANDOMIZER  "
    db $35, $02, $0D, $05, $00, $01, $00, $B1, $BC, $4E, $43

; Repoint all vectors so we can control in software what game to run

org $00ffe4
	dw brk
	dw brk
	dw brk
	dw nmi
	dw reset
	dw irq
	
org $00fff4
	dw brk
	dw brk
	dw brk
	dw nmi
	dw reset
	dw irq

org $c0ffe4
	dw brk
	dw brk
	dw brk
	dw nmi
	dw reset
	dw irq
	
org $c0fff4
	dw brk
	dw brk
	dw brk
	dw nmi
	dw reset
	dw irq

org $00cf50
brk:
	jml $808573

nmi:
	pha : php
    sep #$20
	lda !SRAM_CURRENT_GAME
    beq .alttp
	bmi .sm
    bpl .credits 
.alttp
    plp : pla
	jml $0080c9
.sm
    plp : pla
	jml $809583
.credits
    plp : pla
    jml credits_nmi

reset:              ; Always reset to SM
	sei
	clc
	xce
    %a16()
    lda #$00ff
    sta !SRAM_CURRENT_GAME
    lda #$0000
    sta !SRAM_SAVING
    %a8()
;	lda !SRAM_CURRENT_GAME
;	bne +
;	jml $008000
;+
    lda config_multiworld
    beq +
    jsl mw_init             ; Init multiworld    
+
    jsl init_randolive      ; Init randolive
	jml $80841c
;    jml credits_init

irq:
    jml irq_fastrom

alttp_load_music:
	sei
	jsr $8888
	cli
	rtl

org $c0fc00
base $80fc00
irq_fastrom:
    rep #$30
    phb
    pha
    lda.l !SRAM_CURRENT_GAME
    beq .alttp
    phx
    phy
    phk
    plb
    lda $4211
    ldx $ab
    jsr ($9616,x)
    sta $ab
    sty $4209
    stx $4207
    ply
    plx
    pla
    plb
    rti

.alttp
    sei
    pla
    plb
    jml $0082db    

org $f40000
copy_to_wram:       ; Copies 4 banks of ROM data to WRAM (start bank in X)

    %a8()           ; Make sure that NMI/IRQ's are off and PPU is off before calling this
    %i16()

    pla             ; Grab the return address from the stack and store it in SRAM    
    sta !SRAM_DMA_RET+$2

    pla
    sta !SRAM_DMA_RET+$1
    
    pla
    sta !SRAM_DMA_RET

    pea $0000       ; Set DB to $00 
    plb
    plb

    txa
    sta $4314       ; Store bank to DMA registers

    ldx #$0000
    stx $4312       ; Store source address

    ldx #$8000
    stx $4310       ; DMA A -> B (ROM -> WRAM)

    ldx #$8000
    stx $4315       ; Size (32768 bytes)

    ldx #$0000
    stx $2181       ; WRAM target address $0000

    lda #$00
    stx $2183       ; WRAM target bank (7e)

    lda #$02
    sta $420b       ; Start DMA

    ldx #$8000
    stx $2181       ; WRAM target address $8000

    
    ldx #$8000
    stx $2181       ; WRAM target address $8000
    
    lda #$00
    stx $2183       ; WRAM target bank (7e)
    
    inc $4314       ; Copy from next bank

    ldx #$8000
    stx $4315       ; Size (32768 bytes)

    ldx #$0000
    stx $4312       ; Store source address

    lda #$02
    sta $420b       ; Start DMA

    ldx #$0000
    stx $2181       ; WRAM target address $8000
    lda #$01
    sta $2183       ; WRAM bank 2 (7f)
    
    inc $4314       ; Copy from next bank
    
    ldx #$8000
    stx $4315       ; Size (32768 bytes)

    ldx #$0000
    stx $4312       ; Store source address

    lda #$02
    sta $420b       ; Start DMA

    ldx #$8000
    stx $2181       ; WRAM target address $8000
    lda #$01
    sta $2183       ; WRAM bank 2 (7f)
  
    inc $4314       ; Copy from next bank

    ldx #$8000
    stx $4315       ; Size (32768 bytes)

    ldx #$0000
    stx $4312       ; Store source address

    lda #$02
    sta $420b       ; Start DMA


    lda !SRAM_DMA_RET     ; Push return address to the "new" stack
    pha

    lda !SRAM_DMA_RET+$1     ; Push return address to the "new" stack
    pha
    
    lda !SRAM_DMA_RET+$2
    pha

    rtl             ; Return


copy_to_vram:       ; Copies 2 banks of ROM to VRAM (starting bank in X)
    pha
    phx
    php
    phb

    %a8()           ; Make sure that NMI/IRQ's are off and PPU is off before calling this
    %i16()

    pea $0000       ; Set DB to $00 
    plb
    plb

    lda #$01
    sta $2105
    
    lda #$80
    sta $2115

    txa
    sta $4314       ; Store bank to DMA registers

    ldx #$0000
    stx $4312       ; Store source address

    ldx #$1801
    stx $4310       ; DMA A -> B (ROM -> VRAM)

    ldx #$0000      ; VRAM address
    stx $2116

    ldx #$8000
    stx $4315       ; Size (32768 bytes)

    lda #$02
    sta $420b       ; Start DMA

    inc $4314       ; Next bank
    
    ldx #$4000
    stx $2116       ; WRAM address

    ldx #$8000
    stx $4315       ; Size (32768 bytes)

    ldx #$0000
    stx $4312       ; Store source address

    lda #$02
    sta $420b       ; Start DMA

    plb
    plp
    plx
    pla
    rtl



    
    