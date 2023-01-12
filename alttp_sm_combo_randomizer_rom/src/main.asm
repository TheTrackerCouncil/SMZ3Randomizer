exhirom

; --- Macros and stuff ---
incsrc "macros.asm"                         ; Useful macros
incsrc "sram.asm"                           ; SRAM Variable definitions

; --- Common code ---
incsrc "config.asm"                         ; Common configuration flags
incsrc "common.asm"                         ; Common routines
incsrc "credits.asm"                        ; Common credits scroller
incsrc "spc_play.asm"                       ; Common SPC player
;incsrc "spc_stream.asm"                     ; SPC BRR streaming

; --- Multiworld / Randolive ---
incsrc "multiworld/multiworld.asm"          ; Common multiworld features

; --- Super Metroid code ---
incsrc "sm/config.asm"                      ; Super Metroid configuration flags
incsrc "sm/hirom.asm"                       ; Super Metroid ExHiROM patch
incsrc "sm/transition.asm"                  ; Super Metroid Transition patch
incsrc "sm/teleport.asm"                    ; Super Metroid Teleport patch
incsrc "sm/randomizer/randomizer.asm"       ; Super Metroid Randomizer patches
incsrc "sm/nofanfare.asm"                   ; Super Metroid Remove Item fanfares
incsrc "sm/items.asm"                       ; Super Metroid ALTTP Items patch
incsrc "sm/ending.asm"                      ; Super Metroid Ending conditions
incsrc "sm/newgame.asm"                     ; Super Metroid New Game Initialization
incsrc "sm/minorfixes.asm"                  ; Super Metroid some softlock removals etc
incsrc "sm/demofix.asm"                     ; Super Metroid Stop demos from playing
incsrc "sm/maps.asm"                        ; Super Metroid map pause screen and HUD changes
incsrc "sm/map_icons.asm"                   ; Super Metroid door icons for keysanity map
incsrc "sm/max_ammo.asm"                    ; Super Metroid max ammo patch by personitis, adapted by Leno for Crossover
incsrc "sm/multiworld.asm"                  ; Super Metroid Multiworld support features
incsrc "sm/sprite/sprite.asm"               ; Super Metroid custom Samus sprite "engine" by Artheau
incsrc "sm/titlescreen.asm"                 ; Super Metroid Upload New Title Screen GFX
incsrc "sm/keycards.asm"                    ; Super Metroid Keycard system
incsrc "sm/plminject.asm"                   ; Super Metroid PLM Injection
incsrc "sm/messagebox.asm"                  ; Super Metroid Messagebox improvements
incsrc "sm/sm_msu.asm"                      ; Super Metroid MSU support

; --- ALTTP code ---
incsrc "z3/hirom.asm"	                    ; ALTTP ExHiROM patch
incsrc "z3/transition.asm"                  ; ALTTP Transition patch
incsrc "z3/teleport.asm"                    ; ALTTP Teleport patch
incsrc "z3/randomizer/randomizer.asm"       ; ALTTP Randomizer patches (github.com/mmxbass/z3randomizer)
incsrc "z3/items.asm"                       ; ALTTP Super Metroid Items
incsrc "z3/ending.asm"                      ; ALTTP Ending Conditions
incsrc "z3/newgame.asm"                     ; ALTTP New Game Initialization
incsrc "z3/multiworld.asm"                  ; ALTTP Multiworld support features
incsrc "z3/skiptitle.asm"                   ; ALTTP Skip Title Screen and File Select Screen
