lorom

;########################
;####   Easier Wall Jump   ###
;####################### by Benox50

; Can press jump just before the direction to walljump
; By default 5 frames window
; So player can basically press both jump and the direction at the same time and human error will be forgiven
; Other edits to also make WallJump works better, like not needing to hold the inverse direction and less collision re-check
; Routine also end-up smaller :]

!RAM_WJ_Lee = $09D8 ;*FREE RAM*, you should probably change this...    WallJump leeway/timer to press jump
!WJ_Lee = #$0005 ;How much frames to input a jump before the direction to perform a wall jump





;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;;;; Wall jump check ;;;;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;
;Easier WallJump
{
org $909D35
WJ:
	LDA $0A27 : AND #$00FF
	CMP #$0003 : BEQ +
	CMP #$0014 : BNE WJ_End
+
	LDA $0A1C
	CMP #$0081 : BEQ +
	CMP #$0082 : BEQ +
	LDA $0A96 : CMP #$000B : BMI ++
	JMP WJ_Ok
+
	LDA $0A96
	CMP #$001B : BMI ++
	JMP WJ_Ok
++
	LDA !RAM_WJ_Lee : BEQ +
	DEC !RAM_WJ_Lee : BPL ++
+
print " WJ leeway press ", pc
	LDA $8F : BIT $09B4 : BEQ ++
	LDA !WJ_Lee : STA !RAM_WJ_Lee
++
	LDA $8B
	BIT #$0200 : BNE WJ_SpinL
	BIT #$0100 : BNE WJ_SpinR
WJ_End:
	CLC : RTS
WJ_SpinL:
	JSR WJ_CheckColL : BCC WJ_End
	BRA WJ_Col
WJ_SpinR:
	JSR WJ_CheckColR : BCC WJ_End
WJ_Col:
	TDC : INC : STA $0A94
	LDA $0A1C
	CMP #$0081 : BEQ +
	CMP #$0082 : BEQ +
	LDA #$000A : STA $0A96
	BRA WJ_End
+
	LDA #$001A : STA $0A96
WJ_End2:
	CLC : RTS

WJ_Ok:
	TDC : DEC : STA $0E1C
	LDA !RAM_WJ_Lee : BNE + ;Player has already pressed jump before turn around?
	LDA $8F : BIT $09B4 : BEQ WJ_End2
+
	LDA $0A1E : BIT #$0008 : BNE +
	JSR WJ_CheckColL2 ;A bit succ, but must know which enemy collided with
	BRA ++
+
	JSR WJ_CheckColR2
++
	LDA #$0005 : STA $0DC6
	LDA $16 : BEQ +
	STA $0E1C
+
	SEC : RTS



WJ_CheckColL:
	JSL $94967F : BCS WJ_CheckColL_Y
WJ_CheckColL2:
	TDC : INC : STA $0B02
	JSL $A0A8F0 : TAX : BEQ WJ_CheckColL_N
WJ_CheckColL_Y:
	SEC : RTS
WJ_CheckColL_N:
	STZ $16
	CLC : RTS


WJ_CheckColR:
	LDA $12 : EOR #$FFFF : STA $12
	LDA $14 : EOR #$FFFF : INC : STA $14
	BNE +
	INC $12
+
	JSL $94967F : BCS WJ_CheckColR_Y
WJ_CheckColR2:
	STZ $0B02
	JSL $A0A8F0 : TAX : BEQ WJ_CheckColR_N
WJ_CheckColR_Y:
	SEC : RTS
WJ_CheckColR_N:
	STZ $16
	CLC : RTS
}




;*** Also include fix for screw contact dmg not triggering during wall kick pose, can remove this next block of code if you dont want it

;;;;;;;;;;;;;;;;;;;
;;; Fix screw wall jump contact ;;;
;;;;;;;;;;;;;;;;;;;;;;
;;; Samus movement - wall jumping ;;;
org $90A734
{
WJ_Screw:
; If screw attack equipped, set screw attack contact damage
	LDA $09A2 : BIT #$0008 : BEQ WJ_Screw_No
	LDA #$0003 : STA $0A6E
	BRA WJ_Screw_Yes

WJ_Screw_No:
	LDA $0CD0 : CMP #$003C : BMI WJ_Screw_Yes
	LDA #$0004 : STA $0A6E

WJ_Screw_Yes:
	JMP $8FB3 ; Samus jumping movement


; INFO ;
{
;$0A94 anim delay 
;$0A96 anim frame

; Handle anim delay: $90:8062 20 DC 82    JSR $82DC
;90:831E for storing anim delay was hijacked by morph roll patch

;wall jump is based on $0A96 being B
;909D55 
;90:9E2E

;90:9D35 when can walljump routine it seems

;90/9E82 store 5 to $0DC6 for $91:EABE

;$91:EABE trigger when wall kick, $0DC6 was indexed at 5
}

}



