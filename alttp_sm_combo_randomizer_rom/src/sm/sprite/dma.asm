; --- swap DMA order ---

; Upper tilemaps were loaded before lower, but rendered after. The order is
; swapped to avoid larger DMAs overwriting later ones. Swap the commands in
; $80:9382-93CA with $80:93CB-9413, but most are identical so just change these:

org $C09382+1 : db $1E
org $C09389+1 : db $21
org $C0938E+1 : db $80
org $C093B6+1 : db $80
org $C093CB+1 : db $1D
org $C093D2+1 : db $1F
org $C093D7+1 : db $00
org $C093FF+1 : db $00


; --- disable upper bypass ---

; Since a DMA of zero usually results in a game crash by DMA-ing an entire bank,
; Deerforce made separate subroutines that skipped upper tilemaps in certain cases.
; This is fixed by the DMA order swap, but the subroutines now break a few
; animations in their new form. However, there is not a simple way to avoid DMA,
; you have to use a bypass routine, so the null routine is reused for all cases.

org $D0864E
    rep 28 : dw $8686


; --- hide suit light tile ---

; This tile hid the green light on the right side of Samus suit. It is now
; superfluous so we solve that by making it transparent.

org $DAD620
    rep $20 : db $00


; --- death DMA asm ---

; During the death sequence data is prefetched while Samus is pausing (for
; dramatic effect), so we just prefetch more. The very last one is not
; prefetched because it overwrites the bonk pose
;$9B:B44A    CMP #$0004

org $DBB44A
base $9BB44A
    CMP #$000F


; The final fetch overwrites Samus bonk pose, so it is indexed at the last moment
;$9B:B5AE    LDY #$0008

org $DBB5AE
base $9BB5AE
    LDY #$001E


; Use the correct bank
;$9B:B6EE    LDA #$9B    ;use bank 9B

org $DBB6EE
base $9BB6EE
    LDA #$5A


org $DBFDA0
base $9BFDA0

death_left_table:
    dw $0000+(1*$400)
    dw $0000+(2*$400)
    dw $0000+(3*$400)
    dw $0000+(4*$400)
    dw $0000+(5*$400)
    dw $0000+(6*$400)
    dw $0000+(7*$400)
    dw $0000+(8*$400)
    dw $0000+(9*$400)
    dw $0000+(10*$400)
    dw $0000+(11*$400)
    dw $0000+(12*$400)
    dw $0000+(13*$400)
    dw $0000+(14*$400)
    dw $0000+(15*$400)
    dw $0000

death_right_table:
    dw $4000+(1*$400)
    dw $4000+(2*$400)
    dw $4000+(3*$400)
    dw $4000+(4*$400)
    dw $4000+(5*$400)
    dw $4000+(6*$400)
    dw $4000+(7*$400)
    dw $4000+(8*$400)
    dw $4000+(9*$400)
    dw $4000+(10*$400)
    dw $4000+(11*$400)
    dw $4000+(12*$400)
    dw $4000+(13*$400)
    dw $4000+(14*$400)
    dw $4000+(15*$400)
    dw $4000

dest_table:
    dw $6000+(1*$200)
    dw $6000+(2*$200)
    dw $6000+(3*$200)
    dw $6000+(4*$200)
    dw $6000+(5*$200)
    dw $6000+(6*$200)
    dw $6000+(7*$200)
    dw $6000+(8*$200)
    dw $6000+(9*$200)
    dw $6000+(10*$200)
    dw $6000+(11*$200)
    dw $6000+(12*$200)
    dw $6000+(13*$200)
    dw $6000+(14*$200)
    dw $6000+(15*$200)
    dw $6000

death_routine:       ; New code to load different data based upon left or right facing
    LDA $0A1E        ; load the direction that Samus is facing
    BIT #$0008       ; right facing?
    BNE .right
    LDA death_left_table,y
    RTS

.right
    LDA death_right_table,y
    RTS


; Hook the new pointers
;$9B:B6F5    LDA $B7C9,y    ;destination of DMA data transfer during the death sequence

org $DBB6F5
base $9BB6F5
    LDA dest_table,y


; Hook the new subroutine
;$9B:B6E5    LDA $B7BF,y    ;get the DMA relative pointers

org $DBB6E5
base $9BB6E5
    JSR death_routine
