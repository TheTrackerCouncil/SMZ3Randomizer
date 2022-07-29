; Title screen entry point: $0C:C100
; jump directly to $C2B6 for file select menu
org $0cc100
jmp $c2b6

; This is an address in a jump table that stumbles 
; through a bunch of drawing and animation for the file
; select screen. This just copies the last address to
; the first index, so it doesn't bother with any of the
; presentation and just jumps straight to the logic.
org $0ccc7a
dl $0CCDC6

; file select screen
org $0cce2a
jmp $ce5c ; jump straight to "action" button handler, loading the save
