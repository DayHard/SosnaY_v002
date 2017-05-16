using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace SosnaY_v00
{
    public partial class Form5 : Form
    {
        public Form5()
        {
            InitializeComponent();

        }
      //  public Form1 form1_ = new Form1();
        xmlWork XMLW = new xmlWork();

        public string T,
                     Fstart,    //начальный фокус
                     Fend,      //конечный фокус
                     D,
                     Tay,
                     V;
        public Form6 form6;
        public Form1 form1;
        public Form9 form9;
        public Form13 form13;
        public short click;
        public bool stend;

        //Порог срабатывания (0.05)
        public double ThresholdTrigger;
        //Размер растра {количество средних значений}(100)
        public int RastSize;
        //Время начала (в посылках)
        public int StartTime;


        public void XmlConfigRead() //чтение данных для настроек
        {
            XmlTextReader rd = new XmlTextReader(Convert.ToString("config.xml"));
            rd.Read();
            if (rd.IsStartElement("Sosnay_config"))
            {
                rd.ReadStartElement("Sosnay_config");
                while (rd.IsStartElement("Sosnay_description"))
                {
                    rd.ReadStartElement("Sosnay_description");
                    T = textBox1.Text = rd.ReadElementString("T");
                    Fstart = textBox2.Text = rd.ReadElementString("Fstart");
                    Fend = textBox6.Text = rd.ReadElementString("Fend");
                    D = textBox3.Text = rd.ReadElementString("D");
                    Tay = textBox4.Text = rd.ReadElementString("Tay");
                    V = textBox5.Text = rd.ReadElementString("V");
                    rd.ReadElementString("PZ");
                    rd.ReadElementString("MZ");
                    rd.ReadElementString("PY");
                    rd.ReadElementString("MY");
                    rd.ReadElementString("P");
                    rd.ReadElementString("COM");
                    rd.ReadElementString("Stend");
                    rd.ReadElementString("Cmehenie_z");
                    rd.ReadElementString("Cmehenie_y");
                    rd.ReadEndElement();
                }
            }
            rd.Close();
        }

        public void SaveXML()
        {
            
            XmlTextWriter writer = new XmlTextWriter("config.xml", null);
            writer.WriteStartElement("Sosnay_config");
            writer.Formatting = Formatting.Indented;

            writer.WriteStartElement("Sosnay_description");
            writer.WriteElementString("T", textBox1.Text);
            writer.WriteElementString("Fstart", textBox2.Text);
            writer.WriteElementString("Fend", textBox6.Text);
            writer.WriteElementString("D", textBox3.Text);
            writer.WriteElementString("Tay", textBox4.Text);
            writer.WriteElementString("V", textBox5.Text);
            writer.WriteEndElement();

            writer.WriteFullEndElement();
            writer.Close();

        }

        private void Form5_Load(object sender, EventArgs e)
        {
            XMLW.XmlConfigRead();

            textBox1.Text = Convert.ToString(XMLW.T);
            textBox2.Text = Convert.ToString(XMLW.Fstart);
            textBox6.Text = Convert.ToString(XMLW.Fend);
            textBox3.Text = Convert.ToString(XMLW.D);
            textBox4.Text = Convert.ToString(XMLW.Tay);
            textBox5.Text = Convert.ToString(XMLW.V);
            //XmlConfigRead();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            XMLW.SaveXML(textBox1.Text, textBox2.Text, textBox6.Text, textBox3.Text, textBox4.Text, textBox5.Text, Convert.ToString(XMLW.PZ), Convert.ToString(XMLW.MZ), Convert.ToString(XMLW.PY), Convert.ToString(XMLW.MY), Convert.ToString(XMLW.P), Convert.ToString(XMLW.COM), Convert.ToString(XMLW.stend), Convert.ToString(XMLW.Cmehenie_Z), Convert.ToString(XMLW.Cmehenie_Y));
        }

        private void Form5_FormClosing(object sender, FormClosingEventArgs e)
        {
            form6.Show();
            ThresholdTrigger = Double.Parse(tbThresholdTrigger.Text);
            RastSize = Int32.Parse(tbRastSize.Text);
            StartTime = Int32.Parse(tbStartTime.Text);

            if (click == 1)
            {
              
            }
            else
            {

            }
            this.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Hide();
            form9 = new Form9();
            form9.form5 = this;
            form9.Text = "Центрировка";

            form9.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            form13 = new Form13();
            form13.form5 = this;
            form13.Text = "Тарировка";

            form13.ShowDialog();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            click++;
            if (click == 1)
            {
              
                XMLW.stend = true;
                XMLW.SaveXML(Convert.ToString(XMLW.T), Convert.ToString(XMLW.Fstart), Convert.ToString(XMLW.Fend), Convert.ToString(XMLW.D), Convert.ToString(XMLW.Tay), Convert.ToString(XMLW.V), Convert.ToString(XMLW.PZ), Convert.ToString(XMLW.MZ), Convert.ToString(XMLW.PY), Convert.ToString(XMLW.MY), Convert.ToString(XMLW.P), Convert.ToString(XMLW.COM), Convert.ToString(XMLW.stend), Convert.ToString(XMLW.Cmehenie_Z), Convert.ToString(XMLW.Cmehenie_Y));
            }
            else
            {
                XMLW.stend = false;
                XMLW.SaveXML(Convert.ToString(XMLW.T), Convert.ToString(XMLW.Fstart), Convert.ToString(XMLW.Fend), Convert.ToString(XMLW.D), Convert.ToString(XMLW.Tay), Convert.ToString(XMLW.V), Convert.ToString(XMLW.PZ), Convert.ToString(XMLW.MZ), Convert.ToString(XMLW.PY), Convert.ToString(XMLW.MY), Convert.ToString(XMLW.P), Convert.ToString(XMLW.COM), Convert.ToString(XMLW.stend), Convert.ToString(XMLW.Cmehenie_Z), Convert.ToString(XMLW.Cmehenie_Y));
                click = 0;
            }
        }
        
    }
}
