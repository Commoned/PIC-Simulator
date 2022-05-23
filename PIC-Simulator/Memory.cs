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
        public short trisaLatch;
        public short trisbLatch;

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
                        ret[index] = string.Format("{0:X2}", item);
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
        public char[] Status
        {
            get
            {
                char[] bits = Convert.ToString(memoryb1[0,STATUS], 2).PadLeft(8, '0').ToCharArray();
                
                return bits;

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
        

        public void updateMemView()
        {
            NotifyPropertyChanged("Memoryb1");
            NotifyPropertyChanged("WReg");
            NotifyPropertyChanged("Status");
            NotifyPropertyChanged("PclView");
            NotifyPropertyChanged("FSRReg");
            NotifyPropertyChanged("Option");
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

        public void setBit(short reg, int bit)
        {
            short tempTris =0;
            if(reg == 0)
            {
                tempTris = trisaLatch;
            }
            else
            {
                tempTris = trisbLatch;
            }
            switch (bit)
            {
                case 0:
                   tempTris = (short)(tempTris | 0b_01);
                    break;
                case 1:
                    tempTris = (short)(tempTris | 0b_010);
                    break;
                case 2:
                    tempTris = (short)(tempTris | 0b_100);
                    break;
                case 3:
                    tempTris = (short)(tempTris | 0b_1000);
                    break;
                case 4:
                    tempTris = (short)(tempTris | 0b_10000);
                    break;
                case 5:
                    tempTris = (short)(tempTris | 0b_100000);
                    break;
                case 6:
                    tempTris = (short)(tempTris | 0b_1000000);
                    break;
                case 7:
                    tempTris = (short)(tempTris | 0b_10000000);
                    break;
            }
            if(reg==0)
            {
                trisaLatch = tempTris;
            }
            else{
                trisbLatch = tempTris;
            }


        }
        public void clrBit(short reg, int bit)
        {
            short tempTris = 0;
            if (reg == 0)
            {
                tempTris = trisaLatch;
            }
            else
            {
                tempTris = trisbLatch;
            }
            switch (bit)
            {
                case 0:
                    tempTris = (short)(tempTris & 0b_11111110);
                    break;
                case 1:
                    tempTris = (short)(tempTris & 0b_11111101);
                    break;
                case 2:
                    tempTris = (short)(tempTris & 0b_11111011);
                    break;
                case 3:
                    tempTris = (short)(tempTris & 0b_11110111);
                    break;
                case 4:
                    tempTris = (short)(tempTris & 0b_11101111);
                    break;
                case 5:
                    tempTris = (short)(tempTris & 0b_11011111);
                    break;
                case 6:
                    tempTris = (short)(tempTris & 0b_10111111);
                    break;
                case 7:
                    tempTris = (short)(tempTris & 0b_101111111);
                    break;

            }
            if (reg == 0)
            {
                trisaLatch = tempTris;
            }
            else
            {
                trisbLatch = tempTris;
            }
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
            initMem();
            // To be continued...
        }
    }


}
