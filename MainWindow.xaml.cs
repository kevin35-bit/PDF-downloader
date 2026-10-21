using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PDF_downloader
{
    public partial class MainWindow : System.Windows.Window
    {
        //to do
        //.extra- progress bar

        private Dictionary<string, (string, string)> link_list = new();
        private Dictionary<string, (string, string)> got_downloaded = new();
        settings_handler settings = new();
        public MainWindow()
        {
            InitializeComponent();
            
            settings.startup();
            on_startup();
            if (File.Exists(excel_file_path_textbox.Text)) 
                {
                dictonary_n_list_functions.instance_excel_window(excel_file_path_textbox.Text);
                link_list = excel_and_info_handler(excel_file_path_textbox, excel_list, link_list);
            }
            else excel_file_path_textbox.Text = "no_file_selected";
            if (File.Exists(excel_downloaded_list_path_textbox.Text))
            {
                dictonary_n_list_functions.instance_excel_window(excel_downloaded_list_path_textbox.Text);
               got_downloaded = excel_and_info_handler(excel_downloaded_list_path_textbox, Last_download, got_downloaded);
            }
            else excel_file_path_textbox.Text = "no_file_selected";
            if (!Directory.Exists(output_path_textbox.Text)) output_path_textbox.Text = "no_file_selected";
            make_difference_list();
        }

        private void excel_links_btn_Click(object sender, RoutedEventArgs e)
        {
            open_filedialog_and_set_path_text(excel_file_path_textbox);
            link_list = excel_and_info_handler(excel_file_path_textbox, excel_list, link_list);
        }

        private void excel_downloaded_list_path_btn_Click(object sender, RoutedEventArgs e)
        {
            open_filedialog_and_set_path_text(excel_downloaded_list_path_textbox);
            got_downloaded = excel_and_info_handler(excel_downloaded_list_path_textbox, Last_download, got_downloaded);
        }

        private void output_path_btn_Click(object sender, RoutedEventArgs e)
        {
            string dir_name = open_folderdialog_and_set_path_text(output_path_textbox);
            dir_name = string.Join("", dir_name.ToList().RemoveAll(x => System.IO.Path.GetInvalidPathChars().Contains(x)));
            if (!Directory.Exists(dir_name) && !dir_name.Any(x=> System.IO.Path.GetInvalidPathChars().Contains(x)) || dir_name !="") Directory.CreateDirectory(dir_name);
        }

        private void excel_update_btn_Click(object sender, RoutedEventArgs e)
        {
            link_list = excel_and_info_handler(excel_file_path_textbox, excel_list, link_list);
            got_downloaded = excel_and_info_handler(excel_downloaded_list_path_textbox, Last_download, got_downloaded);
            make_difference_list();
        }

        private string open_filedialog_and_set_path_text(System.Windows.Controls.TextBox textBox)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Filter = "excel files (*.xls*)| *.xls*";
            fileDialog.InitialDirectory = File.Exists(textBox.Text) ? string.Join('\\', textBox.Text.Split("\\"), 0, textBox.Text.Split("\\").Count() - 2) : Environment.CurrentDirectory;
            fileDialog.ShowDialog();
            return textBox.Text = fileDialog.FileName;
        }

        private string open_folderdialog_and_set_path_text(System.Windows.Controls.TextBox textBox)
        {
            OpenFolderDialog fileDialog = new OpenFolderDialog();
            fileDialog.Multiselect = false;
            fileDialog.InitialDirectory = File.Exists(textBox.Text) ? string.Join('\\', textBox.Text.Split("\\"), 0, textBox.Text.Split("\\").Count() - 2) : Environment.CurrentDirectory;
            fileDialog.ShowDialog();
            return textBox.Text = fileDialog.FolderName;
        }

        private void download_btn_Click(object sender, RoutedEventArgs e)
        {
            got_downloaded = download_function.GET_PDFs(link_list.Where(x => !got_downloaded.ContainsKey(x.Key)).ToDictionary<string, (string, string)>(), output_path_textbox.Text).Result;
            dictonary_n_list_functions.refresh_excel_listview(Last_download, got_downloaded);
            make_difference_list();
            foreach (KeyValuePair<string, (string, string)> item in dictonary_n_list_functions.instance_excel_window(excel_downloaded_list_path_textbox.Text))
            {
                if (!got_downloaded.Keys.Contains(item.Key)) got_downloaded.Add(item.Key,item.Value);
            }
            int max_length = got_downloaded.Keys.Max(x => int.Parse(x.Trim('.')));
            dictonary_n_list_functions.create_excel_output(excel_downloaded_list_path_textbox , got_downloaded, max_length);
        }

        private Dictionary<string, (string, string)> excel_and_info_handler(System.Windows.Controls.TextBox textBox, System.Windows.Controls.ListBox listBox, Dictionary<string, (string, string)> list)
        {
            if (!File.Exists(textBox.Text))
            {
                if (textBox.Text != "") MessageBox.Show("selected file in "+textBox.Name+" doesn't exixt","load error");
                return list;
            }
            list = dictonary_n_list_functions.instance_excel_window(textBox.Text);
            listBox = dictonary_n_list_functions.refresh_excel_listview(listBox, list);
            return list;
        }

        void make_difference_list()
        {
            if (Last_download.Items.Count > 0)
            {
                Dictionary<string, (string, string)> temp_dictionary = new(link_list.Where(x => !got_downloaded.ContainsKey(x.Key)));
                dictonary_n_list_functions.refresh_excel_listview(not_downloaded, temp_dictionary);
            }
            else
            {
                not_downloaded.Items.Clear();
                foreach (var item in excel_list.Items)
                {
                    not_downloaded.Items.Add(item);
                }
            }
            not_downloaded_count_label.Content = not_downloaded.Items.Count.ToString();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            settings.save_setting([
            "excel_file_path¤"+excel_file_path_textbox.Text,
            "downloaded_excel_file¤"+excel_downloaded_list_path_textbox.Text,
            "output_path¤"+output_path_textbox.Text         
            ]);
        }
        private void on_startup()
        {
            excel_file_path_textbox.Text = settings.get_value("excel_file_path");
            excel_downloaded_list_path_textbox.Text = settings.get_value("downloaded_excel_file");
            output_path_textbox.Text = settings.get_value("output_path");
        }

        private void reset_btn_Click(object sender, RoutedEventArgs e)
        {
            dictonary_n_list_functions.create_excel_output(excel_downloaded_list_path_textbox, new(),0);
            dictonary_n_list_functions.refresh_excel_listview(Last_download, got_downloaded);
            got_downloaded = excel_and_info_handler(excel_downloaded_list_path_textbox, Last_download, got_downloaded);
            make_difference_list();
        }
    }
}