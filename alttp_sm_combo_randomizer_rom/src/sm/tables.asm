org $F26000

boss_rewards:
; Table to write what the bosses will reward when killed
;  Type   Flag   Icon   Ext
dw $0080, $0001, $4032, $0000   ; Kraid
dw $0080, $0002, $4234, $0000   ; Phantoon
dw $0080, $0004, $4438, $0000   ; Draygon
dw $0080, $0008, $463A, $0000   ; Ridley

; Types: $0000 = Pendant, $0040 = Crystal, $0080 = SM Boss Token
; Flag: Bitmask flag for the boss/pendant/crystal
; Icon: Icon to show on minimap
warnpc $F26100
org $F26100
starting_equipment:
dw $0000, $0000, $0063 ; Equipment, Beams, Energy
dw $0000, $0000, $0000 ; Missiles, Supers, Power Bombs