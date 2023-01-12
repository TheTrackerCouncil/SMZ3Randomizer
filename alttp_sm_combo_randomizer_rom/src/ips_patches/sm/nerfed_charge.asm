;;; 
;;; Nerfed charge beam patch
;;; 
;;; Authors: Smiley for permanent charge and beam damage hijack.
;;;          Flo for hardware division usage and SBA/pseudo screw damage.
;;; 
;;; Originally disassembled from DASH IPS patch
;;; 
;;; Effects : charge beam is available from the start, with nerfed damage

;;; compile with asar v1.81 (https://github.com/RPGHacker/asar/releases/tag/v1.81)

lorom
arch 65816

;;; divides projectile damage by 3
macro divprojdmg3()
	lda $0C2C,X
	sta $4204
	sep #$20
	lda #$03
	sta $4206
	rep #$20
	pha : pla : xba : xba 	; wait for division
	lda $4214
	sta $0C2C,X
endmacro

;;; goes to charge branch whatever items
org $90b81e
	bit #$0000
	bra $0a

;;; disables a "no charge" check
org $90b8f2
	bra $00

;;; hijack for beam damage modification 
org $90b9e6
	jsr charge

;;; hijack for SBA ammo spend
org $90ccd2
	jmp fire_sba

;;; nerfed charge : damage modification
org $90f6a0
charge:
	lda $09A6		; equipped beams
	bit #$1000		; check for charge
	bne .end
	;; if no charge, nerfs charge dmg : divide by 3
	%divprojdmg3()
.end:
	lda $0C18,X
	rts

org $90f810
nochargesba:
; This alternate table is just as inefficient as the original
        dw $0000 ; 0: Power
        dw $0003 ; 1: Wave
        dw $0003 ; 2: Ice
        dw $0000 ; 3: Ice + wave
        dw $0003 ; 4: Spazer
        dw $0000 ; 5: Spazer + wave
        dw $0000 ; 6: Spazer + ice
        dw $0000 ; 7: Spazer + ice + wave
        dw $0003 ; 8: Plasma
        dw $0000 ; 9: Plasma + wave
        dw $0000 ; Ah: Plasma + ice
        dw $0000 ; Bh: Plasma + ice + wave
fire_sba:
	lda.l $90cc21, x : beq .nosba  ; Load original table and exit if sba is not active for current beams
	TAY ; Store vanilla required power bomb count
	lda $09a6 : bit #$1000 : bne + ; Check if the player has the charge beam
		; If no charge beam, substract 3 power bombs
		lda $09ce
		sec : sbc nochargesba,X
		bmi .nosba : BRA .fire ; Check if player has enough power bombs
	+
	; If charge beam, subtract the vanilla amount
	lda $09ce
	SEC : sbc $90cc21, x
	bmi .nosba : BRA .fire ; Check if player has enough power bombs
.fire
	STA $09ce ; Store the updated power bomb amount
	jmp $cce1 ; Jump to SBA code following power bomb decrement
.nosba:			    
	jmp $ccef ; Jump to RTS (no SBA)

;;; nerf pseudo screw damage
org $a0a4cc
	jsr pseudo

org $a0f800
pseudo:
	;; we can't freely use A here. Y shall contain pseudo screw dmg at the end
	pha
	lda $09A6		; equipped beams
	bit #$1000		; check for charge
	beq .nocharge
.charge:
	ldy #$00C8		; vanilla value
	bra .end
.nocharge:
	ldy #$0042		; 66 (approx 200/3)
.end:
	pla
	rts

warnpc $a0f820
