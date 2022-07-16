; RTA Timer (timer 1 is frames, and timer 2 is number of times frames rolled over)
!timer1 = $05b8
!timer2 = $05ba

; Temp variables (define here to make sure they're not reused, make sure they're 2 bytes apart)
; These variables are cleared to 0x00 on hard and soft reset
!door_timer_tmp = $7fff00
!door_adjust_tmp = $7fff02
!add_time_tmp = $7fff04
!region_timer_tmp = $7fff06
!region_tmp = $7fff08
!transition_tmp = $7fff10
!ALTTP_NMI_COUNTER = $7EF43E

; -------------------------------
; HIJACKS
; -------------------------------

; Samus hit a door block (Gamestate change to $09 just before hitting $0a)
org $c2e176
    jml door_entered

; Samus gains control back after door (Gamestate change back to $08 after door transition)
org $c2e764
    jml door_exited

; Door starts adjusting
org $c2e309
    jml door_adjust_start

; Door stops adjusting
org $c2e34c
    jml door_adjust_stop

; Firing charged beam
org $d0b9a1
    jml charged_beam

; Firing SBAs
org $d0ccd2
    jml fire_sba

; Missiles/supers fired
org $d0beb7
    jml missiles_fired

; PBs laid
org $d0c02d
    jml pbs_laid

; Bombs laid
org $d0c107
    jml bombs_laid

org $e2ab13
    jsl game_end

org $c58089
    jsl item_collected


;Patch NMI to skip resetting 05ba and instead use that as an extra time counter
org $c095e5
base $8095e5
sm_patch_nmi:
    ldx #$00
    stx $05b4
    ldx $05b5
    inx
    stx $05b5
    inc $05b6
.inc:
    ;rep #$30
    ;inc $05b8
    jsl sm_mw_nmi_read_messages
    bne +
    inc $05ba
+
    bra .end

org $c09602
base $809602
    bra .inc
.end:
    ply
    plx
    pla
    pld
    plb
    rti

; -------------------------------
; CODE (using bank A1 free space)
; -------------------------------
org $e1ec00
base $a1ec00
get_total_frame_time:
    pha : phx : php
    %ai16()
    lda !SRAM_CURRENT_GAME
    beq .alttp

    lda !timer1
    clc
    adc $a0643e
    sta !SRAM_TIMER1
    lda !timer2
    adc $a06440
    sta !SRAM_TIMER2
    jmp .end    

.alttp
    lda !ALTTP_NMI_COUNTER
    clc
    adc !SRAM_SM_STATS
    sta !SRAM_TIMER1
    lda !ALTTP_NMI_COUNTER+2
    adc !SRAM_SM_STATS+2
    sta !SRAM_TIMER2

.end
    plp : plx : pla
    rtl

; stats:
;     ; STAT ID, ADDRESS,    TYPE (1 = Number, 2 = Time, 3 = Full time), UNUSED
;     dw $00,       0,  3, 0          ; Full RTA Time
;     dw $02,       0,  1, 0          ; Door transitions
;     dw $03,       0,  3, 0          ; Time in doors
;     dw $05,       0,  2, 0          ; Time adjusting doors
;     dw $07,       0,  3, 0          ; Crateria
;     dw $09,       0,  3, 0          ; Brinstar
;     dw $0b,       0,  3, 0          ; Norfair
;     dw $0d,       0,  3, 0          ; Wrecked Ship
;     dw $0f,       0,  3, 0          ; Maridia
;     dw $11,       0,  3, 0          ; Tourian
;     dw $14,       0,  1, 0          ; Charged Shots
;     dw $15,       0,  1, 0          ; Special Beam Attacks
;     dw $16,       0,  1, 0          ; Missiles
;     dw $17,       0,  1, 0          ; Super Missiles
;     dw $18,       0,  1, 0          ; Power Bombs
;     dw $1a,       0,  1, 0          ; Bombs
;     dw $1b,       0,  1, 0          ; Transitions to SM
;     dw $1c,       0,  1, 0          ; Transitions to ALTTP
;     dw $1d,       0,  1, 0          ; Collected items
;     dw 0,         0,  0, 0          ; end of table

; Helper function to add a time delta, X = stat to add to, A = value to check against
; This uses 4-bytes for each time delta
add_time:
    sta !add_time_tmp
    lda !timer1
    sec
    sbc !add_time_tmp
    sta !add_time_tmp
    txa
    jsl load_stat
    clc
    adc !add_time_tmp
    bcc +
    jsl store_stat    ; If carry set, increase the high bits
    inx
    txa
    jsl inc_stat
+
    jsl store_stat
    rts


; Samus hit a door block (Gamestate change to $09 just before hitting $0a)
door_entered:
    lda #$0002  ; Number of door transitions
    jsl inc_stat

    lda !timer1
    sta !door_timer_tmp  ; Save RTA time to temp variable

    ; Run hijacked code and return
    plp
    inc $0998
    jml $82e1b7

; Samus gains control back after door (Gamestate change back to $08 after door transition)
door_exited:
    ; Check for transition from ALTTP
    lda !transition_tmp
    bne +

    ; Increment saved value with time spent in door transition
    lda !door_timer_tmp
    ldx #$0003
    jsr add_time

    ; Store time spent in last room/area unless region_tmp is 0
    lda !region_tmp
    beq +
    tax
    lda !region_timer_tmp
    jsr add_time

    
   ; Store the current frame and the current region to temp variables
+
    lda #$0000
    sta !transition_tmp

    lda !timer1
    sta !region_timer_tmp
    lda $7e079f
    asl
    clc
    adc #$0007    
    sta !region_tmp    ; Store (region*2) + 7 to region_tmp (This uses stat id 7-18 for region timers)

    ; Run hijacked code and return
    lda #$0008
    sta $0998
    jml $82e76a

; Door adjust start
door_adjust_start:
    lda !timer1
    sta !door_adjust_tmp ; Save RTA time to temp variable

    ; Run hijacked code and return
    lda #$e310
    sta $099c
    jml $82e30f

; Door adjust stop
door_adjust_stop:
    lda !door_adjust_tmp
    inc ; Add extra frame to time delta so that perfect doors counts as 0
    ldx #$0005
    jsr add_time

    ; Run hijacked code and return
    lda #$e353
    sta $099c
    jml $82e352

; Charged Beam Fire
charged_beam:
    lda #$0014
    jsl inc_stat

    ; Run hijacked code and return
    LDX #$0000
    LDA $0c2c, x
    JML $90b9a7

; Firing SBAs
fire_sba:
    lda.l $90cc21, x
    beq .nosba      ; If the table lookup for the PB amount to remove is just 0, then exit right away

    lda #$0015      ; SBA happening, increment stat
    jsl inc_stat

    dec $09ce       ; Cheat and don't use the table for the amount of PB's to remove
    bpl +           ; since it'll always cost one
    stz $09ce       ; Handle underflow if it somehow can happen
+
    jml $90cce1     ; Jump to SBA triggering code

.nosba
    jml $90ccef     ; Jump to RTS (don't trigger SBA)

; MissilesSupers used
missiles_fired:
    lda $09d2
    cmp #$0002
    beq .super
    dec $09c6
    lda #$0016
    jsl inc_stat
    bra .end
.super:
    dec $09ca
    lda #$0017
    jsl inc_stat
.end:
    jml $90bec7

; PBs laid
pbs_laid:
    dec 
    sta $09ce
    lda #$0018
    jsl inc_stat
    jml $90c031

; Bombs laid
bombs_laid:
    lda $09d2       ; Check HUD selection index for PB selected
    cmp #$0003          
    beq .powerbomb
    lda #$001a
    bra .end
.powerbomb
    lda #$0018
.end
    jsl inc_stat

    ;run hijacked code and return
    lda $0cd2
    inc
    jml $90c10b

item_collected:
    pha
    lda $1c1f
    cmp #$0014
    beq .noitem
    cmp #$0015
    beq .noitem
    cmp #$0016
    beq .noitem
    cmp #$0017
    beq .noitem
    cmp #$0018
    beq .noitem
    cmp #$001c
    beq .noitem

    lda #$001d
    jsl inc_stat

.noitem
    pla
    rtl


; Increment Statistic (in A)
inc_stat:
    phx
    asl
    tax
    lda $7ffc00, x
    inc
    sta $7ffc00, x
    plx
    rtl

; Decrement Statistic (in A)
dec_stat:
    phx
    asl
    tax
    lda $7ffc00, x
    dec
    sta $7ffc00, x
    plx
    rtl


; Store Statistic (value in A, stat in X)
store_stat:
    phx
    pha
    txa
    asl
    tax
    pla
    sta $7ffc00, x
    plx
    rtl

; Load Statistic (stat in A, returns value in A)
load_stat:
    phx
    asl
    tax
    lda $7ffc00, x
    plx
    rtl 

load_stats:
    phx
    pha
    ldx #$0000
    lda $7e0952
    bne +
-
    lda !SRAM_SM_STATS, x
    sta $7ffc00, x
    inx
    inx
    cpx #$0040
    bne -
    jmp .end
+   
    cmp #$0001
    bne +
    lda !SRAM_SM_STATS+$40, x
    sta $7ffc00, x
    inx
    inx
    cpx #$0040
    bne -
    jmp .end
+   
    lda !SRAM_SM_STATS+$80, x
    sta $7ffc00, x
    inx
    inx
    cpx #$0040
    bne -
    jmp .end

.end:
    pla
    plx
    rtl

save_stats:
    phx
    pha
    ldx #$0000
    lda $7e0952
    bne +
-
    lda $7ffc00, x
    sta !SRAM_SM_STATS, x
    inx
    inx
    cpx #$0040
    bne -
    jmp .end
+   
    cmp #$0001
    bne +
    lda $7ffc00, x
    sta !SRAM_SM_STATS+$40, x
    inx
    inx
    cpx #$0040
    bne -
    jmp .end
+   
    lda $7ffc00, x
    sta !SRAM_SM_STATS+$80, x
    inx
    inx
    cpx #$0040
    bne -
    jmp .end

.end:
    pla
    plx
    rtl

stats_load_sram:
    pha : phx : phy : php
    jsl load_stats      
    lda $7ffc00
    sta !timer1
    lda $7ffc02
    sta !timer2
    plp : ply : plx : pla
    rtl

stats_save_sram:
    pha : phx : phy : php
    lda !timer1
    sta $7ffc00
    lda !timer2
    sta $7ffc02
    jsl save_stats
    plp : ply : plx : pla
    rtl

stats_clear_values:
    pha : phx : phy : php
    rep #$30

    ldx #$0000
    lda #$0000
-
    jsl store_stat
    inx
    cpx #$0180
    bne -

    ; Clear RTA Timer
    lda #$0000
    sta !timer1
    sta !timer2

.ret:
    plp : ply : plx : pla
    rtl

game_end:
    lda !timer1
    sta $7ffc00
    lda !timer2
    sta $7ffc02

    ; Subtract frames from pressing down at ship to this code running
    lda $7ffc00
    sec
    sbc #$013d
    sta $7ffc00
    lda #$0000  ; if carry clear this will subtract one from the high byte of timer
    sbc $7ffc02

    jsl save_stats
    lda #$000a
    jsl $90f084
    rtl
warnpc $e1ffff