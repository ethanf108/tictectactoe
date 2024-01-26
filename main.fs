include ffl/spf.fs
include ffl/scf.fs
include ffl/dtm.fs
include ffl/hct.fs
include ffl/rng.fs

include util.fs
include game.fs

include 1991.fs

sourcedir s" static" s+ set-public-path
sourcedir s" static" s+ set-view-path

1024 dup * 100 * hct-new value game-table

34 rng-new value rand

: log-handle ( c-addr u -- )
  s" HANDLE " type
  ( input ) type
  s"  ?" type
  get-query-string type
  s"  " type .s
  CR ;

: log-error
    s" ERROR " 2swap s+
    2dup type
    s"  " type
    2>R .s 2R> CR ;

: get-query-param ( c-addr u -- false | c-addr u true )
  ( needle -- value? )
  get-query-string
  1 + strcpy 2dup + 1 - '&' swap C! \ add an & to the end
  tis-new dup >R tis-set ( needle -- )
  begin
    '&' R@ tis-scan-char if
      2over 2swap key>value if
	-1 -1
      else
	0
      then
    else
      0 -1
    then
  until
  R> tis-free
  if
    2swap 2drop -1
  else
    2drop 0
  then ;

: game ( -- 0 | game )
  s" id" get-query-param invert if
    0
  else
    game-table hct-get invert if
      0
    then
  then ;

: game? ( -- bool )
  game 0 = invert ;

: handle-/
  s" /" log-handle
  s" /index.html" render-view ;

: handle-/board
  s" /board " log-handle
  game? invert if s" no game" log-error exit then
  game game>board
  str-new dup >R >R
  16 0 do
    dup 15 I - + c@ swap 
  loop
    drop
    s" [%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u]" R> spf-set
    R> str-get ;

: handle-/play
  s" /play" log-handle
  
  game? invert if
    s" no game" log-error exit
  then
  
  game game>done @ if
    s" game is done" log-error exit
  then

  s" player" get-query-param invert if
    s" no player" log-error exit
  then

  s" %u" scf+scan 1 = invert if
    s" invalid player format" log-error exit
  then

  game game>player-2-id @ 0 = if
      s" game not started" log-error exit
  then
  
  game get-current-player-id = invert if
    s" wrong player lol" log-error exit
  then
    
  \ piece, pos
  s" body" get-query-param invert if
      s" no body" log-error exit
  then
  
  \ check if format is good
  s" %u,%u" scf+scan 2 = invert if
    s" invalid format" log-error exit
  then
  
  \ check if inputs are valid (format-wise)
  2dup 16 <
  swap bit-count 4 =
  and invert if
    2drop s" invalid numbers" log-error exit
  then
  
  \ check if inputs are valid (game-wise)
  2dup swap game game-piece-played?
  swap game game-slot-free? invert
  or if
    2drop s" can't play that piece there" log-error exit
  then
  game game-play-piece
  
  game game-won? if
    -1 game game>done !
    game get-current-player-id s" WON %u" str-new dup >R spf-set R> str-get
  else
    game game>turn dup @ invert swap !
    s" OK"
  then ;

: handle-/new ( -- )
  s" /new" log-handle
  s" player" get-query-param invert if
    s" no player" log-error exit
  then
  s" %u" scf+scan 1 = invert if
    s" invalid player format" log-error exit
  then
  dup 0 = if
    drop s" invalid player number" log-error exit
  then
  >R

  str-new >R
  rand rng-next-number
  s" %u" R@ spf-set
  R> str-get 2dup 2dup
  game-new dup >R
  rot rot game-table hct-insert
  2R> game>player-1-id ! ;

: handle-/join ( -- )
  s" /join" log-handle
  game? invert if
    s" no game" log-error exit
  then
  
  s" player" get-query-param invert if
    s" no player" log-error exit
  then
  
  s" %u" scf+scan
  1 = invert if
    s" invalid format" log-error exit
  then

  dup 0 = if
    drop
    s" invalid player number" log-error exit
  then
  
  dup game game>player-1-id @ = if
    drop
    s" silly boy :)" log-error exit
  then
  
  game game>player-2-id @ 0 = invert if
    drop
    s" cannot join" log-error exit
  then
  
  game game>player-2-id !
  s" OK" ;

: handle-/status ( -- )
    s" /status" log-handle
    
    game? invert if
	s" no game" log-error exit
    then

    game game>player-2-id @ 0 = if
	s" WAITING" exit
    then

    game game-won? if
	game get-current-player-id
	s" WON %u" str-new dup >R spf-set
	R> str-get exit
    then

    game get-current-player-id s" OK %u" str-new dup >R spf-set R> str-get ;


/1991 /                        handle-/
/1991 /new                     handle-/new
/1991 /game/<id>/board         handle-/board
/1991 /game/<id>/play          handle-/play
/1991 /game/<id>/join          handle-/join
/1991 /game/<id>/status        handle-/status

CR \ after the "redefined" stuff

8080 1991:

bye
