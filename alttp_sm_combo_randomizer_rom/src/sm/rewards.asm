; $A7:C831 9F 28 D8 7E STA $7ED828,x[$7E:D828]    ; Set Kraid as dead
; $A7:DB85 9F 28 D8 7E STA $7ED828,x[$7E:D82B]    ; Set Phantoon as dead
; $A5:92DE 9F 28 D8 7E STA $7ED828,x[$7E:D82C]    ; Set Draygon as dead
; $A6:C5E2 22 A6 81 80 JSL $8081A6[$80:81A6]      ; Set Ridley as dead

; Hook Kraid death
org $e7c831
    jsl boss_death_kraid
org $e7db85
    jsl boss_death_phantoon
org $e592de
    jsl boss_death_draygon
org $e6c5e2
    jsl boss_death_ridley

org $cfa68f
    dw $91d6    ; Disable G4 Statue Animations
                ; G4 opening is only triggered by the new event flags
                

org $d9f000
base $99f000
boss_death_kraid:
    sta $7ed828,x   ; Store actual boss kill flag
    lda #$0000
    jsr boss_death_reward
    rtl

boss_death_phantoon:
    sta $7ed828,x   ; Store actual boss kill flag
    lda #$0008
    jsr boss_death_reward
    rtl

boss_death_draygon:
    sta $7ed828,x   ; Store actual boss kill flag
    lda #$0010
    jsr boss_death_reward
    rtl

boss_death_ridley:
    jsl $8081a6    ; Store actual boss kill flag
    lda #$0018
    jsr boss_death_reward
    rtl


boss_death_reward:
    phx : php : tax

    ; Load boss reward type from table
    %a8()
    lda.l boss_rewards, x
    sta $c7
    beq .pendant
    cmp #$40
    beq .crystal
    bra .smboss

    .pendant
        lda.l boss_rewards+$2, x
        sta $c9
        ora.l !SRAM_ALTTP_ITEM_BUF+$74
        sta.l !SRAM_ALTTP_ITEM_BUF+$74        
        bra .exit
    .crystal
        lda.l boss_rewards+$2, x
        sta $c9
        ora.l !SRAM_ALTTP_ITEM_BUF+$7A
        sta.l !SRAM_ALTTP_ITEM_BUF+$7A
        bra .exit
    .smboss
        ; Save to special event bits for boss tokens (not the actual boss kill flags that opens doors)
        ; Although in a future update adding the option of this also setting those flags should be a possibility
        lda.l boss_rewards+$2, x
        sta $c9
        ora.l $7ed832
        sta.l $7ed832
        bra .exit

    .exit

    %ai16()

    ; Show message box
    lda #$0063
    jsl $858080

    plp : plx
    rts

rewards_draw_map_icons:
    phx : phy : phb
    pea $8282 : plb :  plb

    lda !SRAM_ALTTP_ITEM_BUF+$7A : bit #$0002 : beq +
        ldy.w #BlueCrystal_Icon_Spritemap
        lda #$0020 : sta $12
        lda #$0002 : sta $14
        lda #$3e00 : sta $16
        jsl $81879f
    +
    
    lda !SRAM_ALTTP_ITEM_BUF+$7A : bit #$0010 : beq +
        ldy.w #BlueCrystal_Icon_Spritemap
        lda #$0020 : sta $12
        lda #$000A : sta $14
        lda #$3e00 : sta $16
        jsl $81879f
    +

    lda !SRAM_ALTTP_ITEM_BUF+$7A : bit #$0040 : beq +
        ldy.w #BlueCrystal_Icon_Spritemap
        lda #$0020 : sta $12
        lda #$0012 : sta $14
        lda #$3e00 : sta $16
        jsl $81879f
    +

    lda !SRAM_ALTTP_ITEM_BUF+$7A : bit #$0020 : beq +
        ldy.w #BlueCrystal_Icon_Spritemap
        lda #$0020 : sta $12
        lda #$001A : sta $14
        lda #$3e00 : sta $16
        jsl $81879f
    +

    lda !SRAM_ALTTP_ITEM_BUF+$7A : bit #$0004 : beq +
        ldy.w #RedCrystal_Icon_Spritemap
        lda #$0020 : sta $12
        lda #$0022 : sta $14
        lda #$3e00 : sta $16
        jsl $81879f
    +

    lda !SRAM_ALTTP_ITEM_BUF+$7A : bit #$0001 : beq +
        ldy.w #RedCrystal_Icon_Spritemap
        lda #$0020 : sta $12
        lda #$002A : sta $14
        lda #$3e00 : sta $16
        jsl $81879f
    +

    lda !SRAM_ALTTP_ITEM_BUF+$7A : bit #$0008 : beq +
        ldy.w #BlueCrystal_Icon_Spritemap
        lda #$0020 : sta $12
        lda #$0032 : sta $14
        lda #$3e00 : sta $16
        jsl $81879f
    +

    lda !SRAM_ALTTP_ITEM_BUF+$74 : bit #$0004 : beq +
        ldy.w #GreenPendant_Icon_Spritemap
        lda #$0020 : sta $12
        lda #$003A : sta $14
        lda #$3e00 : sta $16
        jsl $81879f
    +

    lda !SRAM_ALTTP_ITEM_BUF+$74 : bit #$0002 : beq +
        ldy.w #BluePendant_Icon_Spritemap
        lda #$0020 : sta $12
        lda #$0042 : sta $14
        lda #$3e00 : sta $16
        jsl $81879f
    +

    lda !SRAM_ALTTP_ITEM_BUF+$74 : bit #$0001 : beq +
        ldy.w #RedPendant_Icon_Spritemap
        lda #$0020 : sta $12
        lda #$004A : sta $14
        lda #$3e00 : sta $16
        jsl $81879f    
    +

    lda $7ed832 : bit #$0001 : beq +
        ldy.w #BossKraid_Icon_Spritemap
        lda #$0020 : sta $12
        lda #$00DE : sta $14
        lda #$3e00 : sta $16
        jsl $81879f
    +

    lda $7ed832 : bit #$0002 : beq +
        ldy.w #BossPhantoon_Icon_Spritemap
        lda #$0020 : sta $12
        lda #$00E6 : sta $14
        lda #$3e00 : sta $16
        jsl $81879f
    +

    lda $7ed832 : bit #$0004 : beq +
        ldy.w #BossDraygon_Icon_Spritemap
        lda #$0020 : sta $12
        lda #$00EE : sta $14
        lda #$3e00 : sta $16
        jsl $81879f
    +

    lda $7ed832 : bit #$0008 : beq +
        ldy.w #BossRidley_Icon_Spritemap
        lda #$0020 : sta $12
        lda #$00F6 : sta $14
        lda #$3e00 : sta $16
        jsl $81879f   
    +

    plb : ply : plx
    rtl