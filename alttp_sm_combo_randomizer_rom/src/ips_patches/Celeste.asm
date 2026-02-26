lorom

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;; By Benox50
;;;;; Snap to hole;;;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;; Move samus to nearby hole space or air, Celeste style, if its detected as close enough
; Help player to not miss morph passages like it can sometimes happen in vanilla
; Help also to jump into a 1x1 hole or jump into clearance





;;;;;;;;;;;;;;;;;;;; Solid collision reaction ;;;;;;;;;;;;;;;; *Rewrite to be smaller cause why not
;;;;;;;;;;;;;;;;;;;;;;;;;; Flag and clear if samus is on a slope in RAM, Usage for prevent annoying falling down slope when aiming change n others usages
{
;Block collision reaction - horizontal - solid/shootable/grapple block 
org $948F49
BTS_ColHor:
	STZ $14
	;STZ $1E77
	LDA $20 : BIT $12 : BMI BTS_ColHor_Left 
BTS_ColHor_Right:	
	AND #$FFF0
	SEC : SBC $0AFE : SBC $0AF6 : BPL +
	TDC
+	
	STA $12
	TDC : DEC : STA $0AF8
	BRA BTS_ColHor2
BTS_ColHor_Left:
	ORA #$000F
	SEC : ADC $0AFE
	SEC : SBC $0AF6 : BMI +
	TDC
+
	STA $12
	STZ $0AF8
BTS_ColHor2:
	JMP AirSnap_H
;Block collision reaction - Vertical - solid/shootable/grapple block 
org $948F82
BTS_ColVer:
	STZ $14
	;STZ $1E77
	LDA $20 : BIT $12 : BMI BTS_ColVer_Up 
BTS_ColVer_Down:
	AND #$FFF0
	SEC : SBC $0B00 : SBC $0AFA : BPL +
	TDC 
+
	STA $12
	TDC : DEC : STA $0AFC
	SEC : RTS

BTS_ColVer_Up:
	ORA #$000F
	SEC : ADC $0B00
	SEC : SBC $0AFA : BMI +
	TDC
+
	STA $12
	STZ $0AFC
	JMP AirSnap_V
}





;;; Now the actual code ;;;
;;; FREE SPACE ;;;
org $94DC00

{
;INFO
; Only one block collide check, topmost block of samus radius takes priority, in $1A n $1C
; $0DC4 is our savior in this, contains which block is collided with
; $4202 collided block PosY (but cant read :d)
; Samus in first block of clearance with her hitbox colliding solid means a snap radius of 1/4-ish tile (6 pixels approx.)

;For horizontal collision, only do 1x1 holes morphed
AirSnap_H:
print " AirSnap_H: ", pc
;Buncha checks
	LDA $0DFC : BNE AirSnap_H_End ;Ascent pusher blocks flag
	LDA $0B00 : CMP #$0007 : BNE AirSnap_H_End ;Morphed?
	TDC : TAX : LDY #$0004
	JSR AirSnap_Cal : BCS +
;Hole to check is above
	LDA $0DC4 : SEC : SBC $07A5
	STA $16
	JSR AirSnap_Get : BCC AirSnap_H_End ;Check block
	LDA $16 : SEC : SBC $07A5
	JSR AirSnap_Get : BCS AirSnap_H_End ;Check next block too, be sure its 1x1 clearance
	BRA ++
+;Hole to check is under
	LDA $0DC4 : CLC : ADC $07A5
	STA $16
	JSR AirSnap_Get : BCC AirSnap_H_End
	LDA $16 : CLC : ADC $07A5
	JSR AirSnap_Get : BCS AirSnap_H_End
++
	STZ $0B2E : STZ $0B2C ;Reset samus Y speed
	BRA AirSnap_Snap
AirSnap_H_End:
	SEC : RTS



;For vertical collision, accept snap on full clearance (Straight jump, shinespark and morphed...)
AirSnap_V:
print " AirSnap_V: ", pc
;Buncha checks
	LDA $0DFC : BNE AirSnap_V_End ;Ascent pusher blocks flag
	LDA $0B36 : DEC : BNE AirSnap_V_End ;Only accept collision when samus going-up
	LDX #$0002 : TDC : TAY
	JSR AirSnap_Cal : BCS +
;Clearance is left
	LDA $0DC4 : DEC
	BRA ++
+;Clearance is right
	LDA $0DC4 : INC
++
	JSR AirSnap_Get : BCC AirSnap_V_End ;Check block
;Snap samus pos to block center based on [Y]
AirSnap_Snap:
	LDA $0AF6,y
	AND #$FFF0 : ORA #$0008 ;Block center
	STA $0AF6,y
	CLC : RTS

AirSnap_V_End:
	SEC : RTS




;;; Calculate block n decide snap direction, [X] = collided block PosX or Y, [Y] = samus PosX or Y
AirSnap_Cal:
;Convert collided nth block of room to his PosXY
	LDA $0DC4 : STA $4204
	SEP #$20
	LDA $07A5 : STA $4206
	REP #$20
;Samus block PosY must be either above or under the block (on sides for PosX) in collision to her hitbox/radius
	LDA $0AF6,y : LSR #4 : AND #$00FF    ;Conv to block
	CMP $4214,x : BNE +
	PLA : BRA ++ ;Fully getout!
+; Use samus block pos vs her pixel pos, to know the snap direction
	LDA $0AF6,y : AND #$FFF0 : ORA #$0008 ;Find which direction from samus block center
	CMP $0AF6,y : BPL ++
	CLC : RTS
++
	SEC : RTS



;;; Get n check block type is air for clearance, [A] = block to get in nth block of room
AirSnap_Get:
	ASL : TAX
	LDA $7F0002,x
	AND #$F000 : XBA : LSR #3 : TAX ;Conv to block type index
	LDA $94D5,x : CMP #$8F47 : BEQ + ;Air?
	CLC : RTS
+
	SEC : RTS
}

