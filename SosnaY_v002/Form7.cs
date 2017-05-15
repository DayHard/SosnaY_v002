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

namespace SosnaY_v00
{
    public partial class Form7 : Form
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
                                            //sboy_poket++;
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
                                    sboy_poket++;
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

        #region Инициализация переменных


        //public Form7 form7;
        public Form3 form3;
        public Form6 form6;
        public xmlWork XMLW = new xmlWork();
        public Form9 form9 = new Form9();
        public Form5 form5 = new Form5();

        public SosnaY_v00.Form7.ComPort ComPorts = new ComPort();

        public bool tolerance = false, podschet = false;
        public int MouseClick = 0, podschet_1;
        PointD pointMin, pointCenter, pointMax;

        public int[] ReadBuffers1 = new int[10];
        public byte Pole_250, Pole_250_P, Pole_5880, rejim, rejim1, first_click, Scan_Z_click, Scan_Y_click, Pole_250_1, Pole_250_P_1, Pole_5880_1;
        static System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();
        public PointPairList list1 = new PointPairList();
        public PointPairList list2 = new PointPairList();
        public PointPairList list3 = new PointPairList();
        public double Kor_Z, Kor_Y, Kor_P, rastor;
        public short time_muve_pankr, time_begin_sniatia_prevish, time_sniatia_prevish, time_vozvrata;
        public byte[] In_buffer = new byte[13];
        public byte[] Out_buffer = new byte[16];
        public int[][] ReadBuffers = new int[1000][];
        Form4 form4 = new Form4();
        public bool thReadPoket, Begin, Begin1, Begin2;
        public long number, number_dorisovka, l, timer_25;
        public byte[] GetStatus_Byte1 = new byte[8];
        public byte[] GetStatus_Byte2 = new byte[8];
        public short Kor_Z1, Kor_Y1, dop_zon;
        public byte Pricel, rejim2, centr;
        public double[] StickX = { 0, 0 }; //начальные координады салазки 
        public double[] StickY = { -1.3, 1.3 };//начальные координады салазки

        public double[] StickX1 = { 0, 0 };//начальные координады салазки
        public double[] StickY1 = { -1.3, 1.3 };//начальные координады салазки

        public byte pyss; // для таймера запуска
        int a, b, c;
        double obsKof;
        public double focus = 0;
        public bool endpropise, zaxod; //если не туда нажали, то двигателя остановятся
        public short z_y;
        public bool proverka;
        public string zagolovok, tegg;

        

        //-------------------------------------------------------------------
        //  Инициализация параметров поле управления
        //-------------------------------------------------------------------

        //--------------- Радиус линейной и полной зоны ---------------------    

        public double rL, //радиус линейной зоны
                      rF, //радиус полной зоны
                      n,  //число меток координат
                      t,  //шаг винта КЮ
                      L,  //дальность панкратической системы (250м и 5880м)
                      f,  //фокусное расстояние коллиматора К1 и к2
                      rLplus, //радиус линейной зоны (+)
                      rLminus,//радиус линейной зоны (-)
                      rFplus, //радиус полной зоны (+)
                      rFminus; //радиус полной зоны (-)

        //----------- Величина освещенности Е в центре поля -----------------

        public double k, //коэффециент пересчета
                      u, //амплитуда сигнала энергии в точке
                      v,  //вольтватная характеристика БВК(чуствительность) 
                      d,  //диаметр диафрагмы сетки коллиматора
                      tay; //коэффициент светопропускания коллиматора  

        //----- Величина освещенности Е по краям и запределами поля ---------

        public double L0, //амплитуда сигнала энергии в точке, соответствующей нулевой команде
                      Ll, //амплитуда сигнала энергии на краях линейной зоны
                      E0;  //величина освещенности в центре поля управ.



        #endregion
        

        public Form7()
        {
            InitializeComponent();
        }

        private void Form7_Resize(object sender, EventArgs e)
        {
            SetSize();
        }

        private void SetSize()
        {
            try
            {
                splitContainer1.Size = new Size(ClientRectangle.Width, ClientRectangle.Height);
                splitContainer1.SplitterDistance = ClientRectangle.Width - 220;
                zedGraphControl1.Size = new Size(splitContainer1.Panel1.Width - 10, splitContainer1.Panel1.Height - 110);

                trackBar1.Top = zedGraphControl1.Bottom;
                trackBar2.Top = trackBar1.Bottom;

                string res = Convert.ToString(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width);

                if (res == "1152")
                {
                    trackBar1.Size = new Size(zedGraphControl1.Width - 65, trackBar1.Height);
                    trackBar2.Size = new Size(zedGraphControl1.Width - 65, trackBar2.Height);

                    trackBar1.Left = zedGraphControl1.Left + 65;
                    trackBar2.Left = zedGraphControl1.Left + 65;
                }
                if (res == "1024")
                {
                    trackBar1.Size = new Size(zedGraphControl1.Width - 55, trackBar1.Height);
                    trackBar2.Size = new Size(zedGraphControl1.Width - 55, trackBar2.Height);

                    trackBar1.Left = zedGraphControl1.Left + 55;
                    trackBar2.Left = zedGraphControl1.Left + 55;
                }
                if (res == "1280")
                {
                    trackBar1.Size = new Size(zedGraphControl1.Width - 85, trackBar1.Height);
                    trackBar2.Size = new Size(zedGraphControl1.Width - 85, trackBar2.Height);

                    trackBar1.Left = zedGraphControl1.Left + 80;
                    trackBar2.Left = zedGraphControl1.Left + 80;
                }

                SetScanZero.Top = splitContainer1.Panel2.Bottom - 155;
                groupBox1.Top = splitContainer1.Panel2.Bottom - 95;
                
                groupBox5.Top = splitContainer1.Panel2.Bottom - 795;
                groupBox6.Top = splitContainer1.Panel2.Bottom - 575;
                groupBox7.Top = splitContainer1.Panel2.Bottom - 685;
                
                ScanZ.Top = splitContainer1.Panel2.Bottom - 935;
                ScanY.Top = splitContainer1.Panel2.Bottom - 890;
                cansel.Top = splitContainer1.Panel2.Bottom - 845;
               
                label1.Top = splitContainer1.Panel2.Bottom - 450;
                textBox3.Top = splitContainer1.Panel2.Bottom - 450;
                label17.Top = splitContainer1.Panel2.Bottom - 455;
                label18.Top = splitContainer1.Panel2.Bottom - 447;
                label19.Top = splitContainer1.Panel2.Bottom - 461;
                label20.Top = splitContainer1.Panel2.Bottom - 450;

                label3.Top = splitContainer1.Panel2.Bottom - 399;
                textBox1.Top = splitContainer1.Panel2.Bottom - 400;
                label21.Top = splitContainer1.Panel2.Bottom - 396;
                textBox2.Top = splitContainer1.Panel2.Bottom - 360;
             }
            catch 
            { 
            
            }
        }
  
        private void Form7_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((добавитьToolStripMenuItem.BackColor == Color.LightGreen) && (добавитьВОтчетToolStripMenuItem.BackColor == Color.LightGreen))
            {
                if (form6.pokaz.Text == "1")
                {
                    form6.button1.BackColor = Color.LightGreen;
                }
                if (form6.pokaz.Text == "2")
                {
                    form6.button2.BackColor = Color.LightGreen;
                }
                if (form6.pokaz.Text == "3")
                {
                    form6.button3.BackColor = Color.LightGreen;
                }
                ComPorts.posilca[1] = 14;
                toolStripStatusLabel2.Text = "Off_DP_Off";
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                ComPorts.pokets.poket1 = 0;
                toolStripStatusLabel1.Text = " ";
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();
                toolStripStatusLabel3.Text = "Выключение блокировки панкратики";
                System.Threading.Thread.Sleep(2000);
                добавитьToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                добавитьToolStripMenuItem.Text = "Сохранить результат";
                добавитьВОтчетToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                добавитьВОтчетToolStripMenuItem.Text = "Добавить в отчет";
                ComPorts.port.Close();
                myTimer.Stop();
                form6.Show();
                this.Dispose();
            }
            else
            {
                if ((добавитьToolStripMenuItem.BackColor != Color.LightGreen) && (добавитьВОтчетToolStripMenuItem.BackColor != Color.LightGreen))
                {
                    if (MessageBox.Show("Хотите выйти не сохраняя результат и не добавляя данные в отчет?", "Выход", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        добавитьToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                        добавитьToolStripMenuItem.Text = "Cохранить результат";
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
                    if (добавитьToolStripMenuItem.BackColor != Color.LightGreen)
                    {
                        if (MessageBox.Show("Хотите выйти не сохраняя результат?", "Выход", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            добавитьToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                            добавитьToolStripMenuItem.Text = "Cохранить результат";
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
                                добавитьToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                                добавитьToolStripMenuItem.Text = "Cохранить результат";
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
            //toolStripStatusLabel1.Text = " ";
            //toolStripStatusLabel2.Text = " ";
            //list1.Clear();
            //list2.Clear();
            //list3.Clear();
            //rejim1 = 0;
            //Pole_250_P_1 = 0;
            //Pole_250_1 = 0;
            //Pole_5880_1 = 0;
            ////ComPorts.buffer.
            //for (int i = 0; i < number; i++)
            //{
            //    for (int j = 0; j < 9; j++)
            //    {
            //        ComPorts.buffer[i][j] = 0;
            //    }

            //}
            //number = 0;
            //ComPorts.schet_poket1 = 0;
            //ComPorts.schet_poket = 0;
            //ZedGraphControl Zedgraph = new ZedGraphControl();
            //Zedgraph.Size = zedGraphControl1.Size;
            //zedGraphControl1.GraphPane = Zedgraph.GraphPane;
            //CreateGraph(Zedgraph);
            //zedGraphControl1.Refresh();
            //Zedgraph.Dispose();
            //form6.Show();
            
            //очисткаToolStripMenuItem_Click();
        }

        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            #region
            if ((ScanZ.Checked == true) || (ScanY.Checked == true)) // условие запуска сканирования по выбору Z и Y
            {
                zaxod = false;
                if ("1" == form6.pokaz.Text)
                {
                    #region Поле управления Д=250 М
                    switch (rejim)
                    {
                        case 0:
                            if (Pole_250 == 0)
                            {
                                myTimer.Stop();
                                myTimer.Start();
                                ComPorts.posilca[1] = 27;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "BPO_START";
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.Kol_poket = false;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();
                                //rejim++;
                                Pole_250 = 1;
                            }
                            break;
                        case 1:
                            if (Pole_250 == 1)
                            {
                                ComPorts.posilca[1] = 14;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "Off_DP_Off";
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.Kol_poket = false;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();
                                System.Threading.Thread.Sleep(300);
                                Pole_250 = 2;
                            }
                            break;
                       
                        case 2:
                            if (Pole_250 == 2)
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
                                //System.Threading.Thread.Sleep(1000);
                                Pole_250 = 3;
                            }
                            break;
                        case 3:
                            if (Pole_250 == 3)
                            {
                                //System.Threading.Thread.Sleep(1500);
                                System.Threading.Thread.Sleep(200);
                                ComPorts.posilca[1] = 11;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel2.Text = "Laser_On";
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.Kol_poket = false;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                if (XMLW.stend == true)
                                {
                                    rejim++;
                                }
                                else
                                {
                                    ComPorts.Poket_Write();
                                }
                                toolStripStatusLabel1.Text = " ";
                                Pole_250 = 4;
                            }
                            break;
                        case 4:
                            if (Pole_250 == 4)
                            {
                                if (ScanZ.Checked == true)
                                {
                                    if (checkBox1.Checked == true)
                                    {
                                        ComPorts.posilca[1] = 45;
                                        toolStripStatusLabel2.Text = "ScanZ";
                                    }
                                    else
                                    {
                                        ComPorts.posilca[1] = 22;
                                        toolStripStatusLabel2.Text = "ScanZ";
                                    }
                                }
                                else
                                {
                                    if (ScanY.Checked == true)
                                    {
                                        if (checkBox1.Checked == true)
                                        {
                                            ComPorts.posilca[1] = 44;
                                            toolStripStatusLabel2.Text = "ScanY";
                                        }
                                        else
                                        {
                                            ComPorts.posilca[1] = 23;
                                            toolStripStatusLabel2.Text = "ScanY";
                                        }
                                    }
                                }
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 675;
                                ComPorts.size_On = 3;
                                ComPorts.pokets.poket1 = 1;
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();
                                toolStripStatusLabel1.Text = " ";
                                Pole_250 = 5;
                            }
                            break;
                        case 5:
                            if (Pole_250 == 5)
                            {
                                ComPorts.posilca[1] = 12;
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "Return";
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.Kol_poket = false;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();
                                Pole_250 = 6;
                            }
                            break;
                        case 6:
                            if (Pole_250 == 6)
                            {
                                ComPorts.posilca[1] = 16;
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "Shtorka_Off";
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.Kol_poket = false;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();

                                System.Threading.Thread.Sleep(500);


                              //  SetScanZero.BackColor = Color.Tomato;
                                podschet = true;
                                ComPorts.posilca[1] = 21;
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.Kol_poket = false;
                                toolStripStatusLabel2.Text = "SetScanZero";
                               
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                               // ComPorts.Poket_Write();
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                Pole_250 = 8;
                               // myTimer.Stop();
                                if (ScanZ.Checked == true)
                                    ScanZ.Checked = false;
                                else
                                    if(ScanY.Checked == true)
                                        ScanY.Checked = false;

                                if ((ScanZ.Checked == true) || (ScanY.Checked == true))
                                {
                                    number = 0;
                                    rejim = 0;
                                    for (int i = 0; i < ComPorts.schet_poket; i++)
                                    {
                                        for (int j = 0; j < 9; j++)
                                        {
                                            ComPorts.buffer[i][j] = 0;
                                        }   
                                    }
                                }
                               button6.Enabled = true;
                               button7.Enabled = true;
                               button8.Enabled = true;
                               button9.Enabled = true;
                               // SetScanZero.Enabled = true;
                                //Pysk.Enabled = true;
                            }
                            break;
                    }
                    #endregion
                }
                if ("2" == form6.pokaz.Text)
                {
                    #region Поле управления Д=250 М в режиме превышения
                    switch (rejim)
                    {
                        case 20:
                            if (Pole_250_P == 0)
                            {
                                ComPorts.posilca[1] = 14;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "Off_DP_Off";
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.Kol_poket = false;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();
                                System.Threading.Thread.Sleep(300);
                                Pole_250_P = 1;
                            }
                            break;
                        case 21:
                            if (Pole_250_P == 1)
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
                                System.Threading.Thread.Sleep(200);
                                Pole_250_P = 2;
                            }
                            break;
                        case 22:
                            if (Pole_250_P == 2)
                            {
                                ComPorts.posilca[1] = 11;
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "Laser_On";
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                if (XMLW.stend == true)
                                {
                                    rejim++;
                                }
                                else
                                {
                                    ComPorts.Poket_Write();
                                }
                                Pole_250_P = 3;
                            }
                            break;
                        case 23:
                            if (Pole_250_P == 3)
                            {
                                if (ScanZ.Checked == true)
                                {
                                    if (checkBox1.Checked == true)
                                    {
                                        ComPorts.posilca[1] = 45;
                                        toolStripStatusLabel2.Text = "ScanZ";
                                    }
                                    else
                                    {
                                        ComPorts.posilca[1] = 22;
                                        toolStripStatusLabel2.Text = "ScanZ";
                                    }
                                }
                                else
                                {
                                    if (ScanY.Checked == true)
                                    {
                                        if (checkBox1.Checked == true)
                                        {
                                            ComPorts.posilca[1] = 44;
                                            toolStripStatusLabel2.Text = "ScanY";
                                        }
                                        else
                                        {
                                            ComPorts.posilca[1] = 23;
                                            toolStripStatusLabel2.Text = "ScanY";
                                        }
                                    }
                                }
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 675;
                                ComPorts.size_On = 3;
                                ComPorts.pokets.poket1 = 1;
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();
                                toolStripStatusLabel1.Text = " ";
                                Pole_250_P = 4;
                            }
                            break;
                        case 24:
                            if (Pole_250_P == 4)
                            {
                                ComPorts.posilca[1] = 12;
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "Return";
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();
                                Pole_250_P = 5;

                                ComPorts.posilca[1] = 16;
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "Shtorka_Off";
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.Kol_poket = false;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();
                                System.Threading.Thread.Sleep(5000);

                                
                                ComPorts.posilca[1] = 21;
                                podschet = true;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "SetScanZero";
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();
                                SetScanZero.BackColor = Color.Tomato;
                                Pole_250_P = 5;
                                //myTimer.Stop();
                                if (ScanZ.Checked == true)
                                    ScanZ.Checked = false;
                                else
                                    if (ScanY.Checked == true)
                                        ScanY.Checked = false;
                                if ((ScanZ.Checked == true) || (ScanY.Checked == true))
                                {
                                    number = 0;
                                    rejim = 0;
                                    for (int i = 0; i < ComPorts.schet_poket; i++)
                                    {
                                        for (int j = 0; j < 9; j++)
                                        {
                                            ComPorts.buffer[i][j] = 0;
                                        }
                                    }
                                }
                                button6.Enabled = true;
                                button7.Enabled = true;
                                button8.Enabled = true;
                                button9.Enabled = true;
                                //SetScanZero.Enabled = true;
                                //Pysk.Enabled = true;
                            }
                            break;
                    }
                }
                    #endregion

                if ("3" == form6.pokaz.Text)
                {
                    #region Поле управления Д=5880 М
                    switch (rejim)
                    {
                        case 40:
                            if (Pole_5880 == 0)
                            {
                                ComPorts.posilca[1] = 11;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                ComPorts.pokets.poket1 = 0;
                                toolStripStatusLabel1.Text = "  ";
                                toolStripStatusLabel2.Text = "Laser_On";
                                ComPorts.Kol_poket = false;
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                if (XMLW.stend == true)
                                {
                                    rejim++;
                                }
                                else
                                {
                                    ComPorts.Poket_Write();
                                }
                                System.Threading.Thread.Sleep(8);
                                Pole_5880 = 1;
                            }
                            break;
                        case 41:
                            if (Pole_5880 == 1)
                            {
                                if (ScanZ.Checked == true)
                                {
                                    if (checkBox1.Checked == true)
                                    {
                                        ComPorts.posilca[1] = 45;
                                        toolStripStatusLabel2.Text = "ScanZ";
                                    }
                                    else
                                    {
                                        ComPorts.posilca[1] = 22;
                                        toolStripStatusLabel2.Text = "ScanZ";
                                    }
                                }
                                else
                                {
                                    if (ScanY.Checked == true)
                                    {
                                        if (checkBox1.Checked == true)
                                        {
                                            ComPorts.posilca[1] = 44;
                                            toolStripStatusLabel2.Text = "ScanY";
                                        }
                                        else
                                        {
                                            ComPorts.posilca[1] = 23;
                                            toolStripStatusLabel2.Text = "ScanY";
                                        }
                                    }
                                }
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 675;
                                ComPorts.size_On = 3;
                                ComPorts.pokets.poket1 = 1;
                                toolStripStatusLabel1.Text = "  ";
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();
                                Pole_5880 = 2;
                            }
                            break;
                        case 42:
                            if (Pole_5880 == 2)
                            {
                                ComPorts.posilca[1] = 12;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                ComPorts.pokets.poket1 = 0;
                                toolStripStatusLabel1.Text = "  ";
                                toolStripStatusLabel2.Text = "Return";
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.Kol_poket = false;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();
                                Pole_5880 = 3;

                                System.Threading.Thread.Sleep(200);
                                SetScanZero.BackColor = Color.Tomato;
                                podschet = true;
                                ComPorts.posilca[1] = 21;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "SetScanZero";
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.Kol_poket = false;
                                System.Threading.Thread.Sleep(20);
                                ComPorts.Poket_Write();
                                Pole_5880 = 8;
                                //myTimer.Stop();
                                if (ScanZ.Checked == true)
                                    ScanZ.Checked = false;
                                else
                                    if (ScanY.Checked == true)
                                        ScanY.Checked = false;
                                if ((ScanZ.Checked == true) || (ScanY.Checked == true))
                                {
                                    number = 0;
                                    rejim = 0;
                                    for (int i = 0; i < ComPorts.schet_poket; i++)
                                    {
                                        for (int j = 0; j < 9; j++)
                                        {
                                            ComPorts.buffer[i][j] = 0;
                                        }
                                    }
                                }
                                button6.Enabled = true;
                                button7.Enabled = true;
                                button8.Enabled = true;
                                button9.Enabled = true;
                                //SetScanZero.Enabled = true;
                                //Pysk.Enabled = true;
                            }
                            break;
                    }

                    #endregion
                }
            }
            #endregion

            #region Поле = 250
            if (Begin == true)
            {
                switch (rejim1)
                {
                    case 0:
                        if (Pole_250_1 == 0)
                        {
                            ComPorts.posilca[1] = 10;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 6;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "Get_Status";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            Pole_250_1 = 1;
                        }
                        break;
                    case 1:
                        if (Pole_250_1 == 1)
                        {
                            ComPorts.posilca[1] = 14;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "Off_DP_Off";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.Poket_Write();
                            System.Threading.Thread.Sleep(200);
                            toolStripStatusLabel3.Text = "Выключение блокировки панкратики";
                            Pole_250_1 = 2;
                        }
                        break;
                    case 2:
                        if (Pole_250_1 == 2)
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
                            toolStripStatusLabel3.Text = "Перевод БПО в начальное положение";
                            Pole_250_1 = 3;
                           // rejim1++;
                        }
                        break;
                    case 3:
                        if (Pole_250_1 == 3)
                        {
                            ComPorts.posilca[1] = 18;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 6;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "Test_BVD";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            Pole_250_1 = 4;

                            ScanZ.Enabled = true;
                            ScanY.Enabled = true;
                            Pysk.Enabled = true;
                            SetScanZero.Enabled = true;
                            button6.Enabled = true;
                            button7.Enabled = true;
                            button8.Enabled = true;
                            button9.Enabled = true;
                            toolStripStatusLabel3.Text = " ";
                        }
                        break;
                }
            }
            #endregion

            #region Поле = 250 в режиме ПРЕВЫШЕНИЕ
            if (Begin1 == true)
            {
                switch (rejim1)
                {
                    case 0:
                        if (Pole_250_P_1 == 0)
                        {
                            zaxod = true;
                            ComPorts.posilca[1] = 10;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 6;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "Get_Status";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            Pole_250_P_1 = 1;
                        }
                        break;
                    case 1:
                        if (Pole_250_P_1 == 1)
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
                            toolStripStatusLabel3.Text = "Перевод БПО в начальное положение";
                            Pole_250_P_1 = 2;
                        }
                        break;
                    case 2:
                        if (Pole_250_P_1 == 2)
                        {
                            ComPorts.posilca[1] = 14;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "Off_DP_Off";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.Poket_Write();
                            toolStripStatusLabel3.Text = "Выключение блокировки панкратики";
                            System.Threading.Thread.Sleep(200);

                            ComPorts.posilca[1] = 18;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 6;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "Test_BVD";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            Pole_250_P_1 = 3;
                            ScanZ.Enabled = true;
                            ScanY.Enabled = true;
                            Pysk.Enabled = true;
                            SetScanZero.Enabled = true;
                            button6.Enabled = true;
                            button7.Enabled = true;
                            button8.Enabled = true;
                            button9.Enabled = true;
                            toolStripStatusLabel3.Text = " ";
                        }
                        break;
                }
            }
            #endregion

            #region Поле = 5880
            if (Begin2 == true)
            {
                switch (rejim1)
                {
                    case 0:
                        if (Pole_5880_1 == 0)
                        {
                            ComPorts.posilca[1] = 10;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 6;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "Get_Status";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            Pole_5880_1 = 1;
                        }
                        break;
                    case 1:
                        if (Pole_5880_1 == 1)
                        {
                            ComPorts.posilca[1] = 28;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = " ";
                            toolStripStatusLabel2.Text = "BPO_END";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            toolStripStatusLabel3.Text = "Перевод БПО в конечное положение";
                            Pole_5880_1 = 2;
                            //rejim1++;
                        }
                        break;
                    case 2:
                        if (Pole_5880_1 == 2)
                        {
                            ComPorts.posilca[1] = 14;
                            toolStripStatusLabel2.Text = "Off_DP_Off";
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            ComPorts.pokets.poket1 = 0;
                            toolStripStatusLabel1.Text = " ";
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            toolStripStatusLabel3.Text = "Выключение блокировки панкратики";
                            System.Threading.Thread.Sleep(2000);
                            Pole_5880_1 = 3;
                        }
                        break;
                    case 3:
                        if (Pole_5880_1 == 3)
                        {
                            ComPorts.posilca[1] = 19;
                            toolStripStatusLabel2.Text = "Pankr_End_On";
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            ComPorts.pokets.poket1 = 0;
                            toolStripStatusLabel1.Text = " ";
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            toolStripStatusLabel3.Text = "Перевод ПС в конечное положение";
                            Pole_5880_1 = 4;
                        }
                        break;
                    case 4:
                        if (Pole_5880_1 == 4)
                        {
                            if ("3" == form6.pokaz.Text)
                            {
                                ComPorts.posilca[1] = 18;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 6;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "Test_BVD";
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.Kol_poket = false;


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
                                Pricel = 1;
                                toolStripStatusLabel3.Text = "Открытие шторки";
                                System.Threading.Thread.Sleep(300);

                            
                                ComPorts.Poket_Write();
                                Pole_5880_1 = 5;
                                ScanZ.Enabled = true;
                                ScanY.Enabled = true;
                                Pysk.Enabled = true;
                                SetScanZero.Enabled = true;
                                button6.Enabled = true;
                                button7.Enabled = true;
                                button8.Enabled = true;
                                button9.Enabled = true;
                                toolStripStatusLabel3.Text = " ";
                            }
                            Pole_5880_1 = 5;
                        }
                        break;
                        
                }
            }
            #endregion

            if (Pysk.Checked == true)
            {
                #region Прицеливание
                switch (rejim2)
                {
                    case 0:
                        if (Pricel == 0)
                        {
                            if (("1" == form6.pokaz.Text) || ("2" == form6.pokaz.Text))
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
                                Pricel = 1;
                                //System.Threading.Thread.Sleep(300);
                                System.Threading.Thread.Sleep(1000);
                            }
                            else
                            {
                                Pricel = 1;
                                rejim2++;
                            }
                        }
                            break;
                    case 1:
                            if (Pricel == 1)
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
                                ComPorts.Poket_Write();
                                Pricel = 2;
                            }
                            break;
                        case 2:
                            if (Pricel == 2)
                            {
                                ComPorts.posilca[1] = 25;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 675;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "ADC_On";
                                ComPorts.pokets.poket1 = 1;
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.Kol_poket = false;
                                ComPorts.Poket_Write();
                                Pricel = 3;
                            }
                            break;
                    }
                #endregion
            }

            #region Вывод на экран данных

            ZedGraphControl Zedgraph = new ZedGraphControl();
            Zedgraph.Size = zedGraphControl1.Size;
            zedGraphControl1.GraphPane = Zedgraph.GraphPane;
           
            if (ComPorts.Kol_poket == true)
            {
                #region ADC_OUT
                for (l = number; l < ComPorts.schet_poket; l++)
                {
                    Kor_Z1 = (short)((((ComPorts.buffer[l][3] << 8) & 0x7F00) | ComPorts.buffer[l][2]));
                    Kor_Y1 = (short)((((ComPorts.buffer[l][5] << 8) & 0x7F00) | ComPorts.buffer[l][4]));
                    Kor_P = (double)((double)((double)((double)(((ComPorts.buffer[l][7] << 8) & 0x7F00) | ComPorts.buffer[l][6]) / 1024)));

                    #region KOR_SHUM      
                        //Корректировка кода согласно ТЗ от 4.01.2017. Принес Малашко Денис с подписью Соколовского и Штыцко. На графике освещенности все значения понизить на величину 15 единиц АЦП, значения менее 15 единиц АЦП приравнять к нулю (дословно)
                        //Путем анализа кода выяснилось, что на графике Освещенность рисуется оранжевым цветом и под буквой E. Данные этого графика составляют значения из списка list3
                            if (Kor_P < 12.0 / 1024.0)
                            {
                                Kor_P = 0;
                            }
                            else
                            {
                                Kor_P -= 12.0 / 1024.0;
                            }
                        //Конец корректировки согласно ТЗ от 4.01.2017
                    #endregion



                    if ((ComPorts.buffer[l][3] == 0x80) || (ComPorts.buffer[l][3] == 0x81) || (ComPorts.buffer[l][3] == 0x82) || (ComPorts.buffer[l][3] == 0x83))
                    {

                        Kor_Z = (double)((double)(((Kor_Z1) / (double)(1024)) - XMLW.Cmehenie_Z) / (XMLW.MZ / XMLW.Const)); // смешение от нуля...тарировочный коээфф...5 вольт!
                        Kor_Z = Kor_Z * (-1);
                    }
                    else
                    {
                        Kor_Z = (double)((double)(((Kor_Z1) / (double)(1024)) + XMLW.Cmehenie_Z) / (XMLW.PZ / XMLW.Const));
                    }

                    if ((ComPorts.buffer[l][5] == 0x80) || (ComPorts.buffer[l][5] == 0x81) || (ComPorts.buffer[l][5] == 0x82) || (ComPorts.buffer[l][5] == 0x83))
                    {
                        Kor_Y = (double)((double)(((Kor_Y1) / (double)(1024)) - XMLW.Cmehenie_Y) / (XMLW.MY / XMLW.Const));
                        Kor_Y = Kor_Y * (-1);
                    }
                    else
                    {
                        Kor_Y = (double)((double)(((Kor_Y1) / (double)(1024)) + XMLW.Cmehenie_Y) / (XMLW.PY / XMLW.Const));
                    }

                    if (Kor_P >= 0)
                    {
                        list1.Add(l, Kor_Z);
                        list2.Add(l, Kor_Y);
                        list3.Add(l, Kor_P);
                    }
                    //textBox2.Text = Convert.ToString(Math.Round(Kor_Z,2));
                    //textBox2.Refresh();
                    textBox3.Text = Convert.ToString(Math.Round(Kor_Y, 2));
                    textBox3.Refresh();
                    if (l > 3300)
                    {
                        canselPr();

                        XMLW.endpropise = true;

                        a = XMLW.SearchPoint(proverka == true ? list1 : list2, 0.8, list2.Count, 0);
                        b = XMLW.SearchPoint(proverka == true ? list1 : list2, -0.8, 0, list2.Count);
                        c = XMLW.SearchPoint(proverka == true ? list1 : list2, 0, a, b);

                        obsKof = XMLW.V * XMLW.Tay * 0.25 * (Math.PI) * Math.Pow(((XMLW.D * L / focus)), 2);


                        textBox8.Text = Convert.ToString(Math.Round((((list3[c].Y) / obsKof) * 1024), 2)); //под вопросом!
                        textBox9.Text = Convert.ToString(Math.Round((((list3[b].Y) / obsKof) * 1024), 2));
                        textBox10.Text = Convert.ToString(Math.Round((((list3[a].Y) / obsKof) * 1024), 2));


                        trackBar1.Maximum = XMLW.trBarMax;
                        trackBar2.Maximum = XMLW.trBarMax;
                        trackBar1.Enabled = true;
                        trackBar2.Enabled = true;

                        rejim++;
                        timer_25 = 0;
                        break;
                        myTimer.Stop();
                    }

                    number++;

                }

                if (Pysk.Checked == true)
                {
                    if (timer_25 >= 2500)
                    {
                        Pysk.Checked = false;
                        timer_25 = 0;
                    }
                    else
                        timer_25 += 10;
                }
                #endregion
            }
            else
            {
                if (ComPorts.One_Buffers == true)
                {
                    ReadBuffers1 = ComPorts.pokets.buffer1;

                    #region ADC_OUT однократно
                    if (ReadBuffers1[1] == 104)
                    {

                        Kor_Z1 = (short)((((ReadBuffers1[3] << 8) & 0x7F00) | ReadBuffers1[2]));
                        Kor_Y1 = (short)((((ReadBuffers1[5] << 8) & 0x7F00) | ReadBuffers1[4]));
                        Kor_P = (double)((double)((double)((double)(((ReadBuffers1[7] << 8) & 0x7F00) | ReadBuffers1[6]) / 1024)));

                        Kor_Z = (double)((double)(Kor_Z1) / (double)(1024));
                        Kor_Y = (double)((double)(Kor_Y1) / (double)(1024));
                        if ((ReadBuffers1[3] == 0x80) || (ReadBuffers1[3] == 0x81) || (ReadBuffers1[3] == 0x82) || (ReadBuffers1[3] == 0x83))
                        {
                            Kor_Z = Kor_Z * (-1);
                        }
                        if ((ReadBuffers1[5] == 0x80) || (ReadBuffers1[5] == 0x81) || (ReadBuffers1[5] == 0x82) || (ReadBuffers1[5] == 0x83))
                        {
                            Kor_Y = Kor_Y * (-1);
                        }
                        //textBox2.Text = Convert.ToString(Math.Round(Kor_Z,2));
                        //textBox2.Refresh();
                        textBox3.Text = Convert.ToString(Math.Round(Kor_Y,2));
                        textBox3.Refresh();
                    }

                    //CreateGraph(Zedgraph);
                    //zedGraphControl1.Refresh();
                    #endregion

                    #region GetStatus


                    if (ReadBuffers1[1] == 100)
                    {
                        #region Byte2
                        byte a;             // переменные для перевода в двоичную чистему счисления
                        int i = 0;
                        a = (byte)(ReadBuffers1[2]);
                        while (a > 1)
                        {
                            GetStatus_Byte1[i] = (byte)(a % 2);
                            a = (byte)(a / 2);
                            i++;
                        }
                        if (ReadBuffers1[2] == 0)
                        {
                            GetStatus_Byte1[i] = 0;
                        }
                        else
                            GetStatus_Byte1[i] = 1;

                        if (GetStatus_Byte1[0] == 1) //Готов_У
                        {
                            form4.checkBox1.Checked = true;
                        }
                        else
                            form4.checkBox1.Checked = false;

                        if (GetStatus_Byte1[1] == 1) //ЭМП
                        {
                            form4.checkBox2.Checked = true;
                        }
                        else
                            form4.checkBox2.Checked = false;
                        if (GetStatus_Byte1[2] == 1) // ДП
                        {
                            form4.checkBox3.Checked = true;
                        }
                        else
                            form4.checkBox3.Checked = false;
                        if (GetStatus_Byte1[3] == 1) // Сход БОМ
                        {
                            form4.checkBox4.Checked = true;
                        }
                        else
                            form4.checkBox4.Checked = false;
                        if (GetStatus_Byte1[4] == 1) // Подсветка_Вкл
                        {
                            form4.checkBox5.Checked = true;
                        }
                        else
                            form4.checkBox5.Checked = false;
                        if (GetStatus_Byte1[5] == 1) // БС_Вкл
                        {
                            form4.checkBox6.Checked = true;
                        }
                        else
                            form4.checkBox6.Checked = false;
                        if (GetStatus_Byte1[6] == 1) // Перегрев
                        {
                            form4.checkBox7.Checked = true;
                        }
                        else
                            form4.checkBox7.Checked = false;
                        if (GetStatus_Byte1[7] == 1)
                        {
                            form4.checkBox15.Checked = true;
                        }
                        else
                            form4.checkBox15.Checked = false;


                        #endregion

                        #region Byte3

                        a = 0;
                        for (i = 0; i < 8; i++)
                            GetStatus_Byte2[i] = 0;
                        a = (byte)(ReadBuffers1[3]);
                        i = 0;
                        while (a > 1)
                        {
                            GetStatus_Byte2[i] = (byte)(a % 2);
                            a = (byte)(a / 2);
                            i++;

                        }
                        if (ReadBuffers1[3] == 0)
                        {
                            GetStatus_Byte2[i] = 0;
                        }
                        else
                            GetStatus_Byte2[i] = 1;


                        if (GetStatus_Byte2[0] == 1) // Z центр
                        {
                            form4.checkBox14.Checked = true;
                        }
                        else
                            form4.checkBox14.Checked = false;


                        if (GetStatus_Byte2[1] == 1) // Z лево
                        {
                            form4.checkBox13.Checked = true;
                        }
                        else
                            form4.checkBox13.Checked = false;
                        if (GetStatus_Byte2[2] == 1) // У центр
                        {
                            form4.checkBox12.Checked = true;
                        }
                        else
                            form4.checkBox12.Checked = false;
                        if (GetStatus_Byte2[3] == 1) // У лево
                        {
                            form4.checkBox11.Checked = true;
                        }
                        else
                            form4.checkBox11.Checked = false;
                        if (GetStatus_Byte2[4] == 1) // BPO_START
                        {
                            form4.checkBox10.Checked = true;
                        }
                        else
                            form4.checkBox10.Checked = false;
                        if (GetStatus_Byte2[5] == 1) // BPO_END
                        {
                            form4.checkBox9.Checked = true;
                        }
                        else
                            form4.checkBox9.Checked = false;
                        if (GetStatus_Byte2[6] == 1) // У_Вкл
                        {
                            form4.checkBox8.Checked = true;
                        }
                        else
                            form4.checkBox8.Checked = false;


                        if (GetStatus_Byte2[7] == 1) // У1_Вкл
                        {
                            form4.checkBox16.Checked = true;
                        }
                        else
                            form4.checkBox16.Checked = false;


                        if ((ScanZ.Checked == true) || (ScanY.Checked == true)) // условие запуска сканирования по выбору Z и Y
                        {
                            if ("1" == form6.pokaz.Text)
                            {
                                if ((GetStatus_Byte1[0] == 1) && (GetStatus_Byte2[5] == 1))
                                {
                                    rejim++;
                                }

                              /*  if (toolStripStatusLabel2.Text == "ScanZ")
                                {
                                    if (GetStatus_Byte2[1] == 1)
                                    {
                                        rejim++;
                                    }
                                }
                                else
                                {
                                    if (toolStripStatusLabel2.Text == "ScanY")
                                    {
                                        if (GetStatus_Byte2[3] == 1)
                                        {
                                            rejim++;
                                        }
                                    }

                                }*/
                            }
                            if ("2" == form6.pokaz.Text)
                            {
                                if ((GetStatus_Byte1[0] == 1) && (GetStatus_Byte2[5] == 1))
                                {
                                    rejim++;
                                }
                                /*if (toolStripStatusLabel2.Text == "ScanZ")
                                {
                                    if (GetStatus_Byte1[1] == 1)
                                    {
                                        rejim++;
                                    }
                                }
                                else
                                {
                                    if (toolStripStatusLabel2.Text == "ScanY")
                                    {
                                        if (GetStatus_Byte2[2] == 1)
                                        {
                                            rejim++;
                                        }
                                    }

                                }*/
                            }
                            if ("3" == form6.pokaz.Text)
                            {
                                if ((GetStatus_Byte1[0] == 1) && (GetStatus_Byte2[5] == 1))
                                {
                                    rejim++;
                                }

                               /* if (toolStripStatusLabel2.Text == "ScanZ")
                                {
                                    if (GetStatus_Byte1[1] == 1)
                                    {
                                        rejim++;
                                    }
                                }
                                else
                                {
                                    if (toolStripStatusLabel2.Text == "ScanY")
                                    {
                                        if (GetStatus_Byte2[2] == 1)
                                        {
                                            rejim++;
                                        }
                                    }

                                }*/
                            }
                        }


                        #endregion

                        if (GetStatus_Byte1[0] == 1) //////// еще надо проверить
                        {                            ////////
                            rejim1++;                 ////////
                        }
                        else
                        {

                            Begin = false;
                            rejim1 = 0;
                            myTimer.Stop();
                            MessageBox.Show("Готов_У не установлено");
                        }

                    }



                    #endregion

                    #region Test_BVD

                    if (ReadBuffers1[1] == 102)
                    {
                        int i = 0;
                        byte[] c = new byte[8];
                        byte a = 0;
                        a = (byte)(ReadBuffers1[2]);
                        while (a > 1)
                        {
                            c[i] = (byte)(a % 2);
                            a = (byte)(a / 2);
                            i++;
                        }
                        if (ReadBuffers1[2] == 0)
                        {
                            c[i] = 0;
                        }
                        else
                            c[i] = 1;

                        if (c[0] == 1)
                        {
                            form4.checkBox1.Checked = true;
                        }
                        else
                            form4.checkBox1.Checked = false;

                        if (c[1] == 1)
                        {
                            form4.checkBox2.Checked = true;
                        }
                        else
                            form4.checkBox2.Checked = false;
                        if (c[2] == 1)
                        {
                            form4.checkBox3.Checked = true;
                        }
                        else
                            form4.checkBox3.Checked = false;
                        if (c[3] == 1)
                        {
                            form4.checkBox4.Checked = true;
                        }
                        else
                            form4.checkBox4.Checked = false;

                        rastor = (double)((((ReadBuffers1[4] << 8) & 0x7F00) | ReadBuffers1[3]));
                        if (rastor == 500)
                        {
                            rejim1++;
                        }
                        //if ((rastor < 480) || (rastor > 520))
                        //{
                        //    Begin = false;
                        //    MessageBox.Show("Недостаточная частота растра");
                        //    rejim1 = 0;
                        //    myTimer.Stop();
                        //}
                        //else
                        //    rejim1++;
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
                       
                        if (toolStripStatusLabel2.Text == "Return")
                        {
                            rejim1++;
                        }
                        if (toolStripStatusLabel2.Text == "Pankr_End_On")
                        {
                            rejim1++;
                        }
                        if (toolStripStatusLabel2.Text == "Off_DP_Off")
                        {
                            rejim1++;
                        }
                        if (toolStripStatusLabel2.Text == "BPO_END")
                        {
                            rejim1++;
                        }
                        if (toolStripStatusLabel2.Text == "SetScanZero")
                        {
                               rejim1++;
                        }

                        if (Pysk.Checked == true)
                        {
                            if (toolStripStatusLabel2.Text == "Shtorka_On")
                            {
                                rejim2++;
                            }
                            if (toolStripStatusLabel2.Text == "Laser_On")
                            {
                                rejim2++;
                            }   
                        }
                        #region Ответы по выбору - поля управления!
                        if ((ScanZ.Checked == true) || (ScanY.Checked == true))
                        {
                            #region Поле управления Д=250 М
                            
                                if ("1" == form6.pokaz.Text)
                                {
                                    if (toolStripStatusLabel2.Text == "BPO_START")
                                    {
                                        rejim++;
                                    }
                                    if (toolStripStatusLabel2.Text == "Laser_On")
                                    {
                                        rejim++;
                                    }
                                    if (toolStripStatusLabel2.Text == "Shtorka_On")
                                    {
                                        rejim++;
                                    }
                                    if (toolStripStatusLabel2.Text == "SetScanZero")
                                    {
                                        rejim++;
                                        SetScanZero.BackColor = Color.Red;
                                        myTimer.Stop();
                                       
                                    }
                                    if (toolStripStatusLabel2.Text == "Return")
                                    {
                                        rejim++;
                                    }
                                    if (toolStripStatusLabel2.Text == "Off_DP_Off")
                                    {
                                        rejim++;
                                    }

                                }
                            
                            #endregion

                            #region Поле управления Д=250 М в режиме превышения
                            if ("2" == form6.pokaz.Text)
                            {
                                if (toolStripStatusLabel2.Text == "BPO_START")
                                {
                                    rejim++;
                                }
                                if (toolStripStatusLabel2.Text == "Laser_On")
                                {
                                    rejim++;
                                }
                                if (toolStripStatusLabel2.Text == "Shtorka_On")
                                {
                                    rejim++;
                                }
                                if (toolStripStatusLabel2.Text == "SetScanZero")
                                {
                                    SetScanZero.BackColor = Color.Red;
                                    rejim++;
                                    myTimer.Stop();
                                }
                                if (toolStripStatusLabel2.Text == "Return")
                                {
                                    rejim++;
                                }
                                if (toolStripStatusLabel2.Text == "Off_DP_Off")
                                {
                                    rejim++;
                                }
                            }
                            #endregion

                            #region Поле управления Д=5880 М
                            if ("3" == form6.pokaz.Text)
                            {
                                if (toolStripStatusLabel2.Text == "BPO_END")
                                {
                                    rejim++;
                                }
                                if (toolStripStatusLabel2.Text == "Laser_On")
                                {
                                    rejim++;
                                }
                                if (toolStripStatusLabel2.Text == "Shod")
                                {
                                    rejim++;
                                }
                                if (toolStripStatusLabel2.Text == "SetScanZero")
                                {
                                    rejim++;
                                    SetScanZero.BackColor = Color.Red;
                                    myTimer.Stop();
                                }
                                if (toolStripStatusLabel2.Text == "Return")
                                {
                                    rejim++;
                                }
                                if (toolStripStatusLabel2.Text == "Pankr_End_On")
                                {
                                    rejim++;
                                }
                                if (toolStripStatusLabel2.Text == "Off_DP_Off")
                                {
                                    rejim++;
                                }
                            }
                            #endregion
                        }
                        #endregion
                    }
                    else
                    {
                        if (toolStripStatusLabel2.Text == "BPO_START")
                        {
                            Begin = false;
                            myTimer.Stop();
                            MessageBox.Show("Панкратика не в начальном положение");
                            rejim1 = 0;
                        }
                        if (toolStripStatusLabel2.Text == "SetScanZero")
                        {
                            myTimer.Stop();
                            MessageBox.Show("Сканатор не в нулевом положении");
                            Begin = false;
                            rejim1 = 0;
                        }
                        
                    }
   

                    #region Test_time
                    if (ReadBuffers1[1] == 103)
                    {
                        time_muve_pankr = (short)((((ReadBuffers1[4] << 8) & 0xFF00) | ReadBuffers1[3]));
                        time_begin_sniatia_prevish = (short)((((ReadBuffers1[6] << 8) & 0xFF00) | ReadBuffers1[5]));
                        time_sniatia_prevish = (short)((((ReadBuffers1[8] << 8) & 0xFF00) | ReadBuffers1[7]));
                        time_vozvrata = (short)((((ReadBuffers1[10] << 8) & 0xFF00) | ReadBuffers1[9]));
                    }
                    #endregion
                    #endregion

                }
            }


            ComPorts.One_Buffers = false;
            CreateGraph(Zedgraph);
            zedGraphControl1.Refresh();
            Zedgraph.Dispose();
            #endregion


            if (podschet == true)
            {
                if (podschet_1 >= 2800)
                {
                    SetScanZero.BackColor = Color.WhiteSmoke;
                    podschet = false;
                    podschet_1 = 0;
                    ScanZ.Enabled = true;
                    ScanY.Enabled = true;
                    Pysk.Enabled = true;
                    SetScanZero.Enabled = true;
                    cansel.Enabled = true;
                    button6.Enabled = true;
                    button7.Enabled = true;
                    button8.Enabled = true;
                    button9.Enabled = true;
                }
                else
                {
                    podschet_1 += 10;
                }
 
            }
        }

        private void Laser_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 11;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            toolStripStatusLabel2.Text = "Laser_On";
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 12;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            toolStripStatusLabel2.Text = "Return";
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
           
            if("1" == form6.pokaz.Text)
                Pole_250 = 5;
            if("2" == form6.pokaz.Text)
                Pole_250_P = 5;
            if("3" == form6.pokaz.Text)
                Pole_5880 = 4;
        }

        private void shtorka_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 15;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            toolStripStatusLabel2.Text = "Shtorka_On";
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
        }

        private void BpoStart_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 27;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            toolStripStatusLabel2.Text = "BPO_START";
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
        }

        private void BpoEnd_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 28;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            toolStripStatusLabel2.Text = "BPO_END";
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
        }

        private void Pysk_CheckedChanged(object sender, EventArgs e)
        {
            first_click++;
            if (first_click == 1)
            {
                Pysk.Text = "СТОП";
                list1.Clear();
                list2.Clear();
                list3.Clear();
                for (int i = 0; i < number; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        ComPorts.buffer[i][j] = 0;
                    }

                }
                number = 0;
                ComPorts.schet_poket1 = 0;
                ComPorts.schet_poket = 0;
                ZedGraphControl Zedgraph = new ZedGraphControl();
                Zedgraph.Size = zedGraphControl1.Size;
                zedGraphControl1.GraphPane = Zedgraph.GraphPane;
                CreateGraph(Zedgraph);
                zedGraphControl1.Refresh();
                Zedgraph.Dispose();
                timer_25 = 0;
                Pricel = 0;
                rejim2 = 0;
                XMLW.endpropise = false;

                ScanZ.Enabled = false;// для проверки коллиматора, после раскоментировать
                ScanY.Enabled = false;
                SetScanZero.Enabled = false;
                cansel.Enabled = false;
                button6.Enabled = false;
                button7.Enabled = false;
                button8.Enabled = false;
                button9.Enabled = false;
            }
            if (first_click == 2)
            {
                #region
                ComPorts.posilca[1] = 26;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "ADC_Off";
                ComPorts.pokets.poket1 = 0;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();

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
                System.Threading.Thread.Sleep(30);


                if (("1" == form6.pokaz.Text) || ("2" == form6.pokaz.Text))
                {
                    ComPorts.posilca[1] = 14;
                    ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                    ComPorts.size_Off = 3;
                    ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                    ComPorts.size_On = 3;
                    toolStripStatusLabel1.Text = " ";
                    toolStripStatusLabel2.Text = "Off_DP_OFF";
                    ComPorts.pokets.poket1 = 0;
                    ComPorts.port.DiscardInBuffer();
                    ComPorts.port.DiscardOutBuffer();
                    ComPorts.Kol_poket = false;
                    ComPorts.Poket_Write();
                    System.Threading.Thread.Sleep(300);
                }
                first_click = 0;
              #endregion

                Pysk.Text = "ПУСК";
                XMLW.endpropise = true;
                number = 0;
                ScanZ.Enabled = true;// для проверки коллиматора, после раскоментировать
                ScanY.Enabled = true;
                SetScanZero.Enabled = true;
                cansel.Enabled = true;
                button6.Enabled = true;
                button7.Enabled = true;
                button8.Enabled = true;
                button9.Enabled = true;
            }

        }

        private void ScanZ_CheckedChanged(object sender, EventArgs e)
        {
            this.Text = zagolovok + " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")" + "  Скан. по Z";
            this.Tag = tegg + "Z    ";
            proverka = true;
            centr = 1;
            Scan_Z_click++;
            if (Scan_Z_click == 1)
            {
                z_y = 1;
                добавитьToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                добавитьToolStripMenuItem.Text = "Сохранить результат";
                добавитьВОтчетToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                добавитьВОтчетToolStripMenuItem.Text = "Добавить в отчет";
                XMLW.endpropise = false;
                list1.Clear();
                list2.Clear();
                list3.Clear();
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                for (int i = 0; i < number; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        ComPorts.buffer[i][j] = 0;
                    }

                }
                number = 0;
                ComPorts.schet_poket1 = 0;
                ComPorts.schet_poket = 0;
                ZedGraphControl Zedgraph = new ZedGraphControl();
                Zedgraph.Size = zedGraphControl1.Size;
                zedGraphControl1.GraphPane = Zedgraph.GraphPane;
                CreateGraph(Zedgraph);
                zedGraphControl1.Refresh();
                Zedgraph.Dispose();
                timer_25 = 0;
                textBox4.Text = "0";
                textBox5.Text = "0";
                textBox6.Text = "0";
                textBox7.Text = "0";
                textBox8.Text = "0";
                textBox9.Text = "0";
                textBox10.Text = "0";
                if ("1" == form6.pokaz.Text)
                {
                    Pole_250 = 0;
                    rejim = 0;
                }
                else
                {
                    if ("2" == form6.pokaz.Text)
                    {
                        Pole_250_P = 0;
                        rejim = 20;
                    }
                    else
                    {
                        if ("3" == form6.pokaz.Text)
                        {
                            Pole_5880 = 0;
                            rejim = 40;
                        }
                    }
                }
                ScanY.Enabled = false;
                myTimer.Start();
              //  Pysk.Hide();
                button6.Enabled = false;
                button7.Enabled = false;
                button8.Enabled = false;
                button9.Enabled = false;
                SetScanZero.Enabled = false;
                Pysk.Enabled = false;

            }
            if (Scan_Z_click == 2)
            {
              //  myTimer.Stop();
                Pysk.Show();
                timer_25 = 0;
                Scan_Z_click = 0;
                //ScanY.Enabled = true;
                if ("1" == form6.pokaz.Text)
                {
                    Pole_250 = 0;
                    rejim = 0;
                }
                else
                {
                    if ("2" == form6.pokaz.Text)
                    {
                        Pole_250_P = 0;
                        rejim = 20;
                    }
                    else
                    {
                        if ("3" == form6.pokaz.Text)
                        {
                            Pole_5880 = 0;
                            rejim = 40;
                        }
                    }
                }
            }
        }

        private void ScanY_CheckedChanged(object sender, EventArgs e)
        {
            centr = 1;
            this.Text = zagolovok + " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")" + "  Скан. по Y";
            this.Tag = tegg + "Y    ";
            proverka = false;
            Scan_Y_click++;
            if (Scan_Y_click == 1)
            {
                z_y = 2;
                добавитьToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                добавитьToolStripMenuItem.Text = "Сохранить результат";
                добавитьВОтчетToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                добавитьВОтчетToolStripMenuItem.Text = "Добавить в отчет";
                XMLW.endpropise = false;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                list1.Clear();
                list2.Clear();
                list3.Clear();
                timer_25 = 0;
                for (int i = 0; i < number; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        ComPorts.buffer[i][j] = 0;
                    }

                }
                number = 0;
                ComPorts.schet_poket1 = 0;
                ComPorts.schet_poket = 0;
                ZedGraphControl Zedgraph = new ZedGraphControl();
                Zedgraph.Size = zedGraphControl1.Size;
                zedGraphControl1.GraphPane = Zedgraph.GraphPane;
                CreateGraph(Zedgraph);
                zedGraphControl1.Refresh();
                Zedgraph.Dispose();
                textBox4.Text = "0";
                textBox5.Text = "0";
                textBox6.Text = "0";
                textBox7.Text = "0";
                textBox8.Text = "0";
                textBox9.Text = "0";
                textBox10.Text = "0";

                ScanZ.Enabled = false;
                if ("1" == form6.pokaz.Text)
                {
                    Pole_250 = 0;
                    rejim = 0;
                }
                else
                {
                    if ("2" == form6.pokaz.Text)
                    {
                        Pole_250_P = 0;
                        rejim = 20;
                    }
                    else
                    {
                        if ("3" == form6.pokaz.Text)
                        {
                            Pole_5880 = 0;
                            rejim = 40;
                        }
                    }
                }
                myTimer.Start();
                button6.Enabled = false;
                button7.Enabled = false;
                button8.Enabled = false;
                button9.Enabled = false;
                SetScanZero.Enabled = false;
                Pysk.Enabled = false;
            }
            if (Scan_Y_click == 2)
            {
                //ScanZ.Enabled = true;
                Scan_Y_click = 0;
                timer_25 = 0;
            }
        }

        private void Form7_Load(object sender, EventArgs e)
        {
            zagolovok = this.Text;
            tegg = Convert.ToString(this.Tag);
            this.Text = this.Text + " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")";
            textBox8.Text = "0";
            first_click = 0;
            ComPorts.size_Off = 675;
            ComPorts.posilca[0] = 170;
            rejim1 = 0;
            //button3.Visible = false;
            zaxod = true;
            textBox1.Text = "0";
            textBox1.Refresh();
            //textBox2.Text = "0";
            //textBox2.Refresh();
            textBox3.Text = "0";
            textBox3.Refresh();
            textBox4.Text = "0";
            textBox4.Refresh();
            textBox5.Text = "0";
            textBox5.Refresh();
            textBox6.Text = "0";
            textBox6.Refresh();
            toolStripStatusLabel3.Text = "Подождите! Идет настройка программы.";
            textBox7.Text = "0";
            textBox7.Refresh();
            textBox8.Text = "0";
            textBox8.Refresh();
            textBox9.Text = "0";
            textBox9.Refresh();
            textBox10.Text = "0";
            textBox10.Refresh();
            toolStripStatusLabel1.Visible = false;
            toolStripStatusLabel2.Visible = false;
            
            if (("1" == form6.pokaz.Text) || ("2" == form6.pokaz.Text))
                L = 250;
            if ("3" == form6.pokaz.Text)
                L = 5880;

            podschet_1 = 0;
            ComPorts.ComInitializ();
            if (form6.pokaz.Text == "1") // для проверки коллиматора, после раскоментировать
            {
                rejim = 0;
                Begin = true;
            }
            if (form6.pokaz.Text == "2")
            {
                rejim = 20;
                Begin1 = true;
            }
            if (form6.pokaz.Text == "3")
            {
                rejim = 40;
                Begin2 = true;
            }
            myTimer.Tick += new EventHandler(TimerEventProcessor);
            myTimer.Interval = 10;
            myTimer.Start();

            ZedGraphControl Zedgraph = new ZedGraphControl();
            Zedgraph.Size = zedGraphControl1.Size;
            zedGraphControl1.GraphPane = Zedgraph.GraphPane;
            CreateGraph(Zedgraph);
            zedGraphControl1.Refresh();
            Zedgraph.Dispose();
            ScanZ.Enabled = false;// для проверки коллиматора, после раскоментировать
            ScanY.Enabled = false;
            Pysk.Enabled = false;
            SetScanZero.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;
            SetSize();
            //SetScanZero.BackColor = Color.Red;
            XMLW.XmlConfigRead();
            if (form6.pokaz.Text == "1")
            {
                focus = XMLW.Fstart;
            }
            if (form6.pokaz.Text == "2")
            {
                focus = XMLW.Fstart;
            }
            if (form6.pokaz.Text == "3")
            {
                focus = XMLW.Fend;
            }

            trackBar1.Value = 0;
            trackBar2.Value = 0;
            StickX[0] = 0;
            StickX[1] = 0;
            StickX1[0] = 0;
            StickX1[1] = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();






        }

        private void button2_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 32;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            toolStripStatusLabel2.Text = "motor_off";
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
        }

        public void CreateGraph(ZedGraphControl zgc)
        {
            GraphPane myPane = zgc.GraphPane;

            //// Название заголовка
            myPane.Title.FontSpec.Size = 12;
            //myPane.Title.FontSpec.IsUnderline = true;

            //int index = this.Text;//.IndexOf('(',0,this.Text.Length-1);
            myPane.Title.Text = this.Text;//.Insert(index,"\n");
            
            myPane.XAxis.Title.Text = "";
            myPane.YAxis.Title.Text = "";
            myPane.XAxis.Scale.IsVisible = false;
        
            ////Задание допустимых значение на графике 
            //myPane.YAxis.Scale.Min = -1.1;
            //myPane.YAxis.Scale.Max = 1.1;
            myPane.XAxis.Scale.Min = 0;
            //if (endpropise == true)
            //{
            //    myPane.XAxis.Scale.Max = list1.Count + 1;
            //}

            myPane.XAxis.Scale.Max = XMLW.trBarMax; // XMLW.endpropise == true ? (list1.Count+1) : 4200;

            myPane.YAxis.Cross = 0;

            ////отрисовка сетки графика
            myPane.YAxis.MajorGrid.DashOff = 0.15f;
            myPane.XAxis.MajorGrid.DashOff = 0.15f;

            ////велечина интервала сетки по У
        
            //myPane.XAxis.Scale.MajorStep = 100; //из-за этого сильные тормоза


            ////Создание линий
            myPane.AddStick("", StickX, StickY, Color.Chocolate);
            myPane.AddStick("", StickX1, StickY1, Color.Chocolate);

            ////задание цвета сетки
            myPane.XAxis.MajorGrid.Color = Color.LightGray;
            myPane.YAxis.MajorGrid.Color = Color.LightGray;

            ////разрешение отображения сетки
            myPane.XAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MajorGrid.IsVisible = true;

            myPane.AddCurve("Z", list1, Color.Green, SymbolType.None);
            myPane.AddCurve("Y", list2, Color.Blue, SymbolType.None);
            myPane.AddCurve("E", list3, Color.Orange, SymbolType.None);

            double[] CordX = { 0, 4200};
            double[] CordY = { 0.02, 0.02};

            myPane.AddCurve("", CordX, CordY, Color.Brown, SymbolType.None);

            zgc.AxisChange();
        }

        private void очисткаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            list1.Clear();
            list2.Clear();
            list3.Clear();

            //ComPorts.buffer.
            for (int i = 0; i < number; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    ComPorts.buffer[i][j] = 0;
                }
            
            }
            number = 0;
            ComPorts.schet_poket1 = 0;
            ComPorts.schet_poket = 0;
            ZedGraphControl Zedgraph = new ZedGraphControl();
            Zedgraph.Size = zedGraphControl1.Size;
            zedGraphControl1.GraphPane = Zedgraph.GraphPane;
            CreateGraph(Zedgraph);
            zedGraphControl1.Refresh();
            Zedgraph.Dispose();
        }

        private void zedGraphControl1_Paint(object sender, PaintEventArgs e)
        {
            zedGraphControl1.GraphPane.YAxis.Scale.Min = -1.1;
            zedGraphControl1.GraphPane.YAxis.Scale.Max = 1.1;

            zedGraphControl1.GraphPane.YAxis.Scale.MajorStep = 0.1f;

            zedGraphControl1.GraphPane.XAxis.Scale.Min = 0;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            n = Math.Abs(trackBar1.Value - trackBar2.Value);
            if (("1" == form6.pokaz.Text) || ("2" == form6.pokaz.Text))
            {
                rLminus = (n / 4 * XMLW.T * L) / XMLW.Fstart;//XMLW.F;
            }
            if("3" == form6.pokaz.Text)
            {
                rLminus = (n / 4 * XMLW.T * L) / XMLW.Fend;//208.6;//XMLW.F;
            }
            textBox4.Text = Convert.ToString(Math.Round(rLminus,2));
            textBox4.Refresh();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            n = Math.Abs(trackBar1.Value - trackBar2.Value);
            if (("1" == form6.pokaz.Text) || ("2" == form6.pokaz.Text))
            {
                rFminus = (n / 4 * XMLW.T * L) / XMLW.Fstart; //4900;
            }
            if ("3" == form6.pokaz.Text)
            {
                rFminus = (n / 4 * XMLW.T * L) / XMLW.Fend;//208.6; //4900;
            }
            textBox5.Text = Convert.ToString(Math.Round(rFminus,2));
            textBox5.Refresh();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            n = Math.Abs(trackBar1.Value - trackBar2.Value);
            if (("1" == form6.pokaz.Text) || ("2" == form6.pokaz.Text))
            {
                rLplus = (n / 4 * XMLW.T * L) / XMLW.Fstart;
            }
            if ("3" == form6.pokaz.Text) 
            {
                rLplus = (n / 4 * XMLW.T * L) / XMLW.Fend;
            }
            textBox6.Text = Convert.ToString(Math.Round(rLplus,2));
            textBox6.Refresh();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            n = Math.Abs(trackBar1.Value - trackBar2.Value);
            if (("1" == form6.pokaz.Text) || ("2" == form6.pokaz.Text))
            {
                rFplus = (n / 4 * XMLW.T * L) / XMLW.Fstart;
            }
            if ("3" == form6.pokaz.Text)
            {
                rFplus = (n / 4 * XMLW.T * L) / XMLW.Fend; ;
            }

            textBox7.Text = Convert.ToString(Math.Round(rFplus,2));
            textBox7.Refresh();
        }

        private void настройкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ComPorts.port.Close();
        //    Form9 form9 = new Form9();
            form9.form7 = this;
            form9.ShowDialog();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            StickX[0] = trackBar1.Value;
            StickX[1] = trackBar1.Value;

            ZedGraphControl Zedgraph = new ZedGraphControl();
            Zedgraph.Size = zedGraphControl1.Size;
            zedGraphControl1.GraphPane = Zedgraph.GraphPane;
            CreateGraph(Zedgraph);
            zedGraphControl1.Refresh();
            Zedgraph.Dispose();
            try
            {

                textBox3.Text = Convert.ToString(Math.Round(((list3[trackBar1.Value].Y) / obsKof) * 1024, 2));
            }
            catch { }
            try
            {
                n = Math.Abs(trackBar1.Value - trackBar2.Value);
                if (("1" == form6.pokaz.Text) || ("2" == form6.pokaz.Text))
                {
                    rFplus = (n / 4 * XMLW.T * L) / XMLW.Fstart;
                }
                if ("3" == form6.pokaz.Text)
                {
                    rFplus = (n / 4 * XMLW.T * L) / XMLW.Fend; ;
                }
                textBox1.Text = Convert.ToString(Math.Round(rFplus, 2));
            }
            catch { }
            label4.Refresh();
            textBox1.Refresh();
            //textBox2.Refresh();
            textBox3.Refresh();

        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            StickX1[0] = trackBar2.Value;
            StickX1[1] = trackBar2.Value;
            label8.Text = Convert.ToString(trackBar2.Value);
            ZedGraphControl Zedgraph = new ZedGraphControl();
            Zedgraph.Size = zedGraphControl1.Size;
            zedGraphControl1.GraphPane = Zedgraph.GraphPane;
            CreateGraph(Zedgraph);
            zedGraphControl1.Refresh();
            Zedgraph.Dispose();

            try
            {
                //textBox1.Text = Convert.ToString(Math.Round(list1[trackBar2.Value].Y,2));
                //textBox2.Text = Convert.ToString(Math.Round(list2[trackBar2.Value].Y,2));
                textBox3.Text = Convert.ToString(Math.Round(((list3[trackBar1.Value].Y) / obsKof) * 1024, 2));
            }
            catch { }

            try
            {
                n = Math.Abs(trackBar1.Value - trackBar2.Value);
                if (("1" == form6.pokaz.Text) || ("2" == form6.pokaz.Text))
                {
                    rFplus = (n / 4 * XMLW.T * L) / XMLW.Fstart;
                }
                if ("3" == form6.pokaz.Text)
                {
                    rFplus = (n / 4 * XMLW.T * L) / XMLW.Fend; ;
                }
                textBox1.Text = Convert.ToString(Math.Round(rFplus, 2));
            }
            catch { }
            label4.Refresh();
            textBox1.Refresh();
            //textBox2.Refresh();
            textBox3.Refresh();
            ////----------------------------------
            //textBox8.Text = Convert.ToString(Math.Round((((list3[XMLW.SearchPoint(list2, 0, 0, list2.Count)].Y) / (XMLW.V * (Math.PI / 4) * Math.Pow(((XMLW.D * L) / focus), 2) * XMLW.Tay)) * 100), 2)); //под вопросом!

            //textBox9.Text = Convert.ToString(Math.Round(((((list3[XMLW.SearchPoint(list2, 0.8, 0, list2.Count)].Y) / (list3[trackBar1.Value].Y)) * Convert.ToDouble(textBox8.Text)) * 100), 2));

            //textBox10.Text = Convert.ToString(Math.Round(((((list3[XMLW.SearchPoint(list2, 1, 0, list2.Count)].Y) / (list3[trackBar1.Value].Y)) * Convert.ToDouble(textBox8.Text)) * 100), 2));

            //textBox8.Refresh();
            //textBox9.Refresh();
            //textBox10.Refresh();
            //----------------------------------
        }

        private void SetScanZero_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 21;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            toolStripStatusLabel1.Text = " ";
            toolStripStatusLabel2.Text = "SetScanZero";
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
            //SetScanZero.BackColor();

            SetScanZero.BackColor = Color.Tomato;
            podschet = true;
            podschet_1 = 0;
            ScanZ.Enabled = false;
            ScanY.Enabled = false;
            Pysk.Enabled = false;
            SetScanZero.Enabled = false;
            cansel.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;
        }
        
        public void SaveXML()
        {

            XmlTextWriter writer = new XmlTextWriter("xml\\" + form6.NumberProduct + " - " + this.Text + " -" + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")" + ".xml", null);
            writer.WriteStartElement("Sosnay_data_file");
            writer.Formatting = Formatting.Indented;

            writer.WriteStartElement("Sosnay_description");
            writer.WriteElementString("Number", form6.NumberProduct);
            writer.WriteElementString("Type_Check", this.Text); //ScanZ.Checked == true ? "ScanZ":"ScanY");
            writer.WriteElementString("Date", DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString());
            writer.WriteElementString("Time", DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("Sosnay_data");

            for (int i = 0; i < list1.Count; i++) 
            {
                writer.WriteElementString("Y", Convert.ToString(list1[i].Y));
                writer.WriteElementString("Z", Convert.ToString(list2[i].Y));
                writer.WriteElementString("P", Convert.ToString(list3[i].Y));
            }
            writer.WriteEndElement();

            writer.WriteFullEndElement();
            writer.Close();

        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            form5.ShowDialog();
        }

        //private void button3_Click(object sender, EventArgs e)
        //{
        //    //textBox8.Text = Convert.ToString((list3[trackBar1.Value].Y) / (XMLW.V * (Math.PI / 4) * Math.Pow(((XMLW.D * L) / XMLW.F), 2) * XMLW.Tay)); //под вопросом!

        //    //textBox9.Text = Convert.ToString(list3[XMLW.SearchPoint(list3, 0.8)].Y);

        //    //textBox10.Text = Convert.ToString(list3[XMLW.SearchPoint(list3, 1)].Y);
        //    if (form6.pokaz.Text == "1")
        //    {
        //        focus = XMLW.Fstart;
        //    }
        //    if (form6.pokaz.Text == "3")
        //    {
        //        focus = XMLW.Fend;
        //    }
        //  //  button3.BackColor = Color.Red;

            
        //        int a = XMLW.SearchPoint(list1, 0.8, 0, list2.Count);
        //        int b = XMLW.SearchPoint(list1, -0.8, 0, list2.Count);
        //        int c = XMLW.SearchPoint(list1, 0, a, b);

                

        //        textBox8.Text = Convert.ToString(Math.Round((((list3[c].Y) / (XMLW.V * (Math.PI / 4) * Math.Pow(((XMLW.D * L) / focus), 2) * XMLW.Tay)) * 100), 2)); //под вопросом!

        //        //textBox8.Text = Convert.ToString(Math.Round((((list3[trackBar1.Value].Y) / (XMLW.V * (Math.PI / 4) * Math.Pow(((XMLW.D * L) / focus), 2) * XMLW.Tay)) * 100), 2)); //под вопросом!

        //        textBox9.Text = Convert.ToString(Math.Round(((((list3[XMLW.SearchPoint(list1, 0.8, 0, list2.Count)].Y) / (list3[trackBar1.Value].Y)) * Convert.ToDouble(textBox8.Text)) * 100), 2));

        //        textBox10.Text = Convert.ToString(Math.Round(((((list3[XMLW.SearchPoint(list1, 1, 0, list2.Count)].Y) / (list3[trackBar1.Value].Y)) * Convert.ToDouble(textBox8.Text)) * 100), 2));
            
        //    //textBox8.Text = Convert.ToString(Math.Round((((list3[XMLW.SearchPoint(list1, 0, 0, list2.Count)].Y) / (XMLW.V * (Math.PI / 4) * Math.Pow(((XMLW.D * L) / focus), 2) * XMLW.Tay)) * 100), 2)); //под вопросом!

        //    //textBox9.Text = Convert.ToString(Math.Round(((((list3[XMLW.SearchPoint(list2, 0.8, 0, list2.Count)].Y) / (list3[trackBar1.Value].Y)) * Convert.ToDouble(textBox8.Text)) * 100), 2));

        //    //textBox10.Text = Convert.ToString(Math.Round(((((list3[XMLW.SearchPoint(list2, 1, 0, list2.Count)].Y) / (list3[trackBar1.Value].Y)) * Convert.ToDouble(textBox8.Text)) * 100), 2));
        //}

        private void базаДанныхToolStripMenuItem_Click(object sender, EventArgs e)
        {
           // Form3 form3 = new Form3();
            form3 = new Form3();
            form3.XMLW = XMLW;
            form3.ShowDialog();
            list1 = XMLW.list1;
            list2 = XMLW.list2;
            list3 = XMLW.list3;

            int a = XMLW.SearchPoint(proverka == true ? list1 : list2, 0.8, 0, list2.Count);
            int b = XMLW.SearchPoint(proverka == true ? list1 : list2, -0.8, 0, list2.Count);
            int c = XMLW.SearchPoint(proverka == true ? list1 : list2, 0, a, b);
            double obsKof = XMLW.V * XMLW.Tay * 0.25 * (Math.PI) * Math.Pow(((XMLW.D * L / focus)), 2);

            textBox8.Text = Convert.ToString(Math.Round((((list3[c].Y) / obsKof) * 1024), 2)); //под вопросом!
            textBox9.Text = Convert.ToString(Math.Round((((list3[XMLW.SearchPoint(proverka == true ? list1 : list2, 0.8, (int)(number / 2), 0)].Y) / obsKof) * 1024), 2));
            textBox10.Text = Convert.ToString(Math.Round((((list3[XMLW.SearchPoint(proverka == true ? list1 : list2, -0.8, (int)(list2.Count / 2), list2.Count)].Y) / obsKof) * 1024), 2));

            ZedGraphControl Zedgraph = new ZedGraphControl();
            Zedgraph.Size = zedGraphControl1.Size;
            zedGraphControl1.GraphPane = Zedgraph.GraphPane;
            CreateGraph(Zedgraph);
            zedGraphControl1.Refresh();
            Zedgraph.Dispose();

            if (list1.Count > 0)
            {
                trackBar1.Enabled = true;
                trackBar2.Enabled = true;
                trackBar1.Maximum = list1.Count;
                trackBar2.Maximum = list1.Count;

            }
        }

        private void toleranceToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void zedGraphControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (tolerance == true))
            {
                Point p = new Point(e.X, e.Y);
                //double x, y;
                switch (MouseClick)
                {
                    case 0:
                        {
                            zedGraphControl1.GraphPane.ReverseTransform(p, out pointCenter.X, out pointCenter.Y);
                            MouseClick++;
                            break;
                        }
                    case 1:
                        {
                            zedGraphControl1.GraphPane.ReverseTransform(p, out pointMax.X, out pointMax.Y);
                            MouseClick++;
                            ToleranceNew(pointCenter, pointMax);
                            break;
                        }
                    case 2:
                        {
                            zedGraphControl1.GraphPane.ReverseTransform(p, out pointMin.X, out pointMin.Y);
                            MouseClick++;
                            //tolerance = false;
                            ToleranceNew(pointCenter, pointMin);
                            break;
                        }
                    default:
                        break;
                }

            }
            if ((e.Button == MouseButtons.Middle) && (tolerance == true) && (MouseClick>0))
            {
                ZedGraphControl Zedgraph = new ZedGraphControl();
                Zedgraph.Size = zedGraphControl1.Size;
                zedGraphControl1.GraphPane = Zedgraph.GraphPane;
                CreateGraph(Zedgraph);
                zedGraphControl1.Refresh();
                Zedgraph.Dispose();
                if (MouseClick == 3)
                {
                    ToleranceNew(pointCenter, pointMax);
                }
                MouseClick--;
                zedGraphControl1.Refresh();
                Zedgraph.Dispose();
            }

        }

        private void ToleranceNew(PointD nach, PointD kon)
        {
            double[] CordX = { nach.X, kon.X };
            double[] CordY = { nach.Y, kon.Y };
            double x;
            GraphPane myPane;
            myPane = zedGraphControl1.GraphPane;
            if (kon.Y < 0)
            {
                double dx = Math.Abs(kon.X - nach.X);
                double dy = Math.Abs(kon.Y - nach.Y);
                if (nach.Y < 0.03)
                    x = nach.X - Math.Abs(nach.Y) * dx / dy;
                else
                    x = nach.X + Math.Abs(nach.Y) * dx / dy;
                double[] CoordX1 = { (nach.X + Math.Abs(-0.8 - nach.Y) * dx / dy), (nach.X + Math.Abs(-0.6 - nach.Y) * dx / dy),
                                     (nach.X + Math.Abs(-0.6 - nach.Y) * dx / dy), (nach.X + Math.Abs(-0.3 - nach.Y) * dx / dy),
                                     (nach.X + Math.Abs(-0.3 - nach.Y) * dx / dy), x};
                double[] CoordY1 = { -0.7, -0.5, -0.55, -0.25, -0.27, 0.03 };
                double[] CoordY2 = { -0.9, -0.7, -0.65, -0.35, -0.33, -0.03 };

                myPane.AddCurve("", CordX, CordY, Color.Black, SymbolType.None);
                myPane.AddCurve("", CoordX1, CoordY1, Color.Black, SymbolType.None);
                myPane.AddCurve("", CoordX1, CoordY2, Color.Black, SymbolType.None);
 /*               //Проверка от 0 до -0.3
                for (int i = Convert.ToInt32(Math.Ceiling(x)); i <= Math.Floor((nach.X + Math.Abs(-0.3 - nach.Y) * dx / dy)); i++)
                {
                    double b = proverka == true ? list1.ElementAt(i).Y : list2.ElementAt(i).Y;
                    double c = (nach.Y - Math.Abs(i - nach.X) * dy / dx) + 0.03;
                    double c1 = (nach.Y - Math.Abs(i - nach.X) * dy / dx) - 0.03;
                    if ((b > c) || (b < c1))
                    {
                        MessageBox.Show("Error");
                        break;
                    }
                }
                //Проверка от -0.3 до -0.6
                for (int i = Convert.ToInt32(Math.Ceiling((nach.X + Math.Abs(-0.3 - nach.Y) * dx / dy))); i <= Math.Floor((nach.X + Math.Abs(-0.6 - nach.Y) * dx / dy)); i++)
                {
                    double b = proverka == true ? list1.ElementAt(i).Y : list2.ElementAt(i).Y;
                    double c = (nach.Y - Math.Abs(i - nach.X) * dy / dx) + 0.05;
                    double c1 = (nach.Y - Math.Abs(i - nach.X) * dy / dx) - 0.05;
                    if ((b > c) || (b < c1))
                    {
                        MessageBox.Show("Error1");
                        break;
                    }
                }
                //Проверка от -0.6 до -0.8
                for (int i = Convert.ToInt32(Math.Ceiling((nach.X + Math.Abs(-0.6 - nach.Y) * dx / dy))); i <= Math.Floor((nach.X + Math.Abs(-0.8 - nach.Y) * dx / dy)); i++)
                {
                    double b = proverka == true ? list1.ElementAt(i).Y : list2.ElementAt(i).Y;
                    double c = (nach.Y - Math.Abs(i - nach.X) * dy / dx) + 0.1;
                    double c1 = (nach.Y - Math.Abs(i - nach.X) * dy / dx) - 0.1;
                    if ((b > c) || (b < c1))
                    {
                        MessageBox.Show("Error2");
                        break;
                    }
                }
*/            }
            else
            {
                double dx = Math.Abs(nach.X - kon.X);
                double dy = Math.Abs(nach.Y - kon.Y);
                if (nach.Y < 0.03)
                    x = nach.X - Math.Abs(nach.Y) * dx / dy;
                else
                    x = nach.X + Math.Abs(nach.Y) * dx / dy;
                double[] CoordX1 = { (nach.X - Math.Abs(0.8 - nach.Y) * dx / dy), (nach.X - Math.Abs(0.6 - nach.Y) * dx / dy),
                                     (nach.X - Math.Abs(0.6 - nach.Y) * dx / dy), (nach.X - Math.Abs(0.3 - nach.Y) * dx / dy),
                                     (nach.X - Math.Abs(0.3 - nach.Y) * dx / dy), x};
                double[] CoordY1 = { 0.7, 0.5, 0.55, 0.25, 0.27, -0.03 };
                double[] CoordY2 = { 0.9, 0.7, 0.65, 0.35, 0.33, 0.03 };

                myPane.AddCurve("", CordX, CordY, Color.Black, SymbolType.None);
                myPane.AddCurve("", CoordX1, CoordY1, Color.Black, SymbolType.None);
                myPane.AddCurve("", CoordX1, CoordY2, Color.Black, SymbolType.None);
                //Проверка от 0 до 0.3
/*                for (int i = Convert.ToInt32(Math.Ceiling(x)); i >= Math.Floor((nach.X - Math.Abs(0.3 - nach.Y) * dx / dy)); i--)
                {
                    double b = proverka == true ? list1.ElementAt(i).Y : list2.ElementAt(i).Y;
                    double c = (nach.Y + Math.Abs(i - nach.X) * dy / dx) + 0.03;
                    double c1 = (nach.Y + Math.Abs(i - nach.X) * dy / dx) - 0.03;
                    if ((b < c) || (b > c1))
                    {
                        MessageBox.Show("Error3");
                        break;
                    }
                }
                //Проверка от 0.3 до 0.6
                for (int i = Convert.ToInt32(Math.Ceiling((nach.X - Math.Abs(0.3 - nach.Y) * dx / dy))); i >= Math.Floor((nach.X - Math.Abs(0.6 - nach.Y) * dx / dy)); i--)
                {
                    double b = proverka == true ? list1.ElementAt(i).Y : list2.ElementAt(i).Y;
                    double c = (nach.Y + Math.Abs(i - nach.X) * dy / dx) + 0.05;
                    double c1 = (nach.Y + Math.Abs(i - nach.X) * dy / dx) - 0.05;
                    if ((b > c) || (b < c1))
                    {
                        MessageBox.Show("Error4");
                        break;
                    }
                }
                //Проверка от 0.6 до 0.8
                for (int i = Convert.ToInt32(Math.Ceiling((nach.X - Math.Abs(0.6 - nach.Y) * dx / dy))); i >= Math.Floor((nach.X - Math.Abs(0.8 - nach.Y) * dx / dy)); i--)
                {
                    double b = proverka == true ? list1.ElementAt(i).Y : list2.ElementAt(i).Y;
                    double c = (nach.Y + Math.Abs(i - nach.X) * dy / dx) + 0.1;
                    double c1 = (nach.Y + Math.Abs(i - nach.X) * dy / dx) - 0.1;
                    if ((b > c) || (b < c1))
                    {
                        MessageBox.Show("Error5");
                        break;
                    }
                }
*/
            }
            zedGraphControl1.GraphPane = myPane;
            zedGraphControl1.Refresh();

        }

        private void стопToolStripMenuItem_Click(object sender, EventArgs e)
        {
            myTimer.Stop(); //останавливаем таймер
        }

        private void сохранитьРисунокToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void добавитьВБДToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveXML();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

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
       
        public void canselPr()
        { 
            ScanZ.Enabled = false;
            ScanY.Enabled = false;
            if (zaxod == true)
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

                ComPorts.posilca[1] = 32;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "MotorScanOff";
                ComPorts.pokets.poket1 = 0;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();
                zaxod = false;
            }
            else
            {
                timer_25 = 0;
                ScanZ.Checked = false;
                ScanY.Checked = false;

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
                System.Threading.Thread.Sleep(10);

                ComPorts.posilca[1] = 32;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "MotorScanOff";
                ComPorts.pokets.poket1 = 0;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();
                System.Threading.Thread.Sleep(500);

                ComPorts.posilca[1] = 26;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "ADC_Off";
                ComPorts.pokets.poket1 = 0;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();
                System.Threading.Thread.Sleep(300);
                if (("1" == form6.pokaz.Text) || ("2" == form6.pokaz.Text))
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
                    System.Threading.Thread.Sleep(1000);
                }
                ComPorts.posilca[1] = 21;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "SetScanZero";
                ComPorts.pokets.poket1 = 0;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                SetScanZero.BackColor = Color.Tomato;
                ComPorts.Poket_Write();

                button6.Enabled = true;
                button7.Enabled = true;
                button8.Enabled = true;
                button9.Enabled = true;
                
                podschet_1 = 0;
                podschet = true;
                ScanZ.Enabled = true;
                ScanY.Enabled = true;
                
            }
           
            cansel.Enabled = false;
        
        }
        
        private void cansel_Click(object sender, EventArgs e)
        {
            ScanZ.Enabled = false;
            ScanY.Enabled = false;
            if (zaxod == true)
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

                ComPorts.posilca[1] = 32;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "MotorScanOff";
                ComPorts.pokets.poket1 = 0;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();
                zaxod = false;
            }
            else
            {
                timer_25 = 0;
                ScanZ.Checked = false;
                ScanY.Checked = false;

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
                System.Threading.Thread.Sleep(10);

                ComPorts.posilca[1] = 32;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "MotorScanOff";
                ComPorts.pokets.poket1 = 0;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();
                System.Threading.Thread.Sleep(500);

                ComPorts.posilca[1] = 26;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "ADC_Off";
                ComPorts.pokets.poket1 = 0;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();
                System.Threading.Thread.Sleep(300);
                if (("1" == form6.pokaz.Text) || ("2" == form6.pokaz.Text))
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
                    System.Threading.Thread.Sleep(1000);
                }
                ComPorts.posilca[1] = 21;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "SetScanZero";
                ComPorts.pokets.poket1 = 0;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                SetScanZero.BackColor = Color.Tomato;
                ComPorts.Poket_Write();

                
                podschet_1 = 0;
                podschet = true;
                
            }
           
            cansel.Enabled = false;
            //timer_25 = 0;
        }

        private void допусковаяToolStripMenuItem_Click(object sender, EventArgs e)
        {

            MouseClick = 0;
            tolerance = true;
            if (допусковаяToolStripMenuItem.BackColor == Color.LightGreen)
            {
                допусковаяToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                tolerance = false;
                ZedGraphControl Zedgraph = new ZedGraphControl();
                Zedgraph.Size = zedGraphControl1.Size;
                zedGraphControl1.GraphPane = Zedgraph.GraphPane;
                CreateGraph(Zedgraph);
                zedGraphControl1.Refresh();
                Zedgraph.Dispose();
            }
            else
                допусковаяToolStripMenuItem.BackColor = Color.LightGreen;
            myTimer.Stop();

        }

        private void добавитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveXML();
            XMLW.SaveImage(this);
            //MessageBox.Show("Сохранено");
            добавитьToolStripMenuItem.BackColor = Color.LightGreen;
            добавитьToolStripMenuItem.Text = "Результат сохранен";
        }

        private void выходToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void сохранитьРисунокToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            XMLW.SavePictureBD(this);
        }

        private void добавитьВОтчетToolStripMenuItem_Click(object sender, EventArgs e)
        {
            добавитьВОтчетToolStripMenuItem.BackColor = Color.LightGreen;
            добавитьВОтчетToolStripMenuItem.Text = "Добавлено в отчет";
            if ("3" == form6.pokaz.Text)
            {
                if (z_y == 1)
                {
                    form6.SetValue("D17", textBox6.Text);
                    form6.SetValue("D18", textBox4.Text);
                    form6.SetValue("D19", textBox7.Text);
                    form6.SetValue("D20", textBox5.Text);

                    form6.SetValue("D22", textBox8.Text);
                    form6.SetValue("D23", textBox10.Text);
                    form6.SetValue("D24", textBox9.Text);
                }
                else
                {
                    if (z_y == 2)
                    {
                        form6.SetValue("E17", textBox6.Text);
                        form6.SetValue("E18", textBox4.Text);
                        form6.SetValue("E19", textBox7.Text);
                        form6.SetValue("E20", textBox5.Text);

                        form6.SetValue("E22", textBox8.Text);
                        form6.SetValue("E23", textBox10.Text);
                        form6.SetValue("E24", textBox9.Text);
                    }
                }
            }
            if ("2" == form6.pokaz.Text)
            {
                if (z_y == 1)
                {
                    form6.SetValue("D43", textBox6.Text);
                    form6.SetValue("D44", textBox4.Text);
                    form6.SetValue("D45", textBox7.Text);
                    form6.SetValue("D46", textBox5.Text);

                    form6.SetValue("D48", textBox8.Text);
                    form6.SetValue("D49", textBox10.Text);
                    form6.SetValue("D50", textBox9.Text);
                }
                else
                {
                    if (z_y == 2)
                    {
                        form6.SetValue("E43", textBox6.Text);
                        form6.SetValue("E44", textBox4.Text);
                        form6.SetValue("E45", textBox7.Text);
                        form6.SetValue("E46", textBox5.Text);

                        form6.SetValue("E48", textBox8.Text);
                        form6.SetValue("E49", textBox10.Text);
                        form6.SetValue("E50", textBox9.Text);
                    }
                }
            }
            if ("1" == form6.pokaz.Text)
            {
                if (z_y == 1)
                {
                    form6.SetValue("D30", textBox6.Text);
                    form6.SetValue("D31", textBox4.Text);
                    form6.SetValue("D32", textBox7.Text);
                    form6.SetValue("D33", textBox5.Text);

                    form6.SetValue("D35", textBox8.Text);
                    form6.SetValue("D36", textBox10.Text);
                    form6.SetValue("D37", textBox9.Text);
                }
                else
                {
                    if (z_y == 2)
                    {
                        form6.SetValue("E30", textBox6.Text);
                        form6.SetValue("E31", textBox4.Text);
                        form6.SetValue("E32", textBox7.Text);
                        form6.SetValue("E33", textBox5.Text);

                        form6.SetValue("E35", textBox8.Text);
                        form6.SetValue("E36", textBox10.Text);
                        form6.SetValue("E37", textBox9.Text);
                    }
                }
            }
        }

    }
}
