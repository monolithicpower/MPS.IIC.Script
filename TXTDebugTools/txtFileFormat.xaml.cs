using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MPS.IIC.Script
{
    /// <summary>
    /// Interaction logic for txtFileFormat.xaml
    /// </summary>
    public partial class txtFileFormat
    {

        public txtFileFormat()
        {
            InitializeComponent();
            format_tb1.Text =
                "\n" +
                string.Format("{0,-30}\t#{1,-40}\n", " SET ADDRESS=24", "Set Slave address as 24") +
                "\n" +
                string.Format("{0,-30}\t#{1,-40}\n", " 02 WRITE_BYTE 07", "Write byte value 07 to Reg02") +
                "\n" +
                string.Format("{0,-30}\t#{1,-40}\n", " 21 WRITE_WORD CD10", "Write word value CD10 to Reg21") +
                "\n" +
                string.Format("{0,-30}\t\t#{1,-40}\n", " DELAY 5", "Delay for 5(ms)") +
                "\n" +
                string.Format("{0,-30}\t#{1,-40}\n", " 02 READ_BYTE 14", "Read byte from Reg02, expected value is 14") +
                "\n" +
                string.Format("{0,-30}\t#{1,-40}\n", " 21 READ_WORD CD10", "Read word from Reg21, expected value is CD10") +
                "\n" +
                string.Format("{0,-30}\t\t#{1,-40}\n", " 15 SEND", "Send command to Reg15") +
                "\n" +
                string.Format("{0,-30}\t\t#{1,-40}\n", " LOOP 3", "Start a loop, loop count is 3") +
                "\n" +
                string.Format("{0,-30}\n", " 02 WRITE_BYTE 07", "") +
                "\n" +
                string.Format("{0,-30}\n", " DELAY 10", "") +
                "\n" +
                string.Format("{0,-30}\t\t#{1,-40}\n", " END_LOOP", "End of the Loop");

            format_tb3.Text = @"Hint: For a better readability, the following format is recommanded:
            *2 bits with upper case to represent register address.(eg. 02,10,7E)
            *2 bits with upper case to represent byte value.(eg. 17,08,F0)
            *4 bits with upper case to represent word value.(eg.CD10,0001,D032)
            *Use comment with '#' before the content.(eg. #This is a comment)" +
                                "\n" +
                                "\n" +
                              @"Hint: Do not support the Nested Loop.";
        }
    }
}
