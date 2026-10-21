using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PDF_downloader
{
    /// <summary>
    /// Interaction logic for progress_bar.xaml
    /// </summary>
    public partial class progress_bar : Window
    {

        //ProgressBar HorizontalAlignment="Left" Height="27" Margin="56,56,0,0" VerticalAlignment="Top" Width="177"
        string filename;
        public progress_bar(string filename)
        {
            InitializeComponent();
            filename = filename;
            label.Content = filename;
        }

        public void update_progress(int value, int max_value)
        {
            download_progressbar.Maximum = max_value;
            download_progressbar.Value = value;
            label.Content = filename + ": " + value / max_value * 100 + "%";
        }

        public void close_window()
        {
            this.Close();
        }
    }
}
