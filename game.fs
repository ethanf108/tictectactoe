include ffl/dtm.fs


begin-structure game%
  field: game>id          \ id (string)
  field: game>id-len      \ id len (string len)
  field: game>player-1-id \ player 1's ID
  field: game>player-2-id \ player 2's ID
  field: game>turn        \ whose turn it is (0 or -1)
  field: game>done        \ is game done? (bool)
  16 +field game>board    \ game board (byte array, row major order)
  \ 0 1 2 3
  \ 4 5 6 7 ..
  dtm% +field game>start-time
end-structure

: game-new ( c-addr u -- )
  game% allocate throw
  dup 2swap rot game>id-len !
  over game>id !
  0 over game>turn !
  0 over game>done !
  0 over game>player-1-id !
  0 over game>player-2-id !
  16 0 do \ setup board
    dup game>board I 0 rot rot + c!
  loop
  dup game>start-time dtm-init ;

\ dtm-free is broken?? idk. guess we have a 56 byte mem leak lol
: game-free ( game -- )
  \ dup game>start-time dtm-free
  free throw ;

\ checks if piece has been played
: game-piece-played? ( piece game -- bool )
  game>board swap
  \ 15 and \ AND the b00001111 bits.
  16 0 do
    over I + c@ over = if 2drop -1 unloop exit then
  loop
  2drop 0 ;

: game-slot-free? ( u game -- bool )
  game>board + c@ 0 = if -1 else 0 then ;

: game-play-piece ( piece pos game -- )
  game>board + c! ;

: game-get-piece ( pos game -- piece )
  game>board + c@ ;

: game-dump-board ( game -- )
  16 0 do dup game>board I + c@ . loop drop ;

: game-(check-util) ( u u u u -- bool)
  and and and 0 =
  if 0 else -1 then ;

: game-(invert-util) ( u -- u )
    \ lmao remember when i did this
    \ dup 15 and invert swap 15 invert and or ;
    \ this isn't even needed anymore lmao
    15 xor ;

: game-won? ( game -- bool )
  game>board
  \ dup ( board board -- )
  4 0 do
    dup
    4 0 do \ rows
     dup 4 J * I + + c@ swap
    loop
    drop game-(check-util) swap
    
    dup
    4 0 do \ cols
     dup 4 I * J + + c@ swap
    loop
    drop game-(check-util) swap
  loop
  
  dup
  4 0 do dup 4 I * I + + c@ swap loop \ \ diagonal
  drop game-(check-util) swap
  
  dup
  4 0 do dup 4 I 1 + * I - + c@ swap loop \ / diagonal
  drop game-(check-util) swap

  3 0 do \ circles
      3 0 do
	  dup
	  dup I J 4 * + + c@ swap
	  dup I 1 + J 4 * + + c@ swap
	  dup I J 1 + 4 * + + c@ swap
	  dup I 1 + J 1 + 4 * + + c@ swap
	  drop game-(check-util) swap
      loop
  loop
  
  drop
  18 0 do or loop ;

: get-current-player-id ( game -- u )
  dup game>turn @ if
    game>player-2-id
  else
    game>player-1-id
  then @ ;

\ TODO implement squares for win checking 
