; Patch Zelda 3 SRAM Accesses

org $0087eb
	sta $a07ffe
	lda $a063e1

org $0087fb
	sta $a063e1
	lda $a068e1
	
org $00880b
	sta $a068e1
	lda $a06de1

org $00881b
	sta $a06de1

org $0cccdd
	adc $a06000,x

org $0cccf5
	adc $a06f00,x

org $0ccd5f
	sta $a06f00,x
	sta $a06000,x
	sta $a07000,x
	sta $a06100,x
	sta $a07100,x
	sta $a06200,x
	sta $a07200,x
	sta $a06300,x
	sta $a07300,x
	sta $a06400,x

org $0ccd0a
	sta $a06f00,x
	sta $a06000,x
	sta $a07000,x
	sta $a06100,x
	sta $a07100,x
	sta $a06200,x
	sta $a07200,x
	sta $a06300,x
	sta $a07300,x
	sta $a06400,x

org $0ccdfa
	lda $a063e1,x

	
org $1befa0
	lda $a06354
org $1befa6
	lda $a0635b
org $1befb0
	lda $a06359
org $1befba
	lda $a0635a
org $1befc4
	lda $a06854
org $1befca
	lda $a0685b
org $1befd4
	lda $a06859
org $1befde
	lda $a0685a
org $1befe8
	lda $a06d54
org $1befee
	lda $a06d5b
org $1beff8
	lda $a06d59
org $1bf002
	lda $a06d5a

org $0cd79b
	sta $a06000,x
	sta $a06100,x
	sta $a06200,x
	sta $a06300,x
	sta $a06400,x
	
org $0cd7be
	sta $a063d9,x
	sta $a063db,x	
	sta $a063dd,x	
	sta $a063df,x
	
org $0cdb11
	sta $a063d9,x

org $0cdca9
	lda $a063d9,x

org $0cdb25
	lda $a063d9,x
	
org $0cdb4c
	sta $a07ffe

org $0cdb5b
	sta $a063e1,x
org $0cdb62
	sta $a0620c,x
org $0cdb66
	sta $a0620e,x
org $0cdb6d
	sta $a06401,x

org $0cdb8a
	lda $a063d9

org $0cdb96
	sta $a06212,x
org $0cdb9d
	sta $a063c5,x
org $0cdba4
	sta $a063c7,x

org $0cdbae
	sta $a06340,x

org $0cdbc1
	adc $a06000,x

org $0cdbd7
	sta $a064fe,x
	
org $0cd5d9
	lda $a06359,x

org $0cd626
	lda $a0635a,x

org $0cd6c4
	lda $a06401,x
	
org $0cd52c
	lda $a063d9,x

org $0cd54c
	lda $a0636c,x
 
org $0cce85
	sta $a07ffe
	
org $0cced8
	lda $a06000,x

org $0ccedf
	lda $a06100,x

org $0ccee6
	lda $a06200,x

org $0cceed
	lda $a06300,x

org $0ccef4
	lda $a06400,x

org $0eefeb
	lda $a07ffe

org $0eeff5
	lda $a063d9,x
	
org $0ef011
	lda $a063db,x

org $0ef02d
	lda $a063dd,x
	
org $0ef049
	lda $a063df,x
	
org $00894b
	lda #$a0
	
org $008951
	ldx $7ffe
	
org $008961
	sta $6000,y
	sta $6f00,y
	
org $00896b
	sta $6100,y
	sta $7000,y

org $008975
	sta $6200,y
	sta $7100,y

org $00897f
	sta $6300,y
	sta $7200,y
	
org $008989
	sta $6400,y
	sta $7300,y
	
org $0089b6
	sta $a064fe,x
	sta $a073fe,x

org $0cd4d3
	sta.l $a06000,x
	sta.l $a06100,x
	sta.l $a06200,x
	sta.l $a06300,x
	sta.l $a06400,x
	sta.l $a06f00,x
	sta.l $a07000,x
	sta.l $a07100,x
	sta.l $a07200,x
	sta.l $a07300,x

org $0cd2d1
	lda.b #$a0

org $0cd2dc
	lda $6000,x
	sta $6000,y
	lda $6100,x
	sta $6100,y
	lda $6200,x
	sta $6200,y
	lda $6300,x
	sta $6300,y
	lda $6400,x
	sta $6400,y

