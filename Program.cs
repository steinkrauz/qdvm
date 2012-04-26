using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace qdvm {
    class Program {
        static ScrForm frm;
        static int DrawStar(QDVMthread thr){
            int y = thr.st.Pop();
            int x = thr.st.Pop();
            int stage = thr.st.Pop();
            frm.DrawPix3(x,y,stage);
            frm.Refresh();
            return 0;
        }
        static int GetBounds(QDVMthread thr){
            thr.st.Push(Screen.PrimaryScreen.Bounds.Width);
            thr.st.Push(Screen.PrimaryScreen.Bounds.Height);
            return 0;
        }
        static int EraseStar(QDVMthread thr)
        {
            int y = thr.st.Pop();
            int x = thr.st.Pop();
            frm.ErasePix(x, y);
            return 0;
        }
        static void Main(string[] args) {
            byte mode = 0;
            bool needLog = false;
            bool runGUI = false;
            string baseDir = ".";
            string logFile = "console.txt";
            foreach (string par in args) {
              
                switch (par){
                   case "-dps":
                    if (par.Equals("-dps"))
                        StkQDVM.PrintStack = true;
                    break;
                    case "-base":
                        mode = 1;
                    continue;
                    case "-log":
                        needLog = true;
                        mode = 2;
                    continue;
                    case "-gui":
                        runGUI = true;
                    continue;
                };
                switch (mode) {
                    case 1:
                        baseDir = par;
                        break;
                    case 2: logFile = par;
                        break;
                }
                mode = 0;
            }

            StkQDVM VM = new StkQDVM();
            VM.Init(baseDir);
            StkQDVM.Syscalls[31] = DrawStar;
            StkQDVM.Syscalls[32] = GetBounds;
            StkQDVM.Syscalls[33] = EraseStar;
            if (runGUI) {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                frm = new ScrForm(Screen.PrimaryScreen.Bounds);
                frm.Show();
            }
            try {
                VM.Run();
            } catch (Exception ex) {
                Console.WriteLine("VM failed: {0}", ex.Message);
                //Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
