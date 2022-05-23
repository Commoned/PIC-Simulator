using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace PIC_Simulator
{

    public class Memory : INotifyPropertyChanged
    {
        public static byte PORTA = 0x05;
        public static byte PORTB = 0x06;
        public static byte TRISA = 0x05;
        public static byte TRISB = 0x06;
        public static byte FSR = 0x04;
        public static byte TMR0 = 0x01;
        public static byte OPTION = 0x01;
        public static byte PCL = 0x02;
        public static byte STATUS = 0x03;
        public static byte EEDATA = 0x08;
        public static byte EEADR = 0x09;
        public static byte PCLATH = 0xA;
        public static byte INTCON = 0x0B;
        public static byte W = 0x80;


        public short[] eeprom = new short[1024];
        public short[,] memoryb1 = new short[2,129];
        public short stackpointer = 6;
        public short[] stack = new short[7];
        public short pc = 0;
        public double commandcounter = 0.0 ;
        public double quarztakt = 1.0;

        public Memory()
        {
            initMem();
        }
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }





        public string[] Memoryb1
        {
            get {
                string[] ret = new string[memoryb1.Length/2];
                int index = 0;
                foreach (var item in memoryb1)
                {
                    if (index <= 128)
                    {
                        ret[index] = Convert.ToString(item, 16).ToUpper();
                        index++;
                    }
                    else
                    {
                        return ret;
                    }
                }
                return ret;
            }
            set
            {

            }

        }


        public string WReg
        {
            get
            {
                string hexnum = string.Format("0x{0:X2}", memoryb1[0,Memory.W]);
                return hexnum;
            }
        }

        public string FSRReg
        {
            get
            {
                string hexnum = string.Format("0x{0:X2}", memoryb1[0,Memory.FSR]);
                return hexnum;
            }
        }
        public char[] Status
        {
            get
            {
                char[] bits = Convert.ToString(memoryb1[0,Memory.STATUS], 2).PadLeft(8, '0').ToCharArray();
                
                return bits;

            }
        }
        public string PclView
        {
            get
            {
                string hexnum = string.Format("0x{0:X2}", memoryb1[0,Memory.PCL]);
                return hexnum;
            }
        }
        public string PcllathView
        {
            get
            {
                string hexnum = string.Format("0x{0:X2}", memoryb1[0, Memory.PCLATH]);
                return hexnum;
            }
        }
        
        public string runtimecounter
        {
            get
            {
                string num = string.Format("\u0009 {0:F4} \u00b5s", (commandcounter * quarztakt));
                return num;
            }
        }

        public void updateMemView()
        {
            NotifyPropertyChanged("Memoryb1");
            NotifyPropertyChanged("WReg");
            NotifyPropertyChanged("Status");
            NotifyPropertyChanged("PclView");
            NotifyPropertyChanged("FSRReg");
            NotifyPropertyChanged("PcllathView");
            NotifyPropertyChanged("runtimecounter");
        }

        public short Pcl
        {
            get { return memoryb1[0,0x02]; }
            set { 
                memoryb1[0,0x02] = value;
                NotifyPropertyChanged("Pcl");
                
            }
        }

        

        public void push(short value)
        {
            if(stackpointer != 0)
            {
                stack[stackpointer] = value;
                stackpointer--;
            }
            else
            {
                stack[stackpointer] = value;
                stackpointer = 6;
            }
        }

        public short pop()
        {
            stackpointer++;
            return stack[stackpointer];
        }

        public void initMem()
        {
            memoryb1[0,2] = 0;
            memoryb1[0,3] = 0x18;
            memoryb1[0,10] = 0;
            memoryb1[0,11] = 0;
            memoryb1[1,1] = 0xFF;
            memoryb1[1,3] = 0x18;
            memoryb1[1,5] = 0x1F;
            memoryb1[1,6] = 0xFF;
            memoryb1[1,8] = 0x0;
            memoryb1[1,0x0A] = 0x0;
            memoryb1[1,0x0B] = 0x0;
        }

        public void resetMem()
        {
            for(int i = 0; i<memoryb1.Length/2;i++)
            {
                memoryb1[0,i] = 0;
            }
            for (int i = 0; i < memoryb1.Length / 2; i++)
            {
                memoryb1[1, i] = 0;
            }
            commandcounter = 0;
            initMem();
            // To be continued...
        }
    }


}
