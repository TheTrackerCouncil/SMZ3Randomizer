; ALTTP extended SM items
;
; Big credits to the alttp randomizer code for adding new items
; that I borrowed a lot from :)
;

;org $08c5de
;    jml alttp_skip_item_text

org $410000
alttp_receive_sm_item:
    phx
    phy
    php
    %ai16()
    pha
    lda $02d8
    and #$00ff
    cmp #$00b0
    bcs +
    jmp .no_item
+
    sec
    sbc #$00b0
    asl : asl : asl
    tax

    lda.l alttp_sm_item_table+2,x     ; Load item type
    beq .equipment
    cmp #$0001
    beq .tank
    cmp #$0002
    beq .empty_tank
    cmp #$0004
    beq .ammo
    cmp #$0005 : bne + : brl .keycard : +
    jmp .no_item

.equipment
    lda.l alttp_sm_item_table,x       ; Load SRAM offset
    tay
    lda.l alttp_sm_item_table+4,x     ; Load value
    pha
    tyx
    ora.l !SRAM_SM_ITEM_BUF,x
    sta.l !SRAM_SM_ITEM_BUF,x
    pla
    ora.l !SRAM_SM_ITEM_BUF+$2,x
    sta.l !SRAM_SM_ITEM_BUF+$2,x    
    bra .prevent_spaz_plaz

.tank
    lda.l alttp_sm_item_table,x       ; Load SRAM offset
    tay
    lda.l alttp_sm_item_table+4,X     ; Load value
    tyx
    clc
    adc.l !SRAM_SM_ITEM_BUF+$2,x
    sta.l !SRAM_SM_ITEM_BUF+$2,x
    lda.l !SRAM_SM_ITEM_BUF+$2,x
    sta.l !SRAM_SM_ITEM_BUF,x             ; Refill Samus health fully when grabbing an e-tank 
    bra .end

.empty_tank
    lda.l alttp_sm_item_table,x       ; Load SRAM offset
    tay
    lda.l alttp_sm_item_table+4,X     ; Load value
    tyx
    clc
    adc.l !SRAM_SM_ITEM_BUF,x
    sta.l !SRAM_SM_ITEM_BUF,x
    bra .end

.ammo
    lda.l alttp_sm_item_table,x       ; Load SRAM offset
    tay
    lda.l alttp_sm_item_table+4,X     ; Load value
    pha
    tyx
    clc
    adc.l !SRAM_SM_ITEM_BUF,x
    sta.l !SRAM_SM_ITEM_BUF,x
    pla
    clc
    adc.l !SRAM_SM_ITEM_BUF+$2,x
    sta.l !SRAM_SM_ITEM_BUF+$2,x
    bra .end

.keycard
    lda.l alttp_sm_item_table,x       ; Load SRAM offset
    tay
    lda.l alttp_sm_item_table+4,x      ; Load value
    tyx
    ora.l !SRAM_SM_ITEM_BUF,x
    sta.l !SRAM_SM_ITEM_BUF,x
    bra .end

.end
    %ai16()

    ; jsl sm_fix_checksum        ; Correct SM's savefile checksum
    ; No need to fix checksum here since items don't save to the real SRAM anymore

.no_item
    pla
    plp
    ply
    plx
    rtl

; Prevents both spazer and plasma from being equipped
.prevent_spaz_plaz
    LDA.l #$0004
    TAX
    LDA.l !SRAM_SM_ITEM_BUF,x
    AND #$000C : CMP #$000C : BNE +
        LDA.l !SRAM_SM_ITEM_BUF,x
        AND #$FFFB
        STA.l !SRAM_SM_ITEM_BUF,x
    +
    BRA .end

alttp_sm_item_table:
    ;  offset type   value  extra
    dw $0000, $0000, $4000, $0000      ; Grapple
    dw $0000, $0000, $8000, $0000      ; X-Ray
    dw $0000, $0000, $0001, $0000      ; Varia Suit
    dw $0000, $0000, $0002, $0000      ; Springball
    dw $0000, $0000, $0004, $0000      ; Morphball
    dw $0000, $0000, $0008, $0000      ; Screw attack
    dw $0000, $0000, $0020, $0000      ; Gravity suit
    dw $0000, $0000, $0100, $0000      ; Hi-Jump
    dw $0000, $0000, $0200, $0000      ; Space Jump
    dw $0000, $0000, $1000, $0000      ; Bomb
    dw $0000, $0000, $2000, $0000      ; Speed booster
    
    dw $0004, $0000, $1000, $0000      ; Charge beam
    dw $0004, $0000, $0002, $0000      ; Ice beam
    dw $0004, $0000, $0001, $0000      ; Wave beam
    dw $0004, $0000, $0004, $0000      ; Spazer
    dw $0004, $0000, $0008, $0000      ; Plasma

;  $c0
    dw $0020, $0001,   100, $0000      ; E-Tank
    dw $0032, $0002,   100, $0000      ; Reserve-tank

    dw $0024, $0004,     5, $0000      ; Missiles
    dw $0028, $0004,     5, $0000      ; Super Missiles
    dw $002c, $0004,     5, $0000      ; Power Bombs

;  $c5
    dw $0000, $ffff, $0000, $0000      ; c5
    dw $0000, $ffff, $0000, $0000      ; c6
    dw $0000, $ffff, $0000, $0000      ; c7
    dw $0000, $ffff, $0000, $0000      ; c8
    dw $0000, $ffff, $0000, $0000      ; c9
    dw $0000, $ffff, $0000, $0000      ; ca
    dw $0000, $ffff, $0000, $0000      ; cb
    dw $0000, $ffff, $0000, $0000      ; cc
    dw $0000, $ffff, $0000, $0000      ; cd
    dw $0000, $ffff, $0000, $0000      ; ce
    dw $0000, $ffff, $0000, $0000      ; cf

; $d0
    dw $0070, $0000, $0001, $0000      ; keycard 0
    dw $0070, $0000, $0002, $0000      ; keycard 1
    dw $0070, $0000, $0004, $0000      ; keycard 2
    dw $0070, $0000, $0008, $0000      ; keycard 3
    dw $0070, $0000, $0010, $0000      ; keycard 4
    dw $0070, $0000, $0020, $0000      ; keycard 5
    dw $0070, $0000, $0040, $0000      ; keycard 6
    dw $0070, $0000, $0080, $0000      ; keycard 7
    dw $0070, $0000, $0100, $0000      ; keycard 8
    dw $0070, $0000, $0200, $0000      ; keycard 9
    dw $0070, $0000, $0400, $0000      ; keycard a 
    dw $0070, $0000, $0800, $0000      ; keycard b
    dw $0070, $0000, $1000, $0000      ; keycard c 
    dw $0070, $0000, $2000, $0000      ; keycard d
    dw $0070, $0000, $4000, $0000      ; keycard e 
    dw $0070, $0000, $8000, $0000      ; keycard f

