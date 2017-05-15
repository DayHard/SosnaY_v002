using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel;
using System.Reflection;
using System.IO;

namespace SosnaY_v00
{

    public partial class Form6 : Form
    {
        public string NumberProduct;
        public string TypeChek;
        public Form1 form1;
        public Form2 form2;
        public Form7 form7; //= new Form7();
        public Form8 form8; //= new Form8();
        public Form9 form9; //= new Form9();
        public Form5 form5; //= new Form5();
        public Form10 form10;// = new Form10();
        public Form11 form11;// = new Form11();
        public Form12 form12;// = new Form12();
        public Form14 form14;// = new Form14();
        public Color myColor;


        /// <summary>
        /// //////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        public const string UID = "Excel.Application";
        object oExcel = null;
        object WorkBooks, WorkBook, WorkSheets, WorkSheet, Range, Interior;

        //КОНСТРУКТОР КЛАССА
        public void Excel()
        {
           oExcel = Activator.CreateInstance(Type.GetTypeFromProgID(UID));
        }

        //ВИДИМОСТЬ EXCEL - СВОЙСТВО КЛАССА
        public bool Visible
        {
            set
            {
                if (false == value)
                    oExcel.GetType().InvokeMember("Visible", BindingFlags.SetProperty,
                        null, oExcel, new object[] { false });

                else
                    oExcel.GetType().InvokeMember("Visible", BindingFlags.SetProperty,
                        null, oExcel, new object[] { true });
            }
        }


        //ОТКРЫТЬ ДОКУМЕНТ
        public void OpenDocument(string name)
        {
            WorkBooks = oExcel.GetType().InvokeMember("Workbooks", BindingFlags.GetProperty, null, oExcel, null);
            WorkBook = WorkBooks.GetType().InvokeMember("Open", BindingFlags.InvokeMethod, null, WorkBooks, new object[] { name, true });
            WorkSheets = WorkBook.GetType().InvokeMember("Worksheets", BindingFlags.GetProperty, null, WorkBook, null);
            WorkSheet = WorkSheets.GetType().InvokeMember("Item", BindingFlags.GetProperty, null, WorkSheets, new object[] { 1 });
            // Range = WorkSheet.GetType().InvokeMember("Range",BindingFlags.GetProperty,null,WorkSheet,new object[1] { "A1" });
        }

        // НОВЫЙ ДОКУМЕНТ
        public void NewDocument()
        {
            WorkBooks = oExcel.GetType().InvokeMember("Workbooks", BindingFlags.GetProperty, null, oExcel, null);
            WorkBook = WorkBooks.GetType().InvokeMember("Add", BindingFlags.InvokeMethod, null, WorkBooks, null);
            WorkSheets = WorkBook.GetType().InvokeMember("Worksheets", BindingFlags.GetProperty, null, WorkBook, null);
            WorkSheet = WorkSheets.GetType().InvokeMember("Item", BindingFlags.GetProperty, null, WorkSheets, new object[] { 1 });
            Range = WorkSheet.GetType().InvokeMember("Range", BindingFlags.GetProperty, null, WorkSheet, new object[1] { "A1" });
        }
        //ЗАКРЫТЬ ДОКУМЕНТ
        public void CloseDocument()
        {
            WorkBook.GetType().InvokeMember("Close", BindingFlags.InvokeMethod, null, WorkBook, new object[] { true });
        }
        //СОХРАНИТЬ ДОКУМЕНТ
        public void SaveDocument(string name)
        {
            if (File.Exists(name))
                WorkBook.GetType().InvokeMember("Save", BindingFlags.InvokeMethod, null,
                    WorkBook, null);
            else
                WorkBook.GetType().InvokeMember("SaveAs", BindingFlags.InvokeMethod, null,
                    WorkBook, new object[] { name });
        }

        // ЗАПИСАТЬ ЗНАЧЕНИЕ В ЯЧЕЙКУ
        public void SetValue(string range, string value)
        {
            Range = WorkSheet.GetType().InvokeMember("Range", BindingFlags.GetProperty,
                null, WorkSheet, new object[] { range });
            Range.GetType().InvokeMember("Value", BindingFlags.SetProperty, null, Range, new object[] { value });
        }

        //ОБЪЕДЕНИТЬ ЯЧЕЙКИ 
        // Alignment - ВЫРАВНИВАНИЕ В ОБЪЕДИНЕННЫХ ЯЧЕЙКАХ
        public void SetMerge(string range, int Alignment)
        {
            Range = WorkSheet.GetType().InvokeMember("Range", BindingFlags.GetProperty,
                null, WorkSheet, new object[] { range });
            object[] args = new object[] { Alignment };
            Range.GetType().InvokeMember("MergeCells", BindingFlags.SetProperty, null, Range, new object[] { true });
            Range.GetType().InvokeMember("HorizontalAlignment", BindingFlags.SetProperty, null, Range, args);
        }

        //УСТАНОВИТЬ ОРИЕНТАЦИЮ СТРАНИЦЫ 
        //1 - КНИЖНЫЙ
        //2 - АЛЬБОМНЫЙ
        public void SetOrientation(int Orientation)
        {
            //Range.Interior.ColorIndex
            object PageSetup = WorkSheet.GetType().InvokeMember("PageSetup", BindingFlags.GetProperty,
                null, WorkSheet, null);

            PageSetup.GetType().InvokeMember("Orientation", BindingFlags.SetProperty,
                null, PageSetup, new object[] { Orientation });
        }

        //УСТАНОВИТЬ ШИРИНУ СТОЛБЦОВ
        public void SetColumnWidth(string range, double Width)
        {
            Range = WorkSheet.GetType().InvokeMember("Range", BindingFlags.GetProperty,
                null, WorkSheet, new object[] { range });
            object[] args = new object[] { Width };
            Range.GetType().InvokeMember("ColumnWidth", BindingFlags.SetProperty, null, Range, args);
        }

        //УСТАНОВИТЬ ВЫСОТУ СТРОК
        public void SetRowHeight(string range, double Height)
        {
            Range = WorkSheet.GetType().InvokeMember("Range", BindingFlags.GetProperty,
                null, WorkSheet, new object[] { range });
            object[] args = new object[] { Height };
            Range.GetType().InvokeMember("RowHeight", BindingFlags.SetProperty, null, Range, args);
        }

        //УСТАНОВИТЬ ВИД РАМКИ ВОКРУГ ЯЧЕЙКИ
        public void SetBorderStyle(string range, int Style)
        {
            Range = WorkSheet.GetType().InvokeMember("Range", BindingFlags.GetProperty,
                null, WorkSheet, new object[] { range });
            object[] args = new object[] { 1 };
            object[] args1 = new object[] { 1 };
            object Borders = Range.GetType().InvokeMember("Borders", BindingFlags.GetProperty, null, Range, null);
            Borders = Range.GetType().InvokeMember("LineStyle", BindingFlags.SetProperty, null, Borders, args);
        }

        //ЧТЕНИЕ ДАННЫХ ИЗ ВЫБРАННОЙ ЯЧЕЙКИ
        public string GetValue(string range)
        {
            Range = WorkSheet.GetType().InvokeMember("Range", BindingFlags.GetProperty,
                null, WorkSheet, new object[] { range });
            return Range.GetType().InvokeMember("Value", BindingFlags.GetProperty,
                null, Range, null).ToString();
        }

        //УСТАНОВИТЬ ВЫРАВНИВАНИЕ В ЯЧЕЙКЕ ПО ВЕРТИКАЛИ
        public void SetVerticalAlignment(string range, int Alignment)
        {
            Range = WorkSheet.GetType().InvokeMember("Range", BindingFlags.GetProperty,
                null, WorkSheet, new object[] { range });
            object[] args = new object[] { Alignment };
            Range.GetType().InvokeMember("VerticalAlignment", BindingFlags.SetProperty, null, Range, args);
        }

        //УСТАНОВИТЬ ВЫРАВНИВАНИЕ В ЯЧЕЙКЕ ПО ГОРИЗОНТАЛИ
        public void SetHorisontalAlignment(string range, int Alignment)
        {
            Range = WorkSheet.GetType().InvokeMember("Range", BindingFlags.GetProperty,
                null, WorkSheet, new object[] { range });
            object[] args = new object[] { Alignment };
            Range.GetType().InvokeMember("HorizontalAlignment", BindingFlags.SetProperty, null, Range, args);
        }

        /// <summary>
        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>

        public Form6()
        {
            
            InitializeComponent();
            pokaz.Hide();
           // form6.BackColor = Color.WhiteSmoke;
            myColor = Color.WhiteSmoke;//Color.FromKnownColor(KnownColor.Control); //button1.BackColor;
            button1.BackColor = myColor;
            button2.BackColor = myColor;
            button3.BackColor = myColor;
            button4.BackColor = myColor;
            button5.BackColor = myColor;
            button6.BackColor = myColor;
            button7.BackColor = myColor;
            button8.BackColor = myColor;
            button9.BackColor = myColor;
            button10.BackColor = myColor;
        }

        private void Form6_FormClosing(object sender, FormClosingEventArgs e)
        {
           
            form1.Show();
            
            button1.BackColor = myColor;
            button2.BackColor = myColor;
            button3.BackColor = myColor;
            button4.BackColor = myColor;
            button5.BackColor = myColor;
            button6.BackColor = myColor;
            button7.BackColor = myColor;
            button8.BackColor = myColor;
            button9.BackColor = myColor;
            button10.BackColor = myColor;
            SaveDocument(Application.StartupPath + "\\Отчёты\\" + NumberProduct + "  " + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + "(" + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")" + ".xls");
            CloseDocument();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pokaz.Text = "1";
            pokaz.Refresh();
            this.Hide();
            form7 = new Form7();
            form7.form6 = this;
            form7.Text = "Прибор " + NumberProduct + " - " + button1.Text.Replace("\n", " "); //+ " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")";
            form7.Tag = NumberProduct + "  ПУ 250м  ";
            form7.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            pokaz.Text = "2";
            pokaz.Refresh();
            this.Hide();
            form7 = new Form7();
            form7.form6 = this;
            form7.Text = "Прибор " + NumberProduct + " - Поле управления Д=250м в режиме прев. ";
            form7.Tag = NumberProduct + "  ПУ 250м ПР  ";
            //form7.Text = "Прибор - " + NumberProduct + " " + button2.Text.Replace("\n", " ") + " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")";
            form7.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            pokaz.Text = "3";
            pokaz.Refresh();
            this.Hide();
            form7 = new Form7();
            form7.form6 = this;
            form7.Text = "Прибор " + NumberProduct + " - " + button3.Text.Replace("\n", " ");
            form7.Tag = NumberProduct + "  ПУ 5880м  ";
            //form7.Text = "Прибор - " + NumberProduct + " " + button3.Text.Replace("\n", " ") + " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")"; 
            form7.ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            pokaz.Text = "1";
            pokaz.Refresh();
            this.Hide();
            form8 = new Form8();
            form8.form6 = this;
            form8.Text = "Прибор " + NumberProduct + " - " + button5.Text.Replace("\n", " ");// + " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")";
            form8.Tag = NumberProduct + "  Время задержки    ";

            form8.GraphMax = true;
            form8.groupBox2.Visible = true;
            form8.groupBox3.Visible = false;
            form8.ShowDialog();  
        }

        private void button6_Click(object sender, EventArgs e)
        {
            pokaz.Text = "2";
            pokaz.Refresh();
            this.Hide();
            form8 = new Form8();
            form8.form6 = this;
            form8.Text = "Прибор " + NumberProduct + " - " + button6.Text.Replace("\n", " ");// + " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")";
            form8.Tag = NumberProduct + "  Время движения   ";
            form8.groupBox2.Visible = false;
            form8.groupBox3.Visible = true;
            form8.ShowDialog();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.Hide();
            form14 = new Form14();
            form14.form6 = this;
            form14.Text = "Прибор " + NumberProduct + " - " + button7.Text.Replace("\n", " ") + " " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")";
            form14.Tag = NumberProduct + "  Величина ПР     ";
            form14.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Hide();
            form10 = new Form10();
            form10.form6 = this;
            form10.form1 = form1;
            form10.NumberProduct = NumberProduct;
            form10.ShowDialog();            
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.Hide();
            form11 = new Form11();
            form11.form6 = this;
            form11.Text = NumberProduct + " - Система визуального контроля";
            form11.Tag = NumberProduct + "  Видеокадр     ";
            form11.ShowDialog();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            this.Hide();
            form12 = new Form12();
            form12.form6 = this;
            form12.Text = "Прибор " + NumberProduct + " - " + button9.Text.Replace("\n", " ");// +" " + " (" + DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString() + " - " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ")";
            form12.Tag = NumberProduct + "  Режим ПР     ";
            form12.ShowDialog();
             
        }

        private void button10_Click(object sender, EventArgs e)
        {
            this.Hide();
            form5 = new Form5();
            form5.form6 = this;
            form5.Text = "Настройки";

            form5.ShowDialog();
        }

        private void Form6_Load(object sender, EventArgs e)
        {
           Excel();
           OpenDocument(Application.StartupPath + "\\" + "ПК332 Отчёт - Сосна-У.xls");
           SetValue("G1", NumberProduct);
           SetValue("G2", DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString());
        }
    }
}
