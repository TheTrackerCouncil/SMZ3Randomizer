; Add ALTTP items to SM
;
; Extra message box code lifted from MessageBoxV4 by Kejardon, JAM

!IBranchItem = #$887C
!ISetItem = #$8899
!ILoadSpecialGraphics = #$8764
!ISetGoto = #$8A24
!ISetPreInstructionCode = #$86C1
!IDrawCustom1 = #$E04F
!IDrawCustom2 = #$E067
!IGoto = #$8724
!IKill = #$86BC
!IPlayTrackNow = #$8BDD
!IJSR = #$8A2E
!ISetCounter8 = #$874E
!IGotoDecrement = #$873F
!IGotoIfDoorSet = #$8A72
!ISleep = #$86B4
!IVisibleItem = #i_visible_item
!IChozoItem = #i_chozo_item
!IHiddenItem = #i_hidden_item
!ILoadCustomGraphics = #i_load_custom_graphics
!IPickup = #i_pickup
!IStartDrawLoop = #i_start_draw_loop
!IStartHiddenDrawLoop = #i_start_hidden_draw_loop

!ITEM_PLM_BUF = $7ffb00
!SM_MULTIWORLD_PICKUP = $7ffafe

!Big = #$825A
!Small = #$8289
!EmptySmall = #$8436
!Shot = #$83C5
!Dash = #$83CC
!Jump = #$8756
!ItemCancel = #$875D
!ItemSelect = #$8764
!AimDown = #$876B
!AimUp = #$8773

!EmptyBig = #EmptyBig
!PlaceholderBig = #PlaceholderBig
!DungeonItemBig = #DungeonItemBig
!DungeonKeyItemBig = #DungeonKeyItemBig
!KeycardBig = #KeycardBig

; org $01E9BC
;     db $d1

; org $cf8432
;     dw $efe0
; org $cf8432+$5
;     db $D2

;org $cf86de
;    dw $efe4    ; Morph to progressive sword

org $C22D7C   ; Patch to Crateria surface palette for Z3 items e.g. PoH, Pearl
    incbin "data/Crateria_palette.bin"

org $C23798   ; Crocomire's room changes colors $2E, $2F, $3E, and $3F for reasons unknown.
    incbin "data/Crocomire_palette.bin"

org $c98000      ; New item graphics
base $898000
    incbin "data/newitems.bin"

org $c4efe0     ; First free space in PLM block
base $84efe0

plm_items:
    dw i_visible_item_setup, v_item       ;efe0
    dw i_visible_item_setup, c_item       ;efe4
    dw i_hidden_item_setup,  h_item       ;efe8
v_item:
    dw !IVisibleItem
c_item:
    dw !IChozoItem
h_item:
    dw !IHiddenItem

; Graphics pointers for items (by item index)
item_graphics:
    ; SM (B0-FF)
    dw $8800 : db $00, $00, $00, $00, $00, $00, $00, $00    ; Grapple beam
    dw $8900 : db $01, $01, $00, $00, $03, $03, $00, $00    ; X-ray scope
    dw $8300 : db $00, $00, $00, $00, $00, $00, $00, $00    ; Varia suit
    dw $8200 : db $00, $00, $00, $00, $00, $00, $00, $00    ; Spring ball
    dw $8700 : db $00, $00, $00, $00, $00, $00, $00, $00    ; Morph ball
    dw $8500 : db $00, $00, $00, $00, $00, $00, $00, $00    ; Screw attack
    dw $8100 : db $00, $00, $00, $00, $00, $00, $00, $00    ; Gravity suit
    dw $8400 : db $00, $00, $00, $00, $00, $00, $00, $00    ; Hi-Jump
    dw $8600 : db $00, $00, $00, $00, $00, $00, $00, $00    ; Space jump
    dw $8000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; Bombs
    dw $8A00 : db $00, $00, $00, $00, $00, $00, $00, $00    ; Speed booster
    dw $8B00 : db $00, $00, $00, $00, $00, $00, $00, $00    ; Charge
    dw $8C00 : db $00, $03, $00, $00, $00, $03, $00, $00    ; Ice Beam
    dw $8D00 : db $00, $02, $00, $00, $00, $02, $00, $00    ; Wave beam
    dw $8F00 : db $00, $00, $00, $00, $00, $00, $00, $00    ; Spazer
    dw $8E00 : db $00, $01, $00, $00, $00, $01, $00, $00    ; Plasma beam

    ; C0
    dw $0008 : db $00, $00, $00, $00, $00, $00, $00, $00    ; Energy Tank
    dw $9000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; Reserve tank
    dw $000A : db $00, $00, $00, $00, $00, $00, $00, $00    ; Missile
    dw $000C : db $00, $00, $00, $00, $00, $00, $00, $00    ; Super Missile
    dw $000E : db $00, $00, $00, $00, $00, $00, $00, $00    ; Power Bomb
    
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; C5 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; C6 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; C7 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; C8 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; C9 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; CA - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; CB - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; CC - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; CD - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; CE - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; CF - Unused

    dw $EE00 : db $03, $00, $00, $00, $03, $00, $00, $00    ; D0 - Crateria L1 Key
    dw $EF00 : db $02, $00, $00, $00, $02, $00, $00, $00    ; D1 - Crateria L2 Key
    dw $F000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; D2 - Crateria Boss Key

    dw $EE00 : db $03, $00, $00, $00, $03, $00, $00, $00    ; D3 - Brinstar L1 Key
    dw $EF00 : db $02, $00, $00, $00, $02, $00, $00, $00    ; D4 - Brinstar L2 Key
    dw $F000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; D5 - Brinstar Boss Key

    dw $EE00 : db $03, $00, $00, $00, $03, $00, $00, $00    ; D6 - Norfair L1 Key
    dw $EF00 : db $02, $00, $00, $00, $02, $00, $00, $00    ; D7 - Norfair L2 Key
    dw $F000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; D8 - Norfair Boss Key

    dw $EE00 : db $03, $00, $00, $00, $03, $00, $00, $00    ; D9 - Maridia L1 Key
    dw $EF00 : db $02, $00, $00, $00, $02, $00, $00, $00    ; DA - Maridia L2 Key
    dw $F000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; DB - Maridia Boss Key

    dw $EE00 : db $03, $00, $00, $00, $03, $00, $00, $00    ; DC - Wrecked Ship L1 Key
    dw $F000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; DD - Wrecked Ship Boss Key

    dw $EE00 : db $03, $00, $00, $00, $03, $00, $00, $00    ; DE - Lower Norfair L1 Key
    dw $F000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; DF - Lower Norfair Boss Key

    dw $F500 : db $00, $00, $00, $00, $00, $00, $00, $00    ; E0 - L1 Key Plaque
    dw $F600 : db $00, $00, $00, $00, $00, $00, $00, $00    ; E1 - L2 Key Plaque
    dw $F700 : db $00, $00, $00, $00, $00, $00, $00, $00    ; E2 - Boss Key Plaque

    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; E3 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; E4 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; E5 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; E6 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; E7 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; E8 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; E9 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; EA - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; EB - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; EC - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; ED - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; EE - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00    ; EF - Unused


    ; ALTTP (00-AF)
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 00 Dummy - L1SwordAndShield        
    dw $CA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 01 Master Sword
    dw $CB00 : db $01, $01, $01, $01, $01, $01, $01, $01        ; 02 Tempered Sword
    dw $CC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 03 Gold Sword
    dw $CF00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 04 Blue Shield
    dw $D000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 05 Red Shield
    dw $D100 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 06 Mirror Shield
    dw $AF00 : db $02, $00, $00, $00, $02, $00, $00, $00        ; 07 Fire Rod
    dw $B000 : db $00, $03, $00, $00, $00, $03, $00, $00        ; 08 Ice Rod
    dw $B500 : db $01, $01, $01, $01, $01, $01, $01, $01        ; 09 Hammer
    dw $9100 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 0A Hookshot
    dw $9200 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 0B Bow
    dw $DF00 : db $03, $03, $03, $03, $03, $03, $03, $03        ; 0C Blue Boomerang
    dw $9700 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 0D Powder
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 0E Bee (bottle contents)
    dw $B100 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 0F Bombos

    dw $B200 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 10 Ether
    dw $B300 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 11 Quake
    dw $B400 : db $01, $01, $01, $01, $01, $01, $01, $01        ; 12 Lamp
    dw $B600 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 13 Shovel
    dw $B700 : db $03, $03, $03, $03, $03, $03, $03, $03        ; 14 Flute
    dw $C100 : db $02, $02, $02, $02, $02, $02, $02, $02        ; 15 Somaria
    dw $BA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 16 Empty Bottle
    dw $D200 : db $02, $02, $02, $02, $02, $02, $02, $02        ; 17 Heart Piece
    dw $C200 : db $03, $03, $03, $03, $03, $03, $03, $03        ; 18 Cane of Byrna
    dw $C300 : db $02, $02, $02, $02, $02, $02, $02, $02        ; 19 Cape
    dw $C400 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 1A Mirror
    dw $C500 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 1B Gloves
    dw $C600 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 1C Titan's Mitts
    dw $B900 : db $01, $01, $01, $01, $01, $01, $01, $01        ; 1D Book
    dw $C800 : db $03, $03, $03, $03, $03, $03, $03, $03        ; 1E Flippers
    dw $C900 : db $02, $02, $02, $02, $02, $02, $02, $02        ; 1F Moon Pearl

    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 20 Dummy     
    dw $B800 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 21 Bug-Catching Net
    dw $CD00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 22 Blue Tunic
    dw $CE00 : db $01, $01, $01, $01, $01, $01, $01, $01        ; 23 Red Tunic
    dw $EB00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 24 Dummy - Key       
    dw $EC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 25 Dummy - Compass
    dw $D300 : db $02, $02, $02, $02, $02, $02, $02, $02        ; 26 Heart Container (no animation)
    dw $9400 : db $03, $03, $03, $03, $03, $03, $03, $03        ; 27 One bomb
    dw $D700 : db $03, $03, $03, $03, $03, $03, $03, $03        ; 28 3 Bombs
    dw $9600 : db $01, $01, $01, $01, $01, $01, $01, $01        ; 29 Mushroom
    dw $E000 : db $02, $02, $02, $02, $02, $02, $02, $02        ; 2A Red Boomerang
    dw $BB00 : db $01, $01, $01, $01, $01, $01, $01, $01        ; 2B Red Potion Bottle
    dw $BC00 : db $01, $01, $01, $01, $01, $01, $01, $01        ; 2C Green Potion Bottle
    dw $BD00 : db $03, $03, $03, $03, $03, $03, $03, $03        ; 2D Blue Potion Bottle
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 2E Dummy - Red potion (contents)
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 2F Dummy - Green potion (contents)

    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 30 Dummy - Blue potion (contents)
    dw $D800 : db $03, $03, $03, $03, $03, $03, $03, $03        ; 31 10 Bombs
    dw $EA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 32 Dummy - Big key
    dw $ED00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 33 Dummy - Map
    dw $D900 : db $01, $01, $01, $01, $01, $01, $01, $01        ; 34 1 Rupee
    dw $DA00 : db $03, $03, $03, $03, $03, $03, $03, $03        ; 35 5 Rupees
    dw $DB00 : db $02, $02, $02, $02, $02, $02, $02, $02        ; 36 20 Rupees
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 37 Dummy - Pendant of Courage
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 38 Dummy - Pendant of Wisdom
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 39 Dummy - Pendant of Power
    dw $9200 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 3A Bow and Arrows
    dw $DE00 : db $01, $01, $01, $01, $01, $01, $01, $01        ; 3B Silver Arrows
    dw $BE00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 3C Bee Bottle
    dw $C000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 3D Fairy Bottle
    dw $D300 : db $02, $02, $02, $02, $02, $02, $02, $02        ; 3E Heart Container - Boss
    dw $D300 : db $02, $02, $02, $02, $02, $02, $02, $02        ; 3F Heart Container - Sanc

    dw $DD00 : db $01, $01, $01, $01, $01, $01, $01, $01        ; 40 100 Rupees
    dw $DC00 : db $01, $01, $01, $01, $01, $01, $01, $01        ; 41 50 Rupees
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 42 Dummy - Small heart
    dw $D400 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 43 Single Arrow
    dw $D600 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 44 10 Arrows
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 45 Dummy - Small magic
    dw $9300 : db $01, $01, $01, $01, $01, $01, $01, $01        ; 46 300 Rupees
    dw $DB00 : db $02, $02, $02, $02, $02, $02, $02, $02        ; 47 20 Rupees
    dw $BF00 : db $01, $01, $01, $01, $01, $01, $01, $01        ; 48 Good Bee Bottle
    dw $E700 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 49 Fighter Sword
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 4A Dummy - Activated flute
    dw $C700 : db $02, $02, $02, $02, $02, $02, $02, $02        ; 4B Boots
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 4C Dummy - 50 Bomb upgrade
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 4D Dummy - 70 Arrow upgrade
    dw $E100 : db $01, $01, $01, $01, $01, $01, $01, $01        ; 4E Half Magic
    dw $E200 : db $01, $01, $01, $01, $01, $01, $01, $01        ; 4F Quarter Magic

    dw $CA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 50 Master Sword    
    dw $E300 : db $03, $03, $03, $03, $03, $03, $03, $03        ; 51 5 Bomb Upgrade
    dw $E400 : db $03, $03, $03, $03, $03, $03, $03, $03        ; 52 10 Bomb Upgrade
    dw $E500 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 53 5 Arrow Upgrade
    dw $E600 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 54 10 Arrow Upgrade
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 55 Dummy - Programmable 1
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 56 Dummy - Programmable 2
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 57 Dummy - Programmable 3
    dw $DE00 : db $01, $01, $01, $01, $01, $01, $01, $01        ; 58 Silver Arrows

    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 59 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 5A - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 5B - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 5C - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 5D - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 5E - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 5F - Unused

    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 60 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 61 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 62 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 63 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 64 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 65 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 66 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 67 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 68 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 69 - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 6A - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 6B - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 6C - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 6D - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 6E - Unused
    dw $0000 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 6F - Unused

    dw $ED00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 70 - Unused
    dw $ED00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 71 - Unused
    dw $ED00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 72 - Ganons Tower Map
    dw $ED00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 73 - Turtle Rock Map
    dw $ED00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 74 - Thieves' Town Map
    dw $ED00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 75 - Tower of Hera Map
    dw $ED00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 76 - Ice Palace Map
    dw $ED00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 77 - Skull Woods Map
    dw $ED00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 78 - Misery Mire Map
    dw $ED00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 79 - Palace Of Darkness Map
    dw $ED00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 7A - Swamp Palace Map
    dw $ED00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 7B - Unused
    dw $ED00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 7C - Desert Palace Map
    dw $ED00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 7D - Eastern Palace Map
    dw $ED00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 7E - Unused
    dw $ED00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 7F - Hyrule Castle Map

    dw $EC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 80 - Unused
    dw $EC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 81 - Unused
    dw $EC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 82 - Ganons Tower Compass
    dw $EC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 83 - Turtle Rock Compass
    dw $EC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 84 - Thieves' Town Compass
    dw $EC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 85 - Tower of Hera Compass
    dw $EC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 86 - Ice Palace Compass
    dw $EC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 87 - Skull Woods Compass
    dw $EC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 88 - Misery Mire Compass
    dw $EC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 89 - Palace of Darkness Compass
    dw $EC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 8A - Swamp Palace Compass
    dw $EC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 8B - Unused
    dw $EC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 8C - Desert Palace Compass
    dw $EC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 8D - Eastern Palace Compass
    dw $EC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 8E - Unused
    dw $EC00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 8F - Unused

    dw $EA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 90 - Unused
    dw $EA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 91 - Unused
    dw $EA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 92 - Ganons Tower Big Key
    dw $EA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 93 - Turtle Rock Big Key
    dw $EA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 94 - Thieves' Town Big Key
    dw $EA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 95 - Tower of Hera Big Key
    dw $EA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 96 - Ice Palace Big Key
    dw $EA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 97 - Skull Woods Big Key
    dw $EA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 98 - Misery Mire Big Key
    dw $EA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 99 - Palace of Darkness Big Key
    dw $EA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 9A - Swamp Palace Big Key
    dw $EA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 9B - Unused
    dw $EA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 9C - Desert Palace Big Key
    dw $EA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 9D - Eastern Palace Big Key
    dw $EA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 9E - Unused
    dw $EA00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 9F - Unused

    dw $EB00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; A0 - Hyrule Castle Small Key
    dw $EB00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; A1 - Unused
    dw $EB00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; A2 - Unused
    dw $EB00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; A3 - Desert Palace Small Key
    dw $EB00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; A4 - Castle Tower Small Key
    dw $EB00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; A5 - Swamp Palace Small Key
    dw $EB00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; A6 - Palace of Darkness Small Key
    dw $EB00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; A7 - Misery Mire Small Key
    dw $EB00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; A8 - Skull Woods Small Key
    dw $EB00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; A9 - Ice Palace Small Key
    dw $EB00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; AA - Tower of Hera Small Key
    dw $EB00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; AB - Thieves' Town Small Key
    dw $EB00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; AC - Turtle Rock Small Key
    dw $EB00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; AD - Ganons Tower Small Key
    dw $EB00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; AE - Unused
    dw $EB00 : db $00, $00, $00, $00, $00, $00, $00, $00        ; AF - Unused

    ;dw $D300 : db $02, $02, $02, $02, $02, $02, $02, $02        ; Heart Container
    ;dw $D500 : db $00, $00, $00, $00, $00, $00, $00, $00        ; 5 Arrows

sm_item_table:
    ; pickup, qty,   msg,   type,  ext2,  ext3,  loop,  hloop
    dw $891A, $4000, $0000, $0004, $0000, $0000, $0000, $0000      ; Grapple
    dw $8941, $8000, $0000, $0004, $0000, $0000, $0000, $0000      ; X-ray scope
    dw $88F3, $0001, $0007, $0004, $0000, $0000, $0000, $0000      ; Varia suit
    dw $88F3, $0002, $0008, $0004, $0000, $0000, $0000, $0000      ; Spring ball
    dw $88F3, $0004, $0009, $0004, $0000, $0000, $0000, $0000      ; Morph ball
    dw $88F3, $0008, $000A, $0004, $0000, $0000, $0000, $0000      ; Screw attack
    dw $88F3, $0020, $001A, $0004, $0000, $0000, $0000, $0000      ; Gravity suit
    dw $88F3, $0100, $000B, $0004, $0000, $0000, $0000, $0000      ; Hi-jump
    dw $88F3, $0200, $000C, $0004, $0000, $0000, $0000, $0000      ; Space jump
    dw $88F3, $1000, $0013, $0004, $0000, $0000, $0000, $0000      ; Bombs
    dw $88F3, $2000, $000D, $0004, $0000, $0000, $0000, $0000      ; Speed booster
    dw $88B0, $1000, $000E, $0005, $0000, $0000, $0000, $0000      ; Charge beam
    dw $88B0, $0002, $000F, $0005, $0000, $0000, $0000, $0000      ; Ice beam
    dw $88B0, $0001, $0010, $0005, $0000, $0000, $0000, $0000      ; Wave beam
    dw $88B0, $0004, $0011, $0005, $0000, $0000, $0000, $0000      ; Spazer
    dw $88B0, $0008, $0012, $0005, $0000, $0000, $0000, $0000      ; Plasma
    dw $8968, $0064, $0000, $0000, $0000, $0000, $E0A5, #p_etank_hloop      ; E-Tank
    dw $8986, $0064, $0000, $0006, $0000, $0000, $0000, $0000      ; Reserve tank
    dw $89A9, $0005, $0000, $0001, $0000, $0000, $E0CA, #p_missile_hloop    ; Missiles
    dw $89D2, $0005, $0000, $0002, $0000, $0000, $E0EF, #p_super_hloop      ; Super Missiles
    dw $89FB, $0005, $0000, $0003, $0000, $0000, $E114, #p_pb_hloop         ; Power Bombs


progressive_items:
    db $5e, $59, $04, $49, $01, $02, $03, $00     ; Progressive sword
    db $5f, $5A, $03, $04, $05, $06, $00, $00     ; Progressive shield
    db $60, $5B, $02, $22, $23, $00, $00, $00     ; Progressive armor
    db $61, $54, $02, $1b, $1c, $00, $00, $00     ; Progressive glove
    db $ff

i_visible_item:
    lda #$0006
    jsr i_load_rando_item
    rts

i_chozo_item:
    lda #$0008
    jsr i_load_rando_item
    rts

i_hidden_item:
    lda #$000A
    jsr i_load_rando_item
    rts

p_etank_hloop:
    dw $0004, $a2df
    dw $0004, $a2e5
    dw !IGotoDecrement, p_etank_hloop
    dw !IJSR, $e020
    dw !IGoto, p_hidden_item_loop2

p_missile_hloop:
    dw $0004, $A2EB
    dw $0004, $A2F1
    dw !IGotoDecrement, p_missile_hloop
    dw !IJSR, $e020
    dw !IGoto, p_hidden_item_loop2

p_super_hloop:
    dw $0004, $A2F7
    dw $0004, $A2FD
    dw !IGotoDecrement, p_super_hloop
    dw !IJSR, $e020
    dw !IGoto, p_hidden_item_loop2

p_pb_hloop:
    dw $0004, $A303
    dw $0004, $A309
    dw !IGotoDecrement, p_pb_hloop
    dw !IJSR, $e020
    dw !IGoto, p_hidden_item_loop2

p_visible_item:
    dw !ILoadCustomGraphics
    dw !IBranchItem, .end
    dw !ISetGoto, .trigger
    dw !ISetPreInstructionCode, $df89
    dw !IStartDrawLoop
    .loop
    dw !IDrawCustom1
    dw !IDrawCustom2
    dw !IGoto, .loop
    .trigger
    dw !ISetItem
    dw SOUNDFX : db !Click
    dw !IPickup
    .end
    dw !IGoto, $dfa9

p_chozo_item:
    dw !ILoadCustomGraphics
    dw !IBranchItem, .end
    dw !IJSR, $dfaf
    dw !IJSR, $dfc7
    dw !ISetGoto, .trigger
    dw !ISetPreInstructionCode, $df89
    dw !ISetCounter8 : db $16
    dw !IStartDrawLoop
    .loop
    dw !IDrawCustom1
    dw !IDrawCustom2
    dw !IGoto, .loop
    .trigger
    dw !ISetItem
    dw SOUNDFX : db !Click
    dw !IPickup
    .end
    dw $0001, $a2b5
    dw !IKill   

p_hidden_item:
    dw !ILoadCustomGraphics
    .loop2
    dw !IJSR, $e007
    dw !IBranchItem, .end
    dw !ISetGoto, .trigger
    dw !ISetPreInstructionCode, $df89
    dw !ISetCounter8 : db $16
    dw !IStartHiddenDrawLoop
    .loop
    dw !IDrawCustom1
    dw !IDrawCustom2
    dw !IGotoDecrement, .loop
    dw !IJSR, $e020
    dw !IGoto, .loop2
    .trigger
    dw !ISetItem
    dw SOUNDFX : db !Click
    dw !IPickup
    .end
    dw !IJSR, $e032
    dw !IGoto, .loop2

i_start_draw_loop:
    phy : phx
    lda !ITEM_PLM_BUF, x    ; Load item id
    cmp #$0015
    bcc .smItem
    bra .custom_item

.smItem
    asl #4
    clc : adc #$000C
    tax
    lda sm_item_table, x      ; Load next loop point if available
    beq .custom_item
    plx : ply
    tay
    rts

.custom_item
    plx
    ply
    rts

i_start_hidden_draw_loop:
    phy : phx
    lda !ITEM_PLM_BUF, x    ; Load item id
    cmp #$0015
    bcc .smItem
    bra .custom_item

.smItem
    asl #4
    clc : adc #$000E
    tax
    lda sm_item_table, x      ; Load next loop point if available
    beq .custom_item
    plx : ply
    tay
    rts

.custom_item
    plx
    ply
    rts

i_load_custom_graphics:
    phy : phx 
    lda !ITEM_PLM_BUF, x    ; Load item id

    %a8()
    sta $4202
    lda #$0A
    sta $4203
    nop : nop : %ai16()
    lda $4216               ; Multiply it by 0x0A
    clc
    adc #item_graphics
    tay                     ; Add it to the graphics table and transfer into Y
    lda $0000, y
    cmp #$8000
    bcc .no_custom    
    jsr $8764               ; Jump to original PLM graphics loading routine
    plx
    ply
    rts

.no_custom
    tay
    lda $0000, y
    sta.l $7edf0c, x
    plx
    ply
    rts

i_visible_item_setup:
    phy : phx
    jsr load_item_id                
    %a8()
    sta $4202
    lda #$0A
    sta $4203
    nop : nop : %ai16()
    lda $4216                       ; Multiply it by 0x0A
    tax

    lda item_graphics, x
    cmp #$8000
    bcc .no_custom
    plx : ply
    jmp $ee64

.no_custom
    plx : ply
    tyx
    sta.l $7edf0c, x
    jmp $ee64

i_hidden_item_setup:
    phy : phx
    jsr load_item_id
    %a8()
    sta $4202
    lda #$0A
    sta $4203
    nop : nop : %ai16()
    lda $4216                       ; Multiply it by 0x0A
    tax

    lda item_graphics, x
    cmp #$8000
    bcc .no_custom
    plx : ply
    jmp $ee8e
    
.no_custom
    plx : ply
    tyx
    sta.l $7edf0c, x
    jmp $ee8e


i_load_rando_item:
    cmp #$0006 : bne +
    ldy #p_visible_item
    bra .end
+   cmp #$0008 : bne +    
    ldy #p_chozo_item
    bra .end
+   ldy #p_hidden_item

.end
    rts

; Pick up item
i_pickup:
    phx : phy : php : phx

    lda #$0000
    sta !SM_MULTIWORLD_PICKUP

    lda.l config_multiworld
    beq .own_item

    lda $1dc7, x              ; Load PLM room argument
    asl #3 : tax

    lda.l rando_item_table, x       ; Load item type
    beq .own_item

.multiworld_item                    ; This is someone elses item, send message
    phx
    lda.l rando_item_table+$4, x    ; Load item owner into Y
    tay
    lda.l rando_item_table+$2, x    ; Load original item id into X
    tax
    pla                             ; Multiworld item table id in A
    phx : phy
    jsl mw_write_message            ; Send message
    ply : plx
    jsl sm_mw_display_item_sent     ; Display custom message box
    plx
    bra .end

.own_item
    plx
    lda !ITEM_PLM_BUF, x        ; Load adjusted item id (progression etc)
    cmp #$0040
    bcc .smItem
    sec
    sbc #$0040
    sta !ITEM_PLM_BUF, x        ; Readjust item id so it fits the ALTTP table again
    jsl alttp_item_pickup
    bra .end

.smItem
    jsr receive_sm_item
    bra .end

.end
    plp : ply : plx
    rts

; Get item ID from randomizer table (and adjust item id to local tables as needed for graphics display)
load_item_id:
    phx : phy
    lda.l config_multiworld
    bne .multiworld_item
    lda $1dc7, y                    ; Load PLM room argument
    pha
    xba : and #$00ff                ; Get top 8 bits of room argument as item id
    tax : pla
    and #$00ff
    sta $1dc7, y                    ; Clear out item ID from room argument (so we don't mess with SRAM item indexes)
    txa
    bra .checkItem
.multiworld_item
    lda $1dc7, y                    ; Load PLM room argument
    asl #3 : tax    
    lda.l rando_item_table+$2, x    ; Load item id from table
.checkItem
    jsr check_upgrade_item
    cmp #$00b0                      ; b0+ = SM Item
    bcc .alttpItem
    sec
    sbc #$00b0                      ; Subtract $b0 since we have SM items starting at index 00
    bra +
.alttpItem
    clc
    adc #$0040                      ; Offset alttp items by #$40 to point to the correct table entries
+
    ply
    tyx
    sta !ITEM_PLM_BUF, x            ; Store adjusted item id (used for custom graphics and things) 
    plx
    rts                             

check_upgrade_item:
    phx
    %a8()
    sta $c5
    ldx #$0000
-
    lda progressive_items, x
    cmp #$ff
    beq .notFound
    cmp $c5
    beq .found
    txa : clc : adc #$08 : tax
    bne -
.found
    lda progressive_items+$1, x     ; Load SRAM index offset
    phx : tax
    lda !SRAM_ALTTP_ITEM_BUF, x
    plx
    cmp progressive_items+$2, x     ; Check against max allowed value
    beq +
    inc
+
    stx $c5 : clc : adc $c5 : tax   ; Add upgraded item value to x
    lda progressive_items+$2, x     ; Get new item id
    bra +

.notFound
    lda $c5
+
    %ai16()
    and #$00ff
    plx
    rts

check_upgrade_item_long:
    phb : phk : plb
    jsr check_upgrade_item
    plb
    rtl

receive_sm_item_long:
    jsr receive_sm_item
    rtl

; Item index to receive in A
receive_sm_item:
    cmp #$0015
    bcs .customItem
    asl : asl : asl : asl
    phx
    clc
    adc #sm_item_table ; A contains pointer to pickup routine from item table
    tax
    tay
    iny : iny          ; Y points to data to be used in item pickup routine
    jsr ($0000,x)
    plx
    rts

.customItem
    cmp #$0020
    bcs .keycard
    bra .end

.keycard
    and #$000f
    sta $c7                     ; Store keycard index
    clc : adc #$0080            ; Turn this into an event id
    jsl $8081fa                 ; Set event (receive keycard)

    lda !SM_MULTIWORLD_PICKUP
    bne .multiPickup
    lda #$0062
    jsl $858080                 ; Display message 62 - keycard
    bra .end

.multiPickup
    phx : phy
    jsr SETFX
    lda #$0037
    jsl $809049
    ply : plx
    lda #$005d
    jsl $858080                 ; Display multiworld message
    lda #$0000
    sta !SM_MULTIWORLD_PICKUP   

.end
    rts

warnpc $850000

org $c498e3
base $8498e3
CLIPCHECK:
	LDA $05D7
	CMP #$0002
	BEQ $0E
	LDA #$0000
	JSL $808FF7
	LDA $07F5
	JSL $808FC1
	LDA #$0000
	STA $05D7
	RTL

CLIPSET:
	LDA #$0001
	STA $05D7
	JSL $82BE17
	LDA #$0000
	RTS
SOUNDFX:
	JSR SETFX
	AND #$00FF
	JSL $809049
	RTS
SPECIALFX:
	JSR SETFX
	JSL $8090CB
	RTS
MISCFX:
	JSR SETFX
	JSL $80914D
	RTS
SETFX:
	LDA #$0002
	STA $05D7
	LDA $0000,y
	INY
	RTS

sm_mw_call_receive:
    phx : phy
    jsr SETFX
    lda #$0037
    jsl $809049
    ply : plx
    jsr ($0000,x)
    rtl
    
warnpc $849953

org $f30000
;!SM_INVENTORY_SWAP = $a0638c
;!SM_INVENTORY_SWAP_2 = $a0638e

!SM_INVENTORY_SWAP = !SRAM_ALTTP_ITEM_BUF+$8c
!SM_INVENTORY_SWAP_2 = !SRAM_ALTTP_ITEM_BUF+$8e

alttp_progressive_item:
    phx
    lda $0000,y                 ; Load SRAM offset of progressive item
    iny : iny                   ; Y points to value of first progressive item
    sty $89
    tax
    lda !SRAM_ALTTP_ITEM_BUF,x  ; Load current item value
    and #$00ff
    asl a                       ; Shift left for table lookup
    clc : adc $89
    tax
    ldy $0000,x                 ; Set Y to next instruction by lookup table
    plx
    rtl

alttp_item_pickup:
    phx
    phy
    php
    lda.l config_multiworld
    beq +
    lda !SM_MULTIWORLD_PICKUP
    bne .multiworldItemId
+
    lda !ITEM_PLM_BUF,x               ; Load previously saved item index
    sta $c7                           ; Store current item index
    bra .checkItemSwap
.multiworldItemId                                
    lda $c1                     ; This item was gotten from another player in MW
    jsl check_upgrade_item_long ; Progress item if needed
.checkItemSwap
    jsl check_item_swap         ; Set correct item swap flag if needed
    asl : asl : asl             ; Value * 8 to get a table index
    tax
    lda.l alttp_item_table,x    ; Get ALTTP SRAM index
    tay                         ; Store Index in Y
    lda.l alttp_item_table+4,x  ; Get item type
    sta.l $89                   ; Store item type in temp variable
    and #$00ff

    cmp #$0000
    beq .normal_item_jmp
    cmp #$0001
    beq .increment_item_jmp
    cmp #$0002
    beq .bottle_item_jmp
    cmp #$0003
    beq .piece_of_heart_jmp
    cmp #$0004
    beq .rupees_jmp
    cmp #$0005
    beq .boots_jmp
    cmp #$0006
    beq .flippers_jmp
    cmp #$0007
    beq .silvers_jmp
    cmp #$0010
    beq .dungeon_jmp
    cmp #$0011
    beq .dungeon_key_jmp
    jmp .end

.normal_item_jmp
    jmp .normal_item
.increment_item_jmp
    jmp .increment_item
.bottle_item_jmp
    jmp .bottle_item
.piece_of_heart_jmp
    jmp .piece_of_heart
.rupees_jmp
    jmp .rupees
.boots_jmp
    jmp .boots
.flippers_jmp
    jmp .flippers
.silvers_jmp
    jmp .silvers
.dungeon_jmp
    jmp .dungeon
.dungeon_key_jmp
    jmp .dungeon_key

.normal_item
    lda.l alttp_item_table+2,x               ; Get Item value
    phx
    tyx
    %a8()
    cmp.l !SRAM_ALTTP_ITEM_BUF,x             ; Prevent downgrades
    bcc .normal_item_end
    beq .check_progressive
    sta.l !SRAM_ALTTP_ITEM_BUF,x             ; Save value to ALTTP SRAM

.normal_item_end
    %a16()
    plx
    jmp .end

.check_progressive
    pha
    lda $8a
    and #$01
    cmp #$01
    beq +
    pla
    sta.l !SRAM_ALTTP_ITEM_BUF,x
    bra .normal_item_end

+
    pla   ; Progressive item that we already have, force upgrade
    inc a
    sta.l !SRAM_ALTTP_ITEM_BUF,x
    %a16()
    and #$00ff
    sta $89
    plx
    lda.l alttp_item_table,x
    sta $87
    
    ; find the new index to the alttp_item_table so we can show the correct text box
    ldx #$0000
-
    lda.l alttp_item_table,x
    cmp.l $87
    bne +
    lda.l alttp_item_table+2,x
    cmp.l $89
    bne +
    jmp .end
+    
    txa
    clc
    adc #$0008
    tax
    bra -


.increment_item
    lda.l alttp_item_table+2,x
    phx
    tyx
    %a8()
    clc
    adc.l !SRAM_ALTTP_ITEM_BUF,x
    sta.l !SRAM_ALTTP_ITEM_BUF,x
    %a16()
    plx
    jmp .end

.bottle_item
    phx
    tyx
    lda #$0000
    %a8()
    lda.b #$01
    sta.l !SRAM_ALTTP_ITEM_BUF,x    ; Set we have bottles flag
    ldx #$0000

-
    lda.l !SRAM_ALTTP_ITEM_BUF+$5c,x
    beq .found_bottle_index
    inx
    cpx #$0004
    bne -
    %a16()
    plx
    jmp .end                                ; Bail out if we have all bottles already
.found_bottle_index
    txy
    plx
    lda.l alttp_item_table+2,x    ; Get bottle value
    phx
    tyx
    sta.l !SRAM_ALTTP_ITEM_BUF+$5c,x         ; $a0635b,x             ; Store bottle value to the correct bottle index
    %a16()
    plx
    jmp .end

.piece_of_heart
    %a8()
    phx
    tyx
    lda.l !SRAM_ALTTP_ITEM_BUF,x             ; Load amount of heart pieces
    inc a                                    ; Increment
    cmp.b #$04                               ; Oh we got 4, then give a real heartpiece
    bne .no_heartpiece
    lda #$00
    sta.l !SRAM_ALTTP_ITEM_BUF,x             ; Reset heartpieces
    lda.l !SRAM_ALTTP_ITEM_BUF+$6c           ; $a0636C               
    clc
    adc #$08
    sta.l !SRAM_ALTTP_ITEM_BUF+$6c           ; $a0636C               ; Give a heartpiece
    lda.l !SRAM_ALTTP_ITEM_BUF+$6c
    sta.l !SRAM_ALTTP_ITEM_BUF+$6d           ; Refill Link's Health
    bra .piece_end

.no_heartpiece
    sta.l !SRAM_ALTTP_ITEM_BUF,x             ; Store heartpieces
.piece_end
    lda !SRAM_ALTTP_STATS_BUF+$28            ; Increment global heart piece counter in stats buffer
    inc a                                    ; Logic copied from z3/randomizer/inventory.asm heartpiece routine
    and #$1f
    tax
    lda !SRAM_ALTTP_STATS_BUF+$28
    and #$e0
    sta !SRAM_ALTTP_STATS_BUF+$28
    txa
    ora !SRAM_ALTTP_STATS_BUF+$28
    sta !SRAM_ALTTP_STATS_BUF+$28
    %a16()
    plx
    jmp .end

.rupees
    lda.l alttp_item_table+2,x  ; 16-bit add
    phx
    tyx
    clc
    adc.l !SRAM_ALTTP_ITEM_BUF+$2,x
    sta.l !SRAM_ALTTP_ITEM_BUF,x
    sta.l !SRAM_ALTTP_ITEM_BUF+$2,x
    plx
    jmp .end

.boots
    lda.l alttp_item_table+2,x  ; Get Item value
    phx
    tyx
    %a8()
    sta.l !SRAM_ALTTP_ITEM_BUF,x             ; Save value to ALTTP SRAM
    lda.l !SRAM_ALTTP_ITEM_BUF+$79          ; $a06379
    ora #$04
    sta.l !SRAM_ALTTP_ITEM_BUF+$79          ;$a06379               ; Set dash bit
    %a16()    
    plx
    jmp .end

.flippers
    lda.l alttp_item_table+2,x  ; Get Item value
    phx
    tyx
    %a8()
    sta.l !SRAM_ALTTP_ITEM_BUF,x             ; Save value to ALTTP SRAM
    lda.l !SRAM_ALTTP_ITEM_BUF+$79          ; $a06379
    ora #$02
    sta.l !SRAM_ALTTP_ITEM_BUF+$79          ;$a06379               ; Set swim bit
    %a16()    
    plx
    jmp .end

.silvers
    %a8()
    lda.l !SRAM_ALTTP_ITEM_BUF+$40  ;$a06340               ; Load bow value
    beq .nobow
    clc
    adc #$02
    sta.l !SRAM_ALTTP_ITEM_BUF+$40               ; Store silver arrows and bow
    %a16()
    jmp .end

.dungeon
    %a16()
    lda.l alttp_item_table+2, x ; Get dungeon item bitmask
    phx : tyx
    ora.l !SRAM_ALTTP_ITEM_BUF, x
    sta.l !SRAM_ALTTP_ITEM_BUF, x
    plx
    lda $c7
    and #$000f
    sta $c7     ; Clear out extra bits from item index to use as dungeon id
    jmp .end

.dungeon_key
    %a8()
    lda.l alttp_item_table+2, x ; Get dungeon item value
    phx : tyx
    clc
    adc !SRAM_ALTTP_ITEM_BUF, x
    sta !SRAM_ALTTP_ITEM_BUF, x
    cpx #$007C : bne + : sta !SRAM_ALTTP_ITEM_BUF+$1, x ; Store Hyrule keys to sewer keys
+   lda !SRAM_ALTTP_SMALLKEY_BUF-$7C, x ; Subtract base offset 0x7C to get index into dungeon small key stats buffer
    inc a
    sta !SRAM_ALTTP_SMALLKEY_BUF-$7C, x
    cpx #$007C : bne + : sta !SRAM_ALTTP_SMALLKEY_BUF-$7C+$1, x ; Store Sewer keys to Hyrule Castle keys
+   plx
    %a16()
    lda $c7
    and #$000f
    sta $c7     ; Clear out extra bits from item index to use as dungeon id
    jmp .end

.nobow
    %a8()
    lda.b #$01
    sta.l !SRAM_ALTTP_ITEM_BUF+$76      ;$a06376               ; Give upgrade only silver arrows
    %a16()
    jmp .end

.end
    ;lda #$0168
    ;jsl $82e118                 ; Music fix (no need with nofanfare?)
    lda.l config_multiworld
    beq +

    lda.l !SM_MULTIWORLD_PICKUP
    bne .multiworldMessage
+
    lda.l alttp_item_table+6,x  ; Load message pointer
    and #$00ff
    jsl $858080                 ; Display message
    bra +
.multiworldMessage
    lda #$005d
    jsl $858080                 ; Display multiworld message
    lda #$0000
    sta !SM_MULTIWORLD_PICKUP
+
    jsl zelda_fix_checksum      ; Fix SRAM checksum
    plp
    ply
    plx
    rtl

check_item_swap:
    pha
    phy
    php    
    %ai8()
    tay
	CPY.b #$0C : BNE + ; Blue Boomerang
		LDA !SM_INVENTORY_SWAP : ORA #$80 : STA !SM_INVENTORY_SWAP
		BRL .done
	+ CPY.b #$2A : BNE + ; Red Boomerang
		LDA !SM_INVENTORY_SWAP : ORA #$40 : STA !SM_INVENTORY_SWAP
		BRL .done
	+ CPY.b #$29 : BNE + ; Mushroom
		LDA !SM_INVENTORY_SWAP : ORA #$20 : STA !SM_INVENTORY_SWAP
		BRL .done
	+ CPY.b #$0D : BNE + ; Magic Powder
		LDA !SM_INVENTORY_SWAP : ORA #$10 : STA !SM_INVENTORY_SWAP
		BRL .done
	+ CPY.b #$13 : BNE + ; Shovel
		LDA !SM_INVENTORY_SWAP : ORA #$04 : STA !SM_INVENTORY_SWAP
		BRL .done
	+ CPY.b #$14 : BNE + ; Flute (Inactive)
		LDA !SM_INVENTORY_SWAP : ORA #$02 : STA !SM_INVENTORY_SWAP
		BRL .done
	+ CPY.b #$4A : BNE + ; Flute (Active)
		LDA !SM_INVENTORY_SWAP : ORA #$01 : STA !SM_INVENTORY_SWAP
		BRL .done
	+ CPY.b #$0B : BNE + ; Bow
    	LDA !SM_INVENTORY_SWAP_2 : ORA #$80 : STA !SM_INVENTORY_SWAP_2
		BRL .done
	+ CPY.b #$3A : BNE + ; Bow & Arrows
		LDA !SM_INVENTORY_SWAP_2 : ORA #$80 : STA !SM_INVENTORY_SWAP_2
		BRL .done
	+ CPY.b #$3B : BNE + ; Bow & Silver Arrows
		LDA !SM_INVENTORY_SWAP_2 : ORA #$40 : STA !SM_INVENTORY_SWAP_2
		BRL .done
	+ CPY.b #$43 : BNE + ; Single arrow
		LDA !SM_INVENTORY_SWAP_2 : ORA #$80 : STA !SM_INVENTORY_SWAP_2 ; activate wood arrows in quick-swap
		BRL .done
	+ CPY.b #$58 : BNE + ; Upgrade-Only Silver Arrows
		LDA !SM_INVENTORY_SWAP_2 : ORA #$40 : STA !SM_INVENTORY_SWAP_2
	+ CPY.b #$49 : BNE + ; Fighter's Sword
		JSR .stampSword
		BRL .done
	+ CPY.b #$01 : BNE + ; Master Sword
		JSR .stampSword
		BRL .done
	+ CPY.b #$50 : BNE + ; Master Sword
		JSR .stampSword
		BRL .done
	+ CPY.b #$02 : BNE + ; Tempered Sword
		JSR .stampSword
		BRL .done
	+ CPY.b #$03 : BNE + ; Golden Sword
		JSR .stampSword
		BRL .done
	+ CPY.b #$4B : BNE + ; Pegasus Boots
		JSR .stampBoots
		BRL .done
	+ CPY.b #$1A : BNE + ; Magic Mirror
		JSR .stampMirror
		BRL .done
    + 
.done
    plp
    ply
    pla
    rtl

!SWORD_TIME = "$a06458"
!BOOTS_TIME = "$a0645C"
!FLUTE_TIME = "$a06460"
!MIRROR_TIME = "$a06464"

.stampSword
	REP #$20 ; set 16-bit accumulator
	JSL get_total_frame_time
	LDA.l !SWORD_TIME : BNE +
	LDA.l !SWORD_TIME+2 : BNE +
		LDA.l !SRAM_TIMER1 : STA.l !SWORD_TIME
		LDA.l !SRAM_TIMER2 : STA.l !SWORD_TIME+2
	+
	SEP #$20 ; set 8-bit accumulator
RTS

.stampBoots
	REP #$20 ; set 16-bit accumulator
	JSL get_total_frame_time
	LDA.l !BOOTS_TIME : BNE +
	LDA.l !BOOTS_TIME+2 : BNE +
		LDA.l !SRAM_TIMER1 : STA.l !BOOTS_TIME
		LDA.l !SRAM_TIMER2 : STA.l !BOOTS_TIME+2
	+
	SEP #$20 ; set 8-bit accumulator
RTS

.stampFlute
	REP #$20 ; set 16-bit accumulator
	JSL get_total_frame_time
	LDA.l !FLUTE_TIME : BNE +
	LDA.l !FLUTE_TIME+2 : BNE +
		LDA.l !SRAM_TIMER1 : STA.l !FLUTE_TIME
		LDA.l !SRAM_TIMER2 : STA.l !FLUTE_TIME+2
	+
	SEP #$20 ; set 8-bit accumulator
RTS

.stampMirror
	REP #$20 ; set 16-bit accumulator
	JSL get_total_frame_time
	LDA.l !MIRROR_TIME : BNE +
	LDA.l !MIRROR_TIME+2 : BNE +
		LDA.l !SRAM_TIMER1 : STA.l !MIRROR_TIME
		LDA.l !SRAM_TIMER2 : STA.l !MIRROR_TIME+2
	+
	SEP #$20 ; set 8-bit accumulator
RTS

alttp_item_table:
namespace "alttp_item_"
    ; Offset = ALTTP SRAM Offset
    ; Value = Value to write/add to offset
    ; Type = Type of item (Item / Amount / ...)
    ; Message = Id of message box to show    

    ;  Offset Value  Type  Message
    dw $0000, $0000, $0000, $0000       ; 00 Dummy - L1SwordAndShield 
    dw $0059, $0002, $0100, $0040       ; 01 Master Sword
    dw $0059, $0003, $0100, $0041       ; 02 Tempered Sword
    dw $0059, $0004, $0000, $0042       ; 02 Gold Sword
    dw $005A, $0001, $0100, $0045       ; 04 Shield
    dw $005A, $0002, $0100, $0046       ; 05 Red Shield
    dw $005A, $0003, $0000, $0047       ; 06 Mirror Shield
    dw $0045, $0001, $0000, $0025       ; 07 Firerod
    dw $0046, $0001, $0000, $0026       ; 08 Icerod  
    dw $004B, $0001, $0000, $002B       ; 09 Hammer
    dw $0042, $0001, $0000, $0021       ; 0A Hookshot
    dw $0040, $0002, $0000, $001d       ; 0B Bow                       
    dw $0041, $0001, $0000, $001f       ; 0C Blue Boomerang
    dw $0044, $0002, $0000, $0024       ; 0D Powder
    dw $0000, $0000, $0000, $0000       ; 0E Dummy - Bee (bottle content)
    dw $0047, $0001, $0000, $0027       ; 0F Bombos
    
    dw $0048, $0001, $0000, $0028       ; 10 Ether
    dw $0049, $0001, $0000, $0029       ; 11 Quake
    dw $004A, $0001, $0000, $002A       ; 12 Lamp
    dw $004C, $0001, $0000, $002C       ; 13 Shovel
    dw $004C, $0002, $0000, $002D       ; 14 Flute                      
    dw $0050, $0001, $0000, $0037       ; 15 Somaria
    dw $004F, $0002, $0002, $0030       ; 16 Bottle
    dw $006B, $0001, $0003, $0048       ; 17 Piece of Heart
    dw $0051, $0001, $0000, $0038       ; 18 Byrna
    dw $0052, $0001, $0000, $0039       ; 19 Cape
    dw $0053, $0002, $0000, $003A       ; 1A Mirror
    dw $0054, $0001, $0100, $003B       ; 1B Glove
    dw $0054, $0002, $0000, $003C       ; 1C Mitt
    dw $004E, $0001, $0000, $002F       ; 1D Book
    dw $0056, $0001, $0006, $003E       ; 1E Flippers
    dw $0057, $0001, $0000, $003F       ; 1F Pearl
    
    dw $0000, $0000, $0000, $0000       ; 20 Dummy 
    dw $004D, $0001, $0000, $002E       ; 21 Net
    dw $005B, $0001, $0100, $0043       ; 22 Blue Tunic
    dw $005B, $0002, $0000, $0044       ; 23 Red Tunic
    dw $0000, $0000, $0000, $0000       ; 24 Dummy - key
    dw $0000, $0000, $0000, $0000       ; 25 Dummy - compass
    dw $006C, $0008, $0001, $0049       ; 26 Heart Container - no anim
    dw $0075, $0001, $0001, $0022       ; 27 Bomb 1
    dw $0075, $0003, $0001, $004D       ; 28 3 Bombs                    
    dw $0044, $0001, $0000, $0023       ; 29 Mushroom
    dw $0041, $0002, $0000, $0020       ; 2A Red Boomerang
    dw $004F, $0003, $0002, $0031       ; 2B Red Potion
    dw $004F, $0004, $0002, $0032       ; 2C Green Potion
    dw $004F, $0005, $0002, $0033       ; 2D Blue Potion
    dw $0000, $0000, $0000, $0000       ; 2E Dummy - red
    dw $0000, $0000, $0000, $0000       ; 2F Dummy - green
    
    dw $0000, $0000, $0000, $0000       ; 30 Dummy - blue
    dw $0075, $000A, $0001, $004E       ; 31 10 Bombs
    dw $0000, $0000, $0000, $0000       ; 32 Dummy - big key
    dw $0000, $0000, $0000, $0000       ; 33 Dummy - map
    dw $0060, $0001, $0004, $004F       ; 34 1 Rupee
    dw $0060, $0005, $0004, $0050       ; 35 5 Rupees
    dw $0060, $0014, $0004, $0051       ; 36 20 Rupees
    dw $0000, $0000, $0000, $0000       ; 37 Dummy - Pendant of Courage
    dw $0000, $0000, $0000, $0000       ; 38 Dummy - Pendant of Wisdom
    dw $0000, $0000, $0000, $0000       ; 39 Dummy - Pendant of Power
    dw $0040, $0002, $0000, $001d       ; 3A Bow and arrows
    dw $0040, $0003, $0007, $001e       ; 3B Bow and silver Arrows
    dw $004F, $0007, $0002, $0034       ; 3C Bee
    dw $004F, $0006, $0002, $0036       ; 3D Fairy
    dw $006C, $0008, $0001, $0049       ; 3E Heart Container - Boss
    dw $006C, $0008, $0001, $0049       ; 3F Heart Container - Sanc
    
    dw $0060, $0064, $0004, $0053       ; 40 100 Rupees
    dw $0060, $0032, $0004, $0052       ; 41 50 Rupees
    dw $0000, $0000, $0000, $0000       ; 42 Dummy - small heart
    dw $0076, $0001, $0001, $004A       ; 43 1 Arrow
    dw $0076, $000A, $0001, $004C       ; 44 10 Arrows
    dw $0000, $0000, $0000, $0000       ; 45 Dummy - small magic
    dw $0060, $012C, $0004, $0054       ; 46 300 Rupees
    dw $0060, $0014, $0004, $0051       ; 47 20 Rupees
    dw $004F, $0008, $0002, $0035       ; 48 Good Bee
    dw $0059, $0001, $0100, $005B       ; 49 Fighter Sword
    dw $0000, $0000, $0000, $0000       ; 4A Dummy - activated flute
    dw $0055, $0001, $0005, $003D       ; 4B Boots                      
    dw $0000, $0000, $0000, $0000       ; 4C Dummy - 50+bombs
    dw $0000, $0000, $0000, $0000       ; 4D Dummy - 70+arrows
    dw $007B, $0001, $0100, $0055       ; 4E Half Magic
    dw $007B, $0002, $0000, $0056       ; 4F Quarter Magic              
    
    dw $0059, $0002, $0100, $0040       ; 50 Master Sword
    dw $0070, $0005, $0001, $0057       ; 51 +5 Bombs
    dw $0070, $000A, $0001, $0058       ; 52 +10 Bombs
    dw $0071, $0005, $0001, $0059       ; 53 +5 Arrows
    dw $0071, $000A, $0001, $005A       ; 54 +10 Arrows
    dw $0000, $0000, $0000, $0000       ; 55 Dummy - Programmable 1
    dw $0000, $0000, $0000, $0000       ; 56 Dummy - Programmable 2
    dw $0000, $0000, $0000, $0000       ; 57 Dummy - Programmable 3
    dw $0040, $0003, $0007, $001e       ; 58 Silver Arrows

    dw $0000, $0000, $0000, $0000       ; 59 Unused
    dw $0000, $0000, $0000, $0000       ; 5A Unused
    dw $0000, $0000, $0000, $0000       ; 5B Unused
    dw $0000, $0000, $0000, $0000       ; 5C Unused
    dw $0000, $0000, $0000, $0000       ; 5D Unused
    dw $0000, $0000, $0000, $0000       ; 5E Unused
    dw $0000, $0000, $0000, $0000       ; 5F Unused

    dw $0000, $0000, $0000, $0000       ; 60 Unused
    dw $0000, $0000, $0000, $0000       ; 61 Unused
    dw $0000, $0000, $0000, $0000       ; 62 Unused
    dw $0000, $0000, $0000, $0000       ; 63 Unused
    dw $0000, $0000, $0000, $0000       ; 64 Unused
    dw $0000, $0000, $0000, $0000       ; 65 Unused
    dw $0000, $0000, $0000, $0000       ; 66 Unused
    dw $0000, $0000, $0000, $0000       ; 67 Unused
    dw $0000, $0000, $0000, $0000       ; 68 Unused
    dw $0000, $0000, $0000, $0000       ; 69 Unused
    dw $0000, $0000, $0000, $0000       ; 6A Unused
    dw $0000, $0000, $0000, $0000       ; 6B Unused
    dw $0000, $0000, $0000, $0000       ; 6C Unused
    dw $0000, $0000, $0000, $0000       ; 6D Unused
    dw $0000, $0000, $0000, $0000       ; 6E Unused
    dw $0000, $0000, $0000, $0000       ; 6F Unused

    dw $0068, $0001, $0010, $005e       ; 70 - Unused
    dw $0068, $0002, $0010, $005e       ; 71 - Unused
    dw $0068, $0004, $0010, $005e       ; 72 - Ganons Tower Map
    dw $0068, $0008, $0010, $005e       ; 73 - Turtle Rock Map
    dw $0068, $0010, $0010, $005e       ; 74 - Thieves' Town Map
    dw $0068, $0020, $0010, $005e       ; 75 - Tower of Hera Map
    dw $0068, $0040, $0010, $005e       ; 76 - Ice Palace Map
    dw $0068, $0080, $0010, $005e       ; 77 - Skull Woods Map
    dw $0068, $0100, $0010, $005e       ; 78 - Misery Mire Map
    dw $0068, $0200, $0010, $005e       ; 79 - Palace Of Darkness Map
    dw $0068, $0400, $0010, $005e       ; 7A - Swamp Palace Map
    dw $0068, $0800, $0010, $005e       ; 7B - Unused
    dw $0068, $1000, $0010, $005e       ; 7C - Desert Palace Map
    dw $0068, $2000, $0010, $005e       ; 7D - Eastern Palace Map
    dw $0068, $C000, $0010, $005e       ; 7E - Unused
    dw $0068, $C000, $0010, $005e       ; 7F - Hyrule Castle Map

    dw $0064, $0001, $0010, $005f       ; 80 - Unused
    dw $0064, $0002, $0010, $005f       ; 81 - Unused
    dw $0064, $0004, $0010, $005f       ; 82 - Ganons Tower Compass
    dw $0064, $0008, $0010, $005f       ; 83 - Turtle Rock Compass
    dw $0064, $0010, $0010, $005f       ; 84 - Thieves' Town Compass
    dw $0064, $0020, $0010, $005f       ; 85 - Tower of Hera Compass
    dw $0064, $0040, $0010, $005f       ; 86 - Ice Palace Compass
    dw $0064, $0080, $0010, $005f       ; 87 - Skull Woods Compass
    dw $0064, $0100, $0010, $005f       ; 88 - Misery Mire Compass
    dw $0064, $0200, $0010, $005f       ; 89 - Palace of Darkness Compass
    dw $0064, $0400, $0010, $005f       ; 8A - Swamp Palace Compass
    dw $0064, $0800, $0010, $005f       ; 8B - Unused
    dw $0064, $1000, $0010, $005f       ; 8C - Desert Palace Compass
    dw $0064, $2000, $0010, $005f       ; 8D - Eastern Palace Compass
    dw $0064, $C000, $0010, $005f       ; 8E - Unused
    dw $0064, $C000, $0010, $005f       ; 8F - Unused

    dw $0066, $0001, $0010, $0060       ; 90 - Unused
    dw $0066, $0002, $0010, $0060       ; 91 - Unused
    dw $0066, $0004, $0010, $0060       ; 92 - Ganons Tower Big Key
    dw $0066, $0008, $0010, $0060       ; 93 - Turtle Rock Big Key
    dw $0066, $0010, $0010, $0060       ; 94 - Thieves' Town Big Key
    dw $0066, $0020, $0010, $0060       ; 95 - Tower of Hera Big Key
    dw $0066, $0040, $0010, $0060       ; 96 - Ice Palace Big Key
    dw $0066, $0080, $0010, $0060       ; 97 - Skull Woods Big Key
    dw $0066, $0100, $0010, $0060       ; 98 - Misery Mire Big Key
    dw $0066, $0200, $0010, $0060       ; 99 - Palace of Darkness Big Key
    dw $0066, $0400, $0010, $0060       ; 9A - Swamp Palace Big Key
    dw $0066, $0800, $0010, $0060       ; 9B - Unused
    dw $0066, $1000, $0010, $0060       ; 9C - Desert Palace Big Key
    dw $0066, $2000, $0010, $0060       ; 9D - Eastern Palace Big Key
    dw $0066, $C000, $0010, $0060       ; 9E - Unused
    dw $0066, $C000, $0010, $0060       ; 9F - Unused

    dw $007C, $0001, $0011, $0061       ; A0 - Hyrule Castle Small Key
    dw $007C, $0001, $0011, $0061       ; A1 - Sewers Small Key
    dw $007E, $0001, $0011, $0061       ; A2 - Eastern Palace Small Key
    dw $007F, $0001, $0011, $0061       ; A3 - Desert Palace Small Key
    dw $0080, $0001, $0011, $0061       ; A4 - Castle Tower Small Key
    dw $0081, $0001, $0011, $0061       ; A5 - Swamp Palace Small Key
    dw $0082, $0001, $0011, $0061       ; A6 - Palace of Darkness Small Key
    dw $0083, $0001, $0011, $0061       ; A7 - Misery Mire Small Key
    dw $0084, $0001, $0011, $0061       ; A8 - Skull Woods Small Key
    dw $0085, $0001, $0011, $0061       ; A9 - Ice Palace Small Key
    dw $0086, $0001, $0011, $0061       ; AA - Tower of Hera Small Key
    dw $0087, $0001, $0011, $0061       ; AB - Thieves' Town Small Key
    dw $0088, $0001, $0011, $0061       ; AC - Turtle Rock Small Key
    dw $0089, $0001, $0011, $0061       ; AD - Ganons Tower Small Key
    dw $008A, $0001, $0011, $0061       ; AE - Unused
    dw $008B, $0001, $0011, $0061       ; AF - Unused

;    dw $006C, $0008, $0001, $0049       ; Heart Container
;    dw $0076, $0005, $0001, $004B       ; 5 Arrows




org $c59643
base $859643
    dw !EmptySmall, !Small, bow
    dw !EmptySmall, !Small, silver_arrows
    dw !EmptySmall, !Small, blue_boomerang
    dw !EmptySmall, !Small, red_boomerang
    dw !EmptySmall, !Small, hookshot
    dw !EmptySmall, !Small, bomb_1
    dw !EmptySmall, !Small, mushroom
    dw !EmptySmall, !Small, powder
    dw !EmptySmall, !Small, firerod
    dw !EmptySmall, !Small, icerod
    dw !EmptySmall, !Small, bombos
    dw !EmptySmall, !Small, ether
    dw !EmptySmall, !Small, quake
    dw !EmptySmall, !Small, lamp
    dw !EmptySmall, !Small, hammer
    dw !EmptySmall, !Small, shovel
    dw !EmptySmall, !Small, flute
    dw !EmptySmall, !Small, net
    dw !EmptySmall, !Small, book
    dw !EmptySmall, !Small, bottle_empty
    dw !EmptySmall, !Small, bottle_red
    dw !EmptySmall, !Small, bottle_green
    dw !EmptySmall, !Small, bottle_blue
    dw !EmptySmall, !Small, bottle_bee
    dw !EmptySmall, !Small, bottle_good_bee
    dw !EmptySmall, !Small, bottle_fairy
    dw !EmptySmall, !Small, somaria
    dw !EmptySmall, !Small, byrna
    dw !EmptySmall, !Small, cape
    dw !EmptySmall, !Small, mirror
    dw !EmptySmall, !Small, glove
    dw !EmptySmall, !Small, mitt
    dw !EmptySmall, !Small, boots
    dw !EmptySmall, !Small, flippers
    dw !EmptySmall, !Small, pearl
    dw !EmptySmall, !Small, sword_master
    dw !EmptySmall, !Small, sword_tempered
    dw !EmptySmall, !Small, sword_gold
    dw !EmptySmall, !Small, tunic_blue
    dw !EmptySmall, !Small, tunic_red
    dw !EmptySmall, !Small, shield
    dw !EmptySmall, !Small, shield_red
    dw !EmptySmall, !Small, shield_mirror
    dw !EmptySmall, !Small, heart_piece
    dw !EmptySmall, !Small, heart_container
    dw !EmptySmall, !Small, arrow_1
    dw !EmptySmall, !Small, arrows_5
    dw !EmptySmall, !Small, arrows_10
    dw !EmptySmall, !Small, bombs_3
    dw !EmptySmall, !Small, bombs_10
    dw !EmptySmall, !Small, rupee_1
    dw !EmptySmall, !Small, rupees_5
    dw !EmptySmall, !Small, rupees_20
    dw !EmptySmall, !Small, rupees_50
    dw !EmptySmall, !Small, rupees_100
    dw !EmptySmall, !Small, rupees_300
    dw !EmptySmall, !Small, magic_half
    dw !EmptySmall, !Small, magic_quarter
    dw !EmptySmall, !Small, bomb_upgrade_5
    dw !EmptySmall, !Small, bomb_upgrade_10
    dw !EmptySmall, !Small, arrow_upgrade_5
    dw !EmptySmall, !Small, arrow_upgrade_10
    dw !EmptySmall, !Small, sword_fighter
    dw !PlaceholderBig, !Big, sm_item_sent
    dw !PlaceholderBig, !Big, sm_item_received

    dw !DungeonItemBig,    !Big, map           ; 5E
    dw !DungeonItemBig,    !Big, compass       ; 5F
    dw !DungeonItemBig,    !Big, big_key       ; 60
    dw !DungeonKeyItemBig, !Big, small_key     ; 61 
    dw !KeycardBig,        !Big, keycard       ; 62

    dw !EmptySmall, !Small, btn_array

table box.tbl,rtl
    ;   0                              31
bow:
    dw "______        Bow        _______"
silver_arrows:
    dw "______   Silver Arrows   _______"
blue_boomerang:
    dw "______  Blue Boomerang   _______"
red_boomerang:
    dw "______   Red Boomerang   _______"
hookshot:
    dw "______     Hookshot      _______"
bomb_1:
    dw "______      1 Bomb       _______"
mushroom:
    dw "______     Mushroom      _______"
powder:
    dw "______    Magic Powder   _______"
firerod:
    dw "______     Fire Rod      _______"
icerod:
    dw "______     Ice Rod       _______"
bombos:
    dw "______      Bombos       _______"
ether:
    dw "______      Ether        _______"
quake:
    dw "______      Quake        _______"
lamp:
    dw "______       Lamp        _______"
hammer:
    dw "______      Hammer       _______"
shovel:
    dw "______      Shovel       _______"
flute:
    dw "______      Flute        _______"
net:
    dw "______ Bug-catching Net  _______"
book:
    dw "______  Book of Mudora   _______"
bottle_empty:
    dw "______      Bottle       _______"
bottle_red:
    dw "______    Red Potion     _______"
bottle_green:
    dw "______   Green Potion    _______"
bottle_blue:
    dw "______   Blue Potion     _______"
bottle_bee:
    dw "______       Bee         _______"
bottle_good_bee:
    dw "______     Good Bee      _______"
bottle_fairy:
    dw "______      Fairy        _______"
somaria:
    dw "______  Cane of Somaria  _______"
byrna:
    dw "______   Cane of Byrna   _______"
cape:
    dw "______    Magic Cape     _______"
mirror:
    dw "______      Mirror       _______"
glove:
    dw "______    Power Glove    _______"
mitt:
    dw "______   Titan's Mitt    _______"
boots:
    dw "______   Pegasus Boots   _______"
flippers:
    dw "______  Zora's Flippers  _______"
pearl:
    dw "______    Moon Pearl     _______"
sword_master:
    dw "______   Master Sword    _______"
sword_tempered:
    dw "______  Tempered Sword   _______"
sword_gold:
    dw "______    Gold Sword     _______"
tunic_blue:
    dw "______     Blue Mail     _______"
tunic_red:
    dw "______      Red Mail     _______"
shield:
    dw "______   Small Shield    _______"
shield_red:
    dw "______    Red Shield     _______"
shield_mirror:
    dw "______   Mirror Shield   _______"
heart_piece:
    dw "______   Heart Piece     _______"
heart_container:
    dw "______  Heart Container  _______"
arrow_1:
    dw "______     1 Arrow       _______"
arrows_5:
    dw "______     5 Arrows      _______"
arrows_10:
    dw "______     10 Arrows     _______"
bombs_3:
    dw "______      3 Bombs      _______"
bombs_10:
    dw "______     10 Bombs      _______"
rupee_1:
    dw "______      1 Rupee      _______"
rupees_5:
    dw "______     5 Rupees      _______"
rupees_20:
    dw "______     20 Rupees     _______"
rupees_50:
    dw "______     50 Rupees     _______"
rupees_100:
    dw "______    100 Rupees     _______"
rupees_300:
    dw "______    300 Rupees     _______"
magic_half:
    dw "______    Half Magic     _______"
magic_quarter:
    dw "______   Quarter Magic   _______"
bomb_upgrade_5:
    dw "______  5 Bomb capacity  _______"
bomb_upgrade_10:
    dw "______ 10 Bomb capacity  _______"
arrow_upgrade_5:
    dw "______  5 Arrow capacity _______"
arrow_upgrade_10:
    dw "______ 10 Arrow capacity _______"
sword_fighter:
    dw "______  Fighter's Sword  _______"

sm_item_sent:
    dw "___         You found        ___"
    dw "___      ITEM NAME HERE      ___"
    dw "___           for            ___"
    dw "___          PLAYER          ___"

sm_item_received:
    dw "___       You received       ___"
    dw "___      ITEM NAME HERE      ___"
    dw "___           from           ___"
    dw "___          PLAYER          ___"

map:
    dw "___     This is the map      ___"
    dw "___           for            ___"
    dw "___                          ___"
    dw "___          DUNGEON         ___"

compass:
    dw "___   This is the compass    ___"
    dw "___           for            ___"
    dw "___                          ___"
    dw "___          DUNGEON         ___"

big_key:
    dw "___    This is the big key   ___"
    dw "___           for            ___"
    dw "___                          ___"
    dw "___          DUNGEON         ___"

small_key:
    dw "___    This is a small key   ___"
    dw "___           for            ___"
    dw "___                          ___"
    dw "___          DUNGEON         ___"

keycard:
    dw "___       This is the        ___"
    dw "___         KEYCARD          ___"
    dw "___           for            ___"
    dw "___          REGION          ___"



cleartable

btn_array:
	DW $0000, $012A, $012A, $012C, $012C, $012C, $0000, $0000, $0000, $0000, $0000, $0000, $0120, $0000, $0000
	DW $0000, $0000, $0000, $012A, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000
    DW $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000
    DW $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000
    DW $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000
    DW $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000
    DW $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000
    DW $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000
    DW $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000
    DW $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000
    DW $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000
    DW $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000
    DW $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000, $0000

table box_yellow.tbl,rtl
item_names:
    dw "___      Grappling Beam      ___" ; 00 (b0) (sm items)
    dw "___       X-Ray Scope        ___"
    dw "___        Varia Suit        ___"
    dw "___       Spring Ball        ___"
    dw "___       Morphing Ball      ___"
    dw "___       Screw Attack       ___"
    dw "___       Gravity Suit       ___"
    dw "___       Hi-Jump Boots      ___"
    dw "___        Space Jump        ___"
    dw "___          Bombs           ___"
    dw "___       Speed Booster      ___"
    dw "___       Charge Beam        ___"
    dw "___         Ice Beam         ___"
    dw "___        Wave Beam         ___"
    dw "___     ~ S p A z E r ~      ___"
    dw "___       Plasma Beam        ___" ; 0f
    
    dw "___      An Energy Tank      ___" ; 10
    dw "___      A Reserve Tank      ___"
    dw "___         Missiles         ___"
    dw "___       Super Missiles     ___"
    dw "___        Power Bombs       ___"

    dw "___                          ___"  ;15
    dw "___                          ___"  ;16
    dw "___                          ___"  ;17
    dw "___                          ___"  ;18
    dw "___                          ___"  ;19
    dw "___                          ___"  ;1A
    dw "___                          ___"  ;1B
    dw "___                          ___"  ;1C
    dw "___                          ___"  ;1D
    dw "___                          ___"  ;1E
    dw "___                          ___"  ;1F

    dw "___   Crateria L 1 Keycard   ___" ; 20
    dw "___   Crateria L 2 Keycard   ___" ; 21
    dw "___  Crateria Boss Keycard   ___" ; 22
    dw "___   Brinstar L 1 Keycard   ___" ; 23
    dw "___   Brinstar L 2 Keycard   ___" ; 24
    dw "___  Brinstar Boss Keycard   ___" ; 25
    dw "___    Norfair L 1 Keycard   ___" ; 26
    dw "___    Norfair L 2 Keycard   ___" ; 27
    dw "___   Norfair Boss Keycard   ___" ; 28
    dw "___    Maridia L 1 Keycard   ___" ; 28
    dw "___    Maridia L 2 Keycard   ___" ; 2A
    dw "___   Maridia Boss Keycard   ___" ; 2B
    dw "___ Wrecked Ship L 1 Keycard ___" ; 2C
    dw "___ Wrecked Ship Boss Keycard___" ; 2D
    dw "___Lower Norfair L 1 Keycard ___" ; 2E
    dw "___Lower Norfair Boss Keycard___" ; 2F

    dw "___                          ___"       ; $30+ (alttp items)
    dw "___       Master Sword       ___"
    dw "___      Tempered Sword      ___"
    dw "___        Gold Sword        ___"
    dw "___       Small Shield       ___"
    dw "___        Red Shield        ___"
    dw "___       Mirror Shield      ___"
    dw "___         Fire Rod         ___"
    dw "___         Ice Rod          ___"
    dw "___          Hammer          ___"
    dw "___         Hookshot         ___"
    dw "___           Bow            ___"
    dw "___      Blue Boomerang      ___"
    dw "___       Magic Powder       ___"
    dw "___                          ___"
    dw "___          Bombos          ___"
    dw "___          Ether           ___"
    dw "___          Quake           ___"
    dw "___           Lamp           ___"
    dw "___          Shovel          ___"
    dw "___          Flute           ___"
    dw "___      Cane of Somaria     ___"
    dw "___          Bottle          ___"
    dw "___       Heart Piece        ___"
    dw "___       Cane of Byrna      ___"
    dw "___        Magic Cape        ___"
    dw "___          Mirror          ___"
    dw "___        Power Glove       ___"
    dw "___       Titan's Mitt       ___"
    dw "___      Book of Mudora      ___"
    dw "___      Zora's Flippers     ___"
    dw "___        Moon Pearl        ___"
    dw "___                          ___"
    dw "___     Bug-catching Net     ___"
    dw "___         Blue Mail        ___"
    dw "___          Red Mail        ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___      Heart Container     ___"
    dw "___          1 Bomb          ___"
    dw "___          3 Bombs         ___"
    dw "___         Mushroom         ___"
    dw "___       Red Boomerang      ___"
    dw "___        Red Potion        ___"
    dw "___       Green Potion       ___"
    dw "___       Blue Potion        ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___         10 Bombs         ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___          1 Rupee         ___"
    dw "___         5 Rupees         ___"
    dw "___         20 Rupees        ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___           Bow            ___"
    dw "___       Silver Arrows      ___"
    dw "___           Bee            ___"
    dw "___          Fairy           ___"
    dw "___      Heart Container     ___"
    dw "___      Heart Container     ___"
    dw "___        100 Rupees        ___"
    dw "___         50 Rupees        ___"
    dw "___                          ___"
    dw "___         1 Arrow          ___"
    dw "___         10 Arrows        ___"
    dw "___                          ___"
    dw "___        300 Rupees        ___"
    dw "___         20 Rupees        ___"
    dw "___         Good Bee         ___"
    dw "___      Fighter's Sword     ___"
    dw "___                          ___"
    dw "___       Pegasus Boots      ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___        Half Magic        ___"
    dw "___       Quarter Magic      ___"
    dw "___       Master Sword       ___"
    dw "___      5 Bomb capacity     ___"
    dw "___     10 Bomb capacity     ___"
    dw "___      5 Arrow capacity    ___"
    dw "___     10 Arrow capacity    ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___       Silver arrows      ___" ; 58
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___       Sword Upgrade      ___"
    dw "___       Shield Uprade      ___"
    
    dw "___      Armour Upgrade      ___"  ; 60
    dw "___       Glove Upgrade      ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___"
    dw "___                          ___" ; 6F

    dw "___                          ___" ; 70
    dw "___                          ___"
    dw "___     Ganon's Tower Map    ___"
    dw "___     Turtle Rock Map      ___"
    dw "___     Thieves' Town Map    ___"
    dw "___    Tower of Hera Map     ___"
    dw "___      Ice Palace Map      ___"
    dw "___      Skull Woods Map     ___"
    dw "___      Misery Mire Map     ___"
    dw "___  Palace of Darkness Map  ___"
    dw "___      Swamp Palace Map    ___"
    dw "___                          ___"
    dw "___     Desert Palace Map    ___"
    dw "___    Eastern Palace Map    ___"
    dw "___                          ___"
    dw "___     HYRULE CASTLE Map    ___" ; 7F

    dw "___                          ___" ; 80
    dw "___                          ___"
    dw "___   Ganon's Tower Compass  ___"
    dw "___   Turtle Rock Compass    ___"
    dw "___   Thieves' Town Compass  ___"
    dw "___  Tower of Hera Compass   ___"
    dw "___    Ice Palace Compass    ___"
    dw "___    Skull Woods Compass   ___"
    dw "___    Misery Mire Compass   ___"
    dw "___Palace of Darkness Compass___"
    dw "___    Swamp Palace Compass  ___"
    dw "___                          ___"
    dw "___   Desert Palace Compass  ___"
    dw "___  Eastern Palace Compass  ___"
    dw "___                          ___"
    dw "___                          ___" ; 8F

    dw "___                          ___" ; 90
    dw "___                          ___"
    dw "___   Ganon's Tower Big Key  ___"
    dw "___   Turtle Rock Big Key    ___"
    dw "___  Thieves' Town Big Key   ___"
    dw "___  Tower of Hera Big Key   ___"
    dw "___    Ice Palace Big Key    ___"
    dw "___    Skull Woods Big Key   ___"
    dw "___    Misery Mire Big Key   ___"
    dw "___Palace of Darkness Big Key___"
    dw "___   Swamp Palace Big Key   ___"
    dw "___                          ___"
    dw "___   Desert Palace Big Key  ___"
    dw "___  Eastern Palace Big Key  ___"
    dw "___                          ___"
    dw "___                          ___" ; 9F    

    dw "___     Hyrule Castle Key    ___" ; A0
    dw "___        Sewers Key        ___"
    dw "___    Eastern Palace Key    ___"
    dw "___     Desert Palace Key    ___"
    dw "___      Castle Tower Key    ___"
    dw "___      Swamp Palace Key    ___"
    dw "___  Palace of Darkness Key  ___"
    dw "___      Misery Mire Key     ___"
    dw "___      Skull Woods Key     ___"
    dw "___       Ice Palace Key     ___"
    dw "___     Tower of Hera Key    ___"
    dw "___     Thieves' Town Key    ___"
    dw "___      Turtle Rock Key     ___"
    dw "___      Ganon's Tower Key   ___"
    dw "___                          ___"
    dw "___                          ___" ; AF    

dungeon_names:
    dw "___          UNUSED          ___" ; 0
    dw "___          UNUSED          ___" ; 1
    dw "___       Ganon's Tower      ___" ; 2
    dw "___       Turtle Rock        ___" ; 3
    dw "___       Thieves' Town      ___" ; 4
    dw "___      Tower of Hera       ___" ; 5
    dw "___        Ice Palace        ___" ; 6
    dw "___       Skull Woods        ___" ; 7
    dw "___       Misery Mire        ___" ; 8
    dw "___    Palace of Darkness    ___" ; 9
    dw "___       Swamp Palace       ___" ; A
    dw "___          UNUSED          ___" ; B
    dw "___       Desert Palace      ___" ; C
    dw "___      Eastern Palace      ___" ; D
    dw "___          UNUSED          ___" ; E
    dw "___       Hyrule Castle      ___" ; F

dungeon_names_key:
    dw "___       Hyrule Castle      ___" ; 0
    dw "___          UNUSED          ___" ; 1
    dw "___          UNUSED          ___" ; 2
    dw "___       Desert Palace      ___" ; 3
    dw "___       Castle Tower       ___" ; 4
    dw "___       Swamp Palace       ___" ; 5
    dw "___    Palace of Darkness    ___" ; 6
    dw "___       Misery Mire        ___" ; 7
    dw "___       Skull Woods        ___" ; 8
    dw "___        Ice Palace        ___" ; 9
    dw "___      Tower of Hera       ___" ; A
    dw "___       Thieves' Town      ___" ; B
    dw "___       Turtle Rock        ___" ; C
    dw "___       Ganon's Tower      ___" ; D
    dw "___          UNUSED          ___" ; E
    dw "___          UNUSED          ___" ; F

region_names:
    dw "___         Crateria         ___" ; 0
    dw "___         Crateria         ___" ; 1
    dw "___         Crateria         ___" ; 2
    dw "___         Brinstar         ___" ; 3
    dw "___         Brinstar         ___" ; 4
    dw "___         Brinstar         ___" ; 5
    dw "___         Norfair          ___" ; 6
    dw "___         Norfair          ___" ; 7
    dw "___         Norfair          ___" ; 8
    dw "___         Maridia          ___" ; 9
    dw "___         Maridia          ___" ; A
    dw "___         Maridia          ___" ; B
    dw "___       Wrecked Ship       ___" ; C
    dw "___       Wrecked Ship       ___" ; D
    dw "___      Lower Norfair       ___" ; E
    dw "___      Lower Norfair       ___" ; F

keycard_names:
    dw "___     Level 1 Keycard      ___" ; 0
    dw "___     Level 2 Keycard      ___" ; 1
    dw "___       Boss Keycard       ___" ; 2
    dw "___     Level 1 Keycard      ___" ; 3
    dw "___     Level 2 Keycard      ___" ; 4
    dw "___       Boss Keycard       ___" ; 5
    dw "___     Level 1 Keycard      ___" ; 6
    dw "___     Level 2 Keycard      ___" ; 7
    dw "___       Boss Keycard       ___" ; 8
    dw "___     Level 1 Keycard      ___" ; 9
    dw "___     Level 2 Keycard      ___" ; A
    dw "___       Boss Keycard       ___" ; B
    dw "___     Level 1 Keycard      ___" ; C
    dw "___       Boss Keycard       ___" ; D
    dw "___     Level 1 Keycard      ___" ; E
    dw "___       Boss Keycard       ___" ; F


cleartable

EmptyBig:
	REP #$30
    LDY #$0000
	JMP $841D
PlaceholderBig:
    REP #$30
    JSR write_placeholders
    LDY #$0000
    JMP $841D
DungeonItemBig:
    REP #$30
    JSR write_dungeon
    LDY #$0000
    JMP $841D
DungeonKeyItemBig:
    REP #$30
    JSR write_dungeon_key
    LDY #$0000
    JMP $841D
KeycardBig:
    REP #$30
    JSR write_keycard
    LDY #$0000
    JMP $841D

write_dungeon:
    phx : phy
    lda $1c1f
    cmp #$005e
    beq .adjust
    cmp #$005f
    beq .adjust
    cmp #$0060
    beq .adjust
    bra .end

.adjust
    lda $c7                ; Load dungeon id
    asl #6 : tay
    ldx #$0000
-
    lda dungeon_names, y
    sta.l $7e3300, x
    inx #2 : iny #2
    cpx #$0040
    bne -

.end
    ply : plx
    lda #$0020
    rts

write_dungeon_key:
    phx : phy
    lda $1c1f
    cmp #$0061
    beq .adjust
    bra .end

.adjust
    lda $c7                ; Load dungeon id
    asl #6 : tay
    ldx #$0000
-
    lda dungeon_names_key, y
    sta.l $7e3300, x
    inx #2 : iny #2
    cpx #$0040
    bne -

.end
    ply : plx
    lda #$0020
    rts

write_keycard:
    phx : phy
    lda $1c1f
    cmp #$0062
    beq .adjust
    bra .end

.adjust
    lda $c7                ; Load keycard index
    asl #6 : tay
    phy

    ldx #$0000
-
    lda keycard_names, y
    sta.l $7e3280, x
    inx #2 : iny #2
    cpx #$0040
    bne -

    ply
    ldx #$0000
-
    lda region_names, y
    sta.l $7e3300, x
    inx #2 : iny #2
    cpx #$0040
    bne -

.end
    ply : plx
    lda #$0020
    rts

write_placeholders:
    phx : phy
    lda $1c1f
    cmp #$005c
    beq .adjust
    cmp #$005d
    beq .adjust
    bra .end

.adjust
    lda $c1                 ; Load item id
    cmp #$00b0              
    bcc .alttpItem
    sec
    sbc #$00b0
    bra +
.alttpItem
    clc
    adc #$0030
+
    asl #6 : tay
    ldx #$0000
-
    lda item_names, y       ; Write item name to box
    sta.l $7e3280, x
    inx #2 : iny #2
    cpx #$0040
    bne -

    lda $c3                 ; Load player 1
    asl #4 : tax
    ldy #$0000
-
    lda.l rando_player_table, x
    and #$00ff
    phx
    asl : tax               ; Put char table offset in X
    lda char_table-$40, x 
    tyx
    sta.l $7e3314, x
    iny #2
    plx
    inx
    cpy #$0018
    bne -
    rep #$30

.end
    ply : plx
    lda #$0020
    rts

char_table:
    ; Each unsupported value translate to "?" $38FE to raise a visual indication
    ;  <sp>   !      "      #      $      %      &      '      (      )      *      +      ,      -      .      /
    dw $384E, $38FF, $38AA, $38AE, $38FE, $380A, $38FE, $38FC, $38FE, $38FE, $38FE, $38AF, $38FB, $38CF, $38FA, $38FE
    ;  0      1      2      3      4      5      6      7      8      9      :      ;      <      =      >      ?
    dw $3889, $3880, $3881, $3882, $3883, $3884, $3885, $3886, $3887, $3888, $38AB, $38FE, $38FE, $38FE, $38FE, $38FE
    ;  @      A      B      C      D      E      F      G      H      I      J      K      L      M      N      O
    dw $38FE, $38E0, $38E1, $38E2, $38E3, $38E4, $38E5, $38E6, $38E7, $38E8, $38E9, $38EA, $38EB, $38EC, $38ED, $38EE
    ;  P      Q      R      S      T      U      V      W      X      Y      Z      [      \      ]      ^      _
    dw $38EF, $38F0, $38F1, $38F2, $38F3, $38F4, $38F5, $38F6, $38F7, $38F8, $38F9, $38FE, $38FE, $38FE, $38FE, $38FE

; Lowercase Letters, which simply translate to uppercase
    ;  `      A      B      C      D      E      F      G      H      I      J      K      L      M      N      O
    dw $38FE, $3890, $3891, $3892, $3893, $3894, $3895, $3896, $3897, $3898, $3899, $389A, $389B, $389C, $389D, $389E
    ;  P      Q      R      S      T      U      V      W      X      Y      Z      {      |      }      ~      <DEL>
    dw $389F, $38A0, $38A1, $38A2, $38A3, $38A4, $38A5, $38A6, $38A7, $38A8, $38A9, $38FE, $38FE, $38FE, $38AC, $38FE

org $c58749
base $858749
fix_1c1f:
    LDA $CE     ; if $CE is set, it overrides the message box
    BEQ +
    STA $1C1F
    STZ $CE     ; Clear $CE
+	LDA $1C1F
	CMP #$001D
	BPL +
	RTS
+
	ADC #$027F
	RTS

org $c58243
base $858243
	JSR fix_1c1f

org $c582E5
base $8582E5
	JSR fix_1c1f

org $c58413
base $858413
	DW btn_array

namespace off


; ; Patch item PLM's
; org $CF81CC
; 	dw $efe0	;Power Bomb (Crateria surface)
; org $CF81E8
; 	dw $efe0	;Missile (outside Wrecked Ship bottom)
; org $CF81EE
; 	dw $efe8	;Missile (outside Wrecked Ship top)
; org $CF81F4
; 	dw $efe0	;Missile (outside Wrecked Ship middle)
; org $CF8248
; 	dw $efe0	;Missile (Crateria moat)
; org $CF8264
; 	dw $efe0	;Energy Tank (Crateria gauntlet)
; org $CF83EE
; 	dw $efe0	;Missile (Crateria bottom)
; org $CF8404
; 	dw $efe4	;Bomb
; org $CF8432
; 	dw $efe0	;Energy Tank (Crateria tunnel to Brinstar)
; org $CF8464
; 	dw $efe0	;Missile (Crateria gauntlet right)
; org $CF846A
; 	dw $efe0	;Missile (Crateria gauntlet left)
; org $CF8478
; 	dw $efe0	;Super Missile (Crateria)
; org $CF8486
; 	dw $efe0	;Missile (Crateria middle)
; org $CF84AC
; 	dw $efe4	;Power Bomb (green Brinstar bottom)
; org $CF84E4
; 	dw $efe4	;Super Missile (pink Brinstar)
; org $CF8518
; 	dw $efe0	;Missile (green Brinstar below super missile)
; org $CF851E
; 	dw $efe0	;Super Missile (green Brinstar top)
; org $CF852C
; 	dw $efe4	;Reserve Tank (Brinstar)
; org $CF8532
; 	dw $efe8	;Missile (green Brinstar behind missile)
; org $CF8538
; 	dw $efe0	;Missile (green Brinstar behind Reserve Tank)
; org $CF8608
; 	dw $efe0	;Missile (pink Brinstar top)
; org $CF860E
; 	dw $efe0	;Missile (pink Brinstar bottom)
; org $CF8614
; 	dw $efe4	;Charge Beam
; org $CF865C
; 	dw $efe0	;Power Bomb (pink Brinstar)
; org $CF8676
; 	dw $efe0	;Missile (green Brinstar pipe)
; org $CF86DE
; 	dw $efe0	;Morphing Ball
; org $CF874C
; 	dw $efe0	;Power Bomb (blue Brinstar)
; org $CF8798
; 	dw $efe0	;Missile (blue Brinstar middle)
; org $CF879E
; 	dw $efe8	;Energy Tank (blue Brinstar)
; org $CF87C2
; 	dw $efe0	;Energy Tank (green Brinstar bottom)
; org $CF87D0
; 	dw $efe0	;Super Missile (green Brinstar bottom)
; org $CF87FA
; 	dw $efe0	;Energy Tank (pink Brinstar bottom)
; org $CF8802
; 	dw $efe4	;Missile (blue Brinstar bottom)
; org $CF8824
; 	dw $efe0	;Energy Tank (pink Brinstar top)
; org $CF8836
; 	dw $efe0	;Missile (blue Brinstar top)
; org $CF883C
; 	dw $efe8	;Missile (blue Brinstar behind missile)
; org $CF8876
; 	dw $efe4	;X-Ray Visor
; org $CF88CA
; 	dw $efe0	;Power Bomb (red Brinstar sidehopper room)
; org $CF890E
; 	dw $efe4	;Power Bomb (red Brinstar spike room)
; org $CF8914
; 	dw $efe0	;Missile (red Brinstar spike room)
; org $CF896E
; 	dw $efe4	;Spazer
; org $CF899C
; 	dw $efe8	;Energy Tank (Kraid)
; org $CF89EC
; 	dw $efe8	;Missile (Kraid)
; org $CF8ACA
; 	dw $efe4	;Varia Suit
; org $CF8AE4
; 	dw $efe8	;Missile (lava room)
; org $CF8B24
; 	dw $efe4	;Ice Beam
; org $CF8B46
; 	dw $efe8	;Missile (below Ice Beam)
; org $CF8BA4
; 	dw $efe0	;Energy Tank (Crocomire)
; org $CF8BAC
; 	dw $efe4	;Hi-Jump Boots
; org $CF8BC0
; 	dw $efe0	;Missile (above Crocomire)
; org $CF8BE6
; 	dw $efe0	;Missile (Hi-Jump Boots)
; org $CF8BEC
; 	dw $efe0	;Energy Tank (Hi-Jump Boots)
; org $CF8C04
; 	dw $efe0	;Power Bomb (Crocomire)
; org $CF8C14
; 	dw $efe0	;Missile (below Crocomire)
; org $CF8C2A
; 	dw $efe0	;Missile (Grapple Beam)
; org $CF8C36
; 	dw $efe4	;Grapple Beam
; org $CF8C3E
; 	dw $efe4	;Reserve Tank (Norfair)
; org $CF8C44
; 	dw $efe8	;Missile (Norfair Reserve Tank)
; org $CF8C52
; 	dw $efe0	;Missile (bubble Norfair green door)
; org $CF8C66
; 	dw $efe0	;Missile (bubble Norfair)
; org $CF8C74
; 	dw $efe8	;Missile (Speed Booster)
; org $CF8C82
; 	dw $efe4	;Speed Booster
; org $CF8CBC
; 	dw $efe0	;Missile (Wave Beam)
; org $CF8CCA
; 	dw $efe4	;Wave Beam
; org $CF8E6E
; 	dw $efe0	;Missile (Gold Torizo)
; org $CF8E74
; 	dw $efe8	;Super Missile (Gold Torizo)
; org $CF8F30
; 	dw $efe0	;Missile (Mickey Mouse room)
; org $CF8FCA
; 	dw $efe0	;Missile (lower Norfair above fire flea room)
; org $CF8FD2
; 	dw $efe0	;Power Bomb (lower Norfair above fire flea room)
; org $CF90C0
; 	dw $efe0	;Power Bomb (above Ridley)
; org $CF9100
; 	dw $efe0	;Missile (lower Norfair near Wave Beam)
; org $CF9108
; 	dw $efe8	;Energy Tank (Ridley)
; org $CF9110
; 	dw $efe4	;Screw Attack
; org $CF9184
; 	dw $efe0	;Energy Tank (lower Norfair fire flea room)
; org $CFC265
; 	dw $efe0	;Missile (Wrecked Ship middle)
; org $CFC2E9
; 	dw $efe4	;Reserve Tank (Wrecked Ship)
; org $CFC2EF
; 	dw $efe0	;Missile (Gravity Suit)
; org $CFC319
; 	dw $efe0	;Missile (Wrecked Ship top)
; org $CFC337
; 	dw $efe0	;Energy Tank (Wrecked Ship)
; org $CFC357
; 	dw $efe0	;Super Missile (Wrecked Ship left)
; org $CFC365
; 	dw $efe0	;Super Missile (Wrecked Ship right)
; org $CFC36D
; 	dw $efe4	;Gravity Suit
; org $CFC437
; 	dw $efe0	;Missile (green Maridia shinespark)
; org $CFC43D
; 	dw $efe0	;Super Missile (green Maridia)
; org $CFC47D
; 	dw $efe0	;Energy Tank (green Maridia)
; org $CFC483
; 	dw $efe8	;Missile (green Maridia tatori)
; org $CFC4AF
; 	dw $efe0	;Super Missile (yellow Maridia)
; org $CFC4B5
; 	dw $efe0	;Missile (yellow Maridia super missile)
; org $CFC533
; 	dw $efe0	;Missile (yellow Maridia false wall)
; org $CFC559
; 	dw $efe4	;Plasma Beam
; org $CFC5DD
; 	dw $efe0	;Missile (left Maridia sand pit room)
; org $CFC5E3
; 	dw $efe4	;Reserve Tank (Maridia)
; org $CFC5EB
; 	dw $efe0	;Missile (right Maridia sand pit room)
; org $CFC5F1
; 	dw $efe0	;Power Bomb (right Maridia sand pit room)
; org $CFC603
; 	dw $efe0	;Missile (pink Maridia)
; org $CFC609
; 	dw $efe0	;Super Missile (pink Maridia)
; org $CFC6E5
; 	dw $efe4	;Spring Ball
; org $CFC74D
; 	dw $efe8	;Missile (Draygon)
; org $CFC755
; 	dw $efe0	;Energy Tank (Botwoon)
; org $CFC7A7
; 	dw $efe4	;Space Jump
