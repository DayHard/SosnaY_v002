using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
namespace SosnaY_v00
{
    public partial class Form15 : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);


        public Form15()
        {
            InitializeComponent();
        }

        private void Form15_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawLine(new Pen(Color.GreenYellow, 3), 290, 150, 290, 425);

            e.Graphics.DrawLine(new Pen(Color.GreenYellow, 3), 150, 290, 425, 290);

            e.Graphics.FillEllipse(Brushes.GreenYellow, 280, 280, 20, 20);

            e.Graphics.DrawEllipse(new Pen(Color.GreenYellow, 3), 220, 220, 142, 142);

            //this.BackColor = Color.Red;
            this.BackColor = Color.Black;
            // Make the background color of form display transparently.
            this.TransparencyKey = BackColor;
        }

        private void Form15_MouseClick(object sender, MouseEventArgs e)
        {
           
        }

        private void Form15_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)// при нажатии левой кнопки мыши 
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
    }
}
