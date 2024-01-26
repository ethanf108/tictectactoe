include ffl/tis.fs
include ffl/scf.fs

\ counts set bits in 8-bit unsigned number
: bit-count ( u -- u )
    0 swap
    8 0 do
	dup 1 and rot + swap 1 rshift
    loop
    drop ;

\ lmoa
: strcpy ( c-addr u -- c-addr u )
  here over allot
  over 2>R drop 2R@
  cmove
  2R> ;

\ takes "a=b" string and if needle is "a" returns "b"
: key>value ( c-addr u c-addr u -- false | c-addr u true )
  ( needle string -- ... )
  2dup s" " compare 0 = if
    2drop 2drop 0 exit
  then
  tis-new dup >R tis-set
  '=' R@ tis-scan-char invert if
    2drop 0
  else
      compare 0 = invert if
	  0
      else
	  R@ tis-read-all
	  strcpy -1
      then
  then
  R> tis-free ;

  
