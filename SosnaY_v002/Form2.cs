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
    public partial class Form2 : Form
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
            public byte[] priem = new byte[675];// буфер на прием
            public byte[] priem_nedoch = new byte[2007];//прием недочитанного
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
                        // razm_poket = port.ReadBufferSize - number;
                        //Poket_Read();
                        #region чтение из АЦП
                        /*   for (h = 0; h < (ReadBytes1/9); h++)
                        {
                            buffer[h] = new int[size_Off];
                            for (j = 0; j < ReadBytes1; j += 9)
                            {
                                buffer[schet_poket][j] = priem[j];
                                buffer[schet_poket][j + 1] = priem[j + 1];
                                buffer[schet_poket][j + 2] = priem[j + 2];
                                buffer[schet_poket][j + 3] = priem[j + 3];
                                buffer[schet_poket][j + 4] = priem[j + 4];
                                buffer[schet_poket][j + 5] = priem[j + 5];
                                buffer[schet_poket][j + 6] = priem[j + 6];
                                buffer[schet_poket][j + 7] = priem[j + 7];
                                buffer[schet_poket][j + 8] = priem[j + 8];
                            }
                            schet_poket++;
                        }*/
                        #endregion
                    }
                    port.Write(posilca, 0, size_On);//запись пакета
                }
                catch { }
            }

            public void Poket_Read()
            {
                try
                {
                    #region чтение из АЦП
                    ReadBytes = port.Read(priem_nedoch, 0, 2007);
                    /* for (int i = 0; i < 2007; i = i + 9 )
                     {
                         if (priem[i+1] == 104)
                         {
                             kontr_sum = (byte)(priem_nedoch[i] ^ priem[i + 1] ^ priem[i + 2] ^ priem[i + 3] ^ priem[i + 4] ^ priem[i + 5] ^ priem[i + 6] ^ priem[i + 7]);
                             if (kontr_sum == priem[8])
                             {
                                 buffer[schet_poket] = new short[9];
                                 buffer[schet_poket][0] = priem_nedoch[i];
                                 buffer[schet_poket][1] = priem_nedoch[i+1];
                                 buffer[schet_poket][2] = priem_nedoch[i + 2];
                                 buffer[schet_poket][3] = priem_nedoch[i + 3];
                                 buffer[schet_poket][4] = priem_nedoch[i + 4];
                                 buffer[schet_poket][5] = priem_nedoch[i + 5];
                                 buffer[schet_poket][6] = priem_nedoch[i + 6];
                                 buffer[schet_poket][7] = priem_nedoch[i + 7];
                                 buffer[schet_poket][8] = priem_nedoch[i + 8];
                                 schet_poket++;
                             }
                             else
                             {
                                 sboy_poket++;
                             }
                         }
                     }*/
                    #endregion
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
                    if (posilca[1] == 26)
                    {
                        posilca[10] = 0;
                    }

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
                                        if (schet_poket > 30)
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
                                            One_Buffers = false;
                                            number++;
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

        public Form1 form1;
        public Form3 form3;
        //public Form6 form6_ = new Form6();
        public Form6 form6;
        public Form10 form10;
        public Form16 form16;
        Form4 form4 = new Form4();
        Form5 form5 = new Form5();
       
        public string zagolovok;
        public xmlWork XMLW = new xmlWork();

        public PointPairList list1 = new PointPairList();
        public PointPairList list2 = new PointPairList();
        public PointPairList list3 = new PointPairList();
        public PointPairList list4 = new PointPairList();

        private double maxout, max2out;
        public bool tolerance = false;
        public int MouseClick = 0;
        PointD pointMin, pointCenter, pointMax;
        public double kor1, kor2, obsKof;

        public int pointS, pointS1, pointS2, pointS3, pointS4, pointS5;
        public double rastor, Kor_ZY;
        public bool Product_Kontrol, Product_Ustirovka, Klimat_Ispitaniy, Mexanicheskiy_Ispitaniy;
        public string namePort;
        public byte Command, creck;
        public byte rejim, product_izdel, product_ustirov, mexan_ispit, klimat_ispit, size, two, begins_to, rejims;
        public SosnaY_v00.Form2.ComPort ComPorts = new ComPort();
        public double Kor_Z, Kor_Y, Kor_P, sum;
        public short Kor_Z1, Kor_Y1;
        public short time_muve_pankr, time_begin_sniatia_prevish, time_sniatia_prevish, time_vozvrata;
        public byte[] In_buffer = new byte[13];
        public byte[] Out_buffer = new byte[16];
        public double[] ReadBuffers = new double[55000];
        public int[] ReadBuffers1 = new int[10];
        public bool thReadPoket, Begins;
        static System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();
        public int TimeInterval, TimerOn;
        public long number, number_dorisovka, l, timer_25;
        public byte[] GetStatus_Byte1 = new byte[8];
        public byte[] GetStatus_Byte2 = new byte[8];
        public byte rejim2, Pricel, first_click, first_click2;
        public bool PricelBefore;
        public double[] StickX = { 0, 0 }; //начальные координады салазки 
        public double[] StickY = { -1.1, 1.1 };//начальные координады салазки

        public double[] StickX1 = { 0, 0 };//начальные координады салазки
        public double[] StickY1 = { -1.1, 1.1 };//начальные координады салазки

        public bool dopyskZona = false;
        public bool dopyskZona1 = false;

        public string TypeCheck;
        public double min, min1, min2, min3, max, max1, max2, max3, maxZY, maxZY1;
        public bool endpropise;

        public double[] bufferZ = new double[4196];
        public double[] bufferY = new double[4196];
        public double[] bufferZY = new double[4196];
        public double[] bufferP = new double[4196];

        public int koff;

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

        //Усреднение
        public double[] Buffer_Z = new double[2000000];
        public double[] Buffer_Y = new double[2000000];
        public double[] Buffer_P = new double[2000000];
        public double[] Buffer_ZY = new double[2000000];

        //Счетчик позиции
        public int Counter;
        //Порог срабатывания (0.05)
        public double ThresholdTrigger;// = 0.05D;//0.05D;
        //Размер растра {количество средних значений}(100)
        public int RastSize;// = 100;
        //Время начала (в посылках)
        public int StartTime;// = 1796;

        #endregion

        public Form2()
        {
            InitializeComponent();

            ZedGraphControl Zedgraph = new ZedGraphControl();
            Zedgraph.Size = zedGraphControl1.Size;
            zedGraphControl1.GraphPane = Zedgraph.GraphPane;
            CreateGraph(Zedgraph);
            zedGraphControl1.Refresh();
            Zedgraph.Dispose();

            ComPorts.Kol_poket = false;
            thReadPoket = true;
            ComPorts.size_Off = 3;
            ComPorts.schet_poket = 0;
            ComPorts.razm_poket = 0;
            ComPorts.schet_poket1 = 0;
            ComPorts.sboy_poket = 0;
            ComPorts.posilca[0] = 170;
            ComPorts.h = 0;
            ComPorts.j = 0;
            TimerOn = 0;
            number = 10;
            two = 0;



            number = 0;
            trackBar1.Enabled = false;
            trackBar2.Enabled = false;

            //numericUpDown1.Enabled = false;
            //numericUpDown2.Enabled = false;
            //numericUpDown3.Enabled = false;
            //numericUpDown4.Enabled = false;
            //numericUpDown5.Enabled = false;
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {


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
            //myTimer.Stop();
            //ComPorts.port.Close();

            //form4.Close();
            //form10.Show();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            FilterConfigRead();

            zagolovok = this.Text;
            this.Text = zagolovok + " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")";
            SetSize();
            button3.Visible = false;
            ComPorts.ComInitializ();
            toolStripProgressBar1.Visible = false;
            toolStripStatusLabel1.Visible = false;
            toolStripStatusLabel2.Visible = false;
            ZedGraphControl Zedgraph = new ZedGraphControl();
            Zedgraph.Size = zedGraphControl1.Size;
            zedGraphControl1.GraphPane = Zedgraph.GraphPane;
            CreateGraph(Zedgraph);
            zedGraphControl1.Refresh();
            Zedgraph.Dispose();
            myTimer.Tick += new EventHandler(TimerEventProcessor);
            myTimer.Interval = 5;
            myTimer.Start();
            begins_to = 0;
            Begins = true;
            rejims = 0;
            trackBar1.Value = 0;
            trackBar2.Value = 0;
            StickX[0] = 0;
            StickX[1] = 0;
            StickX1[0] = 0;
            StickX1[1] = 0;
            Product_Kontrol = false;
            Mexanicheskiy_Ispitaniy = false;
            Klimat_Ispitaniy = false;
            XMLW.XmlConfigRead();
            button1.Enabled = false;
            Pysk.Enabled = false;
            PricelBefore = false;
            if (form10.label1.Text != "4")
            {
                усреднитьToolStripMenuItem.Visible = false;
            }
            if (form10.label1.Text == "5")
            {
                усреднитьToolStripMenuItem.Visible = true;
            }
        }

        private void Form2_Resize(object sender, EventArgs e)
        {
            SetSize();
        }

        private void SetSize()
        {
            try
            {
                splitContainer1.Size = new Size(ClientRectangle.Width, ClientRectangle.Height);
                splitContainer1.SplitterDistance = ClientRectangle.Width - 190;
                //zedGraphControl1.Size = new Size(splitContainer1.Panel1.Width - 10, splitContainer1.Panel1.Height - 155);
                zedGraphControl1.Size = new Size(splitContainer1.Panel1.Width - 10, splitContainer1.Panel1.Height - 110);
                trackBar1.Top = zedGraphControl1.Bottom;
                trackBar2.Top = trackBar1.Bottom;

                string res = Convert.ToString(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width);

                if (res == "1152")
                {
                    trackBar1.Size = new Size(zedGraphControl1.Width - 85, trackBar1.Height);
                    trackBar2.Size = new Size(zedGraphControl1.Width - 85, trackBar2.Height);

                    trackBar1.Left = zedGraphControl1.Left + 65;
                    trackBar2.Left = zedGraphControl1.Left + 65;
                }
                if (res == "1024")
                {
                    trackBar1.Size = new Size(zedGraphControl1.Width - 75, trackBar1.Height);
                    trackBar2.Size = new Size(zedGraphControl1.Width - 75, trackBar2.Height);

                    trackBar1.Left = zedGraphControl1.Left + 55;
                    trackBar2.Left = zedGraphControl1.Left + 55;
                }
                if (res == "1280")
                {
                    trackBar1.Size = new Size(zedGraphControl1.Width - 105, trackBar1.Height);
                    trackBar2.Size = new Size(zedGraphControl1.Width - 105, trackBar2.Height);

                    trackBar1.Left = zedGraphControl1.Left + 80;
                    trackBar2.Left = zedGraphControl1.Left + 80;
                }


                label24.Top = trackBar2.Bottom + 10;

                groupBox1.Top = splitContainer1.Panel2.Bottom - 750;

                groupBox3.Top = splitContainer1.Panel2.Bottom - 135;

                groupBox7.Top = splitContainer1.Panel2.Bottom - 463;

                groupBox2.Top = splitContainer1.Panel2.Bottom - 600;

                button4.Top = splitContainer1.Panel2.Bottom - 930;


                textBox4.Top = splitContainer1.Panel2.Bottom - 280;
                textBox4.Height = splitContainer1.Panel2.Bottom - 860;
                //if (textBox4.Height < 20)
                //{
                //    textBox4.Visible = false;
                //    textBox4.Enabled = false;
                //}
            }
            catch
            {

            }
        }

        public void CreateGraph(ZedGraphControl zgc)
        {
            GraphPane myPane = zgc.GraphPane;

            //// Название заголовка
            /// 
            myPane.Title.FontSpec.Size = 12;
            myPane.Title.FontSpec.IsUnderline = false;
            myPane.Title.Text = this.Text;
            myPane.XAxis.Title.Text = "";
            myPane.YAxis.Title.Text = "";

            ////Задание допустимых значение на графике 
            myPane.YAxis.Scale.Min = -1.1;
            myPane.YAxis.Scale.Max = 1.1;
            myPane.XAxis.Scale.Min = 0;

            myPane.XAxis.Scale.Max = XMLW.trBarMax; //XMLW.endpropise == true ? (list1.Count + 1) : 4200;

            myPane.YAxis.Cross = 0;

            ////отрисовка сетки графика
            myPane.YAxis.MajorGrid.DashOff = 0.15f;
            myPane.XAxis.MajorGrid.DashOff = 0.15f;

            ////велечина интервала сетки по У
            //myPane.YAxis.Scale.MajorStep = 0.1f; //из-за этого сильные тормоза


            ////Создание линий
            myPane.AddStick("", StickX, StickY, Color.Chocolate);
            myPane.AddStick("", StickX1, StickY1, Color.Chocolate);

            ////задание цвета сетки
            myPane.XAxis.MajorGrid.Color = Color.LightGray;
            myPane.YAxis.MajorGrid.Color = Color.LightGray;

            ////разрешение отображения сетки
            myPane.XAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MajorGrid.IsVisible = true;

            if (dopyskZona == true)
            {
                double[] CordX = { 0, 2100, 2100, 3200 };
                double[] CordY = { 0.15, 0.15, 0.21, 0.21 };
                double[] CordY1 = { -0.15, -0.15, -0.21, -0.21 };

                myPane.AddCurve("", CordX, CordY, Color.Brown, SymbolType.None);
                myPane.AddCurve("", CordX, CordY1, Color.Brown, SymbolType.None);
            }
            else
            {
                double[] CordX = { 0, 3600 };
                double[] CordX1 = { 2100, 3600 };
                double[] CordY = { 0.18, 0.18 };
                double[] CordY1 = { -0.18, -0.18 };
                double[] CordY2 = { 0.22, 0.22 };
                double[] CordY3 = { -0.22, -0.22 };

                myPane.AddCurve("", CordX, CordY, Color.Brown, SymbolType.None);
                myPane.AddCurve("", CordX, CordY1, Color.Brown, SymbolType.None);

                myPane.AddCurve("", CordX, CordY2, Color.Brown, SymbolType.None);
                myPane.AddCurve("", CordX, CordY3, Color.Brown, SymbolType.None);

            }


            //else
            //{
            //    double[] CordX = { 0, 4000, 4000, 5880 };
            //    double[] CordY = { 0.15, 0.15, 0.21, 0.21 };
            //    double[] CordY1 = { -0.15, -0.15, -0.21, -0.21 };

            //    myPane.AddCurve("", CordX, CordY, Color.Brown, SymbolType.None);
            //    myPane.AddCurve("", CordX, CordY1, Color.Brown, SymbolType.None);
            //}

            //---------------------------------------------------------------------------------
            // Проверка значение +У на положительность
            PointPairList tlist1 = new PointPairList();

            //for (int i = 0; i < list1.Count; i++)
            //{
            //    if (ComPorts.posilca[1] == 37)
            //    {
            //        if (list1.ElementAt(i).Y > 0)
            //            tlist1.Add(i, list1.ElementAt(i).Y + Convert.ToDouble((numericUpDown1.Value - 5) / 5));
            //        else
            //            if (list1.ElementAt(i).Y < 0)
            //                tlist1.Add(i, list1.ElementAt(i).Y + (Convert.ToDouble((numericUpDown3.Value) + 5) / 5));
            //            else
            //                if (list1.ElementAt(i).Y == 0)
            //                    tlist1.Add(i, list1.ElementAt(i).Y);
            //    }
            //    else
            //    {
            //        if (ComPorts.posilca[1] == 38)
            //        {
            //            if (list1.ElementAt(i).Y > 0)
            //                tlist1.Add(i, list1.ElementAt(i).Y + Convert.ToDouble((numericUpDown1.Value + 5) / 5));
            //            else
            //                if (list1.ElementAt(i).Y < 0)
            //                    tlist1.Add(i, list1.ElementAt(i).Y + (Convert.ToDouble((numericUpDown3.Value) - 5) / 5));
            //                else
            //                    if (list1.ElementAt(i).Y == 0)
            //                        tlist1.Add(i, list1.ElementAt(i).Y);
            //        }
            //        else
            //        {
            //            if (list1.ElementAt(i).Y > 0)
            //                tlist1.Add(i, list1.ElementAt(i).Y + Convert.ToDouble((numericUpDown1.Value - 5) / 5));
            //            else
            //                if (list1.ElementAt(i).Y < 0)
            //                    tlist1.Add(i, list1.ElementAt(i).Y + (Convert.ToDouble((numericUpDown3.Value) - 5) / 5));
            //                else
            //                    if (list1.ElementAt(i).Y == 0)
            //                        tlist1.Add(i, list1.ElementAt(i).Y);
            //        }
            //    }
            //}

            //---------------------------------------------------------------------------------
            // Проверка значение +Z на положительность
            PointPairList tlist2 = new PointPairList();
            //for (int i = 0; i < list2.Count; i++)
            //{
            //    if (ComPorts.posilca[1] == 37)
            //    {
            //        if (list2.ElementAt(i).Y > 0)
            //            tlist2.Add(i, list2.ElementAt(i).Y + Convert.ToDouble((numericUpDown2.Value + 5) / 5));
            //        else
            //            if (list2.ElementAt(i).Y < 0)
            //                tlist2.Add(i, list2.ElementAt(i).Y + Convert.ToDouble((numericUpDown4.Value - 5) / 5));
            //            else
            //                if (list2.ElementAt(i).Y == 0)
            //                    tlist2.Add(i, list2.ElementAt(i).Y);
            //    }
            //    else
            //    {
            //        if (ComPorts.posilca[1] == 38)
            //        {
            //            if (list2.ElementAt(i).Y > 0)
            //                tlist2.Add(i, list2.ElementAt(i).Y + Convert.ToDouble((numericUpDown2.Value - 5) / 5));
            //            else
            //                if (list2.ElementAt(i).Y < 0)
            //                    tlist2.Add(i, list2.ElementAt(i).Y + Convert.ToDouble((numericUpDown4.Value + 5) / 5));
            //                else
            //                    if (list2.ElementAt(i).Y == 0)
            //                        tlist2.Add(i, list2.ElementAt(i).Y);
            //        }
            //        else
            //        {
            //            if (list2.ElementAt(i).Y > 0)
            //                tlist2.Add(i, list2.ElementAt(i).Y + Convert.ToDouble((numericUpDown2.Value - 5) / 5));
            //            else
            //                if (list2.ElementAt(i).Y < 0)
            //                    tlist2.Add(i, list2.ElementAt(i).Y + Convert.ToDouble((numericUpDown4.Value - 5) / 5));
            //                else
            //                    if (list2.ElementAt(i).Y == 0)
            //                        tlist2.Add(i, list2.ElementAt(i).Y);
            //        }
            //    }
            //}

            //---------------------------------------------------------------------------------
            // Проверка значение P на положительность
            PointPairList tlist3 = new PointPairList();

            //for (int i = 0; i < list3.Count; i++)
            //{
            //    if (list3.ElementAt(i).Y > 0)
            //        tlist3.Add(i, list3.ElementAt(i).Y - Convert.ToDouble(numericUpDown5.Value / 5));
            //    else
            //        tlist3.Add(i, list3.ElementAt(i).Y);
            //}

            //PointPairList tlist4 = new PointPairList();
            //double sum = 0;
            //for (int i = 0; i < list3.Count; i+=5)
            //{
            //    sum = 0;
            //    try
            //    {
            //        for (int j = i; j < i + 10; j++)
            //            sum += list3.ElementAt(j).Y;
            //        sum /= 10;
            //    }
            //    catch { }
            //    tlist4.Add(i, sum);
            //}


            //----------------------------------------------------------------------------------------
            //Создание 3 линий для графика

            //  LineItem myCurve = myPane.AddCurve("Y", tlist1, Color.Red, SymbolType.None);

            myPane.AddCurve("ZY", list4, Color.Black, SymbolType.None);
            myPane.AddCurve("Z", list1, Color.LimeGreen, SymbolType.None);
            myPane.AddCurve("Y", list2, Color.RoyalBlue, SymbolType.None);
            myPane.AddCurve("E", list3, Color.Orange, SymbolType.None);


            zgc.AxisChange();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            form4.TopMost = true;
            form4.Show();
        }

        public void FilterConfigRead()
        {
            try
            {
                XmlTextReader reader = new XmlTextReader("filterconfig.xml");
                reader.Read();
                if (reader.IsStartElement("Filter_config"))
                {
                    reader.ReadStartElement("Filter_config");
                    while (reader.IsStartElement("Filter_description"))
                    {
                        reader.ReadStartElement("Filter_description");

                        ThresholdTrigger = Convert.ToDouble(reader.ReadElementString("ThresholdTrigger"));
                        RastSize = Convert.ToInt32(reader.ReadElementString("RastSize"));
                        StartTime = Convert.ToInt32(reader.ReadElementString("StartTime"));

                        reader.ReadEndElement();
                    }
                }
                reader.Close();
            }
            catch (FileNotFoundException)
            {
                ThresholdTrigger = 0.05D;
                StartTime = 1796;
                RastSize = 100;
            }

        }

        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            myTimer.Enabled = true;

            if ((Pysk.Checked == true))
            {
                #region Прицеливание
                switch (rejim2)
                {
                    //   case 0:
                    //      if (Pricel == 0)
                    //   {
                    //      ComPorts.posilca[1] = 14;
                    //    ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                    //  ComPorts.size_Off = 3;
                    //           ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                    //           ComPorts.size_On = 3;
                    //         label24.Text = "Выключение блокировки ПС";
                    //       toolStripStatusLabel2.Text = "Off_DP_Off";
                    //     ComPorts.pokets.poket1 = 0;
                    //   ComPorts.port.DiscardInBuffer();
                    //                  ComPorts.port.DiscardOutBuffer();
                    //                ComPorts.Kol_poket = false;
                    //              ComPorts.Poket_Write();
                    //            System.Threading.Thread.Sleep(300);
                    //            Pricel = 1;
                    //          }
                    //          break;
                    case 0:
                        if (Pricel == 0)
                        {
                            if (PricelBefore == false)
                            {
                                ComPorts.posilca[1] = 19;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.size_On = 3;
                                label24.Text = "Перевод ПС в конечное положение";
                                toolStripStatusLabel2.Text = "Pankr_End_On";
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.Kol_poket = false;
                                ComPorts.Poket_Write();

                            }
                            else
                                rejim2++;
                            Pricel = 1;
                        }
                        break;

                    //    case 2:
                    //         if (Pricel == 2)
                    //         {
                    //             ComPorts.posilca[1] = 15;
                    //             ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                    //             ComPorts.size_Off = 3;
                    //             ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                    //             ComPorts.size_On = 3;
                    //             label24.Text = "Открытие шторки";
                    //            toolStripStatusLabel2.Text = "Shtorka_On";
                    //              ComPorts.pokets.poket1 = 0;
                    //             ComPorts.port.DiscardInBuffer();
                    //             ComPorts.port.DiscardOutBuffer();
                    //             ComPorts.Kol_poket = false;
                    ////              ComPorts.Poket_Write();
                    //             Pricel = 3;
                    //          }
                    //          break;
                    case 1:
                        if (Pricel == 1)
                        {
                            System.Threading.Thread.Sleep(300);
                            ComPorts.posilca[1] = 11;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            label24.Text = "Включение лазера";
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
                            label24.Text = " ";
                            toolStripStatusLabel2.Text = "ADC_On";
                            ComPorts.pokets.poket1 = 1;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();

                            PricelBefore = true;
                            Pricel = 3;
                        }
                        break;
                }
                #endregion
            }

            #region
            if (Begins == true)
            {
                switch (rejims)
                {
                    case 0:
                        if (begins_to == 0)
                        {
                            ComPorts.posilca[1] = 28;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = "  ";
                            toolStripStatusLabel2.Text = "BPO_END";
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            label24.Text = "Перевод БПО в конечное положение";
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            begins_to = 1;
                        }
                        break;
                    case 1:
                        if (begins_to == 1)
                        {
                            ComPorts.posilca[1] = 14;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = "  ";
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            toolStripStatusLabel2.Text = "Off_DP_Off";
                            ComPorts.Kol_poket = false;
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            label24.Text = "Выключение блокировки панкратики";
                            ComPorts.Poket_Write();
                            begins_to = 2;
                            System.Threading.Thread.Sleep(300);
                            Begins = false;
                            label24.Text = " ";
                            button1.Enabled = true;
                            Pysk.Enabled = true;
                            PricelBefore = false;
                        }
                        break;
                }
            }
            #endregion

            #region Контроль изделия
            if (Product_Kontrol == true)
            {
                switch (rejim)
                {
                    case 0:
                        if (product_izdel == 0)
                        {
                            //  if (PricelBefore == true)
                            // {
                            ComPorts.posilca[1] = 14;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = "  ";
                            toolStripStatusLabel2.Text = "Off_DP_Off";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            System.Threading.Thread.Sleep(700);
                            //  }

                            product_izdel = 1;
                        }
                        break;
                    case 1:
                        if (product_izdel == 1)
                        {
                            System.Threading.Thread.Sleep(8);
                            ComPorts.posilca[1] = 11;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = "  ";
                            toolStripStatusLabel2.Text = "Laser_On";
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.pokets.poket1 = 0;
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

                            PricelBefore = false;

                            product_izdel = 2;
                        }
                        break;
                    case 2:
                        if (product_izdel == 2)
                        {
                            ComPorts.posilca[1] = 17;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = "  ";
                            toolStripStatusLabel2.Text = "Shod";
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.Kol_poket = false;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.Poket_Write();
                            product_izdel = 3;
                        }
                        break;
                    case 3:
                        if (product_izdel == 3)
                        {
                            ComPorts.posilca[1] = 25;
                            ComPorts.pokets.poket1 = 1;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 675;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = "  ";
                            toolStripStatusLabel2.Text = "ADC_On";
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.Poket_Write();
                            product_izdel = 4;
                        }
                        break;
                    case 4:
                        if (product_izdel == 4)
                        {
                            //ComPorts.port.Read(ComPorts.priem_nedoch, 0, 37800 - ComPorts.schet_poket * 9);
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
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = "  ";
                            toolStripStatusLabel2.Text = "Return";
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.Poket_Write();
                            ComPorts.Kol_poket = false;
                            product_izdel = 5;

                            button1.Enabled = true;
                            Pysk.Enabled = true;
                            //myTimer.Stop();
                        }
                        break;
                }

            }
            #endregion

            #region Механические испытания
            if (Mexanicheskiy_Ispitaniy == true)
            {
                switch (rejim)
                {
                    case 20:
                        if (mexan_ispit == 0)
                        {
                            //if (PricelBefore == true)
                            //{
                            ComPorts.posilca[1] = 14;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = "  ";
                            toolStripStatusLabel2.Text = "Off_DP_Off";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            System.Threading.Thread.Sleep(700);
                            //}

                            mexan_ispit = 1;
                        }
                        break;

                    case 21:
                        if (mexan_ispit == 1)
                        {
                            ComPorts.posilca[1] = 11;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = "  ";
                            toolStripStatusLabel2.Text = "Laser_On";
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.pokets.poket1 = 0;
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
                            PricelBefore = false;
                            mexan_ispit = 2;
                        }
                        break;
                    case 22:
                        if (mexan_ispit == 2)
                        {
                            ComPorts.posilca[1] = 17;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = "  ";
                            toolStripStatusLabel2.Text = "Shod";
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.Kol_poket = false;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.Poket_Write();
                            mexan_ispit = 3;
                        }
                        break;
                    case 23:
                        if (mexan_ispit == 3)
                        {
                            ComPorts.posilca[1] = 25;
                            ComPorts.pokets.poket1 = 1;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 675;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = "  ";
                            toolStripStatusLabel2.Text = "ADC_On";
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.Poket_Write();
                            mexan_ispit = 4;
                        }
                        break;
                    case 24:
                        if (mexan_ispit == 4)
                        {
                            //       ComPorts.port.Read(ComPorts.priem_nedoch, 0, 37800 - ComPorts.schet_poket * 9);
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
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = "  ";
                            toolStripStatusLabel2.Text = "Return";
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.Poket_Write();
                            ComPorts.Kol_poket = false;
                            mexan_ispit = 5;

                            button1.Enabled = true;
                            Pysk.Enabled = true;
                            //myTimer.Stop();
                        }
                        break;
                }
            }
            #endregion

            #region  Климатические испытания
            if (Klimat_Ispitaniy == true)
            {
                switch (rejim)
                {
                    case 30:
                        if (klimat_ispit == 0)
                        {
                            //if (PricelBefore == true)
                            //{
                            ComPorts.posilca[1] = 14;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            toolStripStatusLabel1.Text = "  ";
                            toolStripStatusLabel2.Text = "Off_DP_Off";
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                            System.Threading.Thread.Sleep(700);
                            //}

                            klimat_ispit = 1;
                        }
                        break;

                    case 31:
                        if (klimat_ispit == 1)
                        {
                            System.Threading.Thread.Sleep(10);
                            ComPorts.posilca[1] = 11;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.size_On = 3;
                            ComPorts.pokets.poket1 = 0;
                            toolStripStatusLabel1.Text = "  ";
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

                            PricelBefore = false;

                            klimat_ispit = 2;
                        }
                        break;
                    case 32:
                        if (klimat_ispit == 2)
                        {
                            ComPorts.posilca[1] = 17;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.size_On = 3;
                            ComPorts.pokets.poket1 = 0;
                            toolStripStatusLabel1.Text = "  ";
                            toolStripStatusLabel2.Text = "Shod";
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.Poket_Write();
                            klimat_ispit = 3;
                        }
                        break;
                    case 33:
                        if (klimat_ispit == 3)
                        {
                            ComPorts.posilca[1] = 25;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 675;
                            ComPorts.size_On = 3;
                            ComPorts.pokets.poket1 = 1;
                            toolStripStatusLabel1.Text = "  ";
                            toolStripStatusLabel2.Text = "ADC_On";
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.Poket_Write();
                            klimat_ispit = 4;
                        }
                        break;
                    case 34:
                        if (klimat_ispit == 4)
                        {
                            ComPorts.posilca[1] = 26;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.size_On = 3;
                            ComPorts.pokets.poket1 = 0;
                            toolStripStatusLabel1.Text = "  ";
                            toolStripStatusLabel2.Text = "ADC_Off";
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.Poket_Write();


                            ComPorts.posilca[1] = 12;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.size_On = 3;
                            ComPorts.pokets.poket1 = 0;
                            toolStripStatusLabel1.Text = "  ";
                            toolStripStatusLabel2.Text = "Return";
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.Poket_Write();
                            klimat_ispit = 5;
                            button1.Enabled = true;
                            Pysk.Enabled = true;
                            // myTimer.Stop();
                        }
                        break;
                }
            }
            #endregion

            #region Вывод на экран данных

            //ComPorts.pokets.Kol_poket = false;

            if (ComPorts.Kol_poket == true)
            {
                ZedGraphControl Zedgraph = new ZedGraphControl();
                Zedgraph.Size = zedGraphControl1.Size;
                zedGraphControl1.GraphPane = Zedgraph.GraphPane;

                #region ADC_OUT
                for (l = number; l < ComPorts.schet_poket; l++)
                {
                    Kor_Z1 = (short)((((ComPorts.buffer[l][3] << 8) & 0x7F00) | ComPorts.buffer[l][2]));
                    Kor_Y1 = (short)((((ComPorts.buffer[l][5] << 8) & 0x7F00) | ComPorts.buffer[l][4]));
                    Kor_P = (double)((double)((double)((double)(((ComPorts.buffer[l][7] << 8) & 0x7F00) | ComPorts.buffer[l][6]) / 1024)));

                    if ((ComPorts.buffer[l][3] == 0x80) || (ComPorts.buffer[l][3] == 0x81) || (ComPorts.buffer[l][3] == 0x82) || (ComPorts.buffer[l][3] == 0x83))
                    {

                        Kor_Z = (double)((double)(((Kor_Z1) / (double)(1024)) - XMLW.Cmehenie_Z) / (XMLW.MZ / XMLW.Const));
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

                    Kor_ZY = Math.Sqrt((Math.Pow(Kor_Z, 2) + Math.Pow(Kor_Y, 2)));

                    ////!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        Buffer_Z[Counter] = Kor_Z;
                        Buffer_Y[Counter] = Kor_Y;
                        Buffer_P[Counter] = Kor_P;
                        Buffer_ZY[Counter] = Kor_ZY;

                    if (Counter > StartTime)//1798)
                    {
                        double buf_Z_tmp = 0;
                        double buf_Y_tmp = 0;
                        double buf_P_tmp = 0;
                        double buf_ZY_tmp = 0;


                        for (int i = Counter - RastSize - 1; i < Counter - 1; i++)
                        {
                            buf_Z_tmp += Buffer_Z[i];
                            buf_Y_tmp += Buffer_Y[i];
                            buf_P_tmp += Buffer_P[i];
                            buf_ZY_tmp += Buffer_ZY[i];
                        }

                        buf_Z_tmp /= RastSize;
                        buf_Y_tmp /= RastSize;
                        buf_P_tmp /= RastSize;
                        buf_ZY_tmp /= RastSize;

                        // Расчет корректировки для графика Kor_Z
                        if (Math.Abs(Buffer_Z[Counter] - buf_Z_tmp) > ThresholdTrigger)
                        {
                            // Взависимости от знака +- 0.05
                            if (Buffer_Z[Counter] - buf_Z_tmp >= 0)
                            {
                                Kor_Z = buf_Z_tmp + ThresholdTrigger;
                            }
                            else
                            {
                                Kor_Z = buf_Z_tmp - ThresholdTrigger;
                            }
                            Buffer_Z[Counter] = Kor_Z;
                        }
                        else
                        {
                            Kor_Z = Buffer_Z[Counter];
                        }

                        // Расчет корректировки для графика Kor_Y
                        if (Math.Abs(Buffer_Y[Counter] - buf_Y_tmp) > ThresholdTrigger)
                        {
                            // Взависимости от знака +- 0.05
                            if (Buffer_Y[Counter] - buf_Y_tmp >= 0)
                            {
                                Kor_Y = buf_Y_tmp + ThresholdTrigger;
                            }
                            else
                            {
                                Kor_Y = buf_Y_tmp - ThresholdTrigger;
                            }
                            Buffer_Z[Counter] = Kor_Y;
                        }
                        else
                        {
                            Kor_Y = Buffer_Y[Counter];
                        }

                        // Расчет корректировки для графика Kor_P
                        if (Math.Abs(Buffer_P[Counter] - buf_P_tmp) > ThresholdTrigger)
                        {
                            // Взависимости от знака +- 0.05
                            if (Buffer_P[Counter] - buf_P_tmp >= 0)
                            {
                                Kor_P = buf_P_tmp + ThresholdTrigger;
                            }
                            else
                            {
                                Kor_P = buf_P_tmp - ThresholdTrigger;
                            }
                            Buffer_P[Counter] = Kor_P;
                        }
                        else
                        {
                            Kor_P = Buffer_P[Counter];
                        }

                        // Расчет корректировки графика Kor_ZY
                        if (Math.Abs(Buffer_ZY[Counter] - buf_ZY_tmp) > ThresholdTrigger)
                        {
                            // Взависимости от знака +- 0.05
                            if (Buffer_ZY[Counter] - buf_ZY_tmp >= 0)
                            {
                                Kor_ZY = buf_ZY_tmp + ThresholdTrigger;
                            }
                            else
                            {
                                Kor_ZY = buf_ZY_tmp - ThresholdTrigger;
                            }
                            Buffer_ZY[Counter] = Kor_ZY;
                        }
                        else
                        {
                            Kor_ZY = Buffer_ZY[Counter];
                        }
                    }
                    Counter++;
                    ////!!!!!!!!!!!!!!!!!!!!!!!!!!!

                    list1.Add(l, Kor_Z);
                    list2.Add(l, Kor_Y);
                    list3.Add(l, Kor_P);
                    if (Pysk.Checked == true)
                    {

                    }
                    else
                    {
                        list4.Add(l, Kor_ZY);
                    }

                    bufferZ[l] = Kor_Z;
                    bufferY[l] = Kor_Y;
                    bufferZY[l] = Kor_ZY;
                    bufferP[l] = Kor_P;

                    obsKof = XMLW.V * XMLW.Tay * 0.25 * (Math.PI) * Math.Pow(((XMLW.D * 5880 / XMLW.Fend)), 2);
                    textBox11.Text = Convert.ToString(Math.Round(((Kor_P / obsKof) * 1024), 2));
                    textBox11.Refresh();
                    number++;
                    if (l >= 4195)
                    {
                        //rejim++;
                        timer_25 = 0;
                        XMLW.endpropise = true;
                        textBox11.Text = Convert.ToString(Math.Round(((list3[3000].Y / obsKof) * 1024), 2));
                        textBox11.Refresh();
                        /////////////////////// Max Min //////////////////

                        canselPr();

                        #region Max Min

                        if (dopyskZona == true)
                        {
                            int element_i1 = 0;
                            int element_i2 = 2100;
                            int element_i3 = 0;
                            int element_i4 = 2100;

                            try
                            {
                                max = Math.Abs(list1.ElementAt(0).Y);
                                max1 = Math.Abs(list1.ElementAt(2100).Y);


                                maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(0).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(0).Y * 3.3, 2))), 2);
                                maxZY1 = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(2100).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(2100).Y * 3.3, 2))), 2);

                                for (int i = 1; i < 2100; i++)
                                {
                                    if (Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2) > maxZY)
                                    {
                                        maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2);
                                    }

                                }
                                for (int i = 2101; i < list1.Count; i++)
                                {
                                    if (Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2) > maxZY1)
                                    {
                                        maxZY1 = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2);
                                    }

                                }
                                for (int i = 1; i < 2100; i++)
                                {
                                    if (Math.Abs(list1.ElementAt(i).Y) > max)
                                    {
                                        max = Math.Abs(list1.ElementAt(i).Y);
                                        element_i1 = i;

                                    }

                                }
                                for (int i = 2101; i < list1.Count; i++)
                                {
                                    if (Math.Abs(list1.ElementAt(i).Y) > max1)
                                    {
                                        max1 = Math.Abs(list1.ElementAt(i).Y);
                                        element_i2 = i;
                                    }

                                }
                                max2 = Math.Abs(list2.ElementAt(0).Y);
                                max3 = Math.Abs(list2.ElementAt(2100).Y);


                                for (int i = 1; i < 2100; i++)
                                {
                                    if (Math.Abs(list2.ElementAt(i).Y) > max2)
                                    {
                                        max2 = Math.Abs(list2.ElementAt(i).Y);
                                        element_i3 = i;
                                    }

                                }
                                for (int i = 2101; i < list2.Count; i++)
                                {
                                    if (Math.Abs(list2.ElementAt(i).Y) > max3)
                                    {
                                        max3 = Math.Abs(list2.ElementAt(i).Y);
                                        element_i4 = i;
                                    }

                                }
                                textBox12.Text = Convert.ToString(Math.Round((list1.ElementAt(element_i1).Y * 3.3), 2));
                                textBox13.Text = Convert.ToString(Math.Round((list2.ElementAt(element_i3).Y * 3.3), 2));

                                textBox14.Text = Convert.ToString(Math.Round((list1.ElementAt(element_i2).Y * 3.3), 2));
                                textBox15.Text = Convert.ToString(Math.Round((list2.ElementAt(element_i4).Y * 3.3), 2));
                                textBox16.Text = Convert.ToString(maxZY);
                                textBox17.Text = Convert.ToString(maxZY1);
                            }
                            catch {  }
                            
                        }
                        else
                        /////////////////////////////////////////////////
                        {
                            int element_i1 = 0;
                            int element_i2 = 0;
                            try
                            {
                                maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(0).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(0).Y * 3.3, 2))), 2);

                                for (int i = 1; i < list1.Count; i++)
                                {
                                    if (Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2) > maxZY)
                                    {
                                        maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2);
                                    }

                                }


                                max = Math.Abs(list1[0].Y);
                                maxout = list1[0].Y;

                                for (int i = 1; i < list1.Count; i++)
                                {
                                    if (Math.Abs(list1[i].Y) > max)
                                    {
                                        max = Math.Abs(list1[i].Y);
                                        maxout = list1[i].Y;
                                        element_i1 = i;
                                    }

                                }

                                max2 = Math.Abs(list2[0].Y);
                                max2out = list2[0].Y;

                                for (int i = 1; i < list2.Count; i++)
                                {
                                    if (Math.Abs(list2[i].Y) > max2)
                                    {
                                        max2 = Math.Abs(list2[i].Y);
                                        max2out = list2[i].Y;
                                        element_i2 = i;
                                    }

                                }

                            }
                            catch { }

                            textBox20.Text = Convert.ToString(Math.Round((maxout * 3.3), 2));
                            textBox19.Text = Convert.ToString(Math.Round((max2out * 3.3), 2));
                            textBox18.Text = Convert.ToString(maxZY);

                        }
                        #endregion


                        break;
                    }
                }
                if (Pysk.Checked == true)
                {
                    if (timer_25 >= 4200)
                    {
                        Pysk.Checked = false;
                        timer_25 = 0;
                        XMLW.endpropise = true;

                    }
                    else
                        timer_25 += 10;
                }
                else
                {

                }

                trackBar1.Maximum = XMLW.trBarMax;
                trackBar2.Maximum = XMLW.trBarMax;

                trackBar1.Enabled = true;
                trackBar2.Enabled = true;
                #endregion

                CreateGraph(Zedgraph);
                zedGraphControl1.Refresh();
                Zedgraph.Dispose();

            }
            else
            {
                if (ComPorts.One_Buffers == true)
                {
                    ZedGraphControl Zedgraph = new ZedGraphControl();
                    Zedgraph.Size = zedGraphControl1.Size;
                    zedGraphControl1.GraphPane = Zedgraph.GraphPane;
                    ReadBuffers1 = ComPorts.pokets.buffer1;

                    #region GetStatus

                    #region Byte2
                    if (ReadBuffers1[1] == 100)
                    {
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

                        if (GetStatus_Byte1[0] == 1)
                        {
                            form4.checkBox1.Checked = true;
                        }
                        else
                            form4.checkBox1.Checked = false;

                        if (GetStatus_Byte1[1] == 1)
                        {
                            form4.checkBox2.Checked = true;
                        }
                        else
                            form4.checkBox2.Checked = false;
                        if (GetStatus_Byte1[2] == 1)
                        {
                            form4.checkBox3.Checked = true;
                        }
                        else
                            form4.checkBox3.Checked = false;
                        if (GetStatus_Byte1[3] == 1)
                        {
                            form4.checkBox4.Checked = true;
                        }
                        else
                            form4.checkBox4.Checked = false;
                        if (GetStatus_Byte1[4] == 1)
                        {
                            form4.checkBox5.Checked = true;
                        }
                        else
                            form4.checkBox5.Checked = false;
                        if (GetStatus_Byte1[5] == 1)
                        {
                            form4.checkBox6.Checked = true;
                        }
                        else
                            form4.checkBox6.Checked = false;
                        if (GetStatus_Byte1[6] == 1)
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
                        /* if (a == 1)
                         {
                             GetStatus_Byte2[i] = 0;
                         }
                         */
                        if (ReadBuffers1[3] == 0)
                        {
                            GetStatus_Byte2[i] = 0;
                        }
                        else
                            GetStatus_Byte2[i] = 1;


                        if (GetStatus_Byte2[0] == 1)
                        {
                            form4.checkBox14.Checked = true;
                        }
                        else
                            form4.checkBox14.Checked = false;


                        if (GetStatus_Byte2[1] == 1)
                        {
                            form4.checkBox13.Checked = true;
                        }
                        else
                            form4.checkBox13.Checked = false;
                        if (GetStatus_Byte2[2] == 1)
                        {
                            form4.checkBox12.Checked = true;
                        }
                        else
                            form4.checkBox12.Checked = false;
                        if (GetStatus_Byte2[3] == 1)
                        {
                            form4.checkBox11.Checked = true;
                        }
                        else
                            form4.checkBox11.Checked = false;
                        if (GetStatus_Byte2[4] == 1)
                        {
                            form4.checkBox10.Checked = true;
                        }
                        else
                            form4.checkBox10.Checked = false;
                        if (GetStatus_Byte2[5] == 1)
                        {
                            form4.checkBox9.Checked = true;
                        }
                        else
                            form4.checkBox9.Checked = false;
                        if (GetStatus_Byte2[6] == 1)
                        {
                            form4.checkBox8.Checked = true;
                        }
                        else
                            form4.checkBox8.Checked = false;


                        if (GetStatus_Byte2[7] == 1)
                        {
                            form4.checkBox16.Checked = true;
                        }
                        else
                            form4.checkBox16.Checked = false;



                        if (Product_Kontrol == true)
                        {
                            if ((GetStatus_Byte1[0] == 1) && (GetStatus_Byte2[4] == 1))
                            {
                                rejim++;
                            }
                        }
                        if (Product_Ustirovka == true)
                        {
                            if ((GetStatus_Byte1[0] == 1) && (GetStatus_Byte2[5] == 1))
                            {
                                rejim++;
                            }
                        }
                        if (Mexanicheskiy_Ispitaniy == true)
                        {
                            if ((GetStatus_Byte1[0] == 1) && (GetStatus_Byte2[5] == 1))
                            {
                                rejim++;
                            }
                        }
                        if (Klimat_Ispitaniy == true)
                        {
                            if ((GetStatus_Byte1[0] == 1) && (GetStatus_Byte2[5] == 1))
                            {
                                rejim++;
                            }
                        }

                    }
                        #endregion

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
                    }
                    rastor = (double)((((ReadBuffers1[4] << 8) & 0x7F00) | ReadBuffers1[3])); //Растор00
                    //if ((rastor < 480) || (rastor > 520))
                    //{
                    //    MessageBox.Show("Недостаточная частота растра");
                    //}
                    #endregion

                    #region ADC_OUT однократно
                    if (ReadBuffers1[1] == 104)
                    {
                        Kor_Z = (short)((((ReadBuffers1[3] << 8) & 0xFF00) | ReadBuffers1[2]));
                        Kor_Y = (short)((((ReadBuffers1[5] << 8) & 0xFF00) | ReadBuffers1[4]));
                        Kor_P = (short)((((ReadBuffers1[7] << 8) & 0xFF00) | ReadBuffers1[6]));
                        list1.Add(1, Kor_Z);
                        list2.Add(1, Kor_Y);
                        list3.Add(1, Kor_P);
                    }

                    //CreateGraph(Zedgraph);
                    //zedGraphControl1.Refresh();
                    #endregion

                    #region Ready
                    if (ReadBuffers1[1] == 101)
                    {
                        toolStripStatusLabel1.Text = "ГОТОВО";
                        #region прицеливание
                        if (Pysk.Checked == true)
                        {
                            if (toolStripStatusLabel2.Text == "Laser_On")
                            {
                                rejim2++;
                            }
                            if (toolStripStatusLabel2.Text == "Return")
                            {
                                rejim2++;
                            }
                            if (toolStripStatusLabel2.Text == "Shtorka_On")
                            {
                                rejim2++;
                            }
                            if (toolStripStatusLabel2.Text == "Pankr_End_On")
                            {
                                rejim2++;
                            }
                            if (toolStripStatusLabel2.Text == "Off_DP_Off")
                            {
                                rejim2++;
                            }
                            if (toolStripStatusLabel2.Text == "BPO_END")
                            {
                                rejim2++;
                            }
                        }
                        #endregion

                        #region Ответы по рассогласованию
                        #region Контроль изделия
                        if (Product_Kontrol == true)
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
                            if (toolStripStatusLabel2.Text == "Off_DP_Off")
                            {
                                rejim++;
                            }
                            if (toolStripStatusLabel2.Text == "ADC_Off")
                            {
                                rejim++;
                            }
                            if (toolStripStatusLabel2.Text == "Return")
                            {
                                rejim++;
                                // myTimer.Stop();
                            }
                            //toolStripStatusLabel2.Text = "Return";
                        }
                        #endregion

                        #region Юстировка изделия
                        if (Product_Ustirovka == true)
                        {
                            if (toolStripStatusLabel2.Text == "BPO_END")
                            {
                                rejim++;
                            }
                            if (toolStripStatusLabel2.Text == "Laser_On")
                            {
                                rejim++;
                            }
                            if (toolStripStatusLabel2.Text == "Pankr_End_On")
                            {
                                rejim++;
                            }
                            if (toolStripStatusLabel2.Text == "ADC_Off")
                            {
                                rejim++;
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

                        #region Механическое испытание
                        if (Mexanicheskiy_Ispitaniy == true)
                        {
                            if (toolStripStatusLabel2.Text == "BPO_END")
                            {
                                rejim++;
                            }
                            if (toolStripStatusLabel2.Text == "BPO_START")
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
                            if (toolStripStatusLabel2.Text == "ADC_Off")
                            {
                                rejim++;
                            }
                            if (toolStripStatusLabel2.Text == "Off_DP_Off")
                            {
                                rejim++;
                            }
                            if (toolStripStatusLabel2.Text == "Return")
                            {
                                ////myTimer.Stop();
                                rejim++;
                            }
                        }
                        #endregion

                        #region Климат испытание
                        if (Klimat_Ispitaniy == true)
                        {
                            if (toolStripStatusLabel2.Text == "BPO_END")
                            {
                                rejim++;
                            }
                            if (toolStripStatusLabel2.Text == "BPO_START")
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
                            if (toolStripStatusLabel2.Text == "ADC_Off")
                            {
                                rejim++;
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
                        #endregion

                        #region Настройка прибора
                        if (Begins == true)
                        {
                            if (toolStripStatusLabel2.Text == "BPO_END")
                            {
                                rejims++;
                            }
                            if (toolStripStatusLabel2.Text == "Off_DP_Off")
                            {
                                rejims++;
                            }
                        }
                        #endregion
                    }
                    #endregion

                    #region Test_time
                    if (ReadBuffers1[1] == 103)
                    {
                        time_muve_pankr = (short)((((ReadBuffers1[4] << 8) & 0xFF00) | ReadBuffers1[3]));
                        time_begin_sniatia_prevish = (short)((((ReadBuffers1[6] << 8) & 0xFF00) | ReadBuffers1[5]));
                        time_sniatia_prevish = (short)((((ReadBuffers1[8] << 8) & 0xFF00) | ReadBuffers1[7]));
                        time_vozvrata = (short)((((ReadBuffers1[10] << 8) & 0xFF00) | ReadBuffers1[9]));
                    }
                    #endregion
                    CreateGraph(Zedgraph);
                    zedGraphControl1.Refresh();
                    Zedgraph.Dispose();
                }
                ComPorts.One_Buffers = false;



            }

            #endregion

        }

        private void button1_Click(object sender, EventArgs e)
        {
            XMLW.endpropise = false;

            Pysk.Enabled = false;

            добавитьРезультатВБазуToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
            добавитьРезультатВБазуToolStripMenuItem.Text = "Сохранить результат";
            добавитьВОтчетToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
            добавитьВОтчетToolStripMenuItem.Text = "Добавить в отчет";

            усреднитьToolStripMenuItem.Text = "Усреднить";
            усреднитьToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);

            first_click2 = 0;

            textBox10.Text = "0";
            textBox11.Text = "0";
            textBox12.Text = "0";
            textBox13.Text = "0";
            textBox14.Text = "0";
            textBox15.Text = "0";
            textBox16.Text = "0";
            textBox17.Text = "0";
            //textBox18.Text = "0";
            //textBox19.Text = "0";

            //textBox20.Text = "0";

            textBox8.Text = "0";
            textBox9.Text = "0";

            list1.Clear();
            list2.Clear();
            list3.Clear();
            list4.Clear();
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

            if (form10.label1.Text == "1")
            {
                Product_Ustirovka = false;
                Mexanicheskiy_Ispitaniy = false;
                Klimat_Ispitaniy = false;

                Product_Kontrol = true;
                rejim = 0;
                product_izdel = 0;
            }
            else
            {
                if (form10.label1.Text == "2")
                {
                    Product_Kontrol = false;
                    Mexanicheskiy_Ispitaniy = false;
                    Klimat_Ispitaniy = false;
                    Product_Ustirovka = true;
                    rejim = 10;
                }
                else
                {
                    if (form10.label1.Text == "3")
                    {
                        Product_Kontrol = false;
                        Product_Ustirovka = false;
                        Klimat_Ispitaniy = false;
                        Mexanicheskiy_Ispitaniy = true;
                        mexan_ispit = 0;
                        rejim = 20;
                    }
                    else
                    {
                        if (form10.label1.Text == "4")
                        {
                            Product_Kontrol = false;
                            Product_Ustirovka = false;
                            Mexanicheskiy_Ispitaniy = false;
                            Klimat_Ispitaniy = true;
                            klimat_ispit = 0;
                            rejim = 30;
                        }
                    }
                }
            }
            button1.Enabled = false;
            timer_25 = 0;
            this.Text = zagolovok + " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")";
        }

        private void результатыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            form3.XMLW = XMLW;
            //form3.form2 = this;
            form3.ShowDialog();
            list1 = XMLW.list1;
            list2 = XMLW.list2;
            list3 = XMLW.list3;

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
                //    numericUpDown1.Enabled = true;
                //    numericUpDown2.Enabled = true;
                //    numericUpDown3.Enabled = true;
                //    numericUpDown4.Enabled = true;
                //    numericUpDown5.Enabled = true;
            }
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
                textBox10.Text = Convert.ToString(Math.Round(list1[trackBar1.Value].Y * 3.3, 2));
                kor1 = Convert.ToDouble(textBox10.Text);
                textBox9.Text = Convert.ToString(Math.Round(list2[trackBar1.Value].Y * 3.3, 2));
                kor2 = Convert.ToDouble(textBox9.Text);
                textBox8.Text = Convert.ToString(Math.Round(System.Math.Sqrt((System.Math.Pow(kor1, 2) + System.Math.Pow(kor2, 2))), 2));
                textBox10.Refresh();
                textBox9.Refresh();
                textBox8.Refresh();
                groupBox7.Text = "В момент Т=" + Convert.ToString(Math.Round((double)((double)trackBar1.Value / (double)200), 2)) + "с";
                groupBox7.Refresh();
                //textBox11.Text = Convert.ToString(Math.Round(kor1 * kor2 * Convert.ToDouble(textBox8.Text) * 1.92 *100, 2));
                //textBox11.Refresh();

            }
            catch
            { }
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            StickX1[0] = trackBar2.Value;
            StickX1[1] = trackBar2.Value;

            ZedGraphControl Zedgraph = new ZedGraphControl();
            Zedgraph.Size = zedGraphControl1.Size;
            zedGraphControl1.GraphPane = Zedgraph.GraphPane;
            CreateGraph(Zedgraph);
            zedGraphControl1.Refresh();
            Zedgraph.Dispose();

            //textBox1.Text = Convert.ToString(list1[trackBar2.Value].Y);
            //textBox2.Text = Convert.ToString(list2[trackBar2.Value].Y);
            //textBox3.Text = Convert.ToString(list3[trackBar2.Value].Y);
            //textBox1.Refresh();
            //textBox2.Refresh();
            //textBox3.Refresh();
            try
            {
                textBox10.Text = Convert.ToString(Math.Round(list1[trackBar1.Value].Y * 3.3, 2));
                textBox9.Text = Convert.ToString(Math.Round(list2[trackBar1.Value].Y * 3.3, 2));
                textBox8.Text = Convert.ToString(Math.Round(System.Math.Sqrt((System.Math.Pow(list1[trackBar1.Value].Y, 2) * System.Math.Pow(list2[trackBar1.Value].Y, 2))), 2));
                textBox10.Refresh();
                textBox9.Refresh();
                textBox8.Refresh();
            }
            catch
            { }

        }

        public void SaveXML()
        {

            XmlTextWriter writer = new XmlTextWriter("xml\\" + form10.NumberProduct + " - " + this.Text + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")" + ".xml", null);
            writer.WriteStartElement("Sosnay_data_file");
            writer.Formatting = Formatting.Indented;

            writer.WriteStartElement("Sosnay_description");
            writer.WriteElementString("Number", form10.NumberProduct);
            writer.WriteElementString("Type_Check", "1111");
            writer.WriteElementString("Date", DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString());
            writer.WriteElementString("Time", DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("Sosnay_data");

            for (int i = 0; i < list1.Count; i++) //исправить кол-во элементов
            {
                writer.WriteElementString("Y", Convert.ToString(list1[i].Y));
                writer.WriteElementString("Z", Convert.ToString(list2[i].Y));
                writer.WriteElementString("P", Convert.ToString(list3[i].Y));
            }
            writer.WriteEndElement();

            writer.WriteFullEndElement();
            writer.Close();

        } //Сохранение в файл

        private void addDB_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveXML();
        }

        private void новаяПроверкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            form5.ShowDialog();
        }

        private void toleranceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MouseClick = 0;
            tolerance = true;
            myTimer.Stop();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            ZedGraphControl Zedgraph = new ZedGraphControl();
            Zedgraph.Size = zedGraphControl1.Size;
            zedGraphControl1.GraphPane = Zedgraph.GraphPane;
            CreateGraph(Zedgraph);
            zedGraphControl1.Refresh();
            Zedgraph.Dispose();
        }

        private void getToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 10;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 6;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            toolStripStatusLabel2.Text = "Get_Status";
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
            //product_izdel = 150;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            short[] buf = new short[2];
            buf[0] = 0xFF;
            buf[1] = 129;

            Kor_Z1 = (short)(((buf[1] << 8) & 0x7F00) | buf[0]);

            Kor_Z = (double)((double)(Kor_Z1) / (double)(1024));

        }

        private void zedGraphControl1_Paint_1(object sender, PaintEventArgs e)
        {
            zedGraphControl1.GraphPane.YAxis.Scale.Min = -1.1;
            zedGraphControl1.GraphPane.YAxis.Scale.Max = 1.1;

            zedGraphControl1.GraphPane.YAxis.Scale.MajorStep = 0.1f;
            zedGraphControl1.GraphPane.XAxis.Scale.MajorStep = 200;
            //zedGraphControl1.GraphPane.XAxis.Scale.Format = "f3";
            zedGraphControl1.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XScaleFormatEvent);
            zedGraphControl1.GraphPane.XAxis.Scale.Min = 0;
        }

        public string XScaleFormatEvent(GraphPane pane, Axis axis, double val, int index)
        {
            return (val / 200).ToString();
        }

        private void Pysk_CheckedChanged(object sender, EventArgs e)
        {
            first_click++;
            if (first_click == 1)
            {
                timer_25 = 0;
                Pysk.Text = "СТОП";
                button1.Enabled = false;
                button4.Enabled = false;
                усреднитьToolStripMenuItem.Text = "Усреднить";
                усреднитьToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);

                first_click2 = 0;
                list1.Clear();
                list2.Clear();
                list3.Clear();
                list4.Clear();
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
                Pricel = 0;
                rejim2 = 0;
                XMLW.endpropise = false;
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
                System.Threading.Thread.Sleep(100);

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


                //        ComPorts.posilca[1] = 14;
                //        ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                //        ComPorts.size_Off = 3;
                //        ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                //        ComPorts.size_On = 3;
                //        toolStripStatusLabel1.Text = " ";
                //        toolStripStatusLabel2.Text = "Off_DP_Off";
                //        ComPorts.pokets.poket1 = 0;
                //        ComPorts.port.DiscardInBuffer();
                //        ComPorts.port.DiscardOutBuffer();
                //        ComPorts.Kol_poket = false;
                //        ComPorts.Poket_Write();
                //        System.Threading.Thread.Sleep(300);

                first_click = 0;

                #endregion

                Pysk.Text = "ПУСК";

                list1.Clear();
                list2.Clear();
                list3.Clear();
                list4.Clear();
                timer_25 = 0;
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
                number = 0;
                button1.Enabled = true;
                button4.Enabled = true;
                XMLW.endpropise = true;
            }

        }

        private void zedGraphControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (tolerance == true))
            {
                Point p = new Point(e.X, e.Y);
                //   double x, y;
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
                            tolerance = false;
                            ToleranceNew(pointCenter, pointMin);
                            break;
                        }
                    default:
                        break;
                }
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
            }
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
            }
            zedGraphControl1.GraphPane = myPane;
            zedGraphControl1.Refresh();

        }

        private void очисткаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            list1.Clear();
            list2.Clear();
            list3.Clear();
            //label5.Text = "0";
            //label5.Refresh();
            //label7.Text = "0";
            //label7.Refresh();
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

        private void button4_Click(object sender, EventArgs e)
        {
            button4.Enabled = false;
            button1.Enabled = false;
            Product_Kontrol = false;
            Product_Ustirovka = false;
            Mexanicheskiy_Ispitaniy = false;
            Klimat_Ispitaniy = false;
            Pysk.Enabled = false;
            ComPorts.posilca[1] = 32;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            toolStripStatusLabel2.Text = "MotorScan_Off";
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
            System.Threading.Thread.Sleep(20);

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
            System.Threading.Thread.Sleep(20);

            ComPorts.posilca[1] = 26;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            toolStripStatusLabel2.Text = "ADC_Off";
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
            System.Threading.Thread.Sleep(20);


            ComPorts.posilca[1] = 14;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            toolStripStatusLabel2.Text = "Off_DP_Off";
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
            System.Threading.Thread.Sleep(20);

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
            System.Threading.Thread.Sleep(2000);

            button1.Enabled = true;
            Pysk.Enabled = true;
            timer_25 = 0;

            button4.Enabled = true;
            button1.Enabled = true;
            Pysk.Enabled = true;
        }

        public void canselPr()
        {
            button4.Enabled = false;
            button1.Enabled = false;
            Product_Kontrol = false;
            Product_Ustirovka = false;
            Mexanicheskiy_Ispitaniy = false;
            Klimat_Ispitaniy = false;
            Pysk.Enabled = false;
            ComPorts.posilca[1] = 32;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            toolStripStatusLabel2.Text = "MotorScan_Off";
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
            System.Threading.Thread.Sleep(20);

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
            System.Threading.Thread.Sleep(20);

            ComPorts.posilca[1] = 26;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            toolStripStatusLabel2.Text = "ADC_Off";
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
            System.Threading.Thread.Sleep(20);


            ComPorts.posilca[1] = 14;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            toolStripStatusLabel2.Text = "Off_DP_Off";
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
            System.Threading.Thread.Sleep(20);

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
            System.Threading.Thread.Sleep(2000);

            button1.Enabled = true;
            Pysk.Enabled = true;
            timer_25 = 0;

            button4.Enabled = true;
            button1.Enabled = true;
            Pysk.Enabled = true;
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void добавитьРезультатВБазуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveXML();
            XMLW.SaveImage(this);
            // MessageBox.Show("Сохранено");
            добавитьРезультатВБазуToolStripMenuItem.BackColor = Color.LightGreen;
            добавитьРезультатВБазуToolStripMenuItem.Text = "Результат сохранен";
        }

        private void сохранитьРисунокToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XMLW.SavePictureBD(this);
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
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
            //   toolStripStatusLabel3.Text = "Выключение блокировки панкратики";
            System.Threading.Thread.Sleep(2000);

            if ((добавитьРезультатВБазуToolStripMenuItem.BackColor == Color.LightGreen) && (добавитьВОтчетToolStripMenuItem.BackColor == Color.LightGreen))
            {
                form6.button4.BackColor = Color.LightGreen;
                ComPorts.port.Close();
                myTimer.Stop();
                form10.Close();
                form6.form1 = form1;
                form6.Show();
                добавитьРезультатВБазуToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                добавитьРезультатВБазуToolStripMenuItem.Text = "Сохранить результат";
                добавитьВОтчетToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                добавитьВОтчетToolStripMenuItem.Text = "Добавить в отчет";
                this.Dispose();
            }
            else
            {
                if ((добавитьРезультатВБазуToolStripMenuItem.BackColor != Color.LightGreen) && (добавитьВОтчетToolStripMenuItem.BackColor != Color.LightGreen))
                {
                    if (MessageBox.Show("Хотите выйти не сохраняя результат и не добавляя данные в отчет?", "Выход", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        добавитьРезультатВБазуToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                        добавитьРезультатВБазуToolStripMenuItem.Text = "Cохранить результат";
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
                    if (добавитьРезультатВБазуToolStripMenuItem.BackColor != Color.LightGreen)
                    {
                        if (MessageBox.Show("Хотите выйти не сохраняя результат?", "Выход", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            добавитьРезультатВБазуToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                            добавитьРезультатВБазуToolStripMenuItem.Text = "Cохранить результат";
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
                                добавитьРезультатВБазуToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                                добавитьРезультатВБазуToolStripMenuItem.Text = "Cохранить результат";
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

        private void добавитьВОтчетToolStripMenuItem_Click(object sender, EventArgs e)
        {
            добавитьВОтчетToolStripMenuItem.BackColor = Color.LightGreen;
            добавитьВОтчетToolStripMenuItem.Text = "Добавлено в отчет";
            form6.SetValue("D8", textBox16.Text);
            form6.SetValue("E8", textBox12.Text);
            form6.SetValue("F8", textBox13.Text);

            form6.SetValue("D9", textBox17.Text);
            form6.SetValue("E9", textBox14.Text);
            form6.SetValue("F9", textBox15.Text);

        }

        private void усреднитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            first_click2++;
            if (first_click2 == 1)
            {
                усреднитьToolStripMenuItem.Text = "Исходный график";
                усреднитьToolStripMenuItem.BackColor = Color.LightGreen;

                list1.Clear();
                list2.Clear();
                list3.Clear();
                list4.Clear();
              

                ZedGraphControl Zedgraph = new ZedGraphControl();
                Zedgraph.Size = zedGraphControl1.Size;
                zedGraphControl1.GraphPane = Zedgraph.GraphPane;
                //////////////////
                int k = (int)((2500 * 2) / 50);  // вместо koff повсюду 50
                int s = 4050 - 50;
                int a = 0;
                double SumZ = 0;
                double SumY = 0;
                double SumP = 0;
                double Zsi, Ysi, Psi, ZYsi;

                for (int g = 0; g < k; g++)
                {
                    list1.Add(g, bufferZ[g]);
                    list2.Add(g, bufferY[g]);
                    list3.Add(g, bufferP[g]);
                    list4.Add(g, bufferZY[g]);
                }

                    for (int i = k; i <= s; i++)
                    {
                        if (i < 2500)
                        {
                            a = (int)(50 * (double)((double)(i) / (double)(2500)) * (double)((double)(i) / (double)(2500)));
                        }
                        else
                        {
                            a = 50;
                        }
                        SumZ = 0;
                        SumY = 0;
                        SumP = 0;
                        for (int o = (i - a); o <= (i + a); o++)
                        {
                            SumZ = SumZ + bufferZ[o];
                            SumY = SumY + bufferY[o];
                            SumP = SumP + bufferP[o];
                        }
                        Zsi = (double)(SumZ / ((2 * a) + 1));
                        Ysi = (double)(SumY / ((2 * a) + 1));
                        Psi = (double)(SumP / ((2 * a) + 1));
                        ZYsi = Math.Sqrt((Math.Pow(Zsi, 2) + Math.Pow(Ysi, 2)));

                        list1.Add(i, Zsi);
                        list2.Add(i, Ysi);
                        list3.Add(i, Psi);
                        list4.Add(i, ZYsi);
                    }
                //////////////////
                CreateGraph(Zedgraph);
                zedGraphControl1.Refresh();
                Zedgraph.Dispose();

                #region Max Min

                if (dopyskZona == true)
                {
                    int element_i1 = 0;
                    int element_i2 = 0;
                    int element_i3 = 0;
                    int element_i4 = 0;

                    try
                    {
                        max = Math.Abs(list1.ElementAt(0).Y);
                        max1 = Math.Abs(list1.ElementAt(2100).Y);


                        maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(0).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(0).Y * 3.3, 2))), 2);
                        maxZY1 = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(2100).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(2100).Y * 3.3, 2))), 2);

                        for (int i = 1; i < 2100; i++)
                        {
                            if (Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2) > maxZY)
                            {
                                maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2);
                            }

                        }
                        for (int i = 2101; i < list1.Count; i++)
                        {
                            if (Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2) > maxZY1)
                            {
                                maxZY1 = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2);
                            }

                            max = Math.Abs(list1.ElementAt(0).Y);
                            max1 = Math.Abs(list1.ElementAt(2100).Y);

                        }
                        for (int i = 1; i < 2100; i++)
                        {
                            if (Math.Abs(list1.ElementAt(i).Y) > max)
                            {
                                max = Math.Abs(list1.ElementAt(i).Y);
                                element_i1 = i;

                            }

                        }
                        for (int i = 2101; i < list1.Count; i++)
                        {
                            if (Math.Abs(list1.ElementAt(i).Y) > max1)
                            {
                                max1 = Math.Abs(list1.ElementAt(i).Y);
                                element_i2 = i;
                            }

                        }
                        max2 = Math.Abs(list2.ElementAt(0).Y);
                        max3 = Math.Abs(list2.ElementAt(2100).Y);


                        for (int i = 1; i < 2100; i++)
                        {
                            if (Math.Abs(list2.ElementAt(i).Y) > max2)
                            {
                                max2 = Math.Abs(list2.ElementAt(i).Y);
                                element_i3 = i;
                            }

                        }
                        for (int i = 2101; i < list2.Count; i++)
                        {
                            if (Math.Abs(list2.ElementAt(i).Y) > max3)
                            {
                                max3 = Math.Abs(list2.ElementAt(i).Y);
                                element_i4 = i;
                            }

                        }
                    }
                    catch { }
                    textBox12.Text = Convert.ToString(Math.Round((list1.ElementAt(element_i1).Y * 3.3), 2));
                    textBox13.Text = Convert.ToString(Math.Round((list2.ElementAt(element_i3).Y * 3.3), 2));

                    textBox14.Text = Convert.ToString(Math.Round((list1.ElementAt(element_i2).Y * 3.3), 2));
                    textBox15.Text = Convert.ToString(Math.Round((list2.ElementAt(element_i4).Y * 3.3), 2));
                    textBox16.Text = Convert.ToString(maxZY);
                    textBox17.Text = Convert.ToString(maxZY1);
                }
                else
                /////////////////////////////////////////////////
                {
                    int element_i1 = 0;
                    int element_i2 = 0;
                    try
                    {
                        maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(0).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(0).Y * 3.3, 2))), 2);

                        for (int i = 1; i < list1.Count; i++)
                        {
                            if (Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2) > maxZY)
                            {
                                maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2);
                            }

                        }


                        max = Math.Abs(list1[0].Y);
                        maxout = list1[0].Y;

                        for (int i = 1; i < list1.Count; i++)
                        {
                            if (Math.Abs(list1[i].Y) > max)
                            {
                                max = Math.Abs(list1[i].Y);
                                maxout = list1[i].Y;
                                element_i1 = i;
                            }

                        }

                        max2 = Math.Abs(list2[0].Y);
                        max2out = list2[0].Y;

                        for (int i = 1; i < list2.Count; i++)
                        {
                            if (Math.Abs(list2[i].Y) > max2)
                            {
                                max2 = Math.Abs(list2[i].Y);
                                max2out = list2[i].Y;
                                element_i2 = i;
                            }

                        }

                    }
                    catch { }

                    textBox20.Text = Convert.ToString(Math.Round((maxout * 3.3), 2));
                    textBox19.Text = Convert.ToString(Math.Round((max2out * 3.3), 2));
                    textBox18.Text = Convert.ToString(maxZY);

                }
                #endregion
            }
            else
            {
                усреднитьToolStripMenuItem.Text = "Усреднить";
                усреднитьToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                list1.Clear();
                list2.Clear();
                list3.Clear();
                list4.Clear();
               
                ZedGraphControl Zedgraph = new ZedGraphControl();
                Zedgraph.Size = zedGraphControl1.Size;
                zedGraphControl1.GraphPane = Zedgraph.GraphPane;
                for (int i = 0; i < 4195; i++)
                {
                    list1.Add(i, bufferZ[i]);
                    list2.Add(i, bufferY[i]);
                    list3.Add(i, bufferP[i]);
                    list4.Add(i, bufferZY[i]);
                }
                CreateGraph(Zedgraph);
                zedGraphControl1.Refresh();
                Zedgraph.Dispose();
                first_click2 = 0;

                #region Max Min

                if (dopyskZona == true)
                {
                    int element_i1 = 0;
                    int element_i2 = 0;
                    int element_i3 = 0;
                    int element_i4 = 0;

                    try
                    {
                        max = Math.Abs(list1.ElementAt(0).Y);
                        max1 = Math.Abs(list1.ElementAt(2100).Y);


                        maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(0).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(0).Y * 3.3, 2))), 2);
                        maxZY1 = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(2100).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(2100).Y * 3.3, 2))), 2);

                        for (int i = 1; i < 2100; i++)
                        {
                            if (Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2) > maxZY)
                            {
                                maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2);
                            }

                        }
                        for (int i = 2101; i < list1.Count; i++)
                        {
                            if (Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2) > maxZY1)
                            {
                                maxZY1 = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2);
                            }

                            max = Math.Abs(list1.ElementAt(0).Y);
                            max1 = Math.Abs(list1.ElementAt(2100).Y);

                        }
                        for (int i = 1; i < 2100; i++)
                        {
                            if (Math.Abs(list1.ElementAt(i).Y) > max)
                            {
                                max = Math.Abs(list1.ElementAt(i).Y);
                                element_i1 = i;

                            }

                        }
                        for (int i = 2101; i < list1.Count; i++)
                        {
                            if (Math.Abs(list1.ElementAt(i).Y) > max1)
                            {
                                max1 = Math.Abs(list1.ElementAt(i).Y);
                                element_i2 = i;
                            }

                        }
                        max2 = Math.Abs(list2.ElementAt(0).Y);
                        max3 = Math.Abs(list2.ElementAt(2100).Y);


                        for (int i = 1; i < 2100; i++)
                        {
                            if (Math.Abs(list2.ElementAt(i).Y) > max2)
                            {
                                max2 = Math.Abs(list2.ElementAt(i).Y);
                                element_i3 = i;
                            }

                        }
                        for (int i = 2101; i < list2.Count; i++)
                        {
                            if (Math.Abs(list2.ElementAt(i).Y) > max3)
                            {
                                max3 = Math.Abs(list2.ElementAt(i).Y);
                                element_i4 = i;
                            }

                        }
                    }
                    catch { }
                    textBox12.Text = Convert.ToString(Math.Round((list1.ElementAt(element_i1).Y * 3.3), 2));
                    textBox13.Text = Convert.ToString(Math.Round((list2.ElementAt(element_i3).Y * 3.3), 2));

                    textBox14.Text = Convert.ToString(Math.Round((list1.ElementAt(element_i2).Y * 3.3), 2));
                    textBox15.Text = Convert.ToString(Math.Round((list2.ElementAt(element_i4).Y * 3.3), 2));
                    textBox16.Text = Convert.ToString(maxZY);
                    textBox17.Text = Convert.ToString(maxZY1);
                }
                else
                /////////////////////////////////////////////////
                {
                    int element_i1 = 0;
                    int element_i2 = 0;
                    try
                    {
                        maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(0).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(0).Y * 3.3, 2))), 2);

                        for (int i = 1; i < list1.Count; i++)
                        {
                            if (Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2) > maxZY)
                            {
                                maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2);
                            }

                        }


                        max = Math.Abs(list1[0].Y);
                        maxout = list1[0].Y;

                        for (int i = 1; i < list1.Count; i++)
                        {
                            if (Math.Abs(list1[i].Y) > max)
                            {
                                max = Math.Abs(list1[i].Y);
                                maxout = list1[i].Y;
                                element_i1 = i;
                            }

                        }

                        max2 = Math.Abs(list2[0].Y);
                        max2out = list2[0].Y;

                        for (int i = 1; i < list2.Count; i++)
                        {
                            if (Math.Abs(list2[i].Y) > max2)
                            {
                                max2 = Math.Abs(list2[i].Y);
                                max2out = list2[i].Y;
                                element_i2 = i;
                            }

                        }

                    }
                    catch { }

                    textBox20.Text = Convert.ToString(Math.Round((maxout * 3.3), 2));
                    textBox19.Text = Convert.ToString(Math.Round((max2out * 3.3), 2));
                    textBox18.Text = Convert.ToString(maxZY);

                }
                #endregion
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            first_click2++;
            if (first_click2 == 1)
            {
                усреднитьToolStripMenuItem.Text = "Исходный график";
                усреднитьToolStripMenuItem.BackColor = Color.LightGreen;
                form16 = new Form16();
                form16.form2 = this;
                form16.ShowDialog();

                //XMLW.endpropise = false;
                list1.Clear();
                list2.Clear();
                list3.Clear();
                list4.Clear();
                //ZedGraphControl Zedgraph = new ZedGraphControl();
                //Zedgraph.Size = zedGraphControl1.Size;
                //zedGraphControl1.GraphPane = Zedgraph.GraphPane;
                //CreateGraph(Zedgraph);
                //zedGraphControl1.Refresh();
                //Zedgraph.Dispose();

                ZedGraphControl Zedgraph = new ZedGraphControl();
                Zedgraph.Size = zedGraphControl1.Size;
                zedGraphControl1.GraphPane = Zedgraph.GraphPane;
                //////////////////
                int k = (int)((2500 * 2) / koff);
                int s = 4050 - koff;
                int a = 0;
                double SumZ = 0;
                double SumY = 0;
                double SumP = 0;
                double Zsi, Ysi, Psi, ZYsi;

                for (int g = 0; g < k; g++)
                {
                    list1.Add(g, bufferZ[g]);
                    list2.Add(g, bufferY[g]);
                    list3.Add(g, bufferP[g]);
                    list4.Add(g, bufferZY[g]);
                }

                for (int i = k; i <= s; i++)
                {
                    if (i < 2500)
                    {
                        a = (int)(koff * (double)((double)(i) / (double)(2500)) * (double)((double)(i) / (double)(2500)));
                    }
                    else
                    {
                        a = koff;
                    }
                    SumZ = 0;
                    SumY = 0;
                    SumP = 0;
                    for (int o = (i - a); o <= (i + a); o++)
                    {
                        SumZ = SumZ + bufferZ[o];
                        SumY = SumY + bufferY[o];
                        SumP = SumP + bufferP[o];
                    }
                    Zsi = (double)(SumZ / ((2 * a) + 1));
                    Ysi = (double)(SumY / ((2 * a) + 1));
                    Psi = (double)(SumP / ((2 * a) + 1));
                    ZYsi = Math.Sqrt((Math.Pow(Zsi, 2) + Math.Pow(Ysi, 2)));

                    list1.Add(i, Zsi);
                    list2.Add(i, Ysi);
                    list3.Add(i, Psi);
                    list4.Add(i, ZYsi);
                }
                //////////////////
                CreateGraph(Zedgraph);
                zedGraphControl1.Refresh();
                Zedgraph.Dispose();

                #region Max Min

                if (dopyskZona == true)
                {
                    int element_i1 = 0;
                    int element_i2 = 0;
                    int element_i3 = 0;
                    int element_i4 = 0;

                    try
                    {
                        max = Math.Abs(list1.ElementAt(0).Y);
                        max1 = Math.Abs(list1.ElementAt(2100).Y);


                        maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(0).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(0).Y * 3.3, 2))), 2);
                        maxZY1 = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(2100).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(2100).Y * 3.3, 2))), 2);

                        for (int i = 1; i < 2100; i++)
                        {
                            if (Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2) > maxZY)
                            {
                                maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2);
                            }

                        }
                        for (int i = 2101; i < list1.Count; i++)
                        {
                            if (Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2) > maxZY1)
                            {
                                maxZY1 = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2);
                            }

                            max = Math.Abs(list1.ElementAt(0).Y);
                            max1 = Math.Abs(list1.ElementAt(2100).Y);

                        }
                        for (int i = 1; i < 2100; i++)
                        {
                            if (Math.Abs(list1.ElementAt(i).Y) > max)
                            {
                                max = Math.Abs(list1.ElementAt(i).Y);
                                element_i1 = i;

                            }

                        }
                        for (int i = 2101; i < list1.Count; i++)
                        {
                            if (Math.Abs(list1.ElementAt(i).Y) > max1)
                            {
                                max1 = Math.Abs(list1.ElementAt(i).Y);
                                element_i2 = i;
                            }

                        }
                        max2 = Math.Abs(list2.ElementAt(0).Y);
                        max3 = Math.Abs(list2.ElementAt(2100).Y);


                        for (int i = 1; i < 2100; i++)
                        {
                            if (Math.Abs(list2.ElementAt(i).Y) > max2)
                            {
                                max2 = Math.Abs(list2.ElementAt(i).Y);
                                element_i3 = i;
                            }

                        }
                        for (int i = 2101; i < list2.Count; i++)
                        {
                            if (Math.Abs(list2.ElementAt(i).Y) > max3)
                            {
                                max3 = Math.Abs(list2.ElementAt(i).Y);
                                element_i4 = i;
                            }

                        }
                    }
                    catch { }
                    textBox12.Text = Convert.ToString(Math.Round((list1.ElementAt(element_i1).Y * 3.3), 2));
                    textBox13.Text = Convert.ToString(Math.Round((list2.ElementAt(element_i3).Y * 3.3), 2));

                    textBox14.Text = Convert.ToString(Math.Round((list1.ElementAt(element_i2).Y * 3.3), 2));
                    textBox15.Text = Convert.ToString(Math.Round((list2.ElementAt(element_i4).Y * 3.3), 2));
                    textBox16.Text = Convert.ToString(maxZY);
                    textBox17.Text = Convert.ToString(maxZY1);
                }
                else
                /////////////////////////////////////////////////
                {
                    int element_i1 = 0;
                    int element_i2 = 0;
                    try
                    {
                        maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(0).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(0).Y * 3.3, 2))), 2);

                        for (int i = 1; i < list1.Count; i++)
                        {
                            if (Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2) > maxZY)
                            {
                                maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2);
                            }

                        }


                        max = Math.Abs(list1[0].Y);
                        maxout = list1[0].Y;

                        for (int i = 1; i < list1.Count; i++)
                        {
                            if (Math.Abs(list1[i].Y) > max)
                            {
                                max = Math.Abs(list1[i].Y);
                                maxout = list1[i].Y;
                                element_i1 = i;
                            }

                        }

                        max2 = Math.Abs(list2[0].Y);
                        max2out = list2[0].Y;

                        for (int i = 1; i < list2.Count; i++)
                        {
                            if (Math.Abs(list2[i].Y) > max2)
                            {
                                max2 = Math.Abs(list2[i].Y);
                                max2out = list2[i].Y;
                                element_i2 = i;
                            }

                        }

                    }
                    catch { }

                    textBox20.Text = Convert.ToString(Math.Round((maxout * 3.3), 2));
                    textBox19.Text = Convert.ToString(Math.Round((max2out * 3.3), 2));
                    textBox18.Text = Convert.ToString(maxZY);

                }
                #endregion
            }
            else
            {
                усреднитьToolStripMenuItem.Text = "Усреднить";
                усреднитьToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                list1.Clear();
                list2.Clear();
                list3.Clear();
                list4.Clear();

                ZedGraphControl Zedgraph = new ZedGraphControl();
                Zedgraph.Size = zedGraphControl1.Size;
                zedGraphControl1.GraphPane = Zedgraph.GraphPane;
                for (int i = 0; i < 4195; i++)
                {
                    list1.Add(i, bufferZ[i]);
                    list2.Add(i, bufferY[i]);
                    list3.Add(i, bufferP[i]);
                    list4.Add(i, bufferZY[i]);
                }
                CreateGraph(Zedgraph);
                zedGraphControl1.Refresh();
                Zedgraph.Dispose();
                first_click2 = 0;

                #region Max Min

                if (dopyskZona == true)
                {
                    int element_i1 = 0;
                    int element_i2 = 0;
                    int element_i3 = 0;
                    int element_i4 = 0;

                    try
                    {
                        max = Math.Abs(list1.ElementAt(0).Y);
                        max1 = Math.Abs(list1.ElementAt(2100).Y);


                        maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(0).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(0).Y * 3.3, 2))), 2);
                        maxZY1 = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(2100).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(2100).Y * 3.3, 2))), 2);

                        for (int i = 1; i < 2100; i++)
                        {
                            if (Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2) > maxZY)
                            {
                                maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2);
                            }

                        }
                        for (int i = 2101; i < list1.Count; i++)
                        {
                            if (Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2) > maxZY1)
                            {
                                maxZY1 = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2);
                            }

                            max = Math.Abs(list1.ElementAt(0).Y);
                            max1 = Math.Abs(list1.ElementAt(2100).Y);

                        }
                        for (int i = 1; i < 2100; i++)
                        {
                            if (Math.Abs(list1.ElementAt(i).Y) > max)
                            {
                                max = Math.Abs(list1.ElementAt(i).Y);
                                element_i1 = i;

                            }

                        }
                        for (int i = 2101; i < list1.Count; i++)
                        {
                            if (Math.Abs(list1.ElementAt(i).Y) > max1)
                            {
                                max1 = Math.Abs(list1.ElementAt(i).Y);
                                element_i2 = i;
                            }

                        }
                        max2 = Math.Abs(list2.ElementAt(0).Y);
                        max3 = Math.Abs(list2.ElementAt(2100).Y);


                        for (int i = 1; i < 2100; i++)
                        {
                            if (Math.Abs(list2.ElementAt(i).Y) > max2)
                            {
                                max2 = Math.Abs(list2.ElementAt(i).Y);
                                element_i3 = i;
                            }

                        }
                        for (int i = 2101; i < list2.Count; i++)
                        {
                            if (Math.Abs(list2.ElementAt(i).Y) > max3)
                            {
                                max3 = Math.Abs(list2.ElementAt(i).Y);
                                element_i4 = i;
                            }

                        }
                    }
                    catch { }
                    textBox12.Text = Convert.ToString(Math.Round((list1.ElementAt(element_i1).Y * 3.3), 2));
                    textBox13.Text = Convert.ToString(Math.Round((list2.ElementAt(element_i3).Y * 3.3), 2));

                    textBox14.Text = Convert.ToString(Math.Round((list1.ElementAt(element_i2).Y * 3.3), 2));
                    textBox15.Text = Convert.ToString(Math.Round((list2.ElementAt(element_i4).Y * 3.3), 2));
                    textBox16.Text = Convert.ToString(maxZY);
                    textBox17.Text = Convert.ToString(maxZY1);
                }
                else
                /////////////////////////////////////////////////
                {
                    int element_i1 = 0;
                    int element_i2 = 0;
                    try
                    {
                        maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(0).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(0).Y * 3.3, 2))), 2);

                        for (int i = 1; i < list1.Count; i++)
                        {
                            if (Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2) > maxZY)
                            {
                                maxZY = Math.Round(System.Math.Sqrt((System.Math.Pow(list1.ElementAt(i).Y * 3.3, 2) + System.Math.Pow(list2.ElementAt(i).Y * 3.3, 2))), 2);
                            }

                        }


                        max = Math.Abs(list1[0].Y);
                        maxout = list1[0].Y;

                        for (int i = 1; i < list1.Count; i++)
                        {
                            if (Math.Abs(list1[i].Y) > max)
                            {
                                max = Math.Abs(list1[i].Y);
                                max = list1[i].Y;
                                element_i1 = i;
                            }

                        }

                        max2 = Math.Abs(list2[0].Y);
                        max2out = list2[0].Y;

                        for (int i = 1; i < list2.Count; i++)
                        {
                            if (Math.Abs(list2[i].Y) > max2)
                            {
                                max2 = Math.Abs(list2[i].Y);
                                max2out = list2[i].Y;
                                element_i2 = i;
                            }

                        }

                    }
                    catch { }

                    textBox20.Text = Convert.ToString(Math.Round((maxout * 3.3), 2));
                    textBox19.Text = Convert.ToString(Math.Round((max2out * 3.3), 2));
                    textBox18.Text = Convert.ToString(maxZY);

                }
                #endregion
            }
        }
    }
}
