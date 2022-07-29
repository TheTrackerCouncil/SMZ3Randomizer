;================================================================================
; Dialog Pointer Override
;--------------------------------------------------------------------------------
DialogOverride:
	LDA $7F5035 : BEQ .skip
	LDA $7F5700, X ; use alternate buffer
RTL
	.skip
    LDA $7F1200, X
RTL
;--------------------------------------------------------------------------------
; $7F5035 - Alternate Text Pointer Flag ; 0=Disable
; $7F5036 - Padding Byte (Must be Zero)
; $7F5700 - $7F57FF - Dialog Buffer
;--------------------------------------------------------------------------------
ResetDialogPointer:
	STZ $1CF0 : STZ $1CF1 ; reset decompression buffer
	LDA.b #$00 : STA $7F5035 ; zero out the alternate flag
	LDA.b #$1C : STA $1CE9 ; thing we wrote over
RTL
;--------------------------------------------------------------------------------
;macro LoadDialog(index,table)
;	PHA : PHX : PHY
;	PHB : PHK : PLB
;	LDA $00 : PHA
;	LDA $01 : PHA
;	LDA $02 : PHA
;		LDA.b #$01 : STA $7F5035 ; set flag
;
;		LDA <index> : ASL : !ADD.l <index> : TAX ; get quote offset *3, move to X
;		LDA <table>, X : STA $00 ; write pointer to direct page
;		LDA <table>+1, X : STA $01
;		LDA <table>+2, X : STA $02
;
;		LDX.b #$00 : LDY.b #$00
;		-
;			LDA [$00], Y ; load the next character from the pointer
;			STA $7F5700, X ; write to the buffer
;			INX : INY
;		CMP.b #$7F : BNE -
;	PLA : STA $02
;	PLA : STA $01
;	PLA : STA $00
;	PLB
;	PLY : PLX : PLA
;endmacro
;--------------------------------------------------------------------------------
;macro LoadDialogAddress(address)
;	PHA : PHX : PHY
;	PHP
;	PHB : PHK : PLB
;		SEP #$30 ; set 8-bit accumulator and index registers
;		LDA $00 : PHA
;		LDA $01 : PHA
;		LDA $02 : PHA
;			LDA.b #$01 : STA $7F5035 ; set flag
;
;			LDA.b #<address> : STA $00 ; write pointer to direct page
;			LDA.b #<address>>>8 : STA $01
;			LDA.b #<address>>>16 : STA $02
;
;			LDX.b #$00 : LDY.b #$00
;			-
;				LDA [$00], Y ; load the next character from the pointer
;				STA $7F5700, X ; write to the buffer
;				INX : INY
;			CMP.b #$7F : BNE -
;		PLA : STA $02
;		PLA : STA $01
;		PLA : STA $00
;	PLB
;	PLP
;	PLY : PLX : PLA
;endmacro
;--------------------------------------------------------------------------------
!OFFSET_POINTER = "$7F5094"
!OFFSET_RETURN = "$7F5096"
!DIALOG_BUFFER = "$7F5700"
macro LoadDialogAddress(address)
	PHA : PHX : PHY
	PHP
	PHB : PHK : PLB
		SEP #$20 ; set 8-bit accumulator
		REP #$10 ; set 16-bit index registers
		LDA $00 : PHA
		LDA $01 : PHA
		LDA $02 : PHA
			STZ $1CF0 : STZ $1CF1 ; reset decompression buffer
			LDA.b #$01 : STA $7F5035 ; set flag
			%CopyDialog(<address>)
		PLA : STA $02
		PLA : STA $01
		PLA : STA $00
	PLB
	PLP
	PLY : PLX : PLA
endmacro
;--------------------------------------------------------------------------------
macro CopyDialog(address)
	LDA.b #<address> : STA $00 ; write pointer to direct page
	LDA.b #<address>>>8 : STA $01
	LDA.b #<address>>>16 : STA $02
	%CopyDialogIndirect()
endmacro
;--------------------------------------------------------------------------------
macro CopyDialogTable(tbl, entries)
	REP #$20
	JSL GetRandomInt	
	STA.W $4204
	SEP #$20
	LDA.B #<entries>
	STA.W $4206
	NOP #7
	REP #$20
	LDA.W $4216
	ASL
	TAX
	LDA.L <tbl>, x ; Load pointer into A
	STA $00
	SEP #$20
	LDA.B #<tbl>>>16
	STA.B $02
	%CopyDialogIndirect()
endmacro
;--------------------------------------------------------------------------------
macro CopyDialogIndirect()
	REP #$20 : LDA !OFFSET_POINTER : TAX : LDY.w #$0000 : SEP #$20 ; copy 2-byte offset pointer to X and set Y to 0
	?loop:
		LDA [$00], Y ; load the next character from the pointer
		STA !DIALOG_BUFFER, X ; write to the buffer
		INX : INY
	CMP.b #$7F : BNE ?loop
	REP #$20 ; set 16-bit accumulator
	TXA : INC : STA !OFFSET_RETURN ; copy out X into
	LDA.w #$0000 : STA !OFFSET_POINTER
	SEP #$20 ; set 8-bit accumulator
endmacro
;--------------------------------------------------------------------------------
LoadDialogAddressIndirect:
	STZ $1CF0 : STZ $1CF1 ; reset decompression buffer
	LDA.b #$01 : STA $7F5035 ; set flag
	%CopyDialogIndirect()
	;%LoadDialogAddress(UncleText)
RTL
;--------------------------------------------------------------------------------
!ITEM_TEMPORARY = "$7F5040"
FreeDungeonItemNotice:
	STA !ITEM_TEMPORARY

	PHA : PHX : PHY
	PHP
	PHB : PHK : PLB
		SEP #$20 ; set 8-bit accumulator
		REP #$10 ; set 16-bit index registers
		LDA $00 : PHA
		LDA $01 : PHA
		LDA $02 : PHA
	;--------------------------------

	LDA.l !MULTIWORLD_PICKUP : BEQ + : BRL .skip : +
	LDA.l FreeItemText : BNE + : BRL .skip : +

	LDA #$00 : STA $7F5010 ; initialize scratch
	LDA !ITEM_TEMPORARY
	CMP.b #$24 : BEQ + : BRL ++ : + ; general small key
		%CopyDialog(Notice_SmallKeyOf)
		REP #$20 : LDA !OFFSET_RETURN : DEC #2 : STA !OFFSET_POINTER : SEP #$20
		%CopyDialog(Notice_Self)
		BRL .done
	++ : CMP.b #$25 : BNE + ; general compass
		%CopyDialog(Notice_CompassOf)
		REP #$20 : LDA !OFFSET_RETURN : DEC #2 : STA !OFFSET_POINTER : SEP #$20
		%CopyDialog(Notice_Self)
		BRL .done
	+ : CMP.b #$33 : BNE + ; general map
		%CopyDialog(Notice_MapOf)
		REP #$20 : LDA !OFFSET_RETURN : DEC #2 : STA !OFFSET_POINTER : SEP #$20
		%CopyDialog(Notice_Self)
		BRL .done
	+ : CMP.b #$32 : BNE + ; general big key
		%CopyDialog(Notice_BigKeyOf)
		REP #$20 : LDA !OFFSET_RETURN : DEC #2 : STA !OFFSET_POINTER : SEP #$20
		%CopyDialog(Notice_Self)
		BRL .done
	+
	AND.b #$F0 ; looking at high bits only
	CMP.b #$70 : BNE + ; map of...
		%CopyDialog(Notice_MapOf)
		BRL .dungeon
	+ : CMP.b #$80 : BNE + ; compass of...
		%CopyDialog(Notice_CompassOf)
		BRL .dungeon
	+ : CMP.b #$90 : BNE + ; big key of...
		%CopyDialog(Notice_BigKeyOf)
		BRL .dungeon
	+ : CMP.b #$A0 : BNE + ; small key of...
		LDA !ITEM_TEMPORARY : CMP.b #$AF : BNE ++ : BRL .skip : ++
		%CopyDialog(Notice_SmallKeyOf)
		PLA : AND.b #$0F : STA $7F5020 : LDA.b #$0F : !SUB $7F5020 : PHA
		LDA #$01 : STA $7F5010 ; set up a flip for small keys
		BRL .dungeon
	+ : CMP.b #$D0 : BNE + ; keycard for ...
		%CopyDialog(Notice_KeycardFor)
		BRL .region
	+
	BRL .skip ; it's not something we are going to give a notice for

	.dungeon
	REP #$20 : LDA !OFFSET_RETURN : DEC #2 : STA !OFFSET_POINTER : SEP #$20
	LDA !ITEM_TEMPORARY
	AND.b #$0F ; looking at low bits only
	STA $7F5011
	LDA $7F5010 : BEQ +
		LDA $7F5010
		LDA #$0F : !SUB $7F5011 : STA $7F5011 ; flip the values for small keys
	+
	LDA $7F5011
	CMP.b #$00 : BNE + ; ...light world
		%CopyDialog(Notice_LightWorld) : BRL .done
	+ : CMP.b #$01 : BNE + ; ...dark world
		%CopyDialog(Notice_DarkWorld) : BRL .done
	+ : CMP.b #$02 : BNE + ; ...ganon's tower
		%CopyDialog(Notice_GTower) : BRL .done
	+ : CMP.b #$03 : BNE + ; ...turtle rock
		%CopyDialog(Notice_TRock) : BRL .done
	+ : CMP.b #$04 : BNE + ; ...thieves' town
		%CopyDialog(Notice_Thieves) : BRL .done
	+ : CMP.b #$05 : BNE + ; ...tower of hera
		%CopyDialog(Notice_Hera) : BRL .done
	+ : CMP.b #$06 : BNE + ; ...ice palace
		%CopyDialog(Notice_Ice) : BRL .done
	+ : CMP.b #$07 : BNE + ; ...skull woods
		%CopyDialog(Notice_Skull) : BRL .done
	+ : CMP.b #$08 : BNE + ; ...misery mire
		%CopyDialog(Notice_Mire) : BRL .done
	+ : CMP.b #$09 : BNE + ; ...dark palace
		%CopyDialog(Notice_PoD) : BRL .done
	+ : CMP.b #$0A : BNE + ; ...swamp palace
		%CopyDialog(Notice_Swamp) : BRL .done
	+ : CMP.b #$0B : BNE + ; ...agahnim's tower
		%CopyDialog(Notice_AgaTower) : BRL .done
	+ : CMP.b #$0C : BNE + ; ...desert palace
		%CopyDialog(Notice_Desert) : BRL .done
	+ : CMP.b #$0D : BNE + ; ...eastern palace
		%CopyDialog(Notice_Eastern) : BRL .done
	+ : CMP.b #$0E : BNE + ; ...hyrule castle
		%CopyDialog(Notice_Castle) : BRL .done
	+ : CMP.b #$0F : BNE + ; ...sewers
		%CopyDialog(Notice_Sewers)
	+ BRL .done

	.region
	REP #$20 : LDA !OFFSET_RETURN : DEC #2 : STA !OFFSET_POINTER : SEP #$20
	LDA !ITEM_TEMPORARY
	AND.b #$0F ; looking at low bits only
	CMP.b #$00 : BNE + ; ...light world
		%CopyDialog(Notice_CrateriaL1) : BRL .done
	+ : CMP.b #$01 : BNE + ; ...dark world
		%CopyDialog(Notice_CrateriaL2) : BRL .done
	+ : CMP.b #$02 : BNE + ; ...ganon's tower
		%CopyDialog(Notice_CrateriaBoss) : BRL .done
	+ : CMP.b #$03 : BNE + ; ...turtle rock
		%CopyDialog(Notice_BrinstarL1) : BRL .done
	+ : CMP.b #$04 : BNE + ; ...thieves' town
		%CopyDialog(Notice_BrinstarL2) : BRL .done
	+ : CMP.b #$05 : BNE + ; ...tower of hera
		%CopyDialog(Notice_BrinstarBoss) : BRL .done
	+ : CMP.b #$06 : BNE + ; ...ice palace
		%CopyDialog(Notice_NorfairL1) : BRL .done
	+ : CMP.b #$07 : BNE + ; ...skull woods
		%CopyDialog(Notice_NorfairL2) : BRL .done
	+ : CMP.b #$08 : BNE + ; ...misery mire
		%CopyDialog(Notice_NorfairBoss) : BRL .done
	+ : CMP.b #$09 : BNE + ; ...dark palace
		%CopyDialog(Notice_MaridiaL1) : BRL .done
	+ : CMP.b #$0A : BNE + ; ...swamp palace
		%CopyDialog(Notice_MaridiaL2) : BRL .done
	+ : CMP.b #$0B : BNE + ; ...agahnim's tower
		%CopyDialog(Notice_MaridiaBoss) : BRL .done
	+ : CMP.b #$0C : BNE + ; ...desert palace
		%CopyDialog(Notice_WreckedShipL1) : BRL .done
	+ : CMP.b #$0D : BNE + ; ...eastern palace
		%CopyDialog(Notice_WreckedShipBoss) : BRL .done
	+ : CMP.b #$0E : BNE + ; ...hyrule castle
		%CopyDialog(Notice_LowerNorfairL1) : BRL .done
	+ : CMP.b #$0F : BNE + ; ...sewers
		%CopyDialog(Notice_LowerNorfairBoss)
	+ BRL .done


	.done

	STZ $1CF0 : STZ $1CF1 ; reset decompression buffer
	LDA.b #$01 : STA $7F5035 ; set alternate dialog flag
	LDA.b #$01 : STA $7F50A0

	;--------------------------------
		PLA : STA $02
		PLA : STA $01
		PLA : STA $00
	PLB
	PLP
	PLY : PLX : PLA
	;JSL.l Main_ShowTextMessage
RTL

	.skip
	;--------------------------------
		PLA : STA $02
		PLA : STA $01
		PLA : STA $00
	PLB
	PLP
	PLY : PLX : PLA
RTL
;--------------------------------------------------------------------------------
; DialogBlind:
; 	%LoadDialogAddress(BlindText)
; 	JSL.l Sprite_ShowMessageMinimal
; RTL
; ;--------------------------------------------------------------------------------
; DialogFairyThrow:
; 	LDA.l Restrict_Ponds : BEQ .normal
; 	LDA $7EF35C : ORA $7EF35D : ORA $7EF35E : ORA $7EF35F : BNE .normal
	
; 	.noInventory
; 	LDA $0D80, X : !ADD #$08 : STA $0D80, X
; 	LDA.b #$51
; 	LDY.b #$01
; RTL
; 	.normal
; 	LDA.b #$88
; 	LDY.b #$00
; RTL
; ;--------------------------------------------------------------------------------
; DialogPyramidFairy:
; 	%LoadDialogAddress(PyramidFairyText)
; 	JSL.l Sprite_ShowMessageUnconditional
; RTL
; ;--------------------------------------------------------------------------------
; DialogTriforce:
; 	%LoadDialogAddress(TriforceText)
; 	REP #$20 : LDA.w #$0171 : STA $1CF0 : SEP #$20 ; fix border
; 	JSL.l Main_ShowTextMessage
; 	JSL.l Messaging_Text
; RTL
; ;--------------------------------------------------------------------------------
; DialogGanon1:
; 	JSL.l CheckGanonVulnerability : BCS +
; 		%LoadDialogAddress(GanonText1Alternate)
; 		BRA ++
; 	+
; 		%LoadDialogAddress(GanonText1)
; 	++
; 	JSL.l Sprite_ShowMessageMinimal
; RTL
; ;--------------------------------------------------------------------------------
; DialogGanon2:
; 	JSL.l CheckGanonVulnerability : BCS +
; 		%LoadDialogAddress(GanonText2Alternate)
; 		BRA ++
; 	+
; 		%LoadDialogAddress(GanonText2)
; 	++
; 	JSL.l Sprite_ShowMessageMinimal
; RTL
; ;--------------------------------------------------------------------------------
; DialogPedestal:
; 	PHA
; 	LDA $0202 : CMP.b #$0F : BEQ + ; Show normal text if book is not equipped
; 	-
; 	PLA : JSL Sprite_ShowMessageUnconditional ; Wacky Hylian Text
; RTL
; 	+
; 	BIT $F4 : BVC - ; Show normal text if Y is not pressed
; 	%LoadDialogAddress(MSPedestalText)
; 	PLA : JSL Sprite_ShowMessageUnconditional ; Text From MSPedestalText (tables.asm)
; RTL
;--------------------------------------------------------------------------------
; DialogEtherTablet:
; 	PHA
; 	LDA $0202 : CMP.b #$0F : BEQ + ; Show normal text if book is not equipped
; 	-
; 	PLA : JSL Sprite_ShowMessageUnconditional ; Wacky Hylian Text
; RTL
; 	+
; 	BIT $F4 : BVC - ; Show normal text if Y is not pressed
; 	LDA.l AllowHammerTablets : BEQ ++
; 		LDA $7EF34B : BEQ .yesText : BRA .noText
; 	++
; 		LDA $7EF359 : CMP.b #$FF : BEQ .yesText : CMP.b #$02 : !BGE .noText
; 	;++
; 	.yesText
; 	PLA : JSL Sprite_ShowMessageUnconditional ; Text From MSPedestalText (tables.asm)
; RTL
; 	.noText
; 	PLA
; RTL
; ;--------------------------------------------------------------------------------
; DialogBombosTablet:
; 	PHA
; 	LDA $0202 : CMP.b #$0F : BEQ + ; Show normal text if book is not equipped
; 	-
; 	PLA : JSL Sprite_ShowMessageUnconditional ; Wacky Hylian Text
; RTL
; 	+
; 	BIT $F4 : BVC - ; Show normal text if Y is not pressed
; 	LDA.l AllowHammerTablets : BEQ ++
; 		LDA $7EF34B : BEQ .yesText : BRA .noText
; 	++
; 		LDA $7EF359 : CMP.b #$FF : BEQ .yesText : CMP.b #$02 : !BGE .noText
; 	;++
; 	.yesText
; 	PLA : JSL Sprite_ShowMessageUnconditional ; Text From MSPedestalText (tables.asm)
; RTL
; 	.noText
; 	PLA
; RTL
;--------------------------------------------------------------------------------
; DialogUncle:
; 	;%LoadDialog(UncleQuote,DialogUncleData)
; 	%LoadDialogAddress(UncleText)
; 	JSL Sprite_ShowMessageUnconditional
; RTL
; ;--------------------------------------------------------------------------------
; DialogSahasrahla:
; 	LDA.l $7EF374 : AND #$04 : BEQ + ;Check if player has green pendant
; 		%LoadDialogAddress(SahasrahlaAfterItemText)
; 		JSL.l Sprite_ShowMessageUnconditional
; 		RTL
; 	+
; 	%LoadDialogAddress(SahasrahlaNoPendantText)
; 	JSL.l Sprite_ShowMessageUnconditional
; RTL
; ;--------------------------------------------------------------------------------
; DialogAlcoholic:
; 	%LoadDialogAddress(AlcoholicText)
; 	JSL.l Sprite_ShowMessageUnconditional
; RTL
; ;--------------------------------------------------------------------------------
; DialogBombShopGuy:
; 	LDA.l $7EF37A : AND #$05 : CMP #$05 : BEQ + ;Check if player has crystals 5 & 6
; 		%LoadDialogAddress(BombShopGuyNoCrystalsText)
; 		JSL.l Sprite_ShowMessageUnconditional
; 		RTL
; 	+
; 	%LoadDialogAddress(BombShopGuyText)
; 	JSL.l Sprite_ShowMessageUnconditional
; RTL
;--------------------------------------------------------------------------------
; A0 - A9 - 0 - 9
; AA - C3 - A - Z
; C6 - ?
; C7 - !
; C8 - ,
; C9 - - Hyphen
; CD - Japanese period
; CE - ~
; D8 - ` apostraphe
;;--------------------------------------------------------------------------------
;DialogUncleData:
;;--------------------------------------------------------------------------------
;	.pointers
;	dl #DialogUncleData_weetabix
;	dl #DialogUncleData_bootlessUntilBoots
;	dl #DialogUncleData_onlyOneBed
;	dl #DialogUncleData_onlyTextBox
;	dl #DialogUncleData_mothTutorial
;	dl #DialogUncleData_seedWorst
;	dl #DialogUncleData_chasingTail
;	dl #DialogUncleData_doneBefore
;	dl #DialogUncleData_capeCanPass
;	dl #DialogUncleData_bootsAtRace
;	dl #DialogUncleData_kanzeonSeed
;	dl #DialogUncleData_notRealUncle
;	dl #DialogUncleData_haveAVeryBadTime
;	dl #DialogUncleData_todayBadLuck
;	dl #DialogUncleData_leavingGoodbye
;	dl #DialogUncleData_iGotThis
;	dl #DialogUncleData_raceToCastle
;	dl #DialogUncleData_69BlazeIt
;	dl #DialogUncleData_hi
;	dl #DialogUncleData_gettingSmokes
;	dl #DialogUncleData_dangerousSeeYa
;	dl #DialogUncleData_badEnoughDude
;	dl #DialogUncleData_iAmError
;	dl #DialogUncleData_sub2Guaranteed
;	dl #DialogUncleData_chestSecretEverybody
;	dl #DialogUncleData_findWindFish
;	dl #DialogUncleData_shortcutToGanon
;	dl #DialogUncleData_moonCrashing
;	dl #DialogUncleData_fightVoldemort
;	dl #DialogUncleData_redMailForCowards
;	dl #DialogUncleData_heyListen
;	dl #DialogUncleData_excuseMePrincess
;;--------------------------------------------------------------------------------
;	.weetabix
;	; We’re out of / Weetabix. To / the store!
;	db $00, $c0, $00, $ae, $00, $d8, $00, $bb, $00, $ae, $00, $ff, $00, $b8, $00, $be, $00, $bd, $00, $ff, $00, $b8, $00, $af
;	db $75, $00, $c0, $00, $ae, $00, $ae, $00, $bd, $00, $aa, $00, $ab, $00, $b2, $00, $c1, $00, $cD, $00, $ff, $00, $bd, $00, $b8
;	db $76, $00, $bd, $00, $b1, $00, $ae, $00, $ff, $00, $bc, $00, $bd, $00, $b8, $00, $bb, $00, $ae, $00, $c7
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.bootlessUntilBoots
;	; This seed is / bootless / until boots.
;	db $00, $bd, $00, $b1, $00, $b2, $00, $bc, $00, $ff, $00, $bc, $00, $ae, $00, $ae, $00, $ad, $00, $ff, $00, $b2, $00, $bc
;	db $75, $00, $ab, $00, $b8, $00, $b8, $00, $bd, $00, $b5, $00, $ae, $00, $bc, $00, $bc
;	db $76, $00, $be, $00, $b7, $00, $bd, $00, $b2, $00, $b5, $00, $ff, $00, $ab, $00, $b8, $00, $b8, $00, $bd, $00, $bc, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.onlyOneBed
;	; Why do we only / have one bed?
;	db $00, $c0, $00, $b1, $00, $c2, $00, $ff, $00, $ad, $00, $b8, $00, $ff, $00, $c0, $00, $ae, $00, $ff, $00, $b8, $00, $b7, $00, $b5, $00, $c2
;	db $75, $00, $b1, $00, $aa, $00, $bf, $00, $ae, $00, $ff, $00, $b8, $00, $b7, $00, $ae, $00, $ff, $00, $ab, $00, $ae, $00, $ad, $00, $c6
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.onlyTextBox
;	; This is the / only textbox.
;	db $00, $bd, $00, $b1, $00, $b2, $00, $bc, $00, $ff, $00, $b2, $00, $bc, $00, $ff, $00, $bd, $00, $b1, $00, $ae
;	db $75, $00, $b8, $00, $b7, $00, $b5, $00, $c2, $00, $ff, $00, $bd, $00, $ae, $00, $c1, $00, $bd, $00, $ab, $00, $b8, $00, $c1, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.mothTutorial
;	; I'm going to / go watch the / Moth tutorial.
;	db $00, $b2, $00, $d8, $00, $b6, $00, $ff, $00, $b0, $00, $b8, $00, $b2, $00, $b7, $00, $b0, $00, $ff, $00, $bd, $00, $b8
;	db $75, $00, $b0, $00, $b8, $00, $ff, $00, $c0, $00, $aa, $00, $bd, $00, $ac, $00, $b1, $00, $ff, $00, $bd, $00, $b1, $00, $ae
;	db $76, $00, $b6, $00, $b8, $00, $bd, $00, $b1, $00, $ff, $00, $bd, $00, $be, $00, $bd, $00, $b8, $00, $bb, $00, $b2, $00, $aa, $00, $b5, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.seedWorst
;	; This seed is / the worst.
;	db $00, $bd, $00, $b1, $00, $b2, $00, $bc, $00, $ff, $00, $bc, $00, $ae, $00, $ae, $00, $ad, $00, $ff, $00, $b2, $00, $bc
;	db $75, $00, $bd, $00, $b1, $00, $ae, $00, $ff, $00, $c0, $00, $b8, $00, $bb, $00, $bc, $00, $bd, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.chasingTail
;	; Chasing tail. / Fly ladies. / Do not follow.
;	db $00, $ac, $00, $b1, $00, $aa, $00, $bc, $00, $b2, $00, $b7, $00, $b0, $00, $ff, $00, $bd, $00, $aa, $00, $b2, $00, $b5, $00, $cD
;	db $75, $00, $af, $00, $b5, $00, $c2, $00, $ff, $00, $b5, $00, $aa, $00, $ad, $00, $b2, $00, $ae, $00, $bc, $00, $cD
;	db $76, $00, $ad, $00, $b8, $00, $ff, $00, $b7, $00, $b8, $00, $bd, $00, $ff, $00, $af, $00, $b8, $00, $b5, $00, $b5, $00, $b8, $00, $c0, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.doneBefore
;	; I feel like / I’ve done this / before…
;	db $00, $b2, $00, $ff, $00, $af, $00, $ae, $00, $ae, $00, $b5, $00, $ff, $00, $b5, $00, $b2, $00, $b4, $00, $ae
;	db $75, $00, $b2, $00, $d8, $00, $bf, $00, $ae, $00, $ff, $00, $ad, $00, $b8, $00, $b7, $00, $ae, $00, $ff, $00, $bd, $00, $b1, $00, $b2, $00, $bc
;	db $76, $00, $ab, $00, $ae, $00, $af, $00, $b8, $00, $bb, $00, $ae, $00, $cD, $00, $cD, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.capeCanPass
;	; Magic cape can / pass through / the barrier!
;	db $00, $b6, $00, $aa, $00, $b0, $00, $b2, $00, $ac, $00, $ff, $00, $ac, $00, $aa, $00, $b9, $00, $ae, $00, $ff, $00, $ac, $00, $aa, $00, $b7
;	db $75, $00, $b9, $00, $aa, $00, $bc, $00, $bc, $00, $ff, $00, $bd, $00, $b1, $00, $bb, $00, $b8, $00, $be, $00, $b0, $00, $b1
;	db $76, $00, $bd, $00, $b1, $00, $ae, $00, $ff, $00, $ab, $00, $aa, $00, $bb, $00, $bb, $00, $b2, $00, $ae, $00, $bb, $00, $c7
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.bootsAtRace
;	; Boots at race? / Seed confirmed / impossible.
;	db $00, $ab, $00, $b8, $00, $b8, $00, $bd, $00, $bc, $00, $ff, $00, $aa, $00, $bd, $00, $ff, $00, $bb, $00, $aa, $00, $ac, $00, $ae, $00, $c6
;	db $75, $00, $bc, $00, $ae, $00, $ae, $00, $ad, $00, $ff, $00, $ac, $00, $b8, $00, $b7, $00, $af, $00, $b2, $00, $bb, $00, $b6, $00, $ae, $00, $ad
;	db $76, $00, $b2, $00, $b6, $00, $b9, $00, $b8, $00, $bc, $00, $bc, $00, $b2, $00, $ab, $00, $b5, $00, $ae, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.kanzeonSeed
;	; If this is a / Kanzeon seed, / I'm quitting.
;	db $00, $b2, $00, $af, $00, $ff, $00, $bd, $00, $b1, $00, $b2, $00, $bc, $00, $ff, $00, $b2, $00, $bc, $00, $ff, $00, $aa
;	db $75, $00, $b4, $00, $aa, $00, $b7, $00, $c3, $00, $ae, $00, $b8, $00, $b7, $00, $ff, $00, $bc, $00, $ae, $00, $ae, $00, $ad, $00, $c8
;	db $76, $00, $b2, $00, $d8, $00, $b6, $00, $ff, $00, $ba, $00, $be, $00, $b2, $00, $bd, $00, $bd, $00, $b2, $00, $b7, $00, $b0, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.notRealUncle
;	; I am not your / real uncle.
;	db $00, $b2, $00, $ff, $00, $aa, $00, $b6, $00, $ff, $00, $b7, $00, $b8, $00, $bd, $00, $ff, $00, $c2, $00, $b8, $00, $be, $00, $bb
;	db $75, $00, $bb, $00, $ae, $00, $aa, $00, $b5, $00, $ff, $00, $be, $00, $b7, $00, $ac, $00, $b5, $00, $ae, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.haveAVeryBadTime
;	; You're going / to have a very / bad time.
;	db $00, $c2, $00, $b8, $00, $be, $00, $d8, $00, $bb, $00, $ae, $00, $ff, $00, $b0, $00, $b8, $00, $b2, $00, $b7, $00, $b0
;	db $75, $00, $bd, $00, $b8, $00, $ff, $00, $b1, $00, $aa, $00, $bf, $00, $ae, $00, $ff, $00, $aa, $00, $ff, $00, $bf, $00, $ae, $00, $bb, $00, $c2
;	db $76, $00, $ab, $00, $aa, $00, $ad, $00, $ff, $00, $bd, $00, $b2, $00, $b6, $00, $ae, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.todayBadLuck
;	; Today you / will have / bad luck.
;	db $00, $bd, $00, $b8, $00, $ad, $00, $aa, $00, $c2, $00, $ff, $00, $c2, $00, $b8, $00, $be, $00, $ff, $00, $c0, $00, $b2, $00, $b5, $00, $b5
;	db $75, $00, $b1, $00, $aa, $00, $bf, $00, $ae, $00, $ff, $00, $ab, $00, $aa, $00, $ad, $00, $ff, $00, $b5, $00, $be, $00, $ac, $00, $b4, $00, $c7
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.leavingGoodbye
;	; I am leaving / forever. / Goodbye.
;	db $00, $b2, $00, $ff, $00, $aa, $00, $b6, $00, $ff, $00, $b5, $00, $ae, $00, $aa, $00, $bf, $00, $b2, $00, $b7, $00, $b0
;	db $75, $00, $af, $00, $b8, $00, $bb, $00, $ae, $00, $bf, $00, $ae, $00, $bb, $00, $cD
;	db $76, $00, $b0, $00, $b8, $00, $b8, $00, $ad, $00, $ab, $00, $c2, $00, $ae, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.iGotThis
;	; Don’t worry. / I got this / covered.
;	db $00, $ad, $00, $b8, $00, $b7, $00, $d8, $00, $bd, $00, $ff, $00, $c0, $00, $b8, $00, $bb, $00, $bb, $00, $c2, $00, $cD
;	db $75, $00, $b2, $00, $ff, $00, $b0, $00, $b8, $00, $bd, $00, $ff, $00, $bd, $00, $b1, $00, $b2, $00, $bc
;	db $76, $00, $ac, $00, $b8, $00, $bf, $00, $ae, $00, $bb, $00, $ae, $00, $ad, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.raceToCastle
;	; Race you to / the castle!
;	db $00, $bb, $00, $aa, $00, $ac, $00, $ae, $00, $ff, $00, $c2, $00, $b8, $00, $be, $00, $ff, $00, $bd, $00, $b8
;	db $75, $00, $bd, $00, $b1, $00, $ae, $00, $ff, $00, $ac, $00, $aa, $00, $bc, $00, $bd, $00, $b5, $00, $ae, $00, $c7
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.69BlazeIt
;	; ~69 Blaze It!~
;	db $75, $00, $cE, $00, $a6, $00, $a9, $00, $ff, $00, $ab, $00, $b5, $00, $aa, $00, $c3, $00, $ae, $00, $ff, $00, $b2, $00, $bd, $00, $c7, $00, $cE
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.hi
;	; hi
;	db $75, $00, $ff, $00, $ff, $00, $ff, $00, $ff, $00, $ff, $00, $ff, $00, $b1, $00, $b2
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.gettingSmokes
;	; I'M JUST GOING / OUT FOR A / PACK OF SMOKES.
;	db $00, $b2, $00, $d8, $00, $b6, $00, $ff, $00, $b3, $00, $be, $00, $bc, $00, $bd, $00, $ff, $00, $b0, $00, $b8, $00, $b2, $00, $b7, $00, $b0
;	db $75, $00, $b8, $00, $be, $00, $bd, $00, $ff, $00, $af, $00, $b8, $00, $bb, $00, $ff, $00, $aa
;	db $76, $00, $b9, $00, $aa, $00, $ac, $00, $b4, $00, $ff, $00, $b8, $00, $af, $00, $ff, $00, $bc, $00, $b6, $00, $b8, $00, $b4, $00, $ae, $00, $bc, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.dangerousSeeYa
;	; It's dangerous / to go alone. / See ya!
;	db $00, $b2, $00, $bd, $00, $d8, $00, $bc, $00, $ff, $00, $ad, $00, $aa, $00, $b7, $00, $b0, $00, $ae, $00, $bb, $00, $b8, $00, $be, $00, $bc
;	db $75, $00, $bd, $00, $b8, $00, $ff, $00, $b0, $00, $b8, $00, $ff, $00, $aa, $00, $b5, $00, $b8, $00, $b7, $00, $ae, $00, $cD
;	db $76, $00, $bc, $00, $ae, $00, $ae, $00, $ff, $00, $c2, $00, $aa, $00, $c7
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.badEnoughDude
;	; ARE YOU A BAD / ENOUGH DUDE TO / RESCUE ZELDA?
;	db $00, $aa, $00, $bb, $00, $ae, $00, $ff, $00, $c2, $00, $b8, $00, $be, $00, $ff, $00, $aa, $00, $ff, $00, $ab, $00, $aa, $00, $ad
;	db $75, $00, $ae, $00, $b7, $00, $b8, $00, $be, $00, $b0, $00, $b1, $00, $ff, $00, $ad, $00, $be, $00, $ad, $00, $ae, $00, $ff, $00, $bd, $00, $b8
;	db $76, $00, $bb, $00, $ae, $00, $bc, $00, $ac, $00, $be, $00, $ae, $00, $ff, $00, $c3, $00, $ae, $00, $b5, $00, $ad, $00, $aa, $00, $c6
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.iAmError
;	; I AM ERROR
;	db $76, $00, $ff, $00, $ff, $00, $ff, $00, $ff, $00, $b2, $00, $ff, $00, $aa, $00, $b6, $00, $ff, $00, $ae, $00, $bb, $00, $bb, $00, $b8, $00, $bb
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.sub2Guaranteed
;	; This seed is / sub 2 hours, / guaranteed.
;	db $00, $bd, $00, $b1, $00, $b2, $00, $bc, $00, $ff, $00, $bc, $00, $ae, $00, $ae, $00, $ad, $00, $ff, $00, $b2, $00, $bc
;	db $75, $00, $bc, $00, $be, $00, $ab, $00, $ff, $00, $a2, $00, $ff, $00, $b1, $00, $b8, $00, $be, $00, $bb, $00, $bc, $00, $c8
;	db $76, $00, $b0, $00, $be, $00, $aa, $00, $bb, $00, $aa, $00, $b7, $00, $bd, $00, $ae, $00, $ae, $00, $ad, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.chestSecretEverybody
;	; The chest is / a secret to / everybody.
;	db $00, $bd, $00, $b1, $00, $ae, $00, $ff, $00, $ac, $00, $b1, $00, $ae, $00, $bc, $00, $bd, $00, $ff, $00, $b2, $00, $bc
;	db $75, $00, $aa, $00, $ff, $00, $bc, $00, $ae, $00, $ac, $00, $bb, $00, $ae, $00, $bd, $00, $ff, $00, $bd, $00, $b8
;	db $76, $00, $ae, $00, $bf, $00, $ae, $00, $bb, $00, $c2, $00, $ab, $00, $b8, $00, $ad, $00, $c2, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.findWindFish
;	; I'm off to / find the / wind fish.
;	db $00, $b2, $00, $d8, $00, $b6, $00, $ff, $00, $b8, $00, $af, $00, $af, $00, $ff, $00, $bd, $00, $b8
;	db $75, $00, $af, $00, $b2, $00, $b7, $00, $ad, $00, $ff, $00, $bd, $00, $b1, $00, $ae
;	db $76, $00, $c0, $00, $b2, $00, $b7, $00, $ad, $00, $ff, $00, $af, $00, $b2, $00, $bc, $00, $b1, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.shortcutToGanon
;	; The shortcut / to Ganon / is this way!
;	db $00, $bd, $00, $b1, $00, $ae, $00, $ff, $00, $bc, $00, $b1, $00, $b8, $00, $bb, $00, $bd, $00, $ac, $00, $be, $00, $bd
;	db $75, $00, $bd, $00, $b8, $00, $ff, $00, $b0, $00, $aa, $00, $b7, $00, $b8, $00, $b7
;	db $76, $00, $b2, $00, $bc, $00, $ff, $00, $bd, $00, $b1, $00, $b2, $00, $bc, $00, $ff, $00, $c0, $00, $aa, $00, $c2, $00, $c7
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.moonCrashing
;	; THE MOON IS / CRASHING! RUN / FOR YOUR LIFE!
;	db $00, $bd, $00, $b1, $00, $ae, $00, $ff, $00, $b6, $00, $b8, $00, $b8, $00, $b7, $00, $ff, $00, $b2, $00, $bc
;	db $75, $00, $ac, $00, $bb, $00, $aa, $00, $bc, $00, $b1, $00, $b2, $00, $b7, $00, $b0, $00, $c7, $00, $ff, $00, $bb, $00, $be, $00, $b7
;	db $76, $00, $af, $00, $b8, $00, $bb, $00, $ff, $00, $c2, $00, $b8, $00, $be, $00, $bb, $00, $ff, $00, $b5, $00, $b2, $00, $af, $00, $ae, $00, $c7
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.fightVoldemort
;	; Time to fight / he who must / not be named.
;	db $00, $bd, $00, $b2, $00, $b6, $00, $ae, $00, $ff, $00, $bd, $00, $b8, $00, $ff, $00, $af, $00, $b2, $00, $b0, $00, $b1, $00, $bd
;	db $75, $00, $b1, $00, $ae, $00, $ff, $00, $c0, $00, $b1, $00, $b8, $00, $ff, $00, $b6, $00, $be, $00, $bc, $00, $bd
;	db $76, $00, $b7, $00, $b8, $00, $bd, $00, $ff, $00, $ab, $00, $ae, $00, $ff, $00, $b7, $00, $aa, $00, $b6, $00, $ae, $00, $ad, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.redMailForCowards
;	; RED MAIL / IS FOR / COWARDS.
;	db $00, $bb, $00, $ae, $00, $ad, $00, $ff, $00, $b6, $00, $aa, $00, $b2, $00, $b5
;	db $75, $00, $b2, $00, $bc, $00, $ff, $00, $af, $00, $b8, $00, $bb
;	db $76, $00, $ac, $00, $b8, $00, $c0, $00, $aa, $00, $bb, $00, $ad, $00, $bc, $00, $cD
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.heyListen
;	; HEY! / / LISTEN!
;	db $00, $b1, $00, $ae, $00, $c2, $00, $c7
;	db $76, $00, $b5, $00, $b2, $00, $bc, $00, $bd, $00, $ae, $00, $b7, $00, $c7
;	db $7f, $7f
;;--------------------------------------------------------------------------------
;	.excuseMePrincess
;	; Well / excuuuuuse me, / princess!
;	db $00, $c0, $00, $ae, $00, $b5, $00, $b5
;	db $75, $00, $ae, $00, $c1, $00, $ac, $00, $be, $00, $be, $00, $be, $00, $be, $00, $be, $00, $bc, $00, $ae, $00, $ff, $00, $b6, $00, $ae, $00, $c8
;	db $76, $00, $b9, $00, $bb, $00, $b2, $00, $b7, $00, $ac, $00, $ae, $00, $bc, $00, $bc, $00, $c7
;	db $7f, $7f
;;-------------------------------------------------------------------------------- ^32nd
