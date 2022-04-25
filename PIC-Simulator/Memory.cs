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
        public short[] memoryb1 = new short[128];
        public short[] memoryb2 = new short[128];//Beide Bänke in einem Array maybe
        public short stackpointer = 7;
        public short[] stack = new short[7];

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
                string[] ret = new string[memoryb1.Length];
                int index = 0;
                foreach (var item in memoryb1)
                {
                    ret[index] = Convert.ToString(item, 16).ToUpper();
                    index++;
                }
                return ret;
            }

        }


        public string WReg
        {
            get
            {
                return "0x" + Convert.ToString(memoryb1[Memory.W], 16).ToUpper();
            }
        }

        public string FSRReg
        {
            get
            {
                return "0x" + Convert.ToString(memoryb1[Memory.FSR], 16).ToUpper();
            }
        }

        

        public void updateMemView()
        {
            NotifyPropertyChanged("Memoryb1");
            NotifyPropertyChanged("WReg");
        }

        public short Pcl
        {
            get { return memoryb1[0x02]; }
            set { 
                memoryb1[0x02] = value;
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
            return stack[stackpointer-1];
        }

        public void initMem()
        {
            memoryb1[2] = 0;
            memoryb1[3] = 18;
            memoryb1[10] = 0;
            memoryb1[11] = 0;
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
