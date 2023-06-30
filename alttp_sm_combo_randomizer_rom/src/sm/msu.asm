; Track List:
;
; 1/101 - Apperance fanfare
; 2/102 - Item acquired (not used in SMZ3)
; 3/103 - Item/elevator room
; 4/104 - Opening with intro
; 5/105 - Opening without intro
; 6/106 - Crateria - First landing (with thunder)
; 7/107 - Crateria - First landing (without thunder) (not used in SMZ3)
; 8/108 - Crateria - Space Pirates Appear
; 9/109 - Crateria - Golden statues room
; 10/110 - Theme of Samus Aran (Samus's Ship & East Crateria)
; 11/111 - Green Brinstar
; 12/112 - Red Brinstar
; 13/113 - Upper Norfair
; 14/114 - Lower Norfair
; 15/115 - Inner Maridia
; 16/116 - Outer Maridia
; 17/117 - Tourian
; 18/118 - Mother Brain battle
; 19/119 - Big Boss Battle 1 (Chozo statues, Ridley, and Draygon)
; 20/120 - Evacuation
; 21/121 - Chozo statue awakens
; 22/122 - Big Boss Battle 2 (Crocomire, Kraid, Phantoon, Baby Metroid) 
; 23/123 - Tension/Hostile Incoming (before Kraid, Phantoon, and Baby Metroid. Played in between Croc segments)
; 24/124 - Plant miniboss (Sporespawn and Botwoon)
; 25/125 - Ceres Station (not used in SMZ3)
; 26/126 - Wrecked Ship Powered Off
; 27/127 - Wrecked Ship Powered On
; 28/128 - Theme of Super Metroid (not used in SMZ3)
; 29/129 - Death cry
; 30/130 - Ending
;
; SM Extended tracks
;
; 31/131 - Kraid incoming (falls back to 23/123)
; 32/132 - Kraid battle (falls back to 22/122)
; 33/133 - Phantoon incoming (falls back to 23/123)
; 34/134 - Phantoon battle (falls back to 22/122)
; 35/135 - Draygon battle (falls back to 19/119)
; 36/136 - Ridley battle (falls back to 19/119)
; 37/137 - Baby incoming (falls back to 23/123)
; 38/138 - The baby (falls back to 22/122)
; 39/139 - Hyper beam (falls back 10/110)
; 40/140 - Game over

;;; MSU memory map I/O
!MSU_STATUS = $2000
!MSU_ID = $2002
!MSU_AUDIO_TRACK_LO = $2004
!MSU_AUDIO_TRACK_HI = $2005
!MSU_AUDIO_VOLUME = $2006
!MSU_AUDIO_CONTROL = $2007

;;; SPC communication ports
!SPC_COMM_0 = $2140

;;; MSU_STATUS possible values
!MSU_STATUS_TRACK_MISSING = $8
!MSU_STATUS_AUDIO_PLAYING = %00010000
!MSU_STATUS_AUDIO_REPEAT = %00100000
!MSU_STATUS_AUDIO_BUSY = $40
!MSU_STATUS_DATA_BUSY = %10000000

!RequestedMusic = $063D
!CurrentMusic = $064C
!MusicBank = $07F3

!FULL_VOLUME = $FF

!TRACK_OFFSET = 100

!VAL_COMMAND_MUTE_SPC = $FA
!VAL_COMMAND_UNMUTE_SPC = $FB

;;; **********
;;; * Macros *
;;; **********
macro CheckMSUPresence(labelToJump)
    lda.w !MSU_ID
    cmp.b #'S'
    BEQ + : jmp <labelToJump> : +
endmacro

macro CheckMSUPresence16(labelToJump)
    lda.w !MSU_ID
    cmp.w #$2D53
    BEQ + : jmp <labelToJump> : +
endmacro

; Init MSU and check for missing tracks
org $C08564
    jsl init_msu1

; Run SM MSU-1 code on music change
org $C08F27
base $808F27
    jsr SM_MSU_Main

org $C0F900
base $80F900
init_msu1:
    %CheckMSUPresence16(.exit)
    
    ; Check if SRAM seed data matches ROM seed data.
    ; If it does then there's no need to check the track data
    ldx #$0000
.check_seed
    lda.l rando_seed_data+$10, x        ; Seed guid
    cmp.l !SRAM_MSU_SEED_DATA, x
    bne .do_init
    inx : inx
    cpx #$0020
    bne .check_seed
    bra .exit

.do_init    
    jsl MSUInit

    ldx #$0000
.store_seed
    lda.l rando_seed_data+$10, x        ; Seed guid
    sta.l !SRAM_MSU_SEED_DATA, x
    inx : inx
    cpx #$0020
    bne .store_seed

    ; original hooked code
.exit
    jsl $808261 ; Check for non-corrupt SRAM
    rtl

SM_MSU_Main:
    pha
    phx
    phy
    php
    phb
    
    sep #$30
    
    ;; Make sure the data bank is set to $80
    lda #$80
    pha
    plb
    
    %CheckMSUPresence(.Exit)
    
    ;; Load current requested music
    lda.w !RequestedMusic
    and.b #$7F
    bne +
    jmp .OriginalCode
+

    ;; $04 is usually ambience, call original code
    cmp.b #$04 : BEQ .PlayAmbience

    ;; Check if the song is already playing
    cmp.w !CurrentMusic
    bne +
    jmp .Exit
+    
    ;; If the requested music is less than 4
    ;; it's the common music, skip to play music
    cmp.b #$05
    bmi .PlayMusic
    
    ;; If requested music is greater or equal to 5
    ;; Figure out which music to play depending of
    ;; the current music bank
    sec
    sbc.b #$05
    tay
    
    ;; Load music bank and divide it by 3
    lda.w !MusicBank
    ldx.b #$00
    sec
-
    sbc.b #$3
    bcc +
    inx
    bne -
+
    ;; Load music mapping pointer for current bank
    txa
    asl
    tax
    rep #$20
    lda.l MusicMappingPointers,x
    sta.b $00
    ;; Load music to play from pointer
    sep #$20
    lda ($00),y

    ;; Loading $00 means calling the original code
    bne .PlayMusic
    jml .OriginalCode

.PlayAmbience
    PHA
    LDA.w $0998
    CMP #$1A : BEQ +
    PLA
    JMP .OriginalCode
+
    PLA
    LDA #40
    JML .CheckFallbacks

.PlayMusic
    CMP #10 : BEQ .LoadSamusTheme
    CMP #19 : BEQ .LoadBossThemeOne
    CMP #22 : BEQ .LoadBossThemeTwo
    CMP #23 : BEQ .LoadTensionTheme
    JML .CheckFallbacks

.LoadSamusTheme ; (10)
    REP #$20
    LDA $079B
    LDX #00

    ; Play no music following the baby draining Samus's heath
    CMP #$DCB1 : BNE +
        LDX #$FF
    +
    ; Play "hyper beam" theme when fighting the last phase of Mother Brain
    CMP #$DD58 : BNE +
        LDX #39
    +

    SEP #$20
    LDY #10
    bra .TryExtended

; Boss theme for Ridley, Draygon, and Torizo statues
.LoadBossThemeOne ; (19)
    REP #$20
    LDA $079B
    LDX #00

    ; Draygon
    CMP #$DA60 : BNE +
        LDX #35
    +
    ; Ridley
    CMP #$B32E : BNE +
        LDX #36
    +

    SEP #$20
    LDY #19
    bra .TryExtended

; Boss theme for Kraid, Crocomire, Phantoon, and the Baby
.LoadBossThemeTwo ; (22)
    LDA $079F : TAX
    LDA BossTwoExtendedThemes,X : TAX
    LDY #22
    bra .TryExtended

; Song before fighting some of the bosses
.LoadTensionTheme ; (23)
    LDA $079F : TAX
    LDA TensionExtendedThemes,X : TAX
    LDY #23
    bra .TryExtended

; X = Extended Song (00 => Skip to Fallback, FF -> Original Code)
; Y = Original Song
.TryExtended
    CPX #00 : BNE +
        TYA
        jmp .CheckFallbacks
    +
    CPX #$FF : BNE +
        TYA
        jmp .OriginalCode
    +
    
    PHX
    PHY
    TXA
    DEC : PHA
        AND.b #$07 : TAY
        PLA : LSR #3 : TAX
    LDA.l MSUFallbackTable+$10,X : BEQ .FailExtended : CMP.b #$FF : BEQ .ContinueExtended

    - : CPY #$00 : BEQ +
        LSR : DEY : BRA -
    +
    
    AND.b #$01 : BEQ .FailExtended

    BRA .ContinueExtended

.FailExtended
    PLA
    PLY
    BRA .CheckFallbacks

.ContinueExtended
    PLY
    BRA .continue

.CheckFallbacks
    ;; Check track fallback list if the track is available
    pha
    DEC : PHA
        AND.b #$07 : TAY
        PLA : LSR #3 : TAX
    LDA.l MSUFallbackTable+$10,X : BEQ .fallback : CMP.b #$FF : BEQ .continue
    
    - : CPY #$00 : BEQ +
        LSR : DEY : BRA -
    +
    
    AND.b #$01 : BEQ .fallback

    .continue
    pla
    tay
    clc : adc.b #!TRACK_OFFSET

    sta.w !MSU_AUDIO_TRACK_LO
    stz.w !MSU_AUDIO_TRACK_HI
    
.CheckAudioStatus
    lda.w !MSU_STATUS : bit.b #!MSU_STATUS_AUDIO_BUSY : bne .CheckAudioStatus

    ;; Play the song and add repeat if needed
    jsr TrackNeedLooping
    sta.w !MSU_AUDIO_CONTROL
    
    ;; Set volume
    lda.b #!FULL_VOLUME
    sta.w !MSU_AUDIO_VOLUME
    
    ;; Mute SPC music
    lda.b #!VAL_COMMAND_MUTE_SPC    
    - : STA.w APUIO0 : CMP.w APUIO0 : BNE - ; Wait until mute/unmute command is ACK'ed
    - : STZ.w APUIO0 : LDA.w APUIO0 : BNE - ; Wait until mute/unmute command is completed
    bra .Exit

.fallback
    PLA
    BRA .OriginalCode

.OriginalCode
    lda.b #$00
    sta.w !MSU_AUDIO_CONTROL
    sta.w !MSU_AUDIO_VOLUME

    ;; Unmute SPC music
    lda.b #!VAL_COMMAND_UNMUTE_SPC    
    - : STA.w APUIO0 : CMP.w APUIO0 : BNE - ; Wait until mute/unmute command is ACK'ed
    - : STZ.w APUIO0 : LDA.w APUIO0 : BNE - ; Wait until mute/unmute command is completed

.Exit
    plb
    plp
    ply
    plx
    pla

    sta.w !SPC_COMM_0
    rts
    
MusicMappingPointers:
    dw bank_00
    dw bank_03
    dw bank_06
    dw bank_09
    dw bank_0C
    dw bank_0F
    dw bank_12
    dw bank_15
    dw bank_18
    dw bank_1B
    dw bank_1E
    dw bank_21
    dw bank_24
    dw bank_27
    dw bank_2A
    dw bank_2D
    dw bank_30
    dw bank_33
    dw bank_36
    dw bank_39
    dw bank_3C
    dw bank_3F
    dw bank_42
    dw bank_45
    dw bank_48

MusicMapping:
;; 00 means use SPC music
bank_00: ;; Opening
    db 04,05
bank_03: ;; Opening
    db 04,05
bank_06: ;; Crateria (First Landing)
    db 06,00,07
bank_09: ;; Crateria
    db 08,09
bank_0C: ;; Samus's Ship
    db 10
bank_0F: ;; Brinstar with vegatation
    db 11
bank_12: ;; Brinstar Red Soil
    db 12
bank_15: ;; Upper Norfair
    db 13
bank_18: ;; Lower Norfair
    db 14
bank_1B: ;; Maridia
    db 15,16
bank_1E: ;; Tourian
    db 17,00
bank_21: ;; Mother Brain Battle
    db 18
bank_24: ;; Big Boss Battle 1 (3rd is with alarm)
    db 19,21,20
bank_27: ;; Big Boss Battle 2
    db 22,23
bank_2A: ;; Plant Miniboss
    db 24
bank_2D: ;; Ceres Station
    db 00,25,00
bank_30: ;; Wrecked Ship
    db 26,27
bank_33: ;; Ambience SFX
    db 00,00,00
bank_36: ;; Theme of Super Metroid
    db 28
bank_39: ;; Death Cry
    db 29
bank_3C: ;; Ending
    db 30
bank_3F: ;; "The Last Metroid"
    db 00
bank_42: ;; "is at peace"
    db 00
bank_45: ;; Big Boss Battle 2
    db 22,23
bank_48: ;; Samus's Ship (Mother Brain)
    db 10

BossTwoExtendedThemes:
db #00,#32,#00,#34,#00,#38 ; Boss theme two

TensionExtendedThemes:
db #00,#31,#00,#33,#00,#37 ; Boss theme two

TrackNeedLooping:
;; Samus Aran's Appearance fanfare
    cpy.b #01
    beq NoLooping
;; Item acquisition fanfare
    cpy.b #02
    beq NoLooping
;; Death fanfare
    cpy.b #29
    beq NoLooping
;; Ending
    cpy.b #30
    beq NoLooping

    lda.b #$03
    rts
NoLooping:
    lda.b #$01
    rts