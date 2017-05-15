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
    public partial class Form13 : Form
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


        public Form7 form7;
        public Form3 form3;
        public Form6 form6;
        public xmlWork XMLW = new xmlWork();
        public Form9 form9 = new Form9();
        public Form5 form5;
        public SosnaY_v00.Form13.ComPort ComPorts = new ComPort();
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
        public short Kor_Z1, Kor_Y1;
        public byte Pricel, rejim2;
        public double[] StickX = { 0, 0 }; //начальные координады салазки 
        public double[] StickY = { -1.3, 1.3 };//начальные координады салазки

        public double[] StickX1 = { 0, 0 };//начальные координады салазки
        public double[] StickY1 = { -1.3, 1.3 };//начальные координады салазки

        public byte pyss; // для таймера запуска

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

        public Form13()
        {
            InitializeComponent();
        }

        public void CreateGraph(ZedGraphControl zgc)
        {
            GraphPane myPane = zgc.GraphPane;

            //// Название заголовка
            myPane.Title.Text = "";
            myPane.XAxis.Title.Text = "";
            myPane.YAxis.Title.Text = "";

            ////Задание допустимых значение на графике 
            myPane.YAxis.Scale.Min = -1.3;
            myPane.YAxis.Scale.Max = 1.3;
            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 6000;
            myPane.YAxis.Cross = 0;

            ////отрисовка сетки графика
            myPane.YAxis.MajorGrid.DashOff = 0.15f;
            myPane.XAxis.MajorGrid.DashOff = 0.15f;

            ////Создание линий
            myPane.AddStick("", StickX, StickY, Color.Chocolate);
            myPane.AddStick("", StickX1, StickY1, Color.Chocolate);

            ////задание цвета сетки
            myPane.XAxis.MajorGrid.Color = Color.LightGray;
            myPane.YAxis.MajorGrid.Color = Color.LightGray;

            ////разрешение отображения сетки
            myPane.XAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MajorGrid.IsVisible = true;

            //---------------------------------------------------------------------------------
            // Проверка значение +У на положительность
            PointPairList tlist1 = new PointPairList();

            for (int i = 0; i < list1.Count; i++)
            {
                if (ComPorts.posilca[1] == 37)
                {
                    if (list1.ElementAt(i).Y > 0)
                        tlist1.Add(i, list1.ElementAt(i).Y - Convert.ToDouble(((double)numericUpDown1.Value - XMLW.PZ) / (double)XMLW.PZ));
                    else
                        if (list1.ElementAt(i).Y < 0)
                            tlist1.Add(i, list1.ElementAt(i).Y + (Convert.ToDouble(((double)numericUpDown3.Value) + XMLW.MZ) / XMLW.MZ));
                        else
                            if (list1.ElementAt(i).Y == 0)
                                tlist1.Add(i, list1.ElementAt(i).Y);
                }
                else
                {
                    if (ComPorts.posilca[1] == 38)
                    {
                        if (list1.ElementAt(i).Y > 0)
                            tlist1.Add(i, list1.ElementAt(i).Y + Convert.ToDouble(((double)numericUpDown1.Value + XMLW.PZ) / XMLW.PZ));
                        else
                            if (list1.ElementAt(i).Y < 0)
                                tlist1.Add(i, list1.ElementAt(i).Y + (Convert.ToDouble(((double)numericUpDown3.Value) - XMLW.MZ) / XMLW.MZ));
                            else
                                if (list1.ElementAt(i).Y == 0)
                                    tlist1.Add(i, list1.ElementAt(i).Y);
                    }
                    else
                    {
                        if (list1.ElementAt(i).Y > 0)
                            tlist1.Add(i, list1.ElementAt(i).Y + Convert.ToDouble(((double)numericUpDown1.Value - XMLW.PZ) / XMLW.PZ));
                        else
                            if (list1.ElementAt(i).Y < 0)
                                tlist1.Add(i, list1.ElementAt(i).Y + (Convert.ToDouble(((double)numericUpDown3.Value) - XMLW.MZ) / XMLW.MZ));
                            else
                                if (list1.ElementAt(i).Y == 0)
                                    tlist1.Add(i, list1.ElementAt(i).Y);
                    }
                }
            }

            //---------------------------------------------------------------------------------
            // Проверка значение +Z на положительность
            PointPairList tlist2 = new PointPairList();
            for (int i = 0; i < list2.Count; i++)
            {
                if (ComPorts.posilca[1] == 37)
                {
                    if (list2.ElementAt(i).Y > 0)
                        tlist2.Add(i, list2.ElementAt(i).Y + Convert.ToDouble(((double)numericUpDown2.Value + XMLW.PY) / XMLW.PY));
                    else
                        if (list2.ElementAt(i).Y < 0)
                            tlist2.Add(i, list2.ElementAt(i).Y + Convert.ToDouble(((double)numericUpDown4.Value - XMLW.MY) / XMLW.MY));
                        else
                            if (list2.ElementAt(i).Y == 0)
                                tlist2.Add(i, list2.ElementAt(i).Y);
                }
                else
                {
                    if (ComPorts.posilca[1] == 38)
                    {
                        if (list2.ElementAt(i).Y > 0)
                            tlist2.Add(i, list2.ElementAt(i).Y - Convert.ToDouble(((double)numericUpDown2.Value - XMLW.PY) / XMLW.PY));
                        else
                            if (list2.ElementAt(i).Y < 0)
                                tlist2.Add(i, list2.ElementAt(i).Y + Convert.ToDouble(((double)numericUpDown4.Value + XMLW.MY) / XMLW.MY));
                            else
                                if (list2.ElementAt(i).Y == 0)
                                    tlist2.Add(i, list2.ElementAt(i).Y);
                    }
                    else
                    {
                        if (list2.ElementAt(i).Y > 0)
                            tlist2.Add(i, list2.ElementAt(i).Y + Convert.ToDouble(((double)numericUpDown2.Value - XMLW.PY) / XMLW.PY));
                        else
                            if (list2.ElementAt(i).Y < 0)
                                tlist2.Add(i, list2.ElementAt(i).Y + Convert.ToDouble(((double)numericUpDown4.Value - XMLW.MY) / XMLW.MY));
                            else
                                if (list2.ElementAt(i).Y == 0)
                                    tlist2.Add(i, list2.ElementAt(i).Y);
                    }
                }
            }

            //---------------------------------------------------------------------------------
            // Проверка значение P на положительность
            PointPairList tlist3 = new PointPairList();

            for (int i = 0; i < list3.Count; i++)
            {
                if (list3.ElementAt(i).Y > 0)
                	tlist3.Add(i, list3.ElementAt(i).Y - Convert.ToDouble(numericUpDown5.Value) / XMLW.P);
                else
                    tlist3.Add(i, list3.ElementAt(i).Y);
            }
            //----------------------------------------------------------------------------------------
            ////Создание 3 линий для графика

            ////  LineItem myCurve = myPane.AddCurve("Y", tlist1, Color.Red, SymbolType.None);

            myPane.AddCurve("Z", tlist1, Color.Green, SymbolType.None);
            myPane.AddCurve("Y", tlist2, Color.Blue, SymbolType.None);
            myPane.AddCurve("E", list3, Color.Orange, SymbolType.None);

            zgc.AxisChange();
        }      

        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            myTimer.Enabled = true;

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

                    //Kor_Z = (double)((double)(Kor_Z1) / (double)(1024));
                    //Kor_Y = (double)((double)(Kor_Y1) / (double)(1024));
                    //XMLW.KofVolt = Math.Round((double)((5 * Kor_Z1) / (double)1024),2);
                    if ((ComPorts.buffer[l][3] == 0x80) || (ComPorts.buffer[l][3] == 0x81) || (ComPorts.buffer[l][3] == 0x82) || (ComPorts.buffer[l][3] == 0x83))
                    {

                        Kor_Z = (double)((double)(((Kor_Z1) / (double)(1024)) - Convert.ToDouble(cmehenieZ.Value)) / (XMLW.MZ / XMLW.Const));
                        Kor_Z = Kor_Z * (-1);
                    }
                    else
                    {
                        Kor_Z = (double)((double)(((Kor_Z1) / (double)(1024)) + Convert.ToDouble(cmehenieZ.Value)) / (XMLW.PZ / XMLW.Const));
                    }

                    if ((ComPorts.buffer[l][5] == 0x80) || (ComPorts.buffer[l][5] == 0x81) || (ComPorts.buffer[l][5] == 0x82) || (ComPorts.buffer[l][5] == 0x83))
                    {
                        Kor_Y = (double)((double)(((Kor_Y1) / (double)(1024)) - Convert.ToDouble(cmehenieY.Value)) / (XMLW.MY / XMLW.Const));
                        Kor_Y = Kor_Y * (-1);
                    }
                    else
                    {
                        Kor_Y = (double)((double)(((Kor_Y1) / (double)(1024)) + Convert.ToDouble(cmehenieY.Value)) / (XMLW.PY / XMLW.Const));
                    }
                    //Kor_Z = Kor_Z + Convert.ToDouble(cmehenieZ.Value);
                    //Kor_Y = Kor_Y + Convert.ToDouble(cmehenieY.Value);
                    //if (Kor_Z > 1)
                    //{
                    //    Kor_Z = -Kor_P;
                    //}
                    list1.Add(l, Kor_Z);
                    list2.Add(l, Kor_Y);
                    list3.Add(l, Kor_P);

                 
                    number++;
                  
                }

                if (timer_25 >= 2500)
                {
                    rejim++;
                    timer_25 = 0;
                   
                    //ComPorts.Kol_poket = false;
                }
                else
                    timer_25 += 9;


                CreateGraph(Zedgraph);
                zedGraphControl1.Refresh();
                Zedgraph.Dispose();
                #endregion
            }
            else
            {
                if (ComPorts.One_Buffers == true)
                {
                    ReadBuffers1 = ComPorts.pokets.buffer1;

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
                        if (1 == 1)
                        {
                            rejim1++;
                        }
                        /* if (rastor < 1000)
                         {
                             Begin = false;
                             MessageBox.Show("Недостаточная частота растра");
                             rejim1 = 0;
                         }*/
                    }



                    #endregion

                    #region Ready
                    if (ReadBuffers1[1] == 101)
                    {
                        toolStripStatusLabel1.Text = "ГОТОВО";
//                        if (Pysk.Checked == true)
//                        {
//                            if (toolStripStatusLabel2.Text == "Shtorka_On")
//                            {
//                                rejim2++;
//                            }
//                            if (toolStripStatusLabel2.Text == "Laser_On")
//                            {
//                                rejim2++;
//                            }
//                        }

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
                    ComPorts.One_Buffers = false;

                    //CreateGraph(Zedgraph);
                    //zedGraphControl1.Refresh();
                    //Zedgraph.Dispose();
                }
            }



            #endregion
        }

        private void Form13_FormClosing(object sender, FormClosingEventArgs e)
        {
            ComPorts.port.Close();
            form5.Show();
            this.Dispose();
        }

        private void PZMY_CheckedChanged(object sender, EventArgs e)
        {
            first_click++;
            if (first_click == 1)
            {
                myTimer.Start();
                #region
                timer_25 = 0;
                PZMY.Text = "СТОП";
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

                //XMLW.endpropise = false;
                MZPY.Enabled = false;
                Zero.Enabled = false;

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
                System.Threading.Thread.Sleep(20);

                ComPorts.posilca[1] = 37;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 675;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "TAR_PZMY";
                ComPorts.pokets.poket1 = 1;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();
                numericUpDown1.Enabled = true;
                numericUpDown4.Enabled = true;
                #endregion
            }
            if (first_click == 2)
            {
                #region
                ComPorts.posilca[1] = 40;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "Off_TAR";
                ComPorts.pokets.poket1 = 0;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();
                System.Threading.Thread.Sleep(100);

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


                //ComPorts.posilca[1] = 14;
                //ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                //ComPorts.size_Off = 3;
                //ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                //ComPorts.size_On = 3;
                //toolStripStatusLabel1.Text = " ";
                //toolStripStatusLabel2.Text = "Off_DP_Off";
                //ComPorts.pokets.poket1 = 0;
                //ComPorts.port.DiscardInBuffer();
                //ComPorts.port.DiscardOutBuffer();
                //ComPorts.Kol_poket = false;
                //ComPorts.Poket_Write();
                //System.Threading.Thread.Sleep(300);
                //first_click = 0;

                #endregion
                first_click = 0;
                PZMY.Text = "+Z         -Y";
                MZPY.Enabled = true;
                Zero.Enabled = true;
                numericUpDown1.Enabled = false;
                numericUpDown4.Enabled = false;
                ComPorts.Kol_poket = false;
                myTimer.Stop();
            }
        }

        private void MZPY_CheckedChanged(object sender, EventArgs e)
        {
            first_click++;
            if (first_click == 1)
            {
                myTimer.Start();
                timer_25 = 0;
                MZPY.Text = "СТОП";
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

                //XMLW.endpropise = false;
                PZMY.Enabled = false;
                Zero.Enabled = false;

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
                System.Threading.Thread.Sleep(20);

                ComPorts.posilca[1] = 38;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 675;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "TAR_MZPY";
                ComPorts.pokets.poket1 = 1;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();
                numericUpDown2.Enabled = true;
                numericUpDown3.Enabled = true;

            }
            if (first_click == 2)
            {
                #region
                ComPorts.posilca[1] = 40;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "Off_TAR";
                ComPorts.pokets.poket1 = 0;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();
                System.Threading.Thread.Sleep(100);

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


                //ComPorts.posilca[1] = 14;
                //ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                //ComPorts.size_Off = 3;
                //ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                //ComPorts.size_On = 3;
                //toolStripStatusLabel1.Text = " ";
                //toolStripStatusLabel2.Text = "Off_DP_Off";
                //ComPorts.pokets.poket1 = 0;
                //ComPorts.port.DiscardInBuffer();
                //ComPorts.port.DiscardOutBuffer();
                //ComPorts.Kol_poket = false;
                //ComPorts.Poket_Write();
                //System.Threading.Thread.Sleep(300);
                //first_click = 0;

                #endregion

                first_click = 0;

                MZPY.Text = "-Z         +Y";
                PZMY.Enabled = true;
                Zero.Enabled = true;
                numericUpDown2.Enabled = false;
                numericUpDown3.Enabled = false;
                ComPorts.Kol_poket = false;
                myTimer.Stop();
            }
        }

        private void Zero_CheckedChanged(object sender, EventArgs e)
        {
            first_click++;
            if (first_click == 1)
            {
                myTimer.Start();
                timer_25 = 0;
                Zero.Text = "СТОП";
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
                XMLW.endpropise = false;
                MZPY.Enabled = false;
                PZMY.Enabled = false;

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
                System.Threading.Thread.Sleep(20);

                ComPorts.posilca[1] = 39;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 675;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "TAR_ZERO";
                ComPorts.pokets.poket1 = 1;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();

            }
            if (first_click == 2)
            {
                #region
                ComPorts.posilca[1] = 40;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "Off_TAR";
                ComPorts.pokets.poket1 = 0;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();
                System.Threading.Thread.Sleep(100);

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


                //ComPorts.posilca[1] = 14;
                //ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                //ComPorts.size_Off = 3;
                //ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                //ComPorts.size_On = 3;
                //toolStripStatusLabel1.Text = " ";
                //toolStripStatusLabel2.Text = "Off_DP_Off";
                //ComPorts.pokets.poket1 = 0;
                //ComPorts.port.DiscardInBuffer();
                //ComPorts.port.DiscardOutBuffer();
                //ComPorts.Kol_poket = false;
                //ComPorts.Poket_Write();
                //System.Threading.Thread.Sleep(300);
                //first_click = 0;

                #endregion
                first_click = 0;
                Zero.Text = "0Z     0Y";
                MZPY.Enabled = true;
                PZMY.Enabled = true;
                myTimer.Stop();
            }
        }

        private void zedGraphControl1_Paint(object sender, PaintEventArgs e)
        {
            zedGraphControl1.GraphPane.YAxis.Scale.Min = -1.3;
            zedGraphControl1.GraphPane.YAxis.Scale.Max = 1.3;

            zedGraphControl1.GraphPane.YAxis.Scale.MajorStep = 0.1f;
            //zedGraphControl1.GraphPane.XAxis.Scale.MajorStep = 200;
            //zedGraphControl1.GraphPane.XAxis.Scale.Format = "f3";
            //zedGraphControl1.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XScaleFormatEvent);
            zedGraphControl1.GraphPane.XAxis.Scale.Min = 0;
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
        
        private void Form13_Load(object sender, EventArgs e)
        {
            ComPorts.posilca[0] = 170;
            ComPorts.size_Off = 3;
            ComPorts.ComInitializ();
            myTimer.Tick += new EventHandler(TimerEventProcessor);
            myTimer.Interval = 10;
            //myTimer.Start();
            //Pysk.Enabled = false;
            numericUpDown1.Enabled = false;
            numericUpDown2.Enabled = false;
            numericUpDown3.Enabled = false;
            numericUpDown4.Enabled = false;
            numericUpDown5.Enabled = false;
            
           XMLW.XmlConfigRead();
           numericUpDown1.Value = Convert.ToDecimal(XMLW.PZ);
           numericUpDown3.Value = Convert.ToDecimal(XMLW.MZ);
           numericUpDown2.Value = Convert.ToDecimal(XMLW.PY);
           numericUpDown4.Value = Convert.ToDecimal(XMLW.MY);
           numericUpDown5.Value = Convert.ToDecimal(XMLW.P);
           cmehenieZ.Value = Convert.ToDecimal(XMLW.Cmehenie_Z);
           cmehenieY.Value = Convert.ToDecimal(XMLW.Cmehenie_Y);
        }
         
     	void Button1Click(object sender, EventArgs e)
        {
        	XMLW.XmlConfigRead();
            XMLW.SaveXML(Convert.ToString(XMLW.T), Convert.ToString(XMLW.Fstart), Convert.ToString(XMLW.Fend), Convert.ToString(XMLW.D), Convert.ToString(XMLW.Tay), Convert.ToString(XMLW.V), Convert.ToString(numericUpDown1.Value), Convert.ToString(numericUpDown3.Value), Convert.ToString(numericUpDown2.Value), Convert.ToString(numericUpDown4.Value), Convert.ToString(numericUpDown5.Value), XMLW.COM, Convert.ToString(XMLW.stend), Convert.ToString(cmehenieZ.Value), Convert.ToString(cmehenieY.Value));
        }
    }
}
