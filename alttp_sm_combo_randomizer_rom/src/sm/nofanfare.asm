
;========================================
; Item pickup sound skip
;========================================
!None = $02			;Stops current sounds (use for no sound)
!Missile = $03			;Sound when firing a missile
!Super = $04			;Sound when firing a super missile
!Power = $01			;Sound when a powerbomb explodes
!Click = $37			;Sound when selecting a HUD item
!SpazerShield = $26		;Spazer Shield sound
!PlasmaShield = $27		;Plasma Shield sound
!WaveShield = $28		;Wave Shield sound (doesn't stop)
!IceShield = $24		;Ice Shield sound
!Grapple = $05			;Grapple fire sound
!Crumble = $3D			;Crumble block sound
!Toggle = $38			;Sound when you toggle on/off items on status screen
!Helmet = $2A			;Sound when you select a save
!Flip = $2F			;Sound when you spin underwater
!Save = $2E			;Clip when you save the game
!Hurt = $35			;Samus hurt sound

;--------------------------------SPECIALFX VALUES--------------------------------

!Energy = $01			;Sound when picking up dropped energy
!Refill = $05			;Sound when picking up dropped missiles/bombs
!Bomb = $07			;Regular Bomb explosion
!Enemy = $09			;Enemy vaporising sound (when they die)
!Screw = $0B			;Enemy explode sound (from Screw Attack)
!Splash = $0D			;Water splash, or possibly an enemy sound?
!Bubble = $0F			;A bubble sound (from lava or being underwater)
!Beep = $16			;A weird beep. I don't recognise it
!Hum = $18			;Humming of the ship
!Statue = $19			;Releasing the Statue Spirits
!Quake = $1B			;Earthquake rumble
!Chozo = $1C			;I *think* it's when the Chozo grabs you, but not sure
!Dachora = $1D			;The bird is the word =P
!Skree = $21			;Skreeeee
!Explode = $25			;Some kind of explosion, but I can't think what. Sounds cool anyway
!Laser = $26			;A fucking awesome laser sound =D Possibly from MB
!Suit = $56			;Sound when you 'don a suit' =P

;Many more values for enemy sounds. Some are song-dependent

;--------------------------------MISCFX VALUES--------------------------------

!Land = $04			;Landing sound
!DoorO = $07			;A door opening
!DoorC = $08			;A door closing
!ShipO = $14			;Ship hatch opening
!ShipC = $15			;Ship hatch closing
!CeresO = $2C			;Ceres hatch opening
!Buzz = $09			;A cool buzz. Might be related to Grapple?
!Freeze = $0A			;enemy is frozen
!Text = $0D			;Text sound from the intro
!Gate = $0E			;A gate moving
!Spark = $0F			;Shinespark sound
!ExplodeA = $10			;Another explosion sound
!Lava = $11			;Sound of rising lava
!WTF = $16			;Sounds like a gunshot?
!Pirate = $17			;Pirate laser?
!LaserA = $1C			;A short laser
!Fire = $1D			;Sounds like a Norfair fire explosion thing
!Spin = $21			;Single spin jump sound
!BubbleA = $22			;3 bubbles
!Acid = $2D			;Hurt by acid/lava

;30+ Nothing or crash from what I checked. There were also some that sounded song-dependent

;--------------------------------ITEM SOUNDS-----------------------------------

;For each item, you just need to specify which type of sound, followed by which effect. Some are done as an example, so it should be easy to figure out.
;If you want to play the normal sound clip for an item, put "DW NORMAL *newline* DB $02" (or leave it black if you haven't already changed it)

org $c4E0B3			;Energy Tank
	DW SPECIALFX	;Load sound from SPECIALFX
	DB !Energy	;Run the "energy" sound
org $c4E0D8			;Missile
	DW SOUNDFX	;Load sound from SOUNDFX
	DB !Click	;Run the "click" sound
org $c4E0FD			;Super Missile
	DW SOUNDFX
	DB !Click
org $c4E122			;Powerbomb
	DW SOUNDFX
	DB !Click
org $c4E14F			;Bombs
	DW SPECIALFX
	DB !Explode
org $c4E17D			;Charge Beam
	DW SOUNDFX
	DB !Click
org $c4E1AB			;Ice Beam
	DW SOUNDFX
	DB !Click
org $c4E1D9			;High Jump
	DW SOUNDFX
	DB !Click
org $c4E207			;Speed Booster
	DW SOUNDFX
	DB !Click
org $c4E235			;Wave Beam
	DW SOUNDFX
	DB !Click
org $c4E263			;Spazer Beam
	DW SOUNDFX
	DB !Click
org $c4E291			;Spring Ball
	DW SOUNDFX
	DB !Click
org $c4E2C3			;Varia Suit
	DW SOUNDFX
	DB !None	;I put no sound here because it plays after the message box
org $c4E2F8			;Gravity Suit
	DW SOUNDFX
	DB !None
org $c4E32D			;X-ray
	DW SOUNDFX
	DB !Click	
org $c4E35A			;Plasma Beam
	DW SOUNDFX
	DB !Click
org $c4E388			;Grapple Beam
	DW SOUNDFX
	DB !Grapple
org $c4E3B5			;Space Jump
	DW SOUNDFX
	DB !Click	
org $c4E3E3			;Screw Attack
	DW SOUNDFX
	DB !Click	
org $c4E411			;Morph Ball
	DW SOUNDFX
	DB !Click
org $c4E43F			;Reserve Tank
	DW SPECIALFX
	DB !Energy
org $c4E46F			;Chozo Energy Tank
	DW SPECIALFX
	DB !Energy
org $c4E4A1			;Chozo Missile
	DW SOUNDFX
	DB !Click
org $c4E4D3			;Chozo Super Missile
	DW SOUNDFX
	DB !Click
org $c4E505			;Chozo Powerbomb
	DW SOUNDFX
	DB !Click
org $c4E53F			;Chozo Bombs
	DW SPECIALFX
	DB !Explode
org $c4E57A			;Chozo Charge Beam
	DW SOUNDFX
	DB !Click
org $c4E5B5			;Chozo Ice Beam
	DW SOUNDFX
	DB !Click
org $c4E5F0			;Chozo High Jump
	DW SOUNDFX
	DB !Click
org $c4E62B			;Chozo Speed Booster
	DW SOUNDFX
	DB !Click
org $c4E66F			;Chozo Wave Beam
	DW SOUNDFX
	DB !Click
org $c4E6AA			;Chozo Spazer Beam
	DW SOUNDFX
	DB !Click
org $c4E6E5			;Chozo Spring Ball
	DW SOUNDFX
	DB !Click
org $c4E720			;Chozo Varia Suit
	DW SOUNDFX
	DB !None
org $c4E762			;Chozo Gravity Suit
	DW SOUNDFX
	DB !None
org $c4E7A4			;Chozo X-ray
	DW SOUNDFX
	DB !Click	
org $c4E7DE			;Chozo Plasma Beam
	DW SOUNDFX
	DB !Click
org $c4E819			;Chozo Grapple Beam
	DW SOUNDFX
	DB !Grapple
org $c4E853			;Chozo Space Jump
	DW SOUNDFX
	DB !Click
org $c4E88E			;Chozo Screw Attack
	DW SOUNDFX
	DB !Click
org $c4E8C9			;Chozo Morph Ball
	DW SOUNDFX
	DB !Click
org $c4E904			;Chozo Reserve Tank
	DW SPECIALFX
	DB !Energy
org $c4E93A			;Scenery Energy Tank
	DW SPECIALFX
	DB !Energy
org $c4E972			;Scenery Missile
	DW SOUNDFX
	DB !Click
org $c4E9AA			;Scenery Super Missile
	DW SOUNDFX
	DB !Click
org $c4E9E2			;Scenery Powerbomb
	DW SOUNDFX
	DB !Click
org $c4EA22			;Scenery Bombs
	DW SPECIALFX
	DB !Explode
org $c4EA63			;Scenery Charge Beam
	DW SOUNDFX
	DB !Click
org $c4EAA4			;Scenery Ice Beam
	DW SOUNDFX
	DB !Click
org $c4EAE5			;Scenery High Jump
	DW SOUNDFX
	DB !Click
org $c4EB26			;Scenery Speed Booster
	DW SOUNDFX
	DB !Click
org $c4EB67			;Scenery Wave Beam
	DW SOUNDFX
	DB !Click
org $c4EBA8			;Scenery Spazer Beam
	DW SOUNDFX
	DB !Click
org $c4EBE9			;Scenery Spring Ball
	DW SOUNDFX
	DB !Click
org $c4EC2A			;Scenery Varia Suit
	DW SOUNDFX
	DB !None
org $c4EC72			;Scenery Gravity Suit
	DW SOUNDFX
	DB !None
org $c4ECBA			;Scenery X-ray
	DW SOUNDFX
	DB !Click	
org $c4ECFA			;Scenery Plasma Beam
	DW SOUNDFX
	DB !Click
org $c4ED3B			;Scenery Grapple Beam
	DW SOUNDFX
	DB !Grapple
org $c4ED7B			;Scenery Space Jump
	DW SOUNDFX
	DB !Click
org $c4EDBC			;Scenery Screw Attack
	DW SOUNDFX
	DB !Click
org $c4EDFD			;Scenery Morph Ball
	DW SOUNDFX
	DB !Click	
org $c4EE3E			;Scenery Reserve Tank
	DW SPECIALFX
	DB !Energy

;-------------------------DON'T EDIT THIS STUFF--------------------------

org $c58491
	DW #$0020

org $c2E126
base $82E126
	JSL CLIPCHECK
	BRA $08

;org $c58089
;base $858089
;	BRA $02

org $c48BF2
base $848BF2
NORMAL:
	JSR CLIPSET

