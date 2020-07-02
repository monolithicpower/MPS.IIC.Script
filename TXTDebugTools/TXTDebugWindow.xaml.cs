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
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

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
        string[] contentInfos;
        string savepath;
        //public TXTDebugWindow()
        //{
        //    InitializeComponent();
        //}

        //Singleton 线程不安全
        private static TXTDebugWindow _instance = null;
        private string path;
        private string respath;
        private TXTDebugWindow(string filepath = null)
        {
            InitializeComponent();
            path = filepath;
            datagrid.DataContext = AlltxtCommands;
            this.Closing += TXTDebugWindow_Closing;
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

        private void TXTDebugWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!e.Cancel)
            {
                if (CurrentThread != null)
                {
                    CurrentThread.Abort();
                }
            }
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

        
        public void Browse()
        {
            if (CurrentThread != null)
            {
                CurrentThread.Abort();
            }

            if (path == null)
            {
                path = OpenFile();
                if (path == "")
                    return;
                
                if (!File.Exists(path)) throw new Exception("File don't exists!");
            }
            string filename = Regex.Split(path, "\\\\", RegexOptions.IgnoreCase).Last();
            var resname = "Result-" + filename;
            respath = path.Replace(filename, resname);
            if (File.Exists(respath))
            {
                File.Delete(respath);
            }

            this.path_tb.Text = path;
            AlltxtCommands.Clear();
            this.statusbar.Content = null;
            statusbar.Background = Brushes.LightYellow;
            

            Thread mThread = new System.Threading.Thread(new System.Threading.ThreadStart(
            () =>
            {
                Thread.CurrentThread.Name = "mThread";
                CurrentThread = Thread.CurrentThread;
                
                try
                {
                    using (StreamReader sr = new StreamReader(path))
                    {
                        string content = sr.ReadToEnd();
                        if (string.IsNullOrEmpty(content) || string.IsNullOrWhiteSpace(content))
                        {
                            throw new Exception("Script File is empty.");
                        }

                        contentInfos = content.Split(new char[] { '\r', '\n' });
                        for (int i = 0; i < contentInfos.Length; i++)
                        {
                            var line = contentInfos[i];
                            if (line.Contains("#"))
                            {
                                var tmp = line.Split('#')[0].Trim();
                                contentInfos[i] = tmp;
                            }
                            contentInfos[i] = contentInfos[i].Trim();
                        }
                        contentInfos = contentInfos.Where(s => (!string.IsNullOrEmpty(s)) && (!string.IsNullOrWhiteSpace(s))).ToArray();  //去掉空行
                        statusbar.Invoke(new Action(() => { statusbar.Content = " Executing..."; }));

                        #region Parse Content
                        var Id = 0;
                        while (Id < contentInfos.ToArray().Length)
                        {
                            Id = ExecuteCommand(Id);  //executing
                        }
                        #endregion
                    }
                    statusbar.Invoke(new Action(() =>
                    {
                        statusbar.Content = " Finished";
                        statusbar.Background = Brushes.LightBlue;

                    }));
                }
                catch (Exception ex)
                {
                    statusbar.Invoke(new Action(() =>
                    {
                        statusbar.Content = ex.Message;
                        statusbar.Background = Brushes.LightCoral;
                    }));

                }
                finally
                {
                    CurrentThread = null;
                }
            }));

            mThread.Start();
        }

        #region method
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
                //MessageBox.Show("Open File Failed!");
                return "";
            }
            var path = openDiag.FileName;

            return path;
        }

        [DllImport("kernel32.dll")]
        extern static short QueryPerformanceCounter(ref long x);
        [DllImport("kernel32.dll")]
        extern static short QueryPerformanceFrequency(ref long x);
        //定义延迟函数  us
        public void usDelay(long usdelay)
        {
            long stop_Value = 0;
            long start_Value = 0;
            long freq = 0;
            long n = 0;

            QueryPerformanceFrequency(ref freq);  //获取CPU频率
            long count = usdelay * freq / 1000000;   //这里写成1,000,000就是微秒，写成1,000就是毫秒
            QueryPerformanceCounter(ref start_Value); //获取初始前值

            while (n < count) //不能精确判定
            {
                QueryPerformanceCounter(ref stop_Value);//获取终止变量值
                n = stop_Value - start_Value;
            }
        }


        public int ExecuteCommand(int currentId)
        {
            try
            {
                var cmd = contentInfos[currentId].Trim().Split(new char[] { ' ' });
                cmd = cmd.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

                var txtCommand = new TXTCommandInfos() { Operation = contentInfos[currentId].Trim() };
                switch (cmd[0])
                {
                    case "readbyte":
                        cmd = ReplaceVariable(cmd);
                        byte value;
                        DeviceObject.Devices.PMBus.ReadByte(ToByte(cmd[1]), ToByte(cmd[2]), out value);

                        if (cmd.Length == 4)  //readbyte 01h D1h 80h
                        {
                            txtCommand.ExpectedValue = cmd[3];
                        }
                        else if (cmd.Length == 5)    //readbyte 01h D1h to value
                        {
                            var vkey = cmd[cmd.Length - 1];
                            varDic[vkey] = value;
                        }
                        else if (cmd.Length == 6)    //readbyte 01h D1h 80h to value
                        {
                            var vkey = cmd[cmd.Length - 1];
                            varDic[vkey] = value;
                            txtCommand.ExpectedValue = cmd[3];
                        }
                        txtCommand.ReadbackValue = value.ToString("X2") + 'h';
                        txtCommand.Check = (txtCommand.ExpectedValue == null || txtCommand.ReadbackValue == txtCommand.ExpectedValue).ToString();
                        datagrid.Invoke(new Action(() => 
                        {
                            AlltxtCommands.Add(txtCommand);
                            datagrid.ScrollIntoView(txtCommand);
                        }));
                        currentId++;
                        break;
                    case "writebyte":
                        cmd = ReplaceVariable(cmd);
                        DeviceObject.Devices.PMBus.WriteByte(ToByte(cmd[1]), ToByte(cmd[2]), ToByte(cmd[3]));
                        datagrid.Invoke(new Action(() => 
                        {
                            AlltxtCommands.Add(txtCommand);
                            datagrid.ScrollIntoView(txtCommand);
                        }));
                        currentId++;
                        break;
                    case "readword":
                        cmd = ReplaceVariable(cmd);
                        ushort value1;
                        DeviceObject.Devices.PMBus.ReadWord(ToByte(cmd[1]), ToByte(cmd[2]), out value1);
                        txtCommand.ExpectedValue = cmd[3].PadLeft(4, '0');
                        txtCommand.ReadbackValue = value1.ToString("X4") + 'h';
                        txtCommand.Check = (txtCommand.ReadbackValue == txtCommand.ExpectedValue).ToString();
                        datagrid.Invoke(new Action(() => 
                        {
                            AlltxtCommands.Add(txtCommand);
                            datagrid.ScrollIntoView(txtCommand);
                        }));
                        currentId++;
                        break;
                    case "writeword":
                        cmd = ReplaceVariable(cmd);
                        DeviceObject.Devices.PMBus.WriteWord(ToByte(cmd[1]), ToByte(cmd[2]), ToWord(cmd[3]));
                        datagrid.Invoke(new Action(() => 
                        {
                            AlltxtCommands.Add(txtCommand);
                            datagrid.ScrollIntoView(txtCommand);
                        }));
                        currentId++;
                        break;
                    case "send":
                        cmd = ReplaceVariable(cmd);
                        DeviceObject.Devices.PMBus.SendCommand(ToByte(cmd[1]), ToByte(cmd[2]));
                        datagrid.Invoke(new Action(() => 
                        {
                            AlltxtCommands.Add(txtCommand);
                            datagrid.ScrollIntoView(txtCommand);
                        }));
                        currentId++;
                        break;
                    case "delay":
                        cmd = ReplaceVariable(cmd);
                        //System.Threading.Thread.Sleep(ToInt(cmd[1]));
                        usDelay(Convert.ToInt64(cmd[1]));  //us级别delay
                        txtCommand.ReadbackValue = cmd[1];
                        datagrid.Invoke(new Action(() => 
                        {
                            AlltxtCommands.Add(txtCommand);
                            datagrid.ScrollIntoView(txtCommand);
                        }));
                        currentId++;
                        break;
                    case "loop":
                        cmd = ReplaceVariable(cmd);
                        var endId = FindEndLoop(currentId); //找到对应的endloop
                        var looptime = ToInt(cmd[1]);
                        while (looptime > 0)
                        {
                            var id = currentId + 1;
                            while (id < endId)             //执行内部的操作
                            {
                                id = ExecuteCommand(id);
                            }
                            looptime--;
                        }
                        currentId = endId + 1;
                        break;
                    case "var":
                        string formula = string.Empty;
                        foreach (var c in cmd)
                        {
                            if (c == "var")
                                continue;
                            formula = formula + c;
                        }
                        double vValue = 0;
                        string vKey = string.Empty;
                        if (formula.Contains("="))
                        {
                            vKey = formula.Split('=')[0].Trim();
                            var fml = formula.Split('=')[1].Trim();
                            var newfml = ReplaceVariable(fml);
                            vValue = Calculator.calculate(newfml);      //calculator
                        }
                        else
                        {
                            //vKey = formula.Trim();
                            //vValue = 0;
                            throw new Exception("Invalid operation");
                        }
                        if (keywords.Contains(vKey))
                        {
                            throw new Exception("Variable name can't be key word");
                        }
                        else if (varDic.Any(p => p.Key == vKey))
                        {
                            varDic[vKey] = vValue;
                        }
                        else
                        {
                            varDic.Add(vKey, vValue);
                        }

                        txtCommand.ReadbackValue = vValue.ToString();
                        datagrid.Invoke(new Action(() => 
                        {
                            AlltxtCommands.Add(txtCommand);
                            datagrid.ScrollIntoView(txtCommand);
                        }));
                        currentId++;
                        break;
                    case "savepath":
                        savepath = cmd[1];
                        if (File.Exists(savepath))
                            File.Delete(savepath);
                        txtCommand.ReadbackValue = savepath;
                        datagrid.Invoke(new Action(() => 
                        {
                            AlltxtCommands.Add(txtCommand);
                            datagrid.ScrollIntoView(txtCommand);
                        }));
                        currentId++;
                        break;
                    case "save":
                        string line = "";
                        for (int i = 1; i < cmd.Length; i++)
                        {
                            double val;
                            if (varDic.TryGetValue(cmd[i], out val))
                                line = line + Math.Round(val, 2).ToString() + "\t";
                            else
                                line = line + cmd[i] + "\t";
                        }
                        using (StreamWriter sw = File.AppendText(savepath))
                        {
                            sw.WriteLine(line);
                        }
                        datagrid.Invoke(new Action(() => 
                        {
                            AlltxtCommands.Add(txtCommand);
                            datagrid.ScrollIntoView(txtCommand);
                        }));
                        currentId++;
                        break;
                    case "if":
                        cmd = ReplaceVariable(cmd);
                        var resId = FindElseEndif(currentId);
                        var else_Id = resId[0];
                        var end_Id = resId[1];

                        string f = string.Empty;
                        foreach (var c in cmd)
                        {
                            if (c == "if")
                                continue;
                            f = f + c;
                        }
                        var judge = Calculator.calculate(f);
                        if (judge == 1)   //true
                        {
                            var stop = else_Id != 0 ? else_Id : end_Id;
                            var id = currentId + 1;
                            while (id < stop)
                            {
                                id = ExecuteCommand(id);
                            }
                        }
                        else if (judge == 0)
                        {
                            if (else_Id != 0)  //false and has else branch
                            {
                                var id = else_Id + 1;
                                while (id < end_Id)
                                {
                                    id = ExecuteCommand(id);
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("Invalid operation");
                        }
                        
                        currentId = end_Id + 1;
                        break;
                    default:
                        throw new Exception("Invalid operation");
                }
                return currentId;
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (!ex.Message.Contains("[Line"))
                {
                    message = "[Line " + (currentId+1).ToString() + "] " + ex.Message;
                }
                throw new Exception(message);
            }
        }

        public Dictionary<string, double> varDic = new Dictionary<string, double>();

        public int FindEndLoop(int startId)
        {
            var innerloopCount = 0;  //该loop内部有多少loop
            for (int endId = startId + 1; endId < contentInfos.Count(); endId++)
            {
                var line = contentInfos[endId];        
                if (line.ToLower().Contains("endloop"))
                {
                    if (innerloopCount == 0)
                    {
                        return endId;
                    }
                    else
                    {
                        innerloopCount--;
                    }
                }
                else if (line.ToLower().Contains("loop"))
                {
                    innerloopCount++;
                }
                else
                {
                    continue;
                }
            }
            throw new Exception("Format error: Loop mismatch.");
        }

        public int[] FindElseEndif(int startId)   //int[else_Id, endif_Id]
        {
            var res = new int[2] { 0, 0 };
            var innerifCount = 0;
            for (int id = startId + 1; id < contentInfos.Length; id++)
            {
                var line = contentInfos[id];
                if (line.ToLower().Contains("endif"))
                {
                    if (innerifCount == 0)
                    {
                        res[1] = id;
                        return res;
                    }
                    else
                    {
                        innerifCount--;
                    }
                }
                else if (line.ToLower().Contains("if"))
                {
                    innerifCount++;
                }
                else if (line.ToLower().Contains("else"))
                {
                    if (innerifCount == 0)
                    {
                        res[0] = id;
                    }
                }
                else
                {
                    continue;
                }
            }

            throw new Exception("Format error: if-else-endif mismatch.");

            
        }

        public string[] ReplaceVariable(string[] cmd)
        {
            double value;
            for (int i = 0; i < cmd.Length; i++)
            {
                if (keywords.Contains(cmd[i]) || cmd[i-1] == "to")
                    continue;
                
                if (varDic.TryGetValue(cmd[i], out value))
                {
                    cmd[i] = value.ToString();
                }
            }
            return cmd;
        }

        public string ReplaceVariable(string formula)
        {
            string[] cmd = formula.Split(new char[] { '+', '-', '*', '/', '(', ')', ' ', '&', '|', '^' });
            foreach (var v in cmd)
            {
                double value;
                if (varDic.TryGetValue(v, out value))
                {
                    formula = formula.Replace(v, value.ToString());
                }
                else if (v.EndsWith("h"))
                {
                    formula = formula.Replace(v, ToInt(v).ToString());
                }
            }
            return formula;
        }

        private byte ToByte(string val)
        {
            if (val.EndsWith("h"))
            {
                val = val.Replace('h', ' ').Trim();
                return Convert.ToByte(val, 16);
            }
            return Convert.ToByte(val);
        }

        private ushort ToWord(string val)
        {
            if (val.EndsWith("h"))
            {
                val = val.Replace('h', ' ').Trim();
                return Convert.ToUInt16(val, 16);
            }
            return Convert.ToUInt16(val);
        }

        private int ToInt(string val)
        {
            if (val.EndsWith("h"))
            {
                val = val.Replace('h', ' ').Trim();
                return Convert.ToInt32(val, 16);
            }
            return Convert.ToInt32(val);
        }

        #endregion

        private List<string> keywords = new List<string>()
        {
            "readbyte", "writebyte", "readword", "writeword", "send", "delay",
             "loop", "endloop", "if", "else", "elseif", "endif",
             "var", "to", "save", "savepath",
            "+", "-", "*", "/", "^", "=", "(", ")", "&", "|", ">", "<", ">=", "<=", "==", "ln"
        };

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string dPath = @".\Document\Script Grammer Notes.pdf";
            System.Diagnostics.Process.Start(dPath);
        }
    }
}

