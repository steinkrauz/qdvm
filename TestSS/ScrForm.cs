using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TestSS {
    public partial class ScrForm : Form {
        public ScrForm() {
            InitializeComponent();
        }
        private Bitmap BackBuffer;
        private Graphics BackGraphics;
        private char[] Symbols = {'·','•','●','¤','☼',' '};
        private byte CurSymbol = 0;

        public ScrForm(Rectangle Bounds) {
            InitializeComponent();
            this.Bounds = Bounds;
            BackBuffer = new Bitmap(this.Width, this.Height);
            BackGraphics = Graphics.FromImage(BackBuffer);
        }


        public void DrawPix() {
            Font myFont = new System.Drawing.Font("Courier New", 40, FontStyle.Italic);

            Brush myBrush = new SolidBrush(System.Drawing.Color.Red);
            Brush backBrush = new SolidBrush(System.Drawing.Color.MidnightBlue);

            BackGraphics.DrawString("█", myFont, backBrush, 30, 30);
            BackGraphics.DrawString(Symbols[CurSymbol].ToString(), myFont, myBrush, 30, 30);
            if (++CurSymbol>5) CurSymbol = 0;
            

        }

        private void ScrForm_Load(object sender, EventArgs e) {
            Cursor.Hide();
        }

        private void ScrForm_KeyPress(object sender, KeyPressEventArgs e) {
            Application.Exit();
        }

        private void ScrForm_Paint(object sender, PaintEventArgs e) {
            e.Graphics.DrawImageUnscaled(BackBuffer, 0, 0);
        }

        private void timer1_Tick(object sender, EventArgs e) {
            testSS.LastForm.DrawPix();
            testSS.LastForm.Refresh();
        }


    }
}
