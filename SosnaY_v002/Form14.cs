using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZedGraph;
using System.IO;
using System.IO.Ports;
using System.Xml;
using System.Reflection;

namespace SosnaY_v00
{
    public partial class Form14 : Form
    {
        public class poket
        {
            //public bool Kol_poket;
            public int kol = 0;
            public uint kol1 = 0;

            //  public int[][] buffer = new int[8000][]; // Общий буфер
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
        }   // Класс для разбора и сбора пакета данных

        public class ComPort
        {
            public xmlWork XMLW = new xmlWork();
            public System.IO.Ports.SerialPort port = new System.IO.Ports.SerialPort();
            public byte[] posilca = new byte[16];  // буфер для посылки
            public byte[] priem = new byte[4500];// буфер на прием
            public byte[] priem_nedoch = new byte[54000];//прием недочитанного
            public byte[] priem1 = new byte[11];
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
            public short[][] buffer = new short[50000][];
            public int[][] buffer1 = new int[8000][];

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

                    if (posilca[1] == 26)
                    {

                    }
                    port.Write(posilca, 0, size_On);//запись пакета
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
                    ReadBytes = port.Read(priem, 0, size_Off);
                    schet_poket1++;

                    if (pokets.poket1 == 1)
                    {
                        for (int i = 0; i < ReadBytes; i += 9)
                        {
                            if (priem[i] == 85)
                            {
                                if (priem[i + 1] == 104)
                                {
                                    kontr_sum = (byte)(priem[i + 0] ^ priem[i + 1] ^ priem[i + 2] ^ priem[i + 3] ^ priem[i + 4] ^ priem[i + 5] ^ priem[i + 6] ^ priem[i + 7]);
                                    if (kontr_sum == priem[i + 8])
                                    {
                                        buffer[schet_poket] = new short[9];
                                        buffer[schet_poket][0] = priem[i];
                                        buffer[schet_poket][1] = priem[i + 1];
                                        buffer[schet_poket][2] = priem[i + 2];
                                        buffer[schet_poket][3] = priem[i + 3];
                                        buffer[schet_poket][4] = priem[i + 4];
                                        buffer[schet_poket][5] = priem[i + 5];
                                        buffer[schet_poket][6] = priem[i + 6];
                                        buffer[schet_poket][7] = priem[i + 7];
                                        buffer[schet_poket][8] = priem[i + 8];
                                        schet_poket++;
                                        One_Buffers = false;
                                        if (schet_poket > 15)
                                        {
                                            Kol_poket = true;
                                        }
                                    }
                                    else
                                    {
                                        if ((priem[i] == 0) & (priem[i + 1] == 0) & (priem[i + 2] == 0) & (priem[i + 3] == 0) & (priem[i + 4] == 0) & (priem[i + 5] == 0) & (priem[i + 6] == 0) & (priem[i + 7] == 0) & (priem[i + 8] == 0))
                                        {
                                            //break;
                                        }
                                        else
                                            sboy_poket++;
                                    }
                                }
                                else
                                {
                                    if ((priem[i] == 0) & (priem[i + 1] == 0) & (priem[i + 2] == 0) & (priem[i + 3] == 0) & (priem[i + 4] == 0) & (priem[i + 5] == 0) & (priem[i + 6] == 0) & (priem[i + 7] == 0) & (priem[i + 8] == 0))
                                    {
                                        //break;
                                    }
                                    else
                                        sboy_poket++;
                                }
                            }
                            else
                            {
                                if ((priem[i] == 0) & (priem[i + 1] == 0) & (priem[i + 2] == 0) & (priem[i + 3] == 0) & (priem[i + 4] == 0) & (priem[i + 5] == 0) & (priem[i + 6] == 0) & (priem[i + 7] == 0) & (priem[i + 8] == 0))
                                {
                                    //break;
                                }
                                else
                                    sboy_poket++;
                            }
                        }
                    }
                    else
                    {
                        if (pokets.poket1 == 0)
                        {
                            if (priem[1] == 100)
                            {
                                One_Buffers = true;
                                pokets.Read_Poket(priem, 6);
                            }
                            else
                            {
                                if (priem[1] == 101)
                                {
                                    One_Buffers = true;
                                    pokets.Read_Poket(priem, 3);
                                }
                                else
                                {
                                    if (priem[1] == 102)
                                    {
                                        One_Buffers = true;
                                        pokets.Read_Poket(priem, 6);
                                    }
                                    else
                                    {
                                        if (priem[1] == 103)
                                        {
                                            One_Buffers = true;
                                            pokets.Read_Poket(priem, 11);
                                        }
                                        else
                                        {
                                            if (priem[1] == 104)
                                            {
                                                One_Buffers = true;
                                                pokets.Read_Poket(priem, 9);
                                            }
                                            else
                                            {
                                                One_Buffers = false;
                                                number++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        }


        #region 
        public Form1 form1;
        public Form6 form6;
        public Form5 form5 = new Form5();
        public xmlWork XMLW = new xmlWork();
        public Form3 form3;
        public Form1 form1_ = new Form1(); 
        public bool tolerance = false;
       // public int MouseClick = 0;
  
        public SosnaY_v00.Form14.ComPort ComPorts = new ComPort();
        public int[] ReadBuffers1 = new int[10];
        byte Pankr, rejim, rejim1;
        static System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();
        public double Kor_Z, Kor_Y, Kor_P;
        public short Kor_Z1, Kor_Y1;
        public byte first_click, time, rejim2, Pricel;
        public short time_muve_pankr, time_begin_sniatia_prevish, time_sniatia_prevish, time_vozvrata;
       // public byte[] In_buffer = new byte[13];
       // public byte[] Out_buffer = new byte[16];
        public int[][] ReadBuffers = new int[1000][];
        Form4 form4 = new Form4();
        public bool thReadPoket,Begin, Begin1, Begin2, adc_one;
        public long number, number_dorisovka, l, timer_25;
        public byte[] GetStatus_Byte1 = new byte[8];
        public byte[] GetStatus_Byte2 = new byte[8];
        public byte zadrj, two_click,Clicks;
        #endregion

        public Form14()
        {
            InitializeComponent();
        }

        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            if (adc_one == true)
            {
                ComPorts.posilca[1] = 24;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 9;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "ADC_One";
                ComPorts.pokets.poket1 = 0;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();
            }

            #region Величина превышения1
            if (Begin1 == true)
            {
                switch (rejim1)
                {
                    case 0:
                        if (zadrj == 0)
                        {
                            ComPorts.posilca[1] = 27;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "BPO_START";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            zadrj = 1;
                        }
                        break;
                    case 1:
                        if (zadrj == 1)
                        {
                            ComPorts.posilca[1] = 14;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "Off_DP_Off";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            zadrj = 2;
                        }
                        break;
                    case 2:
                        if (zadrj == 2)
                        {
                            ComPorts.posilca[1] = 12;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "Return"; 
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            System.Threading.Thread.Sleep(2000);
                            zadrj = 3;
                        }
                        break;
                    case 3:
                        if (zadrj == 3)
                        {
                            ComPorts.posilca[1] = 15;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "Shtorka_On";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            zadrj = 4;
                        }
                        break;
                    case 4:
                        if (zadrj == 4)
                        {
                            ComPorts.posilca[1] = 11;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "Laser_On";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            if (XMLW.stend == true)
                            {
                                rejim1++;
                            }
                            else
                            {
                                ComPorts.Poket_Write();
                            }
                            zadrj = 5;
                        }
                        break;
                    case 5:
                        if (zadrj == 5)
                        {
                            adc_one = true;
                            zadrj = 6;
                        }
                        break;
                }
            }
            #endregion

            #region Величина превышения2
            if (Begin2 == true)
            {
                switch (rejim1)
                {
                    case 10:
                        if (zadrj == 0)
                        {
                            ComPorts.posilca[1] = 15;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "Shtorka_On";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            zadrj = 1;
                        }
                        break;
                    case 11:
                        if (zadrj == 1)
                        {
                            ComPorts.posilca[1] = 11;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "Laser_On";
                            
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            if (XMLW.stend == true)
                            {
                                rejim1++;
                            }
                            else
                            { 
                                ComPorts.Poket_Write();
                            }
                            zadrj = 2;
                        }
                        break;
                    case 12:
                        if (zadrj == 2)
                        {
                            ComPorts.posilca[1] = 24;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 9;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "ADC_One";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            zadrj = 3;
                        }
                        break;
                    case 13:
                        if (zadrj == 3)
                        {
                            ComPorts.posilca[1] = 12;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "Return";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();

                            ComPorts.posilca[1] = 16;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "Shtorka_Off";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();

                            zadrj = 4;
                        }
                        break;
                }
            }
            #endregion




            #region Вывод на экран данных
         
            if (ComPorts.One_Buffers == true)
            {
                ReadBuffers1 = ComPorts.pokets.buffer1;
                
                #region ADC_OUT однократно
                    if (ReadBuffers1[1] == 104)
                    {

                        Kor_Z1 = (short)((((ReadBuffers1[3] << 8) & 0x7F00) | ReadBuffers1[2]));
                        Kor_Y1 = (short)((((ReadBuffers1[5] << 8) & 0x7F00) | ReadBuffers1[4]));
                        Kor_P = (double)((double)((double)((double)(((ReadBuffers1[7] << 8) & 0x7F00) | ReadBuffers1[6]) / 1024)));

                        if ((ReadBuffers1[3] == 0x80) || (ReadBuffers1[3] == 0x81) || (ReadBuffers1[3] == 0x82) || (ReadBuffers1[3] == 0x83))
                        {

                            Kor_Z = (double)((double)(((Kor_Z1) / (double)(1024)) - XMLW.Cmehenie_Z) / (XMLW.MZ / XMLW.Const));
                            Kor_Z = Kor_Z * (-1);
                        }
                        else
                        {
                            Kor_Z = (double)((double)(((Kor_Z1) / (double)(1024)) + XMLW.Cmehenie_Z) / (XMLW.PZ / XMLW.Const));
                        }

                        if ((ReadBuffers1[5] == 0x80) || (ReadBuffers1[5] == 0x81) || (ReadBuffers1[5] == 0x82) || (ReadBuffers1[5] == 0x83))
                        {
                            Kor_Y = (double)((double)(((Kor_Y1) / (double)(1024)) - XMLW.Cmehenie_Y) / (XMLW.MY / XMLW.Const));
                            Kor_Y = Kor_Y * (-1);
                        }
                        else
                        {
                            Kor_Y = (double)((double)(((Kor_Y1) / (double)(1024)) + XMLW.Cmehenie_Y) / (XMLW.PY / XMLW.Const));
                        }
                        if (Begin1 == true)
                        {
                            textBox1.Text = Convert.ToString(Math.Round(Kor_Y,2));
                            textBox1.Refresh();
                        }
                        if (Begin2 == true)
                        {
                            textBox2.Text = Convert.ToString(Math.Round(Kor_Y,2));
                            textBox2.Refresh();

                            textBox3.Text = Convert.ToString( Math.Round ( ( ( Convert.ToDouble(textBox1.Text) - Convert.ToDouble(textBox2.Text) ) * 3.3 ) , 2) );
                            textBox3.Refresh();
                            rejim1++;
                        }
                    }

                    #endregion
                

                #region Ready
                    if (ReadBuffers1[1] == 101)
                    {
                        toolStripStatusLabel1.Text = "ГОТОВО";
                        if (toolStripStatusLabel2.Text == "BPO_START")
                        {
                            rejim1++;
                        }
                        if (toolStripStatusLabel2.Text == "Shtorka_On")
                        {
                            rejim1++;
                        }
                        if (toolStripStatusLabel2.Text == "Laser_On")
                        {
                            rejim1++;
                        }
                        if (toolStripStatusLabel2.Text == "Return")
                        {
                            rejim1++;
                        }
                        if(toolStripStatusLabel2.Text == "Off_DP_Off")
                        {
                            rejim1++;
                        }
                    }
                    #endregion
                
                ComPorts.One_Buffers = false;
            }
            #endregion
        }

        private void Form14_Load(object sender, EventArgs e)
        {
            Posilc.Enabled = false;
            toolStripStatusLabel1.Visible = false;
            toolStripStatusLabel2.Visible = false;
            ComPorts.posilca[0] = 170;
            ComPorts.size_Off = 3;
            rejim = 0;
            rejim1 = 0;
            ComPorts.ComInitializ();
            first_click = 0;
            two_click = 0;
            myTimer.Tick += new EventHandler(TimerEventProcessor);
            myTimer.Interval = 500;
            myTimer.Start();
            XMLW.XmlConfigRead();

            ComPorts.posilca[1] = 27;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            toolStripStatusLabel1.Text = " ";
            toolStripStatusLabel2.Text = "BPO_START";
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
        }



        private void Form14_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((добавитьВОтчетToolStripMenuItem.BackColor == Color.LightGreen) && (сохранитьРисунокToolStripMenuItem.BackColor == Color.LightGreen))
            {
                form6.button7.BackColor = Color.LightGreen;
                добавитьВОтчетToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                сохранитьРисунокToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                сохранитьРисунокToolStripMenuItem.Text = "Сохранить результат";
                добавитьВОтчетToolStripMenuItem.Text = "Добавить в отчет";
                ComPorts.port.Close();
                myTimer.Stop();
                form6.Show();
                this.Dispose();
            }
            else
            {

                if ((сохранитьРисунокToolStripMenuItem.BackColor != Color.LightGreen) && (добавитьВОтчетToolStripMenuItem.BackColor != Color.LightGreen))
                {
                    if (MessageBox.Show("Хотите выйти не сохраняя результат и не добавляя данные в отчет?", "Выход", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        сохранитьРисунокToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                        сохранитьРисунокToolStripMenuItem.Text = "Cохранить результат";
                        добавитьВОтчетToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control); ;
                        добавитьВОтчетToolStripMenuItem.Text = "Добавить в отчет";
                        ComPorts.port.Close();
                        myTimer.Stop();
                        form6.Show();
                        this.Dispose();
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    if (сохранитьРисунокToolStripMenuItem.BackColor != Color.LightGreen)
                    {
                        if (MessageBox.Show("Хотите выйти не сохраняя результат?", "Выход", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            сохранитьРисунокToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                            сохранитьРисунокToolStripMenuItem.Text = "Cохранить результат";
                            добавитьВОтчетToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control); ;
                            добавитьВОтчетToolStripMenuItem.Text = "Добавить в отчет";
                            ComPorts.port.Close();
                            myTimer.Stop();
                            form6.Show();
                            this.Dispose();
                        }
                        else
                        {
                            e.Cancel = true;
                        }

                    }
                    else
                    {
                        if (добавитьВОтчетToolStripMenuItem.BackColor != Color.LightGreen)
                        {
                            if (MessageBox.Show("Хотите выйти не добавляя данные в отчет?", "Выход", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                сохранитьРисунокToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                                сохранитьРисунокToolStripMenuItem.Text = "Cохранить результат";
                                добавитьВОтчетToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control); ;
                                добавитьВОтчетToolStripMenuItem.Text = "Добавить в отчет";
                                ComPorts.port.Close();
                                myTimer.Stop();
                                form6.Show();
                                this.Dispose();
                            }
                            else
                            {
                                e.Cancel = true;
                            }
                        }
                    }
                }
            
            }
        }

        private void snap_CheckedChanged(object sender, EventArgs e)
        {
             first_click++;
             if (first_click == 1)
             {
                 snap.Text = "СТОП";
                 Begin1 = true;
                 Begin2 = false;
                 rejim1 = 0;
                 zadrj = 0;
                 Posilc.Enabled = false;
             }
             else
             {
                 if (first_click == 2)
                 {
                     
                     snap.Text = "Y в режиме ''Без превышения''";
                     Begin1 = false;
                     adc_one = false;
                     ComPorts.posilca[1] = 12;
                     ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                     ComPorts.size_Off = 3;
                     ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                     ComPorts.size_On = 3;
                     toolStripStatusLabel1.Text = " ";
                     toolStripStatusLabel2.Text = "Return";
                     ComPorts.pokets.poket1 = 0;
                     ComPorts.port.DiscardInBuffer();
                     ComPorts.port.DiscardOutBuffer();
                     ComPorts.Kol_poket = false;
                     ComPorts.Poket_Write();

                     ComPorts.posilca[1] = 16;
                     ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                     ComPorts.size_Off = 3;
                     ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                     ComPorts.size_On = 3;
                     toolStripStatusLabel1.Text = " ";
                     toolStripStatusLabel2.Text = "Shtorka_Off";
                     ComPorts.pokets.poket1 = 0;
                     ComPorts.port.DiscardInBuffer();
                     ComPorts.port.DiscardOutBuffer();
                     ComPorts.Kol_poket = false;
                     ComPorts.Poket_Write();

                     Posilc.Enabled = true;
                     first_click = 0;
                     rejim1 = 10;
                 }
             }
        }

        private void Posilc_Click(object sender, EventArgs e)
        {
            добавитьВОтчетToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
            сохранитьРисунокToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
            сохранитьРисунокToolStripMenuItem.Text = "Сохранить результат";
            добавитьВОтчетToolStripMenuItem.Text = "Добавить в отчет";
            Begin1 = false;
            Begin2 = true;
            rejim1 = 10;
            zadrj = 0;
            //if (Clicks == 1)
            //{ 
                
            //}
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XMLW.SavePicture(printDocument1, this);
        }

        private void printDocument1_PrintPage_1(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {

            RectangleF destRect = new RectangleF(5.0F, 5.0F, 1080.0F, 768.0F);
            RectangleF srcRect = new RectangleF(0.0F, 0.0F, 1152.0F, 864.0F);
            GraphicsUnit units = GraphicsUnit.Pixel;
            e.Graphics.DrawImage(XMLW.myImage, destRect, srcRect, units);
        }

        private void сохранитьРисунокToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XMLW.SaveImage(this);
            сохранитьРисунокToolStripMenuItem.BackColor = Color.LightGreen;
            сохранитьРисунокToolStripMenuItem.Text = "Результат сохранен";
        }

        private void добавитьВОтчетToolStripMenuItem_Click(object sender, EventArgs e)
        {
            добавитьВОтчетToolStripMenuItem.BackColor =  Color.LightGreen;
            добавитьВОтчетToolStripMenuItem.Text = "Добавлено в отчет";
            form6.SetValue("E89", textBox3.Text);

        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        } 
      }
}
