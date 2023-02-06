org $ff0000
namespace "credits"

!BLUE = "table data/tables/blue.tbl,rtl"
!WHITE = "table data/tables/white.tbl,rtl"
!YELLOW = "table data/tables/yellow.tbl,rtl"
!PURPLE = "table data/tables/purple.tbl,rtl"
!ORANGE = "table data/tables/orange.tbl,rtl"
!GREEN = "table data/tables/green.tbl,rtl"
!CYAN = "table data/tables/cyan.tbl,rtl"
!PINK = "table data/tables/pink.tbl,rtl"
!BIG = "table data/tables/big.tbl,rtl"
!OFF = "cleartable"

!CREDITS_SPEED = $40
!CREDITS_SPEED_COUNTER = $42
!CREDITS_UPDATE_COUNTER = $44
!CREDITS_UPDATE_FLAG = $46
!CREDITS_MODE = $48
!CREDITS_Y = $20
!CREDITS_ADDR = $24
!CREDITS_VRAM_ADDR = $28
!CREDITS_NMI_DONE = $0e
!CREDITS_BG = $30
!CREDITS_BG_MODE = $32
!CREDITS_BG_COUNTER = $34
!CREDITS_BG_UPDATE = $36
!CREDITS_BG_PAL = $7ef000
!CREDITS_BG_DELAY = $38
!CREDITS_STREAM_SKIP = $b4
!CREDITS_STREAM_WAIT = $b6

init:
    ; Set up a consistent SNES state
    sei
    %ai16()
    ldx #$1fff
    txs

    ; Set data bank register to bank 00
    pea $0000
    plb : plb   

    ; Direct page to 2100 for HW regs
    lda #$2100
    tcd

    %a8()

    ; Disable NMI and H-DMA
    lda #$00
    sta $4200
    sta $420c

    ; Turn on forced blanking
    lda #$8f
    sta $00

    lda #$01
    sta $420d   ; enable FastROM

    ; Set "game" to credits so we can have our own NMI routine
    lda #$11
    sta !SRAM_CURRENT_GAME

    ; Clear all PPU registers
	ldx #$0001
-           	; loop over ppu registers $2101-$2133
	stz $00,x   ; write 0 to direct page ($2100) + x
	stz $00,x   ; some registers need to be written twice
	inx
	cpx #$0034    ; while x < $33 ($2133)
	bne -

    ; Restore zero page
    %ai16()
    lda #$0000
    tcd

    ; If previously playing an MSU track, play combo credits MSU track
    ; Otherwise, play the built in music
    pha
    sep #$30

    jsl playmusic

    lda $2002 : cmp.b #'S' : BNE +
        LDA #99 : STA $2004 : STZ $2005 ; Play track 99
        LDA #1 : STA $2007 ; Sets the track to not repeat
        LDA #$FF : STA $2006 ; Set to max volume
    + 

    ; Load credits fonts and palettes into VRAM/CGRAM
    %ai16()
    jsr load_graphics
    jsr load_tilemap

    ; Write stats to tilemap
    jsr write_stats

    ; Blank out background palette
    lda #$0000
    ldx #$0000
-
    sta !CREDITS_BG_PAL,x
    inx : inx
    cpx #$0020
    bne -

    jsr write_bg_pal

    %a8()

    ; Set BG mode 1
    lda #$01
    sta $2105

    ; Set BG1 screen base to start of VRAM
    lda #$02
    sta $2107

    ; Set BG2 screen base to 0x1000
    lda #$08
    sta $2108

    ; Set BG1&2 tilemap base to 0xA000
    lda #$35
    sta $210b

    lda #$03
    sta $212c
    lda #$02
    sta $212d


    ; Disable forced blanking
    lda #$0f
    sta $2100

    rep #$30
    lda #$0000
    sta !CREDITS_Y
    sta !CREDITS_VRAM_ADDR
    sta !CREDITS_BG
    sta !CREDITS_BG_MODE
    sta !CREDITS_BG_DELAY
    sta !CREDITS_MODE
    lda #$001F
    sta !CREDITS_BG_COUNTER


    lda #$0003
    sta !CREDITS_SPEED
    sta !CREDITS_SPEED_COUNTER
    
    lda #$0000
    sta !CREDITS_UPDATE_FLAG
    
    lda #$0040
    sta !CREDITS_UPDATE_COUNTER

    ; Setup pointer to next credits row to load
    lda #$0000
    clc
    adc #$0800
    sta !CREDITS_ADDR
    lda #$007f
    sta !CREDITS_ADDR+2
    
    ; %a16()
    ; lda #$0001
    ; sta !CREDITS_STREAM_SKIP
    ; lda #$0030
    ; sta !CREDITS_STREAM_WAIT
    ; jsl stream_upload
    
    %a8()
    ; Turn NMI back on
    lda #$80
    sta $4200



.loop
    ; Increase scroll register every "CREDITS_SPEED" amount of frames
    rep #$30

    lda !CREDITS_MODE
    bne .scrollend

    lda !CREDITS_SPEED_COUNTER
    dec a
    bne +    
    dec !CREDITS_UPDATE_COUNTER
    inc !CREDITS_Y
    lda !CREDITS_SPEED
+
    sta !CREDITS_SPEED_COUNTER

    ; Check if we need to update tilemap data
    lda !CREDITS_UPDATE_COUNTER
    bne +
    lda #$0010
    sta !CREDITS_UPDATE_COUNTER

    ; Set DMA parameters for NMI DMA copy
    lda !CREDITS_ADDR : sta $d0
    lda !CREDITS_ADDR+2 : sta $d2
    lda !CREDITS_VRAM_ADDR : sta $d4
    lda #$0080 : sta $d6

    ; Tell the NMI routine to do a DMA transfer
    lda #$0001
    sta !CREDITS_UPDATE_FLAG

    lda !CREDITS_ADDR
    clc : adc #$0080
    sta !CREDITS_ADDR

    lda !CREDITS_VRAM_ADDR
    clc : adc #$0040
    cmp #$0800
    bne ++
    lda #$0000
++
    sta !CREDITS_VRAM_ADDR

+

.scrollend

    lda !CREDITS_BG_DELAY
    dec a
    bmi +
    sta !CREDITS_BG_DELAY
    jmp .endbg

+
    ; Credits BG
    LDA !CREDITS_BG_MODE
    cmp #$0000
    beq .fadein
    cmp #$0001
    beq .show
    cmp #$0002
    beq .fadeout
    cmp #$0003
    beq .switch
    jmp .endbg

.fadein
    lda !CREDITS_BG_COUNTER
    sta $04
    lda !CREDITS_BG
    bne +
    lda #samus_pal
    bra ++
+
    lda #triforce_pal
++
    sta $02
    jsr write_faded_pal
    lda !CREDITS_BG_COUNTER
    dec a
    bpl +
    lda #$0001
    sta !CREDITS_BG_MODE
    lda #$0200
+
    sta !CREDITS_BG_COUNTER
    lda #$0001
    sta !CREDITS_BG_UPDATE

    lda #$0004
    sta !CREDITS_BG_DELAY

    jmp .endbg

.show
    lda !CREDITS_BG_COUNTER
    dec a
    bne +
    lda #$0002
    sta !CREDITS_BG_MODE
    lda #$0000
+
    sta !CREDITS_BG_COUNTER
    jmp .endbg

.fadeout
    lda !CREDITS_BG_COUNTER
    sta $04
    lda !CREDITS_BG
    bne +
    lda #samus_pal
    bra ++
+
    lda #triforce_pal
++
    sta $02
    jsr write_faded_pal
    lda !CREDITS_BG_COUNTER
    inc a
    cmp #$0020
    bne +
    lda #$0003
    sta !CREDITS_BG_MODE
    lda #$0000
+
    sta !CREDITS_BG_COUNTER
    lda #$0001
    sta !CREDITS_BG_UPDATE

    lda #$0004
    sta !CREDITS_BG_DELAY

    jmp .endbg

.switch
    lda !CREDITS_BG
    bne +
    lda #$0001
    sta !CREDITS_BG
    bra ++
+
    lda #$0000
    sta !CREDITS_BG
++
    lda #$0002
    sta !CREDITS_BG_UPDATE
    jmp .endbg

.endbg

    ldx #$0000
    phx
-
    lda.l events,x
    beq .endevents
    cmp !CREDITS_ADDR
    bne .next_event
    lda.l events+2,x        ; Load command
    cmp #$0001
    beq .adjust_speed
    cmp #$000f
    beq .stop
    cmp #$0010
    beq .check_config
    bra .next_event

.adjust_speed
    lda.l events+4,x
    sta !CREDITS_SPEED
    bra .next_event

.stop
    lda #$000f
    sta !CREDITS_MODE
    bra .next_event

.check_config
    ; Load config flag offset and check it
    lda.l events+4, x : phx : tax
    lda.l config_flags&$ff0000, x : plx
    cmp #$0000
    bne .next_event

    ; If it's zero, skip to the next part as defined in the event
    lda.l events+6, x 
    sta !CREDITS_ADDR
    bra .next_event

.next_event
    plx
    inx
    phx
    txa : asl #3 : tax
    bra -    

.endevents
    plx

    ; Wait for NMI
    lda #$0000
    sta !CREDITS_NMI_DONE
-
    lda !CREDITS_NMI_DONE
    beq -

    jmp .loop

; Credits NMI routine
nmi:
    pha : phx : phy : php

    %a8()
    lda !CREDITS_Y
    sta $210e
    lda !CREDITS_Y+1
    sta $210e

    %ai16()
    lda !CREDITS_UPDATE_FLAG 
    beq +
    jsr write_vram_zp
    lda #$0000
    sta !CREDITS_UPDATE_FLAG

+
    lda !CREDITS_BG_UPDATE
    cmp #$0001
    beq .writepal
    cmp #$0002
    beq .switchbg
    bra .bgend

.writepal
    jsr write_bg_pal
    bra .bgend
.switchbg
    lda !CREDITS_BG
    bne +
    
    %a8()
    lda #$08
    sta $2108

    lda #$35
    sta $210b
    %a16()
    bra ++

+
    %a8()
    lda #$10
    sta $2108

    lda #$75
    sta $210b
    %a16()

++
    lda #$0000
    sta !CREDITS_BG_MODE
    lda #$001f
    sta !CREDITS_BG_COUNTER

.bgend

    rep #$30
    lda #$0001
    sta !CREDITS_NMI_DONE

;     lda !CREDITS_STREAM_WAIT
;     beq +
;     dec !CREDITS_STREAM_WAIT
;     bra .nmiend
; +
;     jsl stream_samples

.nmiend
    plp : ply : plx : pla
    rti    


load_graphics:
    pha : phx : phy : php

    %ai16()
    jsr clear_vram
    jsr write_vram : dl font_data : dw #$5000 : dw #$2000   ; Copy font data to VRAM
    jsr write_vram : dl tilemap_data : dw #$0400 : dw #$0800 ; Copy first screen of credits tiles to VRAM
    jsr write_vram : dl samus_gfx : dw #$6000 : dw #$2000   ; Copy samus_gfx to VRAM
    jsr write_vram : dl samus_map : dw #$0800 : dw #$0800   ; Copy samus_map to VRAM
    jsr write_vram : dl triforce_gfx : dw #$7000 : dw #$1000   ; Copy triforce_gfx to VRAM
    jsr write_vram : dl triforce_map : dw #$1000 : dw #$0800   ; Copy trifoce_map to VRAM

    %a8()
    ldx #$0000
    lda #$00
    sta $2121
-
    lda.l cgram_data,x
    sta $2122
    lda.l cgram_data+1,x
    sta $2122
    inx : inx
    cpx #$0200
    bne -

    plp : ply : plx : pla
    rts

write_bg_pal:
    pha : phx : phy : php
    
    %a8()
    ldx #$0000
    lda #$30
    sta $2121
-
    lda.l !CREDITS_BG_PAL,x
    sta $2122
    lda.l !CREDITS_BG_PAL+1,x
    sta $2122
    inx : inx
    cpx #$0020
    bne -

    plp : ply : plx : pla
    rts

load_tilemap:
    pha : phx : phy : php    
    %ai16()
    jsr write_wram : dl tilemap_data : dw #$0000 : dw #$8000 ; Copy tilemap data to 7f0000    
    plp : ply : plx : pla
    rts

clear_vram:
    rep #$30
    lda #$0080
    sta $2115
    lda #$0000
    sta $2116
    
    lda #$007f
    ldx #$0000
-
    sta $2118
    inx : inx
    cpx #$1000
    bne -
    rts


write_vram: 
    ; Get a pointer to the caller return address
    rep #$30
    lda #$00ff
    sta $05
    lda $01,s
    sta $03
    clc
    adc #$0007  ; Realign return address
    sta $01,s

    ; Set up DMA transfer
    ldy #$0001

    lda [$03],y ; Source address
    sta $4302
    iny
    iny
    lda [$03],y ; Source bank
    sta $4304
    iny
    lda [$03],y ; Target VRAM address
    sta $2116
    iny
    iny
    lda [$03],y ; Transfer size
    sta $4305

    sep #$20
    lda #$80
    sta $2115
    lda #$18
    sta $4301
    lda #$01
    sta $4300
    sta $420b

    rep #$30
    rts

write_vram_zp: 
    rep #$30
    lda $d0
    sta $4302
    lda $d2
    sta $4304
    lda $d4
    sta $2116
    lda $d6
    sta $4305

    sep #$20
    lda #$80
    sta $2115
    lda #$18
    sta $4301
    lda #$01
    sta $4300
    sta $420b

    rep #$30
    rts

write_wram: 
    ; Get a pointer to the caller return address
    rep #$30
    lda #$00ff
    sta $05
    lda $01,s
    sta $03
    clc
    adc #$0007  ; Realign return address
    sta $01,s

    ; Set up DMA transfer
    ldy #$0001

    lda [$03],y ; Source address
    sta $4302
    iny
    iny
    lda [$03],y ; Source bank
    sta $4304
    iny
    lda [$03],y ; Target WRAM address
    sta $2181
    lda #$0001
    sta $2183
    iny
    iny
    lda [$03],y ; Transfer size
    sta $4305

    sep #$20
    lda #$80
    sta $4301
    lda #$00
    sta $4300
    lda #$01
    sta $420b

    rep #$30
    rts

; Palette address in $02, fade in $04
write_faded_pal:
    pha : phx : phy : php
    %ai16()    
    ldx $02
    ldy #$0000
    
-
    ; red component
    lda $ff0000,x
    and #$001f
    sec
    sbc $04
    bpl +
    lda #$0000
+
    sta $06

    ; green component
    lda $ff0000,x
    lsr #5
    and #$001f
    sec
    sbc $04
    bpl +
    lda #$0000
+
    asl #5
    ora $06
    sta $06

    ; blue component
    lda $ff0000,x
    lsr #10
    and #$001f
    sec
    sbc $04
    bpl +
    lda #$0000
+
    asl #10
    ora $06
    sta $06    

    lda $06
    phx
    tyx
    sta !CREDITS_BG_PAL,x
    plx

    iny : iny
    inx : inx

    cpy #$0020
    bne -

    plp : ply : plx : pla
    rts

draw_full_time:
    phx
    phb
    pea $7f7f : plb : plb
    tax
    lda $0000,x
    sta $16
    lda $0002,x
    sta $14
    lda #$003c
    sta $12
    lda #$ffff
    sta $1a
    jsr div32 ; frames in $14, rest in $16
    iny : iny : iny : iny : iny : iny ; Increment Y three positions forward to write the last value    
    lda $14
    jsr draw_two
    tya
    sec
    sbc #$0010
    tay     ; Skip back 8 characters to draw the top three things
    lda $16
    jsr draw_time
    plb
    plx
    rts  

; Draw time as xx:yy:zz
draw_time:
    phx
    phb
    dey : dey : dey : dey : dey : dey ; Decrement Y by 3 characters so the time count fits
    pea $7f7f : plb : plb
    sta $004204
    sep #$20
    lda #$ff
    sta $1a
    lda #$3c
    sta $004206
    pha : pla : pha : pla : rep #$20
    lda $004216 ; Seconds or Frames
    sta $12
    lda $004214 ; First two groups (hours/minutes or minutes/seconds)
    sta $004204
    sep #$20
    lda #$3c
    sta $004206
    pha : pla : pha : pla : rep #$20
    lda $004216
    sta $14
    lda $004214 ; First group (hours or minutes)
    jsr draw_two
    iny : iny ; Skip past separator
    lda $14 ; Second group (minutes or seconds)
    jsr draw_two
    iny : iny
    lda $12 ; Last group (seconds or frames)
    jsr draw_two
    plb
    plx
    rts        

; Draw 5-digit value to credits tilemap
; A = number to draw, Y = row address
draw_value:
    phx    
    phb
    pea $7f7f : plb : plb
    sta $004204
    lda #$0000
    sta $1a     ; Leading zeroes flag
    sep #$20
    lda #$64
    sta $004206
    pha : pla : pha : pla : rep #$20
    lda $004216 ; Last two digits
    sta $12
    lda $004214 ; Top three digits
    jsr draw_three
    lda $12
    jsr draw_two
    plb
    plx
    rts

draw_three:
    sta $004204
    sep #$20
    lda #$64
    sta $004206
    pha : pla : pha : pla : rep #$20
    lda $004214 ; Hundreds
    asl
    tax
    cmp $1a
    beq +
    lda.l numbers_top,x
    sta $0034,y
    lda.l numbers_bot,x
    sta $0074,y
    dec $1a
+
    iny : iny ; Next number
    lda $004216

draw_two:
    sta $004204
    sep #$20
    lda #$0a
    sta $004206
    pha : pla : pha : pla : rep #$20
    lda $004214
    asl
    tax
    cmp $1a
    beq +
    lda.l numbers_top,x
    sta $0034,y
    lda.l numbers_bot,x
    sta $0074,y
    dec $1a
+
    lda $004216
    asl
    tax
    cmp $1a
    beq +
    lda.l numbers_top,x
    sta $0036,y
    lda.l numbers_bot,x
    sta $0076,y
    dec $1a
+
    iny : iny : iny : iny
    rts

div32: 
    phy
    phx             
    php
    rep #$30
    sep #$10
    sec
    lda $14
    sbc $12
    bcs uoflo
    ldx #$11
    rep #$10

ushftl:
    rol $16
    dex
    beq umend
    rol $14
    lda #$0000
    rol
    sta $18
    sec
    lda $14
    sbc $12
    tay
    lda $18
    sbc #$0000
    bcc ushftl
    sty $14
    bra ushftl
uoflo:
    lda #$ffff
    sta $16
    sta $14
umend:
    plp
    plx
    ply
    rts

write_stats:
    pha : phx : phy : php : phb
    %ai16()       
    
    pea $ffff
    plb : plb

    jsl load_stats      ; Load SM stats from SRAM

    lda $a0643e         ; Create sum of total time
    clc
    adc $7ffc00
    sta $7ffc3c
    lda $a06440
    adc $7ffc02
    sta $7ffc3e

    lda $a06423         ; Sum collected items
    and #$00ff
    clc
    adc $7ffc3a
    sta $7ffc3a

    ldx #$0000
    ldy #$0000

.loop
    tya : asl #3 : tax

    lda stats,x
    bne +
    jmp .end
+
    lda stats+2,x
    cmp #$0000
    beq .sm_stat
    cmp #$0001
    beq .alttp_stat
    jmp .continue

.sm_stat
    lda stats+6,x       ; SM stat type
    cmp #$0001
    beq .sm_number
    cmp #$0002
    beq .sm_time
    cmp #$0003
    beq .sm_fulltime
    jmp .continue

.sm_number
    lda stats+4,x      ; SM stat number
    jsl load_stat
    pha

    lda stats,x
    tyx
    tay
    pla
    jsr draw_value
    txy
    jmp .continue

.sm_time
    ; Load statistic
    lda stats+4,x
    jsl load_stat
    pha

    ; Load row address
    lda stats,x
    tyx
    tay
    pla
    jsr draw_time
    txy
    jmp .continue   

.sm_fulltime   
    lda stats+4,x        ; Get stat id
    asl
    clc
    adc #$fc00           ; Get pointer to value instead of actual value
    pha

    ; Load row address
    lda stats,x
    tyx
    tay
    pla
    jsr draw_full_time
    txy
    jmp .continue

.alttp_stat
    lda stats+6,x       ; ALTTP stat type
    cmp #$0001
    beq .alttp_number
    cmp #$0003
    beq .alttp_fulltime
    cmp #$0000
    bne .alttp_shiftnumber
    jmp .continue

.alttp_number
    lda stats+4,x      ; ALTTP stat address
    phx
    tax
    lda $a06000,x      ; Load value from ALTTP SRAM
    and #$00ff
    plx
    phy
    pha
    lda stats,x        ; Load tilemap address
    tay
    pla
    jsr draw_value
    ply
    jmp .continue

.alttp_fulltime
    lda stats+4,x
    phx
    tax
    lda $a06000,x
    sta $7ffcfc
    lda $a06002,x
    sta $7ffcfe
    plx
    lda stats,x
    phy
    tay
    lda #$fcfc
    jsr draw_full_time
    ply
    jmp .continue

.alttp_shiftnumber
    lda stats+6,x      ; Load bitshift mask
    sta $04
    lda stats+4,x      ; ALTTP stat address
    phx
    tax
    lda $a06000,x      ; Load value from ALTTP SRAM
    sta $02
    jsr alttp_shift_stat
    plx
    phy
    pha
    lda stats,x        ; Load tilemap address
    tay
    pla
    jsr draw_value
    ply
    jmp .continue   

.continue
    iny
    jmp .loop

.end
    plb : plp : ply : plx : pla
    rts

alttp_shift_stat:
    phx
    lda $04
    and #$ff00
    xba
    tax             ; number of shifts in X
    lda $02
    and #$00ff

-    
    cpx #$0000
    beq +
    lsr
    dex
    jmp -

+
    sta $02
    lda $04
    and #$00ff
    sta $04
    lda $02
    and $04
    plx
    rts

events:
    ;  tilemap pointer location                 cmd    val1   val2
    dw tilemap_data_randomizer_staff+$0800,   $0001,  $0004,  $0000
    dw tilemap_data_stop+$0800,               $000f,  $0000,  $0000
    dw tilemap_data_alttp_sprite_credits,     $0010,  config_alttp_sprite,    tilemap_data_sm_sprite_credits
    dw tilemap_data_sm_sprite_credits,        $0010,  config_sm_sprite,       tilemap_data_end_sprite_credits
    dw $0000

stats:
    ; tilemap pointer location                  type    addr    value
    dw tilemap_data_door_transitions,           $0000,  $0002,  $0001
    dw tilemap_data_time_in_doors,              $0000,  $0003,  $0003
    dw tilemap_data_time_aligning_doors,        $0000,  $0005,  $0002
    dw tilemap_data_time_in_crateria,           $0000,  $0007,  $0003
    dw tilemap_data_time_in_brinstar,           $0000,  $0009,  $0003
    dw tilemap_data_time_in_norfair,            $0000,  $000b,  $0003
    dw tilemap_data_time_in_wreckedship,        $0000,  $000d,  $0003
    dw tilemap_data_time_in_maridia,            $0000,  $000f,  $0003
    dw tilemap_data_time_in_tourian,            $0000,  $0011,  $0003
    dw tilemap_data_charged_shots,              $0000,  $0014,  $0001
    dw tilemap_data_sbas,                       $0000,  $0015,  $0001
    dw tilemap_data_missiles,                   $0000,  $0016,  $0001
    dw tilemap_data_supermissiles,              $0000,  $0017,  $0001
    dw tilemap_data_powerbombs,                 $0000,  $0018,  $0001
    dw tilemap_data_bombs,                      $0000,  $001a,  $0001
    dw tilemap_data_alttp_first_sword,          $0001,  $0458,  $0003
    dw tilemap_data_alttp_pegasus_boots,        $0001,  $045c,  $0003
    dw tilemap_data_alttp_flute,                $0001,  $0460,  $0003
    dw tilemap_data_alttp_mirror,               $0001,  $0464,  $0003
    dw tilemap_data_alttp_swordless,            $0001,  $0452,  $040f
    dw tilemap_data_alttp_fighter,              $0001,  $0425,  $040f
    dw tilemap_data_alttp_master,               $0001,  $0425,  $000f
    dw tilemap_data_alttp_tempered,             $0001,  $0426,  $040f
    dw tilemap_data_alttp_gold,                 $0001,  $0426,  $000f
    dw tilemap_data_alttp_gtbigkey,             $0001,  $042a,  $001f
    dw tilemap_data_alttp_bonks,                $0001,  $0420,  $0001
    dw tilemap_data_alttp_savequits,            $0001,  $042d,  $0001
    dw tilemap_data_alttp_deaths,               $0001,  $0449,  $0001
    dw tilemap_data_alttp_faerierevivals,       $0001,  $0453,  $0001
    dw tilemap_data_alttp_menutime,             $0001,  $0444,  $0003
    dw tilemap_data_sm_transitions,             $0000,  $001b,  $0001
    dw tilemap_data_alttp_transitions,          $0000,  $001c,  $0001
    dw tilemap_data_alttp_total_time,           $0001,  $043e,  $0003
    dw tilemap_data_sm_total_time,              $0000,  $0000,  $0003
    dw tilemap_data_total_time,                 $0000,  $001e,  $0003
    dw tilemap_data_collection_rate,            $0000,  $001d,  $0001
    dw $0000


font_data:
    incbin "data/credits-gfx.bin"
cgram_data:
    incbin "data/credits-cgram.bin"
samus_gfx:
    incbin "data/credits-samus-gfx.bin"
samus_map:
    incbin "data/credits-samus-map.bin"

samus_pal:
    db $00, $00, $00, $14, $AA, $15, $00, $00, $80, $5B, $01, $25, $A2, $14, $8D, $56, $C7, $3D, $B8, $14, $40, $18, $40, $31, $42, $1C, $A0, $24, $6D, $14, $05, $14
triforce_pal:
    db $00, $00, $9C, $03, $C6, $00, $18, $63, $18, $63, $18, $63, $18, $63, $18, $63, $18, $63, $18, $63, $18, $63, $18, $63, $18, $63, $18, $63, $18, $63, $18, $63

numbers_top:
    dw $0060, $0061, $0062, $0063, $0064, $0065, $0066, $0067, $0068, $0069, $006a, $006b, $006c, $006d, $006e, $006f
numbers_bot:
    dw $0070, $0071, $0072, $0073, $0074, $0075, $0076, $0077, $0078, $0079, $007a, $007b, $007c, $007d, $007e, $007f 

warnpc $ff8000

org $fe0000
triforce_gfx:
    incbin "data/credits-triforce-gfx.bin"
triforce_map:
    incbin "data/credits-triforce-map.bin"
warnpc $fe8000

org $fd0000
tilemap_data:
    !BIG
    dw " SUPER METROID                  "
    dw " super metroid                  "
    dw "                                "
    dw "             A LINK TO THE PAST "
    dw "             a link to the past "
    dw "                                "
    dw "      CROSSOVER RANDOMIZER      "
    dw "      crossover randomizer      "
    dw "                                "
    dw "                                "
    dw "                                "
    dw "                                "
    !PURPLE
    dw "     ORIGINAL GAME CREDITS      "
    dw "                                "   
    !CYAN : dw "A LINK TO THE PAST" : !BLUE : dw " SUPER METROID"
    dw "                                "
    !CYAN : dw "HIROSHI YAMAUCHI" : !BLUE : dw "    MAKOTO KANOH"
    !CYAN : dw "SHIGERU MIYAMOTO" : !BLUE : dw "  YOSHI SAKAMOTO"
    !CYAN : dw "TAKASHI TEZUKA" : !BLUE : dw "      TOHRU OHSAWA"
    !CYAN : dw "KENSUKE TANABE" : !BLUE : dw "  MASAHIKO MASHIMO"
    !CYAN : dw "YOICHI YAMADA" : !BLUE : dw "  HIROFUMI MATSUOKA"
    !CYAN : dw "YASUHISA YAMAMURA" : !BLUE : dw "     KENJI IMAI"
    !CYAN : dw "SOICHIRO TOMITA" : !BLUE : dw "  HIROYUKI KIMURA"
    !CYAN : dw "TAKAYA IMAMURA" : !BLUE : dw "  TOMOYOSHI YAMANE"
    !CYAN : dw "MASANAO ARIMOTO" : !BLUE : dw "  HIROJI KIYOTAKE"
    !CYAN : dw "TSUYOSHI WATANABE" : !BLUE : dw "  TOMOMI YAMANE"
    !CYAN : dw "TOSHIHIKO NAKAGO" : !BLUE : dw "  KENJI YAMAMOTO"
    !CYAN : dw "YASUNARI SOEJIMA" : !BLUE : dw "   MINAKO HAMANO"
    !CYAN : dw "KASUAKI MORITA" : !BLUE : dw "    KENJI NAKAJIMA"
    !CYAN : dw "TATSUO NISHIYAMA" : !BLUE : dw "  YOSHIKAZU MORI"
    !CYAN : dw "YUICHI YAMAMOTO" : !BLUE : dw "     ISAMU KUBOTA"
    !CYAN : dw "TOSHIO IWAWAKI" : !BLUE : dw " MUTSURU MATSUMOTO"
    !CYAN : dw "YOSHIHIRO NOMOTO" : !BLUE : dw "   YASUHIKO FUJI"
    !CYAN : dw "EIJI NOTO" : !BLUE : dw "     MOTOMU CHIKARAISHI"
    !CYAN : dw "SHIGEHIRO KASAMATSU" : !BLUE : dw "  KOUICHI ABE"
    !CYAN : dw "SATORU TAKAHATA" : !BLUE : dw "   KATSUYA YAMANO"
    !CYAN : dw "KEIZO KATO" : !BLUE : dw "     TSUTOMU KANESHIGE"
    !CYAN : dw "KOJI KONDO" : !BLUE : dw "    MASAFUMI SAKASHITA"
    !CYAN : dw "YASUNARI NISHIDA" : !BLUE : dw "     YASUO INOUE"
    !CYAN : dw "YOSHIAKI KOIZUMI" : !BLUE : dw "     MARY COCOMA"
    !CYAN : dw "TAKAO SHIMIZU" : !BLUE : dw "      YUSUKE NAKANO"
    !CYAN : dw "YOICHI KOTABE" : !BLUE : dw "        SHINYA SANO"
    !CYAN : dw "HIDEKI FUJII" : !BLUE : dw "       NORIYUKI SATO"
    !CYAN : dw "DANIEL OWSEN" : !BLUE : dw "        DANIEL OWSEN"
    !CYAN : dw "YASUHIRO SAKAI" : !BLUE : dw "                  "
    !CYAN : dw "HIROYUKI YAMADA" : !BLUE : dw "     GUMPEI YOKOI"
.randomizer_staff
    dw "                                "
    dw "                                "
    dw "                                "
    dw "                                "
    !ORANGE
    dw "   CROSSOVER RANDOMIZER STAFF   "
    dw "                                "
    dw "                                "
    !BLUE
    dw "         MAIN DEVELOPER         "
    !BIG
    dw "                                "
    dw "              TOTAL             "
    dw "              total             "
    dw "                                "
    dw "                                "
    dw "                                "
    !PINK
    dw "       ADDITIONAL GRAPHICS      "
    !BIG
    dw "                                "
    dw "      ANDREW       NATALIE      "
    dw "      andrew       natalie      "
    dw "                                "
    dw "    QWERTYMODO     ARTHEAU      "
    dw "    qwertymodo     artheau      "
    dw "                                "
    dw "                                "
    dw "                                "
    !CYAN
    dw "     ADDITIONAL CONTRIBUTORS    "
    !BIG
    dw "                                "
    dw "   LENOPHIS        HALFAREBEL   "
    dw "   lenophis        halfarebel   "
    dw "                                "
    dw "     ANDY            SMOLBIG    "
    dw "     andy            smolbig    "
    dw "                                "
    dw "                                "
    dw "                                "
.alttp_sprite_credits
    !PINK
    dw "       ALTTP CUSTOM SPRITE      "
    !BIG
    dw "                                "
    dw "                                "
    dw "                                "
    dw "                                "
    dw "                                "
.sm_sprite_credits
    !ORANGE
    dw "  SUPER METROID CUSTOM SPRITE   "
    !BIG
    dw "                                "
    dw "                                "
    dw "                                "
    dw "                                "
    dw "                                "
.end_sprite_credits
    !PURPLE
    dw "       SPECIAL THANKS TO        "
    !BIG
    dw "                                "
    dw "  WILDANACONDA69   ALUCARD2004  "
    dw "  wildanaconda&)   alucard@}}$  "
    dw "                                "
    dw "      GARRISON      IVAN        "
    dw "      garrison      ivan        "
    dw "                                "
    dw "  KEKUMANSHOYU      LEXOMATICO  "
    dw "  kekumanshoyu      lexomatico  "
    dw "                                "
    dw "                                "
    dw "   AND ALL OTHER BETA TESTERS   "
    dw "   and all other beta testers   "
    dw "                                "
    dw "                                "
    dw "    METROIDCONSTRUCTION COM     "
    dw "    metroidconstruction.com     "
    dw "                                "
    dw "                                "
    dw "  SUPER METROID SRL COMMUNITY   "
    dw "  super metroid srl community   "
    dw "                                "
    dw "                                "
    dw "  ALTTP AND ALTTPR COMMUNITIES  "
    dw "  alttp and alttpr communities  "
    dw "                                "
    dw "                                "
    dw "                                "
    !ORANGE
    dw "  ALTTP RANDOMIZER DEVELOPERS   "
    !BIG
    dw "                                "
    dw " VEETORP KATDEVSGAMES DESSYREQT "
    dw " veetorp katdevsgames dessyreqt "
    dw "                                "
    dw "  SMALLHACKER     CHRISTOSOWEN  "
    dw "  smallhacker     christosowen  "
    dw "                                "
    dw "                                "
    dw "                                "
    dw "                                "
    !BLUE
    dw "    SM RANDOMIZER DEVELOPERS    "
    !BIG
    dw "                                "
    dw "  TOTAL    ANDREW    DESSYREQT  "
    dw "  total    andrew    dessyreqt  "
    dw "                                "
    dw "     LEODOX     PERSONITIS      "
    dw "     leodox     personitis      "
    dw "                                "
    dw "                                "
    dw "                                "
    dw "                                "
    !CYAN
    dw "       GAMEPLAY STATISTICS      "
    dw "                                "
    dw "                                "
    dw "                                "
    !PURPLE
    dw "          SUPER METROID         "
    dw "                                "
    dw "                                "
    !ORANGE
    dw "             DOORS              "
    dw "                                "
    !BIG
.door_transitions
    dw " DOOR TRANSITIONS               "
    dw " door transitions               "
    dw "                                "
.time_in_doors
    dw " TIME IN DOORS      00'00'00^00 "
    dw " time in doors                  "
    dw "                                "
.time_aligning_doors
    dw " TIME ALIGNING DOORS   00'00^00 "
    dw " time aligning doors            "
    dw "                                "
    dw "                                "
    !BLUE
    dw "         TIME SPENT IN          "
    dw "                                "
    !BIG
.time_in_crateria
    dw " CRATERIA           00'00'00^00 "
    dw " crateria                       "
    dw "                                "
.time_in_brinstar
    dw " BRINSTAR           00'00'00^00 "
    dw " brinstar                       "
    dw "                                "
.time_in_norfair
    dw " NORFAIR            00'00'00^00 "
    dw " norfair                        "
    dw "                                "
.time_in_wreckedship
    dw " WRECKED SHIP       00'00'00^00 "
    dw " wrecked ship                   "
    dw "                                "
.time_in_maridia
    dw " MARIDIA            00'00'00^00 "
    dw " maridia                        "
    dw "                                "
.time_in_tourian
    dw " TOURIAN            00'00'00^00 "
    dw " tourian                        "
    dw "                                "
    dw "                                "
    !PINK
    dw "      SHOTS AND AMMO FIRED      "
    dw "                                "
    !BIG
.charged_shots
    dw " CHARGED SHOTS                  "
    dw " charged shots                  "
    dw "                                "
.sbas
    dw " SPECIAL BEAM ATTACKS           "
    dw " special beam attacks           "
    dw "                                "
.missiles
    dw " MISSILES                       "
    dw " missiles                       "
    dw "                                "
.supermissiles
    dw " SUPER MISSILES                 "
    dw " super missiles                 "
    dw "                                "
.powerbombs
    dw " POWER BOMBS                    "
    dw " power bombs                    "
    dw "                                "
.bombs
    dw " BOMBS                          "
    dw " bombs                          "
    dw "                                "
    dw "                                "
    dw "                                "
    dw "                                "
    !YELLOW
    dw "       A LINK TO THE PAST       "
    dw "                                "
    dw "                                "
    !CYAN
    dw "           TIME FOUND           "
    dw "                                "    
    !BIG
.alttp_first_sword
    dw " FIRST SWORD        00'00'00^00 "
    dw " first sword                    "
    dw "                                "
.alttp_pegasus_boots
    dw " PEGASUS BOOTS      00'00'00^00 "
    dw " pegasus boots                  "
    dw "                                "
.alttp_flute
    dw " FLUTE              00'00'00^00 "
    dw " flute                          "
    dw "                                "
.alttp_mirror
    dw " MIRROR             00'00'00^00 "
    dw " mirror                         "
    dw "                                "
    dw "                                "
    !ORANGE
    dw "           BOSS KILLS           "
    dw "                                "    
    !BIG
.alttp_swordless
    dw " SWORDLESS                      "
    dw " swordless                      "
    dw "                                "
.alttp_fighter
    dw " FIGHTER'S SWORD                "
    dw " fighter s sword                "
    dw "                                "
.alttp_master
    dw " MASTER SWORD                   "
    dw " master sword                   "
    dw "                                "
.alttp_tempered
    dw " TEMPERED SWORD                 "
    dw " tempered sword                 "
    dw "                                "
.alttp_gold
    dw " GOLD SWORD                     "
    dw " gold sword                     "
    dw "                                "
    dw "                                "

    !PURPLE
    dw "           GAME STATS           "
    dw "                                "    
    !BIG

.alttp_gtbigkey
    dw " GANON'S TOWER BIG KEY          "
    dw " ganon s tower big key          "
    dw "                                "
.alttp_bonks
    dw " BONKS                          "
    dw " bonks                          "
    dw "                                "
.alttp_savequits
    dw " SAVE AND QUITS                 "
    dw " save and quits                 "
    dw "                                "
.alttp_deaths
    dw " DEATHS                         "
    dw " deaths                         "
    dw "                                "
.alttp_faerierevivals
    dw " FAERIE REVIVALS                "
    dw " faerie revivals                "
    dw "                                "
.alttp_menutime
    dw " TIME IN MENUS      00'00'00^00 "
    dw " time in menus                  "
    dw "                                "
; .alttp_lagtime
;     dw " LAG TIME           00'00'00^00 "
;     dw " lag time                       "
;     dw "                                "
;     dw "                                "
    dw "                                "
    dw "                                "
    !ORANGE
    dw "            COMBINED            "
    dw "                                "
    !BIG
.sm_transitions
    dw " TRANSITIONS TO SM              "
    dw " transitions to sm              "
    dw "                                "
.alttp_transitions
    dw " TRANSITIONS TO ALTTP           "
    dw " transitions to alttp           "
    dw "                                "
    dw "                                "
    dw "                                "
.alttp_total_time
    dw " TIME IN ALTTP      00'00'00^00 "
    dw " time in alttp                  "
    dw "                                "
.sm_total_time
    dw " TIME IN SM         00'00'00^00 "
    dw " time in sm                     "
    dw "                                "
.total_time
    dw " TOTAL TIME         00'00'00^00 "
    dw " total time                     "
    dw "                                "
.collection_rate
    dw " COLLECTED ITEMS                "
    dw " collected items                "
    dw "                                "
    dw "                                "
    dw "                                "
    dw "                                "
    dw "      SEE YOU NEXT MISSION      "
    dw "      see you next mission      "
    dw "                                "
    dw "                                "
    dw "                                "
    dw "                                "
    dw "                                "
    dw "                                "
    dw "                                "
.stop
    dw "                        THE END "
    dw "                        the end "
    dw "                                "
    dw "                                "

    !OFF
warnpc $fd8000

namespace off