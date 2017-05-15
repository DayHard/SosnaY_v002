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
    public partial class Form3 : Form
    {
        public string[] fi;
        public Form7 form7;
       // public Form2 form2;
        public xmlWork XMLW;
        public int numberRow;

        public double [,] WriteBuffer = new double[8000,3]; // массив для данных из файла

        public Form3()
        {
            InitializeComponent();
            LoadXmlFolder();
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
        }

        private void LoadXmlFolder() //Получение списка файлов папки XML и вывод их в DataGrid
        {
            fi = Directory.GetFiles(Application.StartupPath + "\\xml", "*.xml");
            for (int i = 0; i < fi.Length; i++)
            {
                XmlTextReader rd = new XmlTextReader(fi[i]);
                rd.Read();
                if (rd.IsStartElement("Sosnay_data_file"))
                {
                    rd.ReadStartElement("Sosnay_data_file");
                    while (rd.IsStartElement("Sosnay_description"))
                    {
                        rd.ReadStartElement("Sosnay_description");
                        dataGridView1.Rows.Add(rd.ReadElementString("Number"), 
                                               rd.ReadElementString("Type_Check"), 
                                               rd.ReadElementString("Date"), 
                                               rd.ReadElementString("Time"),
                                               fi[i]);
                        rd.ReadEndElement();
                    }
                }
                rd.Close();
            }
   

            
        }

        private void button1_Click(object sender, EventArgs e) //Загрузка данных из XML
        {
            XMLW.LoadDB(Convert.ToString(dataGridView1.Rows[numberRow].Cells[4].Value));
            //form7.Poisk();
            this.Close();
          /*  int i = 0;
            XmlTextReader rd = new XmlTextReader(Convert.ToString(dataGridView1.Rows[numberRow].Cells[4].Value));
            rd.Read();
            if (rd.IsStartElement("Sosnay_data_file"))
            {
                rd.ReadStartElement("Sosnay_data_file");
                rd.ReadToFollowing("Sosnay_data");
                rd.ReadStartElement("Sosnay_data");
                while (rd.IsStartElement("Y"))
                {
                    WriteBuffer [i,0] = Convert.ToDouble(rd.ReadElementString("Y"));
                    WriteBuffer [i,1] = Convert.ToDouble(rd.ReadElementString("Z"));
                    WriteBuffer [i,2] = Convert.ToDouble(rd.ReadElementString("P"));

                    form2.list1.Add(i, WriteBuffer[i, 1]);
                    form2.list2.Add(i, WriteBuffer[i, 0]);
                    form2.list3.Add(i, WriteBuffer[i, 2]);
                    
                    form2.form3 = this;
                    i++;
                }
                rd.ReadEndElement();
            }
            rd.Close();
           
            form2.trackBar1.Enabled = true;
            form2.trackBar2.Enabled = true;

            form2.numericUpDown1.Enabled = true;
            form2.numericUpDown2.Enabled = true;
            form2.numericUpDown3.Enabled = true;
            form2.numericUpDown4.Enabled = true;
            form2.numericUpDown5.Enabled = true;
           */
            // form2.CreateGraph(form2.Zedgraph);
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e) //Получение по нажатию идекса строки
        {
            DataGridView.HitTestInfo hits = dataGridView1.HitTest(e.X, e.Y);
            numberRow = hits.RowIndex;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
