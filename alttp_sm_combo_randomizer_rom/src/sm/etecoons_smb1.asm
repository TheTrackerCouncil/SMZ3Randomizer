; Patches the song the Etecoons sing to be the intro for World 1-1 in Super Mario Bros. 1
; This sound effect is Library 2, Sound 0x35

; From spc.asm: engine starts at $CF8108 vanilla ROM, but $CF0108 in SMZ3

org $CFA510-$8000   ; Overwrite the setup call
    dw $39A8        ; Two voices, high-priority

org $CFABA7-$8000   ; Overwrite the instruction list
    dw $3FA3        ; Melody instruction list
    dw $3FC7        ; Bassline instruction list

; Melody ($24 bytes)

    db $1D,$80,$0A,$AD,$0B
    db $1D,$80,$0A,$AD,$18
    db $1D,$80,$0A,$AD,$18
    db $1D,$80,$0A,$A9,$0B
    db $1D,$80,$0A,$AD,$1B
    db $1D,$80,$0A,$B0,$30
    db $1D,$80,$0A,$A4,$30
    db $FF

; Bassline ($24 bytes)

    db $1D,$70,$0A,$9F,$0B
    db $1D,$70,$0A,$9F,$18
    db $1D,$70,$0A,$9F,$18
    db $1D,$70,$0A,$9F,$0B
    db $1D,$70,$0A,$9F,$18
    db $1D,$70,$0A,$A4,$30
    db $1D,$70,$0A,$98,$30
    db $FF
