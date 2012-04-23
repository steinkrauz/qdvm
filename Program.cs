using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace qdvm {
    class Program {
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
            if (runGUI) {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                ScrForm frm = new ScrForm(Screen.PrimaryScreen.Bounds);
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
