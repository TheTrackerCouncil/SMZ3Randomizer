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


; During horizontal door transitions, the "ready for NMI" flag is set by IRQ at the bottom of the door as an optimisation,
; but the PLM drawing routine hasn't necessarily finished processing yet.
; The Kraid quick kill vomit happens because NMI actually interrupts the PLM drawing routine for the PLM that clears the spike floor,
; *whilst* it's in the middle of writing entries to the $D0 table, which the NMI processes.

; This fix simply clears this NMI-ready flag for the duration of the PLM drawing routine.
; Place this into the freespace after the free space used by items.asm
org $c4fe00
base $84fe00

drawPlmSafe:
{
lda.w $05B4 : pha ; Back up NMI ready flag
stz.w $05B4 ; Not ready for NMI
jsr $8DAA   ; Draw PLM
pla : sta.w $05B4 ; Restore NMI ready flag
rts

warnpc $858000
}

; Patch calls to draw PLM
org $c4861a ; End of PLM processing. Probably the only particularly important one to patch
base $84861a
jsr drawPlmSafe

; org $c48b50 ; End of block respawn instruction. Shouldn't need patching
; base $848b50
; jsr drawPlmSafe

org $c4e094 ; End of animated PLM drawing instruction. Could theoretically happen...
base $84e094
jsr drawPlmSafe
