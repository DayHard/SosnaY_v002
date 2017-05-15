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
    public partial class Form10 : Form
    {
        public Form1 form1;
        public Form6 form6;
        Form2 form2;
        public string NumberProduct, TypeCheck;
        

        public Form10()
        {
            InitializeComponent();
        }

        public void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "1";
            label1.Refresh();
            form2 = new Form2();
            form2.form10 = this;
            form2.form6 = form6;
            form2.form1 = form1;
            this.Hide();
            form2.groupBox2.Show();
            form2.groupBox1.Show();
            form2.Text = "Прибор - " + NumberProduct + " Рассогл. каналов. " + button1.Text.Replace("\n", " "); // + " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")"; //button1.Text;
            form2.Tag = NumberProduct + "  Рассогл. [Контроль]    ";
            form2.textBox10.Show();
            form2.textBox9.Show();
            form2.textBox8.Show();
            form2.trackBar2.Visible = false;
            form2.dopyskZona = true;
            form2.groupBox6.Hide();
            form2.ShowDialog();
            
           
        }

        public void button2_Click(object sender, EventArgs e)
        {
            label1.Text = "2";
            label1.Refresh();
            this.Hide();
            form2 = new Form2();
            form2.form10 = this;
            form2.form6 = form6;
            TypeCheck = form2.Text = "Прибор - " + NumberProduct + " Рассогл. каналов. " + button2.Text.Replace("\n", " ");// + " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")"; //button2.Text;
            form2.Tag = NumberProduct;
            form2.textBox10.Show();
            form2.textBox9.Show();
            form2.textBox8.Show(); 
            form2.dopyskZona = true;
            form2.trackBar2.Visible = false;
            form2.groupBox6.Hide();
            form2.ShowDialog();
            
        }

        public void button3_Click(object sender, EventArgs e)
        {
            label1.Text = "3";
            label1.Refresh();
            this.Hide();
            form2 = new Form2();
            form2.form10 = this;
            form2.form6 = form6;
            form2.form1 = form1;
            TypeCheck = form2.Text = "Прибор - " + NumberProduct + " Рассогл. каналов. " + button3.Text.Replace("\n", " "); // +" " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")"; //button3.Text;
            form2.Tag = NumberProduct + "  Рассогл. [Механ.]    ";
            form2.textBox10.Show();
            form2.textBox9.Show();
            form2.textBox8.Show();
            form2.groupBox2.Show();
            form2.groupBox1.Show();
            form2.trackBar2.Visible = false;
            form2.dopyskZona = true;
            form2.groupBox6.Visible = false;
            form2.ShowDialog();
            
        }

        public void button4_Click(object sender, EventArgs e)
        {
            label1.Text = "4";
            label1.Refresh();
            this.Hide();
            form2 = new Form2();
            form2.form10 = this;
            form2.form6 = form6;
            form2.form1 = form1;
            TypeCheck = form2.Text = "Прибор " + NumberProduct + " Рассогл. каналов. " + button4.Text.Replace("\n", " "); //+ " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")"; //button4.Text;
            form2.Tag = NumberProduct + "  Рассогл. [+]      ";
            form2.groupBox2.Hide();
            form2.groupBox1.Hide();
            form2.textBox10.Show();
            form2.textBox9.Show();
            form2.textBox8.Show();
            form2.trackBar2.Visible = false;
            form2.dopyskZona = false;
            form2.ShowDialog();

            form2.groupBox6.Show();
        }

        public void button5_Click(object sender, EventArgs e)
        {
            label1.Text = "4";
            label1.Refresh();
            this.Hide();
            form2 = new Form2();
            form2.form10 = this;
            form2.form6 = form6;
            form2.form1 = form1;
            form2.Text = "Прибор " + NumberProduct + " " + button5.Text.Replace("\n", " "); //+ " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")"; //button5.Text;
            form2.Tag = NumberProduct + "  Рассогл. [-]      ";
            //form2.groupBox4.Hide();
            form2.groupBox2.Hide();
            form2.groupBox1.Hide();
           // form2.groupBox6.Show();
            form2.textBox10.Show();
            form2.textBox9.Show();
            form2.textBox8.Show();
          //  form2.groupBox6.Show();
           // form2.label1.Show();
           // form2.label2.Show();
           // form2.label3.Show();
            form2.trackBar2.Visible = false;
            form2.dopyskZona = false;
            form2.ShowDialog();
            form2.groupBox6.Show();
        }

        private void Form10_FormClosing(object sender, FormClosingEventArgs e)
        {
            form6.Show();
            this.Dispose();
        }

        private void Form10_Load(object sender, EventArgs e)
        {
            button2.Visible = false;
        }
        
    }
}
