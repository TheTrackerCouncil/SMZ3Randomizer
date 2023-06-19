!MSUCodeSM = $B210

org $CF0108+($1799-$1500) ; SPC 1799 (engine starts at 1500), engine starts at $CF8108 vanilla ROM, but $CF0108 in SMZ3
	db $5f
    dw !MSUCodeSM 

;---------------------------------------------------------------------------------------------------
pushpc
org $D01439 ; Unused sample data
SM_SPC_ExtraCode:
arch spc700
base !MSUCodeSM
SM_SpecialCommand_Mute:
	CMP A,#$F0      ; The thing we overwrote
	BNE +
		JMP $1750   ; SilenceSong
	+
	CMP A,#$FA      ; New mute command $FA
	BNE +

		; $1E15: CF        mul   ya           ;} Track output volume multiplier = ([A] * [music volume multiplier] * [track note volume multiplier] * [track volume multiplier])Â²
		; $1E16: DD        mov   a,y          ;|
		; $1E17: CF        mul   ya           ;|

		MOV $F4,A
		MOV A,#$E8
		MOV $1E15,A 
        MOV A,#$00
		MOV $1E16,A  ; mul ya, mov a,y -> mov a, #$00

		BRA +++
	+

	CMP A,#$FB      ; New unmute command $FB
	BEQ +
		JMP $179D   ; NewSongInput
	+

	MOV $F4,A
    MOV A, #$CF
    MOV $1E15,A
    MOV A, #$DD
    MOV $1E16,A

	+++
	CALL $1750      ; SilenceSong
-	MOV A,$F4
		BNE -
		CMP A,$F4
		BNE -
	MOV $F4,$00
	RET
warnpc $B515
arch 65816
pullpc