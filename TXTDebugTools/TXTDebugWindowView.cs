using Microsoft.Win32;
using MPS.Common.Command;
using MPS.IIC.Model;
using MPS.Instrument;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MPS.IIC.Script
{
    class TXTDebugWindowView : ObservableObject
    {
        public TXTDebugWindowView(string FilePath = null)
        {
            this.path = FilePath;
        }

        public TXTDebugWindowView()
        {

        }

        #region property
        private string _selectedFilePath;

        public string SelectedFilePath
        {
            get { return _selectedFilePath; }
            set
            {
                _selectedFilePath = value;
                RaisePropertyChanged("SelectedFilePath");
            }
        }

        private string _address = "User Command List";

        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                RaisePropertyChanged("Address");
            }
        }


        private ObservableCollection<TXTCommandInfos> _alltxtCommandInfos = new ObservableCollection<TXTCommandInfos>();

        public ObservableCollection<TXTCommandInfos> AlltxtCommandInfos
        {
            get { return _alltxtCommandInfos; }
            set
            {
                _alltxtCommandInfos = value;
                RaisePropertyChanged("AlltxtCommandInfos");
            }
        }


        private string _txtStatusMessage;

        public string TXTStatusMessage
        {
            get { return _txtStatusMessage; }
            set
            {
                _txtStatusMessage = value;
                RaisePropertyChanged("TXTStatusMessage");
            }
        }

        #endregion



        #region command
        public ICommand BrowseCommand
        {
            get
            {
                return new RelayCommand(BrowseMethod, null);
            }
        }

        private string path = null;
        string[] contentInfos;
        Dictionary<string, int> varDic = new Dictionary<string, int>();

        public void BrowseMethod()
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    path = OpenFile();
                    if (!File.Exists(path)) throw new Exception("File don't exists!");
                    this.SelectedFilePath = path;
                }
                else
                {
                    this.SelectedFilePath = this.path;
                }

                using (StreamReader sr = new StreamReader(path))
                {
                    string content = sr.ReadToEnd();
                    if (string.IsNullOrEmpty(content) || string.IsNullOrWhiteSpace(content))
                    {
                        throw new Exception("Script File is empty.");
                    }

                    contentInfos = content.Split(new char[] { '\r', '\n' });
                    contentInfos = contentInfos.Where(s => (!string.IsNullOrEmpty(s)) && (!string.IsNullOrWhiteSpace(s))).ToArray();  //去掉空行

                    #region Parse Content
                    var currentId = 0;
                    while (currentId <= contentInfos.ToArray().Length)
                    {
                        ExecuteCommand(currentId);  //executing
                        currentId++;
                    }
                    #endregion
                }
                TXTStatusMessage = "Done.";
                path = null;
            }
            catch (Exception ex)
            {
                path = null;
                TXTStatusMessage = ex.Message;
            }           
        }

        #endregion


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
        #endregion


        public void ExecuteCommand(int currentId)
        {
            var cmd = contentInfos[currentId].Trim().Split(new char[] { ' ', '=' });
            cmd = cmd.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            var txtCommand = new TXTCommandInfos();
            switch (cmd[0])
            {
                case "readbyte":
                    byte value;
                    DeviceObject.Devices.PMBus.ReadByte(Convert.ToByte(cmd[1], 16), Convert.ToByte(cmd[2], 16), out value);
                    txtCommand.Operation = "readbyte";
                    txtCommand.SlaveAddr = cmd[1];
                    txtCommand.RegAddr = cmd[2];
                    txtCommand.ExpectedValue = cmd[3];
                    txtCommand.ReadbackValue = value.ToString("X2");
                    txtCommand.Check = (txtCommand.ReadbackValue == txtCommand.ExpectedValue).ToString();
                    AlltxtCommandInfos.Add(txtCommand);
                    break;
                case "writebyte":
                    DeviceObject.Devices.PMBus.WriteByte(Convert.ToByte(cmd[1], 16), Convert.ToByte(cmd[2], 16), Convert.ToByte(cmd[3], 16));
                    txtCommand.Operation = "writebyte";
                    txtCommand.SlaveAddr = cmd[1];
                    txtCommand.RegAddr = cmd[2];
                    txtCommand.ExpectedValue = cmd[3];
                    AlltxtCommandInfos.Add(txtCommand);
                    break;
                case "readword":
                    ushort value1;
                    DeviceObject.Devices.PMBus.ReadWord(Convert.ToByte(cmd[1], 16), Convert.ToByte(cmd[2], 16), out value1);
                    txtCommand.Operation = "readword";
                    txtCommand.SlaveAddr = cmd[1];
                    txtCommand.RegAddr = cmd[2];
                    txtCommand.ExpectedValue = cmd[3].PadLeft(4, '0');
                    txtCommand.ReadbackValue = value1.ToString("X4");
                    txtCommand.Check = (txtCommand.ReadbackValue == txtCommand.ExpectedValue).ToString();
                    AlltxtCommandInfos.Add(txtCommand);
                    break;
                case "writeword":
                    DeviceObject.Devices.PMBus.WriteWord(Convert.ToByte(cmd[1], 16), Convert.ToByte(cmd[2], 16), Convert.ToUInt16(cmd[3], 16));
                    txtCommand.Operation = "writeword";
                    txtCommand.SlaveAddr = cmd[1];
                    txtCommand.RegAddr = cmd[2];
                    txtCommand.ExpectedValue = cmd[3];
                    AlltxtCommandInfos.Add(txtCommand);
                    break;
                case "send":
                    DeviceObject.Devices.PMBus.SendCommand(Convert.ToByte(cmd[0], 16), Convert.ToByte(cmd[1], 16));
                    txtCommand.Operation = "send";
                    txtCommand.SlaveAddr = cmd[1];
                    txtCommand.RegAddr = cmd[2];
                    AlltxtCommandInfos.Add(txtCommand);
                    break;
                case "delay":
                    System.Threading.Thread.Sleep(Convert.ToInt32(cmd[1]));
                    AlltxtCommandInfos.Add(txtCommand);
                    break;
                case "loop":
                    var endId = FindEndLoop(currentId); //找到对应的endloop
                    var looptime = Convert.ToInt32(cmd[1]);                  
                    while (looptime > 0)  
                    {
                        for (int id = currentId + 1; id < endId; id++)  //执行内部的操作
                        {
                            ExecuteCommand(id);
                        }
                        looptime--;
                    }
                    currentId = endId;
                    break;
                case "var":
                    //variable setting
                    //查找varDic中有没有
                    //有：改值
                    //没有：添加
                    //考虑前面的命令中的变量替换问题
                    break;
                default:
                    throw new Exception("Invalid operation");
            }
        }

        public int FindEndLoop(int startId)
        {
            var innerloopCount = 0;  //该loop内部有多少loop
            for (int endId = startId + 1; endId < contentInfos.Count(); endId++)
            {
                var line = contentInfos[endId];
                if (line.ToLower().Contains("loop"))
                {
                    innerloopCount++;
                }
                else if (line.ToLower().Contains("endloop"))
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
                else
                {
                    continue;
                }
            }
            throw new Exception("Format error: Loop mismatch.");
        }

        
    }
}

