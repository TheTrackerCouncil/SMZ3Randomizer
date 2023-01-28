!ADD = "CLC : ADC"
!SUB = "SEC : SBC"
!BLT = "BCC"
!BGE = "BCS"

!INVENTORY_SWAP = "$7EF38C"
!INVENTORY_SWAP_2 = "$7EF38E"
!NPC_FLAGS   = "$7EF410"
!NPC_FLAGS_2 = "$7EF411"
!MAP_OVERLAY = "$7EF414" ; [2]
!PROGRESSIVE_SHIELD = "$7EF416" ; ss-- ----
!HUD_FLAG = "$7EF416" ; --h- ----
!FORCE_PYRAMID = "$7EF416" ; ---- p---
!IGNORE_FAIRIES = "$7EF416" ; ---- -i--
!SHAME_CHEST = "$7EF416" ; ---s ----
!HAS_GROVE_ITEM = "$7EF416" ; ---- ---g general flags, don't waste these
!HIGHEST_SWORD_LEVEL = "$7EF417" ; --- -sss
!SRAM_SINK = "$7EF41E" ; <- change this
!FRESH_FILE_MARKER = "$7EF4F0" ; zero if fresh file

!MS_GOT = "$7F5031"
!DARK_WORLD = "$7EF3CA"

!REDRAW = "$7F5000"
!GANON_WARP_CHAIN = "$7F5032";

!TILE_UPLOAD_BUFFER = "$7EA180";

!FORCE_HEART_SPAWN = "$7F5033";
!SKIP_HEART_SAVE = "$7F5034";

!INVENTORY_SWAP = "$7EF38C"
!INVENTORY_SWAP_2 = "$7EF38E"

!ITEM_LIMIT_COUNTS = "$7EF390"
!SHOP_PURCHASE_COUNTS = "$7EF3A0"

!MULTIWORLD_GIVE_INDEX = $19EE

!MULTIWORLD_SWAP = $19F0
!MULTIWORLD_PICKUP = $19F2

!MULTIWORLD_GIVE_PLAYER = $19F4
!MULTIWORLD_GIVE_ITEM = $19F6

!MULTIWORLD_DIALOG = $19F8
!MULTIWORLD_DIALOG_ITEM = $19FA
!MULTIWORLD_DIALOG_PLAYER = $19FC

!MULTIWORLD_DELAY = $19FE

incsrc hooks.asm
incsrc treekid.asm
incsrc spriteswap.asm
incsrc sharedplayerpalettefix.asm
incsrc fairyfountainrooms.asm

org $cae980             ; Free space in SM bank 8a
base $8ae980
incsrc newitems.asm
warnpc $8affff

org $f88000            ; Bank B8 (Free space in SM)
base $b88000           
incsrc itemdowngrade.asm
incsrc bugfixes.asm
incsrc darkworldspawn.asm
incsrc lampmantlecone.asm
incsrc floodgatesoftlock.asm
incsrc heartpieces.asm
incsrc npcitems.asm
incsrc utilities.asm
incsrc flipperkill.asm
incsrc goalitem.asm
incsrc pendantcrystalhud.asm
incsrc potions.asm
incsrc shopkeeper.asm
incsrc bookofmudora.asm
incsrc tablets.asm
incsrc rupeelimit.asm
incsrc fairyfixes.asm
incsrc medallions.asm
incsrc inventory.asm
incsrc zelda.asm
incsrc maidencrystals.asm
incsrc zoraking.asm
incsrc catfish.asm
incsrc flute.asm
incsrc dungeondrops.asm
incsrc halfmagicbat.asm
incsrc mantle.asm
incsrc swordswap.asm
incsrc map.asm
incsrc dialog.asm
incsrc events.asm
incsrc entrances.asm
incsrc heartbeep.asm
incsrc capacityupgrades.asm
incsrc doorframefixes.asm
incsrc z3_msu.asm
incsrc music.asm
incsrc roomloading.asm
incsrc icepalacegraphics.asm
incsrc rngfixes.asm
incsrc openmode.asm
incsrc stats.asm
incsrc textskip_functions.asm
incsrc itemtext.asm
incsrc textrenderer.asm
incsrc quickswap.asm
warnpc $b8ffff

org $1C8000 ; text tables for translation
incbin data/i18n_en.bin
warnpc $1CF356

org $c7fa00
base $87fa00
incsrc framehook.asm

org $c7fb00
base $87fb00
incsrc hud.asm

org $c7ff00
base $87ff00
incsrc init.asm

org $400000            ; Bank 40
incsrc tables.asm

org $500000
GFX_New_Items:
incbin data/newitems.gfx
warnpc $504000

org $504000
GFX_SM_Items:
incbin data/newitems_sm.gfx
warnpc $508000


org $510000 ; bank #$31
GFX_Mire_Bombos:
incbin data/99ff1_bombos.gfx
warnpc $510800

org $510800
GFX_Mire_Quake:
incbin data/99ff1_quake.gfx
warnpc $511000

org $511000
GFX_TRock_Bombos:
incbin data/a6fc4_bombos.gfx
warnpc $511800

org $511800
GFX_TRock_Ether:
incbin data/a6fc4_ether.gfx
warnpc $512000

org $512000
GFX_HUD_Items:
incbin data/c2807_v3.gfx
warnpc $513000

org $513000
GFX_HUD_Main:
incbin data/c2e3e.gfx
warnpc $513800

org $513800
GFX_Hash_Alphabet:
incbin data/hashalphabet.chr.gfx
warnpc $514001

org $514000
IcePalaceFloorGfx:
incbin data/ice_palace_floor.bin
warnpc $51C801

org $530000
GFX_HUD_Palette:
incbin data/hudpalette.pal

;org $520000
;Extra_Text_Table:
;incsrc itemtext.asm

;================================================================================
org $119100 ; PC 0x89100
incbin data/map_icons.gfx
warnpc $119401

org $5f0000
incsrc eventdata.asm

;================================================================================

;================================================================================
;Bank Map
;$20 Code Bank
;$21 Reserved (Frame Hook & Init)
;$22 Contrib Code
;$23 Stats & Credits
;$24 Code Bank
;$29 External hooks (rest of bank not used)
;$2E Reserved for Tournament Use
;$2F Static RNG (rest is reserved for tournament use)
;$30 Main Configuration Table
;$31 Graphics Bank
;$32 Text Bank
;$33 Graphics Bank
;$37 Don't Use ZSNES Graphics
;$38 Don't Use ZSNES Graphics (continued)
;$3A reserved for downstream use
;$3B reserved for downstream use
;$3F reserved for internal debugging
;$7F5700 - $7F57FF reserved for downstream use
;================================================================================
;org $0080DC ; <- 0xDC - Bank00.asm:179 - Kill Music
;db #$A9, #$00, #$EA
;LDA.b #$00 : NOP
;================================================================================
;org $0AC53E ; <- 5453E - Bank0A.asm:1103 - (LDA $0AC51F, X) - i have no idea what this is for anymore
;LDA.b #$7F
;NOP #2
;================================================================================
;org $05DF8B ; <- 2DF8B - Bank05.asm : 2483
;AND.w #$0100 ; allow Sprite_DrawMultiple to access lower half of sprite tiles
;================================================================================
;org $0DF8F1 ; this is required for the X-indicator in the HUD except not anymore obviously
;
;;red pendant
;db $2B, $31, $2C, $31, $3D, $31, $2E, $31
;db $2B, $25, $2C, $25, $2D, $25, $2E, $25
;
;;blue pendant
;db $2B, $31, $2C, $31, $3D, $31, $2E, $31
;db $2B, $2D, $2C, $2D, $2D, $2D, $2E, $2D
;
;;green pendant
;db $2B, $31, $2C, $31, $3D, $31, $2E, $31
;db $2B, $3D, $2C, $3D, $2D, $3D, $2E, $3D
;================================================================================
;org $00CFF2 ; 0x4FF2 - Mire H
;db GFX_Mire_Bombos>>16
;org $00D0D1 ; 0x50D1 - Mire M
;db GFX_Mire_Bombos>>8
;org $00D1B0 ; 0x51B0 - Mire L
;db GFX_Mire_Bombos

;org $00D020 ; 0x5020 - Trock H
;db GFX_TRock_Bombos>>16
;org $00D0FF ; 0x50FF - Trock M
;db GFX_TRock_Bombos>>8
;org $00D1DE ; 0x51DE - Trock L
;db GFX_TRock_Bombos

org $00D09C ; 0x509C - HUD Items H
db GFX_HUD_Items>>16
org $00D17B ; 0x517B - HUD Items M
db GFX_HUD_Items>>8
org $00D25A ; 0x525A - HUD Items L
db GFX_HUD_Items

; this used to be a pointer to a dummy file
org $00D065 ; 005065 - New Items H
db GFX_New_Items>>16
org $00D144 ; 005114 - New Items M
db GFX_New_Items>>8
org $00D223 ; 005223 - New Items L
db GFX_New_Items

org $00D09D ; 0x509D - HUD Main H
db GFX_HUD_Main>>16
org $00D17C ; 0x517C - HUD Main M
db GFX_HUD_Main>>8
org $00D25B ; 0x525B - HUD Main L
db GFX_HUD_Main

;================================================================================
org $008781
UseImplicitRegIndexedLocalJumpTable:

org $00879c
UseImplicitRegIndexedLongJumpTable:

org $008333
Vram_EraseTilemaps_triforce:

org $00893D
EnableForceBlank:

org $00D308
DecompSwordGfx:

org $00D348
DecompShieldGfx:

org $00D463
Tagalong_LoadGfx:

org $00D51B
GetAnimatedSpriteTile:

org $00D52D
GetAnimatedSpriteTile_variable:

org $00E529
LoadSelectScreenGfx:

org $00F945
PrepDungeonExit:

org $00FDEE
Mirror_InitHdmaSettings:

org $01873A
Dungeon_LoadRoom:

org $02A0A8
Dungeon_SaveRoomData:

org $02A0BE
Dungeon_SaveRoomData_justKeys:

org $02B861
Dungeon_SaveRoomQuadrantData:

org $02FD8A ; 17D8A - Bank07.asm: 3732 Note: Different bank
LoadGearPalettes_bunny:

org $05A51D
Sprite_SpawnFallingItem:

org $05DF6C ; 02DF6C - Bank05.asm : 2445
Sprite_DrawMultiple:

org $05DF70 ; 02DF70 - Bank05.asm : 2454
Sprite_DrawMultiple_quantity_preset:

org $05DF75 ; 02DF75 - Bank05.asm : 2461
Sprite_DrawMultiple_player_deferred:

org $05E1A7 ; 02E1A7 - Bank05.asm : 2592
Sprite_ShowSolicitedMessageIfPlayerFacing:

org $05E1F0
Sprite_ShowMessageFromPlayerContact:

org $05E219
Sprite_ShowMessageUnconditional:

org $05FA8E
Sprite_ShowMessageMinimal:

org $05EC96
Sprite_ZeldaLong:

org $0683E6
Utility_CheckIfHitBoxesOverlapLong:

org $06A7DB
Chicken_SpawnAvengerChicken: ; returns short

org $06DC5C
Sprite_DrawShadowLong:

org $06DD40
DashKey_Draw:

org $06DBF8
Sprite_PrepAndDrawSingleLargeLong:

org $06DC00
Sprite_PrepAndDrawSingleSmallLong:

org $06EA18
Sprite_ApplySpeedTowardsPlayerLong:

org $06EAA6
Sprite_DirectionToFacePlayerLong:

org $06F12F
Sprite_CheckDamageToPlayerSameLayerLong:

org $06F86A
OAM_AllocateDeferToPlayerLong:

org $0791B3
Player_HaltDashAttackLong:

org $07999D
Link_ReceiveItem:

org $07E68F
Unknown_Method_0: ; In US version disassembly simply called "$3E6A6 IN ROM"

org $07F4AA
Sprite_CheckIfPlayerPreoccupied:

org $08C3AE
Ancilla_ReceiveItem:

org $08F710
Ancilla_SetOam_XY_Long:

org $0985E2 ; (break on $0985E4)
AddReceivedItem:

org $098BAD
AddPendantOrCrystal:

org $0993DF
AddDashTremor:

org $09AE64
Sprite_SetSpawnedCoords:

org $09AD58
GiveRupeeGift:

org $1CFD69
Main_ShowTextMessage:

org $0DBA71
GetRandomInt:

org $0DBA80
OAM_AllocateFromRegionA:
org $0DBA84
OAM_AllocateFromRegionB:
org $0DBA88
OAM_AllocateFromRegionC:
org $0DBA8C
OAM_AllocateFromRegionD:
org $0DBA90
OAM_AllocateFromRegionE:
org $0DBA94
OAM_AllocateFromRegionF:

org $0DBB67
Sound_SetSfxPanWithPlayerCoords:

org $0DBB6E
Sound_SetSfx1PanLong:

org $0DBB7C
Sound_SetSfx2PanLong:

org $0DBB8A
Sound_SetSfx3PanLong:

org $0DDB7F
HUD_RefreshIconLong:

org $0DE01E ; 6E10E - equipment.asm : 787
BottleMenu_movingOn:

org $0DE346
RestoreNormalMenu:

org $0DE9C8
DrawProgressIcons: ; this returns short

org $0DED29
DrawEquipment: ; this returns short

org $0DFA78
HUD_RebuildLong:

org $0DFA88
HUD_RebuildIndoor_Palace:

org $0EEE10
Messaging_Text:

org $1BED03
Palette_Sword:

org $1BED29
Palette_Shield:

org $1BEDF9
Palette_ArmorAndGloves:

org $1BEE52
Palette_Hud:

org $1CFAAA
ShopKeeper_RapidTerminateReceiveItem:

org $1CF500
Sprite_NullifyHookshotDrag:

org $1DF65D
Sprite_SpawnDynamically:

org $1DF65F
Sprite_SpawnDynamically_arbitrary:

org $1DFD4B
DiggingGameGuy_AttemptPrizeSpawn:

org $1EDE28
Sprite_GetEmptyBottleIndex: ; this is totally in sprite_bees.asm

org $1EF4E7
Sprite_PlayerCantPassThrough:
;================================================================================
