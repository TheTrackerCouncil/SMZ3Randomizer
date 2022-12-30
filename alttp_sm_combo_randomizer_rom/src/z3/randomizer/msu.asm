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
!MSU_REPEAT = $2007

!VAL_VOLUME_INCREMENT = #$10
!VAL_VOLUME_DECREMENT = #$02
!VAL_VOLUME_MUTE = #$0
!VAL_VOLUME_HALF = #$40
!VAL_VOLUME_FULL = #$FF
!VAL_MSU_FLAG = #$2D53

; Checks the status of the MSU and either mutes the SPC
; if it is playing propertly or re-enables the SPC
check_msu:
    .init
        STA !MSU_CURRENT_COMMAND : PHA ; Retrieve current MSU Command and push it to the stack
        LDA !MSU_FLAG : CMP !VAL_MSU_FLAG : BNE .msu_not_found ; If MSU isn't supported
        LDA !MSU_STATUS : AND #$08 : CMP #$08 : BEQ .msu_not_found ; If the current MSU command says that the MSU wasn't found
        BRA .msu_found ; Otherwise the msu is found and is playing

    .msu_found:
        LDA #$F1 : STA !SPC_CONTROL ; Mute the original SPC comand
        PLA ; Clear item from the stack
        JML check_msu_continue

    .msu_not_found:
        PLA : STA !SPC_CONTROL ; Apply the command from the stack that was saved
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

    .no_msu
        SEP #$30
        BRA .exit

    .check_music_request
        LDX !SPC_MUSIC_REQUEST ; Load the PCM music request to X
        CPX #$F1 : BEQ .fade_out
        CPX #$F2 : BEQ .fade_half
        CPX #$F3 : BEQ .full_volume
        CPX #$FF : BEQ .exit
        CPX #0 : BNE .check_new_song
        LDA !MSU_CURRENT_VOLUME : CMP !MSU_TARGET_VOLUME : BNE .change_volume
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
        LDA !MSU_TARGET_VOLUME : BRA .save_volume

    .save_volume
        STA $2006 : STA !MSU_CURRENT_VOLUME
        BRA .exit

    .fade_out
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

    ; Checks if a new song value is stored in X or if
    ; it is the same song from changing screens
    .check_new_song
        CPX !MSU_CURRENT_TRACK : BNE .change_song ; If the track changed, play it
        BRA .exit

    ; Plays an MSU based on the song value stored in X
    .change_song
        STX !MSU_CURRENT_TRACK ; Saves the track being played
        STX !MSU_TRACK_LO : STZ !MSU_TRACK_HI ; Sets the MSU track from X
        LDA.l MSUTrackList,X : STA !MSU_REPEAT ; Sets the track repeat flag from the track table
        LDA !VAL_VOLUME_FULL : STA !MSU_TARGET_VOLUME
        BRA .exit


pendant_fanfare:
    .init
        REP #$20
        LDA !MSU_FLAG : CMP !VAL_MSU_FLAG : BNE .spc
        SEP #$20
        LDA !MSU_STATUS : BIT #$08 : BNE .spc
        LDA !MSU_STATUS : BIT #$10 : BEQ .done
    .continue
        jml pendant_continue
    .spc
        SEP #$20
    .done
        jml pendant_done

crystal_fanfare:
    .init
        REP #$20
        LDA !MSU_FLAG : CMP !VAL_MSU_FLAG : BNE .spc
        SEP #$20
        LDA !MSU_STATUS : BIT #$08 : BNE .spc
        LDA !MSU_STATUS : BIT #$10 : BEQ .done
    .continue
        jml crystal_continue
    .spc
        SEP #$20
    .done
        jml crystal_done

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