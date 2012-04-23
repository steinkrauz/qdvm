using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
            UInt16 res = (UInt16)(lb + hb*256);
            return Convert.ToInt32(res);
        }

    }

    public class  QDVMmsg {
        public int ThrID;
        public int MsgID;
        public byte[] Data;
        public QDVMmsg() {
            Data = new byte[QDVMdef.MSG_DATA_SIZE];
        }
    }

    public class StackFuncs{
          protected static void CheckLowStack(Stack<int> st,int ArgNum) {
            if (st.Count < ArgNum) throw new Exception("stack underflow");
        }
          protected static void CheckFullStack(Stack<int> st, int Num) {
              try {
                  for (int i=0;i<Num;i++)
                    st.Push(0);
              }catch{
                  System.Console.WriteLine("Stack is full!");
              }
              for (int i = 0; i < Num; i++)
                st.Pop();
          }
        protected static string GetStrArg(QDVMthread thr){
            int addr = thr.st.Pop();
            String str = "";
            while (thr.code[addr]!=0) str+=Convert.ToChar(thr.code[addr++]);
            return str;
        }
    }

  

    public enum QDThrState {NEW, RUN, WAIT, FROZEN, DEAD};

    public class QDVMsc:StackFuncs {

        public static int CreateThread(QDVMthread thr) {
            CheckLowStack(thr.st,1);
            for (int i = 0; i < QDVMdef.MAX_THRD_COUNT; i++) {
                if (StkQDVM.Threads[i]==null){
                    StkQDVM.Threads[i] = new QDVMthread();
                    String CodeFile = GetStrArg(thr);
                    StkQDVM.Threads[i].Load(CodeFile);
                    return i;
                }
            }
                return -1;
        }
        public static int NameThread(QDVMthread thr){
            CheckLowStack(thr.st,2);
            int i = thr.st.Pop();
            if (StkQDVM.Threads[i]==null)
                throw new Exception("Dead thread id was given into a syscall");
            StkQDVM.Threads[i].id = GetStrArg(thr);
            return 0;
        }

        public static int FindThread(QDVMthread thr){
            CheckLowStack(thr.st,1);
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
            Console.Write("[{0}]",StkQDVM.ThrId);
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
            for (int i=0;i<QDVMdef.MSG_DATA_SIZE;i++,addr++)
                msg.Data[i] = thr.code[addr];
            StkQDVM.Messages.Add(msg);
            return 0;
        }
        
        public static int GetMessage(QDVMthread thr) {
            CheckLowStack(thr.st, 2);
            int AddrData = thr.st.Pop();
            int AddrMsgID = thr.st.Pop();
            QDVMmsg msg = StkQDVM.Messages.Find(delegate(QDVMmsg mess){
                return mess.ThrID==StkQDVM.ThrId;});
            if (msg==null) return 0;
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
            if (System.Windows.Forms.Application.OpenForms.Count<1)
                return 1;
            return 0;
        }

    }

    public class QDVMthread {
        public QDThrState state;
        public String id;
        public int ip;
        public Stack<int> st;
        public Stack<int> rs;
        public int[] vars;
        public byte[] code;
        public QDVMthread() {
            id = "";
            st = new Stack<int>();
            rs = new Stack<int>();
            vars = new int[QDVMdef.LOC_VAR_COUNT];
            code = null;
            state = QDThrState.NEW;
            ip = 0;
        }
        public void Kick() { state = QDThrState.WAIT; }
        public void Load(string Name) {
            string DiskName = String.Format("{0}{1}{2}", StkQDVM.BaseDir, Path.DirectorySeparatorChar, Name);
            try {
                BinaryReader br = new BinaryReader(File.Open(DiskName, FileMode.Open));
                byte type = br.ReadByte();
                switch (type) {
                    case 0: LoadAbsCode(br); break;
                    default: throw new Exception("Unsupported file format");
                }
                br.Close();
            } catch (Exception ex) {
                Console.Out.WriteLine(ex.Message);
            }
        }

        private void LoadAbsCode(BinaryReader br) {
            int size = br.ReadInt32();
            code = br.ReadBytes(size);
        }

    }


    public delegate T Func<T>(QDVMthread thr);
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
                            Console.ReadLine();
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
