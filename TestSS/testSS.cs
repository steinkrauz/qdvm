using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;

namespace TestSS {
    static class testSS {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ShowScreenSaver();
            Application.Run();
            /*for (int i = 0; i < 100; i++) {
                Application.DoEvents();
                Thread.Sleep(100);
            }*/
        }

        public static ScrForm LastForm;

        static void ShowScreenSaver() {
            foreach (Screen screen in Screen.AllScreens) {
                ScrForm screensaver = new ScrForm(screen.Bounds);
                screensaver.Show();
                LastForm = screensaver;
            }
        }
    }
}
