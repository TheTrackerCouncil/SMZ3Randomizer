; Fix bomb torizo awakening
; Grey door
org $c4ba6f
    lda.w $1C83 : bne +
    iny : iny : rts

+
    lda.w $0000,y : tay : rts

; Statue
org $c4d33b
    lda.w $1C83 : bne +
    lda.w #$0001 : sta.l $7EDE1C,x
    inc.w $1D27,x : inc.w $1D27,x
    lda.w #$D356 : sta.w $1CD7,x

+
    rts

; Door close timer
org $c4ba54
    dw $28
