!SPC_MUSIC_REQUEST = $012C
!SPC_MUSIC_CONTROL = $012B
!SPC_CONTROL = $2140

!MSU_CURRENT_VOLUME = $0127
!MSU_TARGET_VOLUME = $0129
!MSU_CURRENT_TRACK = $0130
!MSU_CURRENT_COMMAND = $0133

!VAL_VOLUME_INCREMENT = #$10
!VAL_VOLUME_DECREMENT = #$02
!VAL_VOLUME_MUTE = #$0
!VAL_VOLUME_HALF = #$40
!VAL_VOLUME_FULL = #$FF

check_msu:
    STA !MSU_CURRENT_COMMAND : PHA ; Retrieve current MSU Command and push it to the stack
    LDA $2000 : AND #$08 : CMP #$08 : BEQ .msu_not_found ; If the current MSU command says that the MSU wasn't found
    BRA .msu_found ; Otherwise the msu is found and is playing


.msu_found:
    LDA #$F1 : STA !SPC_CONTROL ; Mute the original SPC comand
    PLA
    JML check_msu_continue

.msu_not_found:
    PLA : STA $2140
    JML check_msu_continue

msu_main:
    SEP #$20    ; set 8-BIT accumulator
    LDA $4210   ; thing we wrote over
    REP #$20    ; set 16-BIT accumulator
    LDA $2002 : CMP #$2D53 : BNE .no_msu
    SEP #$30
    BRA .check_music_request

    .no_msu
        SEP #$30
        JML spc_continue
        BRA .check_music_request

    .check_music_request
        LDX !SPC_MUSIC_REQUEST ; Load the PCM music request to X
        CPX #$F1 : BEQ .fade_out
        CPX #$F2 : BEQ .fade_half
        CPX #$F3 : BEQ .full_volume
        CPX #$FF : BEQ .exit_msu
        CPX #0 : BNE .check_new_song
        LDA !MSU_CURRENT_VOLUME : CMP !MSU_TARGET_VOLUME : BNE .change_volume
        JML spc_continue

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
        JML spc_continue

    .fade_out
        LDA !VAL_VOLUME_MUTE : CMP !MSU_TARGET_VOLUME : BEQ .change_volume
        STA !MSU_TARGET_VOLUME
        JML spc_continue

    .fade_half
        LDA !VAL_VOLUME_HALF : CMP !MSU_TARGET_VOLUME : BEQ .change_volume
        STA !MSU_TARGET_VOLUME
        JML spc_continue

    .full_volume
        LDA !VAL_VOLUME_FULL : CMP !MSU_TARGET_VOLUME : BEQ .change_volume
        STA !MSU_TARGET_VOLUME
        JML spc_continue

    .exit_msu
        JML spc_continue

    ; Checks if a new song value is stored in X or if
    ; it is the same song from changing screens
    .check_new_song
        CPX !MSU_CURRENT_TRACK : BNE .change_song ; If the track changed, play it
        ;LDX.b #00 : STX !SPC_MUSIC_REQUEST ; Otherwise mute the SPC again
        JML spc_continue

    ; Plays an MSU based on the song value stored in X
    .change_song
        ;TXA
        ;STA $7e0055 ; test
        STX !MSU_CURRENT_TRACK ; Saves the track being played
        STX $2004 : STZ $2005 ; Sets the MSU track from X
        LDA.l MSUTrackList,X : STA $2007 ; Sets the track code from the track table
        LDA !VAL_VOLUME_FULL : STA !MSU_TARGET_VOLUME
        ; LDA #$F1 : STA $2140
        ;STA $7e0056 ; test
        ;LDX.b #00 : STX !SPC_MUSIC_REQUEST ; Clear music request to mute SPC
        JML spc_continue


pendant_fanfare:
        REP #$20
        LDA $2002 : CMP #$2D53 : BNE .spc
        SEP #$20
        LDA $2000 : BIT #$08 : BNE .spc
        LDA $2000 : BIT #$10 : BEQ .done
    .continue
        jml pendant_continue
    .spc
        SEP #$20
    .done
        jml pendant_done

crystal_fanfare:
        REP #$20
        LDA $2002 : CMP #$2D53 : BNE .spc
        SEP #$20
        LDA $2000 : BIT #$08 : BNE .spc
        LDA $2000 : BIT #$10 : BEQ .done
    .continue    
        LDA #2
        STA $7e0056
        jml crystal_continue
    .spc
        SEP #$20
    .done
        LDA #1
        STA $7e0056
        jml crystal_done

ending_wait:
        REP #$20
        LDA $2002 : CMP #$2D53 : BNE .done
        SEP #$20
    .wait
        LDA $2000 : BIT #$10 : BNE .wait
    .done
        SEP #$20
        LDA #$22
        RTL