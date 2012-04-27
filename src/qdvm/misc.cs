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


namespace qdvm {
    public class QDVMdef {
        public const int SYS_VAR_COUNT = 256;
        public const int SYSCALLS_COUNT = 64;
        public const int LOC_VAR_COUNT = 128;
        public const int MAX_THRD_COUNT = 128;
        public const int FRAME_TICKS = 50;
        public const int MSG_DATA_SIZE = 16;
    }

    public class QDVMhlp {
        public static int ConvertLittleEndian(byte[] array) {
            int pos = 0;
            int result = 0;
            foreach (byte by in array) {
                result |= (int)(by << pos);
                pos += 8;
            }
            return result;
        }
        public static int GetJmpAddr(byte lb, byte hb) {
            UInt16 res = (UInt16)(lb + hb * 256);
            return Convert.ToInt32(res);
        }

    }

    public class StackFuncs {
        protected static void CheckLowStack(Stack<int> st, int ArgNum) {
            if (st.Count < ArgNum) throw new Exception("stack underflow");
        }
        protected static void CheckFullStack(Stack<int> st, int Num) {
            try {
                for (int i = 0; i < Num; i++)
                    st.Push(0);
            } catch {
                System.Console.WriteLine("Stack is full!");
            }
            for (int i = 0; i < Num; i++)
                st.Pop();
        }
        protected static string GetStrArg(QDVMthread thr) {
            int addr = thr.st.Pop();
            String str = "";
            while (thr.code[addr] != 0) str += Convert.ToChar(thr.code[addr++]);
            return str;
        }
    }
}
