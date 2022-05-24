using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public short[,] memoryb1 = new short[2, 129];
        public ObservableCollection<string> memView = new ObservableCollection<string>();
        
        public short stackpointer = 7;
        public short[] stack = new short[8];

        public short trisaLatch;
        public short trisbLatch;

        public short pc = 0;
        public double commandcounter = 0.0 ;
        public double quarztakt = 1.0;

        

        public Memory()
        {
            initMem();
            foreach(var item in memoryb1)
            {
                memView.Add(string.Format("{0:X2}", item));
            }
            
        }
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
        
        public string S0
        {
            get
            {
                return string.Format("{0:X2}", stack[7]);
            }
        }

        public string S1
        {
            get
            {
                return string.Format("{0:X2}", stack[6]);
            }
        }
        public string S2
        {
            get
            {
                return string.Format("{0:X2}", stack[5]);
            }
        }
        public string S3
        {
            get
            {
                return string.Format("{0:X2}", stack[4]);
            }
        }
        public string S4
        {
            get
            {
                return string.Format("{0:X2}", stack[3]);
            }
        }
        public string S5
        {
            get
            {
                return string.Format("{0:X2}", stack[2]);
            }
        }
        public string S6
        {
            get
            {
                return string.Format("{0:X2}", stack[1]);
            }
        }
        public string S7
        {
            get
            {
                return string.Format("{0:X2}", stack[0]);
            }
        }

        public string Stackpointer
        {
            get
            {
                return string.Format("{0:X1}", stackpointer);
            }
        }

        public ObservableCollection<string> MemView
        {
            get
            {
                return memView;
            }
            set
            {
                this.memView = value;
            }
        }

        public string WReg
        {
            get
            {
                string hexnum = string.Format("0x{0:X2}", memoryb1[0,W]);
                return hexnum;
            }
        }

        public string FSRReg
        {
            get
            {
                string hexnum = string.Format("0x{0:X2}", memoryb1[0,FSR]);
                return hexnum;
            }
        }
        public char[] Statusbits
        {
            get
            {
                char[] bits = Convert.ToString(memoryb1[0,STATUS], 2).PadLeft(8, '0').ToCharArray();
                
                return bits;

            }
        }

        public string Status
        {
            get
            {
                string hexnum = string.Format("0x{0:X2}", memoryb1[0, STATUS]);
                return hexnum;
            }
        }
        public string PclView
        {
            get
            {
                string hexnum = string.Format("0x{0:X2}", memoryb1[0,PCL]);
                return hexnum;
            }
        }


        public string Option
        {
            get
            {
                string hexnum = string.Format("0x{0:X2}", memoryb1[1, OPTION]);
                return hexnum;
            }
        }

        public char[] Optionbits
        {
            get
            {
                char[] bits = Convert.ToString(memoryb1[1, OPTION], 2).PadLeft(8, '0').ToCharArray();

                return bits;
            }
        }
        public string Intcon
        {
            get
            {
                string hexnum = string.Format("0x{0:X2}", memoryb1[0, INTCON]);
                return hexnum;
            }
        }

        public char[] Intconbits
        {
            get
            {
                char[] bits = Convert.ToString(memoryb1[0, INTCON], 2).PadLeft(8, '0').ToCharArray();

                return bits;
            }
        }

        public string PcllathView
        {
            get
            {
                string hexnum = string.Format("0x{0:X2}", memoryb1[1, Memory.PCLATH]);

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
            int i = 0;
            foreach(var item in memoryb1)
            {
                if (i <= 128 && memView[i]!= string.Format("{0:X2}", memoryb1[0,i]))
                {
                    memView[i] = string.Format("{0:X2}", memoryb1[0, i]);
                }
                i++;
            }
            
            
            NotifyPropertyChanged("WReg");
            NotifyPropertyChanged("Status");
            NotifyPropertyChanged("Statusbits");
            NotifyPropertyChanged("PclView");
            NotifyPropertyChanged("FSRReg");

            NotifyPropertyChanged("S0");
            NotifyPropertyChanged("S1");
            NotifyPropertyChanged("S2");
            NotifyPropertyChanged("S3");
            NotifyPropertyChanged("S4");
            NotifyPropertyChanged("S5");
            NotifyPropertyChanged("S6");
            NotifyPropertyChanged("S7");
            NotifyPropertyChanged("Stackpointer");

            NotifyPropertyChanged("Option");
            NotifyPropertyChanged("Optionbits");
            NotifyPropertyChanged("PcllathView");
            NotifyPropertyChanged("runtimecounter");
            NotifyPropertyChanged("Intcon");
            NotifyPropertyChanged("Intconbits");

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
                stackpointer = 7;
            }
        }

        public short pop()
        {
            stackpointer++;
            if(stackpointer == 8)
            {
                stackpointer = 0;
            }
            var retWert = stack[stackpointer];
            stack[stackpointer] = 0;
            return retWert;
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
            memoryb1[1, TRISA] = 0xFF;
            memoryb1[1, TRISB] = 0xFF;
        }

        public bool checkBit(short toCheck, int bitnum)
        {
            if ((toCheck & 0b_01) == 0 && bitnum == 0)
            {
                return false;
            }
            if ((toCheck & 0b_01) != 0 && bitnum == 0)
            {
                return true;
            }
            if ((toCheck & 0b_010) == 0 && bitnum == 1)
            {
                return false;
            }
            if ((toCheck & 0b_010) != 0 && bitnum == 1)
            {
                return true;
            }
            if ((toCheck & 0b_0100) == 0 && bitnum == 2)
            {
                return false;
            }
            if ((toCheck & 0b_0100) != 0 && bitnum == 2)
            {
                return true;
            }
            if ((toCheck & 0b_01000) == 0 && bitnum == 3)
            {
                return false;
            }
            if ((toCheck & 0b_01000) != 0 && bitnum == 3)
            {
                return true;
            }
            if ((toCheck & 0b_010000) == 0 && bitnum == 4)
            {
                return false;
            }
            if ((toCheck & 0b_010000) != 0 && bitnum == 4)
            {
                return true;
            }
            if ((toCheck & 0b_0100000) == 0 && bitnum == 5)
            {
                return false;
            }
            if ((toCheck & 0b_0100000) != 0 && bitnum == 5)
            {
                return true;
            }
            if ((toCheck & 0b_01000000) == 0 && bitnum == 6)
            {
                return false;
            }
            if ((toCheck & 0b_01000000) != 0 && bitnum == 6)
            {
                return true;
            }
            if ((toCheck & 0b_010000000) == 0 && bitnum == 7)
            {
                return false;
            }
            if ((toCheck & 0b_010000000) != 0 && bitnum == 7)
            {
                return true;
            }
            return false;
        }

        public short setBit(short reg, int bit)// sets specific bit in value and returns it
        {
            switch (bit)
            {
                case 0:
                   reg = (short)(reg | 0b_01);
                    break;
                case 1:
                    reg = (short)(reg | 0b_010);
                    break;
                case 2:
                    reg = (short)(reg | 0b_100);
                    break;
                case 3:
                    reg = (short)(reg | 0b_1000);
                    break;
                case 4:
                    reg = (short)(reg | 0b_10000);
                    break;
                case 5:
                    reg = (short)(reg | 0b_100000);
                    break;
                case 6:
                    reg = (short)(reg | 0b_1000000);
                    break;
                case 7:
                    reg = (short)(reg | 0b_10000000);
                    break;
            }
            return reg;
        }
        public short clrBit(short reg, int bit) // clears specific bit in value and returns it
        {
            
            switch (bit)
            {
                case 0:
                    reg = (short)(reg & 0b_11111110);
                    break;
                case 1:
                    reg = (short)(reg & 0b_11111101);
                    break;
                case 2:
                    reg = (short)(reg & 0b_11111011);
                    break;
                case 3:
                    reg = (short)(reg & 0b_11110111);
                    break;
                case 4:
                    reg = (short)(reg & 0b_11101111);
                    break;
                case 5:
                    reg = (short)(reg & 0b_11011111);
                    break;
                case 6:
                    reg = (short)(reg & 0b_10111111);
                    break;
                case 7:
                    reg = (short)(reg & 0b_101111111);
                    break;

            }
            return reg;
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
            for(int i = 0; i<8;i++)
            {
                stack[i] = 0;
                
            }
            stackpointer = 7;
            initMem();
            // To be continued...
        }
    }


}
