using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;

namespace PDF_downloader
{
    static public class dictonary_n_list_functions
    {
        static Excel.Application? excel_app;
        static Excel.Workbook? workbook;
        static Excel.Worksheet? worksheet;
        public static Dictionary<string, (string, string)>? instance_excel_window(string path)
        {
            if (!File.Exists(path))
            {
                MessageBox.Show("no file in \n " + path + "\nfailed to load", "load error");
                return null;
            }
            try
            {
                //Start Excel and get Application object.
                excel_app = new Excel.Application();
                excel_app.Visible = false;
                workbook = excel_app.Workbooks.Open(path);
                worksheet = workbook.ActiveSheet;
                return add_to_list_of_links();
            }
            catch (Exception theException)
            {
                String errorMessage;
                errorMessage = "Error: ";
                errorMessage = String.Concat(errorMessage, theException.Message);
                errorMessage = String.Concat(errorMessage, theException.StackTrace);
                errorMessage = String.Concat(errorMessage, theException.Source);
                MessageBox.Show(errorMessage, "Error");
                return new();
            }
        }

        public static Dictionary<string, (string, string)> add_to_list_of_links()
        {
            Dictionary<string, (string, string)> temp_list = new();
            int i = 2;
            while (true)
            {
                string id = (worksheet.Cells[i, 1] as Excel.Range).Text;
                if (id is null||id == "") break;
                string value = (worksheet.Cells[i, 2] as Excel.Range).Text;
                if (value == "" || value is null) value = "";
                string value2 = (worksheet.Cells[i, 3] as Excel.Range).Text;
                if (value2 is null) value2 = "";
                temp_list.Add(id, (value, value2));
                i++;
            }
            workbook.Close(0);
            excel_app.Quit();
            return temp_list;
        }
        public static System.Windows.Controls.ListBox refresh_excel_listview(System.Windows.Controls.ListBox listBox, Dictionary<string, (string, string)> list)
        {
            listBox.Items.Clear();
            foreach (KeyValuePair<string, (string, string)> item in list)
            {
                listBox.Items.Add(string.Format("{0}. {1} ¤ {2}",
                item.Key,
                item.Value.Item1 != "" ? item.Value.Item1.Split("//")[1].Split("/").First() : item.Value.Item2.Split("//")[1].Split("/").First(),
                item.Value.Item1 != "" ? item.Value.Item1.Split("/").Last() : item.Value.Item2.Split("/").Last())
                );
            }
            return listBox;
        }

        public static void create_excel_output(System.Windows.Controls.TextBox textBox, Dictionary<string, (string, string)> list)
        {
            if (!Path.IsPathRooted(textBox.Text))
            {
                MessageBox.Show("path for downloaded excel file had an error","error");
                return;
            }
            if (!Directory.Exists(string.Join('\\', textBox.Text.Split("\\"), 0, textBox.Text.Split("\\").Count() - 2))) Directory.Exists(string.Join('\\', textBox.Text.Split("\\"), 0, textBox.Text.Split("\\").Count() - 2));
            try
            {
                //Start Excel and get Application object.
                excel_app = new Excel.Application();
                excel_app.Visible = false;
                workbook = excel_app.Workbooks.Add();
                worksheet = workbook.ActiveSheet;
                int i = 2;
                worksheet.Cells[1, 1] = "ID";
                worksheet.Cells[1, 2] = "URL";
                worksheet.Cells[1, 3] = "alt URL";
                foreach (KeyValuePair<string, (string, string)> valuePair in list)
                {
                    worksheet.Cells[i, 1] = valuePair.Key;
                    worksheet.Cells[i, 2] = valuePair.Value.Item1;
                    worksheet.Cells[i, 3] = valuePair.Value.Item2;
                    i++;
                }
                string filename = Path.ChangeExtension(textBox.Text, ".xlsx");
                if (File.Exists(filename)) File.Delete(filename);
                workbook.SaveAs2(filename);
            }
            catch (Exception theException)
            {
                String errorMessage;
                errorMessage = "Error: ";
                errorMessage = String.Concat(errorMessage, theException.Message);
                errorMessage = String.Concat(errorMessage, " Line: ");
                errorMessage = String.Concat(errorMessage, theException.Source);
                MessageBox.Show(errorMessage, "Error");
            }
            workbook.Close(0);
            excel_app.Quit();
        }
    }
}