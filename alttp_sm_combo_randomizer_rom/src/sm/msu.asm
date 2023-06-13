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

; Init MSU and check for missing tracks
org $C08564
    jsl init_msu1

; Run SM MSU-1 code on music change
org $C08F27
base $808F27
	jsr SM_MSU_Main

org $C0FA00
base $80FA00
init_msu1:
    jsl MSUInit

    ; original hooked code
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
	
	%CheckMSUPresence(.OriginalCode)
	
	;; Load current requested music
	lda.w !RequestedMusic
	and.b #$7F
	bne +
	jmp .OriginalCode
+

	;; $04 is usually ambience, call original code
	cmp.b #$04
	bne +
	jmp .OriginalCode
+

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
	beq .OriginalCode
.PlayMusic
    CMP #10 : BEQ .LoadSamusTheme
    BRA .CheckFallbacks

.LoadSamusTheme
    TAX
    REP #$20

    ; Play no music following the baby draining Samus's health
    LDA $079B
    CMP #$DCB1 : BNE +
        SEP #$20
        TXA
        BRA .OriginalCode
    +

    SEP #$20
    TXA
    bra .CheckFallbacks

.CheckFallbacks
	;; Check track fallback list if the track is available
	pha
	DEC : PHA
		AND.b #$07 : TAY
		PLA : LSR #3 : TAX
	LDA.l MSUFallbackTable+$10,X : BEQ .OriginalCode : CMP.b #$FF : BEQ .continue
	
	- : CPY #$00 : BEQ +
		LSR : DEY : BRA -
	+
	
	AND.b #$01 : BEQ .OriginalCode

	.continue
	pla
	tay
	clc : adc.b #!TRACK_OFFSET

	sta.w !MSU_AUDIO_TRACK_LO
	stz.w !MSU_AUDIO_TRACK_HI
	
.CheckAudioStatus
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