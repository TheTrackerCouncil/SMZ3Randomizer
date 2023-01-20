; Common Combo Randomizer ROM confiuration flags

org $F47000
config_flags:

; Enable "Multiworld"
config_multiworld:  ; $F47000
    dw #$0000

; Custom sprite enabled?
config_alttp_sprite: ; $F47002
    dw #$0000
config_sm_sprite:    ; $F47004
    dw #$0000


; Enables keysanity specific code sections.
config_keysanity:    ; $F47006
    dw #$0000

; Number of SM bosses to defeat
config_sm_bosses:    ; $F47008
    dw #$0004

; starting events
; 0001 is zebes awake (default)
; 0400 is Tourian open (AKA Fast MB)
; 03C0 is G4 statues already grey (no animation)
config_events:       ; F4700A
    dw #$0001