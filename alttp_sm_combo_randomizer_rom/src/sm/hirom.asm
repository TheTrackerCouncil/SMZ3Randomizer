; Super Metroid ExHiROM patch
;

exhirom

!SRAM_BANK = #$00a1
!SRAM_BASE = $a16000	; Select where SRAM is mapped to (default is a0:6000-7FFF)

org $c08000				; Disable copy protection screen
	db $ff

;========================== SRAM Load/Save Repoint ===============================

org $c08257
	sta !SRAM_BASE+$1fe0,x

org $c0828a
	sta !SRAM_BASE+$1fe0,x

org $c08297
	lda !SRAM_BASE+$1fe0,x

org $c08698
	lda !SRAM_BASE,x
	
org $c086aa
	sta !SRAM_BASE,x
	
org $c086b8
	sta !SRAM_BASE,x
	
org $c086c7
	cmp !SRAM_BASE,x

org $c086d9
	sta !SRAM_BASE,x
	
org $c18056
	sta !SRAM_BASE,x
	
org $c1806c
	sta !SRAM_BASE,x
	sta !SRAM_BASE+$1ff0,x
	eor #$ffff
	sta !SRAM_BASE+$0008,x
	sta !SRAM_BASE+$1ff8,x

org $c180a0
	lda !SRAM_BASE,x
	
org $c180b9
	cmp !SRAM_BASE,x

org $c180c2
	cmp !SRAM_BASE+$0008,x
	
org $c180cc
	cmp !SRAM_BASE+$1ff0,x
	
org $c180d5
	cmp !SRAM_BASE+$1ff8,x
	
org $c1810b
	sta !SRAM_BASE,x
		
org $c19ed8
	lda !SRAM_BASE+$1fec
	
org $c19ee7
	and !SRAM_BASE+$1fee
	
org $c19ccb
	sta !SRAM_BASE,x
	sta !SRAM_BASE+$0008,x
	sta !SRAM_BASE+$1ff0,x
	sta !SRAM_BASE+$1ff8,x

org $c1a23c
	sta !SRAM_BASE+$1fec

org $c1a243
	sta !SRAM_BASE+$1fee

org $c19a3b
	lda !SRAM_BANK

org $c19a58
	ldy #$6000

org $c19a61
	cpy #$665c

org $c19a6b
	lda !SRAM_BASE+$1ff0,x
	pha
	lda !SRAM_BASE+$1ff8,X
	pha
	lda !SRAM_BASE,x
	pha
	lda !SRAM_BASE+$0008,x
	pha

org $c19a85
	sta !SRAM_BASE+$0008,x
	pla
	sta !SRAM_BASE,x
	pla
	sta !SRAM_BASE+$1ff8,x
	pla
	sta !SRAM_BASE+$1ff0,x

org $c19ca4
	lda !SRAM_BANK

org $c19cb4
	ldy #$6000

org $c19cbe
	cpy #$665c


;==================================================================================
	

;========================== Music/SFX Bank Loading ===============================
org $c08044			; Patch music loading code to load from $0000-7FFF banks
	jmp $ff00
	
org $c0ff00			; This patch will subtract $8000 from the initial music bank 
	rep #$20		; address so it correctly loads from the lower bank
	lda $00
	and #$7FFF
	tay
	sep #$20
	lda $02
	jmp $8048
	
org $c08101			; These patches adjusts the music loading to wrap to next bank
	bmi nextbank	; at 0x8000
org $c08104		
	bmi nextbank
	
org $c08107
nextbank:
	
org $c0810d			; Start at $0000 in the new bank and not $8000
	ldy #$0000
;==================================================================================


;=========================== Decompression routines ===============================
org $c0b266							; Modify the bank wrapping routine to detect if
	jmp wrap_bank					; bank >= $c0 and in that case wrap at $8000

									
org $c0b12f							; Modify decompression routines to wrap banks
	jsr check_wrap : nop : nop		; differently depending on what bank decompression§
									; is done from. This needs a lot of optimization.
org $c0b15a
	jsr check_wrap : nop : nop		; Decomp > WRAM

org $c0b189
	jsr check_wrap : nop : nop

org $c0b1a0
	jsr check_wrap : nop : nop

org $c0b1b8
	jsr check_wrap : nop : nop

org $c0b1c9
	jsr check_wrap : nop : nop
	
org $c0b1ed
	jsr check_wrap : nop : nop
	
org $c0b20e
	jsr check_wrap : nop : nop

org $c0b21f
	jsr check_wrap : nop : nop
	
org $c0b250
	jsr check_wrap : nop : nop

org $c0b286							; Decomp > RAM
	jsr check_wrap : nop : nop

org $c0b2b1
	jsr check_wrap : nop : nop
	
org $c0b2e3
	jsr check_wrap : nop : nop

org $c0b309
	jsr check_wrap : nop : nop

org $c0b32f
	jsr check_wrap : nop : nop

org $c0b340
	jsr check_wrap : nop : nop

org $c0b380
	jsr check_wrap : nop : nop

org $c0b3af
	jsr check_wrap : nop : nop

org $c0b3c0
	jsr check_wrap : nop : nop

org $c0b421
	jsr check_wrap : nop : nop

org $c0b123							; Check the starting bank and if it's >= $c0
	jmp modify_address_ram			; then subtract $8000 from the starting address
	
org $c0b27b
	jmp modify_address_vram
;==================================================================================


;================================ Hud fixes ========================================
org $c09b4a
	jmp fix_hud_digits	; Make HUD digits load from bank 80 instead of 00
;==================================================================================


;============================== New hook routines =================================	
org $c0fe00
wrap_bank:
	pha
	phb
	pla
	cmp #$bf		; If bank >= $bf, set X to $0000, else $8000
	bcc +
	ldx #$0000
	jmp $b26a
+
	ldx #$8000
	jmp $b26a

check_wrap:
	pha
	phb
	pla
	cmp #$c0		; If bank >= $c0, check wrap at $8000, else $0000
	bcc +
	cpx #$8000
	bne ++
	jsr $b266
	jmp ++
+
	cpx #$0000
	bne ++
	jsr $b266
	jmp ++
++
	pla
	rts

modify_address_ram:	
	pha
	lda $49
	cmp #$c0		; IF bank >=$c0, set to lower address space
	bcc +
	rep #$20
	lda $47
	and #$7fff
	sta $47
	sep #$20
	
+
	pla				; Restore A and execute hijacked code
	stz $50
	ldy #$0000
	jmp $b128	
	
modify_address_vram:	
	pha
	lda $49
	cmp #$c0		; IF bank >=$c0, set to lower address space
	bcc +
	rep #$20
	lda $47
	and #$7fff
	sta $47
	sep #$20
	
+
	pla				; Restore A and execute hijacked code
	stz $50
	ldy $4c
	jmp $b27f

fix_hud_digits:
	sep #$20
	lda #$80
	sta $02
	rep #$30
	jmp $9b4e
;==================================================================================
