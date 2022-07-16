;SHARED SPRITE PALETTE FIX
;
;This is a repair so that sprites that previously shared Link's palette
;no longer share his palette.  In the vanilla game this was not an issue
;but with custom sprites some very strange color transitions can occur.
;
;The convention in the comments here is that bits are labeled [7:0]
;
;Written by Artheau
;during a cold morning on Oct. 29, 2018
;


;Bee (Credits)
;Seems like this was a bug in the vanilla game
;This puts the bee on the palette it uses in the rest of the game (bits 1-3)
;Appears virtually identical
org $0ECFBA
db $76

;Chests (Credits)
;Gives them a much more natural color (bits 1-3)
;There is one hex value for each chest
;The result is a visual improvement
org $0ED35A
db $37
org $0ED362
db $37
org $0ED36A
db $37

;Sweeping Woman (In-Game)
;Puts her on the same color of red that she is in the ending credits (bits 1-3)
org $DB383
db $07

;Ravens (Credits)
;Puts them on the same color as they are in-game (bits 1-3)
org $0ED653
db $09

;Running Man (In-Game)
;Puts the jacket on the same palette as the hat
;bits 1-3 are XORed with the base palette (currently 0b011)
org $05E9DA
db $00
org $05E9EA
db $40
org $05E9FA
db $00
org $05EA0A
db $40
org $05EA1A
db $00
org $05EA2A
db $00
org $05EA3A
db $40
org $05EA4A
db $40

;Running Man (Credits Only)
;Puts the jacket and the arm on the same palette as the hat (bits 1-3)
org $0ECE72
db $47
org $0ECE8A
db $07
org $0ECE92
db $07
org $0ECEA2
db $47
org $0ECEAA
db $07
org $0ECEBA
db $47

;Hoarder (when under a stone)
;Complete fix
;This was a bug that made the hoarder ignore its palette setting only when it was under a rock
org $06AAAC
db $F0
;But now we have to give it the correct palette info (bits 1-3)
org $06AA46
db $0B
org $06AA48
db $0B
org $06AA4A
db $0B
org $06AA4C
db $0B
org $06AA4E
db $4B

;Thief (friendly thief in cave)
;There is a subtle difference in color here (more pastel)
;His palette is given by bits 1-3
org $0DC322
db $07          ;set him to red
;Alternate palette options:
;db $09         ;lavender
;db $0B         ;green
;db $0D         ;yellow (same as he is in the credits)

;Pedestal Pull
;This edit DOES create a visual difference
;so I also present some alternate options
;
;
;Option A: Fix the red pendant, but now it ignores shadows
;and as a result, looks bugged
;org $05893D
;db $07
;
;
;Option B: Make the red pendant a yellow pendant
;org $05893D
;db $0D
;
;
;Option C: Also change the other pendants so that they all
;ignore shadows.  This looks better because they appear to
;glow even brighter
;BUT I had to compromise on the color of the blue pendant
org $058933
db $05          ;change palette of blue pendant
org $058938
db $01          ;change palette of green pendant
org $05893D
db $07          ;change palette of red pendant
;the pendants travel in a direction determined by their color
;so option C also requires a fix to their directional movement
org $058D21
db $04
org $058D22
db $04
org $058D23
db $FC
org $058D24
db $00
org $058D25
db $FE
org $058D26
db $FE
org $058D27
db $FE
org $058D28
db $FC

;Blind Maiden
;Previously she switched palettes when she arrived at the light (although it was very subtle)
;Here we just set it so that she starts at that color
org $0DB410
db $4B          ;sets the palette of the prison sprite (bits 1-3)
org $09A8EB
db $05          ;sets the palette of the tagalong (bits 0-2)

;Crystal Maiden (credits)
;One of the crystal maidens was on Link's palette, but only in the end sequence
;palette given by bits 1-3
org $0EC8C3
db $37

;Cukeman (Everywhere)
;This guy is such a bugfest. Did you know that his body remains an enemy and if you try talking to him,
;you have to target the overlaid sprite that only has eyeballs and a mouth?
;This is why you can still be damaged by him. In any case, I digress.  Let's talk edits.
;
;These edits specifically target the color of his lips
;Bits 1-3 are XORed with his base ID palette (0b100)
;and the base palette cannot be changed without breaking buzzblobs (i.e. green dancing pickles)
org $1AFA93
db $0F
org $1AFAAB
db $0F
org $1AFAC3
db $0F
org $1AFADB
db $0F
org $1AFAF3
db $0F
org $1AFB0B
db $0F
;BUT there is a very specific ramification of the above edits:
;Because his lips were moved to the red palette, his lips
;no longer respond to shadowing effects
;(like how red rupees appear in lost woods)
;this will only be apparent if enemizer places him in areas like lost woods
;or in the end credits sequence during his short cameo,
;so the line below replaces him in the end sequence
;with a buzzblob
org $0ED664
db $00          ;number of cukeman in the scene (up to 4)
