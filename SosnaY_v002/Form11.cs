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
using System.Diagnostics;


//using System;
using System.Runtime.InteropServices;
//using System.Drawing;
using System.Drawing.Imaging;
//using System.Windows.Forms;

namespace SosnaY_v00
{
     
    public partial class Form11 : Form
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

            public void Poket_Read()
            {
                try
                {
                    #region чтение из АЦП
                    ReadBytes = port.Read(priem_nedoch, 0, 2007);
                   
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
                catch { }
            }
        }

        #region Инициализация переменных
        public xmlWork XMLW = new xmlWork();
        public double rastor;
        public string namePort;
        public byte Command, first_click, first_click1;
        public byte  size;
        public SosnaY_v00.Form11.ComPort ComPorts = new ComPort();
        public byte[] In_buffer = new byte[13];
        public byte[] Out_buffer = new byte[16];
        public int[][] ReadBuffers = new int[1000][];
        public int[] ReadBuffers1 = new int[10];
        Form4 form4 = new Form4();
        public Form6 form6;
        static System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();
        public int TimeInterval, TimerOn, time_sxod, pannkr;
        public short Kor_Z1, Kor_Y1,ss;
        public byte[] GetStatus_Byte1 = new byte[8];
        public byte[] GetStatus_Byte2 = new byte[8];
        public double Kor_P, Kor_Z, Kor_Y, sum, enerj, obsKof,L,focus;
        public bool lasers_ons, pankr_On_Off;
        public bool laser_and_sxod, shtorka, sxood, marka;
        public Form15 form15;
        //public Form15 form15_ = new Form15();  
        public int l;
        public Process myProcess;
        public double SumE = 0;
        public double SumY = 0;
        public double SumZ = 0;
        public double[] bufferE = new double[501];
        public double[] bufferY = new double[501];
        public double[] bufferZ = new double[501];
        public int sh = 0;
        public int steps = 1;
        #endregion

        public Form11()
        {
            InitializeComponent();
            ComPorts.size_Off = 3;
            ComPorts.posilca[0] = 170;
        }

        private void Form11_Load(object sender, EventArgs e)
        {
            myTimer.Tick += new EventHandler(TimerEventProcessor);
            myTimer.Interval = 10;
            myTimer.Start();
            ComPorts.ComInitializ();
            l = 0;
            XMLW.XmlConfigRead();
            ComPorts.posilca[1] = 35;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.size_On = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.Poket_Write();
            laser_and_sxod = false;
            shtorka = false;
            //panel1.BackColor = Color.Tomato;
            //panel2.BackColor = Color.LightGreen;
            //panel3.BackColor = Color.LightGreen;
            //panel4.BackColor = Color.LightGreen;
            //panel5.BackColor = Color.LightGreen;
            //panel6.BackColor = Color.LightGreen;
            //panel6.Visible = false;
            panel5.Visible = false;
            panel1.Visible = false;
            //panel2.Visible = false;
            //panel3.Visible = false;
            //panel4.Visible = false;
            label6.Visible = false;
            label8.Visible = false;
            textBox5.Visible = false;
            ComPorts.posilca[1] = 10;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 6;
            ComPorts.size_On = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.Poket_Write();
        }

        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            myTimer.Enabled = true;

                       
            if (checkBox2.Checked == true)
            {
                label4.Visible = false;
                label11.Visible = false;
                label12.Visible = false;
                label14.Visible = false;
                label13.Visible = false;
                label5.Visible = true;
            }
            else
            {
                label4.Visible = true;
                label11.Visible = true;
                label12.Visible = true;
                label14.Visible = true;
                label13.Visible = true;
                label5.Visible = false;
            }

            if (lasers_ons == true)
            {
                ComPorts.posilca[1] = 24;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 9;
                ComPorts.size_On = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.Poket_Write();
            }

            if (sxood == true)
            {
                time_sxod++;
                textBox3.Text = Convert.ToString(Math.Round((double)(time_sxod / 63), 1));
                
                if (time_sxod == 1380)
                {
                    panel5.Visible = false;
                    time_sxod = 0;
                    textBox3.Text = Convert.ToString(Math.Round((double)(time_sxod / 63), 1));
                    sxood = false;
                }
            }
            if (pankr_On_Off == true)
            {
                pannkr++;
                if (pannkr == 1380)
                {
                    panel2.BackColor = Color.LightGreen;
                    panel6.BackColor = Color.LightGreen;
                   // checkBox1.Checked = true;
                    pannkr = 0;
                    pankr_On_Off = false;
                }
            }
            if (panel1.BackColor == Color.LightGreen)
            {
                ss++;
                    if(ss == 22)
                    {
                        panel1.BackColor = Color.FromKnownColor(KnownColor.Control);
                        ss = 0;
                    }
            }
            #region Вывод на экран данных

            //ComPorts.pokets.Kol_poket = false;
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
                            panel1.Visible = false;
                        }
                        else
                            panel1.Visible = true;
                            

                        if (GetStatus_Byte1[1] == 1) //ЭМП
                        {
                            
                        }
                       // else
                       //     form4.checkBox2.Checked = false;
                        if (GetStatus_Byte1[2] == 1) // ДП
                        {
                           // panel6.Visible = true;
                        }
                        //else
                        //    panel6.Visible = false;
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
                        if (GetStatus_Byte2[5] == 1) // BPO_START
                        {
                            panel3.Visible = true;
                            panel4.Visible = false;
                        }
                        else
                            panel3.Visible = false;
                        if (GetStatus_Byte2[4] == 1) // BPO_END
                        {
                            panel3.Visible = false;
                            panel4.Visible = true;
                        }
                        else
                            panel4.Visible = false;
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
                    }



                    #endregion

                    #region Ready
                    if (ReadBuffers1[1] == 101)
                    {
                        //toolStripStatusLabel1.Text = "ГОТОВО";
                    }
                    #endregion

                    #region ADC_OUT однократно
                    if (ReadBuffers1[1] == 104)
                    {
                        Kor_Z1 = (short)((((ReadBuffers1[3] << 8) & 0x7F00) | ReadBuffers1[2]));
                        Kor_Y1 = (short)((((ReadBuffers1[5] << 8) & 0x7F00) | ReadBuffers1[4]));

                        if ((ReadBuffers1[3] == 0x80) || (ReadBuffers1[3] == 0x81) || (ReadBuffers1[3] == 0x82) || (ReadBuffers1[3] == 0x83))
                        {

                            Kor_Z = (double)((double)(Kor_Z1) / ((double)(1024) * (XMLW.MZ / XMLW.Const)));
                            Kor_Z = Kor_Z * (-1);
                        }
                        else
                        {
                            Kor_Z = (double)((double)(Kor_Z1) / ((double)(1024) * (XMLW.PZ / XMLW.Const)));
                        }

                        if ((ReadBuffers1[5] == 0x80) || (ReadBuffers1[5] == 0x81) || (ReadBuffers1[5] == 0x82) || (ReadBuffers1[5] == 0x83))
                        {
                            Kor_Y = (double)((double)(Kor_Y1) / ((double)(1024) * (XMLW.MY / XMLW.Const)));
                            Kor_Y = Kor_Y * (-1);
                        }
                        else
                        {
                            Kor_Y = (double)((double)(Kor_Y1) / ((double)(1024) * (XMLW.PY / XMLW.Const)));
                        }

                        steps = Convert.ToInt16(textBox5.Text);
                        if (steps > 500)
                        {
                            steps = 500;
                            textBox5.Text = Convert.ToString(steps);
                        }
                        Kor_P = (double)((double)((double)((double)(((ReadBuffers1[7] << 8) & 0x7F00) | ReadBuffers1[6]))));
                        sh = sh + 1;
                        label8.Text = Convert.ToString(sh);
                        label8.Refresh();
                        bufferE[sh] = Math.Round(Kor_P, 3);
                        bufferZ[sh] = Math.Round(Kor_Z, 3);
                        bufferY[sh] = Math.Round(Kor_Y, 3);
                        if (sh >= steps)
                        {
                            for (int q = 1; q <= steps; q++)
                            {
                                SumE = SumE + bufferE[q];
                                SumZ = SumZ + bufferZ[q];
                                SumY = SumY + bufferY[q];
                            }
                            textBox1.Text = Convert.ToString(Math.Round((SumZ / steps), 3));
                            textBox1.Refresh();
                            textBox2.Text = Convert.ToString(Math.Round((SumY / steps), 3));
                            textBox2.Refresh();
                            if (checkBox2.Checked == true)
                            {
                                textBox4.Text = Convert.ToString(Math.Round((SumE / steps), 1));
                            }
                            else
                            {
                                if (panel3.BackColor == Color.LightGreen)
                                {
                                    obsKof = XMLW.V * XMLW.Tay * 0.25 * (Math.PI) * Math.Pow(((XMLW.D * L / XMLW.Fstart)), 3);
                                }
                                else
                                {
                                    if (panel4.BackColor == Color.LightGreen)
                                    {
                                        obsKof = XMLW.V * XMLW.Tay * 0.25 * (Math.PI) * Math.Pow(((XMLW.D * L / XMLW.Fend)), 3);
                                    }
                                }
                                //Непонятно почему пришлось делить на 5...
                                textBox4.Text = Convert.ToString(Math.Round((SumE / steps / obsKof / 5), 3));
                             }
                            textBox4.Refresh();
                            sh = 0;
                            SumE = 0;
                            SumY = 0;
                            SumZ = 0;
                          }
                            

                            //textBox4.Text = Convert.ToString(Math.Round(Kor_P, 3));
                            textBox4.Refresh();
                            //label4.Visible = true;
                            //label11.Visible = true;
                            //label12.Visible = true;
                            //label14.Visible = true;
                            //label13.Visible = true;
                            //label5.Visible = false;
                        }
                    }

                    //CreateGraph(Zedgraph);
                    //zedGraphControl1.Refresh();
                    #endregion
                
                ComPorts.One_Buffers = false;
            #endregion

        }

        private void BPO_START_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 27;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.size_On = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.Poket_Write();
            panel4.BackColor = Color.FromKnownColor(KnownColor.Control);
            panel3.BackColor = Color.LightGreen;
            L  = 250;
            
        }

        private void BPO_END_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 28;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.size_On = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.Poket_Write();
            panel3.BackColor = Color.FromKnownColor(KnownColor.Control);
            panel4.BackColor = Color.LightGreen;
            L = 5880;
            this.Refresh();
        }

        private void Laser_On_Click(object sender, EventArgs e)
        {
            sh = 0;
            ComPorts.posilca[1] = 11;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.size_On = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.Poket_Write();
            lasers_ons = true;
            laser_and_sxod = true;
            panel1.BackColor = Color.Tomato;
        }

        private void Return_Click(object sender, EventArgs e)
        {

          //  textBox3.Text = "0";
            panel1.BackColor = Color.FromKnownColor(KnownColor.Control);
            ComPorts.posilca[1] = 12;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.size_On = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.Poket_Write();
            lasers_ons = false;
            laser_and_sxod = false;
            if (first_click == 1)
            {
              
            }
            else
            {
                sum = 0;
                ss = 0;
                time_sxod = 0;
                textBox3.Text = Convert.ToString(Math.Round((double)(time_sxod / 63), 1));
            }
           // time_sxod = 0;
           // textBox3.Text = Convert.ToString(time_sxod);
            sxood = false;
        }

        private void Sxod_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 17;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.size_On = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.Poket_Write();
            laser_and_sxod = true;
            if (first_click == 1)
            {

            }
            else
            {
                sxood = true;
            }

           // panel5.Visible = true;
        }

        private void Form11_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            if (MessageBox.Show("Хотите выйти?", "Выход", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                form6.button8.BackColor = Color.LightGreen;
                ComPorts.port.Close();
                myTimer.Stop();
                form6.Show();
                ComPorts.posilca[1] = 36;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.size_On = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.Poket_Write();
                if (checkBox3.Checked == true)
                {
                    try
                    {
                        myProcess.CloseMainWindow();
                    }
                    catch { }
                    myProcess.Close();
                }
                if (marka == true)
                {
                    form15.Close();
                }
                this.Dispose();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void Shtorka_On_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 15;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.size_On = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.Poket_Write();
            sum = sum + 0.2;
            shtorka = true;
            textBox3.Text = Convert.ToString(sum);
           // checkBox1.Checked = true;
            panel6.BackColor = Color.LightGreen;
            first_click = 1;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            first_click++;
            if (first_click == 1)
            {
                ComPorts.posilca[1] = 13;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                ComPorts.pokets.poket1 = 0;
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();
                sxood = false;
                panel6.BackColor = Color.LightGreen;
            }
            if (first_click == 2)
            {
                ComPorts.posilca[1] = 14;
                ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 3;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                ComPorts.pokets.poket1 = 0;
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();
                first_click = 0;
                //sxood = true;
                sum = 0;
                ss = 0;
                time_sxod = 0;
                pannkr = 0;
                textBox3.Text = Convert.ToString(Math.Round((double)(time_sxod / 63), 1));
                panel6.BackColor = Color.FromKnownColor(KnownColor.Control);
                panel2.BackColor = Color.FromKnownColor(KnownColor.Control);
            }

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            first_click++;
            
            if (first_click == 1)
            {
                
               myProcess = Process.Start("PlayCap.exe");
               checkBox3.Text = "Видео Выкл.";
            }
            else
            {
                if (first_click == 2)
                {
                    checkBox3.Text = "Видео Вкл.";
                    myProcess.CloseMainWindow();
                    myProcess.Close();
                    if (marka == true)
                    {
                        form15.Close();
                    }
                    first_click = 0;
                }
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            
            first_click1++;
            if (first_click1 == 1)
            {
                marka = true;
                form15 = new Form15();  
                form15.Show();
            }
            else
            {
                if (first_click1 == 2)
                {
                    marka = false;
                    form15.Close();
                    first_click1 = 0;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 19;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            ComPorts.pokets.poket1 = 0;
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
            pankr_On_Off = true;
            first_click = 1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //XMLW.SavePictureBD1(myProcess);
            XMLW.SaveImageVideo();
              
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            label6.Visible = true;
            label8.Visible = true;
            textBox5.Visible = true;
        }

        private void спрятатьУсреднениеЕToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label6.Visible = false;
            label8.Visible = false;
            textBox5.Visible = false;
        }

       
     }
}
