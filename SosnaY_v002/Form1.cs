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
using System.IO.Ports;
using System.Reflection;
using System.Drawing.Drawing2D;
using System.Windows;


namespace SosnaY_v00
{
    public partial class Form1 : Form
    {
        public class ComPort
        {
            public System.IO.Ports.SerialPort port = new System.IO.Ports.SerialPort();
            public byte[] posilca = new byte[16];  // буфер для посылки
            public byte[] priem = new byte[675];// буфер на прием
            public int ReadBytes;
            public xmlWork XMLW = new xmlWork();

            public void ComInitializ()
            {
                XMLW.XmlConfigRead();
                port.PortName = XMLW.COM;
                //Время ожидания записи и чтения с порта
                port.WriteTimeout = 50;
                port.ReadTimeout = 50;
                //Настраиваем скорость обмена данными с телефоном - телефон не может обрабатывать данный на максимальной скорости
                port.BaudRate = 115200;
                //Другие необходимые настройки - подходит для большинства телефонов - но возможно придется настраивать:
                //port.BaudRate = 115200;
                port.Parity = Parity.None;
                port.DataBits = 8;
                port.StopBits = StopBits.One;
                port.Handshake = Handshake.None;
                port.DataReceived += new SerialDataReceivedEventHandler(OnDataReceived);
                port.ReceivedBytesThreshold = 3;
                port.ReadBufferSize = 55000;
                try
                {
                    port.Open();
                    port.DtrEnable = true;

                }
                catch { MessageBox.Show("Ошибка открытия порта", "", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            }

            public void Poket_Write()
            {
                try
                {
                    port.Write(posilca, 0, 5);//запись пакета
                }
                catch { }
            }
            private void OnDataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
            {
                try
                {
                    for (int j = 0; j < 675; j++)
                    {
                        priem[j] = 0;
                    }
                    ReadBytes = port.Read(priem, 0, 3);

                }
                catch { }
            }
        }

        public string[] fi;
        public Form5 form5 = new Form5();
        public Form6 form6 = new Form6();
        public xmlWork XMLW = new xmlWork();
        public ComPort ComPorts = new ComPort();
        public Form5 form5_;
        short click;
        public bool stend;

        public Form1()
        {
            InitializeComponent();
            ComboboxList();
            click = 0;
        }

        private void button_number_product_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "")
            {
                MessageBox.Show("Введите номер прибора", "Ошибка", MessageBoxButtons.OK);
            }
            else
            {

                XMLW.XmlCenterRead();
                ComPorts.ComInitializ();
                ComPorts.posilca[1] = 41;
                ComPorts.posilca[2] = XMLW.DZcom;
                ComPorts.posilca[3] = XMLW.DZcenter;
                ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
                ComPorts.Poket_Write();
                System.Threading.Thread.Sleep(8);
                ComPorts.posilca[1] = 41;
                ComPorts.posilca[2] = XMLW.DYcom;
                ComPorts.posilca[3] = XMLW.DYcenter;
                ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
                ComPorts.Poket_Write();
                System.Threading.Thread.Sleep(8);
                ComPorts.port.Close();
                this.Hide();
                form6.form1 = this;
                form6.NumberProduct = comboBox1.Text;
                form6.ShowDialog();
            }
            
        }

        private void ComboboxList() 
        {
         
            int start,end;
                fi = Directory.GetFiles(Application.StartupPath + "\\xml", "*.xml");
                for (int i = 0; i < fi.Length; i++) //цикл для корректного вывода имени файла
                {
                    start = fi[i].LastIndexOf("\\");
                    fi[i] = fi[i].Remove(0, start + 1);
                    end = fi[i].LastIndexOf(".");
                    fi[i] = fi[i].Remove(end);
                    comboBox1.Items.Add(fi[i]);
                }
            
           
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            comboBox1.TabIndex = 1;
            form5.XmlConfigRead();
            form5.Update();
            XMLW.XmlConfigRead();
            //ComPorts.ComInitializ();
            ComPorts.posilca[0] = 170;
            FindComPorts();
            
        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        
        {
            if (e.KeyCode == Keys.Enter)
            {
                button_number_product.PerformClick();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            click++;
            if (click == 1)
            {
                this.Height = 164;
            }
            else
            {
                this.Height = 138;
                click = 0;
            }
        }

        private void FindComPorts()
        {
            string[] availPorts = SerialPort.GetPortNames();
            for (int j = 0; j < availPorts.Length; j++)
            {
                comboBox2.Items.Add(availPorts[j]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            XMLW.XmlConfigRead();
            XMLW.SaveXML(Convert.ToString(XMLW.T), Convert.ToString(XMLW.Fstart), Convert.ToString(XMLW.Fend), Convert.ToString(XMLW.D), Convert.ToString(XMLW.Tay), Convert.ToString(XMLW.V), Convert.ToString(XMLW.PZ), Convert.ToString(XMLW.MZ), Convert.ToString(XMLW.PY), Convert.ToString(XMLW.MY), Convert.ToString(XMLW.P), comboBox2.Text, Convert.ToString(XMLW.stend), Convert.ToString(XMLW.Cmehenie_Z), Convert.ToString(XMLW.Cmehenie_Y));
        }
    }
}
