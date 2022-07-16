;
; Code for injecting PLM's into rooms at runtime.
; This is done to minimize the amount of big room edits needed where a lot of room header information would need to be shuffled around otherwise.
;

; Hijack room loading to be able to inject arbitrary PLM:s into a room
org $c2e8d5
    jsl inject_plms
org $c2eb8b
    jsl inject_plms

org $cff700
base $8ff700
inject_plms:
    ldx #$0000

-
    ; Check if the PLM goes in this room, if the table is $0000 then exit
    lda plm_table, x
    beq .end

    cmp $079b
    bne .next

    ; Ok, Spawn the PLM
    phx 
    txa
    clc
    adc #plm_table+$2
    tax
    lda $0000, x
    jsl $84846a
    plx

.next
    txa
    clc
    adc #$0008
    tax
    bra -

.end
    jsl $8FE8A3  ; Execute door ASM
    rtl

org $cff800
base $8ff800
plm_table:
;   room,   plm,        yyxx,  args
dw $0000, $0000,        $0000, $0000       ; End of table