using MahApps.Metro.Controls;
using Microsoft.Win32;
using MPS.Instrument;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

//namespace MPS.IIC.Script
//{
//    /// <summary>
//    /// Interaction logic for TXTDebugWindow.xaml
//    /// </summary>
//    public partial class TXTDebugWindow
//    {
//        private ObservableCollection<TXTCommandInfos> AlltxtCommands = new ObservableCollection<TXTCommandInfos>();
//        private Thread CurrentThread = null;

//        //public TXTDebugWindow()
//        //{
//        //    InitializeComponent();
//        //}

//        //Singleton 线程不安全
//        private static TXTDebugWindow _instance = null;
//        private string path;
//        private TXTDebugWindow(string filepath = null)
//        {
//            InitializeComponent();
//            path = filepath;
//            datagrid.DataContext = AlltxtCommands;
//            this.Closed += TXTDebugWindow_Closed;
//        }
//        public static TXTDebugWindow CreateInstance(string filepath = null)
//        {
//            if (_instance == null)
//            {
//                _instance = new TXTDebugWindow(filepath);
//            }
//            else
//            {
//                _instance.path = filepath;
//                _instance.Activate();
//            }
//            return _instance;

//        }

//        private void TXTDebugWindow_Closed(object sender, EventArgs e)
//        {
//            _instance = null;
//        }

//        //Singleton 线程安全--readonly  但在释放单例之前不能再次打开
//        //private TXTDebugWindow()
//        //{
//        //    InitializeComponent();
//        //}
//        // public static readonly TXTDebugWindow instance = new TXTDebugWindow();

//        private void Browse_Click(object sender, RoutedEventArgs e)
//        {
//            path = null;
//            Browse();
//        }

//        #region method
//        public void Browse()
//        {
//            if (CurrentThread != null)
//            {
//                CurrentThread.Abort();
//            }

//            Thread mThread = new System.Threading.Thread(new System.Threading.ThreadStart(
//            () =>
//            {
//                Thread.CurrentThread.Name = "mThread";
//                CurrentThread = Thread.CurrentThread;

//                //bool ErrorOccurs = false;
//                int index = 0;
//                try
//                {
//                    if (path == null)
//                    {
//                        path = OpenFile();
//                        if (!File.Exists(path)) throw new Exception("File don't exists!");

//                    }
//                    Application.Current.Dispatcher.Invoke(new Action(() => {
//                        this.path_tb.Text = path;
//                        AlltxtCommands.Clear();
//                        this.statusbar.Content = null;
//                        statusbar.Background = Brushes.LightYellow;
//                    }));

//                    #region parse commands
//                    using (StreamReader sr = new StreamReader(path))
//                    {
//                        string content = sr.ReadToEnd();
//                        if (string.IsNullOrEmpty(content) || string.IsNullOrWhiteSpace(content))
//                        {
//                            throw new Exception("Script File is empty.");
//                        }

//                        string[] _contentInfos = content.Split(new char[] { '\r', '\n' });
//                        _contentInfos = _contentInfos.Where(s => (!string.IsNullOrEmpty(s)) && (!string.IsNullOrWhiteSpace(s))).ToArray();  //去掉空行

//                        #region Parse LOOP-ENDLOOP
//                        List<string> contentInfos = new List<string>();   //用于保存loop后的命令行
//                        List<int> rowIDs = new List<int>();  //用来标记原文本中的行数，与contentInfos对应

//                        for (int j = 0; j < _contentInfos.Length; j++)
//                        {
//                            var str = _contentInfos[j].Trim();
//                            var strInfos = str.Split(new char[] { ' ', '=' }).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
//                            if (strInfos[0].ToLower() == "loop")
//                            {
//                                var times = int.Parse(strInfos[1]);

//                                var k = j + 1;
//                                var s = _contentInfos[k].ToLower();
//                                while (s.Trim() != "end_loop")
//                                {
//                                    k++;
//                                    s = _contentInfos[k].ToLower();  //若超出范围仍未找到endloop 这里报访问越界的错
//                                }

//                                var looptemps = _contentInfos.Skip(j + 1).Take(k - j - 1).ToArray();
//                                for (int m = 0; m < times; m++)
//                                {
//                                    for (int itemID = 0; itemID < looptemps.Length; itemID++)
//                                    {
//                                        contentInfos.Add(looptemps[itemID]);
//                                        rowIDs.Add(j + 2 + itemID);
//                                    }
//                                }
//                                j = k;
//                            }
//                            else
//                            {
//                                contentInfos.Add(_contentInfos[j]);
//                                rowIDs.Add(j + 1);
//                            }
//                        }
//                        #endregion

//                        byte slaveAddr = 0;
//                        TXTCommandInfos temptxtCommand = new TXTCommandInfos();

//                        for (var id = 0; id < contentInfos.ToArray().Length; id++)
//                        {
//                            #region execute command

//                            var str = contentInfos[id].Trim();
//                            index = rowIDs[id];
//                            temptxtCommand = new TXTCommandInfos();

//                            var strInfos = str.Split(new char[] { ' ', '=' });
//                            strInfos = strInfos.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

//                            // set address
//                            if (index == 1)
//                            {
//                                if (strInfos.Length == 3 && strInfos[0].ToLower() == "set" && strInfos[1].ToLower() == "address")
//                                {

//                                    var addr = strInfos[2];
//                                    slaveAddr = Convert.ToByte(addr, 16);
//                                    temptxtCommand.Operation = "SET ADDRESS";
//                                    temptxtCommand.ExpectedValue = addr;
//                                    if (!string.IsNullOrEmpty(temptxtCommand.RegAddr) || !string.IsNullOrEmpty(temptxtCommand.Operation) || !string.IsNullOrEmpty(temptxtCommand.ExpectedValue) || !string.IsNullOrEmpty(temptxtCommand.ReadbackValue) || !string.IsNullOrEmpty(temptxtCommand.Check))
//                                    {
//                                        SaveResult2TXT(temptxtCommand);
//                                        datagrid.Invoke(new Action(() =>
//                                        {
//                                            AlltxtCommands.Add(temptxtCommand);
//                                        }));
//                                    }
//                                    continue;
//                                }
//                                else
//                                {
//                                    throw new Exception("Must set address first.");
//                                }
//                            }

//                            if (strInfos.Length == 3 && strInfos[0].ToLower() == "set" && strInfos[1].ToLower() == "address")
//                            {
//                                var addr = strInfos[2];
//                                slaveAddr = Convert.ToByte(addr, 16);
//                                temptxtCommand.Operation = "SET ADDRESS";
//                                temptxtCommand.ExpectedValue = addr;
//                                if (!string.IsNullOrEmpty(temptxtCommand.RegAddr) || !string.IsNullOrEmpty(temptxtCommand.Operation) || !string.IsNullOrEmpty(temptxtCommand.ExpectedValue) || !string.IsNullOrEmpty(temptxtCommand.ReadbackValue) || !string.IsNullOrEmpty(temptxtCommand.Check))
//                                {
//                                    SaveResult2TXT(temptxtCommand);
//                                    datagrid.Invoke(new Action(() =>
//                                    {
//                                        AlltxtCommands.Add(temptxtCommand);
//                                    }));
//                                }
//                                continue;
//                            }

//                            if (strInfos.Length == 3)
//                            {
//                                temptxtCommand.RegAddr = strInfos[0].PadLeft(2, '0');
//                                temptxtCommand.Operation = strInfos[1].ToUpper();

//                                var regAddr = Convert.ToByte(strInfos[0], 16);
//                                var format = strInfos[1];

//                                switch (format.ToLower())
//                                {
//                                    case "write_byte":
//                                        if (slaveAddr == 48 && regAddr == 209)   // MPQ8645P reg-D1：Vout Command Range
//                                        {
//                                            byte temp;
//                                            DeviceObject.Devices.PMBus.ReadByte(slaveAddr, regAddr, out temp);
//                                            int temp_ = temp & 252;   //temp & 1111 1100
//                                            temp_ += Convert.ToByte(strInfos[2], 16);
//                                            temptxtCommand.ExpectedValue = temp_.ToString().PadLeft(2, '0');
//                                            DeviceObject.Devices.PMBus.WriteByte(slaveAddr, regAddr, (byte)temp_);
//                                        }
//                                        else
//                                        {
//                                            DeviceObject.Devices.PMBus.WriteByte(slaveAddr, regAddr, Convert.ToByte(strInfos[2], 16));
//                                            temptxtCommand.ExpectedValue = strInfos[2].PadLeft(2, '0');
//                                        }
//                                        break;
//                                    case "write_word":
//                                        //ushort value2;
//                                        DeviceObject.Devices.PMBus.WriteWord(slaveAddr, regAddr, Convert.ToUInt16(strInfos[2], 16));
//                                        //DeviceObject.Devices.PMBus.ReadWord(slaveAddr, regAddr, out value2);
//                                        temptxtCommand.ExpectedValue = strInfos[2].PadLeft(4, '0');
//                                        //temptxtCommand.ReadbackValue = StringConvert(value2);
//                                        break;
//                                    case "read_byte":
//                                        byte value3 = 0;
//                                        DeviceObject.Devices.PMBus.ReadByte(slaveAddr, regAddr, out value3);
//                                        temptxtCommand.ExpectedValue = strInfos[2].PadLeft(2, '0');
//                                        temptxtCommand.ReadbackValue = StringConvert(value3);
//                                        temptxtCommand.Check = (temptxtCommand.ExpectedValue.ToUpper() == temptxtCommand.ReadbackValue).ToString();
//                                        break;
//                                    case "read_word":
//                                        ushort value4;
//                                        DeviceObject.Devices.PMBus.ReadWord(slaveAddr, regAddr, out value4);
//                                        temptxtCommand.ExpectedValue = strInfos[2].PadLeft(4, '0');
//                                        temptxtCommand.ReadbackValue = StringConvert(value4);
//                                        temptxtCommand.Check = (temptxtCommand.ExpectedValue.ToUpper() == temptxtCommand.ReadbackValue).ToString();
//                                        break;
//                                    default:
//                                        throw new Exception();
//                                }

//                            }
//                            else if (strInfos.Length == 2 && strInfos[0].ToLower() == "delay")
//                            {
//                                temptxtCommand.Operation = strInfos[0].ToUpper();
//                                temptxtCommand.ExpectedValue = strInfos[1];

//                                int delaytime;
//                                //int.TryParse(strInfos[1], out delaytime);
//                                delaytime = int.Parse(strInfos[1]);
//                                //delay
//                                System.Threading.Thread.Sleep(delaytime);
//                            }
//                            else if (strInfos.Length == 2 && strInfos[1].ToLower() == "send")
//                            {
//                                temptxtCommand.Operation = strInfos[1].ToUpper();
//                                temptxtCommand.RegAddr = strInfos[0].PadLeft(2, '0');
//                                var regAddr = Convert.ToByte(strInfos[0], 16);
//                                DeviceObject.Devices.PMBus.SendCommand(slaveAddr, regAddr);
//                            }
//                            else
//                            {
//                                throw new Exception();
//                            }
//                            #endregion
//                            if (!string.IsNullOrEmpty(temptxtCommand.RegAddr) || !string.IsNullOrEmpty(temptxtCommand.Operation) || !string.IsNullOrEmpty(temptxtCommand.ExpectedValue) || !string.IsNullOrEmpty(temptxtCommand.ReadbackValue) || !string.IsNullOrEmpty(temptxtCommand.Check))
//                            {
//                                SaveResult2TXT(temptxtCommand);
//                                datagrid.Invoke(new Action(() =>
//                                {
//                                    AlltxtCommands.Add(temptxtCommand);
//                                }));
//                            }
//                        }

//                    }
//                    #endregion
//                    statusbar.Invoke(new Action(() =>
//                    {
//                        statusbar.Content = " Finished";
//                        statusbar.Background = Brushes.LightBlue;

//                    }));
//                }
//                catch (Exception ex)
//                {
//                    if (ex is ThreadAbortException)
//                    {
//                        statusbar.Invoke(new Action(() =>
//                        {
//                            statusbar.Content = "Current commands has been terminated";
//                            statusbar.Background = Brushes.LightCoral;
//                        }));
//                    }
//                    else if (index == 0)
//                    {
//                        statusbar.Invoke(new Action(() =>
//                        {
//                            statusbar.Content = " Error: 'LOOP' and 'END_LOOP' mismatch.";
//                            statusbar.Background = Brushes.LightCoral;
//                        }));
//                    }
//                    else if (index == 1)
//                    {
//                        statusbar.Invoke(new Action(() =>
//                        {
//                            statusbar.Content = string.Format(" Error in Line 1:  Please check the command format. " + ex.Message);
//                            statusbar.Background = Brushes.LightCoral;
//                        }));
//                    }
//                    else
//                    {
//                        statusbar.Invoke(new Action(() =>
//                        {
//                            statusbar.Content = string.Format(" Error in Line {0}:  Please check the command format.", index);
//                            statusbar.Background = Brushes.LightCoral;
//                        }));
//                    }
//                }
//                finally
//                {
//                    CurrentThread = null;
//                }
//            }));

//            mThread.Start();
//        }


//        public string OpenFile()
//        {
//            var openDiag = new OpenFileDialog
//            {
//                Filter = "(MPS Debug script File)|*.txt",
//                Title = "Load MPS Debug script file",
//            };
//            var showDialog = openDiag.ShowDialog();
//            if (showDialog == null || !(bool)showDialog)
//            {
//                throw new Exception("Open File Failed!");
//            }
//            var path = openDiag.FileName;

//            return path;
//        }

//        public string StringConvert(byte value)
//        {
//            string res = "";
//            res = Convert.ToString(value, 16).PadLeft(2, '0');
//            return res.ToUpper();
//        }
//        public string StringConvert(ushort value)
//        {
//            string res = "";
//            res = Convert.ToString(value, 16).PadLeft(4, '0');
//            return res.ToUpper();
//        }

//        private void SaveResult2TXT(TXTCommandInfos infos)
//        {
//            string resdir = System.IO.Path.Combine(Environment.CurrentDirectory, "Script Result");
//            string filename = "Result_" + DateTime.Now.ToString("MMddHHmm") + ".txt";
//            if (!Directory.Exists(resdir))
//            {
//                Directory.CreateDirectory(resdir);
//            }
//            var respath = System.IO.Path.Combine(resdir, filename);

//            string res = string.Format("{0}\t{1}\t{2}\t{3}\t{4}", infos.RegAddr, infos.Operation, infos.ExpectedValue, infos.ReadbackValue, infos.Check);
//            using (StreamWriter sw = new StreamWriter(respath, true))
//            {
//                sw.WriteLine(res);
//            }
//        }
//        #endregion
//    }
//}


namespace MPS.IIC.Script
{
    /// <summary>
    /// Interaction logic for TXTDebugWindow.xaml
    /// </summary>
    public partial class TXTDebugWindow
    {
        private ObservableCollection<TXTCommandInfos> AlltxtCommands = new ObservableCollection<TXTCommandInfos>();
        private Thread CurrentThread = null;

        //public TXTDebugWindow()
        //{
        //    InitializeComponent();
        //}

        //Singleton 线程不安全
        private static TXTDebugWindow _instance = null;
        private string path;
        private TXTDebugWindow(string filepath = null)
        {
            InitializeComponent();
            path = filepath;
            datagrid.DataContext = AlltxtCommands;
            this.Closed += TXTDebugWindow_Closed;
        }
        public static TXTDebugWindow CreateInstance(string filepath = null)
        {
            if (_instance == null)
            {
                _instance = new TXTDebugWindow(filepath);
            }
            else
            {
                _instance.path = filepath;
                _instance.Activate();
            }
            return _instance;

        }

        private void TXTDebugWindow_Closed(object sender, EventArgs e)
        {
            _instance = null;
        }

        //Singleton 线程安全--readonly  但在释放单例之前不能再次打开
        //private TXTDebugWindow()
        //{
        //    InitializeComponent();
        //}
        // public static readonly TXTDebugWindow instance = new TXTDebugWindow();

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            path = null;
            Browse();
        }

        #region method
        public void Browse()
        {
            if (CurrentThread != null)
            {
                CurrentThread.Abort();
            }

            Thread mThread = new System.Threading.Thread(new System.Threading.ThreadStart(
            () =>
            {
                Thread.CurrentThread.Name = "mThread";
                CurrentThread = Thread.CurrentThread;

                //bool ErrorOccurs = false;
                int index = 0;
                try
                {
                    if (path == null)
                    {
                        path = OpenFile();
                        if (!File.Exists(path)) throw new Exception("File don't exists!");

                    }
                    Application.Current.Dispatcher.Invoke(new Action(() => {
                        this.path_tb.Text = path;
                        AlltxtCommands.Clear();
                        this.statusbar.Content = null;
                        statusbar.Background = Brushes.LightYellow;
                    }));

                    #region parse commands
                    using (StreamReader sr = new StreamReader(path))
                    {
                        string content = sr.ReadToEnd();
                        if (string.IsNullOrEmpty(content) || string.IsNullOrWhiteSpace(content))
                        {
                            throw new Exception("Script File is empty.");
                        }

                        string[] _contentInfos = content.Split(new char[] { '\r', '\n' });
                        _contentInfos = _contentInfos.Where(s => (!string.IsNullOrEmpty(s)) && (!string.IsNullOrWhiteSpace(s))).ToArray();  //去掉空行

                        #region Parse LOOP-ENDLOOP
                        List<string> contentInfos = new List<string>();   //用于保存loop后的命令行
                        List<int> rowIDs = new List<int>();  //用来标记原文本中的行数，与contentInfos对应

                        for (int j = 0; j < _contentInfos.Length; j++)
                        {
                            var str = _contentInfos[j].Trim();
                            //Parse注释
                            str = str.Split('#')[0].Trim();
                            //
                            var strInfos = str.Split(new char[] { ' ', '=' }).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                            if (strInfos.Length != 0 && strInfos[0].ToLower() == "loop")
                            {
                                var times = int.Parse(strInfos[1]);

                                var k = j + 1;
                                var s = _contentInfos[k].ToLower();
                                s = s.Split('#')[0].Trim();
                                while (s != "end_loop")
                                {
                                    k++;
                                    s = _contentInfos[k].ToLower();  //若超出范围仍未找到endloop 这里报访问越界的错
                                    s = s.Split('#')[0].Trim();
                                }

                                var looptemps = _contentInfos.Skip(j + 1).Take(k - j - 1).ToArray();
                                for (int m = 0; m < times; m++)
                                {
                                    for (int itemID = 0; itemID < looptemps.Length; itemID++)
                                    {
                                        var item = looptemps[itemID];
                                        item = item.Split('#')[0].Trim();
                                        contentInfos.Add(item);
                                        rowIDs.Add(j + 2 + itemID);
                                    }
                                }
                                j = k;
                            }
                            else if (strInfos.Length != 0)
                            {
                                //contentInfos.Add(_contentInfos[j]);
                                contentInfos.Add(str);
                                rowIDs.Add(j + 1);
                            }
                        }
                        #endregion

                        byte slaveAddr = 0;
                        TXTCommandInfos temptxtCommand = new TXTCommandInfos();

                        for (var id = 0; id < contentInfos.ToArray().Length; id++)
                        {
                            #region execute command

                            var str = contentInfos[id].Trim();
                            index = rowIDs[id];
                            temptxtCommand = new TXTCommandInfos();

                            var strInfos = str.Split(new char[] { ' ', '=' });
                            strInfos = strInfos.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

                            // set address
                            if (index == 1)
                            {
                                if (strInfos.Length == 3 && strInfos[0].ToLower() == "set" && strInfos[1].ToLower() == "address")
                                {
                                    var addr = strInfos[2];
                                    slaveAddr = Convert.ToByte(addr, 16);
                                    temptxtCommand.Operation = "SET ADDRESS";
                                    temptxtCommand.ExpectedValue = addr;
                                    if (!string.IsNullOrEmpty(temptxtCommand.RegAddr) || !string.IsNullOrEmpty(temptxtCommand.Operation) || !string.IsNullOrEmpty(temptxtCommand.ExpectedValue) || !string.IsNullOrEmpty(temptxtCommand.ReadbackValue) || !string.IsNullOrEmpty(temptxtCommand.Check))
                                    {
                                        SaveResult2TXT(temptxtCommand);
                                        datagrid.Invoke(new Action(() =>
                                        {
                                            AlltxtCommands.Add(temptxtCommand);
                                        }));
                                    }
                                    continue;
                                }
                                else
                                {
                                    throw new Exception("Must set address first.");
                                }
                            }

                            if (strInfos.Length == 3 && strInfos[0].ToLower() == "set" && strInfos[1].ToLower() == "address")
                            {
                                var addr = strInfos[2];
                                slaveAddr = Convert.ToByte(addr, 16);
                                temptxtCommand.Operation = "SET ADDRESS";
                                temptxtCommand.ExpectedValue = addr;
                                if (!string.IsNullOrEmpty(temptxtCommand.RegAddr) || !string.IsNullOrEmpty(temptxtCommand.Operation) || !string.IsNullOrEmpty(temptxtCommand.ExpectedValue) || !string.IsNullOrEmpty(temptxtCommand.ReadbackValue) || !string.IsNullOrEmpty(temptxtCommand.Check))
                                {
                                    SaveResult2TXT(temptxtCommand);
                                    datagrid.Invoke(new Action(() =>
                                    {
                                        AlltxtCommands.Add(temptxtCommand);
                                    }));
                                }
                                continue;
                            }

                            if (strInfos.Length == 3)
                            {
                                temptxtCommand.RegAddr = strInfos[0].PadLeft(2, '0');
                                temptxtCommand.Operation = strInfos[1].ToUpper();

                                var regAddr = Convert.ToByte(strInfos[0], 16);
                                var format = strInfos[1];

                                switch (format.ToLower())
                                {
                                    case "write_byte":
                                        if (slaveAddr == 48 && regAddr == 209)   // MPQ8645P reg-D1：Vout Command Range
                                        {
                                            byte temp;
                                            DeviceObject.Devices.PMBus.ReadByte(slaveAddr, regAddr, out temp);
                                            int temp_ = temp & 252;   //temp & 1111 1100
                                            temp_ += Convert.ToByte(strInfos[2], 16);
                                            temptxtCommand.ExpectedValue = temp_.ToString().PadLeft(2, '0');
                                            DeviceObject.Devices.PMBus.WriteByte(slaveAddr, regAddr, (byte)temp_);
                                        }
                                        else
                                        {
                                            DeviceObject.Devices.PMBus.WriteByte(slaveAddr, regAddr, Convert.ToByte(strInfos[2], 16));
                                            temptxtCommand.ExpectedValue = strInfos[2].PadLeft(2, '0');
                                        }
                                        break;
                                    case "write_word":
                                        //ushort value2;
                                        DeviceObject.Devices.PMBus.WriteWord(slaveAddr, regAddr, Convert.ToUInt16(strInfos[2], 16));
                                        //DeviceObject.Devices.PMBus.ReadWord(slaveAddr, regAddr, out value2);
                                        temptxtCommand.ExpectedValue = strInfos[2].PadLeft(4, '0');
                                        //temptxtCommand.ReadbackValue = StringConvert(value2);
                                        break;
                                    case "read_byte":
                                        byte value3 = 0;
                                        DeviceObject.Devices.PMBus.ReadByte(slaveAddr, regAddr, out value3);
                                        temptxtCommand.ExpectedValue = strInfos[2].PadLeft(2, '0');
                                        temptxtCommand.ReadbackValue = StringConvert(value3);
                                        temptxtCommand.Check = (temptxtCommand.ExpectedValue.ToUpper() == temptxtCommand.ReadbackValue).ToString();
                                        break;
                                    case "read_word":
                                        ushort value4;
                                        DeviceObject.Devices.PMBus.ReadWord(slaveAddr, regAddr, out value4);
                                        temptxtCommand.ExpectedValue = strInfos[2].PadLeft(4, '0');
                                        temptxtCommand.ReadbackValue = StringConvert(value4);
                                        temptxtCommand.Check = (temptxtCommand.ExpectedValue.ToUpper() == temptxtCommand.ReadbackValue).ToString();
                                        break;
                                    default:
                                        throw new Exception();
                                }

                            }
                            else if (strInfos.Length == 2 && strInfos[0].ToLower() == "delay")
                            {
                                temptxtCommand.Operation = strInfos[0].ToUpper();
                                temptxtCommand.ExpectedValue = strInfos[1];

                                int delaytime;
                                //int.TryParse(strInfos[1], out delaytime);
                                delaytime = int.Parse(strInfos[1]);
                                //delay
                                System.Threading.Thread.Sleep(delaytime);
                            }
                            else if (strInfos.Length == 2 && strInfos[1].ToLower() == "send")
                            {
                                temptxtCommand.Operation = strInfos[1].ToUpper();
                                temptxtCommand.RegAddr = strInfos[0].PadLeft(2, '0');
                                var regAddr = Convert.ToByte(strInfos[0], 16);
                                DeviceObject.Devices.PMBus.SendCommand(slaveAddr, regAddr);
                            }
                            else
                            {
                                throw new Exception();
                            }
                            #endregion
                            if (!string.IsNullOrEmpty(temptxtCommand.RegAddr) || !string.IsNullOrEmpty(temptxtCommand.Operation) || !string.IsNullOrEmpty(temptxtCommand.ExpectedValue) || !string.IsNullOrEmpty(temptxtCommand.ReadbackValue) || !string.IsNullOrEmpty(temptxtCommand.Check))
                            {
                                //保存结果
                                SaveResult2TXT(temptxtCommand);
                                //

                                datagrid.Invoke(new Action(() =>
                                {
                                    AlltxtCommands.Add(temptxtCommand);
                                }));
                            }
                        }

                    }
                    #endregion
                    statusbar.Invoke(new Action(() =>
                    {
                        statusbar.Content = " Finished";
                        statusbar.Background = Brushes.LightBlue;

                    }));
                }
                catch (Exception ex)
                {
                    if (ex is ThreadAbortException)
                    {
                        statusbar.Invoke(new Action(() =>
                        {
                            statusbar.Content = "Current commands has been terminated";
                            statusbar.Background = Brushes.LightCoral;
                        }));
                    }
                    else if (index == 0)
                    {
                        statusbar.Invoke(new Action(() =>
                        {
                            statusbar.Content = " Error: 'LOOP' and 'END_LOOP' mismatch.";
                            statusbar.Background = Brushes.LightCoral;
                        }));
                    }
                    else if (index == 1)
                    {
                        statusbar.Invoke(new Action(() =>
                        {
                            statusbar.Content = string.Format(" Error in Line 1:  Please check the command format. " + ex.Message);
                            statusbar.Background = Brushes.LightCoral;
                        }));
                    }
                    else
                    {
                        statusbar.Invoke(new Action(() =>
                        {
                            statusbar.Content = string.Format(" Error in Line {0}:  Please check the command format.", index);
                            statusbar.Background = Brushes.LightCoral;
                        }));
                    }
                }
                finally
                {
                    CurrentThread = null;
                }
            }));

            mThread.Start();
        }


        public string OpenFile()
        {
            var openDiag = new OpenFileDialog
            {
                Filter = "(MPS Debug script File)|*.txt",
                Title = "Load MPS Debug script file",
            };
            var showDialog = openDiag.ShowDialog();
            if (showDialog == null || !(bool)showDialog)
            {
                throw new Exception("Open File Failed!");
            }
            var path = openDiag.FileName;

            return path;
        }

        public string StringConvert(byte value)
        {
            string res = "";
            res = Convert.ToString(value, 16).PadLeft(2, '0');
            return res.ToUpper();
        }
        public string StringConvert(ushort value)
        {
            string res = "";
            res = Convert.ToString(value, 16).PadLeft(4, '0');
            return res.ToUpper();
        }

        private void SaveResult2TXT(TXTCommandInfos infos)
        {
            string resdir = System.IO.Path.Combine(Environment.CurrentDirectory, "Script Result");
            string filename = "Result_" + DateTime.Now.ToString("MMddHHmm") + ".txt";
            if (!Directory.Exists(resdir))
            {
                Directory.CreateDirectory(resdir);
            }
            var respath = System.IO.Path.Combine(resdir, filename);

            string res = string.Format("{0}\t{1}\t{2}\t{3}\t{4}", infos.RegAddr, infos.Operation, infos.ExpectedValue, infos.ReadbackValue, infos.Check);
            using (StreamWriter sw = new StreamWriter(respath, true))
            {
                sw.WriteLine(res);
            }
        }
        #endregion
    }
}

