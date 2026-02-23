RoomTag_RoomTrigger_KillDoor_ExtendedItems:
    LDA CrystalPendantFlags_2,X
    BEQ .pendant
    CMP #$40
    BEQ .crystal
    BRA .boss

    .pendant
    LDA.l $7EF374
    AND.l CrystalPendantFlags, X
    BEQ .exit
    BRA .open_door

    .crystal
    LDA.l $7EF37A
    AND.l CrystalPendantFlags, X
    BEQ .exit
    BRA .open_door

    .boss
    LDA.l !SRAM_SM_ITEM_BUF+$72
    AND.l CrystalPendantFlags, X
    BEQ .exit

    .open_door
    JML.l $01C529

    .exit
    JML.l $01C53E

RoomTag_KillRoomForPrize_ExtendedItems:
    LDA CrystalPendantFlags_2,X
    BEQ .pendant
    CMP #$40
    BEQ .crystal
    BRA .boss

    .pendant
    LDA.l $7EF374
    AND.l CrystalPendantFlags, X
    BNE .delete_tag
    BRA .spawn_prize

    .crystal
    LDA.l $7EF37A
    AND.l CrystalPendantFlags, X
    BNE .delete_tag
    BRA .spawn_prize

    .boss
    LDA.l !SRAM_SM_ITEM_BUF+$72
    AND.l CrystalPendantFlags, X
    BNE .delete_tag

    .spawn_prize
    JML.l $01C731

    .delete_tag
    JML.l $01C749


WorldMap_LoadSpriteGFX_ExtendedItems:
    ; Spend two frames in this routine to load two different sprite sets for the map
    ; To compensate, abuse the fade-in and start it here already so no frames are lost.

    LDA.b $13
    BNE +
        ; Load original sprite sheet
        LDA.b #$10
        STA.w $0AAA
        JSL $00E43A
        STZ.w $0AAA
        INC.b $13
        BRA .exit
    +
        ; Load extended sprite sheet
        LDA.b #$F1
        STA.w $0AAA
        JSL $00E43A
        STZ.w $0AAA
        INC.w $0200
    
    .exit
    RTL

Graphics_LoadChrHalfSlot_ExtendedItems:
    CPX #$F0
    BCS .extended

    ; Regular code
    LDA $D033, y
    STA $02
    STA $05
    JML $00E4A9

    .extended
    TXA : AND #$0f : TAX
    LDA.l .bank, x : STA $02 : STA $05
    LDA.l .high, x : STA $01
    LDA.l .low, x : STA $00

    JML $00E4B3

    ; Pointers to uncompressed 3BPP sheet to load into VRAM (1 row of tiles)
    ; Even numbers loads to $4600, odd numbers loads to $4400

    .bank
        db $00, GFX_Map_ExtSprites>>16
    .high
        db $00, GFX_Map_ExtSprites>>8
    .low
        db $00, GFX_Map_ExtSprites


Ancilla29_MilestoneItemGet_ExtendedItems:
    LDA $0c5e, x
    CMP #$b0        ; If the item id is > $b0 it's an SM item (or other extended item)
    BCS .extended

    CMP $caaf
    RTL

    .extended
    PHX
    TAX : LDA.l AddReceivedItemExpanded_item_graphics_indices, x : TAY
    PLX
    RTL

AncillaAdd_FallingPrize_ExtendedItems:
    CPY #$80
    BCS .extended

    ; If item id < $80 then the item is a regular item
    LDA $8b7c, y
    BRA .exit

    .extended
    ; Mask with $7f and load item from regular table
    PHX
    TYA : AND #$7f : TAX
    LDA.l .items, x
    LDY #$01
    STY $02D8
    PLX

    .exit    
    STA.w $0C5E, x    
    RTL

    .items 
        db $c5 ; Kraid Boss Token
        db $c6 ; Phantoon Boss Token
        db $c7 ; Draygon Boss Token
        db $c8 ; Ridley Boss Token