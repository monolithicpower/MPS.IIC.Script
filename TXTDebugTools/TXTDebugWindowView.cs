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


        private ObservableCollection<TXTCommandInfos> _alltxtCommandInfos;

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

        public void BrowseMethod()
        {
            int index = 0;
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

                AlltxtCommandInfos = new ObservableCollection<TXTCommandInfos>();

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
                        var strInfos = str.Split(new char[] { ' ', '=' }).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                        if (strInfos[0].ToLower() == "loop")
                        {
                            var times = int.Parse(strInfos[1]);

                            var k = j + 1;
                            var s = _contentInfos[k].ToLower();
                            while (s.Trim() != "end_loop")
                            {
                                k++;
                                s = _contentInfos[k].ToLower();  //若超出范围仍未找到endloop 这里报访问越界的错
                            }

                            var looptemps = _contentInfos.Skip(j + 1).Take(k - j - 1).ToArray();
                            for (int m = 0; m < times; m++)
                            {
                                for (int itemID = 0; itemID < looptemps.Length; itemID++)
                                {
                                    contentInfos.Add(looptemps[itemID]);
                                    rowIDs.Add(j + 2 + itemID);
                                }
                            }
                            j = k;
                        }
                        else
                        {
                            contentInfos.Add(_contentInfos[j]);
                            rowIDs.Add(j + 1);
                        }
                    }
                    #endregion

                    byte slaveAddr = 0;

                    #region Parse Content
                    for (var id = 0; id < contentInfos.ToArray().Length; id++)
                    {
                        var str = contentInfos[id].Trim();
                        index = rowIDs[id];

                        var temptxtCommand = new TXTCommandInfos();//
                        var strInfos = str.Split(new char[] { ' ', '=' });
                        strInfos = strInfos.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

                        #region set address
                        if (index == 1)
                        {
                            if (strInfos.Length == 3 && strInfos[0].ToLower() == "set" && strInfos[1].ToLower() == "address")
                            {

                                var addr = strInfos[2];
                                slaveAddr = Convert.ToByte(addr, 16);
                                temptxtCommand.Operation = "SET ADDRESS";
                                temptxtCommand.ExpectedValue = addr;
                                AlltxtCommandInfos.Add(temptxtCommand);

                                continue;
                            }
                            else
                            {
                                throw new Exception("Must set address first.");
                            }
                        }
                        #endregion


                        if (strInfos.Length == 3)
                        {
                            temptxtCommand.RegAddr = strInfos[0].PadLeft(2, '0');
                            temptxtCommand.Operation = strInfos[1].ToUpper();

                            var regAddr = Convert.ToByte(strInfos[0], 16);
                            var format = strInfos[1];

                            switch (format.ToLower())
                            {
                                case "write_byte":
                                    //byte value1;
                                    DeviceObject.Devices.PMBus.WriteByte(slaveAddr, regAddr, Convert.ToByte(strInfos[2], 16));
                                    //DeviceObject.Devices.PMBus.ReadByte(slaveAddr, regAddr, out value1);
                                    temptxtCommand.ExpectedValue = strInfos[2].PadLeft(2, '0');
                                    //temptxtCommand.ReadbackValue = StringConvert(value1);
                                    break;
                                case "write_word":
                                    //ushort value2;
                                    DeviceObject.Devices.PMBus.WriteWord(slaveAddr, regAddr, Convert.ToUInt16(strInfos[2], 16));
                                    //DeviceObject.Devices.PMBus.ReadWord(slaveAddr, regAddr, out value2);
                                    temptxtCommand.ExpectedValue = strInfos[2].PadLeft(4, '0');
                                    //temptxtCommand.ReadbackValue = StringConvert(value2);
                                    break;
                                case "read_byte":
                                    byte value3;
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
                        AlltxtCommandInfos.Add(temptxtCommand);
                    }
                    #endregion
                }
                #endregion

                TXTStatusMessage = "Script configuration Write Finished.";
                path = null;
            }
            catch (Exception ex)
            {
                path = null;
                if (index == 0)
                {
                    TXTStatusMessage = " 'LOOP' and 'END_LOOP' mismatch.";
                }
                else if (index == 1)
                {
                    TXTStatusMessage = string.Format("Error in Line 1:  Please check the command format. " + ex.Message);
                }
                else
                {
                    TXTStatusMessage = string.Format("Error in Line {0}:  Please check the command format.", index);
                }

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
    }
}

