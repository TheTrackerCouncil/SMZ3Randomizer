org $f57e00
incbin "data/spc-header.bin"
org $f57f00
incbin "data/spc-dmaregs.bin"
org $f60000
incbin "data/spc-lo.bin"
org $f70000
incbin "data/spc-hi.bin"

!AUDIO_R0 = $2140
!AUDIO_R1 = $2141
!AUDIO_R2 = $2142
!AUDIO_R3 = $2143

!XY_8BIT = $10
!A_8BIT = $20

!waitforaudio = "- : cmp !AUDIO_R0 : bne -"
!spcfreeaddr = $ff80

; Set up labels for use in code
org $f57e25
audiopc:
org $f57e27
audioa:
org $f57e28
audiox:
org $f57e29
audioy:
org $f57e2a
audiopsw:
org $f57e2b
audiosp:
org $f60000
musicdata1:
org $f70000
musicdata2:
org $f57f00
dspdata:

org $f50000
playmusic:
    phx
    phy
    pha
    php
    phb

    ; Set bank to 80 for easier access to SMP registers
    pea $8080
    plb
    plb

    sep #$20
    rep #$10

    ; Send $00 to SMP to stop music
    ;lda #$00
    ;sta $002140
    
    ; Send $ff to SMP to enter upload mode
    ;lda #$ff
    ;sta $002140

    ; Clear $12-14 for use later
    rep #$30
    lda #$0000
    sta $12
    sta $14

    ; Call SM's own APU upload code with our "hacked" data to break out of
    ; SM's own SMP code, turn on IPL ROM and jump to it and wait for further uploads
    ;jsl $80800a
    ;dl sm_spc_data

    ; Call our own SPC loading code that uploads the new SPC data and executes it
    jsr loadspc

    plb
    plp
    pla
    ply
    plx

    rtl

loadspc:
    ; Turn off interrupts and NMI during SPC transfer
    lda #$0000
    sta $004200
    sei

    ; Copy $02-ef to SPC RAM
    ; sendmusicblock $e0 $c002 $0002 $00ee
    sep #$30
    lda $2002 : cmp.b #'S' : BEQ +
        rep #$30
        sep #!A_8BIT
        lda #$f6    ; 1
        sta $14
        rep #!A_8BIT
        lda #$0002  ; 2
        sta $12

        rep #!XY_8BIT
        ldx #$0002 ; 3
        ldy #$00ee ; 4
        jsr copyblocktospc
    +
    rep #!XY_8BIT

    ; sendmusicblock $f6 $0100 $0100 $7f00
    sep #!A_8BIT
    lda #$f6 ; 1
    sta $14
    rep #!A_8BIT
    lda #$0100 ; 2
    sta $12

    rep #!XY_8BIT
    ldx #$0100 ; 3
    ldy #$7f00 ; 4
    jsr copyblocktospc

    ; sendmusicblock $f7 $0000 $8000 $7fc0
    sep #$30
    lda $2002 : cmp.b #'S' : BEQ +
        rep #$30
        sep #!A_8BIT
        lda #$f7 ; 1
        sta $14
        rep #!A_8BIT
        lda #$0000 ; 2
        sta $12

        rep #!XY_8BIT
        ldx #$8000 ; 3
        ldy #$7fc0 ; 4
        jsr copyblocktospc
    +
    rep #!XY_8BIT

    ; Create SPC init code that sets up registers
    jsr makespcinitcode

    ; Copy init code to RAM
    ; sendmusicblock $7e $ff00 {spcfreeaddr} $003a
    sep #!A_8BIT
    lda #$7e ; 1
    sta $14
    rep #!A_8BIT
    lda #$ff00 ; 2
    sta $12

    rep #!XY_8BIT
    ldx #!spcfreeaddr ; 3
    ldy #$003a ; 4
    jsr copyblocktospc
    ; endmacro

    ; Initialize the DPS with values from the SPC
    jsr initdsp

    ; Start execution of init code, first $f0-ff init and then registers and finally
    ; jump to the SPC entry point
    rep #!XY_8BIT
    ldx #!spcfreeaddr
    jsr startspcexec

    ; Restore interrupts and NMI and exit
    ;cli
    ;sep #!A_8BIT
    ;lda #$80
    ;sta $004200
    rts

copyblocktospc:
    sep #!A_8BIT
    lda #$aa
    !waitforaudio

    stx !AUDIO_R2

    tyx

    lda #$01
    sta !AUDIO_R1
    lda #$cc
    sta !AUDIO_R0
    !waitforaudio

    ldy #$0000

.loop:
    xba
    lda [$12], y
    xba

    tya

    rep #!A_8BIT
    sta !AUDIO_R0
    sep #!A_8BIT

    !waitforaudio

    iny
    dex
    bne .loop

    ldx #$ffc9
    stx !AUDIO_R2

    xba
    lda #$00
    sta !AUDIO_R1
    xba

    clc
    adc #$02

    rep #!A_8BIT
    sta !AUDIO_R0
    sep #!A_8BIT

    !waitforaudio
    rts

startspcexec:
    sep #!A_8BIT
    lda #$aa
    !waitforaudio

    stx !AUDIO_R2

    lda #$00
    STA !AUDIO_R1
    lda #$cc
    STA !AUDIO_R0
    !waitforaudio
    rts

initdsp:
    rep #!XY_8BIT
    ldx #$0000
-
    cpx #$006c
    beq .skip
    cpx #$007d
    beq .skip
    cpx #$004c
    beq .skip
    cpx #$005c
    beq .skip

    sep #!A_8BIT
    txa
    sta $7eff00
    lda.l dspdata,x

    sta $7eff01
    phx

    ; sendmusicblock $7e $ff00 $00f2 $0002
    sep #!A_8BIT
    lda #$7e ; 1
    sta $14
    rep #!A_8BIT
    lda #$ff00 ; 2
    sta $12

    rep #!XY_8BIT
    ldx #$00f2 ; 3
    ldy #$0002 ; 4
    jsr copyblocktospc
    ; endmacro

    rep #!XY_8BIT
    plx

.skip:
    inx
    cpx #$0080
    bne -
    rts

makespcinitcode:
    sep #!A_8BIT

    lda $f60001
    pha

    lda $f60000
    pha

    lda #$8f
    sta $7eff00
    pla
    sta $7eff01
    lda #$00
    sta $7eff02

    lda #$8f
    sta $7eff03
    pla
    sta $7eff04
    lda #$01
    sta $7eff05

    lda #$cd
    sta $7eff06
    lda.l audiosp
    sta $7eff07
    lda #$bd
    sta $7eff08

    lda #$cd
    sta $7eff09
    lda.l audiopsw
    sta $7eff0a
    lda #$4d
    sta $7eff0b

    lda #$cd
    sta $7eff0c
    lda.l audiox
    sta $7eff0d

    lda #$8d
    sta $7eff0e
    lda.l audioy
    sta $7eff0f

    lda #$8f
    sta $7eff10
    lda.l musicdata1+$fc
    sta $7eff11
    lda #$fc
    sta $7eff12

    lda #$8f
    sta $7eff13
    lda.l musicdata1+$fb
    sta $7eff14
    lda #$fb
    sta $7eff15

    lda #$8f
    sta $7eff16
    lda.l musicdata1+$fa
    sta $7eff17
    lda #$fa
    sta $7eff18

    lda #$8f
    sta $7eff19
    lda.l musicdata1+$f1
    sta $7eff1a
    lda #$f1
    sta $7eff1b

    lda #$e4
    sta $7eff1c
    lda #$fd
    sta $7eff1d

    lda #$e4
    sta $7eff1e
    lda #$fe
    sta $7eff1f

    lda #$e4
    sta $7eff20
    lda #$ff
    sta $7eff21

    lda #$e8
    sta $7eff22
    lda.l audioa
    sta $7eff23

    lda #$8f
    sta $7eff24
    lda #$7d
    sta $7eff25
    lda #$f2
    sta $7eff26

    lda #$8f
    sta $7eff27
    lda.l dspdata+$7d
    sta $7eff28
    lda #$f3
    sta $7eff29

    lda #$8f
    sta $7eff2a
    lda #$6c
    sta $7eff2b
    lda #$f2
    sta $7eff2c

    lda #$8f
    sta $7eff2d
    lda.l dspdata+$6c
    sta $7eff2e
    lda #$f3
    sta $7eff2f

    lda #$8f
    sta $7eff30
    lda #$4c
    sta $7eff31
    lda #$f2
    sta $7eff32

    lda #$8f
    sta $7eff33
    lda.l dspdata+$4c
    sta $7eff34
    lda #$f3
    sta $7eff35

    lda #$8e
    sta $7eff36

    lda #$5f
    sta $7eff37
    rep #!A_8BIT
    lda.l audiopc
    sta $7eff38
    sep #!A_8BIT
    xba
    sta $7eff39
    
    rts

; Overwrite some code in the SM music engine that sets up the transfers
credits_sm_spc_data:
    dw $002a, $15a0
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
    dw $0000, $1500
