;definition for system calls

; creates a new thread with code file
; args: addr of file name resource
; ret: ThreadID
#defnum CreateThread 0

; Gives thread a name (usially a meaningful one)
; args: thread id, addr of name resource
; ret: 0
#defnum NameThread 1

; finds a thread by name
; args: addr of name resource
; ret: ThreadID
#defnum FindThread 2

; switches thread state to WAIT
; args: thread id
; ret: 0
#defnum KickThread 3

; sends a message to a thread specified
; args: thread id, message id, address of 16-byte buffer
; ret: 0
#defnum SendMessage 4

; get a message for current thread from queue
; args: addr for MsgID, addr for 16-byte Buffer
; ret: 0 -- if no message, 1 -- otherwise
#defnum GetMessage 5

; kills given thread
; args: thread id
; ret: 0
#defnum KillThread 6

; prints a string
; args: addr of a string resource
; ret: 0
#defnum DebugPrint 7

; sleeps for given time
; args: time in milliseconds
; ret: 0
#defnum Sleep 8

; processes form event in gui mode
; args: void
; ret: 1 -- ready to quit, 0 -- do continue
#defnum DoEvents 9
