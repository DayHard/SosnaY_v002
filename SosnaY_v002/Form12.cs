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
    public partial class Form12 : Form
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

        public xmlWork XMLW = new xmlWork();
        public SosnaY_v00.Form12.ComPort ComPorts = new ComPort();
        public int[] ReadBuffers1 = new int[10];
        static System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();
        public short time_muve_pankr, time_begin_sniatia_prevish, time_sniatia_prevish, time_vozvrata;
        public long number, number_dorisovka, l, timer_25;
        public byte[] GetStatus_Byte1 = new byte[8];
        public byte[] GetStatus_Byte2 = new byte[8];
        public double Kor_Z, Kor_Y, Kor_P, rastor;
        public short Kor_Z1, Kor_Y1;
        public byte Pricel, rejim2, first_click, index, schet, ret;
        public Form6 form6;
        public string zagolovok;
        public Form12 form12;
        Form4 form4 = new Form4();
        public bool returns, t1, t2, t3, test_time, returns_one, climat = false, Prev,test;
        #endregion

        public Form12()
        {
            InitializeComponent();
            ComPorts.posilca[0] = 170;
            ComPorts.size_Off = 3;
            first_click = 0;
            //textBox4.Enabled = false;
            textBox4.BackColor = Color.White;
        }

        private void Form12_Load(object sender, EventArgs e)
        {
            zagolovok = this.Text;
            this.Text = zagolovok + " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")"; 
            ComPorts.ComInitializ();
            Prev = false;
            climat = true;
            режимЛетоToolStripMenuItem.BackColor = Color.LightGreen;
            int bText = 1400;
            double timeMin = 1.02,
                   timeMax = 1.22,
                   timeMin1 = 1.45,
                   timeMax1 = 1.65;

            if (climat == true)
            {
                for (int i = 0; i < 14; i++)
                {
                    if (i < 13)
                        dataGridView1.Rows.Add();
                    dataGridView1.Rows[i].Cells[0].Value = Convert.ToString(bText);
                    dataGridView1.Rows[i].Cells[1].Value = Convert.ToString(timeMin);
                    dataGridView1.Rows[i].Cells[2].Value = Convert.ToString(timeMax);
                    if (bText < 3000)
                    {
                        bText += 200;
                    }
                    else
                    {
                        if (bText < 3800)
                        {
                            bText += 400;
                        }
                        else
                        {
                            if (bText < 4000)
                            {
                                bText += 200;
                            }
                            else
                            {
                                bText += 500;
                            }
                        }
                    }
                    if (timeMin < 5.5)
                    {
                        timeMin += 0.56;
                        timeMax += 0.56;
                    }
                    else
                    {
                        if (timeMin < 7.74)
                        {
                            timeMin += 1.12;
                            timeMax += 1.12;
                        }
                        else
                        {
                            if (timeMin < 8.3)
                            {
                                timeMin += 0.56;
                                timeMax += 0.56;
                            }
                            else
                            {
                                timeMin += 1.4;
                                timeMax += 1.4;
                            }
                        }
                    }

                }
            }
            else
            {
                for (int i = 0; i < 14; i++)
                {
                    if (i < 13)
                        dataGridView1.Rows.Add();
                    dataGridView1.Rows[i].Cells[0].Value = Convert.ToString(bText);
                    dataGridView1.Rows[i].Cells[1].Value = Convert.ToString(timeMin1);
                    dataGridView1.Rows[i].Cells[2].Value = Convert.ToString(timeMax1);
                    if (bText < 3000)
                    {
                        bText += 200;
                    }
                    else
                    {
                        if (bText < 3800)
                        {
                            bText += 400;
                        }
                        else
                        {
                            if (bText < 4000)
                            {
                                bText += 200;
                            }
                            else
                            {
                                bText += 500;
                            }
                        }
                    }
                    if (timeMin1 < 5.89)
                    {
                        timeMin1 += 0.62;
                        timeMax1 += 0.62;
                    }
                    else
                    {
                        if (timeMin1 < 7.75)
                        {
                            timeMin1 += 1.24;
                            timeMax1 += 1.24;
                        }
                        else
                        {
                            if (timeMin1 < 8.99)
                            {
                                timeMin1 += 0.62;
                                timeMax1 += 0.62;
                            }
                            else
                            {
                                timeMin1 += 1.55;
                                timeMax1 += 1.55;
                            }
                        }
                    }

                }
            }

            myTimer.Tick += new EventHandler(TimerEventProcessor);
            myTimer.Interval = 500;
            myTimer.Start();
        }

        private void Form12_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((сохранитьРисунокToolStripMenuItem.BackColor == Color.LightGreen) && (добавитьВОтчётToolStripMenuItem.BackColor == Color.LightGreen))
            {
                form6.button9.BackColor = Color.LightGreen;
                сохранитьРисунокToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                сохранитьРисунокToolStripMenuItem.Text = "Cохранить результат";
                добавитьВОтчётToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control); ;
                добавитьВОтчётToolStripMenuItem.Text = "Добавить в отчет";
                ComPorts.port.Close();
                myTimer.Stop();
                form6.Show();
                this.Dispose();
            }
            else
            {
                if ((сохранитьРисунокToolStripMenuItem.BackColor != Color.LightGreen) && (добавитьВОтчётToolStripMenuItem.BackColor != Color.LightGreen))
                {
                    if (MessageBox.Show("Хотите выйти не сохраняя результат и не добавляя данные в отчет?", "Выход", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        сохранитьРисунокToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                        сохранитьРисунокToolStripMenuItem.Text = "Cохранить результат";
                        добавитьВОтчётToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control); ;
                        добавитьВОтчётToolStripMenuItem.Text = "Добавить в отчет";
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
                            добавитьВОтчётToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control); ;
                            добавитьВОтчётToolStripMenuItem.Text = "Добавить в отчет";
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
                        if (добавитьВОтчётToolStripMenuItem.BackColor != Color.LightGreen)
                        {
                            if (MessageBox.Show("Хотите выйти не добавляя данные в отчет?", "Выход", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                сохранитьРисунокToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                                сохранитьРисунокToolStripMenuItem.Text = "Cохранить результат";
                                добавитьВОтчётToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control); ;
                                добавитьВОтчётToolStripMenuItem.Text = "Добавить в отчет";
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

        private void Exit_Click(object sender, EventArgs e)
        {
            ComPorts.port.Close();
            myTimer.Stop();
            form6.Show();
            this.Close();
        }


        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            if (returns_one == true)
            {
                if (schet == 0)
                {
                    if (test == true)
                    { }
                    else
                    {
                        System.Threading.Thread.Sleep(50);
                        Returns_On();
                        schet = 1;
                    }
                }
                returns_one = false;
            }
                #region Вывод на экран данных
                if (ComPorts.One_Buffers == true)
                {
                    ReadBuffers1 = ComPorts.pokets.buffer1;

                    #region Ready
                    if (ReadBuffers1[1] == 101)
                    {
                        returns_one = true;
                    }
                    #endregion

                    #region Test_time
                    if (ReadBuffers1[1] == 103)
                    {
                        time_muve_pankr = (short)((((ReadBuffers1[3] << 8) & 0x7F00) | ReadBuffers1[2]));
                        time_begin_sniatia_prevish = (short)((((ReadBuffers1[5] << 8) & 0x7F00) | ReadBuffers1[4]));
                        time_sniatia_prevish = (short)((((ReadBuffers1[7] << 8) & 0x7F00) | ReadBuffers1[6]));
                        time_vozvrata = (short)((((ReadBuffers1[9] << 8) & 0x7F00) | ReadBuffers1[8]));
                      
                        if (t2 == true)
                        {
                            textBox1.Text = Convert.ToString(Math.Round((double)((double)time_sniatia_prevish / (double)1000), 2));
                            textBox1.Refresh();
                            t2 = false;
                        }
                        if (t3 == true)
                        {
                            textBox2.Text = Convert.ToString(Math.Round((double)((double)time_sniatia_prevish / (double)1000), 2));
                            textBox2.Refresh();
                            t3 = false;
                        }
                        if (returns == true)
                        {
                            textBox3.Text = Convert.ToString(Math.Round(((double)((double)time_vozvrata) / (double)1000),2));
                            textBox3.Refresh();
                            returns = false;
                        }
                        if (Prev == true)
                        {
                            dataGridView1.Rows[index].Cells[3].Value = Convert.ToString(Math.Round((double)((double)time_begin_sniatia_prevish / (double)1000), 2) + 0.11) ;
                            dataGridView1.Rows[index].Cells[4].Value = Convert.ToString(Math.Round((double)((double)time_sniatia_prevish / (double)1000), 2));
                            //dataGridView1.Rows[index].Cells[1].Value = Convert.ToString(0.00);
                            //dataGridView1.Rows[index].Cells[2].Value = Convert.ToString(0.00);
                            Prev = false;
                        }
                        test_time = false;
                      
                            ComPorts.posilca[1] = 12;
                            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                            ComPorts.size_Off = 3;
                            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                            ComPorts.size_On = 3;
                            ComPorts.pokets.poket1 = 0;
                            ComPorts.port.DiscardInBuffer();
                            ComPorts.port.DiscardOutBuffer();
                            ComPorts.Kol_poket = false;
                            ComPorts.Poket_Write();
                      
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
                        if ((rastor > 480) && (rastor < 520))
                        {
                            textBox4.Text = Convert.ToString((rastor * 0.2));
                        }
                        //if ((rastor < 480) || (rastor > 520))
                        //{
                        //    //Begin = false;
                        //    MessageBox.Show("Недостаточная частота растра");
                        //   // rejim1 = 0;
                        //    myTimer.Stop();
                        //}
                        ////else
                            
                    }



                    #endregion

                    ComPorts.One_Buffers = false;
                }
            #endregion
        }

        private void Sbros1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "0";
        }

        public void Shod()
        {
            ComPorts.posilca[1] = 20;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 11;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
        }

        public void Returns_On()
        {
            ComPorts.posilca[1] = 12;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
            System.Threading.Thread.Sleep(8);

            ComPorts.posilca[1] = 20;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
                ComPorts.size_Off = 11;
                ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
                ComPorts.size_On = 3;
                ComPorts.pokets.poket1 = 0;
                ComPorts.port.DiscardInBuffer();
                ComPorts.port.DiscardOutBuffer();
                ComPorts.Kol_poket = false;
                ComPorts.Poket_Write();
            test_time = true;
          //  
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
           // Shod();
            ComPorts.posilca[1] = 43;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 11;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();

            Prev = true;
            index = (byte)(e.RowIndex);
        }

        private void Return_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 17;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 3;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
            schet = 0;
            returns = true;
            
        }

        private void BPR_PR_Click(object sender, EventArgs e)
        {
            t2 = true;
            ComPorts.posilca[1] = 42;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 11;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
        }

        private void PR_BPR_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 42;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 11;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
            t3 = true;
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

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "0";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox3.Text = "0";
        }

        private void режимЗимаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            climat = false;

            режимЗимаToolStripMenuItem.BackColor = Color.Turquoise;
            режимЛетоToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);

                сохранитьРисунокToolStripMenuItem.Text = "Сохранить результат";
                добавитьВОтчётToolStripMenuItem.Text = "Добавить в отчёт";
                добавитьВОтчётToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                сохранитьРисунокToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                for (int h = 0; h < 14; h++)
                {
                    dataGridView1.Rows[h].Cells[3].Value = " ";
                    dataGridView1.Rows[h].Cells[4].Value = " ";
                }

            //if (режимЗимаToolStripMenuItem.BackColor == Color.Turquoise)
            //    режимЗимаToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
            //else
            //    режимЗимаToolStripMenuItem.BackColor = Color.Turquoise;
            
            int bText = 1400;
            double timeMin = 1.02,
                   timeMax = 1.22,
                   timeMin1 = 1.45,
                   timeMax1 = 1.65;

            if (climat == true)
            {
                for (int i = 0; i < 14; i++)
                {
                    if (i < 13)
                        dataGridView1.Rows.Add();
                    dataGridView1.Rows[i].Cells[0].Value = Convert.ToString(bText);
                    dataGridView1.Rows[i].Cells[1].Value = Convert.ToString(timeMin);
                    dataGridView1.Rows[i].Cells[2].Value = Convert.ToString(timeMax);
                    if (bText < 3000)
                    {
                        bText += 200;
                    }
                    else
                    {
                        if (bText < 3800)
                        {
                            bText += 400;
                        }
                        else
                        {
                            if (bText < 4000)
                            {
                                bText += 200;
                            }
                            else
                            {
                                bText += 500;
                            }
                        }
                    }
                    if (timeMin < 5.5)
                    {
                        timeMin += 0.56;
                        timeMax += 0.56;
                    }
                    else
                    {
                        if (timeMin < 7.74)
                        {
                            timeMin += 1.12;
                            timeMax += 1.12;
                        }
                        else
                        {
                            if (timeMin < 8.3)
                            {
                                timeMin += 0.56;
                                timeMax += 0.56;
                            }
                            else
                            {
                                timeMin += 1.4;
                                timeMax += 1.4;
                            }
                        }
                    }

                }
            }
            else
            {
                for (int i = 0; i < 14; i++)
                {
                    if (i < 13)
                        dataGridView1.Rows.Add();
                    dataGridView1.Rows[i].Cells[0].Value = Convert.ToString(bText);
                    dataGridView1.Rows[i].Cells[1].Value = Convert.ToString(timeMin1);
                    dataGridView1.Rows[i].Cells[2].Value = Convert.ToString(timeMax1);
                    if (bText < 3000)
                    {
                        bText += 200;
                    }
                    else
                    {
                        if (bText < 3800)
                        {
                            bText += 400;
                        }
                        else
                        {
                            if (bText < 4000)
                            {
                                bText += 200;
                            }
                            else
                            {
                                bText += 500;
                            }
                        }
                    }
                    if (timeMin1 < 5.89)
                    {
                        timeMin1 += 0.62;
                        timeMax1 += 0.62;
                    }
                    else
                    {
                        if (timeMin1 < 7.75)
                        {
                            timeMin1 += 1.24;
                            timeMax1 += 1.24;
                        }
                        else
                        {
                            if (timeMin1 < 8.99)
                            {
                                timeMin1 += 0.62;
                                timeMax1 += 0.62;
                            }
                            else
                            {
                                timeMin1 += 1.55;
                                timeMax1 += 1.55;
                            }
                        }
                    }

                }
            }
        }

        private void режимЛетоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            climat = true;

            режимЛетоToolStripMenuItem.BackColor = Color.LightGreen;
            режимЗимаToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);


           
                сохранитьРисунокToolStripMenuItem.Text = "Сохранить результат";
                добавитьВОтчётToolStripMenuItem.Text = "Добавить в отчёт";
                добавитьВОтчётToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                сохранитьРисунокToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
                for (int h = 0; h < 14; h++)
                {
                    dataGridView1.Rows[h].Cells[3].Value = " ";
                    dataGridView1.Rows[h].Cells[4].Value = " ";
                }

            //if (режимЛетоToolStripMenuItem.BackColor == Color.LightGreen)
            //    режимЛетоToolStripMenuItem.BackColor = Color.FromKnownColor(KnownColor.Control);
            //else
            //    режимЛетоToolStripMenuItem.BackColor = Color.LightGreen;

            int bText = 1400;
            double timeMin = 1.02,
                   timeMax = 1.22,
                   timeMin1 = 1.45,
                   timeMax1 = 1.65;

            if (climat == true)
            {
                for (int i = 0; i < 14; i++)
                {
                    if (i < 13)
                        dataGridView1.Rows.Add();
                    dataGridView1.Rows[i].Cells[0].Value = Convert.ToString(bText);
                    dataGridView1.Rows[i].Cells[1].Value = Convert.ToString(timeMin);
                    dataGridView1.Rows[i].Cells[2].Value = Convert.ToString(timeMax);
                    if (bText < 3000)
                    {
                        bText += 200;
                    }
                    else
                    {
                        if (bText < 3800)
                        {
                            bText += 400;
                        }
                        else
                        {
                            if (bText < 4000)
                            {
                                bText += 200;
                            }
                            else
                            {
                                bText += 500;
                            }
                        }
                    }
                    if (timeMin < 5.5)
                    {
                        timeMin += 0.56;
                        timeMax += 0.56;
                    }
                    else
                    {
                        if (timeMin < 7.74)
                        {
                            timeMin += 1.12;
                            timeMax += 1.12;
                        }
                        else
                        {
                            if (timeMin < 8.3)
                            {
                                timeMin += 0.56;
                                timeMax += 0.56;
                            }
                            else
                            {
                                timeMin += 1.4;
                                timeMax += 1.4;
                            }
                        }
                    }

                }
            }
            else
            {
                for (int i = 0; i < 14; i++)
                {
                    if (i < 13)
                        dataGridView1.Rows.Add();
                    dataGridView1.Rows[i].Cells[0].Value = Convert.ToString(bText);
                    dataGridView1.Rows[i].Cells[1].Value = Convert.ToString(timeMin1);
                    dataGridView1.Rows[i].Cells[2].Value = Convert.ToString(timeMax1);
                    if (bText < 3000)
                    {
                        bText += 200;
                    }
                    else
                    {
                        if (bText < 3800)
                        {
                            bText += 400;
                        }
                        else
                        {
                            if (bText < 4000)
                            {
                                bText += 200;
                            }
                            else
                            {
                                bText += 500;
                            }
                        }
                    }
                    if (timeMin1 < 5.89)
                    {
                        timeMin1 += 0.62;
                        timeMax1 += 0.62;
                    }
                    else
                    {
                        if (timeMin1 < 7.75)
                        {
                            timeMin1 += 1.24;
                            timeMax1 += 1.24;
                        }
                        else
                        {
                            if (timeMin1 < 8.99)
                            {
                                timeMin1 += 0.62;
                                timeMax1 += 0.62;
                            }
                            else
                            {
                                timeMin1 += 1.55;
                                timeMax1 += 1.55;
                            }
                        }
                    }

                }
            }
        }

        private void сохранитьРисунокToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Text = zagolovok + " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")"; 
            XMLW.SaveImage(this);
            сохранитьРисунокToolStripMenuItem.BackColor = Color.LightGreen;
            сохранитьРисунокToolStripMenuItem.Text = "Результат сохранен";
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ComPorts.posilca[1] = 18;
            ComPorts.posilca[2] = (byte)(ComPorts.posilca[0] ^ ComPorts.posilca[1]);
            ComPorts.size_Off = 6;
            ComPorts.port.ReceivedBytesThreshold = ComPorts.size_Off;
            ComPorts.size_On = 3;
            ComPorts.pokets.poket1 = 0;
            ComPorts.port.DiscardInBuffer();
            ComPorts.port.DiscardOutBuffer();
            ComPorts.Kol_poket = false;
            ComPorts.Poket_Write();
        }

        private void добавитьВОтчётToolStripMenuItem_Click(object sender, EventArgs e)
        {
            добавитьВОтчётToolStripMenuItem.BackColor = Color.LightGreen;
            добавитьВОтчётToolStripMenuItem.Text = "Добавлено в отчет";
            if (textBox4.Text != "")
            {
                form6.SetValue("E88", textBox4.Text);
            }
            if (режимЗимаToolStripMenuItem.BackColor == Color.Turquoise)
            {

                form6.SetValue("D59", Convert.ToString(dataGridView1.Rows[0].Cells[3].Value));
                form6.SetValue("D60", Convert.ToString(dataGridView1.Rows[1].Cells[3].Value));
                form6.SetValue("D61", Convert.ToString(dataGridView1.Rows[2].Cells[3].Value));
                form6.SetValue("D62", Convert.ToString(dataGridView1.Rows[3].Cells[3].Value));
                form6.SetValue("D63", Convert.ToString(dataGridView1.Rows[4].Cells[3].Value));
                form6.SetValue("D64", Convert.ToString(dataGridView1.Rows[5].Cells[3].Value));
                form6.SetValue("D65", Convert.ToString(dataGridView1.Rows[6].Cells[3].Value));
                form6.SetValue("D66", Convert.ToString(dataGridView1.Rows[7].Cells[3].Value));
                form6.SetValue("D67", Convert.ToString(dataGridView1.Rows[8].Cells[3].Value));
                form6.SetValue("D68", Convert.ToString(dataGridView1.Rows[9].Cells[3].Value));
                form6.SetValue("D69", Convert.ToString(dataGridView1.Rows[10].Cells[3].Value));
                form6.SetValue("D70", Convert.ToString(dataGridView1.Rows[11].Cells[3].Value));
                form6.SetValue("D71", Convert.ToString(dataGridView1.Rows[12].Cells[3].Value));
                form6.SetValue("D72", Convert.ToString(dataGridView1.Rows[13].Cells[3].Value));

                form6.SetValue("E59", Convert.ToString(dataGridView1.Rows[0].Cells[4].Value));
                form6.SetValue("E60", Convert.ToString(dataGridView1.Rows[1].Cells[4].Value));
                form6.SetValue("E61", Convert.ToString(dataGridView1.Rows[2].Cells[4].Value));
                form6.SetValue("E62", Convert.ToString(dataGridView1.Rows[3].Cells[4].Value));
                form6.SetValue("E63", Convert.ToString(dataGridView1.Rows[4].Cells[4].Value));
                form6.SetValue("E64", Convert.ToString(dataGridView1.Rows[5].Cells[4].Value));
                form6.SetValue("E65", Convert.ToString(dataGridView1.Rows[6].Cells[4].Value));
                form6.SetValue("E66", Convert.ToString(dataGridView1.Rows[7].Cells[4].Value));
                form6.SetValue("E67", Convert.ToString(dataGridView1.Rows[8].Cells[4].Value));
                form6.SetValue("E68", Convert.ToString(dataGridView1.Rows[9].Cells[4].Value));
                form6.SetValue("E69", Convert.ToString(dataGridView1.Rows[10].Cells[4].Value));
                form6.SetValue("E70", Convert.ToString(dataGridView1.Rows[11].Cells[4].Value));
                form6.SetValue("E71", Convert.ToString(dataGridView1.Rows[12].Cells[4].Value));
                form6.SetValue("E72", Convert.ToString(dataGridView1.Rows[13].Cells[4].Value));
            }
            else
            {
                if (режимЛетоToolStripMenuItem.BackColor == Color.LightGreen)
                {
                    form6.SetValue("B59", Convert.ToString(dataGridView1.Rows[0].Cells[3].Value));
                    form6.SetValue("B60", Convert.ToString(dataGridView1.Rows[1].Cells[3].Value));
                    form6.SetValue("B61", Convert.ToString(dataGridView1.Rows[2].Cells[3].Value));
                    form6.SetValue("B62", Convert.ToString(dataGridView1.Rows[3].Cells[3].Value));
                    form6.SetValue("B63", Convert.ToString(dataGridView1.Rows[4].Cells[3].Value));
                    form6.SetValue("B64", Convert.ToString(dataGridView1.Rows[5].Cells[3].Value));
                    form6.SetValue("B65", Convert.ToString(dataGridView1.Rows[6].Cells[3].Value));
                    form6.SetValue("B66", Convert.ToString(dataGridView1.Rows[7].Cells[3].Value));
                    form6.SetValue("B67", Convert.ToString(dataGridView1.Rows[8].Cells[3].Value));
                    form6.SetValue("B68", Convert.ToString(dataGridView1.Rows[9].Cells[3].Value));
                    form6.SetValue("B69", Convert.ToString(dataGridView1.Rows[10].Cells[3].Value));
                    form6.SetValue("B70", Convert.ToString(dataGridView1.Rows[11].Cells[3].Value));
                    form6.SetValue("B71", Convert.ToString(dataGridView1.Rows[12].Cells[3].Value));
                    form6.SetValue("B72", Convert.ToString(dataGridView1.Rows[13].Cells[3].Value));
                    form6.SetValue("C59", Convert.ToString(dataGridView1.Rows[0].Cells[4].Value));
                    form6.SetValue("C60", Convert.ToString(dataGridView1.Rows[1].Cells[4].Value));
                    form6.SetValue("C61", Convert.ToString(dataGridView1.Rows[2].Cells[4].Value));
                    form6.SetValue("C62", Convert.ToString(dataGridView1.Rows[3].Cells[4].Value));
                    form6.SetValue("C63", Convert.ToString(dataGridView1.Rows[4].Cells[4].Value));
                    form6.SetValue("C64", Convert.ToString(dataGridView1.Rows[5].Cells[4].Value));
                    form6.SetValue("C65", Convert.ToString(dataGridView1.Rows[6].Cells[4].Value));
                    form6.SetValue("C66", Convert.ToString(dataGridView1.Rows[7].Cells[4].Value));
                    form6.SetValue("C67", Convert.ToString(dataGridView1.Rows[8].Cells[4].Value));
                    form6.SetValue("C68", Convert.ToString(dataGridView1.Rows[9].Cells[4].Value));
                    form6.SetValue("C69", Convert.ToString(dataGridView1.Rows[10].Cells[4].Value));
                    form6.SetValue("C70", Convert.ToString(dataGridView1.Rows[11].Cells[4].Value));
                    form6.SetValue("C71", Convert.ToString(dataGridView1.Rows[12].Cells[4].Value));
                    form6.SetValue("C72", Convert.ToString(dataGridView1.Rows[13].Cells[4].Value));
                }
            }
            form6.SetValue("E87", textBox1.Text);
            form6.SetValue("E86", textBox2.Text);
            form6.SetValue("E85", textBox3.Text);
        } 

    }
}
