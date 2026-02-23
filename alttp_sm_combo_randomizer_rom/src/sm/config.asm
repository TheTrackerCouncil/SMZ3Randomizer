; General Super Metroid randomizer configuration flags

org $F47200
; Number of SM bosses to defeat
config_sm_bosses:
    dw #$0004

; starting events
; 0001 is zebes awake (default)
; 0400 is Tourian open (AKA Fast MB)
; 03C0 is G4 statues already grey (no animation)
config_events:       ; F47202
    dw #$0001