using MPS.IIC.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPS.IIC.Script
{
    class TXTCommandInfos : ObservableObject
    {
        private string _slaveAddr;

        public string SlaveAddr
        {
            get { return _slaveAddr; }
            set
            {
                _slaveAddr = value;
                RaisePropertyChanged("SlaveAddr");
            }
        }


        private string _regAddr = "";

        public string RegAddr
        {
            get { return _regAddr; }
            set
            {
                _regAddr = value;
                RaisePropertyChanged("RegAddr");
            }
        }

        private string _operation;

        public string Operation
        {
            get { return _operation; }
            set
            {
                _operation = value;
                RaisePropertyChanged("Operation");
            }
        }

        private string _expectedValue;

        public string ExpectedValue
        {
            get { return _expectedValue; }
            set
            {
                _expectedValue = value;
                RaisePropertyChanged("ExpectedValue");
            }
        }

        private string _readbackValue = "";

        public string ReadbackValue
        {
            get { return _readbackValue; }
            set
            {
                _readbackValue = value;
            }
        }

        private string _check = "";

        public string Check
        {
            get { return _check; }
            set
            {
                _check = value;
                RaisePropertyChanged("Check");
            }
        }
    }
}
