/* Author: Steinkrauz <steinkrauz@yahoo.com>
 *
 * This program is free software. It comes without any warranty, to
 * the extent permitted by applicable law. You can redistribute it
 * and/or modify it under the terms of the Do What The Fuck You Want
 * To Public License, Version 2, as published by Sam Hocevar. See
 * http://sam.zoy.org/wtfpl/COPYING for more details. */  

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace qdvm {

    public class StkQDVM {
        public static bool PrintStack = false;
        public static int[] Sysvars;
        public static Func<int>[] Syscalls = new Func<int>[QDVMdef.SYSCALLS_COUNT];
        public static QDVMthread[] Threads = new QDVMthread[QDVMdef.MAX_THRD_COUNT];
        public static List<QDVMmsg> Messages = new List<QDVMmsg>();
        QDVMthread CurThr;
        public static int ThrId;
        public static string BaseDir;

        public StkQDVM() {
            Syscalls[0] = QDVMsc.CreateThread;
            Syscalls[1] = QDVMsc.NameThread;
            Syscalls[2] = QDVMsc.FindThread;
            Syscalls[3] = QDVMsc.KickThread;
            Syscalls[4] = QDVMsc.SendMessage;
            Syscalls[5] = QDVMsc.GetMessage;
            Syscalls[6] = QDVMsc.KillThread;
            Syscalls[7] = QDVMsc.DebugPrint;
            Syscalls[8] = QDVMsc.Sleep;
            Syscalls[9] = QDVMsc.DoEvents;
            
        }

        public void Init(string dir) {
            BaseDir = dir;
            QDVMthread tr0 = new QDVMthread();
            tr0.Load("main.qdm");
            Threads[0] = tr0;
            tr0.Kick();
        }


        void Executor(QDVMthread thr) {
            byte[] opcodes;
            opcodes = thr.code;

            try {

                switch (opcodes[thr.ip++]) {
                    case QDVMoc.VM_NOP: break;
                    case QDVMoc.VM_LDB: QDVMoc.LDb(thr.st, opcodes, opcodes[thr.ip++]); break;
                    case QDVMoc.VM_LDI: QDVMoc.LDi(thr.st, opcodes, opcodes[thr.ip++]); break;
                    case QDVMoc.VM_LSB: thr.st.Push(opcodes[thr.ip++]); break;
                    case QDVMoc.VM_LSI: QDVMoc.LDi(thr.st, opcodes, thr.ip); thr.ip += 4; break;
                    case QDVMoc.VM_LDV: QDVMoc.LDv(thr.st, thr.vars, opcodes[thr.ip++]); break;
                    case QDVMoc.VM_STV: QDVMoc.STv(thr.st, thr.vars, opcodes[thr.ip++]); break;

                    case QDVMoc.VM_ADD: QDVMoc.Add(thr.st); break;
                    case QDVMoc.VM_SUB: QDVMoc.Sub(thr.st); break;
                    case QDVMoc.VM_MUL: QDVMoc.Mul(thr.st); break;
                    case QDVMoc.VM_DIV: QDVMoc.Div(thr.st); break;
                    case QDVMoc.VM_MOD: QDVMoc.Mod(thr.st); break;
                    case QDVMoc.VM_INC: thr.st.Push(1); QDVMoc.Add(thr.st); break;
                    case QDVMoc.VM_DEC: thr.st.Push(1); QDVMoc.Sub(thr.st); break;

                    case QDVMoc.VM_JEQ:
                    case QDVMoc.VM_JNE:
                    case QDVMoc.VM_JGE:
                    case QDVMoc.VM_JGR:
                    case QDVMoc.VM_JLE:
                    case QDVMoc.VM_JLS:
                    case QDVMoc.VM_JMP:
                    case QDVMoc.VM_JZ:
                    case QDVMoc.VM_JNZ:
                        thr.ip = QDVMoc.GetJump(thr.st, opcodes, thr.ip);
                    break;

                    case QDVMoc.VM_RND: QDVMoc.GetRandom(thr.st);break;
                    case QDVMoc.VM_CLK: thr.st.Push((int)(DateTime.Now.ToFileTime() / 10000L)); break;
                    case QDVMoc.VM_HLT: thr.state = QDThrState.FROZEN; break;
                    case QDVMoc.VM_POP: thr.st.Pop();break;
                    case QDVMoc.VM_DUP: thr.st.Push(thr.st.Peek()); break;
                    case QDVMoc.VM_SWP: int a,b; 
                        a = thr.st.Pop(); b = thr.st.Pop();
                        thr.st.Push(a);thr.st.Push(b);break;

                    case QDVMoc.VM_SCL: thr.st.Push(Syscalls[opcodes[thr.ip++]](thr)); break;
                    case QDVMoc.VM_CAL: int addr = QDVMhlp.GetJmpAddr(opcodes[thr.ip], opcodes[thr.ip + 1]);
                        thr.rs.Push(thr.ip + 2);
                        thr.ip = addr;
                        break;
                    case QDVMoc.VM_RET: if (thr.rs.Count > 0) thr.ip = thr.rs.Pop();
                        else {
                            if (thr.state == QDThrState.NEW) thr.state = QDThrState.FROZEN;
                            else
                                thr.state = QDThrState.DEAD;
                        } break;
                    default: throw new Exception(String.Format("Opcode {0} not implemented", opcodes[thr.ip - 1]));
                }
            } catch (Exception ex) {
                //Console.Out.WriteLine(ex.StackTrace);
                throw new Exception(String.Format("{0} at [{1}]:{2}", ex.Message, ThrId, thr.ip - 1));
            }
            if (thr.state == QDThrState.DEAD) {
                Threads[ThrId] = null;
            }
        }

        public void Run() {
            do {
                for (ThrId = 0; ThrId < QDVMdef.MAX_THRD_COUNT; ThrId++) {
                    CurThr = Threads[ThrId];
                    if (CurThr == null) continue;
                    if (CurThr.state == QDThrState.NEW)
                        continue;
                    CurThr.state = QDThrState.RUN;
                    for (int c = 0; c < QDVMdef.FRAME_TICKS; c++) {
                        Executor(CurThr);
                        if (PrintStack) {
                            Console.Write("[{0}]:",ThrId);
                            foreach (int num in CurThr.st) Console.Write("{0}->", num);
                            Console.WriteLine();
                            //Console.ReadLine();
                        }
                        if (CurThr.state != QDThrState.RUN) break;
                    }
                    if (CurThr.state == QDThrState.FROZEN) CurThr.state = QDThrState.WAIT;
                }
                if (Threads[0] == null) break;
            }
            while (true);
            
        }

    }
}
