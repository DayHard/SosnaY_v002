using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SosnaY_v00
{
    public partial class Form16 : Form
    {
        public Form2 form2;
        public Form16()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            form2.koff = Convert.ToInt32(textBox1.Text);
            this.Dispose();
        }
    }
}
