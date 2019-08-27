using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;

namespace MPS.IIC.Script
{
    /// <summary>
    /// Interaction logic for TXTEditWindow.xaml
    /// </summary>
    public partial class TXTEditWindow
    {
        public TXTEditWindow()
        {
            InitializeComponent();
        }

        private void clear_btn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = System.Windows.MessageBox.Show("Are you sure to clear all content?", "Reconfirm",
                                    MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res.ToString() == "Yes")
            {
                this.text_tb.Clear();
            }
            return;
        }

        private void save_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(text_tb.Text))
            {
                SaveFileDialog sfDialog = new SaveFileDialog();
                sfDialog.Title = "Save";
                sfDialog.Filter = "Script configure file|*.txt";
                //sfDialog.ShowDialog();

                if (sfDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }

                string fileName = sfDialog.FileName;

                FileStream fs = File.Open(fileName, FileMode.Create, FileAccess.Write);
                StreamWriter wr = new StreamWriter(fs);

                for (int i = 0; i < text_tb.LineCount; i++)
                {
                    var line = text_tb.GetLineText(i);
                    //line = line.TrimEnd('\n');
                    wr.Write(line);
                }
                wr.Flush();
                wr.Close();
                fs.Close();
                return;
            }
            else
            {
                System.Windows.MessageBox.Show("Script can not be empty.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            }
        }

        private void format_btn_Click(object sender, RoutedEventArgs e)
        {
            txtFileFormat formatwin = new txtFileFormat();
            formatwin.Show();
        }
    }
}