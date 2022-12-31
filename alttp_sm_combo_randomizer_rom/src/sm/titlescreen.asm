; Spritemap format is roughly:
;     nnnn         ; Number of entries (2 bytes)
;     xxxx yy attt ; Entry 0 (5 bytes)
;     ...          ; Entry 1...
; Where:
;     n = number of entries
;     x = X offset of sprite from centre
;     y = Y offset of sprite from centre
;     a = attributes
;     t = tile number

; More specifically, a spritemap entry is:
;     s000000xxxxxxxxx yyyyyyyy YXpp000ttttttttt
; Where:
;     s = size bit
;     x = X offset of sprite from centre
;     y = Y offset of sprite from centre
;     Y = Y flip
;     X = X flip
;     p = priority (relative to background)
;     t = tile number

; NB Must be located in bank $CC
org $CCFA00
titlescreen_map:
    incbin "../data/titlescreen-map.bin"

org $CBA0C7
    dw titlescreen_map

org $CBA0CD
    dw titlescreen_map

org $CB9C5F
    JSL titlescreen_upload_gfx
    NOP

; Offset 0x00D000, free space in SM
org $C0D200
titlescreen_gfx:
    incbin "../data/titlescreen-gfx.bin"
titlescreen_end:

macro gfx_upload_loop(end, target)
?loop:
    CPX <end> : BEQ ?done
    DEX : DEX
    LDA.L titlescreen_gfx,X
    STA.L <target>,X
    BRA ?loop
?done:
endmacro

titlescreen_upload_gfx:
    PHP : REP #$30
        LDX.W #$16C0 ; LDX #titlescreen_end-titlescreen_gfx
        %gfx_upload_loop(#$15E0, $7F7020)
        %gfx_upload_loop(#$1500, $7F6F00)
        %gfx_upload_loop(#$1420, $7F6DE0)
        %gfx_upload_loop(#$1340, $7F6CC0)
        %gfx_upload_loop(#$1280, $7F6B80)
        %gfx_upload_loop(#$0BE0, $7F6A40)
        %gfx_upload_loop(#$0A00, $7F6A20)
        %gfx_upload_loop(#$0840, $7F6A00)
        %gfx_upload_loop(#$0680, $7F69C0)
        %gfx_upload_loop(#$0200, $7F6980)
        %gfx_upload_loop(#$0180, $7F6800)
        %gfx_upload_loop(#$0100, $7F6680)
        %gfx_upload_loop(#$0080, $7F6500)
        %gfx_upload_loop(#$0040, $7F6380)
        .loop
            DEX : BMI .done : DEX
            LDA.L titlescreen_gfx,X
            STA.L $7F61C0,X
            BRA .loop
        .done
    PLP
    LDA #$02 : STA $420B  ; start DMA transfer, the code we wrote over
    RTL


; This is the index of the cursor on the file select screen.
; 0 is the first save slot, 4 is "clear data", 5 is "exit"
!smFileSelectCursorIndex = $0952
; This is a bitfield indictating which save slots hold data.
; The LSB represents the first slot.
!smSaveFilePresenceBits = $0954

; SM file select screen: up button handler
org  $c1a25e
base $81a25e
{
  ; if cursor is at index 0 then set it to 5
  lda !smFileSelectCursorIndex
  bne +
  lda #$0005
  bra smFileSelectControllerHackDone
  
+ ; if cursor is at index 4 then set it to 0
  cmp #$0004
  bne +
  lda #$0000
  bra smFileSelectControllerHackDone
  
+ ; cursor is at index 5, so if a save is present
  ; then go to 4, otherwise go to 0
  lda !smSaveFilePresenceBits
  asl a ; since only the first slot can hold data, we can 
  asl a ; just mul by 4 to get the desired index value
  bra smFileSelectControllerHackDone
}

; SM file select screen: down button handler
org  $c1a286
base $81a286
{
  ; if cursor is at index 0 then go to 4 if a save is
  ; present, otherwise go to 5
  lda !smFileSelectCursorIndex
  bne +
  lda !smSaveFilePresenceBits
  ; the save bits will be zero if there's a save and 1 if not
  ; so we can xor that by 5 to get the desired value
  eor #$0005
  bra smFileSelectControllerHackDone
  
+ ; if cursor is at 4 then set it to 5
  cmp #$0004
  bne +
  lda #$0005
  bra smFileSelectControllerHackDone
  
+ ; if cursor is at 5 then set it to 0
  lda #$0000
  bra smFileSelectControllerHackDone
}

; At this point the routine copies A into the cursor index
; memory location and then continues processing the logic
org  $c1a2af
base $81a2af
smFileSelectControllerHackDone:

; This is the point during the scene drawing just before
; the second save slot is checked and its state drawn.
; Instead of doing that, we'll skip past the second and
; third slots. But before jumping we apply the skipped changes
; to the file presence bits so that they indicate that those
; slots are empty.
org  $c19f46
base $819f46
sec
ror !smSaveFilePresenceBits
ror !smSaveFilePresenceBits
jmp $9fb2

; This simply skips the rendering of the "DATA COPY" text.
org  $c19fc6
base $819fc6
jmp $9fd2

; This is during the routine for the fade-in substate (3). This is
; where the second helmet is about ot be drawn. There's no logic past
; the helmet drawing here, so we can just rts.
org  $c19dcf
base $819dcf
rts

; This is during the primary logic routine (substate 4) for the file
; select screen. This is just before the second helmet is drawn. We'll
; jump past the second and third helmets and continue from that point.
org  $c1a1d2
base $81a1d2
jmp $a1de

; This is during the transition from the file select screen to the
; save config screen (this transition is substate 1F). This skips
; over drawing the second and third helmets.
org  $c19d85
base $819d85
jmp $9d91

















