Overworld_LoadNewTiles:
{
    ; ; add sign to EDM for OWG people to read
    ; LDA $040A : AND #$00FF : CMP #$0005 : BNE +
    ;     LDA #$0101 : STA $7E2E18 ; #$0101 is the sign tile16 id, $7E2D98 is the position of the tile16 on map
    ; +

    ; GT sign
    LDA $040A : AND #$00FF : CMP #$0043 : BNE +
        LDA #$0101 : STA $7E2550
    +

    ; Pyramid sign
    LDA $040A : AND #$00FF : CMP #$005B : BNE +
        LDA #$0101 : STA $7E27B6 ;Moved sign near statue
        LDA #$05C2 : STA $7E27B4 ;added a pyramid peg on the left of the sign
    +

    LDX #$001E : LDA #$0DBE
    RTL
}