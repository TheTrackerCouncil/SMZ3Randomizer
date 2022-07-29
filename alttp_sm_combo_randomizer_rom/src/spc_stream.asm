arch 65186
org $f50000

SAMPLEHDR = $0200
SAMPLEBUF = $1000
WRITEBUF = $0400

;SAMPLEBUFSIZE = $E01F  ; 765 * 0x4b
;SAMPLEBLOCKSIZE = $004B    ; 8khz      ; skip ever 17th
;PITCH = $03fd ; 8khz
;SKIPUPLOAD = 218

SAMPLEBUFSIZE = $E070
SAMPLEBLOCKSIZE = $00AB
PITCH = $08f0 ; 17875hz
SKIPUPLOAD = 47

;SAMPLEBUFSIZE = $DF98  ; 360 * 159
;SAMPLEBLOCKSIZE = $009F    ; 16khz      ; skip ever 17th
;PITCH = $0800 ; 16khz
;SKIPUPLOAD = 17

;SAMPLEBUFSIZE = $DF3E  ; 254 * 225 (24khz)
;SAMPLEBLOCKSIZE = $00E1    ; 24khz
;PITCH = $0c00 ; 24khz
;SKIPUPLOAD = 19

;SAMPLEBUFSIZE = $DFF2   ;  182 * 315 (32khz)
;SAMPLEBLOCKSIZE = $013b    
;PITCH = $1000  ; 32khz
;SKIPUPLOAD = 20

; Call to upload the SPC program
stream_upload:
    pha : phx : phy : php
    %a8()
    jsr spc_wait_boot

    ldx #$0200      ; Get spc driver size in bytes
    ldy #$0400
    jsr spc_begin_upload
    
-
    phx : tyx
    lda.l spc_driver+$4, x
    plx
    jsr spc_upload_byte
    dex
    bne -

    ldx #SAMPLEBUFSIZE
    ldy #$1000
    jsr spc_begin_upload

-
    phx : tyx
    lda.l music_data, x
    plx
    jsr spc_upload_byte
    dex
    bne -

;     ldx #(SAMPLEBUFSIZE-$8000)
;     ldy #$9000
;     jsr spc_begin_upload

;  -
;     phx : tyx
;     lda.l music_data+$10000, x
;     plx
;     jsr spc_upload_byte
;     dex
;     bne -   

    ldy #$0400
    jsr spc_execute


    %a16()
    ; lda.w #((music_data+$10000)+(SAMPLEBUFSIZE-$8000))&$ffff
    ; sta $c0
    ; lda.w #((music_data+$10000)+(SAMPLEBUFSIZE-$8000))>>16
    ; sta $c2

    lda.w #(music_data+SAMPLEBUFSIZE)&$ffff
    sta $c0
    lda.w #(music_data>>16)
    sta $c2

    lda #$0000
    sta $ba

    lda.l music_data_table+1
    and #$00ff
    sta $bc

    lda.l music_data_table+2
    and #$00ff
    sta $be

    jsr stream_writebuf

    lda.w #SKIPUPLOAD
    sta $b8

    plp : ply : plx : pla
    rtl


; Call once per frame to upload new samples
stream_samples:
    pha : phx : phy : php
    %a8()

    lda $b8
    dec a
    bne +

    lda.b #SKIPUPLOAD
    sta $b8
    bra .nostream
+
    sta $b8

    ; Wait for SMP to request data
-
    ldx $2142
    cpx #$cafe
    bne -

    %ai16()

    ; Prepare first byte and write it
    ldy #$0000

    lda.w WRITEBUF, y
    sta $2140

    ; Write let's go to SPC
    ldx #$babe
    stx $2142

    ; Wait for SPC to echo back write index

.transfer_loop
    iny : iny
    cpy #(SAMPLEBLOCKSIZE+1)
    beq .transfer_done
    
    ldx.w WRITEBUF, y
    tya : and #$00ff

    ; Wait for SPC to say it's ready for the next frame of data
    - cmp $2142 : bne -
    
    stx $2140
    bra .transfer_loop

.transfer_done
    
    ; Write #$0000 to 2142 so SPC doesn't directly try to read more samples
    lda #$0000
    sta $2142

    jsr stream_writebuf

    ; Check for looping and reset buffer pointer
    ; Make sure the BRR sample is evenly divided by SAMPLEBLOCKSIZE
    lda $c2
    cmp.w #music_data_all_end>>16
    bne .nostream
    lda $c0
    cmp.w #music_data_all_end
    bne .nostream

    lda.w #(music_data)&$ffff
    sta $c0
    lda.w #(music_data)>>16
    sta $c2

    lda #$0000
    sta $ba

    lda.l music_data_table+1
    and #$00ff
    sta $bc

    lda.l music_data_table+2
    and #$00ff
    sta $be

.nostream
    plp : ply : plx : pla
    rtl

macro swapbank()
    lda $c2
    inc a
    cmp $bc
    bne ?nochange
    lda $be
?nochange
    sta $c2
    lda #$0000
    sta $c0
endmacro

stream_writebuf:
    phx : phy : php
    %ai16()
    ldy #$0000

.loop
    lda [$c0]
    sta.w WRITEBUF, y    

    inc $c0
    lda $c0
    bne +
    %swapbank()
+
    iny
    cpy.w #SAMPLEBLOCKSIZE
    beq .exit
    bra .loop

.exit

    ; Check if we need to update bank table
    lda $c2
    cmp $be
    bcc .nochange

    ldx $ba : inx : inx : stx $ba
    lda.l music_data_table+1, x
    and #$00ff
    sta $bc
    lda.l music_data_table+2, x
    and #$00ff
    sta $be

.nochange

    plp : ply : plx
    rts

spc_wait_boot:
    lda #$AA
    -   cmp $2140
        bne -

    ; Clear in case it already has $CC in it
    ; (this actually occurred in testing)
    sta $2140

    lda #$BB
    -   cmp $2141
        bne -

    rts

spc_begin_upload:
    sty $2142

    ; Send command
    lda $2140
    clc
    adc #$22
    bne +       ; special case fully verified
        inc
    +
    sta $2141
    sta $2140

    ; Wait for acknowledgement
    -   cmp $2140
        bne -

    ; Initialize index
    ldy.w #0

    rts

spc_upload_byte:
    sta $2141

    ; Signal that it's ready
    tya
    sta $2140
    iny

    ; Wait for acknowledgement
    -   cmp $2140
        bne -

    rts

spc_execute:
    sty $2142

    stz $2141

    lda $2140
    clc
    adc #$22
    sta $2140

    ; Wait for acknowledgement
    -   cmp $2140
        bne -

    rts

music_data_table:
    db $60, $7e     ; start bank - end bank
    db $00, $00

spc_driver:
arch spc700-inline

BUFPTR = $10
PREVTIMER = $12

CAFE = $e0
BABE = $e2
BLOCKSIZE = $e4
BUFEND = $e6

; 16-bit moves to zero page
macro movwiz(addr, val)
    mov <addr>, #<val>&$ff
    mov <addr>+1, #<val>>>8
endmacro

macro movwmz(addr, mem)
    mov <addr>, <mem>
    mov <addr>+1, <mem>+1
endmacro

; 16-bit moves
macro movwi(addr, val)
    mov a, #<val>&$ff
    mov <addr>, a
    mov a, #<val>>>8
    mov <addr>+1, a
endmacro

macro movwm(addr, mem)
    mov ya, mem
    mov <addr>, a
    mov <addr>+1, y
endmacro

; DPS writes
macro movdsp(reg, val)
    mov $f2, #<reg>
    mov $f3, #<val>
endmacro

org $0400

    ; write some useful zero page things
    %movwi(SAMPLEHDR, SAMPLEBUF)
    %movwi(SAMPLEHDR+2, SAMPLEBUF)
    %movwi(CAFE, $CAFE)
    %movwi(BABE, $BABE)
    %movwi(BLOCKSIZE, SAMPLEBLOCKSIZE)
    %movwi(BUFEND, SAMPLEBUF+SAMPLEBUFSIZE)
    %movwi(BUFPTR, SAMPLEBUF)
    
    ; Disable timers
    mov $f0, #$0a
    mov $f1, #$00
    mov PREVTIMER, #$00

    ; Playing at 24khz we're playing $5DC0 samples per second ($34BC BRR bytes per second, $05DC BRR blocks per second)
    ; At 60hz SNES clock rate, that's $E1 (225) bytes per frame to transfer

    ; For 32khz it'll be $4650 BRR bytes, $07D0 BRR blocks, $21 blocks per frame ($129 (297 bytes)) per frame

    ; Set a timer for 60.15hz (slightly faster than the SNES clock) and every time this increments, we
    ; transfer 25 BRR blocks (225 bytes)
    ;mov $fa, #$85

    ; Set up DSP for sample playing on channel 0
    %movdsp($6c, $20) ; FLG - Unmute and disable echo
    %movdsp($5c, $ff) ; Key-off all voices
    %movdsp($5c, $00) ; Key-off all voices
    %movdsp($4c, $00) ; Key-off all voices
    %movdsp($4d, $00) ; Clear EON
    %movdsp($7d, $00) ; Echo buffer size to $00 (4 bytes)
    %movdsp($6d, $03) ; Echo buffer to $0300
    %movdsp($5d, $02) ; DIR - Set sample table to $0200
    %movdsp($2c, $00) ; EVOLL - Set left echo volume
    %movdsp($3c, $00) ; EVOLR - Set right echo volume
    %movdsp($0d, $00) ; EFB - Set echo feedback volume
    %movdsp($0c, $7f) ; MVOLL - Set left master volume
    %movdsp($1c, $7f) ; MOVLR - Set right master volume
    %movdsp($00, $7f) ; VxVOLL - Set left channel 0 volume
    %movdsp($01, $7f) ; VxVOLR - Set right channel 0 volume
    %movdsp($02, PITCH&$ff) ; VxPITCHL - Set channel 0 pitch (low byte)
    %movdsp($03, PITCH>>8) ; VxPTTCHH - Set channel 0 pitch (hi byte) (Set to 24khz)
    %movdsp($2d, $00) ; PMON - Set pitch modulation off
    %movdsp($05, $00) ; VxADSR1 - Set channel 0 to use direct gain (No ADSR)
    %movdsp($06, $00) ; VxADSR2 - Set channel 0 ADSR parameters
    %movdsp($07, $7f) ; VxGAIN - Set channel 0 gain
    %movdsp($04, $00) ; VxOUTX - Set channel 0 sample
    %movdsp($4c, $01) ; Key-on channel 0

    ; Write loop flag to last BRR block
    mov a, (SAMPLEBUF+SAMPLEBUFSIZE)-9
    or a, #$03
    mov (SAMPLEBUF+SAMPLEBUFSIZE)-9, a

; Timing is done by the SNES, data sent once per frame
.spc_mainloop
    ; Send $CAFE to SNES to let it know we want more data
    movw ya, CAFE
    movw $f6, ya

    ; Read $f6-$f7 (should be $BABE)
-
    movw ya, $f6
    cmpw ya, BABE
    bne -

    ; Set byte counter to $00
    mov y, #$00
    mov x, #$00
    mov $f6, #$00
    mov $f7, #$00
    %movwi($00, (SAMPLEBLOCKSIZE+1)>>1)

.transfer_loop
    mov a, $f4
    mov (BUFPTR)+y, a
    inc y

    mov a, $f5
    mov (BUFPTR)+y, a
    inc y
    bne +
    inc BUFPTR+1
+
    mov $f6, y
    dbnz $00, .transfer_loop

.transfer_done

    ; Update buffer pointer
    dec y
    mov a, y
    mov y, #$00
    
    addw ya, BUFPTR
    cmpw ya, BUFEND
    bne +

    ; If we're at the end, set loop flag on the last BRR block
    mov a, (SAMPLEBUF+SAMPLEBUFSIZE)-9
    or a, #$03
    mov (SAMPLEBUF+SAMPLEBUFSIZE)-9, a

    mov a, #(SAMPLEBUF)&$ff
    mov y, #(SAMPLEBUF)>>8

+
    movw BUFPTR, ya
    jmp .spc_mainloop

arch 65816
spc_driver_end:


pushpc
check bankcross off
org $600000
music_data:
    incbin "data/music.brr"

; org $440000
;     incbin "data/music.brr":($8000)-($10000)
; org $450000
;     incbin "data/music.brr":($10000)-($18000)
; org $460000
;     incbin "data/music.brr":($18000)-($20000)
; org $470000
;     incbin "data/music.brr":($20000)-($28000)
; org $480000
;     incbin "data/music.brr":($28000)-($30000)
; org $490000
;     incbin "data/music.brr":($30000)-($38000)
; org $4A0000
;     incbin "data/music.brr":($38000)-($40000)
; org $4B0000
;     incbin "data/music.brr":($40000)-($48000)
; org $4C0000
;     incbin "data/music.brr":($48000)-($50000)
; org $4D0000
;     incbin "data/music.brr":($50000)-($58000)
; org $4E0000
;     incbin "data/music.brr":($58000)-($60000)
; org $4F0000
;     incbin "data/music.brr":($60000)-($68000)
; music_data_end:

; org $540000
; music_data_2:
;     incbin "data/music.brr":($68000)-($70000)
; org $550000
;     incbin "data/music.brr":($70000)-($78000)
; org $560000
;     incbin "data/music.brr":($78000)-($80000)
; org $570000
;     incbin "data/music.brr":($80000)-($88000)
; org $580000
;     incbin "data/music.brr":($88000)-($90000)
; org $590000
;     incbin "data/music.brr":($90000)-($98000)
; org $5A0000
;     incbin "data/music.brr":($98000)-($A0000)
; org $5B0000
;     incbin "data/music.brr":($A0000)-($A8000)
; org $5C0000
;     incbin "data/music.brr":($A8000)-($B0000)
; org $5D0000
;     incbin "data/music.brr":($B0000)-($B8000)
; music_data_2_end:

; org $f60000
; music_data_3:
;      incbin "data/music.brr":($B8000)-($C0000)
; org $f70000
;      incbin "data/music.brr":($C0000)-0; ($C8000)
; music_data_3_end:

; org $f90000
; music_data_4:
;     incbin "data/music.brr":($C8000)-($D0000)
; org $fa0000
;     incbin "data/music.brr":($D0000)-($D8000)
; org $fb0000
;     incbin "data/music.brr":($D8000)-($E0000)
; org $fc0000
;     incbin "data/music.brr":($E0000)-($E8000)
;music_data_4_end:
music_data_all_end:
check bankcross on
pullpc
