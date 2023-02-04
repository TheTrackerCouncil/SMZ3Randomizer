; ALTTP Multiworld support
;

org $f81000

; Lookup item in ALTTP table and replace as needed, return as 8-bit A
alttp_multiworld_replace_item:
    lsr !MULTIWORLD_SWAP : bcs .exit      ; If !MULTIWORLD_SWAP is set, skip one item swap
    CMP.b #$20 : BEQ .exit                ; Crystal
	CMP.b #$26 : BEQ .exit                ; Piece of heart completion heart
	CMP.b #$2E : BEQ .exit                ; red refill
	CMP.b #$2F : BEQ .exit                ; green refill
	CMP.b #$30 : BEQ .exit                ; Blue refill
    CMP.b #$32 : BEQ .exit                ; Big key (guard)
	CMP.b #$37 : BEQ .exit                ; Pendant
	CMP.b #$38 : BEQ .exit                ; Pendant
	CMP.b #$39 : BEQ .exit                ; Pendant
    bra .next
.exit
    jmp .end
.next
    phx : phy : php
    %ai16()
    tax

    lda $10
    and #$00ff
    cmp #$0007      ; Only check for shops in dungeon mode
    bne .noShop      

    lda $A0        
    cmp #$00FF : BEQ .paradoxShop
    cmp #$011C : BEQ .bombShop              ; Bomb shop
    cmp #$010F : BEQ .shop
    cmp #$011F : BEQ .shop
    cmp #$0110 : BEQ .shop
    cmp #$0112 : BEQ .shop
    cmp #$0100 : BEQ .shop              ; LW Chest game 1
    cmp #$0118 : BEQ .shop              ; LW Chest game 2
    bra .noShop
.paradoxShop
    lda $A8
    cmp #$0014 : BEQ .shop
    bra .noShop
.bombShop
    lda $A9
    cmp #$0200 : beq .shop
    bra .noShop
.shop
    txa : bra .noReplace
.noShop
    txa
    and #$00ff                          ; Mask off any extra high bits left in A
    asl #3 : tax
    ora #$8000                          ; Set high bit in item index for ALTTP to not clash with SM indexes
    sta !MULTIWORLD_GIVE_INDEX          ; Save the multiworld table index for later
    lda.l alttp_rando_item_table, x     ; Load multiworld item type
    sta !MULTIWORLD_PICKUP              ; Make sure we always set this flag so it's updated depending on item type
    beq .ownItem

    sta !MULTIWORLD_DIALOG
    lda.l alttp_rando_item_table+$4, x
    sta !MULTIWORLD_GIVE_PLAYER        ; Store multiworld owner
    sta !MULTIWORLD_DIALOG_PLAYER    
.ownItem
    lda.l alttp_rando_item_table+$2, x
    sta !MULTIWORLD_GIVE_ITEM
    sta !MULTIWORLD_DIALOG_ITEM
.noReplace
    plp : ply : plx
.end
    stz !MULTIWORLD_SWAP
    rtl

alttp_multiworld_replace_graphics:
    lsr !MULTIWORLD_SWAP : bcs .end     ; If !MULTIWORLD_SWAP is set, skip one item swap
    CMP.b #$20 : BEQ .end                ; Crystal
	CMP.b #$26 : BEQ .end                ; Piece of heart completion heart
	CMP.b #$2E : BEQ .end                ; red refill
	CMP.b #$2F : BEQ .end                ; green refill
	CMP.b #$30 : BEQ .end                ; Blue refill
    CMP.b #$32 : BEQ .end                ; Big key (guard)
	CMP.b #$37 : BEQ .end                ; Pendant
	CMP.b #$38 : BEQ .end                ; Pendant
	CMP.b #$39 : BEQ .end                ; Pendant

    phx : phy : php
    %ai16()
    and #$00ff              ; Mask off any extra high bits left in A
    asl #3 : tax
    lda.l alttp_rando_item_table+$2, x
    plp : ply : plx

.end
    stz !MULTIWORLD_SWAP
    rtl

alttp_mw_handle_queue:
    pha : phx : phy : php
    %ai16()
    
    lda config_multiworld
    bne +
    jmp .end
+

;    lda.l !SRAM_MW_RPTR      ; Don't loop in ALTTP because we have to wait for item pickup to actually start
;    cmp.l !SRAM_MW_WPTR
    lda.l !SRAM_MW_ITEMS_RECV_RPTR
    cmp.l !SRAM_MW_ITEMS_RECV_WPTR    
    beq .end

    %a8()
    lda.l !MULTIWORLD_PICKUP
    bne .cantGive                 ; If a pickup is happening, don't queue another yet
    lda $02E4
    bne .cantGive                 ; Don't queue pickups if link is not allowed to move
    lda $03EF
    bne .cantGive                 ; Don't queue pickups when link is forced into "sword up"

    lda $5d                       ; Check links state
    cmp #$00                      ; Only allow ground state, swimming, dashing and bunny states
    beq .next
    cmp #$04
    beq .next
    cmp #$11
    beq .next
    cmp #$17
    beq .next
    bra .cantGive

.next

    lda $10                       ; Only get items if we're still in dungeon or overworld mode
    cmp #$07
    beq .continue
    cmp #$09
    beq .continue
    bra .cantGive

.continue                         ; Always induce a 10 frame waiting period of "normal gameplay"
    lda $11
    bne .cantGive                 ; Make sure submodule is 0
    lda !MULTIWORLD_DELAY
    beq .cantGive
    dec : sta !MULTIWORLD_DELAY
    cmp #$00
    beq +
    bra .end
+
    %a16()

;    lda.l !SRAM_MW_RPTR
    lda.l !SRAM_MW_ITEMS_RECV_RPTR
    asl #2 : tax
;    lda.l !SRAM_MW_RECVQ, x : sta $7e
;    lda.l !SRAM_MW_RECVQ+$2, x : sta $7c
    lda.l !SRAM_MW_ITEMS_RECV, x : sta $7e
    lda.l !SRAM_MW_ITEMS_RECV+$2, x : sta $7c
    jsr alttp_mw_receive_item

    lda.l !SRAM_MW_ITEMS_RECV_RPTR
    inc a
    sta.l !SRAM_MW_ITEMS_RECV_RPTR
    bra .end

.cantGive
    lda #$0A
    sta !MULTIWORLD_DELAY   ; Wait 10 frames before trying to give item again
.end
    plp : ply : plx : pla
    jsl $078000              ; JSL Player_Main (do stoops lonk things)
    rtl

alttp_mw_no_rupees:
    pha
    lda config_multiworld
    beq +
    lda !MULTIWORLD_PICKUP
    cmp #$01
    beq .end
+
    pla
    jsl GiveRupeeGift
    rtl
.end 
    pla
    rtl

alttp_mw_receive_item:
    ; Item in $7c, World in $7e
    lda $7e
    sta.l !MULTIWORLD_DIALOG_PLAYER
    lda #$0002
    sta.l !MULTIWORLD_PICKUP  ; 1 = pickup multiworld for other player, 2 = get multiworld item from other player
    sta.l !MULTIWORLD_DIALOG  ; dialog 2 = we got item from other player
    %ai8()
    lda #$01
    sta.l !MULTIWORLD_SWAP
    ldy $7c
    sty !MULTIWORLD_DIALOG_ITEM
    jsl Link_ReceiveItem    ; Get item in $7c
    %ai16()
    rts

alttp_mw_send_item:
    phx : phy : php
    ; Item in A
    %ai16()
    lda.l !MULTIWORLD_GIVE_ITEM        ; This gets set by the replacement routine
    and #$00ff                         ; so that progressive items will send the progressive id correctly
    tax
    lda.l !MULTIWORLD_GIVE_PLAYER
    tay
    lda.l !MULTIWORLD_GIVE_INDEX
    jsl mw_write_message
    plp : ply : plx
    rtl

alttp_multiworld_dialog:
    lda $1cf0
    bmi .multiworldDialog
    ; If not multiworld dialog, return
    asl : tax
    jml $0eee8d

.multiworldDialog

    ; Item Name
    lda !MULTIWORLD_DIALOG_ITEM
    and #$00ff
    cmp #$00b0
    bcc .alttpItem
    sec
    sbc #$00b0
    bra +
.alttpItem
    clc
    adc #$0030
+

    %a8()
    sta $211b : xba : sta $211b
    lda #$14
    sta $211c
    %ai16()

    ldx $2134
    ldy #$0000
    phx : tyx : lda #$0074 : sta $7f1200, x : plx : iny
-    
    phx
    lda.l alttp_mw_item_names, x
    and #$00ff
    beq ++
    tax
    lda.l alttp_dialog_char_table-$20, x
    and #$00ff
    xba
    tyx
    sta.l $7f1200, x
    iny #2
    plx
    inx
    bra -    

++
    plx
    phx : tyx : lda #$0075 : sta $7f1200, x : plx : iny

    lda $1cf0
    and #$00ff
    cmp #$0000
    beq +
    clc
    adc #$0013
+
    tax

-
    phx
    lda.l alttp_mw_dialogtable, x
    and #$00ff
    beq ++
    tax
    lda.l alttp_dialog_char_table-$20, x
    and #$00ff
    xba
    tyx
    sta.l $7f1200, x
    iny #2
    plx
    inx
    bra -

++
    plx
    phx : tyx : lda #$0076 : sta $7f1200, x : plx : iny

    lda #$ff00
    tyx
    sta.l $7f1200, x
    sta.l $7f1200+$2, x
    sta.l $7f1200+$4, x
    tya : clc : adc #$0006 : tay

    lda !MULTIWORLD_DIALOG_PLAYER
    and #$00ff
    asl #4 : tax
    
-    
    phx
    lda.l rando_player_table, x
    and #$00ff
    cmp #$0000                  ; FIXME: this will break if we use extended player data (needs to be fixed by then)
    beq ++
    tax
    lda.l alttp_dialog_char_table-$20, x
    and #$00ff
    xba
    tyx
    sta.l $7f1200, x
    iny #2
    plx
    inx
    bra -
++
    plx
    tyx
    lda #$FF00
    sta.l $7f1200, x
    sta.l $7f1200+$2, x
    sta.l $7f1200+$4, x
    sta.l $7f1200+$6, x
    tya : clc : adc #$0008 : tay

    stz !MULTIWORLD_DIALOG
    stz !MULTIWORLD_DIALOG_ITEM
    stz !MULTIWORLD_DIALOG_PLAYER

    tyx
    jml $0eef1c     ; Return back to original code


alttp_mw_dialogtable:
    ; Dialog boxes are 19 characters wide
    ;   0123456789ABCDEF012
.give
    db " found for player  ", $00
.get
    db "   received from   ", $00

alttp_mw_item_names:
    db "   Grappling Beam  ", $00             ; 00 (b0) (sm items)
    db "    X-Ray Scope    ", $00
    db "     Varia Suit    ", $00
    db "    Spring Ball    ", $00
    db "   Morphing Ball   ", $00
    db "    Screw Attack   ", $00
    db "    Gravity Suit   ", $00
    db "   Hi-Jump Boots   ", $00
    db "     Space Jump    ", $00
    db "        Bomb       ", $00
    db "   Speed Booster   ", $00
    db "    Charge Beam    ", $00
    db "      Ice Beam     ", $00
    db "      Wave Beam    ", $00
    db " ~~ S P a z E R ~~ ", $00
    db "    Plasma Beam    ", $00
    db "    Energy Tank    ", $00
    db "    Reserve Tank   ", $00
    db "      Missiles     ", $00
    db "   Super Missiles  ", $00
    db "    Power Bombs    ", $00

    db "                   ", $00  ;15
    db "                   ", $00  ;16
    db "                   ", $00  ;17
    db "                   ", $00  ;18
    db "                   ", $00  ;19
    db "                   ", $00  ;1A
    db "                   ", $00  ;1B
    db "                   ", $00  ;1C
    db "                   ", $00  ;1D
    db "                   ", $00  ;1E
    db "                   ", $00  ;1F
 
    db " Crateria L 1 Card ", $00 ; 20
    db " Crateria L 2 Card ", $00 ; 21
    db " Crateria Boss Card", $00 ; 22
    db " Brinstar L 1 Card ", $00 ; 20
    db " Brinstar L 2 Card ", $00 ; 21
    db " Brinstar Boss Card", $00 ; 22
    db " Norfair L 1 Card  ", $00 ; 20
    db " Norfair L 2 Card  ", $00 ; 21
    db " Norfair Boss Card ", $00 ; 22
    db " Maridia L 1 Card  ", $00 ; 20
    db " Maridia L 2 Card  ", $00 ; 21
    db " Maridia Boss Card ", $00 ; 22
    db "  W.Ship L 1 Card  ", $00 ; 2C
    db "  W.Ship Boss Card ", $00 ; 2D
    db "L.Norfair L 1 Card ", $00 ; 2E
    db "L.Norfair Boss Card", $00 ; 2F

    db "                   ", $00       ; $30+ (alttp items)
    db "    Master Sword   ", $00
    db "   Tempered Sword  ", $00
    db "     Gold Sword    ", $00
    db "     Toy Shield    ", $00
    db "     Red Shield    ", $00
    db "   Mirror Shield   ", $00
    db "      Fire Rod     ", $00
    db "       Ice Rod     ", $00
    db "       Hammer      ", $00
    db "  Hookshot, BOING! ", $00
    db "        Bow        ", $00
    db "   Blue Boomerang  ", $00
    db "    Magic Powder   ", $00
    db "                   ", $00
    db "       Bombos      ", $00
    db "       Ether       ", $00
    db "       Quake       ", $00
    db "       Lamp        ", $00
    db "       Shovel      ", $00
    db "       Flute       ", $00
    db "  Cane of Somaria  ", $00
    db "       Bottle      ", $00
    db "   Piece of Heart  ", $00
    db "   Cane of Byrna   ", $00
    db "     Magic Cape    ", $00
    db "    Magic Mirror   ", $00
    db "     Power Glove   ", $00
    db "    Titan's Mitt   ", $00
    db "   Book of Mudora  ", $00
    db "  Zora's Flippers  ", $00
    db "     Moon Pearl    ", $00
    db "                   ", $00
    db "      Bug Net      ", $00
    db "     Blue Mail     ", $00
    db "      Red Mail     ", $00
    db "                   ", $00
    db "                   ", $00
    db "  Heart Container  ", $00
    db "      One Bomb     ", $00
    db "     Three Bombs   ", $00
    db "      Mushroom     ", $00
    db "   Red Boomerang   ", $00
    db "     Red Potion    ", $00
    db "    Green Potion   ", $00
    db "     Blue Potion   ", $00
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00
    db "     Ten Bombs     ", $00
    db "                   ", $00
    db "                   ", $00
    db "   A single Rupee  ", $00
    db "     Five Rupees   ", $00
    db "   Twenty Rupees   ", $00
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00
    db "        Bow        ", $00
    db "   Silver Arrows   ", $00
    db "        Bee        ", $00
    db "       Fairy       ", $00
    db "  Heart Container  ", $00
    db "  Heart Container  ", $00
    db "One Hundred Rupees ", $00
    db "    Fifty Rupees   ", $00
    db "                   ", $00
    db "   A single Arrow  ", $00
    db "     Ten Arrows    ", $00
    db "                   ", $00
    db "     300 Rupees    ", $00
    db "   Twenty Rupees   ", $00
    db "    A good Bee     ", $00
    db "  Fighter's Sword  ", $00
    db "                   ", $00
    db "   Pegasus Boots   ", $00
    db "                   ", $00
    db "                   ", $00
    db "     Half Magic    ", $00
    db "   Quarter Magic   ", $00
    db "    Master Sword   ", $00
    db "  5 Bomb Capacity  ", $00
    db "  10 Bomb Capacity ", $00
    db " 5 Arrow Capacity  ", $00
    db " 10 Arrow Capactiy ", $00
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00
    db "   Silver Arrows   ", $00   ; 58
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00
    db "   Sword Upgrade   ", $00
    db "   Shield Upgrade  ", $00

    db "   Armour Upgrade  ", $00
    db "   Glove Upgrade   ", $00
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00
    db "                   ", $00 ; 6F 

    db "                   ", $00  ; 70
    db "                   ", $00
    db " Ganon's Tower Map ", $00
    db "  Turtle Rock Map  ", $00
    db " Thieves' Town Map ", $00
    db " Tower of Hera Map ", $00
    db "  Ice Palace Map   ", $00
    db "  Skull Woods Map  ", $00
    db "  Misery Mire Map  ", $00
    db "  Dark Palace Map  ", $00
    db "  Swamp Palace Map ", $00
    db "                   ", $00
    db " Desert Palace Map ", $00
    db " Eastern Palace Map", $00
    db "                   ", $00
    db " Hyrule Castle Map ", $00  ; 7F

    db "                   ", $00  ; 80
    db "                   ", $00
    db "  G. Tower Compass ", $00
    db "Turtle Rock Compass", $00
    db "   T. Town Compass ", $00
    db "    Hera Compass   ", $00
    db "Ice Palace Compass ", $00
    db "Skull Woods Compass", $00
    db "Misery Mire Compass", $00
    db "Dark Palace Compass", $00
    db "  Swamp P. Compass ", $00
    db "                   ", $00
    db " Desert P. Compass ", $00
    db "Eastern P. Compass ", $00
    db "                   ", $00
    db "                   ", $00  ; 8F

    db "                   ", $00  ; 90
    db "                   ", $00
    db " Ganon's Tower B.K ", $00
    db "  Turtle Rock B.K  ", $00
    db " Thieves' Town B.K ", $00
    db " Tower of Hera B.K ", $00
    db "  Ice Palace B.K   ", $00
    db "  Skull Woods B.K  ", $00
    db "  Misery Mire B.K  ", $00
    db "  Dark Palace B.K  ", $00
    db "  Swamp Palace B.K ", $00
    db "                   ", $00
    db " Desert Palace B.K ", $00
    db " Eastern Palace B.K", $00
    db "                   ", $00
    db "                   ", $00  ; 9F

    db " Hyrule Castle Key ", $00 ; A0
    db "    Sewers Key     ", $00
    db " Eastern Palace Key", $00
    db " Desert Palace Key ", $00
    db " Castle Tower Key  ", $00
    db " Swamp Palace Key  ", $00
    db "  Dark Palace Key  ", $00
    db "  Misery Mire Key  ", $00
    db "  Skull Woods Key  ", $00
    db "  Ice Palace Key   ", $00
    db " Tower of Hera Key ", $00
    db " Thieves' Town Key ", $00
    db "  Turtle Rock Key  ", $00
    db " Ganon's Tower Key ", $00
    db "                   ", $00
    db "                   ", $00 ; AF   
 
alttp_dialog_char_table:
    ; Each unsupported symbol translate to "?" $C6 for visual indication
    ;  <sp> !    "    #    $    %    &    '    (    )    *    +    ,    -    .    /
    db $FF, $C7, $D8, $4C, $C6, $C6, $C6, $D8, $C6, $C6, $C6, $C6, $C8, $C9, $CD, $C6
    ;  0    1    2    3    4    5    6    7    8    9    :    ;    <    =    >    ?
    db $A0, $A1, $A2, $A3, $A4, $A5, $A6, $A7, $A8, $A9, $4A, $C6, $C6, $C6, $C6, $C6
    ;  @    A    B    C    D    E    F    G    H    I    J    K    L    M    N    O
    db $4B, $AA, $AB, $AC, $AD, $AE, $AF, $B0, $B1, $B2, $B3, $B4, $B5, $B6, $B7, $B8
    ;  P    Q    R    S    T    U    V    W    X    Y    Z    [    \    ]    ^    _
    db $B9, $BA, $BB, $BC, $BD, $BE, $BF, $C0, $C1, $C2, $C3, $C6, $C6, $C6, $C6, $C6

; Lowercase Letters
    ;  `    a    b    c    d    e    f    g    h    i    j    k    l    m    n    o
    db $C6, $30, $31, $32, $33, $34, $35, $36, $37, $38, $39, $3A, $3B, $3C, $3D, $3E
    ;  p    q    r    s    t    u    v    w    x    y    z    {    |    }    ~    <del>
    db $3F, $40, $41, $42, $43, $44, $45, $46, $47, $48, $49, $C6, $C6, $C6, $CE, $C6

alttp_mw_check_softreset:
    lda $4219 : sta $01
    cmp #$30
    bne .end
    lda $00
    cmp #$30
    bne .end
    lda.l !SRAM_SAVING
    bne .end
    jmp alttp_mw_softreset

.end
    rtl

alttp_mw_softreset:

    lda $000202
    sta !SRAM_ALTTP_EQUIPMENT_1
    lda $000303
    sta !SRAM_ALTTP_EQUIPMENT_2

    sei                         ; Disable IRQ's
    
    %i16()
    %a8()

    lda #$01
    sta $420d                   ; Toggle FastROM on

    lda #$00
    sta $4200                   ; Disable NMI and Joypad autoread
    sta $420c                   ; Disable H-DMA

    lda #$8f
    sta $2100                   ; Enable PPU force blank

    jsl sm_spc_reset            ; Kill the ALTTP music engine and put the SPC in IPL upload mode
                                ; Gotta do this before switching RAM contents

-
    bit $4212                   ; Wait for a fresh NMI
    bmi -

-
    bit $4212
    bpl -

    %ai16()

    ldx #$1ff0
    txs                         ; Adjust stack pointer

    lda #$ffff                  ; Set the "game flag" to SM so IRQ's/NMI runs using the 
    sta !SRAM_CURRENT_GAME      ; correct game

    jsl sm_fix_checksum         ; Fix SRAM checksum (otherwise SM deletes the file on load)

    jml $80841C                 ; Jump to SM boot code