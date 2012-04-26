;Standard subroutine to create a thread

#defnum NameAddr 100
; Args: Name->File->
; ret: void
.MakeThread:
  stv NameAddr
  scl CreateThread
  dup
  ldv NameAddr
  swp
  scl NameThread
  pop
  scl KickThread
  pop
ret