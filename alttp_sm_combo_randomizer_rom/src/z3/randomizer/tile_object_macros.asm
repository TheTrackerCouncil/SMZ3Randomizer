macro tile_object_type1(i, x, y, s)  ; oid < $F8
    ; xxxx_xxss yyyy_yySS iiii_iiii (size ssSS)
    db ((<x><<2)|(<s>>>2)), ((<y><<2)|(<s>&$3)), <i>
endmacro

macro tile_object_type2(i, x, y)     ; oid in [$100,$13F]
    ; 1111_11xx xxxx_yyyy yyii_iiii (oid 1_00ii_iiii)
    db ($FC|(<x>>>4)), (((<x>&$F)<<4)|(<y>>>2)), (((<y>&$3)<<6)|<i>)
endmacro

macro tile_object_type3(i, x, y)     ; oid >= $F80
    ; xxxx_xxss yyyy_yySS 1111_1iii (oid 1111_1iii_SSss)
    db ((<x><<2)|(<i>&$3)), ((<y><<2)|((<i>&$C)>>2)), (<i>>>4)
endmacro
