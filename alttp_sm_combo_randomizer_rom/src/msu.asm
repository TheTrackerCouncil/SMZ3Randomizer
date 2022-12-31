org $0080D7
JML msu_main : NOP
spc_continue:

msu_main:
    SEP #$20    ; set 8-BIT accumulator
    LDA $4210   ; thing we wrote over
    REP #$20    ; set 16-BIT accumulator
    SEP #$30
    LDA #$20 : CMP #$50 : BEQ .test
.nomsu
    SEP #$30
    JML spc_continue
.test
    SEP #$30
    JMP spc_continue

; SEP #$20 ; set 8-bit accumulator
; LDA $4210 ; thing we wrote over
; REP #$20 ; set 16-bit accumulator

; LDX $012C ; Load current SPC song
; LDA #17
; STA $012C
; SEP #$30
; STA $7e0055
; CPX #$30 : BEQ play_song ; If hyrule field, play song
; ; LDA #5
; ; STA $7e0055
; JML exit_msu

; play_song:
;     SEP #$30
;     ; LDA #4
;     ; STA $7e0055
;     LDX #2
;     STX $2004 ; Set MSU Song
;     ;STZ $2007
;     ;LDY #17
;     ;STY $012C
;     ;LDY #1
;     ;STY $004F
;     ;SEP #$30
;    ; LDX #2
;    ; STX $2004 ; Set MSU Song
;    ; STZ $2007 ; Clear MSU Control to start playing
;     ;BRL exit_msu
;     ;SEP #$30
;     rti

; exit_msu: 
; SEP #$30
; NOP
