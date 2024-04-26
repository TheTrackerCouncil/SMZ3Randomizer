; Save Mother Brain's HP when the Hyper Beam turns on
; so we can calculate how many shots it took to finish her,
; without having to count them in realtime.

; Hijack part of the subroutine that enables Hyper Beam.
; We're in 16-bit mode here.
org $91E607 ; STZ $0DC0
    jmp $FFEE

org $91FFEE ; free space
    stz $0DC0 ; Do the operation we overwrote
    lda.w $0FCC ; Enemy 1's health
    sta.w $033E
    jmp $E60A ; CLC
