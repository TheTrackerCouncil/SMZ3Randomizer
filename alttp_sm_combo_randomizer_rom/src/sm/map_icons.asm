org $C2BB30 ; Start of elevator label drawing routine
base $82BB30
JMP Map_Labels_Hijack
Return_From_Hijack:

org $C2FB80 ; Beginning of a large block of free space
base $82FB80
Map_Labels_Hijack:
JSL Draw_Door_Icons
JSL rewards_draw_map_icons  ; Draw boss reward icons

REP #$30 ; this is the code that was overwritten
PHB      ; by the JMP into this routine
JMP Return_From_Hijack

; This is basically a copy of the elevator label
; routine, but it looks to a different address
; for its list of pointers
Draw_Door_Icons:
REP #$30
PHB
PHK
PLB
LDA #$0000
STA $03
LDX $079F
LDA $7ED908,x
AND #$00FF
BEQ .exit
LDA $079F
ASL A
TAX
LDA Door_Icon_List_Pointers,x
TAX

; LOOP
-
    LDA $0000,x
    CMP #$FFFF
    BEQ .exit
    AND #$00FF
    CMP.l #$0001
    BNE +
    LDA.l config_keysanity
    BNE .draw
    TXA
    BRA .next

+
    CMP.l #$0002
    BNE .draw
    LDA.l config_keysanity
    BEQ .draw
    LDA $0000, x
    XBA : AND #$00FF        ; Get boss id
    CLC : ADC #$00A0        ; Add to event-id
    JSL $808233             ; Check if event is set
    BCS .draw
    TXA
    BRA .next

.draw
    LDA $0002,x
    PHX
    SEC
    SBC $B1
    PHA
    LDA $0004,x
    SEC
    SBC $B3
    TAY
    LDA $0006,x : PHA : ASL : TAX       ; Load index to spritemap pointer
    LDA $C569,x : TAX                   ; Load spritemap pointer
    LDA $0005,x : AND #$0E00 : STA $03  ; Load palette data from spritemap
    PLA
    PLX
    JSL $81891F
    PLA
.next
    CLC
    ADC #$0008
    TAX
    BRA -

.exit
PLB
RTL


Door_Icon_List_Pointers:
; pointers to lists
DW Crateria_door_icons
DW Brinstar_door_icons
DW Norfair_door_icons
DW Wrecked_Ship_door_icons
DW Maridia_door_icons
DW Tourian_door_icons

; The following tables are lists of map icons to display.
; Entry format is XX coordinate, YY coordinate, spritemap pointer index.
org $C2FC80
base $82FC80
Crateria_door_icons:
DW $0001,$00B4,$0017,$1D01  ; 1 - Landing Site > Gauntlet Entrance
DW $0001,$00FC,$000F,$1D01  ; 1 - Landing Site > Crateria Power Bombs
DW $0001,$011C,$0027,$1D02  ; 2 - Kihunter Room > Moat
DW $0001,$005C,$0047,$1D0B  ; B - Green Pirate Shaft > Statues Hallway
DW $0001,$00C4,$0037,$1D0B  ; B - Bomb Torizo
DW $0001,$016C,$000F,$1D01  ; 1 - West Ocean > (WS) "Peekaboo Room"
DW $0001,$015C,$0017,$1D01  ; 1 - West Ocean > (WS) Bowling Alley
DW $FFFF

org $C2FD00
base $82FD00
Brinstar_door_icons:
DW $0001,$00EC,$0057,$1D01  ; 1 - Construction Zone > Blue Brin E-Tank
DW $0001,$0044,$0057,$1D02  ; 2 - Green Brin Main Shaft > Beetoms
DW $0001,$00A4,$0047,$1D02  ; 2 - Pink Brin Hoppers > Hoptank Room
DW $0001,$00B4,$004F,$1D02  ; 2 - Spore Spawn Farm > Spore Spawn Super
DW $0001,$00B0,$0023,$1D0B  ; B - Spore Spawn
DW $0001,$01B4,$009F,$1D0B  ; B - Kraid
DW $0002,$01BD,$00A8,$1D15  ; Reward - Kraid
DW $FFFF

org $C2FD80
base $82FD80
Norfair_door_icons:
DW $0001,$004C,$001F,$1D01  ; 1 - Business Center > Ice Beam Gate
DW $0001,$0014,$003F,$1D01  ; 1 - Crocomire Speedway > Crumble Shaft
DW $0001,$0084,$0027,$1D02  ; 2 - Cathedral > Rising Tide
DW $0001,$00BC,$001F,$1D02  ; 2 - Single Chamber > Bubble Mountain
DW $0001,$00AC,$002F,$1D02  ; 2 - U.N. Farm > Bubble Mountain
DW $0001,$00B0,$0033,$1D02  ; 2 - Purple Shaft > Bubble Mountain
DW $0001,$0078,$0053,$1D0B  ; B - Crocomire
;Lower Norfair:
DW $0001,$00EC,$001F,$1D01  ; 1 - Single Chamber > Three Musketeers
DW $0001,$00F4,$0057,$1D01  ; 1 - WRITG > Amphitheater
DW $0001,$00BC,$0087,$1D0B  ; B - Ridley
DW $0302,$00B9,$0098,$1D18  ; Reward - Ridley
DW $FFFF

org $C2FE00
base $82FE00
Wrecked_Ship_door_icons:
DW $0001,$005C,$005F,$1D01  ; 1 - (Cr) West Ocean > "Peekaboo Room"
DW $0001,$004C,$0067,$1D01  ; 1 - (Cr) West Ocean > Bowling Alley
DW $0001,$0054,$006F,$1D01  ; 1 - Gravity Suit Room
DW $0001,$0094,$009F,$1D0B  ; B - Phantoon
DW $0102,$0099,$00A8,$1D16  ; Reward - Phantoon
DW $FFFF

org $C2FE40
base $82FE40
Maridia_door_icons:
DW $0001,$008C,$004F,$1D01  ; 1 - Mt.Everest > Crab Shaft
DW $0001,$009C,$0057,$1D01  ; 1 - Aqueduct > Crab Shaft
DW $0001,$00BC,$0047,$1D02  ; 2 - Botwoon
DW $0001,$0104,$0047,$1D02  ; 2 - Halfie Climb > Botwoon E-Tank
DW $0001,$0104,$003F,$1D0B  ; B - Cactus Alley > Halfie Climb
DW $0001,$0144,$004F,$1D0B  ; B - Draygon
DW $0202,$013D,$005F,$1D17  ; Reward - Draygon 
DW $FFFF

org $C2FEA0
base $82FEA0
Tourian_door_icons:
DW $FFFF

; Spritemaps
;
; EXCERPT FROM PJ'S BANK LOGS http://patrickjohnston.org/bank/index.html
; Spritemap format is roughly:
;     nnnn         ; Number of entries (2 bytes)
;     xxxx yy aatt ; Entry 0 (5 bytes)
;     ...          ; Entry 1...
; Where:
;     n = number of entries
;     x = X offset of sprite from centre
;     y = Y offset of sprite from centre
;     a = attributes
;     t = tile number
;
; More specifically, a spritemap entry is:
;     s000000x,xxxxxxxx,yyyyyyyy,YXppPPPt,tttttttt
; Where:
;     s = size bit
;     x = X offset of sprite from centre
;     y = Y offset of sprite from centre
;     Y = Y flip
;     X = X flip
;     P = palette
;     p = priority (relative to background)
;     t = tile number

Door1_Icon_Spritemap:
DW $0001, $0000 : DB $00 : DW $2021
Door2_Icon_Spritemap:
DW $0001, $0000 : DB $00 : DW $2E22
DoorB_Icon_Spritemap:
DW $0001, $0000 : DB $00 : DW $2023

BlueCrystal_Icon_Spritemap:
DW $0001, $0000 : DB $00 : DW $2EBB
RedCrystal_Icon_Spritemap:
DW $0001, $0000 : DB $00 : DW $2EBC
GreenPendant_Icon_Spritemap:
DW $0001, $0000 : DB $00 : DW $2EBD
BluePendant_Icon_Spritemap:
DW $0001, $0000 : DB $00 : DW $2EBE
RedPendant_Icon_Spritemap:
DW $0001, $0000 : DB $00 : DW $2EBF
BossKraid_Icon_Spritemap:
DW $0001, $0000 : DB $00 : DW $2ECB
BossPhantoon_Icon_Spritemap:
DW $0001, $0000 : DB $00 : DW $2ECD
BossDraygon_Icon_Spritemap:
DW $0001, $0000 : DB $00 : DW $2ECE
BossRidley_Icon_Spritemap:
DW $0001, $0000 : DB $00 : DW $2ECF

warnpc $C2FF6A

; Spritemap pointers
; Index = (local_address - $C569) / 2
; Note that this means the spritemaps have to start on odd numbered addresses
org $C2FF6B
base $82FF6B
DW Door1_Icon_Spritemap ; Index $1D01
DW Door2_Icon_Spritemap ; Index $1D02
org $C2FF7F
base $82FF7F
DW DoorB_Icon_Spritemap ; Index $1D0B

org $C2FF89
base $82FF89
dw BlueCrystal_Icon_Spritemap   ; Index $1D10
dw RedCrystal_Icon_Spritemap    ; Index $1D11
dw GreenPendant_Icon_Spritemap  ; Index $1D12
dw BluePendant_Icon_Spritemap   ; Index $1D13
dw RedPendant_Icon_Spritemap    ; Index $1D14
dw BossKraid_Icon_Spritemap     ; Index $1D15
dw BossPhantoon_Icon_Spritemap  ; Index $1D16
dw BossDraygon_Icon_Spritemap   ; Index $1D17
dw BossRidley_Icon_Spritemap    ; Index $1D18

; If you want to edit these, note that you can use the bits in the spritemap
; entries above to select from any of the loaded palettes.
org $F6C420 ; PC Offset 0x36C420
; This address overwrites parts of some large lettering tiles, but they look like
; they're already altered and probably unused. This stuff can be moved as long as
; it falls into the $4000 region of VRAM after being loaded. The tile numbers used
; in the spritemaps are per-tile offsets from that VRAM address.
Door1_Icon_GFX:
DB $3C, $3C, $76, $4A, $E7, $99, $F7, $89, $F7, $89, $E3, $9D, $7E, $42, $3C, $3C
DB $3C, $3C, $7E, $4A, $FF, $99, $FF, $89, $FF, $89, $FF, $9D, $7E, $42, $3C, $3C
Door2_Icon_GFX:
DB $3C, $3C, $66, $5A, $DB, $A5, $F7, $89, $EF, $91, $C3, $BD, $7E, $42, $3C, $3C
DB $3C, $3C, $7E, $5A, $FF, $A5, $FF, $89, $FF, $91, $FF, $BD, $7E, $42, $3C, $3C
DoorB_Icon_GFX:
DB $3C, $3C, $46, $7A, $DB, $A5, $C7, $B9, $DB, $A5, $C7, $B9, $7E, $42, $3C, $3C
DB $3C, $3C, $7E, $7A, $FF, $A5, $FF, $B9, $FF, $A5, $FF, $B9, $7E, $42, $3C, $3C

; Replace unused (in smz3) file transfer sprite graphics with new Pendant/Crystal/Boss Token icons
; Used for pendant/crystal/boss shuffle
org $F6D760
CrystalPendant_Icons_Gfx:
incbin "data/reward_icons.bin":($00)-($A0)
org $F6D960
Boss1_Icons_Gfx:
incbin "data/reward_icons.bin":($A0)-($C0)
org $F6D9A0
Boss2_Icons_Gfx:
incbin "data/reward_icons.bin":($C0)-($120)