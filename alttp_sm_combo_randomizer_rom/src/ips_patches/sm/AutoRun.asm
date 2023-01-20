; Auto Run 
; Automatically has Samus run without holding down the run button

lorom

org $909781
  JSR AutoRun

org $908542
  JSR AutoRun

org $90FF80
AutoRun:
  PHA
  LDA $008B ; Load current player input
  BIT $09B6 : BEQ + ; Check if player is hitting run button
    PLA
    LDA #$0000 : RTS ; Set the player to not increase speed
  +
  PLA
  RTS
