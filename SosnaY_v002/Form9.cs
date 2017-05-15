using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Xml;

namespace SosnaY_v00
{
    public partial class Form9 : Form
    {
       public class poket
        {
            public int kol = 0;
            public uint kol1 = 0;
            public int[] buffer1 = new int[11];
            public int[] buffer2 = new int[1000];
            public int poket1;

            public void Read_Poket(byte[] In_buffer, byte size)
            {
                byte chek_sum = 0;

                if (In_buffer[0] == 0x55)
                {
                    for (int i = 0; i < (size - 1); i++)
                    {
                        chek_sum ^= In_buffer[i];
                    }

                    if (chek_sum != In_buffer[size - 1])
                    {
                        chek_sum = 0;
                    }
                    else
                    {
                        for (int j = 0; j <= size; j++)
                        {
                            buffer1[j] = In_buffer[j];
                        }
                    }
                }
            } //разбор пакета после чтения

            public void Forming_Poket(byte kommand, byte[] In_buffer, byte size, byte[] Out_buffer) //формирование пакета
            {
                   byte chek_sum = 0;
                   Out_buffer[0] = 170; // стартовый байт
                   Out_buffer[1] = kommand;

                   for (int i = 0; i < size; i++)
                   {
                       Out_buffer[i + 2] = In_buffer[i];
                       chek_sum ^= (byte)(In_buffer[i]);
                   }
                   Out_buffer[size + 2] = (byte)(Out_buffer[0] ^ Out_buffer[1] ^ chek_sum);
            }
        }    

        public class ComPort
        {
            public System.IO.Ports.SerialPort port = new System.IO.Ports.SerialPort();
            public byte[] posilca = new byte[16];  // буфер для посылки
            public byte[] priem = new byte[1008];// буфер на прием
            public xmlWork XMLW = new xmlWork();
            public byte kontr_sum;
            public bool Kol_poket, One_Buffers;
            public int size_Off;
            public int size_On, ReadBytes1;
            public int schet, poket_priem;
            public int h, j, schet_poket, razm_poket, schet_poket1, schet_poket2;
            public poket pokets = new poket();
            public string PortName;
            public int number, ReadBytes, poket_priem1;
            public uint sboy_poket;
           

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
                port.ReceivedBytesThreshold = size_Off;
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
                    port.Write(posilca, 0, size_On);//запись пакета
                }
                catch { }
            }

            private void OnDataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
            {
                try
                {
                    for (int j = 0; j < 3; j++)
                    {

                        priem[j] = 0;
                    }
                    ReadBytes = port.Read(priem, 0, size_Off);
                    pokets.Read_Poket(priem, 3);
                }
                catch { }
            }
        }

        public ComPort ComPorts = new ComPort();
        public xmlWork XMLW = new xmlWork();
        public Form6 form6;
        public Form7 form7;
        public Form5 form5;

        public byte DZcenter, DYcenter, DZcom, DYcom;

        public Form9()
        {
            InitializeComponent();

        }


        private void button1_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 41;
            ComPorts.posilca[2] = (byte)(numericUpDown1.Value);
            ComPorts.posilca[3] = 0;
            ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 5;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Poket_Write();

            DZcenter = 0;
            DZcom = (byte)(numericUpDown1.Value);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 41;
            ComPorts.posilca[2] = (byte)(numericUpDown1.Value);
            ComPorts.posilca[3] = 16;
            ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 5;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Poket_Write();

            DZcenter = 16;
            DZcom = (byte)(numericUpDown1.Value);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 41;
            ComPorts.posilca[2] = (byte)(numericUpDown1.Value);
            ComPorts.posilca[3] = 32;
            ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 5;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Poket_Write();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 41;
            ComPorts.posilca[2] = (byte)(numericUpDown1.Value);
            ComPorts.posilca[3] = 48;
            ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 5;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Poket_Write();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 41;
            ComPorts.posilca[2] = (byte)(numericUpDown1.Value);
            ComPorts.posilca[3] = 64;
            ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 5;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Poket_Write();

            DYcenter = 64;
            DYcom = (byte)(numericUpDown1.Value);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 41;
            ComPorts.posilca[2] = (byte)(numericUpDown1.Value);
            ComPorts.posilca[3] = 80;
            ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 5;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Poket_Write();

            DYcenter = 80;
            DYcom = (byte)(numericUpDown1.Value);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 41;
            ComPorts.posilca[2] = (byte)(numericUpDown1.Value);
            ComPorts.posilca[3] = 96;
            ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 5;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Poket_Write();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 41;
            ComPorts.posilca[3] = 112;
            ComPorts.posilca[2] = (byte)(numericUpDown1.Value);
            ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 5;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Poket_Write();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 41;
            ComPorts.posilca[3] = 128;
            ComPorts.posilca[2] = (byte)(numericUpDown1.Value);
            ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 5;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Poket_Write();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 41;
            ComPorts.posilca[3] = 144;
            ComPorts.posilca[2] = (byte)(numericUpDown1.Value);
            ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 5;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Poket_Write();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 41;
            ComPorts.posilca[3] = 160;
            ComPorts.posilca[2] = (byte)(numericUpDown1.Value);
            ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 5;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Poket_Write();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 41;
            ComPorts.posilca[3] = 176;
            ComPorts.posilca[2] = (byte)(numericUpDown1.Value);
            ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 5;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Poket_Write();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 27;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Poket_Write();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 28;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Poket_Write();
        }

        private void Form9_Load(object sender, EventArgs e)
        {
            ComPorts.size_Off = 3;
            ComPorts.ComInitializ();
            ComPorts.posilca[0] = 170;
             XMLW.XmlCenterRead();
            if (Convert.ToInt16(XMLW.DZcenter) == 0)
            {
                textBox1.Text = Convert.ToString(XMLW.DZcom);
            }
            if (Convert.ToInt16(XMLW.DZcenter) == 16)
            {
                textBox1.Text = "-" + Convert.ToString(XMLW.DZcom);
            }
            if (Convert.ToInt16(XMLW.DYcenter) == 64)
            {
                textBox2.Text = Convert.ToString(XMLW.DYcom);
            }
            if (Convert.ToInt16(XMLW.DYcenter) == 80)
            {
                textBox2.Text = "-" + Convert.ToString(XMLW.DYcom);
            }
            if (Convert.ToInt16(XMLW.DYcenter) == 0)
            {
                textBox2.Text = Convert.ToString(XMLW.DYcom);
            }

        }

        private void Form9_FormClosing(object sender, FormClosingEventArgs e)
        {
            ComPorts.port.Close();
            form5.Show();
            this.Dispose();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 21;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Poket_Write();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 21;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Poket_Write();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            textBox1.Refresh();
            textBox2.Refresh();
            if (Convert.ToInt16(textBox1.Text) > 255 ^ Convert.ToInt16(textBox1.Text) < -255 ^ Convert.ToInt16(textBox2.Text) > 255 ^ Convert.ToInt16(textBox2.Text) < -255)
            {
                MessageBox.Show("Введите целое число шагов от -255 до 255", "ОК", MessageBoxButtons.OK);
            }
            if (Convert.ToInt16(textBox1.Text) > 255 ^ Convert.ToInt16(textBox1.Text) < -255)
            {
                XMLW.XmlCenterRead();
                if (Convert.ToInt16(XMLW.DZcenter) == 0)
                {
                    textBox1.Text = Convert.ToString(XMLW.DZcom);
                }
                if (Convert.ToInt16(XMLW.DZcenter) == 16)
                {
                    textBox1.Text = "-" + Convert.ToString(XMLW.DZcom);
                }
            }
            if (Convert.ToInt16(textBox2.Text) > 255 ^ Convert.ToInt16(textBox2.Text) < -255)
            {
                XMLW.XmlCenterRead();
                if (Convert.ToInt16(XMLW.DYcenter) == 64)
                {
                    textBox2.Text = Convert.ToString(XMLW.DYcom);
                }
                if (Convert.ToInt16(XMLW.DYcenter) == 80)
                {
                    textBox2.Text = "-" + Convert.ToString(XMLW.DYcom);
                }
                if (Convert.ToInt16(XMLW.DYcenter) == 0)
                {
                    textBox2.Text = Convert.ToString(XMLW.DYcom);
                }
            }
            if (Convert.ToInt16(textBox1.Text) > 0)
            {
                    ComPorts.posilca[1] = 41;
                    ComPorts.posilca[2] = (byte)(Math.Abs(Convert.ToInt16(textBox1.Text)));
                    ComPorts.posilca[3] = 0;
                    ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
                    ComPorts.size_Off = 3;
                    ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                    ComPorts.size_On = 5;
                    ComPorts.port.DiscardInBuffer();
                    ComPorts.port.DiscardOutBuffer();
                    ComPorts.Poket_Write();
                    DZcenter = 0;
                    DZcom = (byte)(Math.Abs(Convert.ToInt16(textBox1.Text)));
                    System.Threading.Thread.Sleep(20);
            }
            if (Convert.ToInt16(textBox1.Text) == 0)
            {
                ComPorts.posilca[1] = 41;
                ComPorts.posilca[2] = 0;
                ComPorts.posilca[3] = 0;
                ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 5;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Poket_Write();
                System.Threading.Thread.Sleep(20);
                ComPorts.posilca[1] = 41;
                ComPorts.posilca[2] = 0;
                ComPorts.posilca[3] = 16;
                ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 5;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Poket_Write();
                //DZcenter = 0;
                //DZcom = (byte)(Math.Abs(Convert.ToInt16(textBox1.Text)));
                System.Threading.Thread.Sleep(20);
            }
            if (Convert.ToInt16(textBox2.Text) == 0)
            {
                ComPorts.posilca[1] = 41;
                ComPorts.posilca[2] = 0;
                ComPorts.posilca[3] = 64;
                ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 5;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Poket_Write();
                System.Threading.Thread.Sleep(20);
                ComPorts.posilca[1] = 41;
                ComPorts.posilca[2] = 0;
                ComPorts.posilca[3] = 80;
                ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 5;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Poket_Write();
                //DYcenter = 64;
                //DYcom = (byte)(Math.Abs(Convert.ToInt16(textBox2.Text)));
                System.Threading.Thread.Sleep(20);
            }

            if (Convert.ToInt16(textBox1.Text) < 0)
            {
                    ComPorts.posilca[1] = 41;
                    ComPorts.posilca[2] = (byte)(Math.Abs(Convert.ToInt16(textBox1.Text)));
                    ComPorts.posilca[3] = 16;
                    ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
                    ComPorts.size_Off = 3;
                    ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                    ComPorts.size_On = 5;
                    ComPorts.port.DiscardInBuffer();
                    ComPorts.port.DiscardOutBuffer();
                    ComPorts.Poket_Write();
                    DZcenter = 16;
                    DZcom = (byte)(Math.Abs(Convert.ToInt16(textBox1.Text)));
                    System.Threading.Thread.Sleep(20);
            }
            if (Convert.ToInt16(textBox2.Text) > 0)
            {
                ComPorts.posilca[1] = 41;
                ComPorts.posilca[2] = (byte)(Math.Abs(Convert.ToInt16(textBox2.Text)));
                ComPorts.posilca[3] = 64;
                ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 5;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Poket_Write();
                DYcenter = 64;
                DYcom = (byte)(Math.Abs(Convert.ToInt16(textBox2.Text)));
                System.Threading.Thread.Sleep(20);
            }
            if (Convert.ToInt16(textBox2.Text) < 0)
            {
                    ComPorts.posilca[1] = 41;
                    ComPorts.posilca[2] = (byte)(Math.Abs(Convert.ToInt16(textBox2.Text)));
                    ComPorts.posilca[3] = 80;
                    ComPorts.posilca[4] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1] ^ ComPorts.posilca[2] ^ ComPorts.posilca[3]);
                    ComPorts.size_Off = 3;
                    ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                    ComPorts.size_On = 5;
                    ComPorts.port.DiscardInBuffer();
                    ComPorts.port.DiscardOutBuffer();
                    ComPorts.Poket_Write();
                    DYcenter = 80;
                    DYcom = (byte)(Math.Abs(Convert.ToInt16(textBox2.Text)));
                    System.Threading.Thread.Sleep(20);
            }
            XMLW.SaveCenterXML( Convert.ToString(DZcenter), Convert.ToString(DYcenter), Convert.ToString(DZcom), Convert.ToString(DYcom));
        }
    }
}
