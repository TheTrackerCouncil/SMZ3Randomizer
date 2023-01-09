!SPC_MUSIC_REQUEST = $012C
!SPC_MUSIC_CONTROL = $012B
!SPC_CONTROL = $2140

!MSU_CURRENT_VOLUME = $0127
!MSU_TARGET_VOLUME = $0129
!MSU_CURRENT_TRACK = $0130
!MSU_CURRENT_COMMAND = $0133
!MSU_STATUS = $2000
!MSU_FLAG = $2002
!MSU_TRACK_LO = $2004
!MSU_TRACK_HI = $2005
!MSU_VOLUME = $2006
!MSU_REPEAT = $2007
!MSU_LAST_PLAYED_TRACK = $7F50AB

!VAL_VOLUME_INCREMENT = #$10
!VAL_VOLUME_DECREMENT = #$02
!VAL_VOLUME_MUTE = #$0
!VAL_VOLUME_HALF = #$80
!VAL_VOLUME_FULL = #$FF
!VAL_MSU_FLAG = #$2D53
!VAL_OFFSET = #100

; Checks the status of the MSU and either mutes the SPC
; if it is playing propertly or re-enables the SPC
check_msu:
    .init
        STA !MSU_CURRENT_COMMAND : PHA ; Retrieve current MSU Command and push it to the stack
        REP #$20
        LDA !MSU_FLAG : CMP !VAL_MSU_FLAG : BNE .msu_not_found ; If MSU isn't supported
        SEP #$30
        LDA !MSU_STATUS : AND #$08 : CMP #$08 : BEQ .msu_not_found ; If the current MSU command says that the MSU wasn't found
        BRA .msu_found ; Otherwise the msu is found and is playing

    .msu_found:
        LDA #$F1 : STA !SPC_CONTROL ; Mute the original SPC comand
        PLA ; Clear item from the stack
        JML check_msu_continue

    .msu_not_found:
        SEP #$30
        ; Load the command from the stack, but correct it for where 
        ; falling from Ganon throws 0xE0 (224) instead of 0x1C (28)
        PLA : CMP #$E0 : BNE +
            LDA #$1C
        +
        STA !SPC_CONTROL ; Apply the command from the stack that was saved
        JML check_msu_continue

; Main method that listens for if new songs are being requested
; and played the appropriate MSU for them
msu_main:
    .init
        SEP #$20    ; set 8-BIT accumulator
        LDA $4210   ; thing we wrote over
        REP #$20    ; set 16-BIT accumulator
        LDA !MSU_FLAG : CMP !VAL_MSU_FLAG : BNE .no_msu ; Verify that MSU is supported
        SEP #$30
        BRA .check_music_request

    .check_music_request
        LDX !SPC_MUSIC_REQUEST ; Load the PCM music request to X
        CPX #$F1 : BEQ .fade_out
        CPX #$F2 : BEQ .fade_half
        CPX #$F3 : BEQ .full_volume
        CPX #$FF : BEQ .exit
        CPX #0 : BNE .determine_song
        LDA !MSU_CURRENT_VOLUME : CMP !MSU_TARGET_VOLUME : BNE .change_volume
        BRA .exit

    .no_msu
        SEP #$30
        BRA .exit

    .change_volume
        LDA !MSU_CURRENT_VOLUME
        CMP !MSU_TARGET_VOLUME
        BCC .increase_volume
        BRA .decrease_volume

    .increase_volume
        ADC !VAL_VOLUME_INCREMENT : BCC .save_volume
        LDA !MSU_TARGET_VOLUME : BRA .save_volume

    .decrease_volume
        SBC !VAL_VOLUME_DECREMENT : BCS .save_volume
        ; Clear the last played song when muting so that it'll play if
        ; the same song is reloaded (save and restart in Sanc for example)
        LDA #0 : STA !MSU_LAST_PLAYED_TRACK
        LDA !MSU_TARGET_VOLUME : BRA .save_volume

    .save_volume
        STA !MSU_VOLUME : STA !MSU_CURRENT_VOLUME
        BRA .exit

    .fade_out
        ; Prevent fading out when leaving the under the bridge area
        LDA $7E008A : CMP #$80 : BEQ .exit
        LDA !VAL_VOLUME_MUTE : CMP !MSU_TARGET_VOLUME : BEQ .change_volume
        STA !MSU_TARGET_VOLUME
        BRA .exit

    .fade_half
        LDA !VAL_VOLUME_HALF : CMP !MSU_TARGET_VOLUME : BEQ .change_volume
        STA !MSU_TARGET_VOLUME
        BRA .exit

    .full_volume
        LDA !VAL_VOLUME_FULL : CMP !MSU_TARGET_VOLUME : BEQ .change_volume
        STA !MSU_TARGET_VOLUME
        BRA .exit

    .exit
        JML spc_continue

    ; Determines which song needs to be played based off of the SPC that was requested
    .determine_song
        CPX #2 : BEQ .load_light_world_song
        CPX #16 : BEQ .load_castle_song
        CPX #17 : BEQ .load_dungeon_song
        CPX #22 : BEQ .load_dungeon_song
        CPX #21 : BEQ .load_boss_song
        CPX #13 : BEQ .load_dark_mountain_woods_song
        CPX #9 : BEQ .load_dark_world_song

        ; For some reason falling from Ganon throws 0xE0 (224) instead of 0x1C (28)
        CPX #$E0 : BNE +
            LDX #28
        +

        BRA .change_song

    .load_light_world_song
        LDA $7EF300 : AND #$40 : CMP #$40 : BNE +
            LDX #60 : BRA ++
        +
        LDA $008A : CMP #$00 : BNE ++
            LDX #5
        ++
        BRA .change_song

    .load_dungeon_song
        LDA $040C : LSR : !ADD #$21 : TAX
        BRA .change_song

    .load_boss_song
        LDA $040C : LSR : !ADD #$2D : TAX
        BRA .change_song

    .load_castle_song
        LDA $040C : CMP #$08 : BNE +
            LDX #37
        +
        BRA .change_song

    .load_dark_world_song
        LDA $7EF37A : CMP #$7F : BNE +
            LDX #61
        +
        BRA .change_song

    .load_dark_mountain_woods_song
        LDA $008A : CMP #$40 : BNE +
            LDX #15
        +
        LDA $7EF37A : CMP #$7F : BNE +
            LDX #61
        +
        BRA .change_song

    ; Plays an MSU based on the song value stored in X
    .change_song
        TXA : !ADD !VAL_OFFSET ; Sets A to song and adds 100 for the offset
        CMP !MSU_LAST_PLAYED_TRACK : BEQ .done ; If the track is the same, ignore it
        STA !MSU_LAST_PLAYED_TRACK : STA !MSU_CURRENT_TRACK ; Saves the track being played
        STA !MSU_TRACK_LO : STZ !MSU_TRACK_HI ; Sets the MSU track from A
        LDA.l MSUTrackFlags-1,X : STA !MSU_REPEAT ; Sets the track repeat flag from the track table
        LDA !VAL_VOLUME_FULL : STA !MSU_TARGET_VOLUME : STA !MSU_CURRENT_VOLUME : STA !MSU_VOLUME
        BRA .done

    .done
        JML spc_continue

pendant_fanfare:
    .init
        REP #$20
        LDA !MSU_FLAG : CMP !VAL_MSU_FLAG : BNE .spc
        SEP #$20
        LDA !MSU_STATUS : BIT #$08 : BNE .spc
        LDA !MSU_STATUS : BIT #$10 : BEQ .done
    .continue
        JML pendant_continue
    .spc
        SEP #$20
    .done
        JML pendant_done

crystal_fanfare:
    .init
        REP #$20
        LDA !MSU_FLAG : CMP !VAL_MSU_FLAG : BNE .spc
        SEP #$20
        LDA !MSU_STATUS : BIT #$08 : BNE .spc
        LDA !MSU_STATUS : BIT #$10 : BEQ .done
    .continue
        JML crystal_continue
    .spc
        SEP #$20
    .done
        JML crystal_done

ending_wait:
    .init
        REP #$20
        LDA !MSU_FLAG : CMP #$2D53 : BNE .done
        SEP #$20
    .wait
        LDA !MSU_STATUS : BIT #$10 : BNE .wait
    .done
        SEP #$20
        LDA #$22
        RTL

SpiralStairsPreCheck:
    .init
        REP #$20
        LDA !MSU_FLAG : CMP !VAL_MSU_FLAG : BNE .done
        LDA $A0

        ; Fade out going when going to GT lobby when GT climb music is playing
        CMP.w #$000C : BNE +
            LDA !MSU_CURRENT_TRACK : AND.w #$00FF : CMP.w #59 : BEQ .fade
            BRA .done
        +

        ; Fade out going upstairs from GT lobby when you have the GT big key
        CMP.w #$006B : BNE +
            LDA !MSU_CURRENT_TRACK : AND.w #$00FF : CMP.w #46 : BNE ++
                LDA $7EF366 : AND.w #$0004 : CMP.w #$0004 : BNE .done
                BRA .fade
            ++
            LDA !MSU_CURRENT_TRACK : AND.w #$00FF : CMP.w #22 : BNE ++
                LDA $7EF366 : AND.w #$0004 : CMP.w #$0004 : BNE .done
                BRA .fade
            ++
            BRA .done
        +

        BRA .done


    .fade
        LDX.b #$F1 : STX !SPC_MUSIC_REQUEST

    .done
        LDA $A0
        RTL

SpiralStairsPostCheck:
    .init
        REP #$20
        LDA !MSU_FLAG : CMP !VAL_MSU_FLAG : BNE .done
        LDA $A0

        ; If we're faded out from climbing down the stairs
        CMP.w #$000C : BNE +
            LDX !MSU_CURRENT_TRACK : CPX.b #$F1 : BNE ++ ; TODO: Figure out why current track gets set to 0xF1
                LDX.b #46 : STX !SPC_MUSIC_REQUEST
            ++
            BRA .done
        +

        ; If we're faded out from climbing up the stairs
        CMP.w #$006B : BNE +
            LDX !MSU_CURRENT_TRACK : CPX.b #$F1 : BNE ++ ; TODO: Figure out why current track gets set to 0xF1
                LDX.b #59 : STX !SPC_MUSIC_REQUEST
            ++
            BRA .done
        +

    .done
        SEP #20
        LDA $A0
        RTL