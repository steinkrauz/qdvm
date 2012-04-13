using System;
using System.Collections.Generic;
using System.Text;

namespace qdvm {
    class Program {
        static void Main(string[] args) {
            QDVMthread t1, t2;
            t1 = new QDVMthread();
            t2 = new QDVMthread();
            //t1.code = new byte[] { 5, 0, 4, 1, 10, 6, 0, 5, 1, 4, 2, 10, 6, 1, 5, 0, 5, 1, 4, 1, 4, 1, 20, 28,0,4,3,10, 100, 0, 110 };
            t1.Load("main.qdm");
            t1.Kick();
            t2.Load("main.qdm");
            t2.Kick();

            foreach (string par in args) {
                if (par.Equals("-dps"))
                    StkQDVM.PrintStack = true;
            }

            StkQDVM VM = new StkQDVM();
            StkQDVM.Threads[0] = t1;
            StkQDVM.Threads[1] = t2;
            try {
                VM.Run();
            } catch (Exception ex) {
                Console.WriteLine("VM failed: {0}", ex.Message);
            }
        }
    }
}
