using MPS.Common.Command;
using MPS.IIC.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace MPS.IIC.Script
{
    class TXTEditWindowView : ObservableObject
    {
        private string _configText;

        public string ConfigText
        {
            get { return _configText; }
            set
            {
                _configText = value;
                RaisePropertyChanged("ConfigText");
            }
        }

        public ICommand DebugbtnClick
        {
            get
            {
                return new RelayCommand(debugMethod);
            }
        }

        private void debugMethod()
        {
            string filePath = null;
            if (!string.IsNullOrEmpty(ConfigText) && !string.IsNullOrWhiteSpace(ConfigText))
            {
                //Auto save file to the default path:
                string filedir = Path.Combine(Environment.CurrentDirectory, "Script Config File");
                string filename = "script_" + DateTime.Now.ToString("MMddHHmm") + ".txt";
                if (!Directory.Exists(filedir))
                {
                    Directory.CreateDirectory(filedir);
                }
                filePath = Path.Combine(filedir, filename);
                FileStream fs = File.Open(filePath, FileMode.Create, FileAccess.Write);
                StreamWriter wr = new StreamWriter(fs);
                wr.Write(ConfigText);
                wr.Flush();
                wr.Close();
                fs.Close();

                //Executing
                TXTDebugWindow txtdebugwin = TXTDebugWindow.CreateInstance(filePath);
                //TXTDebugWindowView txtdebugview = new TXTDebugWindowView(filePath);
                //txtdebugwin.DataContext = txtdebugview;
                //txtdebugview.BrowseMethod();
                txtdebugwin.Browse();
                txtdebugwin.ShowDialog();
            }
            else
            {
                System.Windows.MessageBox.Show("Script can not be empty.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None);
                return;
            }
        }
    }
}

