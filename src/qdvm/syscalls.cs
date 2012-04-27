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

    public class QDVMmsg {
        public int ThrID;
        public int MsgID;
        public byte[] Data;
        public QDVMmsg() {
            Data = new byte[QDVMdef.MSG_DATA_SIZE];
        }
    }

    public class QDVMsc : StackFuncs {

        public static int CreateThread(QDVMthread thr) {
            CheckLowStack(thr.st, 1);
            for (int i = 0; i < QDVMdef.MAX_THRD_COUNT; i++) {
                if (StkQDVM.Threads[i] == null) {
                    StkQDVM.Threads[i] = new QDVMthread();
                    String CodeFile = GetStrArg(thr);
                    StkQDVM.Threads[i].Load(CodeFile);
                    return i;
                }
            }
            return -1;
        }
        public static int NameThread(QDVMthread thr) {
            CheckLowStack(thr.st, 2);
            int i = thr.st.Pop();
            if (StkQDVM.Threads[i] == null)
                throw new Exception("Dead thread id was given into a syscall");
            StkQDVM.Threads[i].id = GetStrArg(thr);
            return 0;
        }

        public static int FindThread(QDVMthread thr) {
            CheckLowStack(thr.st, 1);
            String Name = GetStrArg(thr);
            for (int i = 0; i < QDVMdef.MAX_THRD_COUNT; i++) {
                if (StkQDVM.Threads[i] == null) continue;
                if (StkQDVM.Threads[i].id.Equals(Name))
                    return i;
            }
            return -1;
        }

        public static int KickThread(QDVMthread thr) {
            CheckLowStack(thr.st, 1);
            int i = thr.st.Pop();
            StkQDVM.Threads[i].Kick();
            return 0;
        }

        public static int DebugPrint(QDVMthread thr) {
            CheckLowStack(thr.st, 1);
            Console.Write("[{0}]", StkQDVM.ThrId);
            Console.WriteLine(GetStrArg(thr));
            return 0;
        }

        public static int SendMessage(QDVMthread thr) {
            CheckLowStack(thr.st, 3);
            int addr = thr.st.Pop();
            int MsgID = thr.st.Pop();
            int ThrID = thr.st.Pop();
            QDVMmsg msg = new QDVMmsg();
            msg.ThrID = ThrID;
            msg.MsgID = MsgID;
            for (int i = 0; i < QDVMdef.MSG_DATA_SIZE; i++, addr++)
                msg.Data[i] = thr.code[addr];
            StkQDVM.Messages.Add(msg);
            return 0;
        }

        public static int GetMessage(QDVMthread thr) {
            CheckLowStack(thr.st, 2);
            int AddrData = thr.st.Pop();
            int AddrMsgID = thr.st.Pop();
            QDVMmsg msg = StkQDVM.Messages.Find(delegate(QDVMmsg mess)
            {
                return mess.ThrID == StkQDVM.ThrId;
            });
            if (msg == null) return 0;
            thr.code[AddrData] = (byte)msg.MsgID;
            for (int i = 0; i < QDVMdef.MSG_DATA_SIZE; i++, AddrMsgID++)
                thr.code[AddrMsgID] = msg.Data[i];
            return 1;
        }

        public static int KillThread(QDVMthread thr) {
            CheckLowStack(thr.st, 1);
            int ThrID = thr.st.Pop();
            if (StkQDVM.Threads[ThrID] != null)
                StkQDVM.Threads[ThrID] = null;
            return 0;
        }


        public static int Sleep(QDVMthread thr) {
            CheckLowStack(thr.st, 1);
            int SleepTime = thr.st.Pop();
            System.Threading.Thread.Sleep(SleepTime);
            return 0;
        }

        public static int DoEvents(QDVMthread thr) {
            System.Windows.Forms.Application.DoEvents();
            if (System.Windows.Forms.Application.OpenForms.Count < 1)
                return 1;
            return 0;
        }

    }
}
