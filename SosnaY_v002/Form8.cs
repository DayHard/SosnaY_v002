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
    public partial class Form8 : Form
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
            public uint sboy_poket, null_poket;
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
                                            null_poket++;
                                        }
                                        else
                                            sboy_poket++;
                                    }
                                }
                                else
                                {
                                    if (priem[i + 1] == 101)
                                    {
                                        port.ReceivedBytesThreshold = 675;
                                    }
                                    if ((priem[i] == 0) & (priem[i + 1] == 0) & (priem[i + 2] == 0) & (priem[i + 3] == 0) & (priem[i + 4] == 0) & (priem[i + 5] == 0) & (priem[i + 6] == 0) & (priem[i + 7] == 0) & (priem[i + 8] == 0))
                                    {
                                        null_poket++;
                                    }
                                    else
                                        sboy_poket++;
                                }
                            }
                            else
                            {
                               
                                if ((priem[i] == 0) & (priem[i + 1] == 0) & (priem[i + 2] == 0) & (priem[i + 3] == 0) & (priem[i + 4] == 0) & (priem[i + 5] == 0) & (priem[i + 6] == 0) & (priem[i + 7] == 0) & (priem[i + 8] == 0))
                                {
                                  null_poket++;
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


        public bool tolerance = false;
        public int MouseClick = 0;
        PointD pointMin, pointCenter, pointMax;
        public string zagolovok;
        public double[] StickX = { 0, 0 }; //начальные координады салазки 
        public double[] StickY = { -1.1, 1.1 };//начальные координады салазки
        public double[] StickX1 = { 0, 0 };//начальные координады салазки
        public double[] StickY1 = { -1.1, 1.1 };//начальные координады салазки
        
        //public PointPairList list1 = new PointPairList();
        //public PointPairList list2 = new PointPairList();
        //public PointPairList list3 = new PointPairList();

        public ComPort ComPorts = new ComPort();
        public int[] ReadBuffers1 = new int[10];
        byte Pankr, rejim, rejim1;
        static System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();
        public PointPairList list1 = new PointPairList();
        public PointPairList list2 = new PointPairList();
        public PointPairList list3 = new PointPairList();
        public double Kor_Z, Kor_Y, Kor_P;
        public short Kor_Z1, Kor_Y1;
        public byte first_click, time, rejim2, Pricel, rejim3;
        public short time_muve_pankr, time_begin_sniatia_prevish, time_sniatia_prevish, time_vozvrata;
        public byte[] In_buffer = new byte[13];
        public byte[] Out_buffer = new byte[16];
        public int[][] ReadBuffers = new int[1000][];
        public bool PricelBefore;
        Form4 form4 = new Form4();
        public bool thReadPoket, Begin, Begin1, zadL, zadDP, Begin2;
        public long number, number_dorisovka, l, timer_25;
        public byte[] GetStatus_Byte1 = new byte[8];
        public byte[] GetStatus_Byte2 = new byte[8];

        public bool endpropise;
        public bool GraphMax;
        
        #endregion

        public Form8()
        {
            InitializeComponent();

          
        }

        private void Form8_FormClosing(object sender, FormClosingEventArgs e)
        {
            ComPorts.posilca[1] = 14;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.size_On = 3;
            toolStripStatusLabel1.Text = " ";
            toolStripStatusLabel2.Text = "Off_DP_Off";
            ComPorts.pokets.poket1 = 0;
            ComPorts.Kol_poket = false;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            toolStripStatusLabel3.Text = "Выключение блонировки ПС";
            ComPorts.Poket_Write();
            if ((Color.LightGreen == Save.BackColor) && (Color.LightGreen == добавитьВОтчетToolStripMenuItem.BackColor))
            {
                if ("1" == form6.pokaz.Text)
                {
                    form6.button5.BackColor = Color.LightGreen;
                }
                else
                {
                    form6.button6.BackColor = Color.LightGreen;
                }
                Save.Text = "Сохранить результат";
                Save.BackColor = Color.FromKnownColor(KnownColor.Control);
                добавитьВОтчетToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                добавитьВОтчетToolStripMenuItem.Text = "Добавить в отчет";
                 ComPorts.posilca[1] = 14;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "Off_DP_Off";
                ComPorts.pokets.poket1 = 0;
                ComPorts.Kol_poket = false;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                toolStripStatusLabel3.Text = "Выключение блонировки ПС";
                ComPorts.Poket_Write();
                ComPorts.port.Close();
                myTimer.Stop();
                form6.Show();
                this.Dispose();

            }
            else
            {
                if ((Save.BackColor != Color.LightGreen) && (добавитьВОтчетToolStripMenuItem.BackColor != Color.LightGreen))
                {
                    if (MessageBox.Show("Хотите выйти не сохраняя результат и не добавляя данные в отчет?", "Выход", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Save.BackColor = Color.FromKnownColor(KnownColor.Control);
                        Save.Text = "Cохранить результат";
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
                    if (Save.BackColor != Color.LightGreen)
                    {
                        if (MessageBox.Show("Хотите выйти не сохраняя результат?", "Выход", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            Save.BackColor = Color.FromKnownColor(KnownColor.Control);
                            Save.Text = "Cохранить результат";
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
                                Save.BackColor = Color.FromKnownColor(KnownColor.Control);
                                Save.Text = "Cохранить результат";
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
            

/*            if ("1" == form6.pokaz.Text)
            {
                textBox4.Text = " ";
                textBox5.Text = " ";
            }
            else
            {
                textBox6.Text = " ";
                textBox7.Text = " ";
            }
            rejim3 = 0;
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
            Pankr = 0;
  */      }

        public void CreateGraph(ZedGraphControl zgc)
        {
            GraphPane myPane = zgc.GraphPane;

            myPane.Title.FontSpec.Size = 12;

            //// Название заголовка
            myPane.Title.Text = this.Text;
            myPane.XAxis.Title.Text = "";
            myPane.YAxis.Title.Text = "";

            ////Задание допустимых значение на графике 
            myPane.YAxis.Scale.Min = -1.3;
            myPane.YAxis.Scale.Max = 1.3;
            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = GraphMax == true ? 1000 : XMLW.trBarMax; //XMLW.endpropise == true ? (list1.Count + 1) : 4200;
            //myPane.XAxis.Scale.Max = list3.Count + 100;
            myPane.XAxis.Scale.MajorStep = 100;
            //if (list1.Count > 200)
            //{
            //    myPane.XAxis.Scale.Min = list1.Count - 200;
            //    myPane.XAxis.Scale.Max = list1.Count;
            ////    myPane.XAxis.Scale.MajorStep = Math.Floor(list1.Count/4.0); //из-за этого сильные тормоза
            //}
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

            ////---------------------------------------------------------------------------------
            //// Проверка значение +У на положительность
            //PointPairList tlist1 = new PointPairList();

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
            //                    tlist1.Add(i, list1.ElementAt(i).Y + (Convert.ToDouble((numericUpDown3.Value) + 5) / 5));
            //                else
            //                    if (list1.ElementAt(i).Y == 0)
            //                        tlist1.Add(i, list1.ElementAt(i).Y);
            //        }
            //    }
            //}

            ////---------------------------------------------------------------------------------
            //// Проверка значение +Z на положительность
            //PointPairList tlist2 = new PointPairList();
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

            ////---------------------------------------------------------------------------------
            //// Проверка значение P на положительность
            //PointPairList tlist3 = new PointPairList();

            //for (int i = 0; i < list3.Count; i++)
            //{
            //    if (list3.ElementAt(i).Y > 0)
            //        tlist3.Add(i, list3.ElementAt(i).Y - Convert.ToDouble(numericUpDown5.Value / 5));
            //    else
            //        tlist3.Add(i, list3.ElementAt(i).Y);
            //}
            ////----------------------------------------------------------------------------------------
            ////Создание 3 линий для графика

            ////  LineItem myCurve = myPane.AddCurve("Y", tlist1, Color.Red, SymbolType.None);

            myPane.AddCurve("Z", list1, Color.Green, SymbolType.None);
            myPane.AddCurve("Y", list2, Color.Blue, SymbolType.None);
            myPane.AddCurve("E", list3, Color.Orange, SymbolType.None);

            zgc.AxisChange();
        }

        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            #region Прицеливание
            if ((checkBox1.Checked == true) || (checkBox2.Checked == true))
            {
               
                if ("1" == form6.pokaz.Text)
                {
                    #region Прицеливание
                    switch (rejim2)
                    {
                        case 0:
                            if (Pricel == 0)
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
                                toolStripStatusLabel3.Text = "Открытие шторки";
                                System.Threading.Thread.Sleep(300);
                                Pricel = 1;
                            }
                            break;
                        case 1:
                            if (Pricel == 1)
                            {
                                ComPorts.posilca[1] = 11;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "Laser_On";
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.Kol_poket = false;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();
                                toolStripStatusLabel3.Text = "Включение лазера";
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
                                toolStripStatusLabel3.Text = " ";
                                Pricel = 3;
                            }
                            break;
                    }
                    #endregion
                }

                if ("2" == form6.pokaz.Text)
                {
                    #region Прицеливание
                    switch (rejim2)
                    {
                        case 0:
                            if (Pricel == 0)
                            {
                                //ComPorts.posilca[1] = 14;
                                //ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                //ComPorts.size_Off = 3;
                                //ComPorts.size_On = 3;
                                //toolStripStatusLabel1.Text = " ";
                                //toolStripStatusLabel2.Text = "Off_DP_Off";
                                //ComPorts.pokets.poket1 = 0;
                                //ComPorts.Kol_poket = false;
                                //ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                //toolStripStatusLabel3.Text = "Выключение блонировки ПС";
                                //ComPorts.Poket_Write();
                                //System.Threading.Thread.Sleep(300);
                                rejim2++;
                                Pricel = 1;
                            }
                            break;
                        case 1:
                            if (Pricel == 1)
                            {
                                if (PricelBefore == false)
                                {
                                    ComPorts.posilca[1] = 19;
                                    ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                    ComPorts.size_Off = 3;
                                    ComPorts.size_On = 3;
                                    toolStripStatusLabel1.Text = " ";
                                    toolStripStatusLabel2.Text = "Pankr_End_On";
                                    ComPorts.pokets.poket1 = 0;
                                    ComPorts.Kol_poket = false;
                                    ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                    toolStripStatusLabel3.Text = "Перевод ПС в конечное положение";
                                    ComPorts.Poket_Write();
                                }
                                else
                                    rejim2++;

                                Pricel = 2;
                            }
                            break;
                        case 2:
                            if (Pricel == 2)
                            {
                                System.Threading.Thread.Sleep(200);
                                ComPorts.posilca[1] = 11;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "Laser_On";
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.Kol_poket = false;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                toolStripStatusLabel3.Text = "Включение лазера";
                                ComPorts.Poket_Write();
                                Pricel = 3;
                            }
                            break;
                        case 3:
                            if (Pricel == 3)
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
                                toolStripStatusLabel3.Text = " ";
                                ComPorts.Poket_Write();
                                Pricel = 5;
                                PricelBefore = true;
                            }
                            break;
                    }
                    #endregion
                }
            }
            #endregion

            #region Настройка прибора
            if (Begin2 == true)
            {
                if ("1" == form6.pokaz.Text)
                {
                    #region Время открытия шторки и начала движения панкратики
                    switch (rejim3)
                    {
                        case 0:
                            if (Pankr == 0)
                            {
                                ComPorts.posilca[1] = 27;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "BPO_START";
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.Kol_poket = false;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                toolStripStatusLabel3.Text = "Перевод БПО в начальное положение";
                                ComPorts.Poket_Write();
                                Pankr = 1;
                            }
                            break;
                        case 1:
                            if (Pankr == 1)
                            {
                                ComPorts.posilca[1] = 14;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "Off_DP_Off";
                                ComPorts.Kol_poket = false;
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();
                                toolStripStatusLabel3.Text = "Выключение блокировки панкратики";
                                Pankr = 2;
                                System.Threading.Thread.Sleep(300);
                                toolStripStatusLabel3.Text = " ";
                            }
                            break;
                    }
                    #endregion
                }
                if ("2" == form6.pokaz.Text)
                {
                    #region Время движения панкратики и излучения лазера
                    switch (rejim3)
                    {
                        case 0:
                            if (Pankr == 0)
                            {
                                ComPorts.posilca[1] = 28;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                ComPorts.pokets.poket1 = 0;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "BPO_END";
                                ComPorts.Kol_poket = false;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                toolStripStatusLabel3.Text = "Перевод БПО в конечное положение";
                                ComPorts.Poket_Write();
                                Pankr = 1;
                            }
                            break;
                        case 1:
                            if (Pankr == 1)
                            {
                                ComPorts.posilca[1] = 14;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "Off_DP_Off";
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.Kol_poket = false;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();
                                toolStripStatusLabel3.Text = "Выключение блокировки панкратики";
                                System.Threading.Thread.Sleep(200);
                                Pankr = 2;
                                button7.Enabled = true;
                                toolStripStatusLabel3.Text = " ";
                            }
                            break;
                    }
                    #endregion
                }
            }
            #endregion

            #region Проверка
            if (Begin1 == true)
            {
                if ("1" == form6.pokaz.Text)
                {
                    #region Время открытия шторки и начала движения панкратики
                    switch (rejim)
                    {
                        case 0:
                            if (Pankr == 0)
                            {
                                ComPorts.posilca[1] = 16;
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "Shtorka_On";
                                ComPorts.port.DiscardInBuffer();
                                ComPorts.port.DiscardOutBuffer();
                                ComPorts.Kol_poket = false;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();
                                System.Threading.Thread.Sleep(200);
                                Pankr = 1;
                            }
                            break;
                        case 1:
                            if (Pankr == 1)
                            {
                                ComPorts.posilca[1] = 11;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "Laser_On";
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
                                Pankr = 2;
                            }
                            break;
                        case 2:
                            if (Pankr == 2)
                            {
                                System.Threading.Thread.Sleep(1500);
                                ComPorts.posilca[1] = 25;
                                ComPorts.pokets.poket1 = 1;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 675;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "ADC_On";
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();

                                ComPorts.posilca[1] = 17;
                                // ComPorts.pokets.poket1 = 0;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                // ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                //toolStripStatusLabel2.Text = "Shod";
                                //  ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                // ComPorts.Kol_poket = false;
                                ComPorts.Poket_Write();
                                System.Threading.Thread.Sleep(8);
                                ComPorts.port.Read(ComPorts.priem, 0, 3);

                                Pankr = 3;
                            }
                            break;
                        case 3:
                            if (Pankr == 3)
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


                                ComPorts.posilca[1] = 26;
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "ADC_Off";
                                ComPorts.Kol_poket = false;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();

                               

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
                                System.Threading.Thread.Sleep(300);
                                Begin1 = false;
                                checkBox1.Enabled = true;
                                checkBox2.Enabled = true;
                                button10.Enabled = true;
                                Start_time.Enabled = true;
                                button4.Enabled = true;
                                button5.Enabled = true;
                                Pankr = 4;
                            }
                            break;
                    }
                    #endregion
                }
                if ("2" == form6.pokaz.Text)
                {
                    #region Время движения панкратики и излучения лазера
                    switch (rejim)
                    {
                        case 0:
                            if (Pankr == 0)
                            {
                                ComPorts.posilca[1] = 11;
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "Laser_On";
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Kol_poket = false;
                                if (XMLW.stend == true)
                                {
                                    rejim++;
                                }
                                else
                                {
                                    ComPorts.Poket_Write();
                                }
                                Pankr = 1;
                            }
                            break;
                        case 1:
                            if (Pankr == 1)
                            {



                               // System.Threading.Thread.Sleep(1500);
                                ComPorts.posilca[1] = 25;
                                ComPorts.pokets.poket1 = 1;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 675;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "ADC_On";
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();


                                ComPorts.posilca[1] = 17;
                                // ComPorts.pokets.poket1 = 0;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                // ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                //toolStripStatusLabel2.Text = "Shod";
                                //  ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                // ComPorts.Kol_poket = false;
                                ComPorts.Poket_Write();
                                System.Threading.Thread.Sleep(8);
                                ComPorts.port.Read(ComPorts.priem, 0, 3);


                                


                                
                                Pankr = 2;
                            }
                            break;
                        case 2:
                            if (Pankr == 2)
                            {
                                ComPorts.posilca[1] = 26;
                                ComPorts.pokets.poket1 = 0;
                                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                                ComPorts.size_Off = 3;
                                ComPorts.size_On = 3;
                                toolStripStatusLabel1.Text = " ";
                                toolStripStatusLabel2.Text = "ADC_Off";
                                ComPorts.Kol_poket = false;
                                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                                ComPorts.Poket_Write();

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
                                System.Threading.Thread.Sleep(300);

                                Pankr = 4;
                                checkBox1.Enabled = true;
                                checkBox2.Enabled = true;
                                Start_time.Enabled = true;
                                button7.Enabled = true;
                                button10.Enabled = true;
                                Begin1 = false;
                            }
                            break;
                    }
                    #endregion
                }
            }
            #endregion

          
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
                    Kor_P = (double)((double)(((ComPorts.buffer[l][7] << 8) & 0x7F00) | ComPorts.buffer[l][6]) / (double)1024);

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
                    
                    list1.Add(l, Kor_Z);
                    list2.Add(l, Kor_Y);
                    list3.Add(l, Kor_P);

                    if (Begin1 == true)
                    {
                        if ("1" == form6.pokaz.Text)
                        {
                            if (l >= 990)
                            {
                                canselPr();
                                timer_25 = 0;
                                XMLW.endpropise = true;
                            }
                        }
                        if ("2" == form6.pokaz.Text)
                        {
                            if (l >= 4195)
                            {
                                canselPr();
                                timer_25 = 0;
                                XMLW.endpropise = true;
                            }
                        }
                    }
                    
                    label3.Text= (Convert.ToString(Kor_P));
                    number++;
                }
                //if (Begin1 == true)
                //{
                //    if ("1" == form6.pokaz.Text)
                //    {
                //        if (timer_25 >= 600)
                //        {
                //            rejim++;
                //            timer_25 = 0;
                //            XMLW.endpropise = true;
                //        }
                //        else
                //            timer_25 += 10;
                //    }
                //    if ("2" == form6.pokaz.Text)
                //    {
                //        if (timer_25 >= 2800)
                //        {
                //            rejim++;
                //            timer_25 = 0;
                //            XMLW.endpropise = true;
                //        }
                //        else
                //            timer_25 += 10;
                //    }
                //}
                if (checkBox1.Checked == true) 
                {
                    if(timer_25 >= 1000)
                    {
                        checkBox1.Checked = false;
                        XMLW.endpropise = true;
                        timer_25 = 0;
                    }
                    else
                        timer_25 += 10;
                }
                else
                    if (checkBox2.Checked == true)
                    {
                        if (timer_25 >= 4200)
                        {
                            checkBox2.Checked = false;
                            XMLW.endpropise = true;
                            timer_25 = 0;
                        }
                        else
                            timer_25 += 10;
                    }



                trackBar1.Maximum = XMLW.trBarMax;
                trackBar2.Maximum = XMLW.trBarMax;
                
                trackBar1.Enabled = true;
                trackBar2.Enabled = true;
                #endregion

                //toolStripStatusLabel3.Text = Convert.ToString(ComPorts.null_poket);
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
                      
                         if ("1" == form6.pokaz.Text)
                         {
                             if ((GetStatus_Byte1[0] == 1) && (GetStatus_Byte2[5] == 1))
                             {
                                 rejim++;
                             }
                         }
                        if ("2" == form6.pokaz.Text)
                        {
                            if ((GetStatus_Byte1[0] == 1) && (GetStatus_Byte2[5] == 1))
                            {
                                rejim++;
                            }
                        }


                        #endregion
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

                        //rastor = (double)((((ReadBuffers1[4] << 8) & 0x7F00) | ReadBuffers1[3]));
                      
                        //if ((rastor < 480) || (rastor > 520))
                        //{
                        //    Begin = false;
                        //    MessageBox.Show("Недостаточная частота растра");
                        //    rejim1 = 0;
                        //}
                    }



                    #endregion
                   

                    #endregion

                    #region Ready
                    if (ReadBuffers1[1] == 101)
                    {
                        toolStripStatusLabel1.Text = "ГОТОВО";
                        #region Время открытия шторки и начала движения панкратики

                            if ("1" == form6.pokaz.Text)
                            {
                                if ((checkBox1.Checked == true))
                                {
                                    if (toolStripStatusLabel2.Text == "Laser_On")
                                    {
                                        rejim2++;
                                    }
                                    if (toolStripStatusLabel2.Text == "Shtorka_On")
                                    {
                                        rejim2++;
                                    }
                                }
                                if (Begin1 == true)
                                {
                                    if (toolStripStatusLabel2.Text == "Laser_On")
                                    {
                                        rejim++;
                                    }
                                    if (toolStripStatusLabel2.Text == "Shtorka_On")
                                    {
                                        rejim++;
                                    }
                                }
                                if (Begin2 == true)
                                {
                                    if (toolStripStatusLabel2.Text == "Off_DP_Off")
                                    {
                                        rejim3++;
                                        Begin2 = false;
                                        Start_time.Enabled = true;
                                        checkBox1.Enabled = true;
                                    }
                                    if (toolStripStatusLabel2.Text == "BPO_START")
                                    {
                                        rejim3++;
                                    }
                                }
                            }

                            #endregion
                        
                        #region Время движения панкратики и излучения лазера
                            if ("2" == form6.pokaz.Text)
                            {
                                if ((checkBox1.Checked == true) || (checkBox2.Checked == true))
                                {
                                    if (toolStripStatusLabel2.Text == "Pankr_End_On")
                                    {
                                        rejim2++;
                                    }
                                    if (toolStripStatusLabel2.Text == "Laser_On")
                                    {
                                        rejim2++;
                                    }
                                    if (toolStripStatusLabel2.Text == "Return")
                                    {
                                        rejim2++;
                                    }
                                    if (toolStripStatusLabel2.Text == "Off_DP_Off")
                                    {
                                        rejim2++;
                                    }
                                }
                                if (Begin1 == true)
                                {
                                    if (toolStripStatusLabel2.Text == "BPO_END")
                                    {
                                        rejim++;
                                    }
                                    if (toolStripStatusLabel2.Text == "Shod")
                                    {
                                        rejim++;
                                    }
                                    if (toolStripStatusLabel2.Text == "Laser_On")
                                    {
                                        rejim++;
                                    }
                                    if (toolStripStatusLabel2.Text == "Return")
                                    {
                                        rejim++;
                                    }
                                    if (toolStripStatusLabel2.Text == "ADC_Off")
                                    {
                                        rejim++;
                                    }
                                    if (toolStripStatusLabel2.Text == "Shtorka_On")
                                    {
                                        rejim++;
                                    }
                                    if (toolStripStatusLabel2.Text == "Shtorka_Off")
                                    {
                                        rejim++;
                                    }
                                    if (toolStripStatusLabel2.Text == "Off_DP_Off")
                                    {
                                        rejim++;
                                    }
                                    if (toolStripStatusLabel2.Text == "Pankr_Off_On")
                                    {
                                        rejim++;
                                    }
                                }
                                if (Begin2 == true)
                                {
                                    if (toolStripStatusLabel2.Text == "Off_DP_Off")
                                    {
                                        rejim3++;
                                        Begin2 = false;
                                        button7.Enabled = true;
                                        checkBox2.Enabled = true;
                                    }
                                    if (toolStripStatusLabel2.Text == "BPO_END")
                                    {
                                        rejim3++;
                                    }
                                }
                            }
                            #endregion
                    }
                  

                    ComPorts.One_Buffers = false;
                }
                     #endregion
            }
            CreateGraph(Zedgraph);
            zedGraphControl1.Refresh();
            Zedgraph.Dispose();
        }

        private void SetSize()
        {
            try
            {
                splitContainer1.Size = new Size(ClientRectangle.Width, ClientRectangle.Height);
                splitContainer1.SplitterDistance = ClientRectangle.Width - 230;
                zedGraphControl1.Size = new Size(splitContainer1.Panel1.Width - 20, splitContainer1.Panel1.Height - 110);

                trackBar1.Top = zedGraphControl1.Bottom; //zedGraphControl1.Location.X + zedGraphControl1.Height;
                trackBar2.Top = trackBar1.Bottom;        //trackBar1.Top + trackBar2.Height;


                string res = Convert.ToString(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width);

                if (res == "1152")
                {
                    trackBar1.Size = new Size(zedGraphControl1.Width - 75, trackBar1.Height);
                    trackBar2.Size = new Size(zedGraphControl1.Width - 75, trackBar2.Height);

                    trackBar1.Left = zedGraphControl1.Left + 65;
                    trackBar2.Left = zedGraphControl1.Left + 65;
                }
                if (res == "1024")
                {
                    trackBar1.Size = new Size(zedGraphControl1.Width - 60, trackBar1.Height);
                    trackBar2.Size = new Size(zedGraphControl1.Width - 60, trackBar2.Height);

                    trackBar1.Left = zedGraphControl1.Left + 50;
                    trackBar2.Left = zedGraphControl1.Left + 50;
                }
                if (res == "1280")
                {
                    trackBar1.Size = new Size(zedGraphControl1.Width - 100, trackBar1.Height);
                    trackBar2.Size = new Size(zedGraphControl1.Width - 100, trackBar2.Height);

                    trackBar1.Left = zedGraphControl1.Left + 80;
                    trackBar2.Left = zedGraphControl1.Left + 80;
                }
                
                //trackBar1.Size = new Size(zedGraphControl1.Width - 100, trackBar1.Height);
                //trackBar2.Size = new Size(zedGraphControl1.Width - 100, trackBar2.Height);

                //trackBar1.Left = zedGraphControl1.Left + 80;
                //trackBar2.Left = zedGraphControl1.Left + 80;

                //splitContainer1.Panel2.BackColor = Color.Red;
                //groupBox2.BackColor = Color.Plum;

                trackBar1.Maximum = XMLW.trBarMax;
                trackBar2.Maximum = XMLW.trBarMax;

                groupBox2.Size = new Size(splitContainer1.Panel2.Width - 5, splitContainer1.Panel1.Height-50);
                groupBox3.Size = new Size(splitContainer1.Panel2.Width - 5, splitContainer1.Panel1.Height - 53);
                button6.Top = splitContainer1.Panel2.Bottom - 916;
               
                textBox2.Top = splitContainer1.Panel2.Bottom - 500;
                textBox8.Top = splitContainer1.Panel2.Bottom - 500;
                button10.Top = splitContainer1.Panel2.Bottom - 916;
                groupBox1.Top = splitContainer1.Panel2.Bottom - 155;
                groupBox4.Top = splitContainer1.Panel2.Bottom - 155;
            }
            catch
            {

            }
        }

        public void canselPr()
        {
            button6.Enabled = false;
            button7.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            checkBox2.Enabled = false;


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

            ComPorts.posilca[1] = 26;
            ComPorts.pokets.poket1 = 0;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.size_On = 3;
            toolStripStatusLabel1.Text = " ";
            toolStripStatusLabel2.Text = "ADC_Off";
            ComPorts.Kol_poket = false;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
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
            System.Threading.Thread.Sleep(10);

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
            System.Threading.Thread.Sleep(10);

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
            System.Threading.Thread.Sleep(500);

           // list1.Clear();
           // list2.Clear();
           // list3.Clear();
           // textBox6.Text = "0";
           // textBox6.Refresh();
           // textBox7.Text = "0";
           // textBox7.Refresh();

           // textBox4.Text = "0";
           // textBox4.Refresh();
           // textBox5.Text = "0";
           // textBox5.Refresh();

           // for (int i = 0; i < number; i++)
           // {
           //     for (int j = 0; j < 9; j++)
           //     {
           //         ComPorts.buffer[i][j] = 0;
           //    }

           // }
            number = 0;
            ComPorts.schet_poket1 = 0;
            ComPorts.schet_poket = 0;
            //ZedGraphControl Zedgraph = new ZedGraphControl();
            //Zedgraph.Size = zedGraphControl1.Size;
            //zedGraphControl1.GraphPane = Zedgraph.GraphPane;
            //CreateGraph(Zedgraph);
            //zedGraphControl1.Refresh();
            //Zedgraph.Dispose();
            rejim = 0;
            Pankr = 0;
            timer_25 = 0;
            Start_time.Enabled = false;
            button7.Enabled = false;
            Begin1 = false;

            button4.Enabled = true;
            button5.Enabled = true;
            button7.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = true;
            checkBox2.Enabled = true;

            checkBox1.Enabled = true;
            Start_time.Enabled = true;
            button10.Enabled = true;
            button6.Enabled = true;
        }

        private void Form8_Load(object sender, EventArgs e)
        {
            zagolovok = this.Text;
            this.Text = zagolovok + " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")";
            XMLW.XmlConfigRead();
            toolStripStatusLabel1.Visible = false;
            toolStripStatusLabel2.Visible = false;
            toolStripStatusLabel3.Text = "Подождите! Идет настройка программы.";
            ComPorts.posilca[0] = 170;
            ComPorts.size_Off = 3;
            rejim = 0;
            rejim1 = 0;
            ComPorts.ComInitializ();
            first_click = 0;
            Begin2 = true;
            myTimer.Tick += new EventHandler(TimerEventProcessor);
            myTimer.Interval = 10;
            myTimer.Start();
            rejim3 = 0;
            ZedGraphControl Zedgraph = new ZedGraphControl();
            Zedgraph.Size = zedGraphControl1.Size;
            zedGraphControl1.GraphPane = Zedgraph.GraphPane;
            CreateGraph(Zedgraph);
            zedGraphControl1.Refresh();
            Zedgraph.Dispose();
            checkBox2.Enabled = false;
            button7.Enabled = false;
            checkBox1.Enabled = false;
            Start_time.Enabled = false;
            if ("1" == form6.pokaz.Text)
            {
                trackBar1.Maximum = 800;
                trackBar2.Maximum = 800;
                trackBar2.Visible = false;

            }
            else
            {
                trackBar1.Maximum = 6000;
                trackBar2.Maximum = 6000;
                trackBar2.Visible = false;
            }


            trackBar1.Value = 0;
            trackBar2.Value = 0;
            StickX[0] = 0;
            StickX[1] = 0;
            StickX1[0] = 0;
            StickX1[1] = 0;

            XMLW.endpropise = false;

        }

        private void button9_Click(object sender, EventArgs e)
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
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            first_click++;
            if (first_click == 1)
            {
                XMLW.endpropise = false;
                list1.Clear();
                list2.Clear();
                list3.Clear();
                textBox4.Text = "0";
                textBox5.Text = "0";
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
                Start_time.Enabled = false;
                button10.Enabled = false;
                Pricel = 0;
                rejim2 = 0;
                checkBox1.Text = "СТОП";

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
                System.Threading.Thread.Sleep(10);

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
           
                #endregion
                first_click = 0;
                checkBox1.Text = "ПУСК";
                timer_25 = 0;
                for (int i = 0; i < ComPorts.schet_poket; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        ComPorts.buffer[i][j] = 0; ;
                    }
                }
                Start_time.Enabled = true;
                button10.Enabled = true;
                number = 0;
                XMLW.endpropise = true;
            }

        }

        private void button10_Click(object sender, EventArgs e)
        {
            button10.Enabled = false;

            Start_time.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            checkBox1.Enabled = false;


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
            System.Threading.Thread.Sleep(100);

            ComPorts.posilca[1] = 32;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            toolStripStatusLabel1.Text = " ";
            toolStripStatusLabel2.Text = "MotorScan_Off";
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
            System.Threading.Thread.Sleep(100);

            Start_time.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            checkBox1.Enabled = true;
            button10.Enabled = true;
        }

        private void Start_time_Click(object sender, EventArgs e)
        {
            this.Text = zagolovok + " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")";
            Save.Text = "Сохранить результат";
            Save.BackColor = Color.FromKnownColor(KnownColor.Control);
            добавитьВОтчетToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
            добавитьВОтчетToolStripMenuItem.Text = "Добавить в отчет";
            checkBox1.Enabled = false;
            checkBox2.Enabled = false;
           
            XMLW.endpropise = false;
            
            list1.Clear();
            list2.Clear();
            list3.Clear();
            textBox6.Text = "0";
            textBox6.Refresh();
            textBox7.Text = "0";
            textBox7.Refresh();

            textBox4.Text = "0";
            textBox4.Refresh();
            textBox5.Text = "0";
            textBox5.Refresh();

            for (int i = 0; i < number; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    ComPorts.buffer[i][j] = 0;
                }

            }
            if ("2" == form6.pokaz.Text)
            {
                PricelBefore = false;
                ComPorts.posilca[1] = 14;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.size_On = 3;
                toolStripStatusLabel1.Text = " ";
                toolStripStatusLabel2.Text = "Off_DP_Off";
                ComPorts.pokets.poket1 = 0;
                ComPorts.Kol_poket = false;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                toolStripStatusLabel3.Text = "Выключение блонировки ПС";
                ComPorts.Poket_Write();
                System.Threading.Thread.Sleep(700);
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
            rejim = 0;
            Pankr = 0;
            timer_25 = 0;
            Start_time.Enabled = false;
            button5.Enabled = false;
            button4.Enabled = false;
            button7.Enabled = false;
            Begin1 = true;
        }

        private void Form8_Resize_1(object sender, EventArgs e)
        {
            SetSize();
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            form5.ShowDialog();
        }

        private void zedGraphControl1_Paint(object sender, PaintEventArgs e)
        {
            zedGraphControl1.GraphPane.YAxis.Scale.Min = -1.1;
            zedGraphControl1.GraphPane.YAxis.Scale.Max = 1.1;

            zedGraphControl1.GraphPane.YAxis.Scale.MajorStep = 0.1f;

            zedGraphControl1.GraphPane.XAxis.Scale.MajorStep = 200;
            zedGraphControl1.GraphPane.XAxis.ScaleFormatEvent += new Axis.ScaleFormatHandler(XScaleFormatEvent);

            zedGraphControl1.GraphPane.XAxis.Scale.Min = 0;
        }

        public string XScaleFormatEvent(GraphPane pane, Axis axis, double val, int index)
        {
            return (val / 200).ToString();
        }

        public void SaveXML()
        {

            XmlTextWriter writer = new XmlTextWriter("xml\\" + form6.NumberProduct + " - " + this.Text + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")" + ".xml", null);
            writer.WriteStartElement("Sosnay_data_file");
            writer.Formatting = Formatting.Indented;

            writer.WriteStartElement("Sosnay_description");
            writer.WriteElementString("Number", form6.NumberProduct);
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

        }

        private void результатыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            form3 = new Form3();
            form3.XMLW = XMLW;
            form3.ShowDialog();
            trackBar1.Maximum = XMLW.trBarMax;
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
            }
        }

        private void toleranceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MouseClick = 0;
            tolerance = true;

            myTimer.Stop();
        }

        private void zedGraphControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (tolerance == true))
            {
                Point p = new Point(e.X, e.Y);
                double x, y;
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
                //textBox1.Text = Convert.ToString(list1[trackBar1.Value].Y);
                //textBox2.Text = Convert.ToString(list2[trackBar1.Value].Y);
                //textBox3.Text = Convert.ToString(list3[trackBar1.Value].Y);
                if (zadL == true)
                {
                    
                }
                else
                   textBox3.Text = Convert.ToString(Math.Round((double)((double)trackBar1.Value / (double)200), 2));

                if (zadDP == true)
                {
                
                }
                else
                    textBox3.Text = Convert.ToString(Math.Round((double)((double)trackBar1.Value / (double)200), 2));
                   
                    textBox1.Text = Convert.ToString(Math.Round((double)((double)trackBar1.Value / (double)200), 2));
                    textBox1.Refresh();

            }
            catch { }
            //textBox1.Refresh();
            //textBox2.Refresh();
            //textBox3.Refresh();
            //textBox4.Refresh();
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

            try
            {
                //textBox1.Text = Convert.ToString(list1[trackBar2.Value].Y);
                //textBox2.Text = Convert.ToString(list2[trackBar2.Value].Y);
                //textBox3.Text = Convert.ToString(list3[trackBar2.Value].Y);
                textBox5.Text = Convert.ToString(Math.Round((double)((double)trackBar2.Value / (double)200),2));
            }
            catch { }
            //textBox1.Refresh();
            //textBox2.Refresh();
            //textBox3.Refresh();
            //textBox5.Refresh();
        }

        private void очисткаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            list1.Clear();
            list2.Clear();
            list3.Clear();
            label5.Text = "0";
            label5.Refresh();
            label7.Text = "0";
            label7.Refresh();
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

        private void button1_Click(object sender, EventArgs e)
        {
            textBox6.Text = Convert.ToString(Math.Round((double)trackBar1.Value / (double)200, 2));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox4.Text = Convert.ToString(Math.Round((double)((double)trackBar1.Value / (double)200), 2));
            textBox4.Refresh();
            zadL = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox5.Text = Convert.ToString(Math.Round((double)((double)trackBar1.Value / (double)200), 2));
            textBox5.Refresh();
            zadDP = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox7.Text = Convert.ToString(Math.Round((double)((double)trackBar1.Value / (double)200), 2));
          //  textBox7.Refresh();
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XMLW.SavePicture(printDocument1,this);
        }

        private void printDocument1_PrintPage_1(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {

            RectangleF destRect = new RectangleF(5.0F, 5.0F, 1080.0F, 768.0F);
            RectangleF srcRect = new RectangleF(0.0F, 0.0F, 1152.0F, 864.0F);
            GraphicsUnit units = GraphicsUnit.Pixel;
            e.Graphics.DrawImage(XMLW.myImage, destRect, srcRect, units);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            button6.Enabled = false;
            button7.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            checkBox2.Enabled = false;


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
            //System.Threading.Thread.Sleep(1000);

            button10.Enabled = false;

            Start_time.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            checkBox1.Enabled = false;


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
            System.Threading.Thread.Sleep(100);

            ComPorts.posilca[1] = 32;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            toolStripStatusLabel1.Text = " ";
            toolStripStatusLabel2.Text = "MotorScan_Off";
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
            System.Threading.Thread.Sleep(100);

            Start_time.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            checkBox1.Enabled = true;
            button10.Enabled = true;

            button7.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = true;
            checkBox2.Enabled = true;

            checkBox1.Enabled = true;
            Start_time.Enabled = true;
            button10.Enabled = true;
            button6.Enabled = true;
           
           

            //list1.Clear();
            //list2.Clear();
            //list3.Clear();
            //textBox6.Text = "0";
            //textBox6.Refresh();
            //textBox7.Text = "0";
            //textBox7.Refresh();

            //textBox4.Text = "0";
            //textBox4.Refresh();
            //textBox5.Text = "0";
            //textBox5.Refresh();

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
            //rejim = 0;
            //Pankr = 0;
            //timer_25 = 0;
            //Start_time.Enabled = false;
            //button7.Enabled = false;
            //Begin1 = false;

            
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save.BackColor = Color.LightGreen;
            SaveXML();
            XMLW.SaveImage(this);
            //MessageBox.Show("Сохранено");
            Save.Text = "Результат сохранен";
            
            
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveXML();
            this.Close();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            first_click++;
            if (first_click == 1)
            {
                XMLW.endpropise = false;
                list1.Clear();
                list2.Clear();
                list3.Clear();
                textBox6.Text = "0";
                textBox7.Text = "0";
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
                Start_time.Enabled = false;

                button6.Enabled = false;
                button7.Enabled = false;
                button1.Enabled = false;
                button2.Enabled = false;
                Pricel = 0;
                rejim2 = 0;
                checkBox2.Text = "СТОП";
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
                System.Threading.Thread.Sleep(10);

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

                //if ("2" == form6.pokaz.Text)
                //{
                //    ComPorts.posilca[1] = 14;
                //    ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                //    ComPorts.size_Off = 3;
                //    ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                //    ComPorts.size_On = 3;
                //    toolStripStatusLabel1.Text = " ";
                //    toolStripStatusLabel2.Text = "Off_DP_Off";
                //    ComPorts.pokets.poket1 = 0;
                //    ComPorts.port.DiscardInBuffer();
                //    ComPorts.port.DiscardOutBuffer();
                //    ComPorts.Kol_poket = false;
                //    ComPorts.Poket_Write();
                //    System.Threading.Thread.Sleep(400);
                //}
                #endregion


                first_click = 0;
                checkBox2.Text = "ПУСК";
                timer_25 = 0;
                for (int i = 0; i < ComPorts.schet_poket; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        ComPorts.buffer[i][j] = 0; ;
                    }
                }
                Start_time.Enabled = true;
                number = 0;
                checkBox2.Checked = false;
                button7.Enabled = true;
                button1.Enabled = true;
                button2.Enabled = true;
                button6.Enabled = true;
                XMLW.endpropise = true;
                timer_25 = 0;
            }
        }

        private void сохранитьРисунокToolStripMenuItem_Click(object sender, EventArgs e)
        {
          
        }

        private void добавитьВОтчетToolStripMenuItem_Click(object sender, EventArgs e)
        {
            добавитьВОтчетToolStripMenuItem.BackColor = Color.LightGreen;
            добавитьВОтчетToolStripMenuItem.Text = "Добавлено в отчет";

            if ("1" == form6.pokaz.Text)
            {
                form6.SetValue("E81", textBox4.Text);
                form6.SetValue("E82", textBox5.Text);
            }
            if ("2" == form6.pokaz.Text)
            { 
                form6.SetValue("E83", textBox6.Text);
                form6.SetValue("E84", textBox7.Text);
            }
        }

        }

}
