org $cfc922
    jml sm_check_ending_door

org $e9B1D0
    jsl sm_check_ending_mb

org $e9B305
    jml sm_check_ending_mb_event

org $c09E1C
    jml sm_check_ending_mb_timer

org $e9B33C
    jml sm_check_ending_mb_anim

org $cbde80
    jml sm_setup_credits


org $f7fd00
base $b7fd00
sm_check_ending_door:        ; Check if ALTTP has been beaten, and only then activate the escape.
    pha
    lda #$0001
    sta.l !SRAM_SM_COMPLETED      ; Set supermetroid as completed
    lda.l !SRAM_ALTTP_COMPLETED
    bne .alttp_done
    pla
    jsl $808212         ; Clear event flag if set
    jml $8fc932         ; Jump to "RTS"
.alttp_done
    pla
    jsl $8081fa         ; Call the code we replaced
    jml $8fc926         ; Jump to "LDA #$0012"

sm_check_ending_mb:
    lda #$0001
    sta.l !SRAM_SM_COMPLETED      ; Set supermetroid as completed
    lda.l !SRAM_ALTTP_COMPLETED
    bne .alttp_done
    lda #$b2f9
    sta $0fa8
    lda #$0020
    sta $0fb2
    rtl

.alttp_done    
    lda #$0000
    sta $7e7808
    rtl

sm_check_ending_mb_event:
    jsl $90F084
    lda.l !SRAM_ALTTP_COMPLETED
    bne .alttp_done
    jml $A9B31a

.alttp_done
    jml $A9B309

sm_check_ending_mb_timer:
    lda.l !SRAM_ALTTP_COMPLETED
    bne .alttp_done
    clc
    jml $809E2E

.alttp_done
    jsl $809E93         ; Call the code we replaced
    jml $809E20

sm_check_ending_mb_anim:
    lda.l !SRAM_ALTTP_COMPLETED
    bne .alttp_done
    lda #$b3b5
    sta $0fa8
    jml $A9B345

.alttp_done
    lda $1840
    bne +
    jml $a9b341
+
    jml $A9B345

sm_setup_credits:
    %ai16()
    sei
    lda #$0000
    sta $4200

    ; Reset SPC and put it into upload mode
    jsl zelda_spc_reset

    ; Call credits
    jml credits_init
