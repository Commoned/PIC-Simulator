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
        public static byte EECON1 = 0x08;
        public static byte EECON2 = 0x09;
        public static byte PCLATH = 0xA;
        public static byte INTCON = 0x0B;
        public static byte W = 0x80;


        public short[] eeprom = new short[1024];
        public short[,] memoryb1 = new short[2, 129];
        public short[,] comparememoryb1 = new short[2, 129];
        public ObservableCollection<string> memView = new ObservableCollection<string>();
        
        public short stackpointer = 7;
        public short[] stack = new short[8];

        public short trisaLatch;
        public short trisbLatch;

        public short programmcounter = 0;
        public string pclManipulation = "";
        public double commandcounter = 0.0 ;
        public double wdtcounter = 0.0;
        public double wdttime = 0.0;

        public double quarztakt = 1.0;

        public short WDTE = 1;
        public short vt=0xFF;
        

        public Memory()
        {
            initMem();
            
            for(int i = 0; i <= 127; i++)
            {
                memView.Add(string.Format("{0:X2}", memoryb1[0,i]));
            }
            for (int i = 0; i <= 127; i++)
            {
                memView.Add(string.Format("{0:X2}", memoryb1[1, i]));
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

        public string Vt
        {
            get
            {
                return string.Format("0x{0:X2}", vt);
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

        public void increasePc()
        {
            if(Pcl == (short)(programmcounter & 0b_0000_0000_1111_1111))
            {
                if (programmcounter >= 0x3FF)
                {
                    programmcounter = 0;
                }
                else
                {
                    programmcounter++;
                }
                Pcl = (short)(programmcounter & 0b_0000_0000_1111_1111);
            }
            
        }

        public void setPc(short value)
        {
            programmcounter = value;
            Pcl = (short)(programmcounter & 0b_0000_0000_1111_1111);
        }

        public string WReg
        {
            get
            {
                string hexnum = string.Format("0x{0:X2}", memoryb1[0,W]);
                return hexnum;
            }
            
        }

        public string PC
        {
            get
            {
                string hexnum = string.Format("0x{0:X3}", programmcounter);
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


        public string Eecon1
        {
            get
            {
                string hexnum = string.Format("0x{0:X2}", memoryb1[1, EECON1]);
                return hexnum;
            }
        }

        public char[] Eecon1bits
        {
            get
            {
                char[] bits = Convert.ToString(memoryb1[1, EECON1], 2).PadLeft(5, '0').ToCharArray();

                return bits;
            }
        }


        public string PclathView

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

        public string Wdtcounter
        {
            get
            {
                wdttime = wdtcounter * quarztakt;
                string num = string.Format("\u0009 {0:F4} \u00b5s", (wdtcounter * quarztakt));
                
                return num;
            }
        }

        public void updateMemView()
        {
            if(memoryb1[0,W] != comparememoryb1[0,W])
            {
                NotifyPropertyChanged("WReg");
            }
            if (memoryb1[0, STATUS] != comparememoryb1[0, STATUS])
            {
                NotifyPropertyChanged("Status");
                NotifyPropertyChanged("Statusbits");
            }
            if (memoryb1[0, PCL] != comparememoryb1[0, PCL])
            {
                NotifyPropertyChanged("PclView");
            }
            if (memoryb1[0, FSR] != comparememoryb1[0, FSR])
            {
                NotifyPropertyChanged("FSRReg");
            }
            if (memoryb1[0, INTCON] != comparememoryb1[0, INTCON])
            {
                NotifyPropertyChanged("Intcon");
                NotifyPropertyChanged("Intconbits");
            }
            if (memoryb1[1, OPTION] != comparememoryb1[1, OPTION])
            {
                NotifyPropertyChanged("Option");
                NotifyPropertyChanged("Optionbits");
            }
            if (memoryb1[0, PCLATH] != comparememoryb1[0, PCLATH])
            {
                NotifyPropertyChanged("PclathView");
            }
            if (memoryb1[1, EECON1] != comparememoryb1[1, EECON1])
            {
                NotifyPropertyChanged("Eecon1"); NotifyPropertyChanged("Eecon1bits");
            }
            string[] toNotify = {
                "S0", "S1", "S2", "S3", "S4", "S5", "S6", "S7",
                "Stackpointer",
                "Wdtcounter",
                "runtimecounter",
                "Vt",
                "PC",

            };
            foreach(string s in toNotify)
            {
                NotifyPropertyChanged(s);
            }

            for (int i = 0; i <= 127; i++)
            {
                if (memView[i] != string.Format("{0:X2}", memoryb1[0, i]))
                {
                    memView[i] = string.Format("{0:X2}", memoryb1[0, i]);
                }
                if (memView[i + 128] != string.Format("{0:X2}", memoryb1[1, i]))
                {
                    memView[i + 128] = string.Format("{0:X2}", memoryb1[1, i]);
                }
                comparememoryb1[0, i] = memoryb1[0, i];
                comparememoryb1[1, i] = memoryb1[1, i];
            }
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
            programmcounter = 0;
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
            memoryb1[1, TRISA] = 0x1F;
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
            commandcounter = 0;
            for(int i = 0; i<8;i++)
            {
                stack[i] = 0;
            }
            stackpointer = 7;
            vt = 0;
            trisaLatch = 0;
            trisbLatch = 0;
            wdtcounter = 0;
            programmcounter = 0;
            Pcl = 0;
            wdttime = 0;
            // Standardwerte initialisieren nach reset
            
            memoryb1[0, PCL] = 0;
            memoryb1[0, STATUS] = (short)(memoryb1[0, STATUS] & 0b_0001_1111);
            memoryb1[0, PCLATH] = 0;
            memoryb1[0, INTCON] = (short)(memoryb1[0, INTCON] & 0b_01);
            memoryb1[1, OPTION] = 0xFF;
            memoryb1[1, TRISA] = 0x1F;
            memoryb1[1, TRISB] = 0xFF;
            memoryb1[1, EECON1] = (short)(memoryb1[1, EECON1] & 0b_01000);
            memoryb1[1, PCLATH] = 0x0;
            memoryb1[1, INTCON] = (short)(memoryb1[1, INTCON] & 0b_01);
            updateMemView();

        }
    }


}
