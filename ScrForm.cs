//using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System;

namespace qdvm {
    class ScrForm : Form {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();
            // 
            // ScrForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.MidnightBlue;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ScrForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Form1";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ScrForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ScrForm_Paint);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ScrForm_KeyPress);
            this.ResumeLayout(false);

        }

        #endregion


        public ScrForm() {
            InitializeComponent();
        }
        private Bitmap BackBuffer;
        private Graphics BackGraphics;
        private char[] Symbols = { '·', '•', '●', '¤', '☼', ' ' };

        public ScrForm(Rectangle Bounds) {
            InitializeComponent();
            this.Bounds = Bounds;
            BackBuffer = new Bitmap(this.Width, this.Height);
            BackGraphics = Graphics.FromImage(BackBuffer);
        }

        private Font myFont2 = new System.Drawing.Font("Courier New", 40, FontStyle.Regular);
        private Font myFont = new System.Drawing.Font("Courier New", 40, FontStyle.Regular);
        private Brush myBrush = new SolidBrush(System.Drawing.Color.Cyan);
        private Brush backBrush = new SolidBrush(System.Drawing.Color.MidnightBlue);


        public void DrawPix3(int iX, int iY, int Stage)
        {
            BackGraphics.DrawString("█", myFont2, backBrush, iX, iY);
            BackGraphics.DrawString(Symbols[Stage].ToString(), myFont, myBrush, iX, iY);

        }

        public void ErasePix(int iX, int iY) {
            BackGraphics.DrawString("█", myFont2, backBrush, iX, iY);                    
        }


        private void ScrForm_Load(object sender, System.EventArgs e) {
            Cursor.Hide();
        }

        private void ScrForm_KeyPress(object sender, KeyPressEventArgs e) {
            Application.Exit();
        }

        private void ScrForm_Paint(object sender, PaintEventArgs e) {
            e.Graphics.DrawImageUnscaled(BackBuffer, 0, 0);
        }

       }
}
