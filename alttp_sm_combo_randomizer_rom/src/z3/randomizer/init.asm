;--------------------------------------------------------------------------------
; Init_Primary
;--------------------------------------------------------------------------------
; This can be as inefficient as we want. Interrupts are off when this gets
; called and it only gets called once ever during RESET.
;--------------------------------------------------------------------------------
Init_Primary:
	LDA #$00
	
	LDX #$00 ; initalize our ram
	-
		STA $7EC025, X
		STA $7F5000, X
		INX
		CPX #$10 : !BLT -

	LDX #$10 ; initalize more ram
	-
		STA $7F5000, X
		INX
		CPX #$FF : !BLT -
	
	LDX #$00
	-
		LDA !SRAM_ALTTP_CHECKSUM, X : CMP $00FFC0, X : BNE .clear
		INX
		CPX #$15 : !BLT -
	BRA .done
	.clear
		REP #$30 ; set 16-bit accumulator & index registers
		LDA.w #$0000
		-
			STA $a06000, X
			INX
			CPX #$2000 : !BLT -
		SEP #$30 ; set 8-bit accumulator & index registers
		LDX #$00
		-
			LDA $00FFC0, X : STA !SRAM_ALTTP_CHECKSUM, X
			INX
			CPX #$15 : !BLT -
	.done
	
	LDA.b #$01 : STA $420D ; enable fastrom access on upper banks
	
	LDA.b #$10 : STA $BC ; set default player sprite bank
	
	LDA.b #$81 : STA $4200 ; thing we wrote over, turn on NMI & gamepad
RTL
;--------------------------------------------------------------------------------