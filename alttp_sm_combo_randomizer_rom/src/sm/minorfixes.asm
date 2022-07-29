;Fix music coming into the maridia portal
org $CFD924
    db $1B, $05

;Fix for escape bomb block softlock by Capn
;Hijack the door ASM
org $cfe4cf
base $8fe4cf
	jsr NewShaftDoorASM
	rts

;Free space at ec00
org $cfec00
base $8fec00
NewShaftDoorASM:
	;Start with Original ASM functions
	php
	%a8()
	lda #$01
	sta $7ecd38
	lda #$00
	sta $7ecd39
	;Set all blocks in RAM to air
	;Got lucky here that asm code is done after tile loading ^_^
	%a16()
	lda #$00ff
	sta $7f3262
	sta $7f3264
	sta $7f32c2
	sta $7f32c4
	sta $7f3322
	sta $7f3324
	sta $7f3382
	sta $7f3384
	plp
	rts


; Create a new refill room with two doors to be used instead of the regular LN refill room
; so that the right door can lead to an ALTTP portal and still preserve the refill room

org $cfed00
NewLNRefillRoom:
; Room $ED00: Header
.header
	db $39, $02, $15, $0F, $01, $01, $70, $A0, $00 
	dw .doors, $E5E6

.state
; Room $ED00: Default State
	dl $CE8FA6 
	db $17, $00, $03
	dw $87BA, $A623, $8777, $0000, $0000, $0000, $0000, $8D9C, $0000, $91F7

.doors
; Room $ED00: Door list
    dw $98A6 				; Back to Screw attack room
	dw NewLNRefillDoorData_exit 	; New door leading out to ALTTP
	
org $c3ae00
NewLNRefillDoorData:
; New door table data for the added door
.exit
	dw $AFFB : db $00, $04, $01, $06, $00, $00 : dw $8000, $0000
.entry
	dw NewLNRefillRoom : db $00, $05, $0E, $06, $00, $00 : dw $8000, $0000

; Repoint screw attack door into the new room
org $c39a7a
	dw NewLNRefillRoom
