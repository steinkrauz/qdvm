using System;
using System.Collections.Generic;
using System.Text;

namespace qdvm {

    public class QDVMoc : StackFuncs {
        public const byte VM_NOP = 0;
        public const byte VM_LDI = 1;
        public const byte VM_LDB = 2;
        public const byte VM_LSI = 3;
        public const byte VM_LSB = 4;
        public const byte VM_LDV = 5;
        public const byte VM_STV = 6;

        public const byte VM_ADD = 10;
        public const byte VM_SUB = 11;
        public const byte VM_MUL = 12;
        public const byte VM_DIV = 13;
        public const byte VM_MOD = 14;
        public const byte VM_INC = 15;
        public const byte VM_DEC = 16;

        public const byte VM_JEQ = 20;
        public const byte VM_JNE = 21;
        public const byte VM_JLS = 22;
        public const byte VM_JLE = 23;
        public const byte VM_JGR = 24;
        public const byte VM_JGE = 25;
        public const byte VM_JMP = 26;
        public const byte VM_JZ = 27;
        public const byte VM_JNZ = 28;

        public const byte VM_POP = 30;
        public const byte VM_DUP = 31;
        public const byte VM_SWP = 32;
        public const byte VM_RND = 33;
        public const byte VM_CLK = 34;
        public const byte VM_HLT = 35;

        public const byte VM_SCL = 100;
        public const byte VM_CAL = 101;
        public const byte VM_RET = 110;


        static Random rnd = null;

        public static void Add(Stack<int> st) {
            CheckLowStack(st, 2);
            st.Push(st.Pop() + st.Pop());
        }
        public static void Mul(Stack<int> st) {
            CheckLowStack(st, 2);
            st.Push(st.Pop() * st.Pop());
        }

        public static void Sub(Stack<int> st) {
            CheckLowStack(st, 2);
            int d = st.Pop();
            st.Push(st.Pop() - d);
        }

        public static void Div(Stack<int> st) {
            CheckLowStack(st, 2);
            int d = st.Pop();
            st.Push(st.Pop() / d);
        }
        public static void Mod(Stack<int> st) {
            CheckLowStack(st, 2);
            int d = st.Pop();
            st.Push(st.Pop() % d);
        }

        public static void LDi(Stack<int> st, byte[] data, int addr) {
            CheckFullStack(st, 1);
            byte[] ib = new byte[4];
            for (int i = 0; i < 4; i++) ib[i] = data[addr + i];
            st.Push(QDVMhlp.ConvertLittleEndian(ib));
        }



        public static void LDb(Stack<int> st, byte[] data, int addr) {
            CheckFullStack(st, 1);
            st.Push(data[addr]);
        }

        public static void LDv(Stack<int> st, int[] vars, int addr) {
            CheckFullStack(st, 1);
            if (addr > vars.Length) throw new Exception("Range check error");
            st.Push(vars[addr]);
        }
        public static void STv(Stack<int> st, int[] vars, int addr) {
            CheckLowStack(st, 1);
            if (addr > vars.Length) throw new Exception("Range check error");
            vars[addr] = st.Pop();
        }

        public static void GetRandom(Stack<int> st) {
            CheckLowStack(st, 1);
            int maxVal = st.Pop();
            if (rnd == null)
                rnd = new Random((int)DateTime.Now.ToFileTime());
            st.Push(rnd.Next(maxVal));
        }

        public static int GetJump(Stack<int> st, byte[] codes, int ip) {
            bool res = false;
            int addr = QDVMhlp.GetJmpAddr(codes[ip], codes[ip + 1]);
            switch (codes[ip-1]) {
                case QDVMoc.VM_JEQ: res = st.Pop() == st.Pop();break;
                case QDVMoc.VM_JGE: res = st.Pop() >= st.Pop();break;
                case QDVMoc.VM_JGR: res = st.Pop() > st.Pop();break;
                case QDVMoc.VM_JLE: res = st.Pop() <= st.Pop();break;
                case QDVMoc.VM_JLS: res = st.Pop() < st.Pop();break;
                case QDVMoc.VM_JMP: res = true;break;
                case QDVMoc.VM_JNE: res = st.Pop() != st.Pop();break;
                case QDVMoc.VM_JZ: res = st.Pop() == 0; break;
                case QDVMoc.VM_JNZ: res = st.Pop() != 0; break;
            }
            if (!res) addr = ip+2;
            if (addr > codes.Length || addr < 0) throw new Exception(String.Format("Jump {0} outside the code", addr));
            return addr;
        }


    }
}