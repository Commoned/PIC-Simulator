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
        public static byte W = 0x10;


        public short[] eeprom = new short[1024];
        public short[] reg = new short[128];
        public Register[] memoryb1 = new Register[128];
        public short[] memoryb2 = new short[128];//Beide Bänke in einem Array maybe
        public short stackpointer = 6;
        public short[] stack = new short[7];

        public Memory()
        {
            initMem();
        }
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }


        

        public Register[] Memoryb1
        {
            get {
                
                return memoryb1;
            }
        }


        public string WReg
        {
            get
            {
                string hexnum = string.Format("0x{0:X2}", memoryb1[Memory.W].Value);
                return hexnum;
            }
        }

        public string FSRReg
        {
            get
            {
                string hexnum = string.Format("0x{0:X2}", memoryb1[Memory.FSR].Value);
                return hexnum;
            }
        }
        public char[] Status
        {
            get
            {
                char[] bits = Convert.ToString(memoryb1[Memory.STATUS].Value, 2).PadLeft(8, '0').ToCharArray();
                
                return bits;

            }
        }
        public string PclView
        {
            get
            {
                string hexnum = string.Format("0x{0:X2}", memoryb1[Memory.PCL].Value);
                return hexnum;
            }
        }

        

        public void updateMemView()
        {
            NotifyPropertyChanged("Memoryb1");
            NotifyPropertyChanged("Value");
            
            NotifyPropertyChanged("WReg");
            NotifyPropertyChanged("Status");
            NotifyPropertyChanged("PclView");
        }

        public short Pcl
        {
            get { return memoryb1[0x02].Value; }
            set { 
                memoryb1[0x02].Value = value;
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
            
            for(int i=0; i<=127; i++)
            {
                memoryb1[i] = new Register(i, 0,this);
            }

            memoryb1[2].Value = 0;

            
            memoryb1[3].Value = 0x18;
            memoryb1[10].Value = 0;
            memoryb1[11].Value = 0;
            memoryb2[1] = 0xFF;
            memoryb2[3] = 0x18;
            memoryb2[5] = 0x1F;
            memoryb2[6] = 0xFF;
            memoryb2[8] = 0x0;
            memoryb2[0x0A] = 0x0;
            memoryb2[0x0B] = 0x0;
        }

        public void resetMem()
        {
            initMem();
            // To be continued...
        }
    }


}
