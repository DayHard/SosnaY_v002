using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using ZedGraph;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Diagnostics;

    public class xmlWork : Object
    {
        private enum DrawingOprions
        { 
            PRF_CHECKVISIVLE = 0x00000001,
            PRF_NONCLIENT = 0x00000002,
            PRF_CLIENT = 0x00000004,
            PRF_ERASEBKGND = 0x00000008,
            PRF_CHILDREN = 0x00000010,
            PRF_OWNED = 0x00000020
        }

        private const int WM_PRINT = 0x0317;
        private const int WM_PRINTCLIENT = 0x0318;
        
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int msg, IntPtr dc, DrawingOprions opts);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool BitBlt
        (IntPtr hDecDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, System.Int32 dwRop);

        public Image myImage;



        [DllImport("user32.dll")]
        public extern static IntPtr GetDesktopWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern UInt64 Bitblt(IntPtr hDestDC, int x, int y, int nWidth, int nHeigth, IntPtr hSrcDC, int xSrc, int ySrc, System.Int32 dwRop);





        public double T, Fstart, Fend, D, Tay, V, PZ, MZ, PY, MY, P, KofVolt, Const = 5, Cmehenie_Z, Cmehenie_Y;
        public string COM;

        public byte DZcenter, DYcenter, DZcom, DYcom;

        public PointPairList list1 = new PointPairList();
        public PointPairList list2 = new PointPairList();
        public PointPairList list3 = new PointPairList();

        public bool endpropise, stend;

        public int trBarMax = 4400;


        public void XmlConfigRead() //чтение данных для настроек
        {
            try
            {
                XmlTextReader rd = new XmlTextReader(Convert.ToString("config.xml"));
                rd.Read();
                if (rd.IsStartElement("Sosnay_config"))
                {
                    rd.ReadStartElement("Sosnay_config");
                    while (rd.IsStartElement("Sosnay_description"))
                    {
                        rd.ReadStartElement("Sosnay_description");
                        T = Convert.ToDouble(rd.ReadElementString("T"));
                        Fstart = Convert.ToDouble(rd.ReadElementString("Fstart"));
                        Fend = Convert.ToDouble(rd.ReadElementString("Fend"));
                        D = Convert.ToDouble(rd.ReadElementString("D"));
                        Tay = Convert.ToDouble(rd.ReadElementString("Tay"));
                        V = Convert.ToDouble(rd.ReadElementString("V"));    
                        PZ = Convert.ToDouble(rd.ReadElementString("PZ"));
                        MZ = Convert.ToDouble(rd.ReadElementString("MZ"));
                        PY = Convert.ToDouble(rd.ReadElementString("PY"));
                        MY = Convert.ToDouble(rd.ReadElementString("MY"));
                        P = Convert.ToDouble(rd.ReadElementString("P"));
                        COM = rd.ReadElementString("COM");
                        stend = Convert.ToBoolean(rd.ReadElementString("Stend"));
                        Cmehenie_Z = Convert.ToDouble(rd.ReadElementString("Cmehenie_z"));
                        Cmehenie_Y = Convert.ToDouble(rd.ReadElementString("Cmehenie_y"));
                        rd.ReadEndElement();
                        
                    }
                }
                rd.Close();
            }
            catch 
            {
                MessageBox.Show("Config.xml is not found!");
            }
        }
        
        public void XmlCenterRead() //чтение данных для настроек сканатора
        {
            try
            {
                XmlTextReader rd = new XmlTextReader(Convert.ToString("center.xml"));
                rd.Read();
                if (rd.IsStartElement("Sosnay_center"))
                {
                    rd.ReadStartElement("Sosnay_center");
                    while (rd.IsStartElement("Sosnay_description"))
                    {
                        rd.ReadStartElement("Sosnay_description");
                        DZcenter = Convert.ToByte(rd.ReadElementString("DZcenter"));
                        DYcenter = Convert.ToByte(rd.ReadElementString("DYcenter"));
                        DZcom = Convert.ToByte(rd.ReadElementString("DZcom"));
                        DYcom = Convert.ToByte(rd.ReadElementString("DYcom"));
                        rd.ReadEndElement();
                    }
                }
                rd.Close();
            }
            catch
            {
                MessageBox.Show("Center.xml is not found!");
            }
        }

        public void SaveCenterXML (string valueDZcenter, string valueDYcenter, string valueDZcom1, string valueDZcom2)
        {

            XmlTextWriter writer = new XmlTextWriter("center.xml", null);
            writer.WriteStartElement("Sosnay_center");
            writer.Formatting = Formatting.Indented;

            writer.WriteStartElement("Sosnay_description");
            writer.WriteElementString("DZcenter", valueDZcenter);
            writer.WriteElementString("DYcenter", valueDYcenter);
            writer.WriteElementString("DZcom", valueDZcom1);
            writer.WriteElementString("DYcom", valueDZcom2);
            writer.WriteEndElement();

            writer.WriteFullEndElement();
            writer.Close();

        }

        public void SaveXML(string valueT, string valueFstart, string valueFend, string valueD, string valueTay, string valueV, string valuePZ, string valueMZ, string valuePY, string valueMY, string valueP, string valueCOM, string valueStend, string Cmehenie_Z, string Cmehenie_Y)
        {

            XmlTextWriter writer = new XmlTextWriter("config.xml", null);
            writer.WriteStartElement("Sosnay_config");
            writer.Formatting = Formatting.Indented;

            writer.WriteStartElement("Sosnay_description");
            writer.WriteElementString("T", valueT);
            writer.WriteElementString("Fstart", valueFstart);
            writer.WriteElementString("Fend", valueFend);
            writer.WriteElementString("D", valueD);
            writer.WriteElementString("Tay", valueTay);
            writer.WriteElementString("V", valueV);
            writer.WriteElementString("PZ", valuePZ);
            writer.WriteElementString("MZ", valueMZ);
            writer.WriteElementString("PY", valuePY);
            writer.WriteElementString("MY", valueMY);
            writer.WriteElementString("P", valueP);
            writer.WriteElementString("COM", valueCOM);
            writer.WriteElementString("Stend", valueStend);
            writer.WriteElementString("Cmehenie_z", Cmehenie_Z);
            writer.WriteElementString("Cmehenie_y", Cmehenie_Y);

            writer.WriteEndElement();

            writer.WriteFullEndElement();
            writer.Close();

        }
        //Поиск точки по заданному уровню (в нашем случае 0.8)
        public int SearchPoint(PointPairList list, double Iskomoe, int nach, int kon)
        {

            if (Iskomoe == 0)
            {
                double maxY = list[nach].Y;
                double minY = list[nach].Y;
                int maxX = 0;
                int minX = 0;

                if (Iskomoe > 0)
                {
                    maxY = 10;
                }
                else
                {
                    minY = -10;
                }
                for (int i = nach; i < kon; i++)
                {
                    if ((list[i].Y < Iskomoe) && (list[i].Y > minY))
                    {
                        minY = list[i].Y;
                        minX = i;
                    }
                }

                //for (int i = nach; i < kon; i++)
                //{
                //    if ((list[i].Y > Iskomoe) && (list[i].Y < maxY))
                //    {
                //        maxY = list[i].Y;
                //        maxX = i;
                //    }
                //}
                return minX;
                //if (Math.Abs(Iskomoe - minY) < Math.Abs(Iskomoe - maxY))
                //{
                //    //IskomoeY = minX;
                //    return minX;
                //}
                //else
                //{
                //    //IskomoeY = maxX;
                //    return maxX;
                //}
            }
            if (Iskomoe == -0.8)
            {
                double maxY = list[nach].Y;
                double minY = list[nach].Y;
                int maxX = 0;
                int minX = 0;

                if (Iskomoe > 0)
                {
                    maxY = 10;
                }
                else
                {
                    minY = -10;
                }
                for (int i = nach; i < kon; i++)
                {
                    if ((-0.79 > list[i].Y) && (-0.81 < list[i].Y))
                    {
                        minY = list[i].Y;
                        minX = i;
                        break;
                    }
                    //if ((list[i].Y < Iskomoe) && (list[i].Y > minY))
                    //{
                    //    minY = list[i].Y;
                    //    minX = i;
                    //}
                }

                //for (int i = nach; i < kon; i++)
                //{
                //    if ((list[i].Y > Iskomoe) && (list[i].Y < maxY))
                //    {
                //        maxY = list[i].Y;
                //        maxX = i;
                //    }
                //}

                //maxX = (maxX + minX)/2;
                return minX;
                /*if (Math.Abs(Iskomoe - minY) < Math.Abs(Iskomoe - maxY))
                {
                    //IskomoeY = minX;
                    return minX;
                }
                else
                {
                    //IskomoeY = maxX;
                    return maxX;
                }*/
            }
            else
            {
                double maxY = list[nach - 1].Y;
                double minY = list[nach - 1].Y;
                int maxX = 0;
                int minX = 0;

                if (Iskomoe > 0)
                {
                    maxY = 10;
                }
                else
                {
                    minY = -10;
                }
                for (int i = nach-1; i > kon; i--)
                {
                    if ((0.79 < list[i].Y) && (0.81 > list[i].Y))
                    {
                        minY = list[i].Y;
                        minX = i;
                        break;
                    }
                }

                //for (int i = nach-1; i > kon; i--)
                //{
                //    if ((list[i].Y > Iskomoe) && (list[i].Y < maxY))
                //    {
                //        maxY = list[i].Y;
                //        maxX = i;
                //    }
                //}
                return minX;
                //if (Math.Abs(Iskomoe - minY) < Math.Abs(Iskomoe - maxY))
                //{
                //    //IskomoeY = minX;
                //    return minX;
                //}
                //else
                //{
                //    //IskomoeY = maxX;
                //    return maxX;
                //}
            }
        }

        public void LoadDB(string file)
        {
            list1.Clear();
            list2.Clear();
            list3.Clear();
            int i = 0;
            XmlTextReader rd = new XmlTextReader(file);
            rd.Read();
            if (rd.IsStartElement("Sosnay_data_file"))
            {
                rd.ReadStartElement("Sosnay_data_file");
                rd.ReadToFollowing("Sosnay_data");
                rd.ReadStartElement("Sosnay_data");
                while (rd.IsStartElement("Y"))
                {
                    list1.Add(i, Convert.ToDouble(rd.ReadElementString("Y")));
                    list2.Add(i, Convert.ToDouble(rd.ReadElementString("Z")));
                    list3.Add(i, Convert.ToDouble(rd.ReadElementString("P")));
                    i++;
                }
                rd.ReadEndElement();
            }
            rd.Close();

            endpropise = true;
            
            
            
        }

        public void SavePicture(PrintDocument printDocument, Form Form)
        {
            Graphics gr1 = Form.CreateGraphics();
            myImage = new Bitmap(Form.Width, Form.Height, gr1);
            Graphics gr2 = Graphics.FromImage(myImage);
            IntPtr dc1 = gr1.GetHdc();
            IntPtr dc2 = gr2.GetHdc();
            BitBlt(dc2, 0, Form.ClientRectangle.Top+20, Form.ClientRectangle.Width, Form.ClientRectangle.Height, dc1, 0, 0, 13369376);
            gr1.ReleaseHdc(dc1);
            gr2.ReleaseHdc(dc2);
            myImage.Save("screenshot.png", ImageFormat.Png);
            printDocument.DefaultPageSettings.Landscape = true;
            printDocument.Print();
        }

        public void SavePictureBD(Form Form)
        {
            
            Graphics gr1 = Form.CreateGraphics();
            myImage = new Bitmap(Form.Width, Form.Height, gr1);
            Graphics gr2 = Graphics.FromImage(myImage);
            IntPtr dc1 = gr1.GetHdc();
            IntPtr dc2 = gr2.GetHdc();

            BitBlt(dc2, 0, Form.ClientRectangle.Top + 20, Form.ClientRectangle.Width, Form.ClientRectangle.Height, dc1, 0, 0, 13369376);
            gr1.ReleaseHdc(dc1);
            gr2.ReleaseHdc(dc2);
            myImage.Save("images\\" + Form.Text + ".png", ImageFormat.Png);
        }

        public void SavePictureBD1(Process myPr)
        {
            Graphics gr1 = Graphics.FromHwnd(myPr.MainWindowHandle);
            myImage = new Bitmap(300, 300, gr1);
            Graphics gr2 = Graphics.FromImage(myImage);
            IntPtr dc1 = gr1.GetHdc();
            IntPtr dc2 = gr2.GetHdc();

            BitBlt(dc2, 0, 300, 300, 300, dc1, 0, 0, 13369376);
            gr1.ReleaseHdc(dc1);
            gr2.ReleaseHdc(dc2);
            myImage.Save("images\\" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + "(" + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")" + ".jpeg", ImageFormat.Jpeg);
        }

        public void SaveImage(Form Form)
        {
            using (Bitmap bm = new Bitmap(Form.Width, Form.Height))
            {
                using (Graphics g = Graphics.FromImage(bm))
                {
                    IntPtr dc = g.GetHdc();
                    try
                    {
                        SendMessage(Form.Handle, WM_PRINT, dc, DrawingOprions.PRF_CHILDREN | DrawingOprions.PRF_CLIENT | DrawingOprions.PRF_NONCLIENT);
                    }
                    finally
                    {
                        g.ReleaseHdc(dc);
                    }
                    bm.Save("images\\" + Form.Tag + DateTime.Now.Year.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Day.ToString() + "(" + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")" + ".png");
                }
            }
        }

        public void SaveImageVideo()
        { 
            Image myImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height);
            Graphics gr1 = Graphics.FromImage(myImage);
            IntPtr dc1 = gr1.GetHdc();
            IntPtr dc2 = GetWindowDC(GetDesktopWindow());
            BitBlt(dc1,0,0,Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height,dc2,0,0, 13369376);
            gr1.ReleaseHdc(dc1);
            myImage.Save("images\\" + "Видеокадр   " + DateTime.Now.Year.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Day.ToString() + "(" + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")" + ".jpg", ImageFormat.Jpeg);
        }
      
    }
